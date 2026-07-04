# Building Azure AI Agents in Microsoft Foundry Portal

> **Source:** [YouTube — Build a Gift Tracker Agent in Microsoft Foundry (20 min)](https://www.youtube.com/watch?v=zKqyLB8qHeM)  
> **Presenter:** Frankie — Microsoft Azure MVP  
> **Extracted & Structured:** End-to-end walkthrough of the Microsoft Foundry portal, agent creation, file search, web search, memory, code interpreter, and deployment.

---

## Table of Contents

1. [What Is Microsoft Foundry Portal?](#1-what-is-microsoft-foundry-portal)
2. [Foundry Portal Navigation Overview](#2-foundry-portal-navigation-overview)
3. [Creating a New Project from Scratch](#3-creating-a-new-project-from-scratch)
4. [The Gift Tracker Agent — Concept](#4-the-gift-tracker-agent--concept)
5. [Agent Tools Overview](#5-agent-tools-overview)
6. [Building the Agent — Step by Step](#6-building-the-agent--step-by-step)
   - [Step 1 — System Prompt (Instructions)](#step-1--system-prompt-instructions)
   - [Step 2 — File Search (Knowledge)](#step-2--file-search-knowledge)
   - [Step 3 — Web Search Tool](#step-3--web-search-tool)
   - [Step 4 — Code Interpreter](#step-4--code-interpreter)
   - [Step 5 — Memory Store](#step-5--memory-store)
7. [Testing the Agent](#7-testing-the-agent)
8. [Operate Tab — Monitoring](#8-operate-tab--monitoring)
9. [Docs Tab & Built-in AI Chatbot](#9-docs-tab--built-in-ai-chatbot)
10. [Deployment Options](#10-deployment-options)
11. [Architecture Diagram](#11-architecture-diagram)
12. [Interview Quick Reference](#12-interview-quick-reference)

---

## 1. What Is Microsoft Foundry Portal?

**Microsoft Foundry** (accessed at [ai.azure.com](https://ai.azure.com)) is the **unified enterprise portal** for building, deploying, and monitoring Azure AI agents and AI applications. It is the successor/evolution of the Azure AI Studio interface.

```mermaid
flowchart LR
    Dev["👩‍💻 Developer"] --> Portal["ai.azure.com\n(Microsoft Foundry Portal)"]
    Portal --> Proj["📁 Projects"]
    Proj --> Agent["🤖 AI Agents\n(Build tab)"]
    Proj --> Models["🧠 Models\n(Model Catalog)"]
    Proj --> Operate["📊 Operate tab\n(Monitoring)"]
    Proj --> Docs["📖 Docs tab\n(Built-in Help)"]

    style Portal fill:#0f172a,color:#fff
    style Agent fill:#1e40af,color:#fff
    style Operate fill:#7c3aed,color:#fff
```

### Key Characteristics
- **Single hub** — discover models, build agents, monitor performance
- **Project-based** — each project scopes its models, agents, and resources
- **Backed by Azure** — all resources live in a linked Azure Resource Group
- **AI chatbot built-in** — ask Foundry questions about the portal itself

---

## 2. Foundry Portal Navigation Overview

```mermaid
graph TB
    subgraph Foundry["🏗️ Microsoft Foundry Portal (ai.azure.com)"]
        direction TB
        Home["🏠 Home / Projects"]

        subgraph Build["🔨 Build Tab"]
            Agents["Agents\n(Create + manage)"]
            Playground["Playground\n(Test models directly)"]
            Models2["Model Deployments"]
        end

        subgraph Operate["📊 Operate Tab"]
            AgentList["Registered Agents"]
            Metrics["Performance Metrics"]
            Register["Register External Agent"]
        end

        Docs["📖 Docs Tab\n(Documentation + AI chatbot)"]
    end

    Home --> Build
    Home --> Operate
    Home --> Docs

    style Build fill:#1e40af,color:#fff
    style Operate fill:#7c3aed,color:#fff
    style Docs fill:#059669,color:#fff
```

| Tab | Purpose |
|---|---|
| **Build** | Create and configure agents, add tools, set instructions |
| **Operate** | Monitor running agents, register existing agents, view metrics |
| **Docs** | Access Microsoft Foundry documentation + ask the built-in AI chatbot |

---

## 3. Creating a New Project from Scratch

### Prerequisites
- An Azure subscription
- A resource group (e.g., `rg-ais-eus2-demo-gift-tracker`)

### Steps

```mermaid
flowchart LR
    A(["🌐 ai.azure.com"]) --> B["Click Projects\n→ New Project"]
    B --> C["Enter Project Name\n(e.g. gift-tracker)"]
    C --> D["Link Azure\nResource Group"]
    D --> E["Select Region\n(e.g. East US 2)"]
    E --> F["Create Project"]
    F --> G(["✅ Project Created\nwith auto-deployed models"])

    style G fill:#059669,color:#fff
    style D fill:#1e40af,color:#fff
```

### What Gets Auto-Deployed in a New Project

When you create a new project, Microsoft Foundry **automatically deploys**:

| Resource | Purpose |
|---|---|
| **GPT-4.1** | Default LLM for your agents |
| **text-embedding-3-small** | Embedding model for vector search / file search |
| **Azure AI Search** (Microsoft-managed) | Powers file search tool for agents |

> **Note:** These auto-deployed resources are **Microsoft-managed** — you don't pay for the compute separately; they are included through the project's resource allocation. For production or large-scale use, create your own dedicated resources.

---

## 4. The Gift Tracker Agent — Concept

The demo builds a **Gift Tracker Agent** — a personal AI assistant that helps a user:

```mermaid
mindmap
  root(("🎁 Gift Tracker\nAgent"))
    Track Past Gifts
      What was given last year
      Avoid repeating gifts
      Per-family-member history
    Budget Planning
      Calculate total spend
      Set per-person budgets
      Math via code interpreter
    Gift Ideas
      Personalized to each person
      Based on interests and dislikes
      Web search for current ideas
    Memory
      Remember past conversations
      Store preferences
      Cross-session continuity
    Family Profiles
      Children, spouse, etc.
      Individual interests
      Upcoming birthdays/holidays
```

### Use Case Summary

> *"Imagine you want to plan gifts for your children ahead of time — you don't want to repeat gifts, you want to stay on budget, find things they'll love, and not be surprised when holidays arrive."*

---

## 5. Agent Tools Overview

An Azure AI Agent in Foundry can be equipped with **four types of tools**:

```mermaid
flowchart TD
    AGENT["🤖 Azure AI Agent\n(GPT-4.1 core)"]

    FS["📂 File Search\n(RAG over uploaded files)\nUses: Azure AI Search\n(Microsoft-managed)"]
    WS["🌐 Web Search\n(Real-time internet queries)\nUses: Bing Search API"]
    CI["🐍 Code Interpreter\n(Python execution sandbox)\nUses: Sandboxed container"]
    MEM["🧠 Memory\n(Cross-session recall)\nUses: Memory store\n(Auto-created vector store)"]

    AGENT --> FS
    AGENT --> WS
    AGENT --> CI
    AGENT --> MEM

    style AGENT fill:#0f172a,color:#fff
    style FS fill:#1e40af,color:#fff
    style WS fill:#059669,color:#fff
    style CI fill:#7c3aed,color:#fff
    style MEM fill:#dc2626,color:#fff
```

| Tool | What It Does | Best For |
|---|---|---|
| **File Search** | RAG over documents you upload (JSON, PDF, DOCX, TXT) | Private knowledge base |
| **Web Search** | Real-time Bing web search | Current events, live data, product research |
| **Code Interpreter** | Executes Python in a sandbox | Math, data analysis, charting, computation |
| **Memory** | Stores chat summaries for cross-session recall | Personalization, conversation continuity |

---

## 6. Building the Agent — Step by Step

### Step 1 — System Prompt (Instructions)

The **system prompt** defines the agent's personality, scope, and behavior rules.

```mermaid
flowchart LR
    Foundry["Foundry\nBuild Tab"] --> NewAgent["Create New Agent"]
    NewAgent --> Name["Set Agent Name\n(e.g. 'Gift Tracker Agent')"]
    Name --> Model["Select Model\n(GPT-4.1 default)"]
    Model --> Prompt["Add System Instructions\n(Paste system prompt)"]
    Prompt --> Save["💾 Save Agent\n(creates a version)"]

    style Prompt fill:#7c3aed,color:#fff
    style Save fill:#059669,color:#fff
```

**Example System Prompt for the Gift Tracker Agent:**
```
You are a specialized Gift Tracker assistant. Your purpose is to help users:
- Track gifts given to family members across all occasions (birthdays, Christmas, etc.)
- Plan and budget future gifts based on the family member's profile and preferences
- Suggest personalized gift ideas using their interests and past gift history
- Remember past conversations and user preferences
- Use web search to find current, up-to-date gift ideas

Only answer questions related to gift tracking, planning, and suggestions.
Always cite the source of information (file search, web search, or memory).
```

> **Best Practice:** Save the agent frequently — Foundry keeps **version history**, letting you roll back to any previous version if you make a mistake.

---

### Step 2 — File Search (Knowledge)

**File Search** is the simplest way to add a **private knowledge base** to your agent. It uses a Microsoft-managed Azure AI Search instance behind the scenes.

```mermaid
flowchart LR
    Files["📁 JSON / PDF / DOCX files\n(Family profiles, gift history)"]
    Upload["Drag & Drop Upload\nin Foundry Build tab"]
    MgdAIS["Microsoft-Managed\nAzure AI Search\n(auto-created)"]
    Embed["text-embedding-3-small\n(auto-vectorized)"]
    FileIdx["File Search Index"]
    Agent2["🤖 Agent uses File Search\nduring conversations"]

    Files --> Upload
    Upload --> Embed
    Embed --> MgdAIS
    MgdAIS --> FileIdx
    FileIdx --> Agent2

    style MgdAIS fill:#1e40af,color:#fff
    style Embed fill:#7c3aed,color:#fff
    style Agent2 fill:#059669,color:#fff
```

#### Files Used in the Demo
The demo uploads **JSON files** containing:
- **Family member profiles** (name, age, interests, dislikes)
- **Past gift history** (what was given on which occasion, year, cost)
- **Upcoming events** (birthday dates, holiday schedules)

#### When to Use File Search vs Azure AI Search

| Situation | Use File Search | Use Azure AI Search |
|---|---|---|
| Quick demo / prototype | ✅ | ❌ |
| Small document set (< few hundred files) | ✅ | ❌ |
| SQL Database, Cosmos DB, or Blob Storage source | ❌ | ✅ |
| Large enterprise knowledge base | ❌ | ✅ |
| Need custom indexing, filters, or hybrid search | ❌ | ✅ |

> **Quote from video:** *"File Search is quick and easy to show how you can upload information and then retrieve it quickly — we're using a Microsoft-managed Azure AI Search resource. This is a very simple and fast way to set up RAG for your Azure AI agents."*

---

### Step 3 — Web Search Tool

**Web Search** gives the agent access to **real-time Bing search** results — enabling it to find current product listings, prices, news, and more that aren't in the uploaded files.

```mermaid
sequenceDiagram
    participant User
    participant Agent as Gift Tracker Agent
    participant Files as File Search
    participant Web as Bing Web Search

    User->>Agent: "Find 2 more gift ideas for Adam based on his profile"
    Agent->>Files: Retrieve Adam's interests and dislikes
    Files-->>Agent: Adam: likes tech, dislikes sports
    Agent->>Web: Search "tech gifts for teens 2025 under $50"
    Web-->>Agent: Top web results with prices
    Agent-->>User: Here are 2 more ideas:\n1. Raspberry Pi kit — $35\n2. LED desk lamp — $29
```

**Behaviour observed in demo:**
- Agent automatically retrieved the profile from file search first
- Then used Adam's interests/dislikes as search query context for Bing
- Results included **real-time pricing** from web

---

### Step 4 — Code Interpreter

**Code Interpreter** runs a Python sandbox allowing the agent to perform **accurate mathematical calculations**, data analysis, and show its work.

```mermaid
flowchart LR
    UserQ["User: 'Do math on all gifts\ngiven to Adam last year\nusing Code Interpreter'"]
    CI2["🐍 Code Interpreter\n(Python sandbox)"]
    Data["Data pulled from\nFile Search\n(birthday + Christmas gifts)"]
    Code["Generated Python:\ngifts = [35, 29, 45, 80, 20]\nprint(f'Total: ${sum(gifts)}')"]
    Result["Result: Total = $209\n(showing work step by step)"]

    UserQ --> CI2
    CI2 --> Data
    Data --> Code
    Code --> Result

    style CI2 fill:#7c3aed,color:#fff
    style Result fill:#059669,color:#fff
```

**Key capability:** Code Interpreter ensures **mathematical accuracy** — rather than having the LLM estimate totals in its response, it actually executes Python code and returns the verified result.

**Demo example query:**
```
"Do math on all of the gifts that were given to Adam last year and add them all up 
using code interpreter. Include both birthday and Christmas gifts."
```

---

### Step 5 — Memory Store

**Memory** enables the agent to **remember past conversations** across separate sessions — unlike standard chat which has no memory between conversations.

```mermaid
flowchart TB
    Conv1["💬 Conversation 1\nUser discusses gift ideas for Adam's birthday\nAgent suggests 5 ideas"]
    Summary1["📝 Chat Summary Stored\nin Memory Store\n(vector store)"]

    Conv2["💬 Conversation 2 (new session)\nUser: 'Which child were we\ndiscussing gifts for?'"]
    MemSearch["🔍 Memory Search\n(semantic search over stored summaries)"]
    Recall["✅ Agent Recalls:\n'You were discussing Adam's birthday.\nHere were the ideas...'"]

    Conv1 --> Summary1
    Conv2 --> MemSearch
    Summary1 --> MemSearch
    MemSearch --> Recall

    style Summary1 fill:#1e40af,color:#fff
    style MemSearch fill:#7c3aed,color:#fff
    style Recall fill:#059669,color:#fff
```

#### How to Create a Memory Store

1. In the Foundry Build tab → scroll to **Memory** section
2. Click **Add Memory → Create Memory Store**
3. A default memory store is auto-created (backed by a vector store)
4. Click **Save Agent**

> **What gets stored:** At the end of each conversation, a **summary of the chat** is automatically written to the memory store. When the agent starts a new conversation, it semantically searches past summaries to recall relevant context.

---

## 7. Testing the Agent

### Test Queries Used in Demo

| Query | Tool Invoked | Result |
|---|---|---|
| `"What can you do?"` | None | Describes its capabilities |
| `"Give me gift ideas for Adam for his birthday"` | File Search + Memory | Pulled Adam's profile from files, searched memory for past discussions |
| `"Search online for 2 more ideas based on Adam's preferences"` | Web Search + File Search | Combined profile data with live Bing results |
| `"Which child were we discussing?"` *(new session)* | Memory | Recalled "Adam" from previous session summary |
| `"Do math on all Adam's gifts last year using code interpreter"` | Code Interpreter + File Search | Pulled gift data from files, ran Python, showed calculation |

### Multi-Tool Behaviour

```mermaid
sequenceDiagram
    participant User
    participant Agent as Gift Tracker Agent
    participant FS2 as File Search
    participant Mem as Memory Store
    participant Web2 as Web Search
    participant CI3 as Code Interpreter

    Note over Agent: Each query may invoke multiple tools automatically

    User->>Agent: "Gift ideas for Adam's birthday"
    Agent->>FS2: Retrieve Adam's profile + gift history
    Agent->>Mem: Search past conversations about Adam
    FS2-->>Agent: Interests, past gifts
    Mem-->>Agent: Previous discussion context
    Agent-->>User: Personalised gift suggestions

    User->>Agent: "Find 2 more ideas online"
    Agent->>FS2: Re-fetch Adam's interests
    Agent->>Web2: Bing search with profile context
    Web2-->>Agent: Current products + prices
    Agent-->>User: 2 new ideas with real prices

    User->>Agent: "Calculate total gifts last year"
    Agent->>FS2: Fetch all gift records
    Agent->>CI3: Execute Python sum calculation
    CI3-->>Agent: Verified total
    Agent-->>User: Total = $209 (showing Python work)
```

---

## 8. Operate Tab — Monitoring

The **Operate tab** in Foundry is where you monitor and manage your deployed agents.

```mermaid
flowchart LR
    Op["📊 Operate Tab"] --> AL["Agent List\n(All agents in project)"]
    Op --> Metrics2["Performance Metrics\n(per agent)"]
    Op --> Register2["Register External Agent\n(containerized / hosted elsewhere)"]

    AL --> Filter["Filter by:\n- Project\n- All projects\n- Agent status"]
    Metrics2 --> Info["View:\n- Request counts\n- Latency\n- Tool usage"]

    style Op fill:#7c3aed,color:#fff
    style Register2 fill:#1e40af,color:#fff
```

### Key Features

| Feature | Description |
|---|---|
| **Agent Dashboard** | See all running agents, their status, and usage metrics |
| **Per-Project View** | Filter metrics to a single project or view all projects |
| **Register Agent** | Add an existing externally-hosted agent to the Foundry dashboard |
| **Containerize** | Wrap an existing agent in a container and register it for unified monitoring |

> **Use case for Register Agent:** If you already have an agent running on another platform or backend, you can register it in Foundry to see its performance on the same Foundry dashboard — without migrating the agent itself.

---

## 9. Docs Tab & Built-in AI Chatbot

The **Docs tab** provides documentation and a **built-in AI chatbot** that answers questions about Foundry itself.

```mermaid
flowchart LR
    User2["Developer"] --> DocTab["📖 Docs Tab"]
    DocTab --> Static["Static Documentation\n(Foundry guides)"]
    DocTab --> Chatbot["🤖 Built-in AI Chatbot\n(Ask questions about Foundry)"]

    Chatbot --> Q1["'What models are\navailable in my region?'"]
    Chatbot --> Q2["'How do I set up\nagentic RAG?'"]
    Chatbot --> Q3["'What is the difference\nbetween file search\nand Azure AI Search?'"]

    Q1 & Q2 & Q3 --> Ans["Real-time answers\nbased on Foundry docs"]

    style Chatbot fill:#059669,color:#fff
    style Ans fill:#0f172a,color:#fff
```

> **Practical tip:** If you're unsure where to go or what to do in Foundry, the built-in chatbot is a fast way to get contextual help — it searches the actual Foundry documentation to answer your questions.

---

## 10. Deployment Options

Once your agent is working in Foundry, you can deploy it through multiple channels:

```mermaid
flowchart TB
    Agent3["✅ Completed Agent\n(Gift Tracker)"]

    Pub1["📋 Copy API Endpoint\n+ Code Sample\n(Python / JS / C#)"]
    Pub2["🏢 Microsoft 365 Copilot\n(Publish as M365\nCopilot agent)"]
    Pub3["💬 Microsoft Teams\n(Publish as Teams bot)"]
    Pub4["🔗 Backend API\n(Integrate into\nyour own UI/app)"]
    Pub5["☁️ Azure App Service /\nAzure Container Apps\n(Host agent as API)"]

    Agent3 --> Pub1
    Agent3 --> Pub2
    Agent3 --> Pub3
    Agent3 --> Pub4
    Agent3 --> Pub5

    style Agent3 fill:#0f172a,color:#fff
    style Pub2 fill:#059669,color:#fff
    style Pub3 fill:#1e40af,color:#fff
```

| Deployment Path | How | Best For |
|---|---|---|
| **M365 Copilot** | Foundry → Publish → M365 Copilot | Enterprise users already on Microsoft 365 |
| **Microsoft Teams** | Foundry → Publish → Teams | Internal team tool |
| **API Endpoint** | Use the auto-generated endpoint code | Custom web/mobile app |
| **Backend service** | Copy Python/JS/C# SDK code | Any custom backend |

---

## 11. Architecture Diagram

### Complete Gift Tracker Agent Architecture

```mermaid
flowchart TB
    User3["👤 User\n(Gift Planner)"]

    subgraph Foundry2["🏗️ Microsoft Foundry (ai.azure.com)"]
        Agent4["🤖 Gift Tracker Agent\n(GPT-4.1)"]
        SysPrompt["📋 System Instructions\n(Scope + behavior rules)"]
        Versioning["🔄 Version History"]
    end

    subgraph Tools["🛠️ Agent Tools"]
        FS3["📂 File Search\n(Family profiles, gift history\nin JSON files)"]
        WS3["🌐 Web Search\n(Bing — live gift ideas)"]
        CI4["🐍 Code Interpreter\n(Budget math, calculations)"]
        Mem3["🧠 Memory Store\n(Cross-session recall via\nchat summaries)"]
    end

    subgraph Azure["☁️ Azure Backend"]
        AIS3["Azure AI Search\n(Microsoft-managed)\n(Powers File Search)"]
        Embed2["text-embedding-3-small\n(Auto-vectorization)"]
        GPT4["GPT-4.1\n(Inference)"]
    end

    subgraph Deploy["🚀 Deployment"]
        M365["M365 Copilot"]
        Teams2["Microsoft Teams"]
        API2["API Endpoint\n(Python/JS/C#)"]
    end

    User3 <--> Agent4
    Agent4 --> SysPrompt
    Agent4 --> FS3
    Agent4 --> WS3
    Agent4 --> CI4
    Agent4 --> Mem3
    FS3 --> AIS3
    AIS3 --> Embed2
    Agent4 --> GPT4
    Agent4 --> Deploy

    style Agent4 fill:#0f172a,color:#fff
    style AIS3 fill:#1e40af,color:#fff
    style Embed2 fill:#7c3aed,color:#fff
    style GPT4 fill:#059669,color:#fff
    style Deploy fill:#374151,color:#fff
```

---

## 12. Interview Quick Reference

### Core Concepts from This Video

**Q: What is Microsoft Foundry and how does it differ from Azure AI Studio?**
> Microsoft Foundry (ai.azure.com) is the evolution of Azure AI Studio — the unified enterprise portal for building, testing, and monitoring Azure AI agents. It organises work into **projects**, each backed by an Azure Resource Group, and auto-provisions models like GPT-4.1 and `text-embedding-3-small` on project creation.

**Q: What is the difference between File Search and Azure AI Search for agents?**
> **File Search** is a simplified, Microsoft-managed RAG tool ideal for quickly uploading documents (PDF, JSON, DOCX) to give an agent a knowledge base. It uses a **Microsoft-managed Azure AI Search** resource behind the scenes. **Azure AI Search** (self-provisioned) should be used when your data lives in Blob Storage, SQL, or Cosmos DB; when you need large-scale indexing; or when you need custom hybrid search configurations.

**Q: How does Memory work in Azure AI Agents?**
> Memory stores **summaries of past conversations** in a vector store. When a new conversation starts, the agent semantically searches these summaries for relevant context. This enables **cross-session continuity** — the agent can remember which family member you discussed in a previous session without you repeating yourself.

**Q: What is Code Interpreter used for?**
> Code Interpreter runs a **Python sandbox** inside the agent's reasoning loop. It is used for any task requiring deterministic computation — budget calculations, data analysis, showing mathematical work step-by-step. This avoids LLM hallucination on math since the code is actually executed.

**Q: What are the four main tools available to an Azure AI Agent?**

| Tool | What It Does |
|---|---|
| **File Search** | RAG over uploaded documents via managed Azure AI Search |
| **Web Search** | Real-time Bing search for current information |
| **Code Interpreter** | Python execution sandbox for accurate computation |
| **Memory** | Cross-session vector-based recall of past conversations |

**Q: How do you deploy an Azure AI Agent built in Foundry?**
> From the Foundry Build tab, you can: (1) **Publish to M365 Copilot** as a Copilot agent, (2) **Publish to Microsoft Teams** as a bot, (3) **Copy the API endpoint** code (Python/JS/C#) for integration into any custom backend, or (4) **Register an external agent** in the Operate tab for unified monitoring.

**Q: What is the Operate tab for?**
> The Operate tab is the **monitoring dashboard** for your deployed agents. You can view performance metrics (request count, latency, tool usage), filter by project, and register externally-hosted agents so they appear in the same Foundry dashboard.

### Decision Guide — Which Tool to Add?

```mermaid
flowchart TD
    START(["🤖 Configure Your Agent"])

    NeedK{{"Agent needs\nprivate knowledge?"}}
    NeedRT{{"Need real-time\nor web data?"}}
    NeedMath{{"Agent needs to\ndo calculations?"}}
    NeedMem{{"Need to remember\npast conversations?"}}

    FS4["✅ Add File Search\n(or Azure AI Search for\nlarge-scale data)"]
    WS4["✅ Add Web Search\n(Bing API)"]
    CI5["✅ Add Code Interpreter\n(Python sandbox)"]
    Mem4["✅ Add Memory Store\n(vector summary store)"]

    START --> NeedK
    NeedK -->|Yes| FS4
    NeedK -->|No / Next| NeedRT
    NeedRT -->|Yes| WS4
    NeedRT -->|No / Next| NeedMath
    NeedMath -->|Yes| CI5
    NeedMath -->|No / Next| NeedMem
    NeedMem -->|Yes| Mem4

    style START fill:#0f172a,color:#fff
    style FS4 fill:#1e40af,color:#fff
    style WS4 fill:#059669,color:#fff
    style CI5 fill:#7c3aed,color:#fff
    style Mem4 fill:#dc2626,color:#fff
```

---

*Last updated: June 2026 | Microsoft Foundry Portal — ai.azure.com*
