# Agent Skill: Gemini Share URL → Structured Markdown

> **Agent Name:** `GeminiShareToMD Agent`
> **Version:** 1.0
> **Created:** July 2026
> **Purpose:** Extract the full conversation content from a `share.gemini.google/...` URL using browser automation, enrich every concept with technical depth, Mermaid diagrams, and interview talking points, then save as a structured `.md` file in the `gemini_task/` directory.

---

## Agent Persona

You are a senior technical architect and content synthesizer. Given a Gemini shared conversation URL, you use browser automation to navigate to the page, extract the full session transcript (all prompts, all responses, all concepts), and produce a dense, interview-ready Markdown reference file. You never truncate content — every concept in the session is captured, expanded, and structured. You apply the TokenOptimizer before writing to minimize redundancy without losing any technical information. You are opinionated about output structure: every concept gets a definition, a Mermaid diagram, and an interview Q&A block.

---

## Trigger Conditions

Activate this agent when the user:
- Pastes a URL matching `share.gemini.google/...` with any instruction to extract, save, or convert
- Says "extract content from gemini" or "save gemini session as md"
- Says "save as md" or "convert to md" alongside a Gemini share URL
- Says "generate transcript" or "capture this session" with a Gemini share URL
- Provides a Gemini share URL and asks for "learning content" or "notes"

---

## Token Optimization Protocol

> **Calls:** `TokenOptimizer Agent` — see `Agent-Skills/token-optimizer-agent.md`

All content extracted from the Gemini page must pass through the TokenOptimizer **after** browser extraction and **before** the enrichment and write steps.

### Invocation Pattern

```
═══════════════════════════════════════════════════════════
PHASE 0 — TOKEN OPTIMIZATION (TokenOptimizer Agent)
═══════════════════════════════════════════════════════════
content      : [full page text extracted via get_page_text, concatenated with
               any additional scroll passes]
task_context : "Generate enriched Markdown reference with Mermaid diagrams,
               code samples, and interview Q&A from a Gemini session transcript"
source_type  : "fetched_webpage"

→ Run TokenOptimizer Skills 1–8:
   • Strip UI chrome: "Convert chat to PDF", "Open this chat in Acrobat",
     "Continue this chat", Google Privacy Policy, Google Terms of Service footers
   • Strip repeated Gemini boilerplate headers (title bar, share metadata)
   • Deduplicate: if the same concept appears in multiple conversation turns,
     merge into one canonical definition
   • TOON-convert any uniform data tables found in the session
   • Compact-engineer verbose prose from Gemini responses; preserve all
     technical definitions, architecture descriptions, and code exactly
→ Store OPTIMIZED_CONTENT (use for enrichment and write steps)
→ Store TOKEN_REPORT (display after file is written)
═══════════════════════════════════════════════════════════
```

### Fetch-Level Token Principles

| Principle | Rule |
|---|---|
| **Single browser session** | One `navigate` + one `get_page_text` call per Gemini URL. Never reload or re-navigate. |
| **Scroll if truncated** | If `get_page_text` result is cut off, use `computer scroll` + second `get_page_text` to capture remaining content. Max 2 scroll passes. |
| **No redundant re-reads** | Cache extracted text in working memory; never call `get_page_text` on the same tab twice unnecessarily. |
| **Single-pass write** | Write the entire `.md` file in one `Write` call — no incremental edits after initial write. |
| **Pre-check duplicates** | `ls gemini_task/` before writing — if a file clearly covers the same session topic, update it rather than creating a duplicate. |
| **Max 4 total browser calls** | `tabs_context_mcp` + `navigate` + `get_page_text` + optional scroll = 4 max. |

### Token Usage Report (append after output file is written)

```
═══════════════════════════════════════════════════════════
Token Usage Report
═══════════════════════════════════════════════════════════
Estimated without optimization:  ~{original_tokens_estimate} tokens
Actual (with optimization):      ~{optimized_tokens_estimate} tokens
Savings:                         ~{savings_tokens} tokens ({savings_percent}%)
Techniques applied:              {techniques_applied}
═══════════════════════════════════════════════════════════
* Estimates: prose chars ÷ 4, code chars ÷ 3. Actual API usage varies by model.
```

---

## Skills Taxonomy

### Skill 1 — Browser Extraction

Load browser tools in a single `ToolSearch` call, then extract the page.

#### Step 1a — Load Tools (one call)

```
ToolSearch query: "select:mcp__claude-in-chrome__tabs_context_mcp,
                   mcp__claude-in-chrome__navigate,
                   mcp__claude-in-chrome__get_page_text,
                   mcp__claude-in-chrome__computer,
                   mcp__claude-in-chrome__tabs_create_mcp"
```

#### Step 1b — Get Tab Context

```
mcp__claude-in-chrome__tabs_context_mcp { createIfEmpty: true }
```

Always call this first. Never reuse stale tab IDs from a prior session.

#### Step 1c — Navigate to Gemini Share URL

```
mcp__claude-in-chrome__navigate {
  tabId: <fresh tab ID from 1b>,
  url:   "<share.gemini.google/...>"
}
```

Gemini share URLs redirect to `gemini.google.com/share/<id>?skid=...` — this is expected behavior. Do not treat the redirect as an error.

#### Step 1d — Extract Page Text

```
mcp__claude-in-chrome__get_page_text { tabId: <tabId> }
```

**Extraction coverage checklist:**

| Element | Expected in Output | Action if Missing |
|---|---|---|
| Session title | Yes — in `<h1>` or page title | Infer from first user prompt |
| Model + date metadata | Yes — "Created with X Flash-Lite..." | Extract if present, skip if absent |
| All "You said" prompts | Yes — every user turn | Scroll down and re-extract |
| All model responses | Yes — every Gemini response | Scroll down and re-extract |
| Footer chrome | Yes — strip in Phase 0 | Remove: Privacy, ToS, "Continue this chat" |

#### Step 1e — Scroll Pass (if content is truncated)

If the extracted text cuts off mid-conversation (no footer text visible):

```
mcp__claude-in-chrome__computer {
  action:           "scroll",
  tabId:            <tabId>,
  coordinate:       [760, 400],
  scroll_direction: "down",
  scroll_amount:    10
}
```

Then call `get_page_text` once more. Maximum **2 scroll passes** total.

---

### Skill 2 — Session Structure Analysis

After extraction and Token Optimization, parse the session into a structured inventory before writing anything.

#### 2.1 Identify Conversation Turns

Each Gemini share page contains alternating turns:
- **User turn:** Prefixed with "You said" in the raw text
- **Model turn:** The Gemini response following each user turn

Build a turn inventory:

```
TURN INVENTORY:
[ ] Turn 1 — User: <prompt summary> | Model: <response summary>
[ ] Turn 2 — User: <prompt summary> | Model: <response summary>
...
[ ] Turn N — (error / no response turns — note separately)
```

#### 2.2 Identify Distinct Concepts

From all successful model responses, extract every distinct concept or topic discussed:

```
CONCEPT INVENTORY:
[ ] Concept 1: <name> — Type: <architecture / pattern / system-design / DevOps>
[ ] Concept 2: <name> — Diagram needed: <flowchart TD / sequenceDiagram / stateDiagram-v2>
...
```

Mark any turns where Gemini returned an error or could not process the prompt — these become `> Note:` blockquotes in the output, not full sections.

#### 2.3 Detect Source Content Type

Inspect the user prompts for embedded URLs or content types:

| Detected Pattern in Prompt | Handling |
|---|---|
| `facebook.com/share/...` | Social media video/image — extract described architecture only |
| `youtube.com/watch?...` | Video reference — expand from described concepts |
| Architecture description in text | Generate Mermaid diagram recreating it |
| Code snippets or pseudocode | Preserve exactly + add language-appropriate equivalent |
| "Download as md" / "save session" | Note in output as a meta-request — no separate section needed |

---

### Skill 3 — Content Enrichment

For each concept in the inventory, expand beyond what Gemini provided.

| Source Content | Always Add |
|---|---|
| Mentions an architecture pattern | Full definition, when to use vs. alternatives, trade-offs |
| Lists components (Load Balancer, Queue, etc.) | Full role of each component, failure modes, config notes |
| Shows a process flow | Mermaid diagram recreating and extending it |
| Brief description of a pattern | Code example (language per Skill 4), interview Q&A |
| Error / failed Gemini turn | `> Note:` blockquote explaining what was attempted |
| "meta" requests ("save as md") | Skip — do not create a section for session meta-commands |

**Enrichment depth target:** Output should be **3–5x the raw Gemini response content** — every concept fully defined, not just summarized.

**Always add these, even if absent from the session:**
- Mermaid architecture diagram (at least one per concept)
- Interview Q&A block (min 5 rows per concept)
- Comparison table (Classic vs Modern / Before vs After) where applicable

---

### Skill 4 — Language & Tech Stack Detection

Before writing code, detect the technology context from the session:

```
IF session discusses .NET / C# / ASP.NET:
  → Use: BackgroundService, Polly, StackExchange.Redis, Confluent.Kafka, Minimal API

IF session discusses Java / Spring:
  → Map to .NET equivalents (see concept-txt-to-md-agent.md Skill 2 mapping table)
  → Or use Java idioms if user explicitly requested Java

IF session discusses Python / FastAPI / Flask:
  → Use: asyncio, FastAPI, Pydantic, asyncpg, httpx

IF session discusses system design / language-agnostic:
  → Default to Python for code examples (concise, widely understood)
  → Add .NET equivalent in a collapsible note or secondary block

IF session contains no code:
  → Add one minimal pseudocode block per architecture concept
```

---

### Skill 5 — Mermaid Diagram Generation

Generate at least **1 Mermaid diagram per concept**. Use the selection matrix and color palette below.

#### Diagram Type Selection Matrix

| Concept Type | Primary Diagram | Secondary Diagram |
|---|---|---|
| Traffic / load handling | `flowchart TD` (architecture layers) | `sequenceDiagram` (request flow) |
| Event-driven / sourcing pattern | `flowchart LR` (event pipeline) | `stateDiagram-v2` (state transitions) |
| Microservices architecture | `flowchart TD` with subgraphs | `sequenceDiagram` (inter-service) |
| Database / storage strategy | `flowchart LR` | None needed |
| Retry / resilience | `stateDiagram-v2` | `sequenceDiagram` (retry attempts) |
| System design (e-commerce, URL shortener) | `flowchart TD` with subgraphs | `sequenceDiagram` (request path) |
| Security / access control | `flowchart TD` nested | None needed |
| CI/CD / deployment pipeline | `flowchart LR` (stage pipeline) | None needed |

#### Mandatory Color Palette — Always use `classDef`, never inline `style nodeId fill:...`

#### Standard classDef Block (paste in every flowchart diagram)

```mermaid
flowchart TD
    ... (nodes and edges) ...

    classDef userNode    fill:#0078D4,stroke:#005A9E,color:#fff
    classDef aiNode      fill:#7719AA,stroke:#5A0E80,color:#fff
    classDef dataNode    fill:#107C10,stroke:#0A5C0A,color:#fff
    classDef processNode fill:#FF8C00,stroke:#CC7000,color:#fff
    classDef errorNode   fill:#E81123,stroke:#B30D1A,color:#fff
    classDef outputNode  fill:#00B294,stroke:#007D68,color:#fff
    classDef infraNode   fill:#605E5C,stroke:#3B3A39,color:#fff

    class NODE_ID1 userNode
    class NODE_ID2,NODE_ID3 aiNode
    class NODE_ID4 dataNode
```

#### Mermaid Syntax Safety Rules

| Issue | Wrong | Correct |
|---|---|---|
| Parentheses in labels | `node[Label (detail)]` | `node["Label (detail)"]` |
| Ampersands | `node[A & B]` | `node["A & B"]` |
| Forward slashes | `node[TCP/UDP]` | `node["TCP/UDP"]` |
| Colons in labels | `node[Key: Value]` | `node["Key: Value"]` |
| Percentages | `node[70%]` | `node["70%"]` |
| Hyphens in node IDs | `my-node["..."]` | `myNode["..."]` |
| Diagram > 20 nodes | One large diagram | Split into 2 diagrams |
| Long labels (>30 chars) | Single line | Use `\n` inside `"..."` |

---

### Skill 6 — Document Structure Template

Every output file must follow this structure:

```markdown
# [Session Title — clean, descriptive]

> **Source:** [share.gemini.google/...](url) → redirects to [gemini.google.com/share/...](url)
> **Model:** [Gemini model name, e.g., 3.1 Flash-Lite]
> **Session Date:** [date from page metadata]
> **Saved:** [today's date]

---

## Table of Contents

1. [Session Overview](#1-session-overview)
2. [Concept 1 Title](#2-concept-1-title)
3. [Concept 2 Title](#3-concept-2-title)
...
N. [Interview Q&A Cheatsheet](#n-interview-qa-cheatsheet)

---

## 1. Session Overview

[2–3 sentences: what this session covers, how many concepts, any notable failed turns]

### Session Map

| Turn | User Prompt Summary | Gemini Response | Status |
|---|---|---|---|
| 1 | ... | ... | ✅ Extracted |
| 2 | ... | ... | ✅ Extracted |
| 3 | ... | ... | ⚠️ Error — synthesized |

---

## 2. [Concept Title]

### Overview

[3–5 sentences: full definition, why it matters, where it fits in the architecture ecosystem]

### Architecture Diagram

```mermaid
flowchart TD
    ...
    classDef ... (full classDef block always present)
    class ...
```

### How It Works

[Step-by-step numbered explanation, 4–8 steps]

### Key Components

| Component | Role | Technology Options |
|---|---|---|
| ... | ... | ... |

### Code Example

```python
# or csharp, java, etc. — based on Skill 4 detection
...
```

### Interview Q&A

| Question | Answer |
|---|---|
| ... | ... |
| (min 5 rows) | |

---

## [N]. Interview Q&A Cheatsheet

[Consolidated Q&A from all concepts — 8–12 pairs]

**Q: [Question]?**
> [2–4 sentence answer using precise technical vocabulary]

---

*Extracted from Gemini shared session · [date] · GeminiShareToMD Agent v1.0*
```

---

### Skill 7 — File Naming & Placement

**Directory:** Always `/Users/rajaghosh/repo/InterviewPrep/gemini_task/`

**Filename rules:**

| Session Content | Filename Pattern |
|---|---|
| Single topic (e.g., Microservices Scaling) | `Microservices-Scaling-Architecture.md` |
| Multiple unrelated topics | `Gemini-Session-[YYYY-MM-DD].md` |
| System design deep dive | `[Topic]-System-Design.md` |
| Pattern-focused session (retry, CQRS, etc.) | `[Pattern-Name]-Complete.md` |
| Tutorial / how-it-works session | `[Topic]-Explained.md` |

**Rules:**
- Kebab-case, no spaces, no special characters
- Max 6 words
- Always `.md` extension
- Always in `gemini_task/` directory

**Collision handling:**

```bash
ls /Users/rajaghosh/repo/InterviewPrep/gemini_task/
# If a file covers the same session topic:
#   → UPDATE (add new sections) rather than create a duplicate
# If a different session on same topic:
#   → Append "-2" to filename
```

---

### Skill 8 — Error Turn Handling

Gemini sessions often contain turns where the model returned an error or could not process a Facebook/private URL. Handle these consistently:

```
IF Gemini response is an error ("I seem to be encountering an error",
   "Sorry, something went wrong", "I cannot directly view"):

  → DO NOT create a full section for this concept
  → DO insert a blockquote note immediately after the previous section:

  > **Note (Turn N):** Gemini was unable to process the URL `<url>`.
  > The requested content could not be extracted from this private/encrypted link.
  > If you have the video title or description, provide it to extract the learning content.

IF user prompt is a meta-request ("Download the total session as a md file",
   "save this conversation"):

  → Skip entirely — do not create a section
  → This is the purpose of the current agent run
```

---

### Skill 9 — Quality Validation Checklist

After writing the file, validate against this checklist:

```
EXTRACTION CHECKS:
[ ] All "You said" turns identified and processed
[ ] All successful Gemini responses captured and expanded
[ ] Error turns converted to blockquote notes (not full sections)
[ ] Meta-request turns skipped
[ ] Session metadata (model, date) present in file header

CONTENT CHECKS:
[ ] Every concept has a 3–5 sentence overview (not just the Gemini bullet points)
[ ] Every concept has at least 1 Mermaid diagram
[ ] Every concept has an Interview Q&A table (min 5 rows)
[ ] Code example present for every architecture concept
[ ] Session Map table present in Section 1

DIAGRAM CHECKS:
[ ] All diagrams use classDef (NOT per-node inline style)
[ ] Full color palette applied consistently
[ ] No node label contains unquoted: ( ) & / : %
[ ] No node ID contains hyphens (use camelCase)
[ ] No diagram exceeds 20 nodes (split if needed)

FILE CHECKS:
[ ] Saved to gemini_task/ directory
[ ] Filename is kebab-case, ≤ 6 words
[ ] Source URL present in header metadata block
[ ] Table of Contents with anchor links present
[ ] Ends with italicized footer line
[ ] No placeholder text [TODO] or [FILL IN] in output
[ ] TokenOptimizer report appended at end
```

---

## Full Agent Workflow

**Execution order (each phase gates the next):**

1. **Phase 0 — TokenOptimizer:** Strip UI chrome, deduplicate, compact prose, store TOKEN_REPORT
2. **Phase 1 — Browser Extraction:** ToolSearch (1 call) → `tabs_context_mcp` → `navigate` → `get_page_text` → scroll if truncated (max 2 passes)
3. **Phase 2 — Analyse:** Build turn inventory, extract concept inventory, detect tech stack, flag error turns
4. **Phase 3 — Enrich & Plan:** Plan diagrams per concept (Skill 5 matrix), plan enrichment, determine filename and dedup check
5. **Phase 4 — Write:** Single `Write` call — full `.md` at 3–5x source length; append TOKEN_REPORT
6. **Phase 5 — Validate:** Run Skill 9 checklist; fix any missing diagrams or incomplete sections before reporting done

---

---

*Agent Skill v1.0 | GeminiShareToMD Agent | Created July 2026*
