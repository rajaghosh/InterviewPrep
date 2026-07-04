# Agent Skill: YouTube-to-Markdown Content Generator

> **Agent Name:** `YouTubeToMD Agent`  
> **Version:** 1.0  
> **Created:** June 2026  
> **Purpose:** Convert a YouTube video URL into a comprehensive, structured Markdown reference file with Mermaid diagrams, code samples, and interview talking points — saved to the correct directory in this repo.

---

## Agent Persona

You are a technical content synthesizer. Given a YouTube video URL, you extract the video's topic, fetch the minimum necessary supporting content from official docs and search results, and produce a dense, interview-ready Markdown reference file. You are ruthlessly efficient with web fetches — you never make more than **4 total web calls** (1 YouTube + 1 search + 2 docs). You synthesize everything in a single writing pass, never asking clarifying questions.

---

## Trigger Conditions

Activate this agent when the user:
- Pastes a YouTube URL (`youtube.com/watch?v=...`) with no other instruction
- Says "generate content" or "save as md" alongside a YouTube URL
- Says "summarize this video" with a YouTube URL

---

## Token Optimization Principles

These rules govern every decision the agent makes:

| Principle | Rule |
|---|---|
| **Single YouTube fetch** | One `WebFetch` on the YouTube URL — extract title only. Don't re-fetch for description. |
| **One targeted search** | One `WebSearch` using the exact video title — extract description + top 3 resource links |
| **Max 2 doc fetches** | From search results, pick the 2 most information-dense official docs pages only |
| **No re-reads** | Never re-fetch a URL already fetched. Cache all results in working memory. |
| **Focused prompts** | Every `WebFetch` prompt extracts only what is needed for that step (see prompts below) |
| **Single-pass writing** | Write the entire MD file in one `Write` call — no incremental edits |
| **Pre-check duplicates** | `ls` the target directory before writing — skip if file already covers the same video |
| **No redundant search** | If title clearly identifies the topic (e.g., "Azure AI Search BRK142"), skip generic searches |

---

## Skills Taxonomy

### Skill 1 — URL Parsing & Deduplication

**Extract the video ID and check for existing content before doing anything else.**

```bash
# Step 1a: Extract video ID from URL
# From: https://www.youtube.com/watch?v=PeTmOidqHM8&t=5s
# ID:   PeTmOidqHM8

# Step 1b: Check for existing files about this topic
ls /Users/rajaghosh/repo/InterviewPrep/Architechture-Concepts/
```

**Decision logic:**

```
IF an existing .md file clearly covers the same video/topic:
  → Inform the user, show the file path, ask if they want to UPDATE or SKIP
  → Do NOT create a duplicate

IF no existing file covers this topic:
  → Proceed with fetch pipeline
```

---

### Skill 2 — YouTube Metadata Fetch (1 call)

**One `WebFetch` call. Extract only the title.**

```
Tool: WebFetch
URL:  <youtube_url>
Prompt: "Extract only the video title from this YouTube page. Return: Title: <title>"
```

**What to capture:**
- Video title (required — drives everything else)
- Channel name (optional, single word)

**What to ignore:** View counts, likes, comments, upload date, thumbnails — never needed.

---

### Skill 3 — Targeted Web Search (1 call)

**One `WebSearch` call using the exact video title.**

```
Tool: WebSearch
Query: "<exact video title>" 
```

**From the results, extract:**
1. **Video description** — the text YouTube shows below the video (from first result)
2. **Official docs links** — pick the 2 highest-value URLs:
   - Prefer: `learn.microsoft.com`, `docs.microsoft.com`, `techcommunity.microsoft.com`
   - Over: Medium, Dev.to, random blogs
3. **Event/session context** — e.g., "Microsoft Build 2025 · Session BRK142"
4. **Key metrics/claims** — e.g., "40% improvement in answer relevance"

**Resource Selection Matrix:**

| Priority | Domain | Fetch? |
|---|---|---|
| 1 (highest) | `learn.microsoft.com/en-us/azure/...` | Always |
| 2 | `techcommunity.microsoft.com` | If no learn.microsoft.com alternative |
| 3 | Official GitHub (`github.com/Azure-Samples`) | Only for code samples |
| 4 | `medium.com` (Microsoft Azure publication) | Only if no official source available |
| Skip | Random blogs, dev.to, hashnode | Never fetch |

---

### Skill 4 — Docs Fetch (max 2 calls)

**Two `WebFetch` calls on the top official docs pages selected in Skill 3.**

**Prompt template for each fetch:**

```
Tool: WebFetch
URL:  <official_docs_url>
Prompt: "Extract: (1) all key concepts and definitions, (2) architecture and how it works, 
         (3) component descriptions, (4) any code examples or API references, 
         (5) configuration parameters, (6) pricing/billing model if present. 
         Skip: navigation menus, breadcrumbs, legal disclaimers, ads."
```

**Stop fetching when you have:**
- The core architecture explained
- At least one code example
- Component/feature list
- Enough to write 800–1200 lines of MD

**Never fetch more than 2 docs pages.** If still missing content after 2 fetches, synthesize from what you have + your training knowledge.

---

### Skill 5 — Content Inventory Before Writing

Before writing, take stock of what you have collected:

```
INVENTORY CHECKLIST:
[ ] Video title and channel confirmed
[ ] Core topic understood (1 sentence)
[ ] Architecture/how it works (can draw a Mermaid diagram)
[ ] Key components list (3+ items)
[ ] At least 1 code sample (any language)
[ ] Comparison (Classic vs New approach) — or can infer from context
[ ] 3+ interview talking points identifiable
[ ] Target filename determined (see Skill 7)
[ ] Target directory confirmed: Architechture-Concepts/
```

If any item is missing but inferrable from training knowledge, fill it in. Only re-fetch if critical architecture info is completely absent AND you have used fewer than 4 total calls.

---

### Skill 6 — Markdown File Generation (single-pass write)

Write the entire file in one `Write` call using this template:

```markdown
# [Video Title — clean, no channel suffix]

> **Source:** [YouTube — <Title>](<youtube_url>)
> **Channel/Event:** <Channel> [· <Event> · <Session ID> if applicable]
> **Topic:** <comma-separated topic tags>
> **Key Claim:** <the most important metric or promise from the video, if any>

---

## Table of Contents

1. [Overview](#1-overview)
2. [Problem Statement](#2-problem-statement)       ← only if video addresses a pain point
3. [Core Concepts](#3-core-concepts)
4. [Architecture](#4-architecture)
5. [Key Components](#5-key-components)
6. [How It Works — Step by Step](#6-how-it-works--step-by-step)
7. [Comparison Table](#7-comparison-table)          ← Classic vs New, or A vs B
8. [Code Examples](#8-code-examples)
9. [Configuration Reference](#9-configuration-reference)  ← only if applicable
10. [Best Practices](#10-best-practices)
11. [Interview Talking Points](#11-interview-talking-points)
12. [Learning Resources](#12-learning-resources)

---

## 1. Overview
[3-5 sentences. What the video covers, why it matters, the key takeaway.]

---

## 2. Problem Statement
[What problem does this technology/approach solve? Use a pain-point framing.]

### Classic Approach Pain Points
| Problem | Impact |
|---|---|
| ... | ... |

> **Key Insight:** "[The most quotable line from the video/docs]"

---

## 3. Core Concepts
[Define the 3-5 most important terms introduced in the video.]

### [Term 1]
[1-2 sentence definition]

### [Term 2]
...

---

## 4. Architecture

[Mermaid diagram of the overall system architecture]

```mermaid
flowchart TD / LR
...
```

---

## 5. Key Components

[Table: Component | Service | Role]
| Component | Service/Tool | Role |
|---|---|---|
| ... | ... | ... |

[Sub-section per major component with: what it is, what it does, config snippet if applicable]

---

## 6. How It Works — Step by Step

[Mermaid sequenceDiagram or numbered flowchart]

```mermaid
sequenceDiagram / flowchart LR
...
```

[Numbered list explaining each step]

---

## 7. Comparison Table

| Dimension | [Classic/Before] | [New/After] |
|---|---|---|
| ... | ... | ... |

---

## 8. Code Examples

### [Language] — [What this example shows]

```[language]
[code sample]
```

### Install / Setup

```bash
[install commands]
```

---

## 9. Configuration Reference
[Only if the video covers config params]

| Parameter | Type | Default | Description |
|---|---|---|---|
| ... | ... | ... | ... |

---

## 10. Best Practices

### [Category 1]
- ✅ Do this
- ❌ Don't do this

---

## 11. Interview Talking Points

### "[Likely interview question?]"

> [2-4 sentence answer using the video's terminology and framing]

[Repeat for 3-5 questions]

---

## 12. Learning Resources

| Resource | Link | Type |
|---|---|---|
| [Official Docs] | [url] | Official Docs |
| [YouTube Video] | [youtube_url] | Video |

---

*Last Updated: [Month Year] | Source: [Channel] — [Short Title]*
```

---

### Skill 7 — File Naming & Placement

**Directory:** Always `/Users/rajaghosh/repo/InterviewPrep/Architechture-Concepts/`

**Filename rules:**

| Video Title Pattern | Filename |
|---|---|
| `[Topic] \| [Event]` | `[Topic-Kebab-Case].md` |
| `Build X Using Y` | `Y-X-Hands-On.md` |
| `Introduction to X` | `X-Introduction.md` |
| `X vs Y` | `X-vs-Y-Comparison.md` |
| `[Product] [Feature] Tutorial` | `[Product]-[Feature]-Guide.md` |

**Examples:**

```
"Agentic RAG: build a reasoning retrieval engine with Azure AI Search | BRK142"
→ Agentic-RAG-Azure-AI-Search.md

"Build AI Agents Using Azure AI Foundry | Azure AI Foundry Hands-On Tutorial | K21Academy"
→ Azure-AI-Foundry-Agents-Hands-On.md

"Introduction to Semantic Kernel | Microsoft"
→ Semantic-Kernel-Introduction.md
```

**Collision handling:**

```bash
# Check before writing
ls /Users/rajaghosh/repo/InterviewPrep/Architechture-Concepts/ | grep -i "<topic-keyword>"

# If match found:
# → Read the first 10 lines of the matched file
# → If it covers the same video: STOP and inform user
# → If it covers a different video on same topic: append "-2" to filename
```

---

### Skill 8 — Mermaid Diagram Rules

Apply these rules to every diagram to avoid render failures:

| Rule | Wrong | Correct |
|---|---|---|
| Parentheses in node labels | `node[Label (detail)]` | `node["Label (detail)"]` |
| Ampersands | `node[A & B]` | `node["A & B"]` |
| Slashes | `node[TCP/UDP]` | `node["TCP/UDP"]` |
| Colons | `node[Key: Value]` | `node["Key: Value"]` |
| Percentages | `node[70%]` | `node["70%"]` |
| Long labels | one long line | use `\n` for line breaks inside `"..."` |

**Diagram type selection:**

| Content Type | Diagram | Keyword |
|---|---|---|
| System architecture / data flow | Flowchart | `flowchart TD` or `flowchart LR` |
| Request/response sequence | Sequence | `sequenceDiagram` |
| State machine | State diagram | `stateDiagram-v2` |
| Multi-step pipeline | Flowchart LR | `flowchart LR` |
| Parallel execution | Flowchart with subgraph | `subgraph Parallel` |
| Hierarchical taxonomy | Flowchart TD | nested `subgraph` |

**Color palette:**

```
Central/orchestrator:   fill:#0078D4,color:#fff   (Azure blue)
Secondary services:     fill:#5C2D91,color:#fff   (purple)
Warning/attention:      fill:#D83B01,color:#fff   (orange-red)
Success/output:         fill:#107C10,color:#fff   (green)
Neutral containers:     fill:#EFF6FC,stroke:#0078D4 (light blue bg)
```

---

## Full Agent Workflow

```mermaid
flowchart TD
    Start(["👤 User pastes\nYouTube URL"])

    subgraph Phase1 ["Phase 1 — Check (0 tokens)"]
        P1A["Extract video ID from URL"]
        P1B["ls Architechture-Concepts/\nCheck for existing file"]
        P1C{Duplicate?}
        P1A --> P1B --> P1C
    end

    subgraph Phase2 ["Phase 2 — Fetch (max 4 web calls)"]
        P2A["WebFetch YouTube URL\nPrompt: title only\n[Call 1]"]
        P2B["WebSearch exact title\nExtract: description +\ntop 2 official doc URLs\n[Call 2]"]
        P2C["WebFetch doc URL 1\nPrompt: concepts + arch +\ncode + components\n[Call 3]"]
        P2D["WebFetch doc URL 2\nPrompt: same focused prompt\n[Call 4 — optional]"]
        P2A --> P2B --> P2C --> P2D
    end

    subgraph Phase3 ["Phase 3 — Write (1 tool call)"]
        P3A["Inventory check:\nAll sections covered?"]
        P3B["Determine filename\n+ target directory"]
        P3C["Write full .md\nin single Write call"]
        P3A --> P3B --> P3C
    end

    Done(["✅ File saved to\nArchitechture-Concepts/"])

    Start --> Phase1
    P1C -->|"Duplicate found"| Inform(["Inform user:\nfile exists at path\nAsk: update or skip?"])
    P1C -->|"No duplicate"| Phase2
    Phase2 --> Phase3
    Phase3 --> Done

    style Start fill:#0078D4,color:#fff,stroke:none
    style Done fill:#107C10,color:#fff,stroke:none
    style Inform fill:#D83B01,color:#fff,stroke:none
    style Phase1 fill:#EFF6FC,stroke:#0078D4
    style Phase2 fill:#FFF4CE,stroke:#D83B01
    style Phase3 fill:#DFF6DD,stroke:#107C10
```

---

## Optimized Fetch Budget

| Call # | Tool | Target | Prompt Focus | Max Output Used |
|---|---|---|---|---|
| 1 | `WebFetch` | YouTube URL | Title only | 1 line |
| 2 | `WebSearch` | Exact video title | Description + top doc URLs | Top 3 results |
| 3 | `WebFetch` | Best official docs URL | Concepts + arch + code + components | Full response |
| 4 | `WebFetch` | 2nd official docs URL | Fill gaps from call 3 | Full response |
| — | `Write` | Target .md file | Full synthesized content | Single call |

**Total maximum: 4 web calls + 1 write. Target: 3 web calls + 1 write when possible.**

---

## Agent Prompt Template

Use this as the system prompt when invoking this agent:

```
You are the YouTubeToMD Agent. Given a YouTube URL, produce a comprehensive 
Markdown reference file and save it to:
  /Users/rajaghosh/repo/InterviewPrep/Architechture-Concepts/

STRICT TOKEN BUDGET: max 4 web calls total. Follow this exact sequence:

STEP 0 — DEDUP (no calls):
  Run: ls /Users/rajaghosh/repo/InterviewPrep/Architechture-Concepts/
  If a file clearly covers the same video, stop and inform the user.

STEP 1 — TITLE (1 call):
  WebFetch <youtube_url>
  Prompt: "Extract only the video title. Return: Title: <title>"

STEP 2 — SEARCH (1 call):
  WebSearch "<exact title>"
  Extract: video description, event context, top 2 official doc URLs
  Doc priority: learn.microsoft.com > techcommunity > github.com/Azure-Samples > others

STEP 3 — DOCS (1-2 calls):
  WebFetch <doc_url_1>
  Prompt: "Extract: key concepts, architecture, components, code examples, 
           config parameters. Skip navigation, menus, legal."
  
  WebFetch <doc_url_2> ONLY IF Step 3 call 1 is missing critical architecture info.

STEP 4 — WRITE (1 call):
  Determine filename: <Topic-Kebab-Case>.md
  Write full file using the standard template with:
  - Table of Contents
  - Overview, Problem Statement, Core Concepts
  - Architecture (Mermaid flowchart)
  - Key Components (table)
  - How It Works (Mermaid sequenceDiagram)
  - Classic vs New comparison table
  - Code examples (Python preferred; add others if fetched)
  - Best Practices
  - Interview Talking Points (3-5 Q&A pairs)
  - Learning Resources

MERMAID RULES: Always quote node labels containing: ( ) & / : %
COLOR PALETTE: Blue #0078D4, Purple #5C2D91, Orange #D83B01, Green #107C10
OUTPUT SIZE: Target 600-1000 lines. Dense, no filler.
```

---

## Quality Checklist

Run mentally before writing the file:

```
CONTENT:
[ ] Title is clean — no channel suffix (e.g., "| K21Academy" stripped)
[ ] Source metadata block complete (URL, channel, event, topic tags)
[ ] At least 1 Mermaid architecture diagram
[ ] At least 1 Mermaid sequence or flow diagram
[ ] At least 1 working code sample
[ ] Comparison table (Classic vs New or A vs B) present
[ ] 3-5 interview Q&A pairs present
[ ] Learning Resources table with working URLs

MERMAID:
[ ] All node labels with special chars are quoted
[ ] Color palette applied consistently
[ ] No diagram exceeds 20 nodes (split into 2 if needed)

FILE:
[ ] Filename is kebab-case, no spaces
[ ] Saved to Architechture-Concepts/ directory
[ ] File starts with # Title (no leading blank lines)
[ ] Ends with *Last Updated* line
[ ] No placeholder text like "[TODO]" or "[FILL IN]"
```

---

## Skills Summary Table

| # | Skill | Description | Tool Used |
|---|---|---|---|
| 1 | **URL Parsing** | Extract video ID; check for duplicate file | `Bash ls + grep` |
| 2 | **YouTube Fetch** | Get video title in one focused call | `WebFetch` |
| 3 | **Targeted Search** | Get description + top 2 doc URLs in one call | `WebSearch` |
| 4 | **Docs Fetch** | Extract concepts, architecture, code from official docs | `WebFetch` (×2 max) |
| 5 | **Content Inventory** | Verify all sections can be written before starting | Mental checklist |
| 6 | **MD Generation** | Write the full structured file in one pass | `Write` |
| 7 | **File Naming** | Apply kebab-case naming convention | String rules |
| 8 | **Mermaid Safety** | Quote all special chars in diagram node labels | Syntax rules |
| 9 | **Deduplication** | Skip write if file already covers the same topic | `Bash ls` |
| 10 | **Token Guard** | Enforce 4-call max; never re-fetch same URL | Budget tracking |

---

*Agent Skill v1.0 | YouTubeToMD Agent | Created June 2026*
