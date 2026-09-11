# 01 — Agentic AI Fundamentals

> **Level:** Beginner → Intermediate | **Time to complete:** 3–4 hours | **Azure services touched:** Azure OpenAI, Azure AI Foundry Agent Service

---

## 1. Overview

### What Is Agentic AI?

An **AI Agent** is a software system that uses a Large Language Model (LLM) as its reasoning engine to autonomously perceive its environment, plan a course of action, execute tools, and iterate toward a goal — without requiring a human to direct every step.

The shift from *chat AI* to *agentic AI* is one of the most significant architectural transitions in enterprise software since the move to microservices. Where a chatbot responds once to a single user turn, an agent:

1. **Perceives** — reads context (user input, documents, databases, APIs)
2. **Plans** — decides what actions to take and in what order
3. **Acts** — calls tools, writes files, queries APIs, triggers workflows
4. **Observes** — reads the results of its actions
5. **Iterates** — decides whether the goal is met or another action is needed

### Why It Matters Enterprise-Wide

| Traditional Software | Agentic AI |
|---|---|
| Pre-defined logic flows | Dynamic, goal-directed reasoning |
| Human operators for exceptions | Autonomous exception handling |
| Point-to-point integrations | Tool ecosystem via function calling |
| Hours/days for process automation | Minutes to wire up multi-step workflows |
| Hard to generalize across domains | One agent can span HR, Finance, IT |

### When to Use Agentic AI

**Use when:**
- The task requires multi-step reasoning that cannot be predetermined
- Inputs are unstructured (documents, emails, voice, code)
- The workflow involves conditional branching based on data retrieved at runtime
- You need natural-language interfaces to complex enterprise systems
- Automation of knowledge work tasks (research, summarization, drafting, analysis)

**Avoid when:**
- The task is fully deterministic and well-defined (use a standard API or script)
- Latency SLAs are under 200ms (agent loops add overhead)
- Every decision requires full human review (regulatory/legal contexts — use human-in-the-loop patterns with tight guardrails instead of full autonomy)
- Your source data is too sensitive to pass to an LLM (air-gapped, HIPAA PHI without proper contracts)

---

## 2. Business Problem

### What Business Problem Does Agentic AI Solve?

Enterprise knowledge work is the largest untapped automation opportunity. McKinsey estimates 60–70% of employee time is spent on tasks that are partially or fully automatable with generative AI: reading documents, synthesizing information, drafting outputs, coordinating systems.

Traditional RPA (Robotic Process Automation) handles *structured* workflows — screen scraping, form filling, rule-based routing. It breaks the moment the format changes. Agentic AI handles *unstructured* workflows because the LLM's reasoning adapts to variation.

### Enterprise Scenarios

| Domain | Agent Capability | Business Value |
|---|---|---|
| Customer Support | Triage tickets, look up orders, draft responses, escalate | 60% deflection rate, 24/7 coverage |
| HR | Answer policy questions, process onboarding, screen resumes | 4 hours/week per HR FTE saved |
| Finance | Reconcile invoices, flag anomalies, draft variance reports | 80% reduction in manual reconciliation |
| Software Engineering | Code review, PR summarization, test generation, incident triage | 30% developer velocity gain |
| Legal | Contract review, clause extraction, risk flagging | 75% faster first-pass review |
| IT Operations | Alert triage, runbook execution, capacity planning | MTTR reduction 40–60% |

### ROI Framework

```
ROI = (Hours Automated × Hourly Cost × Accuracy Rate) - (Infra Cost + Governance Overhead)
```

For a 100-agent enterprise deployment processing 10,000 tasks/day at $0.05/task infra cost vs. $25/hour human cost at 3 minutes/task:
- Human cost: 10,000 × 0.05 hours × $25 = **$12,500/day**
- AI cost: 10,000 × $0.05 = **$500/day**
- Gross saving: **$12,000/day** before governance overhead

---

## 3. Core Concepts

### 3.1 The Four Pillars of an Agent

```
┌──────────────────────────────────────────────────────────┐
│                      🤖  AI AGENT                        │
│                                                          │
│  ┌───────────────────┐  Recall / Store  ┌─────────────┐  │
│  │   🧠 LLM Brain    │◄───────────────►│  💾 Memory  │  │
│  │  GPT-4o / Claude  │                  │ Short-term  │  │
│  │     / Gemini      │                  │ + Long-term │  │
│  └────────┬──────────┘                  └─────────────┘  │
│           │ Reason                                        │
│           ▼            Select & Execute                   │
│  ┌────────────────────┐ ─────────────► ┌──────────────┐  │
│  │   📋 Planning      │                │  🔧 Tools    │  │
│  │  ReAct /           │ ◄──────────── │  APIs · DBs  │  │
│  │  Chain-of-Thought  │  Observe Results│  Code Exec  │  │
│  └────────────────────┘                └──────────────┘  │
└──────────────────────────────────────────────────────────┘
         ▲  Perceive                           │
         │                                     ▼  Act
    ┌────┴─────────────────────────────────────────────┐
    │   🌍 Environment   (User Input · Data · Systems) │
    └──────────────────────────────────────────────────┘
```

| Pillar | Role | Azure Implementation |
|---|---|---|
| **LLM Brain** | Reasoning, language understanding, decision-making | Azure OpenAI GPT-4o / o1 |
| **Memory** | Context across turns and sessions | Azure Cosmos DB, Azure AI Search, Redis Cache |
| **Tools** | Actions the agent can take | Azure Functions, custom plugins, MCP servers |
| **Planning** | Deciding what to do next | ReAct loop, Planner patterns, Semantic Kernel |

### 3.2 Agent vs. Chatbot vs. Copilot

```
   💬 CHATBOT            🤝 COPILOT              🤖 AGENT
 ┌─────────────┐      ┌─────────────┐       ┌─────────────┐
 │  User Input │      │  User Input │       │ Goal / Task │
 └──────┬──────┘      └──────┬──────┘       └──────┬──────┘
        │                    │                      │
 ┌──────▼──────┐      ┌──────▼──────┐       ┌──────▼──────┐
 │ Single LLM  │      │ LLM +       │       │  LLM Plans  │
 │   Call      │      │ Context     │       │    Steps    │
 └──────┬──────┘      └──────┬──────┘       └──────┬──────┘
        │                    │ (optional)           │
 ┌──────▼──────┐      ┌──────▼──────┐       ┌──────▼──────┐
 │   Single    │      │  Tool Call  │       │  Tool Call 1│
 │  Response   │      └──────┬──────┘       └──────┬──────┘
 └─────────────┘             │                     │
                      ┌──────▼──────┐       ┌──────▼──────┐
                      │  Augmented  │       │  Observe →  │
                      │  Response   │       │  Re-plan?   │
                      └─────────────┘       └──────┬──────┘
                                                   │
                                            ┌──────▼──────┐
                                            │  Tool Call N│
                                            └──────┬──────┘
                                                   │
                                            ┌──────▼──────┐
                                     ┌─No── │  Goal Met?  │
                                     │      └──────┬──────┘
                                     │             │ Yes
                                     ▼             ▼
                                  (loop)    ┌─────────────┐
                                            │ Final Answer│
                                            └─────────────┘
```

**The key distinction:** An agent maintains a *loop* that continues until the goal is achieved, rather than making a single LLM call.

### 3.3 The ReAct Loop (Reason + Act)

ReAct (Yao et al., 2022) is the foundational pattern behind most production agents. The LLM alternates between **Thought** (reasoning about what to do) and **Action** (calling a tool), observing results after each action.

```
  User            Agent          LLM (GPT-4o)            Tools
   │                │                 │                     │
   │ "Summarize Q2  │                 │                     │
   │  sales, flag   │                 │                     │
   │  anomalies"    │                 │                     │
   │───────────────►│                 │                     │
   │                │ System prompt + │                     │
   │                │ user goal       │                     │
   │                │────────────────►│                     │
   │                │                 │ Thought: Need Q2    │
   │                │◄────────────────│ data.               │
   │                │                 │ Action: query_sales_db(Q2)
   │                │ query_sales_db(quarter="Q2", year=2025)│
   │                │────────────────────────────────────── ►│
   │                │ {revenue:$4.2M, anomaly:true, EMEA:-34%}
   │                │◄───────────────────────────────────────│
   │                │ Observation:    │                     │
   │                │ EMEA -34%       │                     │
   │                │────────────────►│                     │
   │                │                 │ Thought: Get EMEA   │
   │                │◄────────────────│ detail.             │
   │                │                 │ Action: query_sales_db(EMEA,Q2)
   │                │ query_sales_db(region="EMEA", quarter="Q2")
   │                │────────────────────────────────────── ►│
   │                │ {$620K vs $940K prior year,            │
   │                │  root_cause: "3 accounts churned"}     │
   │                │◄───────────────────────────────────────│
   │                │ Observation:    │                     │
   │                │ Enough context  │                     │
   │                │────────────────►│                     │
   │                │                 │ Final Answer:       │
   │                │◄────────────────│ Q2 $4.2M (+8% YoY) │
   │ Report + EMEA  │                 │ EMEA anomaly detail │
   │ analysis       │                 │                     │
   │◄───────────────│                 │                     │
```

### 3.4 Agent Lifecycle

An agent is not a request–response system — it is a **stateful, iterative process** that moves through well-defined phases between receiving a goal and delivering a result. Understanding the lifecycle is essential for debugging runaway agents, designing human-in-the-loop checkpoints, and setting appropriate timeout and retry budgets.

The lifecycle has **six core states** plus two exception paths (human review and error handling):

```
  [start]
     │
     ▼
 ┌──────────┐  User submits goal   ┌─────────────────┐
 │   IDLE   │─────────────────────►│   PERCEIVING    │
 └──────────┘                      │ Assemble context│
                                   └────────┬────────┘
                                            │ Context ready
                                            ▼
                          ┌─────────────────────────────┐
                          │          PLANNING           │
                          │  SelectTool → FormatArgs    │
                          └──────┬──────────────┬───────┘
                   Plan created  │              │ Confidence low /
                                 │              │ Irreversible action
                                 │              ▼
                                 │     ┌─────────────────┐
                                 │     │  HUMAN REVIEW   │
                                 │     │ (HITL checkpoint)│
                                 │     └────────┬────────┘
                                 │              │ Approved → back to Planning
                                 ▼              │
                          ┌──────────────┐      │
                          │    ACTING    │◄─────┘
                          │ Execute tool │
                          └──────┬───────┘
                                 │                 Tool fails
                                 │         ┌────────────────────┐
                                 │         │   ERROR HANDLING   │
                                 │         │ Classify + retry   │
                                 │         └────────┬───────────┘
                                 │      Retry ──────┘    │ Max retries
                                 ▼                       │
                          ┌──────────────┐               │
                          │  OBSERVING   │               │
                          └──────┬───────┘               │
                                 │                       │
                  Goal not met   │ Goal achieved          │
                  ◄──────────────┤                       │
                  (→ PLANNING)   ▼                       ▼
                          ┌──────────────────────────────────┐
                          │          RESPONDING              │
                          │  Format + deliver final answer   │
                          └──────────────┬───────────────────┘
                                         │ Delivered
                                         ▼
                                    ┌──────────┐
                                    │   IDLE   │
                                    └──────────┘
```

#### Phase-by-Phase Breakdown

| Phase | What Happens | Key Inputs | Key Outputs | Latency Budget |
|---|---|---|---|---|
| **Idle** | Agent awaits trigger; no compute consumed | — | — | 0 ms |
| **Perceiving** | Assembles context: user goal, conversation history, retrieved documents, tool schemas | Raw user message, memory stores | Enriched prompt / context window | 50–500 ms |
| **Planning** | LLM reasons about next action; selects tool and formats arguments | Enriched context | Tool name + JSON arguments | 500–3000 ms |
| **Acting** | Executes the chosen tool (API call, DB query, code run) | Tool name + arguments | Raw tool response | 100 ms – 30 s |
| **Observing** | LLM evaluates tool result; decides if the goal is met or another step is needed | Tool response appended to context | Continue / finish decision | 200–1000 ms |
| **Responding** | Formats and delivers the final answer to the user | Accumulated observations | User-facing response | 200–800 ms |

#### State Transitions in Detail

**Happy path — Perceiving → Planning → Acting → Observing → (loop) → Responding**

Each Planning → Acting → Observing cycle is one **iteration** of the ReAct loop. A well-designed agent completes most tasks in 2–5 iterations. Beyond 10 iterations, treat it as a signal that either the goal is under-specified or the tools are too coarse-grained.

**Exception path 1 — Planning → HumanReview → Planning**

The agent requests human confirmation when:
- Its own confidence score (from the LLM's log-probabilities or a self-critique step) drops below a configured threshold
- The planned action involves irreversible side-effects (sending an email, deleting a record, charging a card)
- The task violates a declared policy in the system prompt

> In Azure AI Foundry, implement this as an **approval step** in the agent's action graph, surfacing the proposed action to a reviewer before execution.

**Exception path 2 — Acting → ErrorHandling → Planning / Responding**

| Error Class | Strategy | Max Retries |
|---|---|---|
| Transient (rate limit, timeout) | Exponential back-off, retry same tool | 3 |
| Input error (wrong arguments) | Replan with corrected arguments | 2 |
| Tool unavailable | Substitute with alternative tool if available | 1 |
| Unrecoverable (auth failure, quota exhausted) | Surface error to user, terminate loop | 0 |

#### Lifecycle Instrumentation

Every production agent should emit a structured event at each state transition. This is the minimum telemetry needed to build a reliable agent:

```python
# Azure Monitor / OpenTelemetry trace attributes per lifecycle event
{
  "agent.run_id":        "run-uuid-4321",
  "agent.phase":         "planning",          # perceiving | planning | acting | observing | responding | error
  "agent.iteration":     2,
  "agent.tool_selected": "query_sales_db",
  "agent.tool_latency_ms": 340,
  "agent.context_tokens": 4812,
  "agent.goal_achieved": False,
  "agent.error_class":   None
}
```

Aggregate `agent.iteration` per `run_id` to detect infinite loops. Alert when `agent.iteration > 8` for the same goal.

#### Lifecycle vs. Concurrency

A single agent instance runs its lifecycle phases **sequentially** — one tool at a time. To parallelize work:

- Use **sub-agents** (each running their own lifecycle on a sub-task)
- Use **parallel tool calls** (supported in GPT-4o and Azure OpenAI — the LLM can request multiple tool calls in a single Planning step, which the orchestrator dispatches concurrently before the next Observing step)

> **Interview question:** *"How does an agent know when to stop?"*  
> The termination signal comes from the LLM during the Observing phase — it returns a `finish` decision (often `finish_reason: "stop"` in the API response) when the accumulated observations are sufficient to answer the original goal. A production agent also enforces a hard stop via a **maximum iteration count** and **wall-clock timeout** to prevent runaway loops.

### 3.5 Memory Architecture

An LLM has no persistent state — every API call is stateless. An agent's apparent ability to "remember" is entirely the responsibility of the agent framework: it must decide *what* to surface into the LLM's context window at each Planning step, and *what* to write back to storage after each Observing step. Memory architecture is therefore one of the most consequential design decisions in an agentic system.

Agents need four distinct types of memory, each operating at a different timescale and stored in a different backing system:

```
                           ┌───────────────┐
                           │   🤖 Agent    │
                           └───────┬───────┘
          ┌──────────────┬─────────┴─────────┬──────────────┐
     Read/│         Recall/                  │         Execute
     Write│         Store│            Retrieve│              │
          ▼              ▼                   ▼              ▼
  ┌─────────────┐ ┌─────────────┐ ┌──────────────┐ ┌─────────────┐
  │ 🔵 IN-      │ │ 🟢 EPISODIC │ │ 🟡 SEMANTIC  │ │ 🔴 PROCED-  │
  │ CONTEXT     │ │             │ │              │ │ URAL        │
  │ (Working)   │ │ (Session)   │ │ (Knowledge)  │ │ (Skill)     │
  │─────────────│ │─────────────│ │──────────────│ │─────────────│
  │ Current     │ │ Past inter- │ │ Facts,       │ │ How to use  │
  │ conversation│ │ actions,    │ │ domain know- │ │ tools,      │
  │ task state, │ │ completed   │ │ ledge,       │ │ operating   │
  │ observations│ │ tasks       │ │ doc corpus   │ │ procedures  │
  │─────────────│ │─────────────│ │──────────────│ │─────────────│
  │ Lifespan:   │ │ Lifespan:   │ │ Lifespan:    │ │ Lifespan:   │
  │ Session     │ │ Days/weeks  │ │ Permanent    │ │ Permanent   │
  │─────────────│ │─────────────│ │──────────────│ │─────────────│
  │ Token window│ │ Redis /     │ │ Vector DB    │ │ System      │
  │             │ │ Cosmos DB   │ │ (AI Search)  │ │ prompt /    │
  │             │ │             │ │              │ │ plugins     │
  └─────────────┘ └─────────────┘ └──────────────┘ └─────────────┘
```

#### The Four Memory Types

| Memory Type | Analogy | Lifespan | Read Pattern | Write Pattern | Azure Backing |
|---|---|---|---|---|---|
| **In-Context (Working)** | Whiteboard | Current session only | Always present in prompt | Agent appends observations each iteration | Token window (128k–200k tokens) |
| **Episodic (Session)** | Notebook | Days to weeks | Retrieved by session ID or semantic similarity | Written at session end or on significant events | Azure Cache for Redis / Cosmos DB (NoSQL) |
| **Semantic (Knowledge)** | Library | Permanent | Vector similarity search (RAG) | Updated by data pipeline, not the agent at runtime | Azure AI Search (vector + hybrid) |
| **Procedural (Skill)** | Muscle memory | Permanent | Injected as system prompt or plugin definition | Updated via prompt engineering / plugin deployment | System prompt, Azure AI Foundry plugins |

#### Real-World Examples for Each Memory Type

**In-Context (Working Memory)**

> **Scenario:** An IT support agent is troubleshooting a broken CI/CD pipeline for a developer.
>
> During a single session the agent: reads the developer's description of the failure → calls `get_pipeline_logs(run_id=4821)` → observes a YAML parse error → calls `get_file_content("ci-pipeline.yml")` → spots a missing closing bracket on line 47 → suggests the fix.
>
> Every tool result and every reasoning step is appended to the context window. The agent can reference the log output from step 2 when explaining the fix in step 5 — all within the same session, all within the same token window. When the session ends, none of this is automatically saved anywhere.

---

**Episodic (Session Memory)**

> **Scenario:** A financial analysis agent serves the same portfolio manager, Ana, every week.
>
> Last Monday, Ana asked: *"Analyze our emerging markets exposure."* The agent ran 6 tool calls, hit a rate-limit on the data vendor API, retried, and eventually produced a report. That outcome — including which tools failed, what the final answer was, and how long it took — was written as an episodic entry.
>
> This Monday, Ana asks: *"Compare this week's EM exposure to last week."* The agent retrieves last week's episode by `user_id + topic embedding`, pulls the prior result summary, and uses it directly in its context — skipping the 6-call data gathering phase that already ran. The agent also knows to set a longer timeout on the data vendor call because it failed last time.

---

**Semantic (Knowledge Memory)**

> **Scenario:** An HR assistant agent answers employee questions across a company of 8,000 people.
>
> An employee asks: *"How many weeks of parental leave do I get if I adopt?"*
>
> The agent does not have this answer in its system prompt — it would be impossible to enumerate every policy. Instead, during the Perceiving phase it embeds the question, runs a vector similarity search against the indexed HR policy corpus in a vector store, and retrieves the three most relevant chunks: the parental leave policy, the adoption supplement clause, and the eligibility section. Those chunks are injected into the context window, and the LLM synthesises a precise answer grounded in the actual policy document, citing the source URL.
>
> If the HR team updates the parental leave policy next month, only the data ingestion pipeline needs to re-index that document — the agent itself is unchanged.

---

**Procedural (Skill Memory)**

> **Scenario:** A customer-facing sales agent for a SaaS company.
>
> The system prompt bakes in the rules the agent must always follow:
> ```
> You are Aria, a sales assistant for Acme SaaS.
> ALWAYS call get_customer_account(email) before discussing pricing.
> NEVER quote a price below the list price without calling get_approval_workflow().
> If the customer mentions a competitor, call log_competitive_mention() silently.
> Escalate to a human rep if the deal value exceeds $50,000.
> ```
> These rules are procedural memory — the agent executes them reflexively on every call, just as a trained sales rep follows a playbook without consciously recalling it each time. The tool schemas (what `get_customer_account` accepts and returns) are also procedural: they tell the LLM exactly how to call each tool.

---

#### RAG and Semantic Memory — Same Thing?

A common point of confusion: **RAG is the retrieval mechanism; semantic memory is the storage concept.** They are not the same, but RAG is the standard pattern used to *read* from semantic memory.

| | RAG | Semantic Memory |
|---|---|---|
| **What it is** | A retrieval *technique* | A memory *type* (architectural concept) |
| **Defines** | *How* content moves from storage into the context window | *What* is stored and its lifecycle properties |
| **Layer** | Runtime retrieval pattern | Storage + content classification |

They diverge in two ways:

1. **RAG can query episodic memory too.** If past session summaries are embedded and stored in a vector index, RAG retrieves relevant prior interactions. That is still RAG, but it reads from episodic memory, not semantic.
2. **Semantic memory can be read without RAG.** Keyword search, direct SQL lookup, or document fetch are all valid reads from a knowledge store. RAG is just the most effective pattern for unstructured corpora.

```
Semantic Memory  =  the indexed knowledge corpus  (permanent, shared, org-wide)
RAG              =  the pattern that reads from it at runtime and loads chunks into in-context memory
```

When you see "RAG" in an architecture diagram it almost always means "retrieving from semantic memory" — but RAG is the *verb*, semantic memory is the *noun*.

#### What Is an Embedding Model?

An **embedding model** is a neural network that converts text (or other data) into a fixed-size list of numbers called a **vector** or **embedding**. The key property is that semantically similar text produces vectors that are close together in the vector space, and dissimilar text produces vectors that are far apart.

```
"parental leave policy"    →  [0.12, -0.87, 0.34, 0.91, ...]   (768 numbers)
"maternity and paternity"  →  [0.11, -0.85, 0.36, 0.89, ...]   ← very close
"quarterly sales report"   →  [-0.54, 0.23, -0.71, 0.04, ...]  ← far away
```

The distance between two vectors is measured with **cosine similarity** — the smaller the angle between them, the more semantically related the texts are. This is what makes vector search possible: instead of matching keywords, the retrieval step finds the chunks whose *meaning* is closest to the query.

**Where it fits in the semantic memory pipeline:**

```
Ingestion:   document chunk  →  embedding model  →  vector  →  stored in vector index
Retrieval:   user query      →  embedding model  →  vector  →  find nearest vectors in index
```

The same embedding model must be used for both ingestion and retrieval. If you embed documents with one model and queries with another, the vector spaces won't align and retrieval will produce garbage results.

**Common embedding models:**

| Model | Dimensions | Type | Best for |
|---|---|---|---|
| `all-MiniLM-L6-v2` | 384 | Open-source | Fast, low-cost, general purpose |
| `all-mpnet-base-v2` | 768 | Open-source | Better quality, still self-hostable |
| `text-embedding-3-small` | 1536 | API | High quality, low cost per token |
| `text-embedding-3-large` | 3072 | API | Highest retrieval quality, higher cost |
| `embed-english-v3.0` (Cohere) | 1024 | API | Strong for retrieval-specific tasks |

**The core trade-off:** More dimensions = higher retrieval quality but more storage, more compute at query time, and higher cost per embedding call. For most production RAG systems, a mid-range model (768–1536 dimensions) is the right balance between quality and cost.

**What are dimensions and why are they needed?**

A **dimension** is a single number in the output vector. An embedding of 768 dimensions is a list of 768 floating-point numbers:

```
"parental leave policy"  →  [0.12, -0.87, 0.34, 0.91, -0.23, 0.67, ...]
                              dim1   dim2   dim3   dim4   dim5   dim6  ... (768 total)
```

No single dimension has a human-readable meaning — the model distributes semantic information across all of them collectively. Think of it as **coordinates in semantic space**: just as GPS uses 3 numbers (latitude, longitude, altitude) to locate any point on Earth, an embedding uses N numbers to locate a piece of text in an N-dimensional meaning space. Texts with similar meaning land close together in that space; unrelated texts land far apart.

**Why dimensions are needed:**

| Reason | Explanation |
|---|---|
| **Mathematical comparison** | Computers cannot compare meaning directly — but they can compute distance between two lists of numbers (cosine similarity). Dimensions make meaning measurable. |
| **Information capacity** | More dimensions = more room to encode subtle distinctions. 64 dimensions cannot distinguish "bank (river)" from "bank (finance)"; 768 usually can. |
| **Vector search** | The entire retrieval system (RAG, similarity search) depends on the vector existing — without it there is nothing to index or search against. |

**Dimension trade-off in practice:**

| Dimensions | Storage per vector | Search speed | Quality |
|---|---|---|---|
| 384 | ~1.5 KB | Very fast | Good for general use |
| 768 | ~3 KB | Fast | Strong for most tasks |
| 1536 | ~6 KB | Moderate | High quality |
| 3072 | ~12 KB | Slower | Marginal gain over 1536 |

**Why those exact KB numbers?**

Each dimension is stored as a 32-bit floating-point number (float32) = **4 bytes**:

```
384  dims × 4 bytes = 1,536  bytes ≈ 1.5 KB
768  dims × 4 bytes = 3,072  bytes ≈ 3 KB
1536 dims × 4 bytes = 6,144  bytes ≈ 6 KB
3072 dims × 4 bytes = 12,288 bytes ≈ 12 KB
```

This compounds fast at scale:

| Corpus size | 384-dim index | 768-dim index | 1536-dim index |
|---|---|---|---|
| 100K chunks | 150 MB | 300 MB | 600 MB |
| 1M chunks | 1.5 GB | 3 GB | 6 GB |
| 10M chunks | 15 GB | 30 GB | 60 GB |

For large corpora, the dimension choice determines whether the index fits in RAM (fast) or must be paged from disk (slow).

**Why search speed decreases with more dimensions:**

At query time, cosine similarity must be computed between the query vector and every vector in the index — one multiplication per dimension per comparison:

```
cosine_similarity(query, chunk) = (query · chunk) / (|query| × |chunk|)

384  dims  →  384   multiplications per comparison
3072 dims  →  3,072 multiplications per comparison  (8× more work per pair)
```

With 1 million chunks, that is 3 billion multiplications per query at 3072 dims vs. 384 million at 384 dims. Even with approximate nearest-neighbour algorithms (HNSW, IVF), higher dimensions slow both index build time and query latency. Larger vectors also cause more CPU/GPU cache misses, adding further overhead.

**Why quality improves with more dimensions — but only up to a point:**

More dimensions give the model more room to encode subtle distinctions:

```
64   dims  →  cannot distinguish "bank (river)" vs "bank (finance)"
384  dims  →  distinguishes them; misses subtle domain nuances
768  dims  →  handles most domain-specific distinctions well
1536 dims  →  captures fine-grained semantic differences
3072 dims  →  marginal improvement; training data matters more than extra dimensions
```

The gains follow **diminishing returns**:

```
384  →  768   :  large quality jump
768  →  1536  :  moderate improvement
1536 →  3072  :  small improvement, rarely worth the storage/compute cost
```

Beyond a certain point the **curse of dimensionality** sets in — in very high-dimensional spaces, distances between vectors start to converge (everything looks roughly equidistant), which actually hurts retrieval precision.

**Why the embedding model is a long-lived dependency:**

Every chunk in the index is stored as a vector in the coordinate space of the model that created it. That vector is not transferable to another model's space:

```
"parental leave policy" via model-A  →  [0.12, -0.87, 0.34, ...]   ← model-A space
"parental leave policy" via model-B  →  [0.55,  0.21, -0.89, ...]  ← model-B space
```

These two vectors live in completely different spaces. If you switch models, every stored vector becomes invalid — similarity queries would compare query vectors from model-B's space against chunk vectors from model-A's space and return garbage results.

Switching models requires:
1. Re-running the full ingestion pipeline — re-chunk, re-embed, and re-upsert every document
2. Rebuilding the entire vector index schema if the new model has different dimensions (e.g., 768 → 1536)

At scale this is expensive:

| Corpus | Re-embedding cost (API, ~$0.02 / 1M tokens) | Time |
|---|---|---|
| 100K chunks (~400 tokens each) | ~$0.80 | Hours |
| 1M chunks | ~$8 | Days |
| 10M chunks | ~$80 | Days–weeks |

The cost is not only money — it is the operational risk of a pipeline that may fail midway, leaving the index in a partially-updated, inconsistent state.

> **The embedding model is the primary key of your vector index.** Switching it is like migrating a database primary key from UUID to BIGINT — every row and every foreign key must be rewritten. You would not do it casually. Choose before you build, and treat it as a long-term architectural commitment.

#### What Is a Neural Network?

A **neural network** is a computational system loosely inspired by how neurons in the brain connect and signal each other. It learns to map inputs to outputs by adjusting the strength of connections (called **weights**) across thousands or millions of simple processing units, organized in layers.

**Structure:**

```
Input Layer        Hidden Layers           Output Layer
                  (learned features)

  [word]    →    [layer 1]  →  [layer 2]  →  [layer N]  →  [vector / prediction]
  tokens         patterns       patterns       patterns
```

- **Input layer** — receives raw data (e.g., tokenized text)
- **Hidden layers** — each layer learns increasingly abstract representations (edges → shapes → concepts)
- **Output layer** — produces the result (a vector, a probability, a next token)

**How it learns — training:**

The network is shown millions of examples. For each one it makes a prediction, compares it to the correct answer (the **loss**), and nudges all weights slightly in the direction that reduces that error. This process — called **backpropagation + gradient descent** — is repeated billions of times until the network generalizes well to unseen data.

**Types relevant to agents:**

| Type | Full Name | What it does | Example use in agents |
|---|---|---|---|
| **Transformer** | — | Models relationships between all tokens in a sequence simultaneously | The LLM itself (GPT-4, Claude, Gemini) |
| **Encoder-only** | — | Compresses input into a dense vector capturing its meaning | Embedding models for semantic memory / RAG |
| **Encoder-Decoder** | — | Translates or transforms one sequence into another | Summarization, machine translation |
| **CNN** | Convolutional Neural Network | Detects local patterns in grids by sliding filters over the input | Image understanding in multimodal agents |

**What each type actually does:**

**Transformer** — The dominant architecture for language. Its key innovation is **self-attention**: every token in the input looks at every other token simultaneously to figure out which ones are relevant to understanding it.

```
"The bank was steep"  →  "bank" attends strongly to "steep"
"The bank was closed" →  "bank" attends strongly to "closed"
```

Self-attention resolves ambiguity by weighing token relationships across the entire sequence at once. The LLM your agent uses is a **decoder-only Transformer** — a variant not in the table above — that generates the next token one at a time, conditioned on everything before it.

**Encoder-only** — A Transformer with only the reading half. It takes a sentence and produces a single dense vector (an embedding) that captures its meaning. Trained to predict masked words, it learns context from both directions (left and right). Examples: BERT, sentence-transformers.

This is how semantic memory works: the agent encodes a query into a vector, then finds stored vectors that are geometrically close to it. It never generates text — it only understands and compresses it.

**Encoder-Decoder** — Has both halves: an encoder that reads and compresses the input, and a decoder that generates a new sequence from that compressed representation. The two halves are trained jointly. Examples: T5, BART.

Used in summarization (compress a long conversation into a short memory) or translation. Less common in modern agent stacks since large decoder-only models handle these tasks well enough.

**CNN (Convolutional Neural Network)** — Not built for language — built for **spatial data** (images, grids). A small filter (e.g., 3×3 pixels) slides across the input detecting local patterns. Early layers detect edges and textures; deeper layers detect shapes and objects.

Multimodal agents that process images (screenshots, charts, scanned documents) use CNNs or CNN-derived vision encoders to extract visual features before passing them to the Transformer reasoning core.

**One-line mental model:**

| Type | Mental model |
|---|---|
| **Transformer** | Reads everything at once, reasons about token relationships |
| **Encoder-only** | Reads → compresses to a point (for search / retrieval) |
| **Encoder-Decoder** | Reads → compresses → writes something new |
| **CNN** | Slides a window over a grid, finds local spatial patterns |

**The link to embedding models:**

An embedding model is specifically an **encoder-only** neural network. Its job is not to generate text — it compresses the meaning of a piece of text into a single fixed-size vector. The hidden layers progressively distill the input tokens into a compact representation that captures semantics rather than exact wording. This is why two sentences with different words but the same meaning produce similar vectors.

**What are parameters in a neural network?**

A **parameter** is a single learnable number (a weight) inside the network. The model learns the value of every parameter during training by processing billions of text examples. After training, the weights are fixed — they encode everything the model "knows." The total count of these weights is what people mean when they say a model has "7 billion parameters."

**Where parameters live inside an LLM:**

```
LLM internals
│
├── Token embedding table       — maps each vocabulary token to a starting vector
├── Attention layers (×N)
│       Query matrix  (Q)       — "what am I looking for?"
│       Key matrix    (K)       — "what do I contain?"
│       Value matrix  (V)       — "what do I return if matched?"
│       Output matrix (O)       — "how do I combine the matches?"
├── Feed-forward layers (×N)    — two large matrices per layer; where factual "knowledge" is stored
└── Output projection           — maps the final vector back to vocabulary probabilities
```

**Why parameter count matters:**

| Parameters | Capability | Example models |
|---|---|---|
| ~100M | Basic grammar, common facts | Early BERT, GPT-2 |
| ~7B | Strong reasoning, broad knowledge | Llama 3 8B, Mistral 7B |
| ~70B | Expert-level on most tasks | Llama 3 70B, Qwen 72B |
| ~175B+ | Near-human across diverse domains | GPT-3 class |
| 1T+ | Mixture-of-Experts; only a fraction of params active per call | GPT-4 class (estimated) |

**The parameter trade-off:**

```
More parameters  →  more knowledge capacity, better reasoning  →  more VRAM, slower, costlier
Fewer parameters →  faster inference, cheaper, runs on-device  →  less capable
```

**Parameters vs. Dimensions — side by side:**

| | LLM Parameters | Embedding Model Dimensions |
|---|---|---|
| **What it is** | Count of learnable weights inside the network | Size of the output vector |
| **Typical range** | 7M – 1T+ | 384 – 3072 |
| **Affects** | Reasoning quality, VRAM, inference cost | Retrieval quality, storage cost, search speed |
| **Set by** | Architecture + training compute budget | Model architecture choice |
| **Changed at runtime?** | No (fixed after training) | No |

#### What Is a CNN (Convolutional Neural Network)?

A **CNN — Convolutional Neural Network** — is a type of neural network designed to detect **local patterns** in structured, grid-like data (images, audio, time-series). Instead of connecting every input to every neuron, it slides small filters called **kernels** across the input to detect features at each position.

**Key layers:**

| Layer | What it does |
|---|---|
| **Convolutional** | Slides a filter across the input to detect local features — edges, textures, shapes in early layers; complex patterns in deeper layers |
| **Pooling** | Shrinks the spatial representation by keeping only the strongest signal in each region, reducing compute and adding positional tolerance |
| **Fully connected** | Combines all learned features to produce the final output (a class label, a vector, a score) |

**How filters work:**

```
Input image patch (3×3):          Filter (3×3):         Output (single value):
  1  0  1                           0  1  0
  0  1  0          ✕                1  1  1      =    sum of element-wise products
  1  0  1                           0  1  0
```

Each filter is a small weight matrix that the network learns during training. Early filters learn to detect low-level features (horizontal edges, color gradients); deeper filters combine those into higher-level concepts (faces, objects, document layout).

**Where CNNs appear in agentic systems:**

| Use case | What the CNN perceives |
|---|---|
| Multimodal agents | Screenshots, scanned documents, charts, diagrams |
| Computer-use agents | Desktop UI state — buttons, text fields, windows |
| Document intelligence | Invoice layout, form fields, table structure |
| Video understanding | Frame-by-frame scene recognition |

> CNNs are not used when the agent reasons over text — that is the Transformer's job. CNNs are the perception layer when the agent's input is **visual**.

#### How Each Memory Type Works During the Lifecycle

**In-Context Memory** is the only memory the LLM can directly "see." Everything else must be explicitly loaded into this window before the Planning step. The context window has a hard token limit, so the framework must actively manage what goes in:

```
┌─────────────────────────── Context Window (e.g., 128k tokens) ───────────────────────────────┐
│  [System Prompt — Procedural]   ~1k tokens  — tool schemas, persona, rules                   │
│  [Retrieved Docs — Semantic]    ~8k tokens  — top-k chunks from vector store                 │
│  [Session History — Episodic]   ~4k tokens  — summary of prior sessions                      │
│  [Current Conversation]        ~10k tokens  — turns so far in this session                   │
│  [Iteration Scratchpad]         ~2k tokens  — current thought/action/observation chain        │
│                                                                                               │
│  Remaining budget available for tool responses → ~103k tokens                                 │
└───────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Episodic Memory** gives the agent continuity across sessions — it knows what it tried before, what succeeded, and who it spoke with. The retrieval strategy is critical:

- **Exact retrieval** (by `session_id` or `user_id`): fast, used to reload a paused task
- **Semantic retrieval** (by embedding similarity): used when the agent asks "have I seen a similar problem before?"

```python
# Writing an episodic memory entry
# Backing store can be Redis, DynamoDB, Firestore, MongoDB, or any document DB
import datetime, json, redis

store = redis.Redis(host=REDIS_HOST, port=6379, db=0)

episode = {
    "session_id": session_id,
    "user_id": user_id,
    "goal": goal_text,
    "outcome": "success",
    "tools_used": ["query_sales_db", "send_email"],
    "iterations": 4,
    "summary": "Summarized Q2 EMEA anomaly and emailed the VP of Sales.",
    "timestamp": datetime.datetime.utcnow().isoformat()
}

# Expire after 30 days — episodic memory has a finite useful lifespan
store.setex(f"episode:{session_id}", 60 * 60 * 24 * 30, json.dumps(episode))
```

**Semantic Memory** is how the agent grounds itself in organizational knowledge — product documentation, policy documents, support tickets, financial reports. At runtime, the agent generates a query embedding and retrieves the top-k relevant chunks (RAG pattern — covered in depth in module 14):

```python
# Retrieving from semantic memory
# Vector store can be Chroma, Pinecone, Weaviate, Qdrant, pgvector, or any embedding DB
import chromadb
from sentence_transformers import SentenceTransformer

client = chromadb.HttpClient(host=VECTOR_STORE_HOST, port=8000)
collection = client.get_collection("knowledge-base")

encoder = SentenceTransformer("all-MiniLM-L6-v2")   # swap for any embedding model
query_vector = encoder.encode(user_query).tolist()

results = collection.query(
    query_embeddings=[query_vector],
    n_results=5,
    include=["documents", "metadatas"]
)

retrieved_chunks = results["documents"][0]   # top-k chunks injected into context window
```

**Procedural Memory** is the only memory type the agent cannot update itself at runtime — it is defined by the engineer. It includes:
- The **system prompt** (agent persona, operating rules, safety guardrails)
- **Tool schemas** (function name, description, parameter types) — the LLM reads these to know what it can do
- **Plugins / skills** (packaged skill modules that encapsulate multi-step workflows as callable units)

#### Context Window Management Strategy

As a session grows, in-context memory fills up. The three standard strategies for managing overflow:

| Strategy | Mechanism | Trade-off |
|---|---|---|
| **Sliding window** | Drop oldest N turns | Simple; loses early context |
| **Summarization** | LLM compresses old turns into a summary block | Retains meaning; adds latency + cost |
| **Hierarchical retrieval** | Move old turns to episodic store; retrieve on demand | Most robust; adds architectural complexity |

For most enterprise agents, **summarization** is the right default: cheap enough (one extra LLM call per ~20 turns) and preserves the semantic content the agent needs to avoid repeating work.

#### Read/Write Access Matrix

| Memory Type | Agent Reads | Agent Writes | Human/Pipeline Writes |
|---|---|---|---|
| In-Context | Every iteration | Every iteration (append) | At session start |
| Episodic | On recall trigger | End of session / key events | Rarely |
| Semantic | During Perceiving (RAG) | Never at runtime | Data ingestion pipeline |
| Procedural | Injected at agent startup | Never at runtime | Prompt engineering / deployment |

> **Key insight:** The agent only *writes* to in-context and episodic memory. Semantic and procedural memory are **read-only from the agent's perspective** — they are maintained by external processes. This separation is intentional: it prevents the agent from corrupting its own knowledge base.

#### Writing to Semantic Memory — When and How

Since the agent never writes to semantic memory at runtime, a separate **data ingestion pipeline** is responsible for populating and maintaining it. Understanding this pipeline is as important as understanding retrieval.

**When to write:**

| Trigger | Example | Action |
|---|---|---|
| New document published | New product spec uploaded to a content repo | Chunk, embed, and upsert all chunks |
| Document updated | HR policy revised | Delete old chunks for that `doc_id`, re-chunk and re-embed the new version |
| Document deleted / expired | Policy retired, contract terminated | Delete all chunks for that `doc_id` from the index |
| Scheduled re-index | Nightly or weekly sweep | Re-process documents whose source hash has changed since last run |
| Human approval gate passed | Knowledge article reviewed and approved | Trigger ingestion only after sign-off — prevents unreviewed content from entering the index |

**How to write — the ingestion pipeline:**

```
Source Document
      │
      ▼
  Extractor ──── pulls raw text from file storage, CMS, database, or web
      │
      ▼
   Chunker ───── splits text into overlapping chunks (e.g., 512 tokens, 50-token overlap)
      │
      ▼
   Embedder ──── converts each chunk to a dense vector using an embedding model
      │
      ▼
  Vector Store ─ upserts (chunk text + vector + metadata) into the index
```

```python
# Generic semantic memory ingestion pipeline
from sentence_transformers import SentenceTransformer
import chromadb, datetime

encoder   = SentenceTransformer("all-MiniLM-L6-v2")   # any embedding model
client     = chromadb.HttpClient(host=VECTOR_STORE_HOST, port=8000)
collection = client.get_or_create_collection("knowledge-base")

def chunk_document(text: str, chunk_size: int = 512, overlap: int = 50) -> list[str]:
    words = text.split()
    return [" ".join(words[i : i + chunk_size]) for i in range(0, len(words), chunk_size - overlap)]

def ingest_document(doc_id: str, title: str, content: str, source_url: str, category: str):
    # Step 1 — delete stale chunks so updates don't leave orphaned content
    collection.delete(where={"doc_id": doc_id})

    # Step 2 — chunk
    chunks = chunk_document(content)

    # Step 3 — embed and upsert with metadata for filtered retrieval
    embeddings = encoder.encode(chunks).tolist()
    collection.upsert(
        ids=[f"{doc_id}::chunk_{i}" for i in range(len(chunks))],
        embeddings=embeddings,
        documents=chunks,
        metadatas=[{
            "doc_id":      doc_id,
            "title":       title,
            "source_url":  source_url,
            "category":    category,
            "chunk_index": i,
            "ingested_at": datetime.datetime.utcnow().isoformat()
        } for i in range(len(chunks))]
    )
```

**Chunking strategies** — how you split documents directly affects retrieval quality:

| Strategy | How | Best For |
|---|---|---|
| **Fixed-size with overlap** | Split every N tokens; overlap the last M tokens with the next chunk | General-purpose corpora, fast to implement |
| **Sentence / paragraph boundary** | Split on `.`, `\n\n`; never cut mid-sentence | Narrative text, policies, articles |
| **Recursive** | Try paragraph → sentence → word; stop when chunk is small enough | Mixed-format documents |
| **Structure-aware** | Split on headings (`#`, `##`); keep section context in metadata | Technical docs, wikis, API references |

**Metadata to attach per chunk** — metadata is what enables *filtered retrieval* at query time (e.g., "search only within HR policies" or "only docs updated in the last 6 months"):

- `doc_id` — deterministic identifier; used to delete/replace chunks on update
- `title` / `source_url` — surfaced to the user as a citation
- `category` / `topic` — enables category-scoped retrieval
- `ingested_at` — used for freshness filtering
- `security_level` — enforces access control at retrieval time (only return chunks the user is authorised to see)

**Handling document updates correctly** is the most common bug in semantic memory pipelines. The critical rule: **always delete by `doc_id` before re-inserting**. If you skip the delete, the old (stale) chunks coexist with new ones in the index, and retrieval may surface the outdated version alongside the current one.

#### Interview Questions

**Q1: What's the difference between episodic and semantic memory in an agent?**

> Episodic memory stores *what happened* — events, past interactions, and outcomes — and is scoped to a specific user or session. Semantic memory stores *what is known* — facts, documents, and domain knowledge — and is shared across all users and sessions.
>
> The critical operational difference: episodic memory is written by the agent at runtime (at the end of a session or on key events); semantic memory is written by external data ingestion pipelines, never by the agent itself. In storage terms: episodic is typically a document store or cache (Redis, DynamoDB, MongoDB), semantic is a vector search index (Chroma, Pinecone, Weaviate, pgvector).

---

**Q2: Why is the LLM's context window described as a type of memory? What are its limitations?**

> The context window functions as **working memory** — it holds everything the model can currently "see" and reason over. It is the only memory the LLM reads directly; all other memory types (episodic, semantic, procedural) must be explicitly loaded into it before a Planning step.
>
> Its limitations:
> - **Hard token cap** (128k–200k tokens depending on the model): you cannot fit an unlimited history
> - **No persistence**: the window is destroyed when the session ends — nothing carries over automatically
> - **Cost scales with size**: every token in context costs money on every LLM call
> - **Recency bias**: LLMs tend to attend more strongly to content near the end of the window (the "lost in the middle" problem), so very long contexts may cause the model to ignore early instructions

---

**Q3: A user complains the agent doesn't remember their preferences from last week. Which memory type is missing and how would you implement it?**

> This is a missing **episodic memory** store. The agent has in-context memory for the current session and may have semantic memory for domain knowledge, but it has no mechanism to persist and recall *per-user, cross-session* state.
>
> Implementation steps:
> 1. At session end, serialize a summary of what the user preferred (tools, tone, data sources) to a document store keyed by `user_id`
> 2. At the start of each new session, retrieve that record and inject a *user profile* block into the context window above the conversation history
> 3. Optionally embed the preference summary and store it in a vector index so the agent can do semantic recall across all past sessions ("last time I asked about X…")

---

**Q4: What are the risks of allowing an agent to write to its own semantic memory at runtime?**

> Three concrete failure modes:
>
> 1. **Hallucination propagation** — the agent writes a confident-sounding but wrong fact into the knowledge base. Future retrievals surface that wrong fact to all users.
> 2. **Prompt injection via write path** — a malicious user crafts input that causes the agent to write adversarial content into the index, poisoning future retrieval for other users.
> 3. **Drift over time** — with no human review gate, the semantic store gradually diverges from authoritative source-of-truth documents, making the agent's answers less reliable as time passes.
>
> The safe pattern: agents read from semantic memory, and a separate, human-reviewed ingestion pipeline writes to it. Enforce this at the infrastructure level with Role-Based Access Control — the agent's service identity gets read-only access to the vector index, and only the ingestion pipeline holds write access.

---

**Q5: How would you prevent an agent from "forgetting" context mid-task when the conversation grows beyond the context window limit?**

> Three strategies, in order of implementation complexity:
>
> | Strategy | How | When to use |
> |---|---|---|
> | **Sliding window** | Discard the oldest N conversation turns when the token count exceeds 80% of the limit | Short-lived tasks where early turns are low-value |
> | **Summarization** | Call the LLM to compress old turns into a 200–400 token summary block; replace the old turns with the summary | Most enterprise agents — good balance of cost and fidelity |
> | **Hierarchical retrieval** | Move old turns to the episodic store; at each Planning step, embed the current query and retrieve the most relevant past turns on demand | Long-running agents, complex multi-day tasks |
>
> For most production systems, **summarization** is the right default. Implement it as a background step: once the estimated token count crosses a threshold, summarize the oldest 50% of turns and replace them with the compressed block before the next LLM call.

---

**Q6: Compare in-context memory vs. episodic memory on cost, latency, and durability.**

> | Dimension | In-Context Memory | Episodic Memory |
> |---|---|---|
> | **Cost** | High — every token in context is charged on every LLM call | Low — a document store key-lookup is a fraction of a cent per thousand reads |
> | **Latency** | Zero (already in the prompt) | 5–50 ms for a key-based read; 50–200 ms for a semantic search recall |
> | **Durability** | None — destroyed when the session ends | Persistent — survives process restarts, days, weeks |
> | **Scope** | Single session, single agent instance | Cross-session, can be shared across agent instances for the same user |
> | **Best for** | Current task state, tool results, active reasoning chain | User preferences, prior task outcomes, cross-session continuity |
>
> The practical rule: keep only what the current iteration *needs* in context; push everything else to episodic storage and pull it back with a targeted retrieval when needed.

---

**Q7: Does RAG represent semantic memory?**

> Mostly yes, but they are not the same thing — RAG is the *retrieval mechanism*, semantic memory is the *storage concept*.
>
> | | RAG | Semantic Memory |
> |---|---|---|
> | **What it is** | A retrieval technique | A memory type (architectural concept) |
> | **Defines** | *How* content moves from storage into the context window | *What* is stored and its lifecycle properties |
> | **Layer** | Runtime retrieval pattern | Storage + content classification |
>
> RAG implements access to semantic memory in the common case — it embeds the query, does a vector similarity search against the knowledge store, and injects the top-k chunks into the context window. But they diverge in two important ways:
>
> 1. **RAG can also read from episodic memory.** If past session summaries are embedded and stored in a vector index, RAG retrieves relevant prior interactions. That is still RAG, but the *source* is episodic memory, not semantic.
> 2. **Semantic memory can be read without RAG.** A keyword search, a SQL lookup, or a direct document fetch are all valid ways to access a knowledge store. RAG just happens to be the most effective pattern for unstructured corpora.
>
> The clean mental model:
> ```
> Semantic Memory  =  the indexed knowledge corpus  (permanent, shared, org-wide)
> RAG              =  the pattern that reads from it and loads chunks into in-context memory
> ```
> RAG is the *verb*, semantic memory is the *noun*. When an interviewer asks "how does the agent use semantic memory?", the answer is almost always "via RAG."

---

**Q8: When and how does content get written into semantic memory? Can the agent do it?**

> The agent **cannot and should not** write to semantic memory at runtime. Semantic memory is written exclusively by an external **data ingestion pipeline**, triggered by discrete events — not by the agent's reasoning loop.
>
> **When it gets written:**
> - A new document is published (new policy, product spec, knowledge article)
> - An existing document is updated — old chunks are deleted by `doc_id` first, then re-ingested
> - A document is retired — its chunks are deleted from the index
> - A scheduled re-index job detects that a source document's hash has changed
> - A human approval gate is passed — content only enters the index after review
>
> **How the pipeline works (four steps):**
> 1. **Extract** — pull raw text from the source (file storage, CMS, database)
> 2. **Chunk** — split into overlapping segments (e.g., 512 tokens with 50-token overlap) using a strategy suited to the document type
> 3. **Embed** — convert each chunk to a dense vector using an embedding model
> 4. **Upsert** — write (chunk text + vector + metadata) into the vector index; metadata enables filtered retrieval and citations
>
> **Why the agent must not write to it:** If the agent wrote its own observations or inferences into the semantic store, hallucinations would propagate to all future users, adversarial inputs could poison the index via prompt injection, and the store would drift from authoritative source documents with no human review gate. The fix is structural: give the agent's service identity read-only access to the index, and reserve write access for the ingestion pipeline service account only.

---

### 3.6 Tool / Function Calling

Tools are the actuators of an agent — the way it interacts with the outside world. The LLM is told what tools exist (name, description, input schema) and decides when and how to call them.

```
                    Selects tool + generates JSON args
  ┌─────────┐  ──────────────────────────────────────►  ┌─────────────────┐
  │ 🧠 LLM  │                                            │ Tool Dispatcher │
  │         │◄──────────────────────────────────────────  └────────┬────────┘
  └─────────┘              Result                                   │
                                              ┌────────────────────┼─────────────────────┐
                                              ▼                    ▼                     ▼
                                    ┌─────────────────┐ ┌─────────────────┐ ┌──────────────────┐
                                    │ 📊 search_      │ │ 📧 send_email   │ │ 🗄️ query_        │
                                    │   documents     │ │                 │ │    database      │
                                    └─────────────────┘ └─────────────────┘ └──────────────────┘
                                    ┌─────────────────┐ ┌─────────────────┐
                                    │ 🐍 execute_code │ │ 🌐 call_rest_   │
                                    │                 │ │    api          │
                                    └─────────────────┘ └─────────────────┘
```

---

## 4. Deep Technical Detail

### 4.1 How the LLM Reasons

Modern LLMs used for agents (GPT-4o, o1, Claude 3.5 Sonnet) process a **context window** containing:

```
[System Prompt]
  → Agent persona, capabilities, constraints, tool definitions (JSON Schema)

[Conversation History]
  → Prior turns (user messages, assistant thoughts, tool results)

[Current Turn]
  → User goal or the result of the last tool call
```

The LLM produces either:
- A **tool call** response: structured JSON specifying `tool_name` + `arguments`
- A **final answer**: natural language response to the user

The hosting framework (Semantic Kernel, LangChain, AutoGen) intercepts tool calls, executes them against real backends, and feeds results back into the context.

### 4.2 Token Budget Management

Every agent interaction consumes tokens. Poor token management is the #1 source of production agent failures.

```
┌────────────────────────────────────────────────────────────────┐
│                 CONTEXT WINDOW  (~128K tokens)                 │
│                                                                │
│  ┌──────────────────┐  ┌──────────────────┐  ┌─────────────┐  │
│  │  System Prompt   │  │  Tool Definitions │  │ Generation  │  │
│  │   ~2–5K tokens   │  │   ~1–3K tokens    │  │   Budget    │  │
│  │   (stable)       │  │   (stable)        │  │  ~4K tokens │  │
│  └──────────────────┘  └──────────────────┘  └─────────────┘  │
│                                                                │
│  ┌───────────────────────────┐   ┌─────────────────────────┐   │
│  │  Conversation History ⚠️  │   │   Tool Results  ⚠️      │   │
│  │  grows unbounded          │   │   can be very large     │   │
│  │                           │   │                         │   │
│  │  → must truncate          │   │  → must filter / chunk  │   │
│  │    or summarize old turns │   │    before adding        │   │
│  └───────────────────────────┘   └─────────────────────────┘   │
│                                                                │
│  ⚠️  Unmanaged history + large results = context overflow      │
└────────────────────────────────────────────────────────────────┘
```

**Enterprise rule:** Implement a `ContextManager` that summarizes old turns and truncates tool outputs beyond a configurable byte limit.

### 4.3 Failure Modes

| Failure Mode | Description | Mitigation |
|---|---|---|
| **Hallucination** | LLM fabricates tool arguments or facts | Validate outputs with JSON Schema; use structured outputs |
| **Infinite loop** | Agent keeps re-planning without making progress | Max iteration limit (default: 10 steps) |
| **Tool failure cascade** | One tool error causes all downstream steps to fail | Retry with exponential backoff; fallback tools |
| **Context overflow** | History exceeds context window | Rolling summary + token budget enforcement |
| **Prompt injection** | Malicious content in tool results hijacks reasoning | Sanitize tool outputs; use content safety |
| **Goal drift** | Agent pursues a sub-goal and forgets the original | Include original goal in every LLM call |

### 4.4 Agent Autonomy Spectrum

```
◄──────── More Human Control                   More Automation ────────►

 ┌──────────────┐      ┌──────────────┐      ┌──────────────┐      ┌──────────────┐
 │ 🔴 FULLY     │─────►│ 🟡 SUPERVISED│─────►│ 🟢 SEMI-     │─────►│ 🔵 FULLY     │
 │    GUIDED    │      │              │      │    AUTONOMOUS│      │    AUTONOMOUS│
 │              │      │              │      │              │      │              │
 │ Human        │      │ Human        │      │ Human        │      │ Human        │
 │ approves     │      │ approves     │      │ reviews      │      │ monitors via │
 │ every action │      │ high-risk    │      │ final output │      │ dashboards   │
 │              │      │ actions only │      │ only         │      │              │
 │──────────────│      │──────────────│      │──────────────│      │──────────────│
 │ Use when:    │      │ Use when:    │      │ Use when:    │      │ Use when:    │
 │ Stakes are   │      │ Starting out │      │ ≥95% accuracy│      │ Proven,      │
 │ very high    │      │ ← START HERE │      │ after 30 days│      │ low-stakes   │
 └──────────────┘      └──────────────┘      └──────────────┘      └──────────────┘
```

**Enterprise guidance:** Start at Supervised. Move to Semi-autonomous only after 30 days of production data showing accuracy ≥ 95% on your task category.

---

## 5. Azure AI Foundry Implementation

### 5.1 Azure AI Foundry Agent Service

Azure AI Foundry's Agent Service provides a managed runtime for agents with built-in threading, tool execution, file handling, and evaluation — no custom framework needed for straightforward use cases.

**Architecture:**

```
┌──────────────────────────────────────────────────────────┐
│         Client App  (Python / TypeScript / REST)         │
└─────────────────────────┬────────────────────────────────┘
                          │
                          ▼
┌──────────────────────────────────────────────────────────┐
│              Azure AI Foundry Agent Service              │
│                                                          │
│  ┌────────────────────┐  ┌──────────┐  ┌─────────────┐  │
│  │   Agent Object     │  │  Thread  │  │     Run     │  │
│  │ instructions +     │  │  (conv.  │  │ (execution  │  │
│  │ tools + model      │  │  state)  │  │  instance)  │  │
│  └────────────────────┘  └──────────┘  └─────────────┘  │
│                                                          │
│  ┌───────────────────────────────────────────────────┐   │
│  │  Run Steps  (tool calls + messages per iteration) │   │
│  └───────────────────────────────────────────────────┘   │
└───────────────────┬──────────────────────┬───────────────┘
                    │                      │
                    ▼                      ▼
       ┌────────────────────┐   ┌──────────────────────┐
       │  Azure OpenAI      │   │    Built-in Tools    │
       │  GPT-4o / o1       │   │                      │
       └────────────────────┘   │  • Code Interpreter  │
                                │  • File Search       │
                                │  • Function Calling  │
                                └──────────────────────┘
```

### 5.2 Create a Foundry Project (CLI)

```bash
# Create resource group
az group create --name rg-agents-prod --location eastus2

# Create Azure AI Hub (the organizational container)
az ml workspace create \
    --kind hub \
    --name aih-enterprise-prod \
    --resource-group rg-agents-prod \
    --location eastus2

# Create AI Project under the hub
az ml workspace create \
    --kind project \
    --name aip-support-agent \
    --resource-group rg-agents-prod \
    --hub-id /subscriptions/<sub>/resourceGroups/rg-agents-prod/providers/Microsoft.MachineLearningServices/workspaces/aih-enterprise-prod

# Assign Cognitive Services OpenAI User to project's Managed Identity
az role assignment create \
    --role "Cognitive Services OpenAI User" \
    --assignee <managed-identity-principal-id> \
    --scope /subscriptions/<sub>/resourceGroups/rg-agents-prod
```

---

## 6. Working Code Example

A complete, runnable agent that takes a user goal and autonomously uses tools to research and respond.

### Project Structure

```
agentic-fundamentals/
├── .env
├── requirements.txt
├── agent.py          ← main agent
├── tools.py          ← tool definitions
└── run.py            ← entry point
```

### tools.py

```python
import json
import random
from datetime import datetime, timedelta


def get_sales_data(quarter: str, year: int, region: str = "ALL") -> dict:
    """
    Simulates a database query for sales data.
    In production this would call Azure SQL or Cosmos DB.
    """
    mock_data = {
        ("Q2", 2025, "ALL"): {
            "revenue": 4_200_000,
            "units": 18_400,
            "yoy_growth": 8.2,
            "anomalies": [{"region": "EMEA", "delta_pct": -34.0}],
        },
        ("Q2", 2025, "EMEA"): {
            "revenue": 620_000,
            "prior_year_revenue": 940_000,
            "delta_pct": -34.0,
            "root_cause": "3 major accounts churned in April",
            "churned_accounts": ["Acme Corp", "GlobalTech", "EuroRetail"],
        },
    }
    key = (quarter.upper(), year, region.upper())
    result = mock_data.get(key, {"error": f"No data found for {quarter} {year} {region}"})
    return result


def send_report_email(to: str, subject: str, body: str) -> dict:
    """Simulates sending an email report. In production: Microsoft Graph API."""
    print(f"\n[EMAIL SENT] To: {to} | Subject: {subject}")
    return {"status": "sent", "message_id": f"msg_{random.randint(10000, 99999)}", "timestamp": datetime.utcnow().isoformat()}


def get_current_date() -> dict:
    """Returns current date context."""
    return {"date": datetime.utcnow().strftime("%Y-%m-%d"), "quarter": "Q2"}


# Tool definitions in OpenAI function-calling format
TOOL_DEFINITIONS = [
    {
        "type": "function",
        "function": {
            "name": "get_sales_data",
            "description": "Query sales data for a given quarter, year, and optional region. Use this to retrieve revenue, unit counts, YoY growth, and anomalies.",
            "parameters": {
                "type": "object",
                "properties": {
                    "quarter": {"type": "string", "description": "Quarter in format Q1, Q2, Q3, Q4"},
                    "year": {"type": "integer", "description": "4-digit year"},
                    "region": {
                        "type": "string",
                        "description": "Region filter. Use 'ALL' for global, or specify: EMEA, APAC, AMER",
                        "default": "ALL",
                    },
                },
                "required": ["quarter", "year"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "send_report_email",
            "description": "Send a formatted report via email to a recipient.",
            "parameters": {
                "type": "object",
                "properties": {
                    "to": {"type": "string", "description": "Recipient email address"},
                    "subject": {"type": "string", "description": "Email subject line"},
                    "body": {"type": "string", "description": "Email body in plain text or Markdown"},
                },
                "required": ["to", "subject", "body"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_current_date",
            "description": "Get today's date and current quarter. Use this when you need temporal context.",
            "parameters": {"type": "object", "properties": {}},
        },
    },
]

TOOL_DISPATCH = {
    "get_sales_data": get_sales_data,
    "send_report_email": send_report_email,
    "get_current_date": get_current_date,
}
```

### agent.py

```python
import json
import os
from openai import AzureOpenAI
from dotenv import load_dotenv
from tools import TOOL_DEFINITIONS, TOOL_DISPATCH

load_dotenv()

client = AzureOpenAI(
    azure_endpoint=os.environ["AZURE_OPENAI_ENDPOINT"],
    api_key=os.environ["AZURE_OPENAI_API_KEY"],
    api_version=os.environ.get("AZURE_OPENAI_API_VERSION", "2024-10-21"),
)

DEPLOYMENT = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o")

SYSTEM_PROMPT = """You are an Enterprise Sales Analysis Agent. 
Your job is to help business stakeholders understand sales performance and anomalies.

You have access to tools to query sales data and send email reports.

When analyzing data:
1. Always retrieve global data first, then drill into anomalies
2. Be specific about numbers — never say "significantly" without a percentage
3. Suggest root causes when data supports them
4. Only send emails when explicitly asked by the user

Respond in a professional, executive-ready tone."""


def run_agent(user_goal: str, max_iterations: int = 10) -> str:
    """
    Core ReAct agent loop.
    Continues until the LLM produces a final text response (no more tool calls)
    or the max iteration limit is reached.
    """
    messages = [
        {"role": "system", "content": SYSTEM_PROMPT},
        {"role": "user", "content": user_goal},
    ]

    print(f"\n{'='*60}")
    print(f"AGENT GOAL: {user_goal}")
    print(f"{'='*60}")

    for iteration in range(max_iterations):
        print(f"\n--- Iteration {iteration + 1} ---")

        response = client.chat.completions.create(
            model=DEPLOYMENT,
            messages=messages,
            tools=TOOL_DEFINITIONS,
            tool_choice="auto",
        )

        message = response.choices[0].message

        # No tool calls → agent is done
        if not message.tool_calls:
            print(f"\nFINAL ANSWER:\n{message.content}")
            return message.content

        # Process each tool call in the response
        messages.append({"role": "assistant", "content": message.content, "tool_calls": [
            {
                "id": tc.id,
                "type": "function",
                "function": {"name": tc.function.name, "arguments": tc.function.arguments},
            }
            for tc in message.tool_calls
        ]})

        for tool_call in message.tool_calls:
            fn_name = tool_call.function.name
            fn_args = json.loads(tool_call.function.arguments)

            print(f"  TOOL CALL: {fn_name}({json.dumps(fn_args, indent=2)})")

            if fn_name not in TOOL_DISPATCH:
                result = {"error": f"Unknown tool: {fn_name}"}
            else:
                try:
                    result = TOOL_DISPATCH[fn_name](**fn_args)
                except Exception as e:
                    result = {"error": str(e)}

            print(f"  TOOL RESULT: {json.dumps(result, indent=2)}")

            messages.append({
                "role": "tool",
                "tool_call_id": tool_call.id,
                "content": json.dumps(result),
            })

    return "Max iterations reached. Task incomplete."
```

### run.py

```python
from agent import run_agent

if __name__ == "__main__":
    # Example 1: Analysis only
    result = run_agent(
        "Summarize our Q2 2025 global sales performance and explain any anomalies in detail."
    )

    # Example 2: Analysis + action
    # result = run_agent(
    #     "Summarize our Q2 2025 sales performance and email the report to cfo@company.com"
    # )
```

### Expected Output

```
============================================================
AGENT GOAL: Summarize our Q2 2025 global sales performance and explain any anomalies in detail.
============================================================

--- Iteration 1 ---
  TOOL CALL: get_sales_data({
    "quarter": "Q2",
    "year": 2025
  })
  TOOL RESULT: {
    "revenue": 4200000,
    "units": 18400,
    "yoy_growth": 8.2,
    "anomalies": [{"region": "EMEA", "delta_pct": -34.0}]
  }

--- Iteration 2 ---
  TOOL CALL: get_sales_data({
    "quarter": "Q2",
    "year": 2025,
    "region": "EMEA"
  })
  TOOL RESULT: {
    "revenue": 620000,
    "prior_year_revenue": 940000,
    "delta_pct": -34.0,
    "root_cause": "3 major accounts churned in April",
    "churned_accounts": ["Acme Corp", "GlobalTech", "EuroRetail"]
  }

--- Iteration 3 ---

FINAL ANSWER:
## Q2 2025 Sales Performance Summary

**Global Performance**
- Total Revenue: $4.2M (+8.2% YoY)
- Units Sold: 18,400

**Anomaly: EMEA Region — ⚠️ -34% YoY**
- Q2 2025: $620K vs. Q2 2024: $940K (−$320K)
- Root Cause: Three major accounts (Acme Corp, GlobalTech, EuroRetail) churned in April 2025

**Recommendation:** Immediate retention analysis on remaining EMEA accounts; initiate win-back campaign for churned accounts.
```

---

## 7. Enterprise Pattern Notes

### Pattern 1: ReAct (Reason + Act)
The foundational loop demonstrated in the code above. Best for: task completion agents, research agents, data analysis.

### Pattern 2: Reflection
The agent reviews its own output before returning it to the user, catching errors and improving quality.

```
  ┌──────┐    ┌────────────────┐    ┌──────────────────┐
  │ GOAL │───►│ Generate Draft │───►│  Self-Critique   │◄──┐
  └──────┘    └────────────────┘    │  (LLM call)      │   │
                                    └────────┬─────────┘   │
                                             │              │
                             ┌── Issues? ────┤              │
                             │               │ Passes       │
                             ▼               ▼              │
                      ┌──────────┐   ┌──────────────┐      │
                      │  Revise  │───┘│ Final Output │      │
                      └──────────┘   └──────────────┘      │
                           │                                │
                           └────────────────────────────────┘
                                    (loop until passes)
```

### Pattern 3: Supervisor–Worker
One LLM supervises multiple specialized worker agents, delegating sub-tasks and aggregating results. Covered in depth in [10 — Multi-Agent Systems](./10-Multi-Agent-Systems.md).

### Pattern 4: Human-in-the-Loop (HITL)
Agent pauses at critical decision points and waits for human approval before proceeding. Required for high-stakes actions (payments, emails to customers, infrastructure changes).

```
  ┌─────────┐     ┌──────────────────────┐
  │  AGENT  │────►│  High-risk action?   │
  └─────────┘     └───────────┬──────────┘
                              │
                 ┌── No ──────┤
                 │            │ Yes
                 ▼            ▼
         ┌────────────┐  ┌──────────────────────┐
         │ Execute    │  │  Pause & Notify Human │
         │   Tool     │  └──────────┬────────────┘
         └────────────┘             │
                              Human Decision
                         ┌──────────┴──────────┐
                      Approved             Rejected
                         │                     │
                         ▼                     ▼
                  ┌────────────┐       ┌────────────────┐
                  │  Execute   │       │  Agent Replans │
                  │   Tool     │       └────────────────┘
                  └────────────┘
```

---

## 7.1 The AI Agent 4-Part Loop

Every autonomous AI agent — regardless of framework — executes the same fundamental loop:

```
                    ┌─────────────────────────────────────┐
                    │      GOAL / TERMINATION CHECK       │
                    │  • Is the goal achieved?            │
                    │  • Max iterations reached?          │
                    │  • User interrupted?                │
                    └──────────┬──────────────────────────┘
               Continue       │                │ Goal met / limit
                              │                ▼
                              │        ┌──────────────────┐
                              │        │  Return Final    │
                              │        │    Response      │
                              │        └──────────────────┘
                              ▼
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│  1. PERCEIVE │─────►│  2. REASON   │─────►│   3. ACT     │
│              │      │              │      │              │
│ Observe:     │      │ Plan:        │      │ Execute:     │
│ user input,  │      │ CoT, ReAct,  │      │ call tools,  │
│ tool results,│      │ tool select, │      │ write files, │
│ memory, APIs │      │ goal check   │      │ send msgs,   │
│              │      │              │      │ call APIs    │
└──────────────┘      └──────────────┘      └──────┬───────┘
       ▲                                           │
       │                                           ▼
       │                                    ┌──────────────┐
       └────────────────────────────────────│  4. REMEMBER │
                                            │              │
                                            │ Update conv. │
                                            │ history,     │
                                            │ write episodic│
                                            │ memory,      │
                                            │ record state │
                                            └──────┬───────┘
                                                   │
                                                   └──► GOAL CHECK
```

**Loop implementation in LangGraph:**

```python
# four_part_loop.py — the agent loop made explicit
from typing import TypedDict, Annotated
from langgraph.graph import StateGraph, END
from langgraph.graph.message import add_messages

class AgentState(TypedDict):
    messages: Annotated[list, add_messages]  # Conversation + tool results (Perception + Memory)
    iteration: int                            # Loop counter
    goal_achieved: bool

def perception_node(state: AgentState) -> AgentState:
    """Observe: current messages already contain perception — return unchanged."""
    return {"iteration": state["iteration"] + 1}

def reasoning_node(state: AgentState) -> AgentState:
    """Reason: LLM decides next action (tool call or final answer)."""
    response = llm_with_tools.invoke(state["messages"])
    return {"messages": [response]}

def action_node(state: AgentState) -> AgentState:
    """Act: execute tool calls from the LLM's reasoning step."""
    last_message = state["messages"][-1]
    tool_results = execute_tool_calls(last_message.tool_calls)
    return {"messages": tool_results}

def memory_node(state: AgentState) -> AgentState:
    """Remember: persist anything needed across future iterations."""
    save_to_episodic_memory(state["messages"][-1])
    return {}

def should_continue(state: AgentState) -> str:
    last = state["messages"][-1]
    if state["iteration"] >= 10:         # Hard iteration cap
        return "end"
    if not hasattr(last, "tool_calls") or not last.tool_calls:
        return "end"                     # No more tool calls → LLM is done
    return "action"

graph = StateGraph(AgentState)
graph.add_node("reasoning", reasoning_node)
graph.add_node("action", action_node)
graph.add_node("memory", memory_node)
graph.set_entry_point("reasoning")
graph.add_conditional_edges("reasoning", should_continue, {"action": "action", "end": END})
graph.add_edge("action", "memory")
graph.add_edge("memory", "reasoning")
agent = graph.compile()
```

---

## 7.2 RICEFWID — Agent Capability Assessment Framework

**RICEFWID** is a structured framework for evaluating whether an agent has the prerequisites to complete a task. Use it at design time (do we have everything the agent needs?) and at runtime (why did the agent fail?).

| Letter | Stands for | Question to ask |
|---|---|---|
| **R** | Resources | Does the agent have sufficient compute, memory, and token budget? |
| **I** | Intent | Is the agent's goal clearly specified and unambiguous? |
| **C** | Capabilities | Does the agent have the right tools and skills for this task? |
| **E** | Experience | Has the agent (or its prompt) been grounded with relevant examples? |
| **F** | Features | Are all required model features available (tool calling, structured output)? |
| **W** | Workflows | Is the orchestration pattern right for this task's complexity? |
| **I** | Inputs | Are all required inputs available and in the right format? |
| **D** | Data | Does the agent have access to accurate, up-to-date data via RAG or tools? |

```python
# ricefwid_checklist.py — use before deploying a new agent use case
from dataclasses import dataclass

@dataclass
class RICEFWIDAssessment:
    use_case: str
    resources:    str   # Token budget, compute, latency budget
    intent:       str   # Goal statement — is it specific and measurable?
    capabilities: list[str]  # List of tools the agent has
    experience:   str   # Few-shot examples or grounding data
    features:     list[str]  # Model features needed (tool_calling, vision, structured_output)
    workflows:    str   # Orchestration pattern (ReAct, Plan-and-Execute, Supervisor)
    inputs:       list[str]  # Required input fields
    data:         str   # RAG knowledge base, APIs, or databases available

# Example assessment for an HR Policy Agent
hr_assessment = RICEFWIDAssessment(
    use_case="HR Policy Q&A Copilot",
    resources="GPT-4o, 30s latency budget, 4096 token context window for response",
    intent="Answer employee questions about HR policies accurately and within 30 seconds",
    capabilities=["azure_ai_search", "get_employee_profile", "escalate_to_hr_team"],
    experience="10 few-shot Q&A pairs from HR team, grounded in policy doc v3.2",
    features=["tool_calling", "structured_output"],
    workflows="ReAct (search → answer, 3 max iterations)",
    inputs=["employee_id", "question", "locale"],
    data="Azure AI Search index of all HR policy documents (updated weekly)",
)
```

---

## 7.3 Claude Agentic Harness

The **Claude Agentic Harness** is Anthropic's reference architecture for building agents with Claude. It defines how Claude's tool use, context management, and multi-turn reasoning loop fits together — and is the basis for Claude Code, Claude Desktop, and any custom Claude-powered agent.

> Source: [Anthropic Concepts & Architecture](https://platform.claude.com/docs/concepts-and-architecture)

```
┌──────────────────────────────────────────────────────────────────┐
│                     CLAUDE AGENTIC HARNESS                       │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │  User Message  (or automated trigger)                     │   │
│  └─────────────────────────┬─────────────────────────────────┘   │
│                            │                                      │
│                            ▼                    ┌──────────────┐  │
│  ┌───────────────────────────────────────────┐  │ Token Budget │  │
│  │  Context Assembly                         │◄►│ (track ctx   │  │
│  │  • System prompt (persona, tools,         │  │  growth)     │  │
│  │    constraints)                           │  └──────────────┘  │
│  │  • Conversation history                   │                    │
│  │  • Tool results from prior turns          │                    │
│  └─────────────────────────┬─────────────────┘                   │
│                            │                                      │
│                            ▼                                      │
│              ┌─────────────────────────┐                          │
│              │     Claude API Call     │                          │
│              │  claude-sonnet / opus   │                          │
│              └─────────────┬───────────┘                          │
│                            │                                      │
│                    stop_reason?                                   │
│           ┌── end_turn ────┤                                      │
│           │                └── tool_use ──────────────────────┐   │
│           ▼                                                    ▼   │
│  ┌─────────────────┐          ┌──────────────────────────────┐ │   │
│  │  Return to User │          │  Tool Dispatcher             │ │   │
│  └─────────────────┘          │  (your application code)     │ │   │
│                               └──────────┬───────────────────┘ │   │
│                                          │          ┌──────────┴─┐  │
│                         ┌────────────────┼──────────►  Max Iters │  │
│                         │                │          │  HITL Check│  │
│                         ▼                ▼          └────────────┘  │
│               ┌──────────────────────────────────┐                  │
│               │  Execute Tool                    │                  │
│               │  (file I/O · API · DB · shell)   │                  │
│               └──────────────┬───────────────────┘                  │
│                              │                                      │
│                   Append tool_result to                             │
│                   conversation history ─────────────────────────────┼──► Context Assembly
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

### Key Harness Concepts

**1. `stop_reason` drives the loop**

Claude signals its intent through `stop_reason` in the API response:

| `stop_reason` | Meaning | Harness action |
|---|---|---|
| `end_turn` | Claude has finished its response | Return to user — loop ends |
| `tool_use` | Claude wants to call one or more tools | Execute tools, append results, call Claude again |
| `max_tokens` | Output was cut off at the token limit | Increase `max_tokens` or simplify the task |
| `stop_sequence` | A custom stop sequence was triggered | Application-defined — inspect and handle |

**2. The `tool_result` message format**

After executing a tool, the harness appends the result as a `user`-role message containing a `tool_result` block:

```python
# claude_harness.py — minimal agentic loop with Claude
import asyncio
import anthropic

client = anthropic.AsyncAnthropic()

TOOLS = [
    {
        "name": "read_file",
        "description": "Read the contents of a file from the local filesystem.",
        "input_schema": {
            "type": "object",
            "properties": {
                "path": {"type": "string", "description": "Absolute path to the file"}
            },
            "required": ["path"],
        },
    },
    {
        "name": "write_file",
        "description": "Write content to a file. Creates the file if it does not exist.",
        "input_schema": {
            "type": "object",
            "properties": {
                "path": {"type": "string"},
                "content": {"type": "string"},
            },
            "required": ["path", "content"],
        },
    },
]


async def execute_tool(name: str, inputs: dict) -> str:
    if name == "read_file":
        with open(inputs["path"]) as f:
            return f.read()
    if name == "write_file":
        with open(inputs["path"], "w") as f:
            f.write(inputs["content"])
        return f"Written {len(inputs['content'])} chars to {inputs['path']}"
    return f"Unknown tool: {name}"


async def run_agent(user_message: str, max_iterations: int = 10) -> str:
    messages = [{"role": "user", "content": user_message}]

    for iteration in range(max_iterations):
        response = await client.messages.create(
            model="claude-sonnet-4-6",
            max_tokens=4096,
            system="You are a helpful coding assistant. Use tools to read and write files as needed.",
            tools=TOOLS,
            messages=messages,
        )

        # Append Claude's response to history
        messages.append({"role": "assistant", "content": response.content})

        if response.stop_reason == "end_turn":
            # Extract text from response content blocks
            return next(
                (block.text for block in response.content if hasattr(block, "text")),
                "",
            )

        if response.stop_reason == "tool_use":
            tool_results = []
            for block in response.content:
                if block.type == "tool_use":
                    result = await execute_tool(block.name, block.input)
                    tool_results.append({
                        "type": "tool_result",
                        "tool_use_id": block.id,
                        "content": result,
                    })
            # Append all tool results as a single user message
            messages.append({"role": "user", "content": tool_results})

    return "Max iterations reached without a final answer."
```

**3. Token budget management — `betas=["token-efficient-tools-2025-02-19"]`**

Claude's harness tracks how many tokens the conversation history consumes. As context grows through multiple tool calls, the harness must:
- Monitor `usage.input_tokens` in each response
- Summarize or truncate older tool results when approaching 80% of the model's context limit
- Use Claude's `cache_control` breakpoints to cache the system prompt and reduce cost on repeated calls

**4. Extended Thinking**

For complex reasoning tasks, enable `thinking` blocks (`betas=["interleaved-thinking-2025-05-14"]`). Claude reasons through a problem step-by-step in a `thinking` content block before generating tool calls or a final answer. This is particularly useful for multi-step planning tasks where the correct sequence of tool calls is non-obvious.

### Claude Harness vs Generic Agent Frameworks

| Dimension | Claude Harness (direct API) | LangGraph / CrewAI |
|---|---|---|
| **Control** | Full — you own the loop | Framework-managed — less flexible |
| **Transparency** | Every message visible | Abstracted — harder to debug |
| **Latency** | Minimal overhead | Framework adds overhead |
| **Features** | Must implement memory/state yourself | Built-in memory, persistence, branching |
| **Best for** | Production systems needing precise control | Rapid prototyping, complex multi-agent graphs |

---

## 8. Production Checklist

### Security
- [ ] System prompt hardened against prompt injection (see [33 — Security](./33-Security.md))
- [ ] Tool outputs sanitized before injecting into LLM context
- [ ] Tool permissions follow least-privilege (agent can only read, not write, unless required)
- [ ] Secrets stored in Azure Key Vault, not `.env` in production
- [ ] Managed Identity used for all Azure service authentication

### Monitoring
- [ ] Every LLM call logged with: prompt tokens, completion tokens, latency, model version
- [ ] Tool call results logged (with PII redaction)
- [ ] Iteration count per run tracked (alert if > 7 iterations consistently)
- [ ] Final answer logged for quality review sampling
- [ ] Azure Application Insights connected for distributed tracing

### Cost
- [ ] Max iteration limit enforced (prevents runaway billing)
- [ ] Tool results truncated to a maximum byte length before injecting
- [ ] Use `gpt-4o-mini` for classification/routing sub-tasks; `gpt-4o` for reasoning
- [ ] Enable Azure OpenAI token quota alerts at 80% threshold

### Testing
- [ ] Unit tests for each tool function (separate from the agent)
- [ ] Integration test suite with recorded tool responses (deterministic replay)
- [ ] Adversarial test cases for prompt injection scenarios
- [ ] Evaluation dataset with 50+ golden (input, expected_output) pairs

---

## 9. Interview Q&A

### Q1 (Beginner): What is the difference between a chatbot and an AI agent?

**Answer:** A chatbot makes a single LLM call per user turn and returns a response. An AI agent runs a loop: it can call tools, observe results, re-plan, and call more tools — iterating until a goal is achieved. The key architectural difference is the *agentic loop* with tool execution capability. A chatbot has no memory beyond the conversation window and no ability to take actions in external systems.

---

### Q2 (Beginner): What are the four core components of an AI agent?

**Answer:**
1. **LLM (Brain)** — the reasoning engine that interprets goals and decides what to do
2. **Tools** — functions the agent can call to interact with the outside world (APIs, databases, code execution)
3. **Memory** — context persistence across turns (in-context) and sessions (external stores)
4. **Planning** — the mechanism (often ReAct) by which the agent decides the sequence of actions to achieve a goal

---

### Q3 (Intermediate): What is the ReAct pattern and why is it important?

**Answer:** ReAct (Reason + Act) is an LLM prompting and execution pattern where the model alternates between generating a *Thought* (reasoning about the current state and what to do next) and an *Action* (calling a specific tool with specific arguments). After each action, the result is fed back as an *Observation*. This cycle continues until the goal is achieved. ReAct is important because it makes the agent's reasoning transparent and debuggable — you can inspect the thought-action-observation chain to understand why the agent made decisions, which is critical for production systems.

---

### Q4 (Intermediate): What are the main failure modes of production AI agents and how do you mitigate them?

**Answer:** Key failure modes:
- **Infinite loops** → enforce a max iteration limit (typically 10–15 steps)
- **Context overflow** → implement rolling summarization and truncate tool outputs
- **Hallucinated tool arguments** → use structured outputs (JSON Schema validation) and validate before executing
- **Prompt injection via tool results** → sanitize external content before injecting into LLM context
- **Goal drift** → include the original user goal in every LLM call, not just the first one
- **Tool failure cascades** → wrap every tool call in try/except with retry logic and graceful degradation

---

### Q5 (Advanced): How do you decide between agentic AI and traditional automation (RPA, workflow engines)?

**Answer:** The decision turns on *structure vs. variability*:
- **Traditional automation (RPA, BPM)** excels when: inputs are structured/predictable, the workflow is deterministic, latency is critical (<100ms), and auditability requires deterministic traces
- **Agentic AI** excels when: inputs are unstructured (documents, emails, voice), the workflow requires reasoning about context to determine next steps, the process has high variability, or you need natural-language interfaces

In practice, the enterprise pattern is a hybrid: workflow engines (Azure Logic Apps, Power Automate) handle the structured orchestration layer and invoke agents for the reasoning-intensive sub-tasks. This gives you the reliability of a state machine with the flexibility of an LLM reasoner.

---

### Q6 (Architecture): How would you design an agent system to handle 10,000 concurrent agent runs for an enterprise customer support platform?

**Answer:** Key architectural decisions:

1. **Stateless agent workers** — each agent run is a stateless compute unit (Azure Container Apps or AKS pods), pulling conversation state from Azure Cosmos DB at the start of each iteration
2. **Async tool execution** — all tool calls are async; use `asyncio.gather()` for parallel tool calls where possible
3. **Message-driven orchestration** — incoming tasks land in Azure Service Bus; workers pull and process independently (fan-out pattern)
4. **Token quota management** — Azure OpenAI PTU (Provisioned Throughput Unit) deployments for predictable throughput; per-tenant quota enforcement
5. **Circuit breaker** — if Azure OpenAI returns 429s, fall back to a queue with exponential backoff; never fail silently
6. **Horizontal scaling** — KEDA (Kubernetes Event-Driven Autoscaling) scales worker pods based on Service Bus queue depth
7. **Observability** — OpenTelemetry traces correlated across the entire run (agent → tool → LLM → tool result)
8. **Human escalation path** — runs that exceed iteration limits or hit low-confidence thresholds are routed to a human review queue

---

### Q7 (Scenario): A production agent keeps generating incorrect JSON arguments for a tool call, causing tool failures. How do you fix this?

**Answer:**
1. **Immediate fix:** Enable **structured outputs** (OpenAI's `response_format: {type: "json_schema"}`) to force the model to produce schema-valid JSON for tool arguments. This moves validation to the model inference layer.
2. **Better tool descriptions:** Rewrite the tool's `description` and parameter `description` fields to be more explicit — include examples of valid values.
3. **Validate before executing:** Add a Pydantic model validation step between argument generation and tool execution; return validation errors back to the LLM as an `Observation` so it can self-correct.
4. **Few-shot examples:** Add 2–3 example tool calls (correct arguments) to the system prompt.
5. **Upgrade the model:** Switch from a smaller model to GPT-4o for the planning step if the task requires complex argument construction.
6. **Log and analyze:** Capture every failed tool call with the generated arguments; analyze patterns to identify whether the issue is in the tool schema or the system prompt.

---

## Cross-links

- Previous: [00 — Introduction](./00-Introduction.md)
- Next: [02 — LLMs and Foundation Models](./02-LLMs-and-Foundation-Models.md)
- Related: [05 — Semantic Kernel](./05-Semantic-Kernel.md) | [06 — LangChain](./06-LangChain.md) | [10 — Multi-Agent Systems](./10-Multi-Agent-Systems.md)
- Advanced: [22 — Planning and Reasoning](./22-Planning-and-Reasoning.md) | [21 — Memory](./21-Memory.md)

---

*Module 01 | Series: Enterprise Agentic AI Tutorial | Last updated: June 2026*
