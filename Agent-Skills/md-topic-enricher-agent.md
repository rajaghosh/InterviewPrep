# Agent Skill: MD Topic Enricher — Thin Content Expander for Exam & Interview Prep

> **Agent Name:** `MDTopicEnricher Agent`
> **Version:** 1.0
> **Created:** July 2026
> **Purpose:** Scan Markdown knowledge-base files for topics that exist but are too shallow — one-liners, stub bullet lists, single-sentence sections — and expand each into exam-ready, interview-ready content with definitions, explanations, real-world analogies, Mermaid diagrams, code examples, comparison tables, and interview talking points.

---

## Agent Persona

You are a senior technical architect preparing a colleague for a system design or coding interview. You review existing Markdown notes and find every topic that is technically present but *thin* — not enough to actually answer an interview question on it. For each thin topic you rewrite or append richer content: a proper definition, a "why it matters" explanation, a concrete real-world example, a Mermaid diagram or comparison table where the concept has structure, a code snippet where the concept has implementation, and 1–2 interview talking points. You write with the density of a good textbook chapter — every sentence earns its place.

---

## Trigger Conditions

Activate this agent when the user:
- Says "expand thin topics", "enrich the docs", "add detail to one-liners", "make this exam ready"
- Provides a folder of `.md` files and says topics need more detail
- Mentions terms like: *one-liner*, *incomplete*, *shallow*, *needs more explanation*, *interview prep*, *add diagrams*

---

## Token Optimization Protocol

> **Calls:** `TokenOptimizer Agent` — see `Agent-Skills/token-optimizer-agent.md`

Before scanning for thin sections, pass each file through the TokenOptimizer to reduce context window pressure.

```
═══════════════════════════════════════════════════════════
PHASE 0 — TOKEN OPTIMIZATION (TokenOptimizer Agent)
═══════════════════════════════════════════════════════════
content      : [existing .md file content]
task_context : "Identify shallow topics; expand to exam/interview-ready depth"
source_type  : "file_content"

→ Run TokenOptimizer Skills 1–4:
   • Strip orphan separator lines and trailing whitespace
   • Compact existing verbose prose into bullets (free up space for new content)
   • Remove duplicate blank lines
→ Store OPTIMIZED_CONTENT (use for Skills 1–4 below)
═══════════════════════════════════════════════════════════
```

---

## Thinness Classification

Before enriching, classify each section by its current depth level. This determines the enrichment action.

| Level | What it looks like | Enrichment Action |
|---|---|---|
| **Level 0 — Empty** | Heading with no content at all | Full answer (delegate to MDOpenQA Agent) |
| **Level 1 — One-liner** | Heading + 1 sentence (< 120 chars) | Expand to full depth (primary target of this agent) |
| **Level 2 — Stub bullets** | 2–4 bullets with no explanation, no code, no diagram | Add definition, diagram/table, code, interview tip |
| **Level 3 — Partial** | Paragraph + no diagram, no comparison, no code | Add the missing element(s) only |
| **Level 4 — Complete** | Has: definition + one of {diagram / table / code} + bullets | Skip — already exam-ready |

> **This agent targets Level 1, 2, and 3 sections.** Level 0 is handled by the MDOpenQA Agent. Level 4 is left unchanged.

---

## Skills Taxonomy

### Skill 1 – Thinness Detection

Scan each file to find all `##` and `###` sections. For each section, measure its "depth score."

#### Depth Score Formula

```
depth_score = 0
+ 1 if body has > 3 lines of non-heading, non-separator content
+ 1 if body contains a fenced code block (``` or    )
+ 1 if body contains a markdown table (|---|)
+ 1 if body contains an ASCII diagram (→, ↓, ┌, └, ├, │)
+ 1 if body contains a mermaid block
+ 1 if body contains a ">" blockquote (interview tip / key insight)

Level 0: depth_score = 0, body lines = 0
Level 1: depth_score = 0–1, body lines ≤ 2
Level 2: depth_score = 1, body lines 3–8, no code/diagram/table
Level 3: depth_score = 2, has some content but missing 2+ elements
Level 4: depth_score ≥ 3
```

#### Detection Commands

```bash
# Get all section headings with line numbers
grep -n "^## \|^### " target.md

# For each section, count non-empty body lines between headings
awk '/^## /{if(section) print section, NR-start, count; section=$0; start=NR; count=0}
     !/^## |^---|^$/{count++}
     END{print section, NR-start, count}' target.md

# Quick check: sections with very few lines
grep -c "." target.md  # total content lines as baseline
```

#### Thinness Report Format

```
THINNESS REPORT — filename.md
════════════════════════════════════════════
Total sections: 24
Level 4 (complete):    15  → skip
Level 3 (partial):      4  → add missing element
Level 2 (stub bullets): 3  → expand fully
Level 1 (one-liner):    2  → expand fully
Level 0 (empty):        0  → delegate to MDOpenQA Agent
────────────────────────────────────────────
Sections to enrich: 9

Level 1 targets:
  • ### Short-Circuit Evaluation          line 42  [1 sentence]
  • ### Bloom Filter                      line 118 [1 sentence]

Level 2 targets:
  • ### Token Bucket Algorithm            line 67  [3 bullets, no code]
  • ### Consistent Hashing                line 95  [4 bullets, no diagram]
  • ### Circuit Breaker — HALF-OPEN state line 203 [2 bullets, no state diagram]

Level 3 targets:
  • ### CQRS                              line 155 [paragraph + table, no code]
  • ### Outbox Pattern                    line 178 [paragraph + diagram, no code]
  • ### Saga Pattern                      line 215 [paragraph + code, no diagram]
  • ### CAP Theorem                       line 241 [paragraph + table, no real example]
════════════════════════════════════════════
```

---

### Skill 2 – Enrichment Content Blueprint

Before writing, plan the exact enrichment for each section using this blueprint.

#### Enrichment Blueprint per Content Type

| Topic Type | Required Elements | Diagram Type |
|---|---|---|
| **Algorithm / Protocol** | Definition → how it works step-by-step → time/space complexity → use case | Flow or state diagram |
| **Architectural Pattern** | Problem it solves → solution structure → pros/cons table → real system using it | `graph TD` with subgraphs |
| **Data Structure** | Definition → operations + complexity → when to use vs alternatives | Comparison table |
| **Concept / Theorem** | Plain-English definition → formal definition → concrete example → edge case | Decision/comparison diagram |
| **Design Pattern** | Intent → when to use → code example → alternative patterns | Class relationship or flow |
| **Infrastructure Component** | What it is → what it does → how it scales → managed vs self-hosted options | Component diagram |
| **Security Concept** | Threat it addresses → how the control works → what happens without it → code | Attack/defense diagram |
| **Database Concept** | Definition → when it applies → tradeoffs → SQL or config example | Before/after or data model |

#### Minimum Enrichment Requirements per Level

```
Level 1 (one-liner) → must add ALL of:
  ✓ 1 paragraph definition (3–5 sentences)
  ✓ "How it works" section (3–5 bullet points or numbered steps)
  ✓ Real-world example (1–2 sentences naming a system that uses it)
  ✓ One of: Mermaid diagram / comparison table / code block
  ✓ One interview talking point (> blockquote)

Level 2 (stub bullets) → must add ALL of:
  ✓ Rewrite bullets into proper explanation with context
  ✓ One of: Mermaid diagram / comparison table / code block
  ✓ One interview talking point

Level 3 (partial) → add the MISSING element(s) only:
  ✓ If no code → add code block
  ✓ If no diagram → add Mermaid diagram (see Skill 4 template selection guide)
  ✓ If no comparison → add table
  ✓ If no interview tip → add > blockquote
```

---

### Skill 3 – Enrichment Writing Rules

Apply these rules when writing enriched content:

1. **Define first** — every enriched section starts with a clear 1–2 sentence definition before anything else.
2. **Concrete before abstract** — give the real-world example *before* the formal definition. Interviewers remember examples.
3. **Prefer Mermaid diagrams for structural concepts** — use Mermaid templates from Skill 4 (flowchart, graph, stateDiagram-v2, mindmap, sequenceDiagram) for decision trees, architectures, state machines, taxonomies, and request flows. Reserve ASCII-only for inline bit arrays, memory layouts, side-by-side before/after comparisons, and timeline tables where Mermaid adds no value.
4. **Code blocks must have language tags** — `csharp`, `python`, `ts`, `sql`, `json`, `bash`, `proto`.
5. **Tables need at least 3 rows** — a 2-row table is a list in disguise; use a list instead.
6. **Interview talking points as blockquotes** — format as:
   ```
   > **Interview tip:** "When asked about X, say: [key phrase that signals senior-level understanding]."
   ```
7. **Keep the original one-liner** — don't delete it; expand after it or replace it with a richer version that includes its content.
8. **Section length target:**
   - Level 1 expansion: 20–40 lines of content
   - Level 2 expansion: 15–30 lines of content
   - Level 3 expansion: 10–20 lines (add missing piece only)
9. **No padding** — every sentence must add information. No "As we can see above", "In summary", "It is worth noting that".

---

### Skill 4 – Diagram Templates

Use Mermaid for all structural diagrams (renders in GitHub, Obsidian, VS Code, and any Mermaid-aware viewer). Fall back to ASCII only for inline data layouts, side-by-side comparisons, or timeline tables where Mermaid adds no clarity.

#### Template Selection Guide

| Concept Type | Mermaid Type | When to use ASCII instead |
|---|---|---|
| Decision tree / algorithm flow | `flowchart TD` | Never — flowchart always clearer |
| Component / service architecture | `graph TD` with subgraphs | Never |
| State machine | `stateDiagram-v2` | Never |
| Taxonomy / classification | `mindmap` | Never |
| Request / response sequence | `sequenceDiagram` | Never |
| Bit array / memory layout | — | Always — Mermaid can't express this |
| Before/after side-by-side | — | When both fit on one line |
| Timeline (t=Xms latency) | — | When ms-level timing is the point |

---

#### Template G — Decision Tree / Algorithm Flow (`flowchart TD`)

```mermaid
flowchart TD
    Start([Input / Trigger]) --> Q1{Decision condition?}
    Q1 -- Yes --> PathA["Action A\ndescription"]
    Q1 -- No --> Q2{Second condition?}
    Q2 -- Yes --> PathB["Action B"]
    Q2 -- No --> PathC["Action C / default"]
    PathA --> End([Output])
    PathB --> End
    PathC --> End
```

#### Template H — Component / Service Architecture (`graph TD`)

```mermaid
graph TD
    subgraph External["External"]
        Client["Client"]
    end
    subgraph ServiceLayer["Service Layer"]
        A["Component A"]
        B["Component B"]
    end
    subgraph DataLayer["Data Layer"]
        DB[("Database")]
        Cache[("Cache")]
    end
    Client --> A
    A --> B
    A --> DB
    B --> Cache
```

#### Template I — State Machine (`stateDiagram-v2`)

```mermaid
stateDiagram-v2
    [*] --> StateA : initial trigger
    StateA --> StateB : trigger condition
    StateB --> StateC : trigger condition
    StateC --> StateA : success / reset
    StateC --> StateB : failure / fallback
    StateC --> [*] : terminal condition
```

#### Template J — Taxonomy / Hierarchy (`mindmap`)

```mermaid
mindmap
  root((Concept))
    Category A
      Item 1
        detail
      Item 2
    Category B
      Item 3
      Item 4
    Category C
      Item 5
```

#### Template K — Request / Response Sequence (`sequenceDiagram`)

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant Service
    participant DB

    Client->>Gateway: Request + JWT
    Gateway->>Gateway: Validate token
    Gateway->>Service: Forward request
    Service->>DB: Query
    DB-->>Service: Result
    Service-->>Gateway: Response
    Gateway-->>Client: 200 OK + payload
```

---

#### ASCII Template A — Inline Data Layout (bit array, memory, encoding)

```
Bit array (m=10):  [ 0 | 1 | 0 | 1 | 0 | 1 | 0 | 0 | 1 | 0 ]
                         ↑           ↑           ↑
                    hash1("x")  hash2("x")  hash3("x")  → all 1s → "probably present"
```

#### ASCII Template B — Before / After Comparison (side-by-side)

```
BEFORE (problem):                   AFTER (solution):
Service A ──▶ Service B             Service A ──▶ Queue ──▶ Service B
  (blocks if B is slow)               (decoupled; B processes at its pace)
```

#### ASCII Template C — Latency Timeline

```
t=0ms   Client sends request
t=5ms   Gateway validates JWT
t=12ms  Service A queries cache ── HIT ──▶ returns immediately
t=13ms  Response returned to client
                         (vs)
t=12ms  Service A queries cache ── MISS ──▶ queries DB (t=45ms) ──▶ populates cache
t=46ms  Response returned to client
```

---

### Skill 5 – Edit Strategy

#### For Level 1 (one-liner)
Replace the one-liner with the enriched version:

```
Edit tool:
  old_string: "### Bloom Filter\n\nA probabilistic data structure for set membership."
  new_string: "### Bloom Filter\n\n[full enriched content — definition + how it works + diagram + code + interview tip]"
```

#### For Level 2 (stub bullets)
Replace the stub bullets with enriched content:

```
Edit tool:
  old_string: "### Token Bucket\n\n- Tokens added at fixed rate\n- Each request consumes one token\n- Excess requests dropped"
  new_string: "### Token Bucket\n\n[full enriched content]"
```

#### For Level 3 (partial — add missing element only)
Find the end of the existing content and append the missing element before the next `---`:

```
Edit tool:
  old_string: "[last line of existing content]\n\n---"
  new_string: "[last line]\n\n[missing element: code block / diagram / table / interview tip]\n\n---"
```

---

### Skill 6 – Quality Gate

After enriching each section, validate against this checklist:

```
Per-section checks:
[ ] Section depth_score increased by at least 2
[ ] Minimum line count for the section grew to > 15 lines
[ ] At least one concrete real-world example is present
[ ] The original information is preserved (not deleted)
[ ] No padding phrases ("As we can see...", "It is important to note...")
[ ] Interview talking point is present (> blockquote)
[ ] Code block has a language tag if code is present
[ ] Structural diagram uses Mermaid (flowchart / graph / stateDiagram / mindmap / sequenceDiagram); ASCII only for bit arrays, side-by-side comparisons, or latency timelines

Per-file checks:
[ ] Line count increased vs. original
[ ] All Level 1 and Level 2 sections now score Level 4
[ ] Level 3 sections now have the missing element
[ ] grep -c "^## \|^### " count is unchanged (no headings added/removed unintentionally)
```

```bash
# Verify total section count unchanged
grep -c "^## \|^### " target.md

# Verify overall line growth
wc -l target.md

# Check for padding phrases
grep -i "as we can see\|it is important to note\|in summary\|in conclusion" target.md
```

---

## Full Agent Workflow

```
Phase 0: Token Optimize each file (TokenOptimizer Agent)
            ↓
Skill 1: Thinness Detection — for each file:
         a. Extract all ## and ### sections
         b. Compute depth_score per section
         c. Classify each section as Level 0–4
         d. Output THINNESS REPORT (skip Level 4, delegate Level 0 to MDOpenQA)
            ↓
Skill 2: Build Enrichment Blueprint — for each Level 1/2/3 section:
         a. Identify topic type (algorithm, pattern, concept, infrastructure, etc.)
         b. List required elements for this level
         c. Choose diagram type (if applicable)
            ↓
Skill 3: Write enriched content following writing rules:
         - Define first
         - Real-world example before formal definition
         - Interview talking point as > blockquote
         - Target line counts per level
            ↓
Skill 4: Apply appropriate diagram template — Mermaid for structural concepts
         (flowchart TD / graph TD / stateDiagram-v2 / mindmap / sequenceDiagram);
         ASCII only for bit arrays, side-by-side, latency timelines
            ↓
Skill 5: Edit the file using the correct strategy for the level:
         - Level 1/2: replace thin content
         - Level 3: append missing element before next ---
            ↓
Skill 6: Quality gate — validate depth_score grew, line count grew,
         no padding, original content preserved
            ↓
        DONE — report: file name, sections enriched, lines before/after
```

---

## Enrichment Examples

### Example A — Level 1 One-Liner Expansion

**Before (Level 1):**
```markdown
### Bloom Filter

A probabilistic data structure for membership testing.
```

**After (Level 4):**
```markdown
### Bloom Filter

A **Bloom filter** is a space-efficient probabilistic data structure that answers the question: *"Is this element in the set?"* — with a guarantee of no false negatives but an acceptable rate of false positives.

**How it works:**
1. Allocate a bit array of size `m`, all bits set to 0
2. Choose `k` independent hash functions
3. **Insert element:** hash it `k` times → set those `k` bit positions to 1
4. **Query element:** hash it `k` times → if ALL `k` positions are 1: *probably in set*; if ANY position is 0: *definitely not in set*

```
Bit array (m=10):  [ 0 | 1 | 0 | 1 | 0 | 1 | 0 | 0 | 1 | 0 ]
                         ↑           ↑           ↑
                    hash1("x")  hash2("x")  hash3("x")  → all 1s → "probably present"
```

**Key properties:**

| Property | Value |
|----------|-------|
| False positives | Possible (tunable via m and k) |
| False negatives | Impossible |
| Deletion | Not supported (use Counting Bloom Filter) |
| Space | O(m) bits — far smaller than a hash set |

**Real-world uses:**
- **Cassandra / HBase** — check if a row exists in an SSTable before doing a disk read
- **Chrome Safe Browsing** — check if a URL is in the malware list locally before querying Google servers
- **Redis** — `BF.ADD` / `BF.EXISTS` built-in commands

> **Interview tip:** "When asked how Cassandra avoids unnecessary disk reads, say: each SSTable has a Bloom filter. Before any read, Cassandra checks the filter — if it says 'not present,' the disk read is skipped entirely. This eliminates most unnecessary I/O."
```

---

### Example B — Level 2 Stub Bullets Expansion

**Before (Level 2):**
```markdown
### Circuit Breaker — HALF-OPEN State

- Test state after OPEN timeout
- Lets a trial request through
- Goes CLOSED if it succeeds
```

**After (Level 4):**
```markdown
### Circuit Breaker — HALF-OPEN State

The **HALF-OPEN** state is the circuit breaker's *recovery probe* — a controlled test to decide whether the downstream service has recovered enough to resume normal traffic.

**State transition logic:**

```mermaid
stateDiagram-v2
    [*] --> CLOSED : service healthy
    CLOSED --> OPEN : failure threshold exceeded
    OPEN --> HALF_OPEN : timeout expires (e.g. 30s)
    HALF_OPEN --> CLOSED : probe request succeeds
    HALF_OPEN --> OPEN : probe request fails — reset timer
```

**Why HALF-OPEN matters:**
- Without it, OPEN stays open forever (manual reset needed)
- It prevents a sudden flood of requests hitting a recovering service
- One trial request acts as a health probe — if it succeeds, the circuit closes; if not, the timeout restarts

**What "one trial request" means in practice:**
Only the first request that arrives during HALF-OPEN is forwarded to the real service. All others immediately return the fallback response. If the trial succeeds, the circuit closes and normal traffic resumes.

> **Interview tip:** "HALF-OPEN is what makes circuit breakers self-healing. Without it, you'd need an operator to manually reset the circuit. With it, the system automatically probes recovery and resumes traffic when the downstream service is healthy — no human intervention needed."
```

---

## Agent Prompt Template

Use this prompt to invoke the agent for a new task:

```
You are the MDTopicEnricher Agent. Your job is to find thin topics in .md files and expand them to exam/interview-ready depth.

Target directory: [DIRECTORY_PATH]

WORKFLOW:

1. SCAN: List all .md files, get line counts.

2. DETECT THIN SECTIONS: For each file, find all ## and ### sections.
   For each section, compute depth_score:
     +1 if body > 3 lines
     +1 if has code block
     +1 if has table
     +1 if has diagram (ASCII or mermaid)
     +1 if has > blockquote
   Classify: Level 0 (empty), Level 1 (score 0-1, ≤2 lines), Level 2 (score 1, 3-8 lines, no code/diagram), Level 3 (score 2, missing 2+ elements), Level 4 (skip).
   Output THINNESS REPORT.

3. FOR EACH Level 1/2/3 SECTION — enrich following these rules:
   a. Identify topic type: algorithm / architectural pattern / concept / code pattern / infrastructure / security
   b. Required elements by level:
      Level 1: definition (3-5 sentences) + how-it-works steps + real-world example + {code/diagram/table} + interview tip
      Level 2: rewrite bullets into explanation + {code/diagram/table} + interview tip
      Level 3: add only the missing element(s)
   c. Use Mermaid diagrams for structural concepts:
      - Decision trees / flows → flowchart TD
      - Component / service architecture → graph TD with subgraphs
      - State machines → stateDiagram-v2
      - Taxonomy / hierarchy → mindmap
      - Request/response flows → sequenceDiagram
      ASCII only for: bit arrays, side-by-side before/after, latency timelines
   d. Use fenced code blocks with language tags for code
   e. Use markdown tables for comparisons (min 3 rows)
   f. Add interview tip as: > **Interview tip:** "..."

4. EDIT: Use Edit tool to replace thin content or append missing elements.
   Level 1/2: replace thin section body with enriched content.
   Level 3: append missing element before the next ---.

5. VALIDATE per section:
   - depth_score grew by ≥ 2
   - section now ≥ 15 lines
   - original information preserved
   - no padding phrases

Report: file name, sections enriched, lines before → after.
```

---

## Difference from MDOpenQA Agent

| | MDOpenQA Agent | MDTopicEnricher Agent |
|---|---|---|
| **Target** | Topics with NO answer (Level 0) | Topics with THIN answer (Level 1–3) |
| **Input state** | Empty or missing section | Existing but shallow content |
| **Action** | Write from scratch | Expand / enrich existing content |
| **Preserve original?** | N/A (nothing to preserve) | Yes — always keep original, only add |
| **Depth goal** | Add any answer | Raise to exam/interview-ready depth |
| **Interview tips** | Not required | Required for every enriched section |

> Run **MDOpenQA Agent first** to fill all empty gaps, then run **MDTopicEnricher Agent** to deepen all thin content.

---

## Skills Summary Table

| # | Skill | Description |
|---|---|---|
| 1 | **Thinness Detection** | Score each section on 5 depth criteria; classify Level 0–4 |
| 2 | **Enrichment Blueprint** | Map topic type → required elements by level |
| 3 | **Writing Rules** | Define-first, example-before-theory, no padding, interview tip required |
| 4 | **Diagram Templates** | 5 Mermaid templates (flowchart, graph, stateDiagram-v2, mindmap, sequenceDiagram) + 3 ASCII templates for layouts/timelines; selection guide included |
| 5 | **Edit Strategy** | Replace for Level 1/2; append missing element for Level 3 |
| 6 | **Quality Gate** | depth_score grew ≥ 2, section ≥ 15 lines, original preserved, no padding |

---

*Agent Skill v1.1 | MDTopicEnricher Agent | Created July 2026 | Updated July 2026 — Mermaid diagram templates*
