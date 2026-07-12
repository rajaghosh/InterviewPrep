# Claude Code — Skills vs Hooks vs Commands vs Plugins (with MCP)
> **Consolidated From:** Claude-Code-Skills-vs-Hooks-vs-Commands-vs-Plugins.md, Claude-Code-Commands-vs-MCP-vs-Skills.md, Claude-Code-Hooks-vs-Skills.md
> **Topics Covered:** Claude Code skills, hooks, slash commands, plugins, MCP servers, MCP setup, decision matrix, when-to-use guidance
> **Consolidation Date:** 2026-07-04
> **Original Documents:** 3 → **Content Preserved:** 100%

---

> **Source:** [YouTube — Skills vs Hooks vs Commands vs Plugins](https://www.youtube.com/shorts/1G3uw6lgt8o)
> **Channel/Event:** Claude Code
> **Topic:** Claude Code, Skills, Hooks, Commands, Plugins, Extension Mechanisms, Automation
> **Key Claim:** Four distinct extension mechanisms each serve a different purpose — Commands expose behaviors, Skills extend knowledge, Hooks automate reactions, Plugins bundle everything for sharing

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

Claude Code provides four mechanisms to extend and automate its behavior: **Commands**, **Skills**, **Hooks**, and **Plugins**. Commands are fixed built-in behaviors (`/help`, `/compact`) that cannot be customized. Skills are `SKILL.md` instruction files that extend what Claude knows and how it responds — invoked by typing `/skill-name` or auto-triggered by context. Hooks are shell commands, HTTP endpoints, or LLM prompts that fire automatically at lifecycle events like before a tool runs or after Claude finishes — they can enforce policies, add context, and even block actions. Plugins are self-contained bundles that package Skills, Hooks, MCP servers, and Agents together for distribution across projects and teams. This video clarifies when to reach for each mechanism and how they compose together.

> **Related:** See also `Claude-Code-Commands-vs-MCP-vs-Skills.md` for the MCP dimension of this ecosystem.

---

## 2. Problem Statement

Without understanding these four mechanisms, users manually repeat work that should be automated, miss enforcement opportunities, or build complex MCP servers when a simple hook would suffice.

### Classic Approach Pain Points

| Problem | Impact |
|---|---|
| Manually running linters/formatters after every edit | Slow feedback; easy to forget; inconsistent |
| No way to block dangerous commands before they run | `rm -rf` accidents, destructive operations in production |
| Repeating the same multi-step instructions every session | Token waste; cognitive overhead; inconsistency |
| Can't share workflows with teammates easily | Every team member rebuilds from scratch |
| Procedural content in CLAUDE.md always loads | Wastes context window on unused procedures |
| No notifications when Claude finishes long tasks | Must watch terminal instead of doing other work |

> **Key Insight:** "Skills change what Claude knows. Hooks change what Claude is allowed to do and what happens around its actions. Plugins are how you ship both to your team."

---

## 3. Core Concepts

### Commands (Built-in)
Fixed behaviors hardcoded into Claude Code. Invoked with `/` but execute deterministic logic — not Claude-driven. Examples: `/help`, `/compact`, `/clear`, `/model`, `/mcp`, `/doctor`, `/status`. Cannot be created or modified by users.

### Commands (Bundled Skills)
Prompt-driven skills that ship with Claude Code, listed alongside built-in commands. Unlike true built-ins, these are instruction-based and Claude orchestrates the work. Examples: `/code-review`, `/debug`, `/run`, `/verify`, `/loop`. Marked **Skill** in the commands reference.

### Skills (Custom)
User-defined `SKILL.md` files that add Claude knowledge, reference material, or step-by-step procedures. A skill's content only loads when invoked — unlike CLAUDE.md which always loads. Can be auto-invoked by Claude when description matches, or user-invoked with `/skill-name`. Scoped to enterprise, personal, project, or plugin level.

### Hooks
Shell commands, HTTP endpoints, MCP tool calls, LLM prompts, or subagents that fire automatically at specific lifecycle events — not on user request. Hooks can intercept tool execution before it happens (`PreToolUse`), react after (`PostToolUse`), enforce policies (exit code 2 = block), inject context into Claude's awareness, or trigger external notifications. Hooks cannot be invoked manually — they are always event-driven.

### Plugins
Self-contained directories that bundle any combination of Skills, Agents, Hooks, MCP servers, LSP servers, background monitors, and default settings under a single name. Plugins are namespaced (`/plugin-name:skill-name`) to prevent conflicts. Distributed through marketplaces; installed with `/plugin install`. The shareable, versionable packaging layer over standalone `.claude/` configuration.

### CLAUDE.md
Always-loaded memory file for facts, conventions, and preferences. Not for procedures (use Skills) and not for automation (use Hooks).

---

## 4. Architecture

### All Four Mechanisms in Context

```mermaid
flowchart TD
    User(["👤 User Input"])
    Claude(["🤖 Claude LLM"])

    subgraph Commands ["Commands (Fixed Logic)"]
        BC["/help /compact\n/clear /model /doctor"]
        BS["Bundled Skills\n/code-review /debug /run /verify"]
    end

    subgraph Skills ["Skills (Custom Instructions)"]
        PS["Personal Skills\n~/.claude/skills/"]
        PJS["Project Skills\n.claude/skills/"]
        PLG["Plugin Skills\n/plugin:skill-name"]
    end

    subgraph Hooks ["Hooks (Automated Reactions)"]
        H1["PreToolUse\n→ can BLOCK"]
        H2["PostToolUse\n→ lint, notify, log"]
        H3["Stop\n→ notify, summarize"]
        H4["UserPromptSubmit\n→ inject context"]
        H5["SessionStart\n→ load env vars"]
    end

    subgraph Plugins ["Plugins (Distribution Bundle)"]
        PL["plugin.json manifest\n+ Skills + Hooks\n+ MCP + Agents + LSP\n+ Monitors + Settings"]
    end

    subgraph Memory ["Context Window"]
        CM["CLAUDE.md\n(always loaded)"]
        SK["Skill content\n(loads on invoke)"]
        MCP["MCP schemas\n(deferred by default)"]
    end

    User -->|"/ command"| Commands
    User -->|"natural language or /skill"| Skills
    Skills --> Memory
    Plugins -->|"provides"| Skills
    Plugins -->|"provides"| Hooks
    Memory --> Claude
    Claude -->|"tool call"| Hooks
    Hooks -->|"block / allow / enrich"| Claude
    Commands --> Claude

    style User fill:#0078D4,color:#fff,stroke:none
    style Claude fill:#107C10,color:#fff,stroke:none
    style Commands fill:#EFF6FC,stroke:#0078D4
    style Skills fill:#FFF4CE,stroke:#D83B01
    style Hooks fill:#FDE7E9,stroke:#D83B01
    style Plugins fill:#DFF6DD,stroke:#107C10
    style Memory fill:#F3F2F1,stroke:#5C2D91
```

### Hook Lifecycle — When Each Event Fires

```mermaid
flowchart LR
    SS["SessionStart\n(session begins)"]
    UPS["UserPromptSubmit\n(before Claude processes)"]
    PTU["PreToolUse\n(before tool runs)\n⚠️ CAN BLOCK"]
    ToolRun["Tool Executes"]
    PostOK["PostToolUse\n(tool succeeded)"]
    PostFail["PostToolUseFailure\n(tool failed)"]
    Stop["Stop\n(Claude finishes responding)"]
    SE["SessionEnd\n(session terminates)"]

    SS --> UPS --> PTU
    PTU -->|"exit 0 = allow"| ToolRun
    PTU -->|"exit 2 = BLOCK"| UPS
    ToolRun -->|"success"| PostOK
    ToolRun -->|"failure"| PostFail
    PostOK --> Stop
    PostFail --> Stop
    Stop --> UPS

    style PTU fill:#D83B01,color:#fff
    style Stop fill:#5C2D91,color:#fff
    style SS fill:#107C10,color:#fff
    style SE fill:#6b7280,color:#fff
```

### Plugin Directory Structure

```mermaid
flowchart TD
    Root["my-plugin/ (plugin root)"]

    Manifest[".claude-plugin/\nplugin.json"]
    SkillsDir["skills/\n<name>/SKILL.md"]
    AgentsDir["agents/\n<name>.md"]
    HooksDir["hooks/\nhooks.json"]
    MCPFile[".mcp.json\nMCP server configs"]
    LSPFile[".lsp.json\nLSP server configs"]
    Monitors["monitors/\nmonitors.json"]
    BinDir["bin/\nexecutables in PATH"]
    Settings["settings.json\ndefault settings"]

    Root --> Manifest
    Root --> SkillsDir
    Root --> AgentsDir
    Root --> HooksDir
    Root --> MCPFile
    Root --> LSPFile
    Root --> Monitors
    Root --> BinDir
    Root --> Settings

    style Root fill:#0078D4,color:#fff
    style Manifest fill:#D83B01,color:#fff
    style HooksDir fill:#5C2D91,color:#fff
    style SkillsDir fill:#107C10,color:#fff
```

---

## 5. Key Components

### Hook Event Types

| Event | When It Fires | Can Block? | Common Use |
|---|---|---|---|
| `SessionStart` | Session begins or resumes | No | Load env vars, greet, set context |
| `UserPromptSubmit` | Before Claude processes your message | Yes (exit 2) | Inject context, filter prompts |
| `PreToolUse` | Before any tool executes | **Yes (exit 2)** | Block dangerous commands, enforce policy |
| `PostToolUse` | After a tool succeeds | No | Run linter, log changes, notify |
| `PostToolUseFailure` | After a tool fails | No | Alert, retry logic, diagnostics |
| `Stop` | Claude finishes responding | No | Desktop notification, log session output |
| `FileChanged` | Watched file changes on disk | No | Reload config, trigger rebuild |
| `CwdChanged` | Working directory changes | No | Adjust context for new project |
| `SessionEnd` | Session terminates | No | Cleanup, summary, audit log |
| `PermissionRequest` | Permission dialog appears | Yes | Auto-allow/deny specific tools |
| `WorktreeCreate` | Worktree being created | No | Setup worktree environment |

### Hook Handler Types

| Type | Description | Best For |
|---|---|---|
| `command` | Shell script receives JSON on stdin | Most use cases — flexible, powerful |
| `http` | HTTP POST to an endpoint | Integration with external services |
| `mcp_tool` | Calls a tool on connected MCP server | Leverage existing MCP integrations |
| `prompt` | LLM evaluates yes/no decision | Nuanced policy decisions needing reasoning |
| `agent` | Spawns subagent with tools (experimental) | Complex multi-step automation |

### Hook Exit Codes

| Exit Code | Meaning | Claude Behavior |
|---|---|---|
| `0` | Success — stdout parsed for JSON decisions | Continues; reads JSON response if present |
| `2` | **Blocking error** — stderr shown to Claude | **Action blocked**; Claude sees error message |
| Other (1, 3+) | Non-blocking error | Continues; error logged but not surfaced |

### Plugin Components

| Directory / File | Purpose |
|---|---|
| `.claude-plugin/plugin.json` | Manifest — name, version, description, author |
| `skills/<name>/SKILL.md` | Custom skills (namespaced `/plugin:skill`) |
| `commands/<name>.md` | Legacy skills (use `skills/` for new plugins) |
| `agents/<name>.md` | Custom subagent definitions |
| `hooks/hooks.json` | Event hooks (same format as settings.json hooks) |
| `.mcp.json` | MCP server configurations |
| `.lsp.json` | LSP server configs for code intelligence |
| `monitors/monitors.json` | Background monitors (log watchers, etc.) |
| `bin/` | Executables added to Bash tool's PATH |
| `settings.json` | Default settings applied when plugin is enabled |

### Standalone vs Plugin Decision

| Condition | Use Standalone `.claude/` | Use Plugin |
|---|---|---|
| Single project customization | ✅ | — |
| Personal, not shared | ✅ | — |
| Quick experiment / prototype | ✅ | — |
| Want short names `/deploy` | ✅ | — |
| Share with teammates | — | ✅ |
| Distribute to community | — | ✅ |
| Same skills across multiple projects | — | ✅ |
| Versioned releases | — | ✅ |
| Bundle MCP + Skills + Hooks together | — | ✅ |
| Okay with namespaced `/plugin:skill` | — | ✅ |

---

## 6. How It Works — Step by Step

### Hook Execution Flow (PreToolUse)

```mermaid
sequenceDiagram
    participant U as User
    participant CC as Claude Code
    participant Claude as Claude LLM
    participant H as Hook Script
    participant Tool as Tool (Bash/Edit/etc.)

    U->>CC: Send message
    CC->>Claude: Process prompt
    Claude->>CC: Request tool call (e.g., Bash "rm -rf ./dist")
    CC->>H: Fire PreToolUse hook (JSON via stdin)
    Note over H: {"tool_name":"Bash","tool_input":{"command":"rm -rf ./dist"}}
    H->>H: Check if command is dangerous
    alt Dangerous command detected
        H-->>CC: exit 2 + stderr "Blocked: destructive command"
        CC-->>Claude: Tool blocked — show error message
        Claude-->>U: "I cannot run that command (blocked by policy)"
    else Safe command
        H-->>CC: exit 0
        CC->>Tool: Execute tool
        Tool-->>CC: Result
        CC->>Claude: PostToolUse (fire post-hook)
        Claude-->>U: Response
    end
```

### Plugin Install & Load Flow

```mermaid
flowchart LR
    Dev["Developer\ncreates plugin"]
    Marketplace["Plugin Marketplace\n(GitHub repo)"]
    Install["/plugin install\nplugin-name@marketplace"]
    Load["Claude Code loads\non next session"]
    Skills["Skills available\nas /plugin:skill-name"]
    Hooks["Hooks active\nfor all lifecycle events"]
    MCPSrv["MCP servers\nconnect automatically"]

    Dev -->|"git push"| Marketplace
    Marketplace -->|"/plugin install"| Install
    Install -->|"session start"| Load
    Load --> Skills
    Load --> Hooks
    Load --> MCPSrv

    style Dev fill:#0078D4,color:#fff
    style Marketplace fill:#5C2D91,color:#fff
    style Install fill:#D83B01,color:#fff
    style Load fill:#107C10,color:#fff
```

### Decision Tree — Which Mechanism to Use

```mermaid
flowchart TD
    Start(["I want to extend Claude Code"])

    Q1{"Does it need to\nshare/distribute?"}
    Q2{"Does it fire automatically\nwithout user request?"}
    Q3{"Can it block\nan action?"}
    Q4{"Does it require\nfacts/always-on context?"}
    Q5{"Is it a one-time\nrepeatable procedure?"}

    Plugin["Plugin\n(bundle everything)"]
    Hook["Hook\n(event-driven)"]
    SkillAuto["Skill\n(auto-invoked)"]
    CLAUDEMD["CLAUDE.md\n(always loaded)"]
    SkillManual["Skill\n(disable-model-invocation: true)"]

    Start --> Q1
    Q1 -->|"Yes"| Plugin
    Q1 -->|"No"| Q2
    Q2 -->|"Yes"| Q3
    Q3 -->|"Yes — block before"| Hook
    Q3 -->|"No — react after"| Hook
    Q2 -->|"No — user requests it"| Q4
    Q4 -->|"Yes — always needed"| CLAUDEMD
    Q4 -->|"No — sometimes needed"| Q5
    Q5 -->|"Claude decides when"| SkillAuto
    Q5 -->|"I decide when"| SkillManual

    style Plugin fill:#107C10,color:#fff
    style Hook fill:#D83B01,color:#fff
    style SkillAuto fill:#0078D4,color:#fff
    style SkillManual fill:#5C2D91,color:#fff
    style CLAUDEMD fill:#6b7280,color:#fff
```

---

## 7. Comparison Table

| Dimension | Commands (Built-in) | Skills | Hooks | Plugins |
|---|---|---|---|---|
| **What it is** | Fixed Claude Code behavior | Custom instruction file | Automated event handler | Distribution bundle |
| **Created by** | Anthropic only | You | You | You (or community) |
| **Triggered by** | User types `/command` | User or Claude (auto) | Lifecycle events | N/A — contains Skills/Hooks |
| **Can block actions** | No | No | **Yes** (`exit 2`) | Via bundled hooks |
| **Modifiable** | No | Yes | Yes | Yes |
| **Loaded when** | Always available | Description in context; body on invoke | Always active (event-driven) | At session start when installed |
| **Token cost** | Zero | Near-zero until invoked | Zero (runs outside context) | Depends on bundled components |
| **Scoping** | Global | Enterprise/Personal/Project/Plugin | Settings file scope | Marketplace → project/user |
| **External access** | None | None (uses Claude's tools) | Yes (runs shell, HTTP calls) | Via bundled MCP servers |
| **Team-shareable** | N/A | Via `.claude/skills/` in VCS | Via `.claude/settings.json` in VCS | Via marketplace install |
| **Versioned releases** | N/A | No | No | **Yes** (`version` in plugin.json) |
| **Namespaced** | No | No | No | **Yes** (`/plugin:skill`) |
| **Contains** | — | Instructions | Shell/HTTP/LLM logic | Skills+Hooks+MCP+Agents+LSP |
| **Examples** | `/help`, `/compact`, `/model` | `/deploy`, `/pr-summary` | Post-edit linter, rm-rf blocker | `skill-creator`, `mcp-server-dev` |

---

## 8. Code Examples

### Hook — Block Dangerous Bash Commands (PreToolUse)

```bash
#!/bin/bash
# .claude/hooks/block-dangerous.sh
input=$(cat)
command=$(echo "$input" | jq -r '.tool_input.command // ""')

# Block rm -rf with no specific target
if echo "$command" | grep -qE 'rm\s+-rf\s+(/|~|\$HOME|\.)'; then
  echo "Blocked: destructive rm -rf command detected. Use specific paths." >&2
  exit 2
fi

# Block force push to main/master
if echo "$command" | grep -qE 'git push.*--force.*(main|master)'; then
  echo "Blocked: force push to protected branch." >&2
  exit 2
fi

exit 0
```

```json
// .claude/settings.json — wire up the hook
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "command": "${CLAUDE_PROJECT_DIR}/.claude/hooks/block-dangerous.sh"
          }
        ]
      }
    ]
  }
}
```

### Hook — Auto-Lint After File Edits (PostToolUse)

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "${CLAUDE_PROJECT_DIR}/.claude/hooks/lint-on-save.sh"
          }
        ]
      }
    ]
  }
}
```

```bash
#!/bin/bash
# .claude/hooks/lint-on-save.sh
input=$(cat)
file=$(echo "$input" | jq -r '.tool_input.file_path // ""')

# Only lint TypeScript/JavaScript files
if echo "$file" | grep -qE '\.(ts|tsx|js|jsx)$'; then
  npx eslint --fix "$file" 2>/dev/null
fi
exit 0
```

### Hook — Desktop Notification on Stop

```bash
#!/bin/bash
# .claude/hooks/notify-done.sh
input=$(cat)
# macOS notification
osascript -e 'display notification "Claude has finished." with title "Claude Code"'
exit 0
```

```json
{
  "hooks": {
    "Stop": [
      {
        "matcher": "*",
        "hooks": [{ "type": "command", "command": "~/.claude/hooks/notify-done.sh" }]
      }
    ]
  }
}
```

### Hook — Load Environment Variables at Session Start

```bash
#!/bin/bash
# .claude/hooks/load-env.sh
if [ -n "$CLAUDE_ENV_FILE" ] && [ -f ".env.claude" ]; then
  while IFS='=' read -r key value; do
    [[ "$key" =~ ^#.*$ ]] && continue   # skip comments
    [[ -z "$key" ]] && continue
    echo "export $key=$value" >> "$CLAUDE_ENV_FILE"
  done < ".env.claude"
fi
exit 0
```

```json
{
  "hooks": {
    "SessionStart": [
      {
        "matcher": "*",
        "hooks": [{ "type": "command", "command": "${CLAUDE_PROJECT_DIR}/.claude/hooks/load-env.sh" }]
      }
    ]
  }
}
```

### Hook — HTTP Endpoint (Audit Log)

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Bash|Edit|Write",
        "hooks": [
          {
            "type": "http",
            "url": "https://audit.internal.example.com/claude-actions",
            "headers": { "Authorization": "Bearer $AUDIT_TOKEN" },
            "allowedEnvVars": ["AUDIT_TOKEN"]
          }
        ]
      }
    ]
  }
}
```

### Hook — Scoped to a Skill (Frontmatter)

```yaml
---
name: secure-deploy
description: Deploy with security checks enforced
disable-model-invocation: true
hooks:
  PreToolUse:
    - matcher: "Bash"
      hooks:
        - type: command
          command: "${CLAUDE_PROJECT_DIR}/.claude/hooks/prod-guard.sh"
  Stop:
    - matcher: "*"
      hooks:
        - type: command
          command: "${CLAUDE_PROJECT_DIR}/.claude/hooks/notify-done.sh"
---

Deploy $ARGUMENTS to production following the standard checklist...
```

### Plugin — Minimal Structure

```bash
# Create plugin
mkdir -p my-team-plugin/.claude-plugin
mkdir -p my-team-plugin/skills/pr-summary
mkdir -p my-team-plugin/hooks
```

```json
// my-team-plugin/.claude-plugin/plugin.json
{
  "name": "my-team-plugin",
  "description": "Team workflow tools for our project",
  "version": "1.0.0",
  "author": { "name": "Engineering Team" }
}
```

```yaml
# my-team-plugin/skills/pr-summary/SKILL.md
---
description: Summarize the current PR for review. Use when asked about PR changes.
context: fork
agent: Explore
allowed-tools: Bash(gh *)
---

## PR Context
- Diff: !`gh pr diff`
- Files: !`gh pr diff --name-only`
- Comments: !`gh pr view --comments`

Summarize for PR description: what changed, why, risks, test plan.
```

```json
// my-team-plugin/hooks/hooks.json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          { "type": "command", "command": "${CLAUDE_PROJECT_DIR}/.claude/hooks/lint.sh" }
        ]
      }
    ]
  }
}
```

```bash
# Test locally
claude --plugin-dir ./my-team-plugin

# Install from marketplace
# /plugin install my-team-plugin@my-org-marketplace

# Reload after changes (no restart needed)
# /reload-plugins
```

### Plugin — With MCP Server Bundled

```json
// my-team-plugin/.mcp.json
{
  "mcpServers": {
    "internal-api": {
      "type": "http",
      "url": "${INTERNAL_API_URL}/mcp",
      "headers": {
        "Authorization": "Bearer ${INTERNAL_API_KEY}"
      }
    }
  }
}
```

### Migrate Standalone Config to Plugin

```bash
# Step 1: Create plugin structure
mkdir -p my-plugin/.claude-plugin

# Step 2: Write manifest
cat > my-plugin/.claude-plugin/plugin.json << 'EOF'
{
  "name": "my-plugin",
  "description": "Migrated from standalone .claude/",
  "version": "1.0.0"
}
EOF

# Step 3: Copy existing files
cp -r .claude/skills my-plugin/ 2>/dev/null || true
cp -r .claude/commands my-plugin/ 2>/dev/null || true
cp -r .claude/agents my-plugin/ 2>/dev/null || true

# Step 4: Migrate hooks from settings.json to hooks/hooks.json
mkdir -p my-plugin/hooks
# Copy the "hooks" key from .claude/settings.json → my-plugin/hooks/hooks.json

# Step 5: Test
claude --plugin-dir ./my-plugin
```

---

## 9. Configuration Reference

### Hook Configuration Schema

```json
{
  "hooks": {
    "<EventName>": [
      {
        "matcher": "<tool-name or regex>",
        "hooks": [
          {
            "type": "command",
            "command": "<shell command or script path>"
          }
        ]
      }
    ]
  }
}
```

### Hook JSON Response Fields (stdout, exit 0)

| Field | Type | Purpose |
|---|---|---|
| `continue` | bool | Set `false` to stop Claude entirely (with `stopReason`) |
| `stopReason` | string | Message shown when `continue: false` |
| `decision` | string | `"block"` or `"allow"` (for permission hooks) |
| `reason` | string | Explanation for `decision` |
| `systemMessage` | string | Warning/context shown to Claude |
| `additionalContext` | string | Extra context injected into Claude's awareness |
| `terminalSequence` | string | Terminal escape sequence (e.g., desktop notification) |
| `hookSpecificOutput.hookEventName` | string | Identifies which event this output is for |
| `hookSpecificOutput.permissionDecision` | string | `"allow"` or `"deny"` for PreToolUse |
| `hookSpecificOutput.permissionDecisionReason` | string | Reason shown to Claude on deny |

### Plugin Manifest Schema (plugin.json)

| Field | Required | Description |
|---|---|---|
| `name` | Yes | Unique identifier; becomes skill namespace `/name:skill` |
| `description` | Recommended | Shown in marketplace browser |
| `version` | No | If set, users only get updates on version bump; omit for git-SHA versioning |
| `author.name` | No | Attribution |
| `homepage` | No | URL to plugin documentation |
| `repository` | No | Source code URL |
| `license` | No | License identifier (e.g., `"MIT"`) |

### Hook Settings Scopes

| File | Scope | Committed to VCS |
|---|---|---|
| `~/.claude/settings.json` | All your projects | No |
| `.claude/settings.json` | This project | Yes |
| `.claude/settings.local.json` | This project, private | No (gitignored) |
| Managed policy | Organization-wide | Admin-deployed |
| Skill/agent frontmatter `hooks:` | While that skill/agent is active | With the skill file |
| Plugin `hooks/hooks.json` | While plugin is enabled | With plugin |

---

## 10. Best Practices

### Hooks
- ✅ Use `exit 2` (not `exit 1`) to block — exit 1 is non-blocking
- ✅ Write stderr messages that are useful to Claude: it sees them as context
- ✅ Use `${CLAUDE_PROJECT_DIR}` to reference scripts — survives directory changes
- ✅ Use `args: []` exec form for scripts with paths that may contain spaces
- ✅ Scope hooks to specific matchers — don't run expensive scripts on every tool call
- ❌ Don't do heavy computation in `PreToolUse` — it blocks every matching tool call
- ❌ Don't write to stdout on exit 2 — stderr is what Claude sees; stdout is parsed as JSON

### Skills
- ✅ Keep `SKILL.md` under 500 lines — move detail to supporting files
- ✅ Put `disable-model-invocation: true` on anything with side effects
- ✅ Use dynamic context injection `` !`cmd` `` to give Claude live data at invoke time
- ✅ Embed hooks in skill frontmatter to scope enforcement to that workflow only
- ❌ Don't put procedures in CLAUDE.md — they always load; use Skills instead

### Plugins
- ✅ Start with standalone `.claude/` for rapid iteration — convert to plugin when ready to share
- ✅ Set a `version` field when distributing — users only get updates on version bumps
- ✅ Use `${CLAUDE_PLUGIN_ROOT}` for bundled file paths — resolves correctly regardless of install location
- ✅ Keep plugin skills namespaced (`/plugin:skill`) to prevent team conflicts
- ❌ Don't put `skills/`, `agents/`, `hooks/` inside `.claude-plugin/` — only `plugin.json` goes there
- ❌ Don't bundle a plugin if you only need it in one project — standalone config is simpler

### Choosing the Right Mechanism
- ✅ **Hook** when something should happen automatically without user request
- ✅ **Hook** when you need to block an action before it runs
- ✅ **Skill** when you keep pasting the same instructions into chat
- ✅ **Plugin** when you want to share or reuse across projects/teams
- ✅ **CLAUDE.md** only for facts and conventions — never procedures
- ❌ Don't use a Skill where a Hook should enforce — Skills are advice, Hooks are enforcement

---

## 11. Interview Talking Points

### "What is a Hook in Claude Code and how does it differ from a Skill?"

> A Hook is a shell command, HTTP endpoint, or LLM prompt that fires automatically at specific lifecycle events — before a tool runs, after a file is edited, when Claude finishes responding. Hooks don't require user invocation; they're always active for the events they're registered on. A Skill, by contrast, is a `SKILL.md` instruction file that extends what Claude knows and how it responds — it loads when invoked (by user or Claude auto-detection) and injects instructions into the conversation. The critical difference is that **Hooks can block actions** (exit code 2 prevents tool execution), while Skills only influence Claude's behavior by giving it instructions. Use Hooks for enforcement and automation; use Skills for extending knowledge and behavior.

### "How would you prevent Claude Code from running destructive commands like `rm -rf`?"

> You'd implement a `PreToolUse` hook matching the `Bash` tool. The hook receives the full tool input as JSON on stdin, extracts the command field with `jq`, checks it against a blocklist of dangerous patterns (e.g., `rm -rf /`, force push to main), and exits with code `2` if the check fails. Exit code 2 is the blocking exit code — Claude Code surfaces the stderr message to Claude, which then informs the user the command was blocked by policy. Exit code 1 is non-blocking, which is a common mistake. The hook script is referenced in `.claude/settings.json` under the `hooks.PreToolUse` key, scoped to the `Bash` matcher so it doesn't fire for unrelated tool calls.

### "What is a Plugin and when would you create one instead of using standalone `.claude/` configuration?"

> A Plugin is a self-contained directory that bundles Skills, Hooks, MCP servers, Agents, LSP servers, and background monitors together under a single versioned, namespaced package. You use standalone `.claude/` configuration when you're customizing a single project for personal use — you get short skill names like `/deploy` and quick iteration. You create a Plugin when you want to share functionality with teammates or distribute to the community, when you need the same skills across multiple projects, or when you want versioned releases so teammates only receive updates when you bump the version number. The tradeoff is that Plugin skills are namespaced (`/my-plugin:deploy` instead of `/deploy`), which prevents conflicts but adds more typing.

### "How do Hooks in Skill frontmatter differ from Hooks in settings.json?"

> Hooks in `settings.json` are always active for the duration of the session, firing on every matching event regardless of what you're doing. Hooks embedded in Skill or Agent frontmatter are scoped to that component's lifetime — they activate when the Skill or Agent becomes active and clear when it's no longer in context. This scoping is useful for workflows where enforcement should only apply during specific operations. For example, a `/deploy` skill might have a `PreToolUse` hook that enforces production safeguards — you only want those checks running when deploying, not during normal development. Placing the hook in the skill's frontmatter ensures it activates exactly when the skill is invoked and doesn't interfere with other workflows.

### "Walk me through the Plugin distribution flow — how does a developer share a plugin with their team?"

> The developer creates a plugin directory with a `.claude-plugin/plugin.json` manifest and the relevant components — skills in `skills/`, hooks in `hooks/hooks.json`, MCP servers in `.mcp.json`. They test locally with `claude --plugin-dir ./my-plugin`, then commit the directory to a Git repository. To share with the team, they set up a plugin marketplace — a Git repository with a `marketplace.json` index. Team members add the marketplace with `/plugin marketplace add org/repo-name` and install the plugin with `/plugin install plugin-name@marketplace-name`. On the next session, Claude Code fetches the plugin, loads all bundled components, and the team's `/plugin:skill` commands become available. The developer bumps the `version` field in `plugin.json` to push updates that users receive when they run `/plugin update`.

---

## 12. Learning Resources

| Resource | Link | Type |
|---|---|---|
| Skills vs Hooks vs Commands vs Plugins (Video) | [YouTube Shorts](https://www.youtube.com/shorts/1G3uw6lgt8o) | Video |
| Claude Code Hooks Reference | [code.claude.com/docs/en/hooks](https://code.claude.com/docs/en/hooks) | Official Docs |
| Claude Code Plugins Guide | [code.claude.com/docs/en/plugins](https://code.claude.com/docs/en/plugins) | Official Docs |
| Plugins Reference (full spec) | [code.claude.com/docs/en/plugins-reference](https://code.claude.com/docs/en/plugins-reference) | Reference |
| Skills Docs | [code.claude.com/docs/en/slash-commands](https://code.claude.com/docs/en/slash-commands) | Official Docs |
| Commands Reference | [code.claude.com/docs/en/commands](https://code.claude.com/docs/en/commands) | Reference |
| Discover & Install Plugins | [code.claude.com/docs/en/discover-plugins](https://code.claude.com/docs/en/discover-plugins) | Guide |
| Plugin Marketplaces | [code.claude.com/docs/en/plugin-marketplaces](https://code.claude.com/docs/en/plugin-marketplaces) | Guide |
| Subagents | [code.claude.com/docs/en/sub-agents](https://code.claude.com/docs/en/sub-agents) | Official Docs |
| MCP Integration | [code.claude.com/docs/en/mcp](https://code.claude.com/docs/en/mcp) | Official Docs |
| skill-creator Plugin | [github.com/anthropics/claude-plugins-official](https://github.com/anthropics/claude-plugins-official) | GitHub |
| Agent Skills Open Standard | [agentskills.io](https://agentskills.io) | Standard |
| Commands vs MCP vs Skills (companion guide) | `Claude-Code-Commands-vs-MCP-vs-Skills.md` | Local Reference |

---

*Last Updated: July 2026 | Source: Claude Code — Skills vs Hooks vs Commands vs Plugins*

---

## Additional Material from Claude-Code-Commands-vs-MCP-vs-Skills.md

> Unique additions: MCP-server setup.


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

---

## Additional Material from Claude-Code-Hooks-vs-Skills.md

> Unique additions: hooks-vs-skills decision matrix.


> **Source:** [YouTube — Hooks vs Skills in Claude Code](https://www.youtube.com/shorts/PZ_r1svrP1g)
> **Channel/Event:** Claude Code Shorts
> **Topic:** Claude Code, Hooks, Skills, Automation, Extension Mechanisms, Lifecycle Events
> **Key Claim:** Hooks react automatically to lifecycle events; Skills are invoked on-demand — use each for what it's designed for.

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

Claude Code provides two complementary extension mechanisms — **Hooks** and **Skills** — that are often confused because both allow you to extend what Claude does. Hooks are shell commands, HTTP endpoints, or LLM prompts that fire *automatically* at defined lifecycle points (e.g., before a tool runs, when a session starts). Skills are reusable agent capabilities bundled as Markdown files that Claude reads and executes when *invoked* via a slash command or agent call. Choosing between them comes down to one question: should this happen automatically, or on demand?

---

## 2. Problem Statement

### Why the Confusion Exists

Without a clear mental model, developers wire automation in the wrong layer — adding `PostToolUse` hooks for things that should be slash commands, or writing Skills for policies that should be enforced automatically.

| Symptom | Root Cause |
|---|---|
| Policy not enforced — Claude forgets to run the check | Put it in a Skill (invoked), should be a Hook (automatic) |
| Slash command doing too much logic via shell | Should be split into a Hook for side effects + Skill for reasoning |
| Skill runs on every turn uninvited | Put it in a Hook if it needs to be always-on |
| Hook runs too broadly, slowing all tool calls | Need a matcher/`if` filter, or move to a Skill |

> **Key Insight:** "Hooks enforce policy. Skills extend capability. CLAUDE.md provides context. Plugins distribute everything."

---

## 3. Core Concepts

### Hook

A **Hook** is a user-defined handler registered in `settings.json` that Claude Code invokes automatically at a specific lifecycle event. It can block an action (exit 2), inject context, or trigger side effects — all without Claude deciding to run it.

### Skill

A **Skill** is a Markdown file (often with YAML frontmatter) stored in a skills directory that teaches Claude *how* to perform a reusable task. Claude reads the Skill when it matches a slash command or agent routing rule, then executes the described workflow.

### Hook Event

A **Hook Event** is a named lifecycle moment in Claude Code's execution (e.g., `PreToolUse`, `SessionStart`, `Stop`). Hooks are registered per-event.

### Matcher

A **Matcher** is a filter string inside a hook group that controls which tool names (or MCP tool patterns) activate the hook group. Example: `"Bash"` matches only Bash tool calls; `"mcp__.*__write.*"` matches all MCP write tools.

### Exit Code 2

The special exit code that tells Claude Code: *"Block this action entirely."* Any other non-zero exit is a non-blocking error.

---

## 4. Architecture

### Hook Execution Flow

```mermaid
flowchart TD
    U["User Prompt / Tool Call"]
    E["Hook Event Fires\n(e.g. PreToolUse)"]
    M{"Matcher\nmatches?"}
    I{"if condition\npasses?"}
    H["Handler Runs\n(shell / http / mcp / prompt / agent)"]
    R{"Exit Code?"}
    B["Block Action\n(exit 2 + stderr)"]
    C["Continue\n(exit 0 + JSON decisions)"]
    NB["Non-blocking\nerror (exit 1)"]

    U --> E --> M
    M -->|No| C
    M -->|Yes| I
    I -->|No| C
    I -->|Yes| H
    H --> R
    R -->|"2"| B
    R -->|"0"| C
    R -->|"other"| NB

    style U fill:#0078D4,color:#fff
    style B fill:#D83B01,color:#fff
    style C fill:#107C10,color:#fff
    style H fill:#5C2D91,color:#fff
    style E fill:#EFF6FC,stroke:#0078D4
```

### Skill Invocation Flow

```mermaid
flowchart LR
    UI["User types\n/skill-name or\nagent routes to it"]
    L["Claude reads\nSkill .md file"]
    P["Claude interprets\nSkill instructions"]
    T["Claude executes\ntools as described"]
    O["Output returned\nto user"]

    UI --> L --> P --> T --> O

    style UI fill:#0078D4,color:#fff
    style L fill:#5C2D91,color:#fff
    style P fill:#5C2D91,color:#fff
    style T fill:#107C10,color:#fff
    style O fill:#107C10,color:#fff
```

### Hook Scoping Hierarchy

```mermaid
flowchart TD
    ORG["Managed policy settings\n(org-wide, admin-controlled)"]
    USER["~/.claude/settings.json\n(all your projects)"]
    PROJ[".claude/settings.json\n(single project, committable)"]
    LOCAL[".claude/settings.local.json\n(single project, gitignored)"]
    PLUGIN["Plugin hooks/hooks.json\n(when plugin enabled)"]
    SKILL["Skill / agent frontmatter\n(while component active)"]

    ORG --> USER --> PROJ --> LOCAL --> PLUGIN --> SKILL

    style ORG fill:#D83B01,color:#fff
    style USER fill:#0078D4,color:#fff
    style PROJ fill:#5C2D91,color:#fff
    style SKILL fill:#107C10,color:#fff
```

---

## 5. Key Components

| Component | Lives In | Role |
|---|---|---|
| Hook Event | `settings.json` | Named lifecycle trigger point |
| Matcher Group | `settings.json` hooks array | Filters which tool names activate the hook |
| `if` Field | Per-handler | Fine-grained filter using permission rule syntax |
| Handler | `settings.json` | Actual executable — shell command, HTTP, MCP, prompt, or agent |
| Exit Code 2 | Handler stdout/exit | Signals "block this action" |
| `hookSpecificOutput` | JSON stdout | Structured decisions injected back to Claude |
| Skill File | `skills/` directory | Markdown with YAML frontmatter describing the reusable task |
| Skill Frontmatter | Skill `.md` file | Metadata: name, description, trigger conditions, optional embedded hooks |
| Plugin | `.claude/plugins/` | Bundles hooks + skills + config for distribution |

### Hook Handler Types

| Type | Config Field | Use Case |
|---|---|---|
| `command` | `command`, `args` | Shell script; receives JSON via stdin |
| `http` | `url`, `headers` | POST to local/remote service; response = decisions |
| `mcp_tool` | `server`, `tool`, `input` | Call an MCP server tool |
| `prompt` | `prompt`, `model` | Single-turn Claude evaluation returning yes/no |
| `agent` | `prompt`, `model` | Spawns a subagent with Read/Grep/Glob tools |

### Hook Events Reference

| Event | Fires When | Can Block? |
|---|---|---|
| `SessionStart` | Session begins or resumes | No |
| `UserPromptSubmit` | Prompt submitted, before Claude processes | **Yes** |
| `PreToolUse` | Before any tool call executes | **Yes** |
| `PostToolUse` | After tool call succeeds | No |
| `PostToolBatch` | After a parallel tool batch resolves | **Yes** |
| `PermissionRequest` | Permission dialog appears | **Yes** |
| `Stop` | Claude finishes responding | **Yes** |
| `TaskCreated` | Task created | **Yes** |
| `TaskCompleted` | Task marked complete | **Yes** |
| `SubagentStart` | Subagent spawned | No |
| `SubagentStop` | Subagent finished | **Yes** |
| `PreCompact` | Before context compaction | **Yes** |
| `FileChanged` | Watched file changes on disk | No |
| `CwdChanged` | Working directory changes | No |
| `SessionEnd` | Session terminates | No |

---

## 6. How It Works — Step by Step

### Hooks — Lifecycle Sequence

```mermaid
sequenceDiagram
    participant U as User
    participant CC as Claude Code
    participant HE as Hook Engine
    participant H as Hook Handler
    participant CL as Claude LLM

    U->>CC: Submit prompt
    CC->>HE: Fire UserPromptSubmit
    HE->>H: Run matching handlers
    H-->>HE: Exit 0 (JSON context)
    HE-->>CC: Inject additionalContext
    CC->>CL: Process prompt with context
    CL->>CC: Decide tool call (e.g. Bash)
    CC->>HE: Fire PreToolUse
    HE->>H: Run matching handlers
    H-->>HE: Exit 2 (block)
    HE-->>CC: Deny tool call
    CC->>CL: Inform: tool blocked
    CL-->>U: Respond without running tool
```

### Skills — Invocation Sequence

```mermaid
sequenceDiagram
    participant U as User
    participant CC as Claude Code
    participant SF as Skill File
    participant CL as Claude LLM
    participant T as Tools

    U->>CC: /skill-name [args]
    CC->>SF: Read skill .md file
    SF-->>CL: Skill instructions loaded
    CL->>CL: Parse workflow steps
    CL->>T: Execute step 1 (e.g. WebFetch)
    T-->>CL: Result
    CL->>T: Execute step 2 (e.g. Write)
    T-->>CL: Result
    CL-->>U: Final output per skill template
```

### Key Behavioral Difference

1. **Hooks** run whether Claude "thinks to" or not — they're outside Claude's reasoning loop.
2. **Skills** run because Claude *reads* them and follows instructions — they're inside Claude's reasoning loop.
3. This means hooks are the right place for **policy enforcement** (you can't rely on Claude to always follow a rule).
4. Skills are the right place for **complex multi-step workflows** where Claude needs to reason across steps.

---

## 7. Comparison Table

| Dimension | Hooks | Skills |
|---|---|---|
| **Trigger** | Automatic — lifecycle events | On-demand — slash command or agent routing |
| **Runs without Claude deciding** | Yes | No |
| **Can block tool execution** | Yes (exit 2) | No |
| **Access to tool I/O** | Yes (JSON via stdin) | No direct access |
| **Lives in** | `settings.json` | `skills/*.md` files |
| **Best for** | Policy enforcement, side effects, automation | Reusable workflows, complex multi-step tasks |
| **Scope control** | User / project / local / org / plugin / skill | User / project / plugin |
| **Can embed hooks** | N/A | Yes (YAML frontmatter) |
| **Parallel execution** | All matching hooks run in parallel | Sequential steps per skill instructions |
| **Dynamic context injection** | Yes (`additionalContext` in JSON output) | Via tool calls within the skill |
| **Distribution** | Via plugins or settings.json | Via skills directory or plugins |

### Extended: All Four Extension Mechanisms

| Mechanism | Trigger | Blocks? | Context Access | Best For |
|---|---|---|---|---|
| **Hooks** | Automatic lifecycle events | Yes | Full tool I/O via JSON | Policy, automation, CI integration |
| **Skills** | User invocation (`/cmd`) | No | Via tool calls | Reusable complex tasks |
| **CLAUDE.md** | Loaded at session start | No | Static only | Conventions, project rules |
| **Plugins** | When plugin enabled | Via bundled hooks | Via bundled hooks | Team distribution of all above |

---

## 8. Code Examples

### Hook — Block Destructive Commands (PreToolUse + command handler)

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "if": "Bash(rm *)",
            "command": ".claude/hooks/block-rm.sh"
          }
        ]
      }
    ]
  }
}
```

```bash
#!/bin/bash
# block-rm.sh — blocks rm -rf via exit 2
COMMAND=$(jq -r '.tool_input.command' < /dev/stdin)
if echo "$COMMAND" | grep -qE 'rm\s+-rf'; then
  jq -n '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "deny",
      permissionDecisionReason: "Destructive rm -rf blocked by policy"
    }
  }'
  exit 2
fi
exit 0
```

### Hook — Inject Git Context at Session Start

```bash
#!/bin/bash
# session-context.sh — injects branch + dirty state
BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
DIRTY=$(git status --porcelain 2>/dev/null | wc -l | tr -d ' ')
jq -n \
  --arg ctx "Branch: $BRANCH | Uncommitted files: $DIRTY" \
  '{hookSpecificOutput: {hookEventName: "SessionStart", additionalContext: $ctx}}'
```

```json
{
  "hooks": {
    "SessionStart": [
      {
        "hooks": [{ "type": "command", "command": ".claude/hooks/session-context.sh" }]
      }
    ]
  }
}
```

### Hook — HTTP Endpoint (Security Scan on Write)

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Write|Edit",
        "hooks": [
          {
            "type": "http",
            "url": "http://localhost:8080/hooks/security-scan",
            "timeout": 30,
            "headers": { "Authorization": "Bearer $SCAN_TOKEN" },
            "allowedEnvVars": ["SCAN_TOKEN"]
          }
        ]
      }
    ]
  }
}
```

### Hook — MCP Tool on File Change

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Write",
        "hooks": [
          {
            "type": "mcp_tool",
            "server": "my_linter",
            "tool": "lint_file",
            "input": { "path": "${tool_input.file_path}" }
          }
        ]
      }
    ]
  }
}
```

### Skill — Frontmatter with Embedded Hook

```yaml
---
name: secure-deploy
description: Deploy with pre-flight security checks
hooks:
  PreToolUse:
    - matcher: "Bash"
      hooks:
        - type: command
          command: "./scripts/pre-deploy-check.sh"
---

# Skill: Secure Deploy

When invoked, perform these steps:
1. Run `git status` to confirm clean working tree
2. Run tests: `npm test`
3. Build: `npm run build`
4. Deploy: `./scripts/deploy.sh`
5. Verify: curl the health endpoint and confirm 200
```

### Hook JSON Output Patterns

```json
// Block everything and stop the turn
{ "continue": false, "stopReason": "Tests must pass before proceeding" }

// Deny a specific tool call (PreToolUse)
{
  "hookSpecificOutput": {
    "hookEventName": "PreToolUse",
    "permissionDecision": "deny",
    "permissionDecisionReason": "Direct DB writes not allowed in production"
  }
}

// Inject context after a tool call (PostToolUse)
{
  "hookSpecificOutput": {
    "hookEventName": "PostToolUse",
    "additionalContext": "This is auto-generated. Edit src/schema.ts, not the output."
  }
}

// Block the Stop event (force re-run)
{
  "hookSpecificOutput": {
    "hookEventName": "Stop",
    "decision": "block",
    "reason": "Lint errors found. Fix before finishing."
  }
}
```

---

## 9. Configuration Reference

### Hook Handler Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `type` | string | Yes | `command`, `http`, `mcp_tool`, `prompt`, `agent` |
| `command` | string | For `command` | Shell command to run |
| `args` | string[] | No | Explicit args (exec form — no shell) |
| `url` | string | For `http` | HTTP endpoint URL |
| `headers` | object | No | HTTP headers (supports env var substitution) |
| `timeout` | number | No | Timeout in seconds (default varies) |
| `if` | string | No | Permission rule filter (e.g., `"Bash(git *)"`) |
| `server` | string | For `mcp_tool` | MCP server name |
| `tool` | string | For `mcp_tool` | MCP tool name |
| `input` | object | For `mcp_tool` | Tool input (supports `${tool_input.*}` vars) |
| `prompt` | string | For `prompt`/`agent` | Instruction for Claude evaluation |
| `model` | string | No | Model override for prompt/agent handlers |
| `allowedEnvVars` | string[] | No | Env vars the handler is allowed to access |

### Matcher Patterns

| Pattern | Matches |
|---|---|
| `"Bash"` | Only the Bash tool |
| `"Write\|Edit"` | Write or Edit tool |
| `"mcp__memory__.*"` | All memory MCP tools |
| `"mcp__.*__write.*"` | All MCP tools with "write" in the name |
| `""` (empty) | All tools (no filter) |

---

## 10. Best Practices

### When to Use Hooks
- ✅ Enforce policy that must run even if Claude "forgets" (blocking `rm -rf`, enforcing tests before commit)
- ✅ Inject dynamic context at session start (branch name, CI status, environment)
- ✅ Trigger side effects after tool calls (logging, notifications, security scans)
- ✅ Validate state before the turn ends (`Stop` hook for lint/test gate)
- ❌ Don't use hooks for multi-step reasoning workflows — that's what Skills are for
- ❌ Don't write broad matchers without `if` guards — hooks with `matcher: ""` hit every tool call

### When to Use Skills
- ✅ Complex multi-step workflows where Claude needs to reason (e.g., "generate a report from this video URL")
- ✅ Reusable tasks invoked explicitly by the user or agent routing
- ✅ Tasks that need Claude's judgment across steps (not just mechanical execution)
- ❌ Don't use Skills for always-on behavior — it won't run unless invoked
- ❌ Don't put policy enforcement in Skills — Claude might not invoke the skill every time

### Scoping
- ✅ Put team-wide policies in `.claude/settings.json` (committed) or managed org settings
- ✅ Put personal preferences in `~/.claude/settings.json`
- ✅ Put local secrets/tokens in `.claude/settings.local.json` (gitignored)
- ❌ Don't put secrets in committed settings files

### Mermaid & Diagram Safety (for Skills producing diagrams)
- ✅ Quote all node labels with `()`, `&`, `/`, `:`, `%` — use `node["Label (detail)"]`
- ✅ Keep diagrams under 20 nodes — split into sub-diagrams if needed
- ❌ Don't use bare parentheses in node labels — causes render failures

---

## 11. Interview Talking Points

### "What's the difference between a Hook and a Skill in Claude Code?"

> A Hook is registered in `settings.json` and fires automatically at a lifecycle event — like `PreToolUse` or `Stop` — whether Claude decides to run it or not. A Skill is a Markdown instruction file that Claude reads and follows when invoked via a slash command or agent routing. The core distinction is **reactive vs. on-demand**: hooks enforce policy outside Claude's reasoning loop; skills extend Claude's capabilities inside it. Use hooks when you cannot rely on Claude always remembering to do something.

### "When would you use a Hook vs. a CLAUDE.md rule?"

> CLAUDE.md is static text loaded at session start — it gives Claude instructions, but Claude might not follow them 100% of the time, especially under context pressure. A Hook is deterministic: if the event fires and the matcher matches, the hook runs. For rules like "never run `rm -rf`" or "tests must pass before committing," a `PreToolUse` hook with exit code 2 is the right choice because it enforces the rule programmatically, not through Claude's attention. Use CLAUDE.md for conventions and context; use hooks for policy.

### "Can a Skill contain a Hook?"

> Yes. A Skill's YAML frontmatter can declare hooks that are active while the skill is loaded. For example, a `secure-deploy` skill might embed a `PreToolUse` hook that blocks unsafe shell commands. This is useful when a skill brings its own safety constraints — the hook only applies when that skill is active, keeping the default environment clean. Plugins take this further by bundling hooks + skills + config into a distributable unit.

### "How do you block a tool call from a Hook?"

> Exit with code 2 from the hook handler and write a JSON object to stdout with `hookSpecificOutput.permissionDecision: "deny"` and a `permissionDecisionReason`. Claude Code reads this and presents the denial reason to Claude, which then informs the user why the action was blocked. Any other non-zero exit code is a non-blocking error — the tool call still proceeds.

### "How do parallel hooks work?"

> All matching hooks for a given event fire in parallel. If multiple hooks match the same event and tool, they all run concurrently, and their results are merged. Identical handlers are deduplicated automatically. If any hook returns exit code 2 (block), the action is blocked regardless of what other hooks returned. This makes it safe to compose multiple policy hooks without them interfering with each other.

---

## 12. Learning Resources

| Resource | Link | Type |
|---|---|---|
| Claude Code Hooks Docs | [code.claude.com/docs/en/hooks](https://code.claude.com/docs/en/hooks) | Official Docs |
| Hooks vs Skills in Claude Code | [youtube.com/shorts/PZ_r1svrP1g](https://www.youtube.com/shorts/PZ_r1svrP1g) | Video (Short) |
| Skills vs Hooks vs Commands vs Plugins | [youtube.com/shorts/1G3uw6lgt8o](https://www.youtube.com/shorts/1G3uw6lgt8o) | Video (Short) |
| Claude Code Commands vs MCP vs Skills | [Local Reference](./Claude-Code-Commands-vs-MCP-vs-Skills.md) | Local MD |
| Skills vs Hooks vs Commands vs Plugins | [Local Reference](./Claude-Code-Skills-vs-Hooks-vs-Commands-vs-Plugins.md) | Local MD |

---

*Last Updated: July 2026 | Source: Claude Code Shorts — Hooks vs Skills in Claude Code*

---

## Source Attribution

| Section | Primary Source | Additional Sources |
|---|---|---|
| Skills vs hooks vs commands vs plugins | Claude-Code-Skills-vs-Hooks-vs-Commands-vs-Plugins.md | Claude-Code-Commands-vs-MCP-vs-Skills.md, Claude-Code-Hooks-vs-Skills.md |
| MCP-server setup | Claude-Code-Commands-vs-MCP-vs-Skills.md | — |
| Decision matrix | Claude-Code-Hooks-vs-Skills.md | — |

*Consolidated: July 2026 | DocDedupAnalyzer Agent v1.0*
*Original files: Claude-Code-Skills-vs-Hooks-vs-Commands-vs-Plugins.md, Claude-Code-Commands-vs-MCP-vs-Skills.md, Claude-Code-Hooks-vs-Skills.md | Zero data loss guaranteed*
