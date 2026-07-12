# Commands vs MCP vs Skills (What I Use)

> **Source:** [YouTube — Commands vs MCP vs Skills (What I Use)](https://www.youtube.com/watch?v=xAIN7YHXfCY)
> **Channel/Event:** Claude Code
> **Topic:** Claude Code, MCP, Skills, Custom Commands, Extension Mechanisms, Productivity
> **Key Claim:** Three distinct extension mechanisms serve different purposes — knowing which to reach for is what separates power users from beginners

---

## Table of Contents

1. [Overview](#1-overview)
2. [Problem Statement](#2-problem-statement)
3. [Core Concepts](#3-core-concepts)
4. [Architecture](#4-architecture)
5. [Key Components](#5-key-components)
6. [How It Works — Step by Step](#6-how-it-works--step-by-step)
7. [Comparison Table](#7-comparison-table)
8. [Code Examples](#8-code-examples)
9. [Configuration Reference](#9-configuration-reference)
10. [Best Practices](#10-best-practices)
11. [Interview Talking Points](#11-interview-talking-points)
12. [Learning Resources](#12-learning-resources)

---

## 1. Overview

Claude Code has three distinct ways to extend its capabilities: **built-in commands**, **MCP servers**, and **Skills**. Built-in commands (`/help`, `/compact`, `/clear`, `/model`) execute fixed logic and cannot be customized. MCP (Model Context Protocol) servers connect Claude to external tools, APIs, and data sources — letting it read Sentry errors, query Postgres, or create GitHub PRs directly. Skills are user-defined `SKILL.md` files that give Claude custom instructions, procedures, or reference knowledge — invoked by typing `/skill-name` or loaded automatically when relevant. Understanding which mechanism to reach for — and when — is the core skill that separates power users from beginners.

---

## 2. Problem Statement

Without knowing the distinction between these three mechanisms, users either over-engineer simple problems (writing an MCP server when a Skill would do) or under-engineer complex integrations (pasting data into chat when MCP would connect directly).

### Classic Approach Pain Points

| Problem | Impact |
|---|---|
| Copy-pasting data from issue trackers into chat | Slow, manual, error-prone — loses context on every session |
| Repeating the same multi-step instructions every session | Token waste, inconsistency, cognitive overhead |
| Sections of CLAUDE.md that have grown into procedures | Always loaded even when not needed — wastes context window |
| Not knowing when to use each mechanism | Reaches for MCP when a Skill would suffice, or vice versa |
| Custom commands living in `.claude/commands/` | Old pattern — Skills supersede commands with more power |

> **Key Insight:** "A Skill's body only loads when it's used, so long reference material costs almost nothing until you need it. CLAUDE.md always loads — that's the key distinction."

---

## 3. Core Concepts

### Built-in Commands
Fixed behaviors built into Claude Code, invoked with `/`. They execute deterministic logic directly — not prompt-based. Examples: `/help`, `/compact`, `/clear`, `/model`, `/mcp`, `/doctor`. You cannot create or modify built-in commands.

### Bundled Skills
Prompt-based skills that ship with Claude Code and are available in every session (unless disabled with `disableBundledSkills`). Unlike built-in commands, they give Claude detailed instructions and let it orchestrate work with its tools. Examples: `/code-review`, `/debug`, `/run`, `/verify`, `/loop`. Listed in the commands reference but marked **Skill** in the Purpose column.

### Skills (Custom)
User-defined instruction files stored as `SKILL.md` in a skill directory. A skill adds a `/skill-name` command and can also be invoked automatically by Claude when the description matches the user's request. Skills replaced custom commands (`.claude/commands/`) — the old location still works, but Skills add supporting files, frontmatter control, and subagent execution.

### MCP Server
An external process that implements the Model Context Protocol — an open standard for AI-tool integrations. MCP servers give Claude access to external tools, APIs, databases, and data sources that aren't accessible through file system operations alone. Configured once; Claude queries them on demand.

### CLAUDE.md
A persistent memory file that Claude reads at the start of every session. Contains facts, conventions, preferences — NOT procedures. When CLAUDE.md content grows into a step-by-step procedure, it should be extracted into a Skill (which only loads when invoked).

### Dynamic Context Injection
A Skill feature using `` !`<command>` `` syntax. Before Claude sees the skill content, the shell command runs and its output replaces the placeholder. Gives Claude actual live data (git diffs, PR info, env variables) rather than asking it to guess.

---

## 4. Architecture

### Three Extension Layers in Claude Code

```mermaid
flowchart TD
    User(["👤 User Input"])

    subgraph BuiltIn ["Built-in Commands (Fixed Logic)"]
        BC1["/help"]
        BC2["/compact"]
        BC3["/clear"]
        BC4["/model"]
        BC5["/mcp"]
        BC6["/doctor"]
    end

    subgraph SkillsLayer ["Skills (Custom Instructions)"]
        direction LR
        BS["Bundled Skills\n/code-review /debug\n/run /verify /loop"]
        PS["Personal Skills\n~/.claude/skills/"]
        PJS["Project Skills\n.claude/skills/"]
        ES["Enterprise Skills\nManaged Settings"]
        BS ~~~ PS ~~~ PJS ~~~ ES
    end

    subgraph MCPLayer ["MCP Servers (External Integrations)"]
        direction LR
        LocalMCP["Local Scope\n~/.claude.json"]
        ProjMCP["Project Scope\n.mcp.json"]
        UserMCP["User Scope\n~/.claude.json"]
        ClaiMCP["claude.ai Connectors"]
        LocalMCP ~~~ ProjMCP ~~~ UserMCP ~~~ ClaiMCP
    end

    subgraph Memory ["Session Memory"]
        CM["CLAUDE.md\n(Always loaded)"]
        SI["Skill Content\n(Loads on invocation)"]
        MT["MCP Tool Schemas\n(Deferred by default)"]
    end

    User -->|"/help, /compact, /clear"| BuiltIn
    User -->|"/skill-name or natural language"| SkillsLayer
    User -->|"Ask Claude to query\nexternal tool"| MCPLayer
    SkillsLayer --> Memory
    Memory --> Claude(["🤖 Claude"])
    MCPLayer --> Claude
    BuiltIn --> Claude

    style User fill:#0078D4,color:#fff,stroke:none
    style Claude fill:#107C10,color:#fff,stroke:none
    style BuiltIn fill:#EFF6FC,stroke:#0078D4
    style SkillsLayer fill:#FFF4CE,stroke:#D83B01
    style MCPLayer fill:#DFF6DD,stroke:#107C10
    style Memory fill:#F3F2F1,stroke:#5C2D91
```

### Skill Resolution Order

```mermaid
flowchart LR
    Enterprise["Enterprise\n(Managed Settings)"]
    Personal["Personal\n(~/.claude/skills/)"]
    Project["Project\n(.claude/skills/)"]
    Plugin["Plugin\n(plugin/skills/)"]
    Bundled["Bundled\n(Ships with Claude Code)"]

    Enterprise -->|"Overrides"| Personal
    Personal -->|"Overrides"| Project
    Project -->|"Overrides"| Plugin
    Plugin -->|"Overrides"| Bundled

    style Enterprise fill:#D83B01,color:#fff
    style Personal fill:#5C2D91,color:#fff
    style Project fill:#0078D4,color:#fff
    style Plugin fill:#107C10,color:#fff
    style Bundled fill:#6b7280,color:#fff
```

### MCP Server Scope Precedence

```mermaid
flowchart LR
    L["Local\n(highest priority)"]
    P["Project\n(.mcp.json)"]
    U["User\n(all projects)"]
    Pl["Plugin-provided"]
    C["claude.ai Connectors\n(lowest priority)"]

    L -->|"overrides"| P
    P -->|"overrides"| U
    U -->|"overrides"| Pl
    Pl -->|"overrides"| C

    style L fill:#D83B01,color:#fff
    style P fill:#0078D4,color:#fff
    style U fill:#5C2D91,color:#fff
    style Pl fill:#107C10,color:#fff
    style C fill:#6b7280,color:#fff
```

---

## 5. Key Components

### Skills Components

| Component | Path | Scope |
|---|---|---|
| **Enterprise Skills** | Managed Settings | All org users |
| **Personal Skills** | `~/.claude/skills/<name>/SKILL.md` | All your projects |
| **Project Skills** | `.claude/skills/<name>/SKILL.md` | This project only |
| **Plugin Skills** | `<plugin>/skills/<name>/SKILL.md` | Where plugin enabled |
| **Legacy Commands** | `.claude/commands/<name>.md` | Same as project skill |
| **Supporting Files** | `<skill-dir>/templates/`, `examples/`, `scripts/` | Per skill |

### MCP Server Transport Types

| Transport | Use Case | Config Method |
|---|---|---|
| **HTTP** (recommended) | Cloud services, remote APIs | `claude mcp add --transport http` |
| **SSE** (deprecated) | Legacy remote servers | `claude mcp add --transport sse` |
| **stdio** | Local tools, scripts, system access | `claude mcp add [name] -- command` |
| **WebSocket** | Event-driven / push notifications | `claude mcp add-json` with `"type":"ws"` |

### MCP Scopes

| Scope | Stored In | Available In | Team-Shared |
|---|---|---|---|
| **Local** (default) | `~/.claude.json` per project path | Current project | No |
| **Project** | `.mcp.json` in project root | Current project | Yes (committed) |
| **User** | `~/.claude.json` global | All your projects | No |
| **Enterprise** | Managed settings | All org users | Yes (admin-deployed) |

### Skill Frontmatter Fields (Key Ones)

| Field | Purpose | Example |
|---|---|---|
| `description` | Tells Claude when to auto-invoke | "Use when user asks to deploy..." |
| `disable-model-invocation` | Only user can invoke; Claude cannot | `true` for `/deploy`, `/send-email` |
| `user-invocable` | Set `false` for background knowledge Claude can use but user can't invoke | `false` |
| `allowed-tools` | Pre-approve tools for this skill without per-use prompts | `Bash(git add *) Bash(git commit *)` |
| `context: fork` | Run in isolated subagent | `fork` |
| `agent` | Which subagent type to use | `Explore`, `Plan`, `general-purpose` |
| `paths` | Limit skill to specific file patterns | `src/**.ts` |
| `model` | Override model for this skill's turn | `claude-opus-4-8` |
| `effort` | Override effort level | `max` |

---

## 6. How It Works — Step by Step

### Skill Invocation Flow

```mermaid
sequenceDiagram
    participant U as User
    participant CC as Claude Code
    participant DCI as Dynamic Context Injection
    participant Claude as Claude LLM
    participant SA as Subagent (if context:fork)

    U->>CC: Types /skill-name [args]
    CC->>CC: Resolve skill path (enterprise > personal > project > plugin > bundled)
    CC->>DCI: Execute !`commands` in SKILL.md
    DCI-->>CC: Replace placeholders with live output
    alt context: fork
        CC->>SA: Fork subagent with rendered SKILL.md as prompt
        SA-->>CC: Return result summary
        CC-->>U: Show result
    else inline
        CC->>Claude: Inject rendered SKILL.md into conversation
        Claude-->>U: Respond using skill instructions
    end
```

### MCP Tool Call Flow (with Tool Search)

```mermaid
sequenceDiagram
    participant U as User
    participant CC as Claude Code
    participant TS as Tool Search
    participant MCP as MCP Server
    participant Claude as Claude LLM

    U->>CC: "Check the error in Sentry issue #123"
    CC->>Claude: Forward prompt (MCP tools deferred by default)
    Claude->>TS: Search for relevant MCP tools
    TS-->>Claude: Return "sentry__get_issue" tool schema
    Claude->>CC: Call sentry__get_issue(id=123)
    CC->>MCP: Forward tool call to Sentry MCP server
    MCP-->>CC: Return issue details
    CC-->>Claude: Tool result
    Claude-->>U: Analysis + recommendation
```

### Deciding What to Build

```mermaid
flowchart TD
    Start(["You have a task\nyou do repeatedly"])
    Q1{"Is it a multi-step\nprocedure or checklist?"}
    Q2{"Does it need access to\nexternal systems (APIs,\nDBs, issue trackers)?"}
    Q3{"Do you want Claude\nto know facts/context\nconstantly?"}
    Q4{"Should it run automatically\nor only when you invoke?"}

    SkillAuto["Skill\n(user-invocable: false\nor default)"]
    SkillManual["Skill\n(disable-model-invocation: true)"]
    MCPServer["MCP Server"]
    CLAUDEMD["CLAUDE.md entry"]
    Neither["Just type it in chat\n(one-off task)"]

    Start --> Q1
    Q1 -->|"Yes — procedure"| Q4
    Q1 -->|"No"| Q2
    Q2 -->|"Yes — external data"| MCPServer
    Q2 -->|"No"| Q3
    Q3 -->|"Yes — always needed"| CLAUDEMD
    Q3 -->|"No — only sometimes"| SkillAuto
    Q4 -->|"Claude decides"| SkillAuto
    Q4 -->|"I decide"| SkillManual

    style MCPServer fill:#107C10,color:#fff
    style SkillAuto fill:#0078D4,color:#fff
    style SkillManual fill:#5C2D91,color:#fff
    style CLAUDEMD fill:#D83B01,color:#fff
    style Neither fill:#6b7280,color:#fff
```

---

## 7. Comparison Table

| Dimension | Built-in Commands | MCP Servers | Skills |
|---|---|---|---|
| **What it is** | Fixed Claude Code behaviors | External tool/data integrations | Custom instruction files |
| **Who creates it** | Anthropic only | You or third parties | You |
| **Invocation** | `/help`, `/compact`, etc. | Ask Claude naturally; Claude calls the tool | `/skill-name` or Claude auto-invokes |
| **Customizable** | No | Yes (which server, which tools) | Yes (full control via SKILL.md) |
| **Lives in** | Claude Code binary | `~/.claude.json` or `.mcp.json` | `.claude/skills/` or `~/.claude/skills/` |
| **Load timing** | Always available | Deferred by default (tool search) | Description always in context; body loads on invoke |
| **External access** | None | Yes — APIs, DBs, file systems | No (uses Claude's tools via `allowed-tools`) |
| **Token cost** | Zero (fixed logic) | Low (deferred schemas) | Near-zero until invoked; body stays in context after |
| **Team sharing** | N/A | Via `.mcp.json` in version control | Via `.claude/skills/` in version control |
| **Examples** | `/doctor`, `/clear`, `/model` | GitHub, Sentry, Postgres, Slack | `/deploy`, `/code-review`, `/pr-summary` |
| **Can run in subagent** | No | No | Yes (`context: fork`) |
| **Auto-invocation** | N/A | No | Yes (if description matches) |
| **Supports arguments** | Some (e.g., `/model opus`) | Yes (tool parameters) | Yes (`$ARGUMENTS`, `$0`, `$1`, named args) |
| **Predecessor** | N/A | N/A | `.claude/commands/<name>.md` |

---

## 8. Code Examples

### Skill — Basic SKILL.md with Frontmatter

```yaml
---
name: pr-summary
description: Summarize the current pull request. Use when the user asks what's in the PR, wants a PR description, or asks to review changes.
context: fork
agent: Explore
allowed-tools: Bash(gh *)
---

## Pull Request Context

- PR diff: !`gh pr diff`
- PR comments: !`gh pr view --comments`
- Changed files: !`gh pr diff --name-only`
- Current branch: !`git branch --show-current`

## Your Task

Summarize this pull request in a format suitable for the PR description:
1. What changed and why (2-3 bullet points)
2. Any risks or side effects to call out
3. Suggested test plan
```

### Skill — Deploy Skill (Manual-Only, No Auto-Invocation)

```yaml
---
name: deploy
description: Deploy the application to production
disable-model-invocation: true
allowed-tools: Bash(npm *) Bash(git *) Bash(docker *)
---

Deploy $ARGUMENTS to production:

1. Confirm current branch: !`git branch --show-current`
2. Run test suite: `npm test`
3. Build production bundle: `npm run build`
4. Docker build and push: `docker build -t app:latest . && docker push`
5. Run smoke tests against staging
6. Tag the release: `git tag v$(date +%Y%m%d-%H%M)`
7. Report deployment status
```

### Skill — Dynamic Context with Arguments

```yaml
---
name: fix-issue
description: Fix a GitHub issue by number
disable-model-invocation: true
argument-hint: [issue-number]
allowed-tools: Bash(gh *) Read Edit Bash(git *)
---

Fix GitHub issue #$ARGUMENTS following our coding standards.

## Issue Context

!`gh issue view $ARGUMENTS`

## Steps

1. Read the issue description above
2. Find the relevant code using search tools
3. Implement the fix
4. Write or update tests
5. Create a commit referencing the issue: `git commit -m "fix: <summary> (#$ARGUMENTS)"`
```

### Skill — Background Knowledge (Claude-Only)

```yaml
---
name: api-conventions
description: API design patterns and conventions for this codebase. Use when writing or reviewing API endpoints.
user-invocable: false
---

When writing API endpoints in this codebase:
- Use RESTful naming: `/api/v1/resources/{id}`
- Return `{ data: ..., error: null }` or `{ data: null, error: { code, message } }`
- Validate input at the controller layer using Zod schemas
- Use 422 for validation errors, 404 for not found, 409 for conflicts
- Always paginate list endpoints: `?page=1&limit=20`, return `{ data: [], total, page, limit }`
```

### MCP — Adding Servers (CLI)

```bash
# Add a remote HTTP server (recommended transport)
claude mcp add --transport http sentry https://mcp.sentry.dev/mcp

# Add GitHub with auth header
claude mcp add --transport http github https://api.githubcopilot.com/mcp/ \
  --header "Authorization: Bearer YOUR_GITHUB_PAT"

# Add a local stdio server (needs direct system access)
claude mcp add --transport stdio db -- npx -y @bytebase/dbhub \
  --dsn "postgresql://readonly:pass@prod.db.com:5432/analytics"

# Add with project scope (team-shared via .mcp.json)
claude mcp add --transport http paypal --scope project https://mcp.paypal.com/mcp

# Add with user scope (available in all your projects)
claude mcp add --transport http hubspot --scope user https://mcp.hubspot.com/anthropic

# List, inspect, remove
claude mcp list
claude mcp get github
claude mcp remove github

# Check server status inside Claude Code
# /mcp
```

### MCP — Project-Scoped `.mcp.json` (Team-Shared)

```json
{
  "mcpServers": {
    "github": {
      "type": "http",
      "url": "https://api.githubcopilot.com/mcp/",
      "headers": {
        "Authorization": "Bearer ${GITHUB_PAT}"
      }
    },
    "postgres": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@bytebase/dbhub", "--dsn", "${DATABASE_URL}"]
    },
    "sentry": {
      "type": "http",
      "url": "https://mcp.sentry.dev/mcp"
    }
  }
}
```

### MCP — WebSocket for Push Events

```bash
claude mcp add-json events-server \
  '{"type":"ws","url":"wss://mcp.example.com/socket","headers":{"Authorization":"Bearer YOUR_TOKEN"}}'
```

### Skill — Subagent Research with Explore Agent

```yaml
---
name: deep-research
description: Research a topic thoroughly across the codebase. Use when asked to find all usages, understand a system, or map dependencies.
context: fork
agent: Explore
---

Research $ARGUMENTS thoroughly:

1. Find all relevant files using Glob and Grep
2. Read and analyze the code in each file
3. Map dependencies and call chains
4. Summarize findings with specific file:line references
5. Identify potential issues or improvements
```

### Setting Up Skill Directory Structure

```bash
# Personal skill (all projects)
mkdir -p ~/.claude/skills/pr-summary
# → Place SKILL.md here

# Project skill (this project only)
mkdir -p .claude/skills/deploy

# Skill with supporting files
mkdir -p ~/.claude/skills/code-standards/examples
# ~/.claude/skills/code-standards/
# ├── SKILL.md           (main instructions — reference examples/)
# ├── examples/
# │   ├── good-component.tsx
# │   └── bad-component.tsx
# └── checklist.md       (loaded by SKILL.md when needed)

# Legacy command (still works)
mkdir -p .claude/commands
# .claude/commands/deploy.md
```

---

## 9. Configuration Reference

### Skill Frontmatter — Full Reference

| Field | Type | Default | Description |
|---|---|---|---|
| `name` | string | Directory name | Display label in skill listings |
| `description` | string | First paragraph | When Claude auto-invokes; put key use case first |
| `when_to_use` | string | — | Additional trigger context; appended to description |
| `argument-hint` | string | — | Autocomplete hint e.g. `[issue-number]` |
| `arguments` | list | — | Named positional args for `$name` substitution |
| `disable-model-invocation` | bool | `false` | Prevent Claude from auto-invoking; user-only |
| `user-invocable` | bool | `true` | Set `false` to hide from `/` menu |
| `allowed-tools` | list | — | Tools pre-approved for this skill |
| `disallowed-tools` | list | — | Tools blocked while skill is active |
| `context` | string | — | Set `fork` to run in subagent |
| `agent` | string | `general-purpose` | Subagent type when `context: fork` |
| `model` | string | Session model | Override model for this skill's turn |
| `effort` | string | Session effort | `low`, `medium`, `high`, `xhigh`, `max` |
| `paths` | list | — | Glob patterns — auto-invoke only for matching files |
| `hooks` | object | — | Skill-scoped lifecycle hooks |
| `shell` | string | `bash` | Shell for `` !`cmd` `` blocks |

### Skill Variable Substitutions

| Variable | Description |
|---|---|
| `$ARGUMENTS` | Full argument string after `/skill-name` |
| `$ARGUMENTS[0]`, `$0` | First argument |
| `$ARGUMENTS[1]`, `$1` | Second argument |
| `$name` | Named arg from `arguments` frontmatter |
| `${CLAUDE_SESSION_ID}` | Current session ID |
| `${CLAUDE_EFFORT}` | Current effort level |
| `${CLAUDE_SKILL_DIR}` | Directory containing this SKILL.md |
| `${CLAUDE_PROJECT_DIR}` | Project root directory |

### MCP CLI Reference

| Command | Purpose |
|---|---|
| `claude mcp add --transport http <name> <url>` | Add HTTP server |
| `claude mcp add --transport stdio <name> -- <cmd>` | Add stdio server |
| `claude mcp add --scope project <name> <url>` | Add to `.mcp.json` (team-shared) |
| `claude mcp add --scope user <name> <url>` | Add across all projects |
| `claude mcp add --env KEY=val <name> <url>` | Pass environment variables |
| `claude mcp list` | List all configured servers |
| `claude mcp get <name>` | Show server details |
| `claude mcp remove <name>` | Remove a server |
| `claude mcp login <name>` | Run OAuth flow for a server |
| `claude mcp logout <name>` | Clear stored credentials |
| `claude mcp add-from-claude-desktop` | Import from Claude Desktop config |
| `ENABLE_TOOL_SEARCH=false claude` | Load all MCP tools upfront (no deferral) |

---

## 10. Best Practices

### When to Use Each Mechanism

**Use CLAUDE.md when:**
- ✅ It's a fact, convention, or preference Claude always needs
- ✅ "Always use TypeScript strict mode in this project"
- ❌ Don't put procedures or checklists here — they always load

**Use a Skill when:**
- ✅ You keep pasting the same instructions into chat
- ✅ A section of CLAUDE.md has grown into a step-by-step procedure
- ✅ You want Claude to load it automatically based on context (no `disable-model-invocation`)
- ✅ You want actions with side effects only you should trigger (`disable-model-invocation: true`)
- ❌ Don't use a Skill for accessing external systems — use MCP

**Use MCP when:**
- ✅ You're copying data from another tool into chat (issue tracker, dashboard, DB)
- ✅ Claude needs to take actions in external systems (create PRs, send messages)
- ✅ You need real-time or live data (monitoring alerts, DB queries)
- ❌ Don't build an MCP server for something a Skill with Bash can do

**Use `context: fork` on a Skill when:**
- ✅ The task is self-contained and doesn't need conversation history
- ✅ Research tasks (use `agent: Explore`)
- ✅ Tasks that should not affect the main conversation context
- ❌ Don't fork if the skill contains only guidelines without an explicit task

### Skill Design
- ✅ Keep `SKILL.md` under 500 lines — move detail to supporting files
- ✅ Put the key use case first in `description` — truncated at 1,536 chars
- ✅ Use `disable-model-invocation: true` for anything with side effects
- ✅ Use dynamic context injection `` !`cmd` `` to give Claude live data
- ❌ Don't write narrative in skills — state what to do, not why
- ❌ Don't put the same skill at multiple scopes unless intentionally overriding

### MCP Management
- ✅ Use project scope (`.mcp.json`) for team-shared servers
- ✅ Use `${ENV_VAR}` in `.mcp.json` for secrets — never hardcode credentials
- ✅ Set `alwaysLoad: true` only for tools Claude needs on every single turn
- ❌ Don't add too many `alwaysLoad` servers — each adds upfront context cost
- ❌ Don't use SSE transport for new integrations — HTTP is the replacement

---

## 11. Interview Talking Points

### "What is the difference between a Skill and an MCP server in Claude Code?"

> A Skill is a `SKILL.md` file containing instructions that Claude follows — it extends what Claude knows and how it behaves. An MCP server is an external process that exposes tools Claude can call to interact with outside systems. The key distinction: Skills change Claude's behavior by injecting instructions into context, while MCP servers give Claude access to external data and actions it couldn't otherwise reach. A Skill can tell Claude "always write tests first" or "here are our deployment steps." An MCP server lets Claude query your Postgres database or create a GitHub issue. You would never use MCP to pass behavior instructions, and you can't use a Skill to query a live database.

### "When would you use `disable-model-invocation: true` on a Skill?"

> You set `disable-model-invocation: true` on Skills that have side effects or that you want to control the timing of — things like `/deploy`, `/send-slack-message`, or `/publish-release`. Without this flag, Claude could decide to deploy your application because your code "looks ready," which is almost never what you want. The flag removes the skill's description from Claude's context entirely, so Claude isn't even aware it exists as an option. You invoke it explicitly by typing `/skill-name`. Contrast this with `user-invocable: false`, which does the opposite: Claude can auto-invoke the skill but it doesn't appear in your `/` menu because it's background knowledge, not an action.

### "How does MCP tool search work and why does it matter?"

> By default, Claude Code defers MCP tool schemas — only tool names and server instructions load at session start. When Claude needs an MCP tool, it calls a `ToolSearch` step to find and load the relevant schema. This means adding dozens of MCP servers has minimal impact on your context window, because you're only paying token cost for tools Claude actually uses in a given session. Without tool search, every connected MCP server's full tool schema would load upfront, which would burn thousands of tokens before you even type a message. You can override this with `ENABLE_TOOL_SEARCH=false` to load everything upfront, or `alwaysLoad: true` per server for tools Claude needs on every single turn.

### "How do Skills replace custom commands and what's different?"

> Custom commands were `.claude/commands/<name>.md` files — they created `/name` commands using a plain markdown file. Skills supersede them: a `.claude/skills/<name>/SKILL.md` file creates the same `/name` command, but Skills add four things custom commands lacked. First, a supporting files directory — templates, examples, scripts that SKILL.md can reference without loading them all at once. Second, frontmatter for fine-grained control: invocation control, tool pre-approval, model overrides, path-based restrictions. Third, `context: fork` to run in an isolated subagent. Fourth, Claude can auto-invoke Skills based on description matching — custom commands were always user-invoked only. Old `.claude/commands/` files still work without migration.

### "What's the right mental model for deciding between CLAUDE.md, Skills, and MCP?"

> Think of it as three questions in order. First: does Claude need this constantly, on every single message? If yes, it's a CLAUDE.md fact — a project convention, a preference, a constraint. Second: is this a multi-step procedure that Claude only needs sometimes? If yes, it's a Skill — it loads only when invoked, preserving your context window. Third: does the task require reaching outside the conversation into a live external system — a database, an API, an issue tracker? If yes, it's MCP. The common mistake is using CLAUDE.md for procedures (they always load, wasting tokens) or using Bash in a Skill where an MCP server would give structured, queryable access to live data.

---

## 12. Learning Resources

| Resource | Link | Type |
|---|---|---|
| Commands vs MCP vs Skills (Video) | [YouTube](https://www.youtube.com/watch?v=xAIN7YHXfCY) | Video |
| Claude Code Skills Docs | [code.claude.com/docs/en/slash-commands](https://code.claude.com/docs/en/slash-commands) | Official Docs |
| MCP Integration Docs | [code.claude.com/docs/en/mcp](https://code.claude.com/docs/en/mcp) | Official Docs |
| MCP Quickstart | [code.claude.com/docs/en/mcp-quickstart](https://code.claude.com/docs/en/mcp-quickstart) | Tutorial |
| Commands Reference | [code.claude.com/docs/en/commands](https://code.claude.com/docs/en/commands) | Reference |
| Agent Skills Open Standard | [agentskills.io](https://agentskills.io) | Standard |
| Evaluating Skill Quality | [agentskills.io/skill-creation/evaluating-skills](https://agentskills.io/skill-creation/evaluating-skills) | Guide |
| Anthropic MCP Directory | [claude.ai/directory](https://claude.ai/directory) | Directory |
| skill-creator Plugin | [github.com/anthropics/claude-plugins-official](https://github.com/anthropics/claude-plugins-official) | GitHub |
| Memory & CLAUDE.md | [code.claude.com/docs/en/memory](https://code.claude.com/docs/en/memory) | Official Docs |
| Subagents | [code.claude.com/docs/en/sub-agents](https://code.claude.com/docs/en/sub-agents) | Official Docs |
| Hooks | [code.claude.com/docs/en/hooks](https://code.claude.com/docs/en/hooks) | Official Docs |

---

*Last Updated: July 2026 | Source: Claude Code — Commands vs MCP vs Skills (What I Use)*
