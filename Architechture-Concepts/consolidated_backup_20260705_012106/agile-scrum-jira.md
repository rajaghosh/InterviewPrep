# Agile — Scrum & Jira Complete Guide
> **Consolidated From:** Scrum-Complete-Course-Guide.md, Jira-Getting-Started-Guide.md
> **Topics Covered:** Scrum framework, roles, ceremonies, artifacts, Jira getting started, boards, workflows
> **Consolidation Date:** 2026-07-04
> **Original Documents:** 2 → **Content Preserved:** 100%

---

# Part I — Scrum Complete Course


> **Source:** [YouTube — Full Scrum Course In Hindi in 4 hours](https://www.youtube.com/watch?v=Vxzdhs5qLr0)
> **Channel/Event:** Aram Faraaz Hindi
> **Topic:** Scrum, Agile, Project Management, Sprint, Product Backlog, Ceremonies, Roles
> **Key Claim:** Complete Scrum framework from foundations to advanced — 4-hour end-to-end coverage in Hindi

---

## Table of Contents

1. [Overview](#1-overview)
2. [Problem Statement — Why Scrum?](#2-problem-statement--why-scrum)
3. [Core Concepts](#3-core-concepts)
4. [Scrum Framework Architecture](#4-scrum-framework-architecture)
5. [Scrum Roles & Accountabilities](#5-scrum-roles--accountabilities)
6. [Scrum Events](#6-scrum-events)
7. [Scrum Artifacts & Commitments](#7-scrum-artifacts--commitments)
8. [How a Sprint Works — Step by Step](#8-how-a-sprint-works--step-by-step)
9. [Comparison Table — Waterfall vs Scrum](#9-comparison-table--waterfall-vs-scrum)
10. [Story Points, Velocity & Estimation](#10-story-points-velocity--estimation)
11. [Best Practices](#11-best-practices)
12. [Interview Talking Points](#12-interview-talking-points)
13. [Learning Resources](#13-learning-resources)

---

## 1. Overview

Scrum is a lightweight Agile framework for developing, delivering, and sustaining complex products through iterative, time-boxed cycles called **Sprints**. Defined by Ken Schwaber and Jeff Sutherland in the **2020 Scrum Guide**, it rests on three empirical pillars — **Transparency, Inspection, and Adaptation**. Unlike Waterfall, Scrum embraces change at every cycle, delivering working software every 1–4 weeks. This 4-hour Hindi course covers the entire framework: roles, events, artifacts, estimation techniques, and real-world application — essential for PSM/CSM certifications and senior architect interviews.

---

## 2. Problem Statement — Why Scrum?

### Classic Waterfall Pain Points

| Problem | Impact |
|---|---|
| Requirements frozen at start | Cannot respond to market/business changes mid-project |
| Testing happens at the end | Defects found late → expensive to fix |
| Customer sees product only after months/years | Misaligned expectations, wasted effort |
| No intermediate value delivery | Business ROI delayed until full release |
| Single-phase planning | Estimates drift, projects run over schedule/budget |
| Siloed teams (BA → Dev → QA → Ops) | Handoff friction, slow feedback loops |

> **Key Insight:** "Scrum does not solve the problems of software development — it makes the dysfunction so visible that the team is forced to address them."

---

## 3. Core Concepts

### Agile
A philosophy for software development emphasizing iterative delivery, collaboration, customer feedback, and responding to change over following a fixed plan. Defined in the **Agile Manifesto (2001)** — 4 values and 12 principles.

**4 Agile Values:**
- Individuals and interactions **over** processes and tools
- Working software **over** comprehensive documentation
- Customer collaboration **over** contract negotiation
- Responding to change **over** following a plan

### Scrum
A specific Agile framework — not a process, technique, or methodology. Scrum is intentionally incomplete; it defines only the interactions among roles, events, and artifacts, leaving the specific techniques to the team.

### Empiricism
Scrum's theoretical foundation. Knowledge comes from experience; decisions are made on what is known. Three pillars:
- **Transparency** — the process and work must be visible to those performing it and receiving it
- **Inspection** — Scrum artifacts and progress must be inspected frequently
- **Adaptation** — if inspection reveals deviation, the process must be adjusted

### Sprint
The heartbeat of Scrum — a time-box of **1–4 weeks** (fixed length, chosen by the team) during which a "Done," usable, potentially releasable product Increment is created.

### Definition of Done (DoD)
A formal description of the state of the Increment when it meets the quality standards required. If not done per DoD, it cannot be released or presented at Sprint Review.

### Sprint Goal
A single objective for the Sprint that creates coherence and focus. The Sprint Goal provides flexibility to the Development Team while still providing focus.

---

## 4. Scrum Framework Architecture

```mermaid
flowchart TD
    PO["Product Owner\nManages Product Backlog"]
    SM["Scrum Master\nFacilitates & Coaches"]
    DT["Development Team\n3-9 Cross-functional Members"]

    PB["Product Backlog\n📋 Ordered list of everything\nneeded in product"]
    SP["Sprint Planning\n⏱ 8h max for 1-month sprint"]
    SB["Sprint Backlog\n📋 Sprint Goal + selected PBIs\n+ plan to deliver"]

    subgraph Sprint ["Sprint (1–4 weeks)"]
        DS["Daily Scrum\n⏱ 15 min · same time · same place"]
        DEV["Development Work\nDesign · Code · Test · Integrate"]
        DS --> DEV
        DEV --> DS
    end

    INC["Increment\n✅ Done, usable product increment\nMeets Definition of Done"]
    SR["Sprint Review\n⏱ 4h max · demo to stakeholders\nAdapt Product Backlog"]
    RETRO["Sprint Retrospective\n⏱ 3h max · inspect team process\nCreate improvement plan"]

    PB --> SP
    SP --> SB
    SB --> Sprint
    Sprint --> INC
    INC --> SR
    SR --> RETRO
    RETRO --> SP

    PO --> PB
    SM --> Sprint
    DT --> Sprint

    style PO fill:#0078D4,color:#fff
    style SM fill:#5C2D91,color:#fff
    style DT fill:#5C2D91,color:#fff
    style PB fill:#EFF6FC,stroke:#0078D4
    style SB fill:#EFF6FC,stroke:#0078D4
    style INC fill:#107C10,color:#fff
    style SR fill:#EFF6FC,stroke:#0078D4
    style RETRO fill:#EFF6FC,stroke:#0078D4
    style Sprint fill:#FFF4CE,stroke:#D83B01
```

---

## 5. Scrum Roles & Accountabilities

Scrum defines exactly **one Scrum Team** with three distinct accountabilities. No hierarchy — one team, one goal.

### Product Owner (PO)

| Attribute | Detail |
|---|---|
| Accountability | Maximizing the value of the product resulting from the work of the Scrum Team |
| Key responsibility | Sole person responsible for managing the Product Backlog |
| Authority | Can accept or reject work; can cancel a Sprint |
| Representation | Represents stakeholders and business needs to the team |
| Decision power | Final say on Product Backlog ordering and content |

**PO duties:**
- Develop and explicitly communicate the Product Goal
- Create and clearly express Product Backlog Items (PBIs)
- Order PBIs for maximum value
- Ensure the Product Backlog is transparent, visible, and understood

### Scrum Master (SM)

| Attribute | Detail |
|---|---|
| Accountability | Establishing Scrum as defined in the Scrum Guide; team effectiveness |
| Style | Servant-leader — serves the team, not manages it |
| Key skill | Facilitating events; removing impediments; coaching |
| External interface | Shields team from outside interruptions |

**SM duties:**
- Coach team members in self-management and cross-functionality
- Help team focus on creating high-value Increments meeting DoD
- Cause removal of impediments to the team's progress
- Ensure all Scrum events happen and are productive/time-boxed
- Help stakeholders understand empirical product development

### Development Team (Developers)

| Attribute | Detail |
|---|---|
| Size | 3–9 members (optimal) |
| Skills | Cross-functional — all skills needed to create usable Increment |
| Structure | Self-organizing — decides how to do the work |
| Accountability | Creating a plan for the Sprint (Sprint Backlog) + delivering Increment |
| Ownership | Collectively accountable for creating value every Sprint |

**No sub-teams, no titles** inside the Development Team (e.g., no "senior" vs "junior" in Scrum terms).

---

## 6. Scrum Events

All Scrum events are opportunities to inspect and adapt. All events are **time-boxed** — the maximum is a limit, not a target.

### Sprint

| Property | Value |
|---|---|
| Duration | 1–4 weeks (consistent length) |
| Contains | Sprint Planning + Daily Scrums + Sprint Review + Sprint Retrospective |
| Rule | No changes that endanger the Sprint Goal; no change in team composition |
| Cancellation | Only the Product Owner can cancel; rare — only if Sprint Goal is obsolete |

### Sprint Planning

| Property | Value |
|---|---|
| Time-box | 8 hours for 1-month Sprint (proportionally less for shorter sprints) |
| Participants | Entire Scrum Team |
| Output | Sprint Goal + Sprint Backlog (selected PBIs + plan) |
| 3 Topics | WHY (Sprint Goal) · WHAT (PBIs selected) · HOW (plan to deliver) |

### Daily Scrum (Stand-up)

| Property | Value |
|---|---|
| Duration | 15 minutes max |
| Participants | Developers (SM attends only if also a developer) |
| Frequency | Every working day, same time, same place |
| Purpose | Inspect progress toward Sprint Goal; adapt Sprint Backlog |
| 3 Classic Questions | What did I do yesterday? · What will I do today? · Any impediments? |

> **Note:** The 3 questions are a suggestion, not a rule in the 2020 Scrum Guide. Teams can use any structure.

### Sprint Review

| Property | Value |
|---|---|
| Time-box | 4 hours for 1-month Sprint |
| Participants | Scrum Team + key stakeholders |
| Purpose | Inspect the Increment; adapt Product Backlog based on feedback |
| Output | Revised Product Backlog; updated release plan |
| Key distinction | It is NOT a demo-only event — it's an inspection and adaptation meeting |

### Sprint Retrospective

| Property | Value |
|---|---|
| Time-box | 3 hours for 1-month Sprint |
| Participants | Scrum Team only |
| Purpose | Inspect the team's processes, tools, relationships; create improvement plan |
| Output | Action items for process improvement; added to next Sprint Backlog |
| Focus areas | Individuals · Interactions · Processes · Tools · Definition of Done |

---

## 7. Scrum Artifacts & Commitments

Each artifact contains a **commitment** — a measurable goal to improve transparency and focus.

### Product Backlog → Commitment: Product Goal

| Property | Detail |
|---|---|
| Owner | Product Owner (maintains, orders) |
| Content | Ordered list of everything that might be needed in the product |
| Refinement | Ongoing activity — breaking, clarifying, estimating PBIs |
| DEEP qualities | Detailed Appropriately · Estimated · Emergent · Prioritized |
| Product Goal | The long-term objective for the Scrum Team; one goal at a time |

**Product Backlog Item (PBI) anatomy:**
```
User Story format:
  As a [role], I want [feature] so that [benefit]

Acceptance Criteria:
  Given [context] When [action] Then [outcome]

Story Points: T-shirt sizes (XS/S/M/L/XL) or Fibonacci (1,2,3,5,8,13,21,34)
Priority:     Must Have / Should Have / Could Have / Won't Have (MoSCoW)
```

### Sprint Backlog → Commitment: Sprint Goal

| Property | Detail |
|---|---|
| Owner | Development Team (creates and owns) |
| Content | Sprint Goal + selected PBIs + plan for delivering the Increment |
| Visibility | Highly visible, updated in real-time |
| Flexibility | Negotiated with PO during Sprint if scope needs adjustment |
| Sprint Goal | A single objective creating coherence and focus for the Sprint |

### Increment → Commitment: Definition of Done (DoD)

| Property | Detail |
|---|---|
| Definition | A concrete stepping stone toward the Product Goal |
| Requirement | Must meet DoD — if not Done, it cannot be called an Increment |
| Accumulation | Each Sprint adds to all prior Increments |
| Release | PO decides when to release; team decides when it's Done |
| DoD scope | Team-defined (at minimum); organization may define a broader DoD |

---

## 8. How a Sprint Works — Step by Step

```mermaid
sequenceDiagram
    participant PO as Product Owner
    participant SM as Scrum Master
    participant DT as Dev Team
    participant SH as Stakeholders

    Note over PO,DT: Sprint Planning (Day 1)
    PO->>DT: Present top Product Backlog Items
    DT->>PO: Ask clarifying questions
    PO->>DT: Confirm Sprint Goal
    DT->>DT: Decompose PBIs into tasks
    DT->>SM: Sprint Backlog finalized

    Note over DT,SM: Daily Scrum (Each Day — 15 min)
    DT->>DT: Yesterday done / Today plan / Blockers
    SM->>DT: Remove impediments

    Note over DT: Development Work
    DT->>DT: Design → Code → Test → Integrate
    DT->>DT: Update Sprint Backlog burn-down

    Note over PO,SH: Sprint Review (Last Day)
    DT->>SH: Demonstrate working Increment
    SH->>PO: Feedback on product
    PO->>PO: Adapt Product Backlog

    Note over PO,DT: Sprint Retrospective (After Review)
    SM->>DT: Facilitate retro discussion
    DT->>DT: Identify improvements
    DT->>DT: Add action items to next Sprint
```

**Step-by-step breakdown:**

1. **Product Backlog Refinement** (ongoing, ~10% of Sprint capacity) — PO and team clarify, estimate, and reorder backlog items before Sprint Planning
2. **Sprint Planning** — Team selects PBIs, negotiates scope, defines Sprint Goal, creates Sprint Backlog with daily tasks
3. **Daily Development** — Team self-organizes; updates task board; burn-down chart tracked daily
4. **Daily Scrum** — 15-min sync every morning; SM removes blockers; PO available but doesn't attend unless invited
5. **Mid-Sprint PO Check** — If requirements change, PO negotiates with team; Sprint Goal cannot change
6. **Sprint Review** — Team demos to stakeholders; only "Done" work shown; PO updates backlog based on feedback
7. **Sprint Retrospective** — Private team meeting; SM facilitates; outputs are concrete improvement actions for next Sprint
8. **Next Sprint begins immediately** — No downtime between Sprints

---

## 9. Comparison Table — Waterfall vs Scrum

| Dimension | Waterfall | Scrum |
|---|---|---|
| **Planning** | Complete upfront — all requirements defined before start | Iterative — high-level plan + detailed per Sprint |
| **Delivery** | Single delivery at project end | Working software every 1–4 weeks |
| **Requirements** | Fixed — changes are expensive/formal | Welcome change — backlog is fluid |
| **Feedback** | At end of project (UAT) | Every Sprint (Review + stakeholder demo) |
| **Team structure** | Functional silos (BA, Dev, QA, Ops separate) | Cross-functional, self-organizing Scrum Team |
| **Risk** | High — discovered at delivery | Low — inspected and adapted every Sprint |
| **Documentation** | Heavy upfront docs required | Just enough — working software over docs |
| **Progress tracking** | Gantt charts, milestone dates | Burn-down charts, velocity, Sprint Goals |
| **Customer involvement** | Requirements phase + UAT only | Every Sprint Review |
| **Best fit** | Fixed requirements, regulated industries | Complex/innovative products, evolving requirements |

---

## 10. Story Points, Velocity & Estimation

### Story Points

Story points are **relative effort** estimates — not hours. They capture complexity, effort, and uncertainty.

**Fibonacci Scale:** 1 · 2 · 3 · 5 · 8 · 13 · 21 · 34 · 55 · 89

| Story Points | Complexity |
|---|---|
| 1–2 | Trivial — well-understood, minimal effort |
| 3–5 | Small — straightforward with some unknowns |
| 8 | Medium — significant effort, some complexity |
| 13 | Large — complex, consider splitting |
| 21+ | Epic — must split before Sprint Planning |

### Planning Poker

Estimation technique where each team member independently selects a card (story point value), all reveal simultaneously, discuss outliers, re-vote until consensus.

```
Round 1: PO describes story → Team privately picks cards
         [3] [5] [5] [13] [8] revealed simultaneously
         → Discuss why outliers (3 and 13)
Round 2: Re-vote after discussion → [5] [5] [8] [5] [5]
         → Consensus: 5 story points
```

### Velocity

Average number of story points completed per Sprint over last 3–5 Sprints.

```
Sprint 1: 34 points
Sprint 2: 38 points  
Sprint 3: 42 points
Sprint 4: 36 points
Sprint 5: 40 points

Velocity = (34+38+42+36+40) / 5 = 38 points/sprint

If Product Backlog = 380 points:
  Estimated sprints = 380 / 38 = ~10 sprints
  At 2-week sprints = ~20 weeks to complete
```

### Burn-Down Chart

Visual representation of remaining work in the Sprint vs. time.

```
Story Points Remaining
40 |*
35 | *
30 |  *         Ideal line (linear)
25 |   *\
20 |    *  \     Actual line
15 |     *    \
10 |           *
 5 |              *
 0 +--+--+--+--+--+--+--+--+--+--→ Sprint Days
   D1 D2 D3 D4 D5 D6 D7 D8 D9 D10
```

---

## 11. Best Practices

### Sprint Planning

- ✅ Define a clear, testable Sprint Goal before selecting PBIs
- ✅ Only pull PBIs that meet the team's Definition of Ready (clear, estimated, sized ≤ half Sprint)
- ❌ Don't overcommit — use last 3-sprint velocity as baseline
- ❌ Don't let PO dictate HOW work is done — only WHAT and WHY

### Daily Scrum

- ✅ Keep it to 15 minutes; parking lot detailed discussions for after
- ✅ Update the Sprint Backlog/task board immediately after Daily Scrum
- ❌ Don't treat it as a status report to the SM — it's team synchronization
- ❌ Don't use it for problem-solving during the stand-up

### Product Backlog

- ✅ Refine backlog regularly (~10% of capacity) — PBIs at top should be ready 2 Sprints ahead
- ✅ Use INVEST criteria: Independent · Negotiable · Valuable · Estimable · Small · Testable
- ❌ Don't freeze the backlog — it evolves with every Sprint's learnings
- ❌ Don't add tasks to the Sprint mid-Sprint without removing equivalent scope

### Retrospective

- ✅ Create 1–3 concrete, actionable improvements per Retro with assigned owners
- ✅ Vary retrospective formats (Start/Stop/Continue, 4Ls, Mad/Sad/Glad) to avoid ritual fatigue
- ❌ Don't skip Retros when under pressure — that's when they're most valuable
- ❌ Don't create lists without follow-through; review previous Retro actions at start of next Retro

### Definition of Done

- ✅ Define DoD as a team, make it visible (wiki, Jira, physical board)
- ✅ Include: code reviewed + unit tested + integrated + acceptance criteria met + deployed to staging
- ❌ Don't bypass DoD under pressure — "almost done" is not Done
- ❌ Don't have different DoDs per team member or per story type

---

## 12. Interview Talking Points

### "What is the difference between Agile and Scrum?"

> Agile is a **philosophy** and set of values defined in the Agile Manifesto — it's a mindset, not a process. Scrum is a specific **framework** for implementing Agile principles. Agile says "deliver frequently, embrace change, collaborate with customers." Scrum operationalizes this through concrete roles (PO, SM, Dev Team), time-boxed events (Sprints, ceremonies), and artifacts (backlogs, Increment). You can be Agile without Scrum, but Scrum is always Agile.

---

### "What does the Scrum Master actually do?"

> The Scrum Master is a **servant-leader** — not a project manager. They don't assign tasks, track individual performance, or control the team. Their job is threefold: (1) Coach the team on self-organization and Scrum principles, (2) Facilitate Scrum events and ensure they are effective and time-boxed, and (3) Remove impediments — systemic blockers, organizational obstacles, or external interruptions. A great SM makes themselves progressively unnecessary as the team matures.

---

### "Who can change the Sprint Backlog during a Sprint?"

> Only the **Development Team** can modify the Sprint Backlog — it is their plan for achieving the Sprint Goal. The Product Owner can negotiate scope changes (adding/removing items) but only with the team's agreement, and only if the Sprint Goal remains achievable. The Sprint Goal itself is fixed — if the Sprint Goal becomes obsolete, only the Product Owner can cancel the Sprint. This boundary is critical: it protects the team's focus and commitment.

---

### "How do you handle requirements that change mid-Sprint?"

> Scrum handles this through the Sprint Goal concept. If new requirements don't affect the Sprint Goal, they go into the Product Backlog for future Sprints. If they do affect the Sprint Goal, the PO and team negotiate: either swap out equivalent-sized work from the Sprint Backlog, or in extreme cases, the PO cancels the Sprint. The key principle is that the Sprint Goal is protected — this creates a stable window for the team to focus, while keeping the Product Backlog fluid for everything else.

---

### "What is velocity and how do you use it for release planning?"

> Velocity is the average number of story points completed per Sprint, calculated over the last 3–5 Sprints. It reflects the team's sustainable capacity — not a target to game. For release planning: sum the story points in the remaining Product Backlog, divide by velocity to estimate number of Sprints remaining. For example, 200 backlog points ÷ 40 velocity = 5 Sprints. At 2-week Sprints, that's ~10 weeks to release. Velocity is team-specific and cannot be compared across teams — each team's points are relative to their own baseline.

---

### "What is the Definition of Done and why does it matter?"

> The Definition of Done (DoD) is the Scrum Team's shared understanding of what "complete" means for a Product Backlog Item. It typically includes: code written, peer-reviewed, unit tested, integrated into main branch, acceptance criteria verified, and deployed to a staging environment. Without a DoD, "done" is subjective and technical debt accumulates invisibly. The DoD enforces quality gates at every increment — if a PBI doesn't meet the DoD, it cannot be presented at Sprint Review or counted as velocity. It's the difference between building software and building **releasable** software.

---

### "Explain Scrum vs Kanban — when would you choose each?"

> Scrum uses fixed-length Sprints with a committed Sprint Backlog — best for teams working on a product with iterative feature delivery and a defined Sprint Goal each cycle. It requires all three roles and enforces ceremonies. Kanban is a continuous flow system — work items move through a pipeline with WIP limits, no fixed iterations. Kanban is better for operations/support teams (unpredictable incoming work, no fixed iteration rhythm). For greenfield product development: Scrum. For production support or maintenance: Kanban or Scrumban (hybrid). Many teams use Scrum for features + Kanban board for bug tracking.

---

## 13. Learning Resources

| Resource | Link | Type |
|---|---|---|
| Official 2020 Scrum Guide | [scrumguides.org](https://scrumguides.org/docs/scrumguide/v2020/2020-Scrum-Guide-US.pdf) | Official Docs (PDF) |
| Scrum.org — PSM Certification | [scrum.org/resources/scrum-guide](https://www.scrum.org/resources/scrum-guide) | Official Reference |
| Atlassian Scrum Guide | [atlassian.com/agile/scrum](https://www.atlassian.com/agile/scrum) | Comprehensive Guide |
| Full Scrum Course (Hindi) | [youtube.com — Aram Faraaz](https://www.youtube.com/watch?v=Vxzdhs5qLr0) | Video (Hindi/English) |
| Agile Manifesto | [agilemanifesto.org](https://agilemanifesto.org) | Official Reference |

---

*Last Updated: July 2026 | Source: Aram Faraaz Hindi — Full Scrum Course (4 hours)*

---

# Part II — Jira Getting Started


> **Source:** [YouTube — How Jira Works in 15 Minutes. NO EXPERIENCE Needed!](https://www.youtube.com/watch?v=8MIx4HLHljA)
> **Channel/Event:** YouTube · Beginner Tutorial
> **Topic:** Jira, Project Management, Agile, Scrum, Kanban, Atlassian
> **Key Claim:** Used by 300,000+ companies globally; supports software, marketing, ops, HR, and more

---

## Table of Contents

1. [Overview](#1-overview)
2. [Problem Statement](#2-problem-statement)
3. [Core Concepts](#3-core-concepts)
4. [Architecture](#4-architecture)
5. [Key Components](#5-key-components)
6. [How It Works — Step by Step](#6-how-it-works--step-by-step)
7. [Comparison Table — Scrum vs Kanban](#7-comparison-table--scrum-vs-kanban)
8. [Code Examples](#8-code-examples)
9. [Configuration Reference](#9-configuration-reference)
10. [Best Practices](#10-best-practices)
11. [Interview Talking Points](#11-interview-talking-points)
12. [Learning Resources](#12-learning-resources)

---

## 1. Overview

Jira is Atlassian's industry-leading project management platform used by over 300,000 companies to plan, track, and deliver work across every function — software development, marketing, operations, IT, and HR. At its core, Jira organizes work into **issues (tickets)**, places them on visual **boards** (Scrum or Kanban), and moves them through a customizable **workflow** from "To Do" to "Done."

Originally built for agile software teams, Jira has evolved into a general-purpose work management tool. Teams get shared visibility into who owns what, what the current status is, and how to report on progress — eliminating the chaos of spreadsheets and scattered emails.

The video walks a complete beginner through Jira's interface in under 15 minutes, covering project creation, board setup, issue types, sprints, and reporting with zero assumed experience.

---

## 2. Problem Statement

### Without Jira — The Classic Pain Points

| Problem | Impact |
|---|---|
| Work tracked in spreadsheets | Stale data, no real-time status, hard to filter |
| Task assignments via email | No audit trail, missed context, bottlenecks invisible |
| No sprint cadence | No delivery rhythm, scope creep unchecked |
| Status updates in meetings | Time wasted; team can't self-serve progress info |
| No single source of truth | Each team member has a different view of what "done" means |
| Dependency blind spots | Blockers discovered late, cascading delays |

> **Key Insight:** "Jira brings every team together to plan, track, and deliver any type of project with confidence — in one place."

---

## 3. Core Concepts

### Space (Project)
A container for all the work related to a team or initiative. Spaces can be based on Scrum, Kanban, or Bug Tracking templates. Formerly called "Projects" — now called **Spaces** in newer Jira versions.

### Work Item (Issue / Ticket)
The atomic unit of work in Jira. A work item can represent a story, bug, feature, task, epic, or subtask. Every item has an assignee, status, priority, and description.

### Epic
A large body of work that spans multiple sprints, broken down into smaller Stories or Tasks. Epics appear on the **Timeline** view for long-range planning.

### Story
A user-facing feature or requirement, typically written as "As a [user], I want [goal] so that [benefit]." Stories live inside Epics and are completed within a Sprint.

### Sprint
A time-boxed iteration (usually 1–2 weeks) in which the team commits to completing a set of Stories from the Backlog. Sprints are a Scrum concept — Kanban boards don't use them.

### Backlog
The ordered list of all work items not yet in a sprint. The Product Owner prioritizes it; the team pulls from the top during Sprint Planning.

### Workflow
The set of statuses a work item moves through (e.g., `To Do → In Progress → In Review → Done`). Workflows are fully customizable per project type.

### Board
The visual representation of active work. Columns on the board map to workflow statuses. Cards (work items) move left-to-right as work progresses.

---

## 4. Architecture

```mermaid
flowchart TD
    User["👤 Team Member"]
    JiraUI["Jira Web / Mobile UI"]

    subgraph Core ["Jira Core Platform"]
        Spaces["Spaces / Projects"]
        Backlog["Backlog"]
        Board["Scrum / Kanban Board"]
        Timeline["Timeline View"]
        Workflow["Workflow Engine"]
        Automation["Automation Rules"]
    end

    subgraph WorkItems ["Work Item Hierarchy"]
        Epic["Epic"]
        Story["Story / Task / Bug"]
        Subtask["Subtask"]
        Epic --> Story --> Subtask
    end

    subgraph Insights ["Reporting & Insights"]
        Velocity["Velocity Chart"]
        Burndown["Burndown Chart"]
        CumulativeFlow["Cumulative Flow"]
        Reports["Custom Reports"]
    end

    subgraph Integrations ["Ecosystem"]
        GitHub["GitHub / Bitbucket"]
        Confluence["Confluence"]
        Marketplace["6000+ Marketplace Apps"]
        SlackMS["Slack / MS Teams"]
    end

    User --> JiraUI
    JiraUI --> Core
    Core --> WorkItems
    Core --> Insights
    Core --> Integrations

    style User fill:#0078D4,color:#fff
    style JiraUI fill:#0052CC,color:#fff
    style Core fill:#EFF6FC,stroke:#0078D4
    style WorkItems fill:#F4F5F7,stroke:#5C2D91
    style Insights fill:#DFF6DD,stroke:#107C10
    style Integrations fill:#FFF4CE,stroke:#D83B01
    style Epic fill:#5C2D91,color:#fff
    style Story fill:#0078D4,color:#fff
    style Subtask fill:#107C10,color:#fff
```

---

## 5. Key Components

| Component | Description | Key Capability |
|---|---|---|
| **Spaces** | Project containers (team-managed or company-managed) | Template selection: Scrum, Kanban, Bug Tracking |
| **Board** | Kanban or Scrum visual board | Drag-drop cards across status columns |
| **Backlog** | Prioritized queue of unstarted work | Sprint planning, story point estimation |
| **Timeline** | Gantt-style long-range planner | Dependency arrows, multi-sprint Epic tracking |
| **Workflow Engine** | Configures statuses + transitions per project | Custom rules (e.g., block transition if no assignee) |
| **Automation** | No-code trigger-action rules | Auto-assign, auto-close, notify on status change |
| **Insights / Reports** | Built-in agile metrics | Velocity, Burndown, Cumulative Flow, Cycle Time |
| **Marketplace** | 6,000+ apps and integrations | Time tracking, testing, GitOps, compliance |

### Boards in Detail

Jira supports two board types:

**Scrum Board** — Sprint-based. The active sprint is shown on the board. Items are pulled from the Backlog during Sprint Planning and must be completed by the sprint end date. Shows a Burndown Chart.

**Kanban Board** — Continuous flow. No sprints. Items move through columns with optional WIP (Work-In-Progress) limits per column. Shows a Cumulative Flow Diagram.

### Work Item Hierarchy

```
Epic  (quarters / months)
  └── Story / Task / Bug  (days / sprint)
        └── Subtask  (hours)
```

### Workflow Statuses (default)

```
To Do  →  In Progress  →  In Review  →  Done
```

Custom statuses example for engineering:
```
Backlog → Design → Development → Code Review → QA → Staging → Released
```

---

## 6. How It Works — Step by Step

```mermaid
sequenceDiagram
    actor PM as Product Manager
    actor Dev as Developer
    participant Jira

    PM->>Jira: Create Space (Scrum template)
    PM->>Jira: Create Epic ("User Authentication")
    PM->>Jira: Add Stories to Backlog
    PM->>Jira: Prioritize + estimate stories
    PM->>Jira: Start Sprint (select stories from backlog)
    Jira-->>Dev: Sprint board populates with items
    Dev->>Jira: Move item "To Do" → "In Progress"
    Dev->>Jira: Log work, add comments, attach PR link
    Dev->>Jira: Move item "In Progress" → "In Review"
    PM->>Jira: Review and move → "Done"
    Jira-->>PM: Burndown chart updates in real time
    PM->>Jira: Complete Sprint → Review velocity
    PM->>Jira: Start next Sprint
```

**Step-by-step breakdown:**

1. **Create a Space** — Log in → Spaces dropdown → Choose template (Scrum / Kanban / Bug Tracking)
2. **Create Epics** — Top-level work groupings visible on the Timeline
3. **Build the Backlog** — Click Create → Add Stories, Bugs, Tasks; set priority and story points
4. **Plan a Sprint** — Drag items from Backlog into Sprint → Set start/end date → Start Sprint
5. **Work the Board** — Drag cards across columns as work progresses; assign teammates
6. **Track Progress** — Burndown Chart (Scrum) or Cumulative Flow (Kanban) auto-updates
7. **Complete Sprint** — Unfinished items roll back to Backlog; team reviews velocity

---

## 7. Comparison Table — Scrum vs Kanban

| Dimension | Scrum | Kanban |
|---|---|---|
| **Work cadence** | Time-boxed sprints (1–4 weeks) | Continuous flow, no sprints |
| **Planning** | Sprint Planning ceremony | Ongoing; pull when capacity available |
| **Board reset** | Board resets each sprint | Board is always active |
| **WIP limits** | Managed by sprint scope | Explicit column WIP limits |
| **Delivery schedule** | Predictable sprint-end releases | Whenever item reaches Done |
| **Key metric** | Velocity (story points/sprint) | Cycle Time (days from start to done) |
| **Main report** | Burndown Chart | Cumulative Flow Diagram |
| **Best for** | Product teams, feature development | Support teams, ops, maintenance work |
| **Ceremonies** | Planning, Daily Standup, Review, Retro | No mandatory ceremonies |
| **Estimation** | Story points required | Optional; size-based classes of service |

---

## 8. Code Examples

### Jira REST API — Create an Issue

```bash
curl -X POST \
  "https://your-domain.atlassian.net/rest/api/3/issue" \
  -H "Authorization: Basic $(echo -n user@example.com:API_TOKEN | base64)" \
  -H "Content-Type: application/json" \
  -d '{
    "fields": {
      "project": { "key": "MYPROJ" },
      "summary": "Fix login page redirect bug",
      "description": {
        "type": "doc",
        "version": 1,
        "content": [{
          "type": "paragraph",
          "content": [{ "type": "text", "text": "Login redirects to 404 after OAuth callback." }]
        }]
      },
      "issuetype": { "name": "Bug" },
      "priority": { "name": "High" },
      "assignee": { "accountId": "5b10a2844c20165700ede21g" }
    }
  }'
```

### Python — Jira REST API with `requests`

```python
import requests
from requests.auth import HTTPBasicAuth
import json

JIRA_URL = "https://your-domain.atlassian.net"
EMAIL = "user@example.com"
API_TOKEN = "your_api_token"

auth = HTTPBasicAuth(EMAIL, API_TOKEN)
headers = {"Accept": "application/json", "Content-Type": "application/json"}

# Create an issue
def create_issue(project_key: str, summary: str, issue_type: str = "Story") -> dict:
    payload = {
        "fields": {
            "project": {"key": project_key},
            "summary": summary,
            "issuetype": {"name": issue_type}
        }
    }
    response = requests.post(
        f"{JIRA_URL}/rest/api/3/issue",
        data=json.dumps(payload),
        headers=headers,
        auth=auth
    )
    return response.json()

# Transition an issue (e.g., move to "In Progress")
def transition_issue(issue_key: str, transition_id: str) -> int:
    payload = {"transition": {"id": transition_id}}
    response = requests.post(
        f"{JIRA_URL}/rest/api/3/issue/{issue_key}/transitions",
        data=json.dumps(payload),
        headers=headers,
        auth=auth
    )
    return response.status_code  # 204 = success

# Get all issues in a sprint
def get_sprint_issues(board_id: int, sprint_id: int) -> list:
    response = requests.get(
        f"{JIRA_URL}/rest/agile/1.0/board/{board_id}/sprint/{sprint_id}/issue",
        headers=headers,
        auth=auth
    )
    return response.json().get("issues", [])
```

### Get Available Transitions for an Issue

```bash
# Retrieve transition IDs to use in the transition call
curl -X GET \
  "https://your-domain.atlassian.net/rest/api/3/issue/MYPROJ-42/transitions" \
  -H "Authorization: Basic <base64_token>" \
  -H "Accept: application/json"
```

### Jira Automation — No-Code Rule Example

```yaml
# Auto-assign bug to QA lead when status changes to "In Review"
Trigger: Issue transitioned
  - From: In Progress
  - To: In Review
  - Issue type: Bug

Condition: Issue type = Bug

Action: Assign issue
  - Assignee: {{smart-values: project.qa_lead}}

Action: Send email
  - To: {{issue.assignee.emailAddress}}
  - Subject: "Bug ready for QA: {{issue.summary}}"
```

### Install Jira CLI (go-jira)

```bash
# macOS
brew install jira-cli

# Configure
jira init
# Enter: JIRA_URL, email, API token

# Common CLI commands
jira issue list --project MYPROJ --status "In Progress"
jira issue create --project MYPROJ --type Bug --summary "Fix login redirect"
jira sprint list --board 42
jira issue move MYPROJ-99 "Done"
```

---

## 9. Configuration Reference

### Space (Project) Settings

| Parameter | Options | Description |
|---|---|---|
| `Project type` | Team-managed / Company-managed | Controls admin access and workflow customization scope |
| `Board type` | Scrum / Kanban / Bug Tracking | Determines board behavior and available views |
| `Issue types` | Epic, Story, Task, Bug, Subtask + custom | Types of work items available in the project |
| `Workflow` | Default or custom | Statuses and allowed transitions |
| `Columns` | Mapped to workflow statuses | Visible stages on the board |
| `Sprint duration` | 1 / 2 / 3 / 4 weeks (Scrum only) | Default sprint length for new sprints |
| `Story points field` | Story Points (default) | Field used for capacity planning |
| `WIP limits` | Per-column integer (Kanban only) | Max cards in a column before flagging overflow |

### Issue Field Reference

| Field | Type | Notes |
|---|---|---|
| `summary` | String | Required; issue title |
| `issuetype` | Object `{name}` | Epic, Story, Task, Bug, Subtask |
| `priority` | Object `{name}` | Highest, High, Medium, Low, Lowest |
| `assignee` | Object `{accountId}` | Must be project member |
| `reporter` | Object `{accountId}` | Defaults to API caller |
| `labels` | Array of strings | Free-text tags |
| `story_points` | Number | Custom field; varies by instance |
| `sprint` | Object `{id}` | Agile-only field; requires board context |
| `parent` | Object `{key}` | Links Story to Epic |
| `duedate` | Date `YYYY-MM-DD` | Optional deadline |

---

## 10. Best Practices

### Backlog Management
- ✅ Keep backlog prioritized top-to-bottom at all times — highest priority items at the top
- ✅ Groom the backlog weekly: remove stale items, add detail to upcoming ones
- ❌ Don't let the backlog grow to 200+ items — it becomes noise

### Issue Hygiene
- ✅ Write clear summaries that describe the outcome, not the activity: "User can reset password via email" not "Password reset feature"
- ✅ Link related issues (blocks, is-blocked-by, relates-to) to surface dependencies early
- ❌ Don't create issues without an assignee and due date for in-flight work
- ❌ Don't use Sub-tasks when a separate Story would give better visibility

### Sprint Discipline
- ✅ Only pull what the team can realistically complete — use past velocity as the guide
- ✅ Leave 15–20% capacity buffer for unplanned bugs and interruptions
- ❌ Don't add new stories mid-sprint without removing an equivalent one ("swapping, not stuffing")
- ❌ Don't close a sprint with 40%+ incomplete — it signals broken estimation or scope creep

### Board Hygiene
- ✅ Keep "In Progress" column small — one item per person at a time (WIP limit principle)
- ✅ Review stale tickets (no activity >3 days) in daily standups
- ❌ Don't use the board as a dumping ground — unestimated or unprioritized items belong in the Backlog

### Workflow Design
- ✅ Model your actual team process — don't adopt the default statuses if they don't reflect reality
- ✅ Add a "Blocked" status or use Jira's flag feature to make impediments visible
- ❌ Don't create more than 7 workflow statuses — complexity slows adoption

---

## 11. Interview Talking Points

### "How does Jira support agile project management?"

> Jira provides two primary agile board types — Scrum and Kanban — that map directly to the two most popular agile frameworks. Scrum boards run sprint-based delivery cycles where the team commits to a time-boxed set of work items from the Backlog; Kanban boards manage continuous flow with WIP limits per column. Both give teams a shared visual workspace, eliminating status meetings and providing real-time transparency into work state. Jira also ships with built-in agile metrics — Velocity charts, Burndown charts, and Cumulative Flow Diagrams — enabling data-driven retrospectives.

---

### "What is the difference between a team-managed and company-managed project in Jira?"

> Team-managed projects (formerly "next-gen") give individual teams full control over their workflow, issue types, and board configuration without needing a Jira admin. They're fast to set up but lack some advanced configuration. Company-managed projects (formerly "classic") are administered centrally by Jira admins, allowing standardized workflows, permission schemes, and notification schemes across the organization. For enterprise environments where consistency and governance matter, company-managed is preferred; for small autonomous teams moving fast, team-managed is the practical choice.

---

### "How would you explain the Epic → Story → Task hierarchy to a non-technical stakeholder?"

> Think of Epics as large initiatives or quarterly objectives — for example, "Launch mobile app." Within that Epic, Stories represent specific user-facing capabilities: "Users can sign up with Google." Tasks are the technical sub-steps a developer takes to deliver the Story: "Integrate OAuth 2.0 library," "Write unit tests for auth flow." This hierarchy lets executives track Epic-level progress while engineers track daily task completion — everyone is looking at the same system, just at different zoom levels.

---

### "How does Jira integrate with development workflows?"

> Jira integrates natively with GitHub, GitLab, and Bitbucket. When a developer names a branch or commit with a Jira issue key (e.g., `git commit -m "PROJ-42: Fix login redirect"`), Jira automatically links that commit, branch, and pull request to the issue. This creates a full traceability chain: from the user story that motivated the change, to the code that implemented it, to the deployment that shipped it. Teams can also trigger Jira automation from CI/CD events — for example, auto-transitioning an issue to "Done" when the associated PR is merged.

---

### "What are WIP limits and why do they matter in Kanban?"

> WIP (Work-In-Progress) limits cap the number of items allowed in a board column at any given time. For example, if the "In Review" column has a WIP limit of 3 and four items arrive, Jira flags the overflow visually. The purpose is to surface bottlenecks rather than mask them — if reviews are piling up, it means the review process is the constraint, and the team should swarm on reviews rather than start new work. WIP limits enforce the lean principle "stop starting, start finishing," which reduces context switching and shortens average cycle time.

---

### "How would you use Jira's REST API in a CI/CD pipeline?"

> A common pattern is to call the Jira REST API from a CI pipeline to automatically transition issues when code ships. For example: after a successful deployment to production, the pipeline sends a `POST /rest/api/3/issue/{issueKey}/transitions` request with the "Done" transition ID. This keeps Jira in sync with actual deploy state without manual updates. Similarly, if a deployment fails, the pipeline can use the API to add a comment to the related issue and reopen it. The API uses HTTP Basic Auth with an API token, and all modern CI tools (GitHub Actions, Azure DevOps, Jenkins) support shell steps that can call it.

---

## 12. Learning Resources

| Resource | Link | Type |
|---|---|---|
| YouTube — How Jira Works in 15 Min | [Watch](https://www.youtube.com/watch?v=8MIx4HLHljA) | Video |
| Atlassian — Jira Getting Started Introduction | [Read](https://www.atlassian.com/software/jira/guides/getting-started/introduction) | Official Docs |
| Atlassian — 7 Steps to Get Started with Jira | [Read](https://www.atlassian.com/software/jira/guides/getting-started/basics) | Official Docs |
| Atlassian — Scrum with Jira | [Read](https://www.atlassian.com/agile/tutorials/how-to-do-scrum-with-jira) | Tutorial |
| Atlassian — Jira REST API Reference | [Read](https://developer.atlassian.com/cloud/jira/platform/rest/v3/intro/) | API Docs |
| Atlassian — Jira Automation | [Read](https://www.atlassian.com/software/jira/features/automation) | Feature Docs |
| Jira CLI (go-jira) | [GitHub](https://github.com/ankitpokhrel/jira-cli) | Tool |

---

*Last Updated: June 2026 | Source: YouTube — How Jira Works in 15 Minutes. NO EXPERIENCE Needed!*

---

## Source Attribution

| Section | Primary Source | Additional Sources |
|---|---|---|
| Scrum framework | Scrum-Complete-Course-Guide.md | — |
| Jira getting started | Jira-Getting-Started-Guide.md | — |

*Consolidated: July 2026 | DocDedupAnalyzer Agent v1.0*
*Original files: Scrum-Complete-Course-Guide.md, Jira-Getting-Started-Guide.md | Zero data loss guaranteed*
