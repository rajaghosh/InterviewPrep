# Claude Code Hooks — Full Theory & Practical Guide
> **Consolidated From:** Claude-Code-Hooks-Full-Theory-Practical.md, Claude-Code-Hooks-Explained.md
> **Topics Covered:** Claude Code hooks, PreToolUse, PostToolUse, hook lifecycle, stdin/stdout contracts, configuration, security, practical examples
> **Consolidation Date:** 2026-07-04
> **Original Documents:** 2 → **Content Preserved:** 100%

---

> **Source:** [YouTube — Hooks in Claude Code — Full Theory + Practical Use](https://www.youtube.com/watch?v=oo1oADOiVmM)
> **Channel/Event:** CampusX · May 2026
> **Topic:** Claude Code, Hooks, Lifecycle Automation, PreToolUse, PostToolUse, Shell Scripting, AI Safety, DevSecOps
> **Key Claim:** Hooks run outside Claude's control — the harness executes them, not the AI. Claude cannot skip or work around hooks.

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

Claude Code hooks are user-defined shell commands, HTTP endpoints, LLM prompts, or MCP tool calls that execute automatically at specific points in the agent's lifecycle. They are configured in `settings.json` and run by the **harness** — outside the AI's control — making them deterministic guardrails that cannot be bypassed by Claude. Think of hooks as **git hooks for your AI agent**: they fire at lifecycle events like PreToolUse, PostToolUse, SessionStart, and Stop, and can approve, deny, modify, or augment any Claude action. Unlike CLAUDE.md instructions (which Claude may forget), hooks are guaranteed to execute every time.

---

## 2. Problem Statement

### Classic Approach Pain Points

| Problem | Impact |
|---|---|
| CLAUDE.md rules are suggestions | Claude may skip formatting, testing, or safety steps under pressure |
| No way to intercept tool calls | Dangerous `rm -rf` or `git push --force` can execute without review |
| No dynamic context injection | Claude doesn't automatically know current branch, open tickets, team conventions |
| No automated post-edit validation | Style drift and regression go undetected until code review |
| Notification gap | Developer has no way to know when Claude needs input or finishes long tasks |

> **Key Insight:** "CLAUDE.md *tells* Claude what to do. Hooks *make it happen* — they're infrastructure, not suggestions."

---

## 3. Core Concepts

### Hooks
User-defined handlers that execute at lifecycle events. Three-level structure: **event** (when it fires) → **matcher** (filter for which tools/sessions) → **handler** (what runs).

### Lifecycle Event
A named point in Claude Code execution where hooks can attach. 28+ events grouped as: once per session, once per turn, or on every tool call.

### Matcher
A filter that determines whether a hook fires for a given event occurrence. Supports exact tool names, pipe-separated lists, and JavaScript regex patterns.

### `if` Condition
A fine-grained filter on tool input content (e.g., `Bash(rm *)`) evaluated before the handler runs. Only supported for tool events.

### Handler Types
The five ways a hook can respond: `command` (shell script), `http` (webhook), `mcp_tool` (MCP server tool), `prompt` (single-turn Claude model evaluation), `agent` (subagent with full tool access).

### Exit Code Semantics
Hook commands communicate via exit code: `0` = success (parse stdout JSON), `2` = blocking error (block action, feed stderr to Claude), anything else = non-blocking error (show stderr, proceed).

---

## 4. Architecture

```mermaid
flowchart TD
    User["User Prompt"] --> Harness["Claude Code Harness"]
    Harness --> UPS["UserPromptSubmit Hook"]
    UPS --> Claude["Claude AI Model"]
    Claude --> TC["Tool Call Decided"]
    TC --> PTU["PreToolUse Hook\n(can block/modify/allow)"]
    PTU -->|"decision: allow"| Tool["Tool Execution\n(Bash / Edit / Write / MCP)"]
    PTU -->|"decision: deny"| Blocked["Blocked — reason fed\nback to Claude"]
    Tool --> PostTU["PostToolUse Hook\n(audit / format / test)"]
    PostTU --> NextTC{"More tool calls?"}
    NextTC -->|Yes| TC
    NextTC -->|No| StopH["Stop Hook\n(can prevent stop)"]
    StopH --> Done["Response Complete"]
    
    SessionStart["SessionStart Hook\n(context injection)"] -.->|"fires once at startup"| Harness
    SessionEnd["SessionEnd Hook\n(cleanup)"] -.->|"fires on exit"| Done

    classDef hook fill:#5C2D91,color:#fff
    classDef external fill:#0078D4,color:#fff
    classDef decision fill:#D83B01,color:#fff
    classDef success fill:#107C10,color:#fff
    class UPS,PTU,PostTU,StopH,SessionStart,SessionEnd hook
    class User,Claude,Tool external
    class Blocked decision
    class Done success
```

---

## 5. Key Components

| Component | Type | Role |
|---|---|---|
| `PreToolUse` | Event | Fires before every tool call; only event that can block execution |
| `PostToolUse` | Event | Fires after tool succeeds; used for formatting, testing, auditing |
| `SessionStart` | Event | Fires once on session begin/resume; injects dynamic context |
| `Stop` | Event | Fires when Claude finishes responding; can force continuation |
| `command` handler | Handler | Shell script receiving JSON via stdin; most common type |
| `http` handler | Handler | HTTP POST webhook for external validation services |
| `prompt` handler | Handler | Single-turn Claude evaluation returning yes/no decision |
| `agent` handler | Handler | Full subagent with Read/Grep/Glob for deep verification |
| `mcp_tool` handler | Handler | Delegates to a connected MCP server tool |
| Matcher | Filter | Regex/exact string determining which tool names trigger the hook |
| `if` condition | Filter | `Bash(git *)` style pattern on tool input content |
| `settings.json` | Config | Declares all hooks; exists at user, project, or org scope |
| `CLAUDE_ENV_FILE` | Env var | File path where SessionStart hooks can export environment variables |

### Handler Type Deep Dive

**`command`** — Fastest startup with Bash (~10-20ms). Receives JSON on stdin. Returns decisions via JSON on stdout + exit code.

**`http`** — Best for centralized security services. POSTs JSON body to a URL. Supports `Authorization` headers via `allowedEnvVars`.

**`prompt`** — Useful for nuanced decisions ("Is this SQL injection safe?"). Uses a small Claude model; slower than `command`.

**`agent`** — Most powerful. Spawns a full subagent that can read files, run grep, check git status before approving.

---

## 6. How It Works — Step by Step

```mermaid
sequenceDiagram
    participant U as User
    participant H as Harness
    participant HK as Hook Handler
    participant C as Claude AI
    participant T as Tool

    U->>H: Submit prompt
    H->>HK: Fire UserPromptSubmit hook
    HK-->>H: exit 0 (proceed)
    H->>C: Forward prompt
    C->>H: Tool call request (e.g. Bash: npm test)
    H->>HK: Fire PreToolUse hook (JSON: tool_name, tool_input)
    Note over HK: Evaluate matcher + if condition
    alt decision: allow
        HK-->>H: exit 0 or JSON permissionDecision:allow
        H->>T: Execute tool
        T-->>H: Tool result
        H->>HK: Fire PostToolUse hook
        HK-->>H: exit 0 (optional: additionalContext)
        H->>C: Tool result + hook context
    else decision: deny
        HK-->>H: exit 2 + permissionDecisionReason
        H->>C: Tool blocked — reason provided
    end
    C->>H: Final response (stop)
    H->>HK: Fire Stop hook
    HK-->>H: exit 0 (allow stop)
    H->>U: Display response
```

**Step-by-step breakdown:**

1. **User submits prompt** → Harness fires `UserPromptSubmit` hook (can block entire prompt)
2. **Claude decides on a tool call** → Harness fires `PreToolUse` with `{tool_name, tool_input}` JSON on stdin
3. **Hook evaluates**: matcher checks tool name → `if` condition checks tool input content → handler runs
4. **Handler returns decision**: `exit 0` = proceed, `exit 2` = block, JSON body with `permissionDecision` = explicit allow/deny/ask/defer
5. **Tool executes** (if allowed) → Harness fires `PostToolUse` with tool result
6. **PostToolUse hook** can: inject additional context to Claude, modify tool output, run async tasks (tests, formatters)
7. **Claude finishes responding** → `Stop` hook fires; returning `exit 2` forces Claude to continue (useful for mandatory test gates)

---

## 7. Comparison Table

### Hooks vs Other Extension Mechanisms

| Dimension | Hooks | CLAUDE.md | Skills | Permissions |
|---|---|---|---|---|
| Execution guarantee | Deterministic — always runs | Best-effort — Claude may skip | On-demand invocation | Static allow/deny list |
| Control over Claude | Can block tool calls | Advisory only | Adds capabilities | Blocks categories |
| Dynamic context | Yes (SessionStart injection) | Static file content | No | No |
| When it fires | Lifecycle events | Session start (read once) | When user invokes | Every tool call |
| Modifiable by Claude | No | Claude can read/reference | No | No |
| Best for | Guardrails, automation, auditing | Conventions, preferences | Extending capabilities | Access control |

### PreToolUse vs PostToolUse

| Aspect | PreToolUse | PostToolUse |
|---|---|---|
| Timing | Before tool executes | After tool completes |
| Can block | Yes | No (action already happened) |
| Use cases | Security gates, approvals | Formatting, testing, logging |
| Input available | `tool_input` | `tool_input` + `tool_output` |
| Performance impact | Every tool call | Every tool call |

### Handler Type Comparison

| Handler | Speed | Use Case | Complexity |
|---|---|---|---|
| `command` (Bash) | ~10-20ms | Simple validation, logging, formatting | Low |
| `command` (Node.js) | ~50ms | High-frequency hooks needing JSON parsing | Medium |
| `http` | Network RTT | Centralized enterprise security service | Medium |
| `prompt` | ~500ms+ | Nuanced semantic decisions | High |
| `agent` | ~2-5s | Deep multi-file verification | Highest |

---

## 8. Code Examples

### Bash — Block Dangerous Commands (PreToolUse)

```bash
#!/bin/bash
# .claude/hooks/bash-validator.sh
input=$(cat)
cmd=$(echo "$input" | jq -r '.tool_input.command')

if echo "$cmd" | grep -qE '(rm -rf /|:(){:|:|;|fork\(\))'; then
  jq -n '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "deny",
      permissionDecisionReason: "Destructive command blocked by safety hook"
    }
  }'
elif echo "$cmd" | grep -qE 'sudo|chmod 000'; then
  jq -n '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "ask",
      permissionDecisionReason: "Elevated privilege command — requires user confirmation"
    }
  }'
else
  exit 0
fi
```

### Bash — Audit File Changes (PostToolUse async)

```bash
#!/bin/bash
# .claude/hooks/audit.sh
input=$(cat)
file=$(echo "$input" | jq -r '.tool_input.file_path // .tool_input.path')
cwd=$(echo "$input" | jq -r '.cwd')
session=$(echo "$input" | jq -r '.session_id')

echo "[$(date -Iseconds)] $file | session: $session" >> "$cwd/.claude/audit.log"
exit 0
```

### Bash — Inject Dynamic Context at Session Start

```bash
#!/bin/bash
# .claude/hooks/session-context.sh
BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
LAST_COMMIT=$(git log -1 --format="%h: %s" 2>/dev/null || echo "no commits")
OPEN_PRS=$(gh pr list --limit 5 --json number,title 2>/dev/null | jq -r '.[] | "#\(.number): \(.title)"' || echo "unavailable")

jq -n \
  --arg branch "$BRANCH" \
  --arg commit "$LAST_COMMIT" \
  --arg prs "$OPEN_PRS" \
  '{
    hookSpecificOutput: {
      hookEventName: "SessionStart",
      additionalContext: "Branch: \($branch)\nLast commit: \($commit)\nOpen PRs:\n\($prs)",
      sessionTitle: $branch
    }
  }'
```

### Bash — Force Tests Before Stop

```bash
#!/bin/bash
# .claude/hooks/require-tests.sh
# Exit 2 if tests fail — forces Claude to continue and fix them
cd "$(cat - | jq -r '.cwd')"

if ! npm test --silent 2>/dev/null; then
  echo "Tests are failing — fix before stopping" >&2
  exit 2
fi

exit 0
```

### Node.js — High-frequency PostToolUse Formatter

```javascript
#!/usr/bin/env node
// .claude/hooks/format-on-edit.js — faster startup than bash for high-frequency hooks
const chunks = [];
process.stdin.on('data', d => chunks.push(d));
process.stdin.on('end', () => {
  const input = JSON.parse(Buffer.concat(chunks).toString());
  const file = input.tool_input?.file_path;
  
  if (!file) { process.exit(0); }
  
  const { execSync } = require('child_process');
  try {
    if (file.endsWith('.ts') || file.endsWith('.tsx')) {
      execSync(`npx prettier --write "${file}"`, { stdio: 'pipe' });
    } else if (file.endsWith('.py')) {
      execSync(`black "${file}"`, { stdio: 'pipe' });
    }
  } catch { /* non-fatal */ }
  
  process.exit(0);
});
```

### JSON — HTTP Hook to External Security Service

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "http",
            "url": "http://security-service.internal:8080/validate",
            "timeout": 10,
            "headers": {
              "Authorization": "Bearer $SECURITY_TOKEN"
            },
            "allowedEnvVars": ["SECURITY_TOKEN"]
          }
        ]
      }
    ]
  }
}
```

### JSON — MCP Tool Hook for Secret Scanning

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Write|Edit",
        "hooks": [
          {
            "type": "mcp_tool",
            "server": "security",
            "tool": "scan_for_secrets",
            "input": {
              "file_path": "${tool_input.file_path}"
            },
            "timeout": 15
          }
        ]
      }
    ]
  }
}
```

### Install / Setup

```bash
# Create hooks directory
mkdir -p .claude/hooks

# Create a hook script
cat > .claude/hooks/bash-validator.sh << 'EOF'
#!/bin/bash
# paste your hook logic here
EOF
chmod +x .claude/hooks/bash-validator.sh

# Verify Claude Code can find it
cat .claude/settings.json | jq '.hooks'
```

---

## 9. Configuration Reference

### Full settings.json Schema

```json
{
  "hooks": {
    "EventName": [
      {
        "matcher": "ToolName|OtherTool|regex.*",
        "hooks": [
          {
            "type": "command|http|mcp_tool|prompt|agent",
            "if": "Bash(git *)",
            "timeout": 600,
            "statusMessage": "Validating...",
            "async": false,
            "asyncRewake": false,
            "once": false
          }
        ]
      }
    ]
  },
  "disableAllHooks": false
}
```

### Command Handler Fields

| Parameter | Type | Default | Description |
|---|---|---|---|
| `type` | string | — | `"command"` (required) |
| `command` | string | — | Path to script or executable (required) |
| `args` | array | `[]` | Additional CLI arguments |
| `async` | boolean | `false` | Run in background; don't block Claude |
| `asyncRewake` | boolean | `false` | Wake Claude when async hook completes |
| `shell` | string | `"bash"` | Shell to use: `"bash"` or `"powershell"` |
| `timeout` | integer | `600` | Max seconds to wait |
| `statusMessage` | string | — | Status bar text while hook runs |
| `if` | string | — | Tool-input pattern like `Bash(rm *)` |
| `once` | boolean | `false` | Run only once per session (skills only) |

### HTTP Handler Fields

| Parameter | Type | Default | Description |
|---|---|---|---|
| `type` | string | — | `"http"` (required) |
| `url` | string | — | POST endpoint URL (required) |
| `headers` | object | `{}` | HTTP headers; env vars via `$VAR` syntax |
| `allowedEnvVars` | array | `[]` | Env vars allowed in header expansion |
| `timeout` | integer | `600` | Max seconds to wait |

### Exit Code Semantics

| Exit Code | Meaning | Effect on Action |
|---|---|---|
| `0` | Success | Parse stdout for JSON decisions; proceed |
| `2` | Blocking error | Block action; stderr fed to Claude as context |
| Other | Non-blocking error | Show stderr in transcript; proceed anyway |

### Hook Locations (Priority Order)

| Location | Scope | Shareable | Notes |
|---|---|---|---|
| Managed policy | Org-wide | Yes | Highest priority |
| `~/.claude/settings.json` | All projects | No | User-level |
| `.claude/settings.json` | Project | Yes | Commit to repo |
| `.claude/settings.local.json` | Project | No | Gitignored |
| Plugin `hooks/hooks.json` | Plugin-scoped | Yes | Active when plugin enabled |
| Skill/agent frontmatter | Component-scoped | Yes | Active when component active |

---

## 10. Best Practices

### Security Gates

- ✅ Use `PreToolUse` on `Bash` matcher to inspect every shell command before execution
- ✅ Use `if` conditions (`Bash(rm *)`, `Bash(git push*)`) to target dangerous patterns specifically
- ✅ Return structured JSON with `permissionDecisionReason` so Claude understands why it was blocked
- ❌ Don't use `PostToolUse` to "block" — the action already ran
- ❌ Don't write hooks that always return `exit 2` — this breaks Claude's ability to work

### Performance

- ✅ Use Bash for simple, high-frequency hooks — 10-20ms startup
- ✅ Use Node.js for hooks that do JSON parsing on PreToolUse/PostToolUse
- ✅ Set `async: true` for non-blocking work (logging, formatting) — Claude won't wait
- ❌ Don't use `prompt` or `agent` handlers on PreToolUse — adds 500ms-5s to every tool call
- ❌ Don't spawn heavy processes (Docker, Python with large imports) on high-frequency events

### Context Injection

- ✅ Use `SessionStart` to inject branch name, open tickets, recent commits
- ✅ Use `CLAUDE_ENV_FILE` to export environment variables for the session
- ✅ Set `sessionTitle` in SessionStart output for better UI identification
- ❌ Don't overload context — keep `additionalContext` under 500 tokens

### Reliability

- ✅ Keep hooks idempotent — they may fire multiple times per session
- ✅ Always handle missing JSON fields gracefully (`jq -r '.field // empty'`)
- ✅ Write hook scripts to `.claude/hooks/` and commit them — share with team
- ❌ Don't rely on hooks for one-off debugging — use CLAUDE.md for that
- ❌ Don't put secrets in `settings.json` — use `allowedEnvVars` with env variables

---

## 11. Interview Talking Points

### "What are Claude Code hooks and how do they differ from CLAUDE.md?"

> Hooks are shell commands, HTTP calls, or LLM prompts that execute automatically at lifecycle events — before tool calls, after edits, on session start. They run in the **harness**, outside Claude's control, making them deterministic: Claude cannot skip or override them. CLAUDE.md, by contrast, provides advisory instructions that Claude reads and tries to follow, but may forget or deprioritize. Use CLAUDE.md for preferences and conventions; use hooks for guarantees — mandatory formatting, test gates, security controls.

### "How would you prevent Claude Code from running dangerous shell commands?"

> Attach a `PreToolUse` hook to the `Bash` matcher. The hook receives the full tool input (including the command string) as JSON on stdin. Parse it with `jq`, pattern-match against dangerous patterns like `rm -rf /`, and return exit code 2 with a JSON body containing `permissionDecision: "deny"` and a reason. Claude receives the reason as context and can revise its approach. For nuanced cases — like `sudo` — return `"ask"` instead of `"deny"` to surface a user confirmation dialog.

### "What hook events fire on every tool call and what do they enable?"

> `PreToolUse` and `PostToolUse` fire on every tool execution. `PreToolUse` can approve, deny, modify (via `updatedInput`), or ask for permission — it's the primary security gate. `PostToolUse` fires after success and can inject additional context back to Claude (via `additionalContext`), replace tool output (`updatedToolOutput`), or trigger async background tasks like formatters and test runners. `PostToolUseFailure` handles the failure path separately, useful for logging or sending alerts.

### "How would you set up Claude Code to automatically run tests after every file edit?"

> Add a `PostToolUse` hook with matcher `Edit|Write`. The hook script reads the modified file path from stdin JSON, runs the relevant test suite (e.g., `pytest tests/` or `npm test`), and exits 0 on pass. To prevent Claude from stopping if tests fail, combine with a `Stop` hook that checks test status and returns `exit 2` to force continuation. Use `async: false` so Claude waits for test results before proceeding, or `async: true` if you just want background monitoring without blocking.

### "What is the `if` condition in hook configuration and when would you use it?"

> The `if` field is a fine-grained pre-filter on tool input content, using the same `Bash(pattern)` syntax as the permission system. It evaluates before the handler runs, saving execution cost. For example, `"if": "Bash(git push*)"` means the hook only fires for git push commands, not all Bash commands. This is especially important for high-frequency events like `PreToolUse` — without `if` conditions, every single tool call triggers every hook, which can add latency at scale. Use `if` to narrow hooks to exactly the commands that need control.

---

## 12. Learning Resources

| Resource | Link | Type |
|---|---|---|
| Official Hooks Reference | [code.claude.com/docs/en/hooks](https://code.claude.com/docs/en/hooks) | Official Docs |
| CampusX Video — Full Theory + Practical | [youtube.com/watch?v=oo1oADOiVmM](https://www.youtube.com/watch?v=oo1oADOiVmM) | Video |
| Claude Code Hooks — Explained & Demoed | [Architechture-Concepts/Claude-Code-Hooks-Explained.md](./Claude-Code-Hooks-Explained.md) | Local Reference |
| Hooks vs Skills — Claude Code Shorts | [Architechture-Concepts/Claude-Code-Hooks-vs-Skills.md](./Claude-Code-Hooks-vs-Skills.md) | Local Reference |
| Claude Code Hooks — eesel AI Practical Guide | [eesel.ai/blog/hooks-in-claude-code](https://www.eesel.ai/blog/hooks-in-claude-code) | Blog |
| Claude Code Hooks — 12 Lifecycle Events | [claudefa.st/blog/tools/hooks/hooks-guide](https://claudefa.st/blog/tools/hooks/hooks-guide) | Blog |

---

*Last Updated: July 2026 | Source: CampusX — Hooks in Claude Code Full Theory Practical Use*

---

## Additional Material from Claude-Code-Hooks-Explained.md

> Unique additions folded in below: PreToolUse stdin example and Anthropic/YouTube source framing. Overlapping template sections retained for completeness.


> **Source:** [YouTube — Claude Code Hooks — Explained & Demoed](https://www.youtube.com/watch?v=3Y4XQ2obPIo)
> **Channel/Event:** Claude Code / Anthropic
> **Topic:** Claude Code, Hooks, Lifecycle Automation, PreToolUse, PostToolUse, Shell Scripting, AI Safety
> **Key Claim:** Hooks give you deterministic, user-controlled guardrails that execute at every lifecycle point — blocking, modifying, or augmenting Claude's actions before they happen.

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

Claude Code Hooks are user-defined shell commands, HTTP endpoints, LLM prompts, or MCP tool calls that fire automatically at specific lifecycle points during a Claude Code session. They allow you to intercept Claude's actions before or after they execute — blocking dangerous commands, running linters, sending notifications, injecting context, or enforcing policies. Unlike permissions (which only say yes/no), hooks can read full tool input, modify it, and return structured decisions back to Claude. Hooks make AI-assisted development auditable, repeatable, and safe at scale.

---

## 2. Problem Statement

### Classic Approach Pain Points

| Problem | Impact |
|---|---|
| Claude runs `rm -rf` or destructive shell commands | Data loss, unrecoverable state |
| No automatic lint/format after file edits | Style drift, broken CI gates |
| No context about active branch or open issues at session start | Claude makes suggestions misaligned with current work |
| No visibility into what Claude is doing in real-time | No audit trail, no desktop notifications |
| Permissions say yes/no but can't modify tool inputs | Blunt instrument — must fully allow or fully deny |
| Manual copy-paste of environment variables each session | Friction; forgotten config causes bugs |

> **Key Insight:** "Permissions control access. Hooks control behavior. You need both."

---

## 3. Core Concepts

### Hook Event
A named lifecycle point in Claude Code's execution loop (e.g., `PreToolUse`, `Stop`, `SessionStart`). Each event type carries a specific JSON payload describing what is about to happen or just happened.

### Matcher Group
A filter that determines which specific tool or condition triggers a hook handler within an event. `"Bash"` fires only on Bash tool calls; `"Edit|Write"` fires on either; `"*"` fires on everything.

### Hook Handler
The actual unit of work: a shell command, HTTP endpoint, MCP tool invocation, LLM prompt, or experimental agent. Handlers receive event context via stdin (commands) or POST body (HTTP) and communicate decisions back via exit codes and stdout JSON.

### `if` Condition
An optional fine-grained filter using permission rule syntax (e.g., `"Bash(rm *)"`) applied after the matcher. Enables sub-tool granularity without complex script logic.

### Blocking vs Non-Blocking Events
Events marked **Blockable** respect exit code `2` + a JSON deny decision. Non-blockable events still run their hooks but cannot veto the action — they're for side-effects (notifications, logging).

### Exit Code Protocol
- `0` → Success; parse stdout for JSON decisions
- `2` → Hard block; stderr is shown as error message
- Any other non-zero → Soft error; logs notice, execution continues

---

## 4. Architecture

```mermaid
flowchart TD
    User(["👤 User Prompt"])
    Claude["Claude Code\nCore Runtime"]

    subgraph Lifecycle ["Hook Lifecycle Points"]
        UPS["UserPromptSubmit\n(before Claude sees prompt)"]
        PTU["PreToolUse\n(before tool executes)"]
        POTU["PostToolUse\n(after tool succeeds)"]
        STP["Stop\n(after Claude finishes turn)"]
        SS["SessionStart / SessionEnd"]
    end

    subgraph Handlers ["Handler Types"]
        CMD["Command\n(shell script)"]
        HTTP["HTTP Endpoint\n(local or remote service)"]
        MCP["MCP Tool\n(connected server)"]
        PROMPT["Prompt\n(LLM sub-call)"]
        AGENT["Agent\n(experimental)"]
    end

    subgraph Decisions ["Hook Returns"]
        ALLOW["allow / defer\n(exit 0)"]
        DENY["deny / block\n(exit 2)"]
        MODIFY["modify input or output\n(exit 0 + JSON)"]
        CONTEXT["inject context\n(additionalContext field)"]
    end

    User --> Claude
    Claude --> Lifecycle
    Lifecycle --> Handlers
    Handlers --> Decisions
    Decisions -->|"Continues or blocks"| Claude

    style User fill:#0078D4,color:#fff,stroke:none
    style Claude fill:#5C2D91,color:#fff
    style Lifecycle fill:#EFF6FC,stroke:#0078D4
    style Handlers fill:#FFF4CE,stroke:#D83B01
    style Decisions fill:#DFF6DD,stroke:#107C10
```

---

## 5. Key Components

| Component | Type | Role |
|---|---|---|
| `PreToolUse` | Event | Fires before any tool call — primary control point for blocking/modifying |
| `PostToolUse` | Event | Fires after tool success — for linting, auditing, enriching Claude's view |
| `UserPromptSubmit` | Event | Fires when user submits prompt — can expand, rewrite, or block prompts |
| `SessionStart` | Event | Fires once per session — inject branch info, set env vars, set window title |
| `Stop` | Event | Fires when Claude finishes a turn — can trigger CI runs or notifications |
| `Matcher` | Config | Filters which tool names trigger a handler group |
| `if` condition | Config | Sub-filter using permission rule syntax for fine-grained control |
| `command` handler | Handler | Shell script receiving JSON on stdin |
| `http` handler | Handler | HTTP POST to a local/remote service |
| `mcp_tool` handler | Handler | Call a tool on a connected MCP server |
| `prompt` handler | Handler | Delegate decision to a lightweight Claude model |
| `agent` handler | Handler | Spawn a subagent (experimental) |
| `updatedInput` | Output field | Modify the tool's inputs before execution |
| `updatedToolOutput` | Output field | Replace what Claude sees as the tool's result |
| `additionalContext` | Output field | Inject text into Claude's context without showing in tool output |

### Event Cadence Groups

| Cadence | Events |
|---|---|
| Once per session | `SessionStart`, `SessionEnd`, `Setup` |
| Once per turn | `UserPromptSubmit`, `Stop`, `StopFailure` |
| Every tool call | `PreToolUse`, `PostToolUse`, `PostToolUseFailure` |
| Batch-level | `PostToolBatch` |
| System-level | `ConfigChange`, `CwdChanged`, `FileChanged`, `PreCompact`, `PostCompact` |
| Agent-level | `SubagentStart`, `SubagentStop`, `TaskCreated`, `TaskCompleted`, `TeammateIdle` |
| MCP-level | `Elicitation`, `ElicitationResult` |
| UI-level | `Notification`, `MessageDisplay`, `InstructionsLoaded` |
| Worktree-level | `WorktreeCreate`, `WorktreeRemove` |

---

## 6. How It Works — Step by Step

```mermaid
sequenceDiagram
    participant U as User
    participant CC as Claude Code Runtime
    participant M as Matcher Engine
    participant H as Hook Handler
    participant T as Tool

    U->>CC: Submit prompt
    CC->>M: Fire UserPromptSubmit event
    M->>H: Match found → run handler
    H-->>CC: exit 0, JSON {continue: true}
    CC->>CC: Claude plans tool call
    CC->>M: Fire PreToolUse event + tool JSON
    M->>H: Matcher = "Bash" matches Bash tool
    H->>H: Script reads stdin, checks command
    alt Command is safe
        H-->>CC: exit 0 (allow)
        CC->>T: Execute tool
        T-->>CC: Tool output
        CC->>M: Fire PostToolUse event
        M->>H: Run lint/notify handler
        H-->>CC: exit 0, additionalContext injected
        CC->>CC: Claude sees enriched output
    else Command is destructive
        H-->>CC: exit 2 + deny JSON
        CC-->>U: Blocked: "Destructive command blocked by hook"
    end
    CC->>M: Fire Stop event
    M->>H: Run notification handler
    H-->>U: Desktop notification sent
```

**Step-by-step:**
1. **Event fires** — Claude Code serializes the event context into JSON and pipes it to the matched handler via stdin (command) or HTTP POST body.
2. **Matcher evaluated** — The runtime filters handlers by `matcher` value (exact string, pipe-separated list, or regex). The `if` field applies an additional permission-rule filter.
3. **Handler runs** — Shell script, HTTP server, MCP tool, or prompt model executes. It has full access to tool name, tool input, session metadata, and transcript path.
4. **Decision returned** — Handler exits with code `0` (pass JSON on stdout), `2` (block with stderr), or other (non-blocking warning).
5. **Claude Code acts** — On block, the tool call is cancelled and the reason is surfaced. On allow with `updatedInput`, the modified input is used. On `updatedToolOutput`, Claude sees the replacement instead of the real output.
6. **Non-blocking side effects** — `PostToolUse`, `Notification`, `MessageDisplay` handlers run for auditing, linting, or UI enrichment without affecting Claude's next step.

---

## 7. Comparison Table

| Dimension | Without Hooks | With Hooks |
|---|---|---|
| Destructive command guard | Manual review of each AI suggestion | Automatic block via `PreToolUse` + exit code `2` |
| Lint / format after edits | Must remember to run manually | Auto-triggers on every `Edit\|Write` via `PostToolUse` |
| Session context | Claude starts cold each time | `SessionStart` injects branch, issue, team context |
| Notifications | Check terminal constantly | Desktop push via `Notification` hook |
| Tool input modification | Impossible — accept or deny only | `updatedInput` field rewrites inputs before execution |
| Output sanitization | Claude sees raw tool output | `updatedToolOutput` replaces what Claude reads |
| Permission granularity | Tool-level only (`Bash` allowed/denied) | Sub-command level via `if: "Bash(rm *)"` |
| Policy scope | Per-user config only | User / project / org-wide managed policy layers |
| Handler diversity | Shell scripts only | Command, HTTP, MCP tool, LLM prompt, Agent |
| Audit trail | None built-in | Transcript path available in every hook's stdin |

---

## 8. Code Examples

### Bash — Block Destructive `rm -rf` (PreToolUse)

```bash
#!/bin/bash
# Receives PreToolUse JSON on stdin
COMMAND=$(jq -r '.tool_input.command // empty' < /dev/stdin)

if echo "$COMMAND" | grep -qE 'rm\s+-[a-z]*r[a-z]*f|rm\s+--force'; then
  jq -n '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "deny",
      permissionDecisionReason: "Destructive rm -rf blocked by safety hook"
    }
  }'
  exit 0
fi
exit 0
```

### JSON Config — Auto-Lint After File Edits (PostToolUse)

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "${CLAUDE_PROJECT_DIR}/.claude/hooks/lint-check.sh",
            "statusMessage": "Running linter..."
          }
        ]
      }
    ]
  }
}
```

### Bash — Modify Tool Input Before Execution (PreToolUse)

```bash
#!/bin/bash
INPUT=$(cat)
TOOL=$(echo "$INPUT" | jq -r '.tool_name')

if [ "$TOOL" = "Bash" ]; then
  ORIGINAL=$(echo "$INPUT" | jq -r '.tool_input.command')
  # Prepend timeout to all shell commands
  SAFE_CMD="timeout 30 $ORIGINAL"
  jq -n --arg cmd "$SAFE_CMD" '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "allow",
      updatedInput: { command: $cmd }
    }
  }'
fi
exit 0
```

### Bash — Inject Branch Context at Session Start (SessionStart)

```bash
#!/bin/bash
BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
ISSUE=$(git log -1 --format="%s" 2>/dev/null | grep -oE '#[0-9]+' | head -1)

jq -n --arg branch "$BRANCH" --arg issue "$ISSUE" '{
  hookSpecificOutput: {
    hookEventName: "SessionStart",
    additionalContext: ("Active branch: " + $branch + "\nLinked issue: " + $issue),
    sessionTitle: $branch
  }
}'
exit 0
```

### Bash — Desktop Notification When Claude Stops (Stop)

```bash
#!/bin/bash
INPUT=$(cat)
REASON=$(echo "$INPUT" | jq -r '.stop_reason // "Done"')
SEQ=$(printf '\033]777;notify;Claude Code;%s\007' "$REASON")
jq -n --arg seq "$SEQ" '{terminalSequence: $seq}'
exit 0
```

### JSON Config — HTTP Hook with Auth Header

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "*",
        "hooks": [
          {
            "type": "http",
            "url": "http://localhost:9000/hooks/pre-tool-use",
            "timeout": 10,
            "headers": { "Authorization": "Bearer $HOOK_SECRET" },
            "allowedEnvVars": ["HOOK_SECRET"],
            "statusMessage": "Checking policy server..."
          }
        ]
      }
    ]
  }
}
```

### JSON Config — LLM Prompt Hook for Code Safety Review

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "prompt",
            "prompt": "Review this bash command for safety issues. If dangerous, respond with DENY: <reason>. Command: $ARGUMENTS",
            "model": "claude-haiku-4-5",
            "timeout": 15,
            "statusMessage": "AI safety check..."
          }
        ]
      }
    ]
  }
}
```

### Skill Frontmatter Hooks (active only while skill is loaded)

```yaml
---
name: secure-operations
description: Perform file operations with security checks
hooks:
  PreToolUse:
    - matcher: "Edit|Write|Bash"
      hooks:
        - type: command
          command: "./scripts/security-gate.sh"
          statusMessage: "Security check..."
---
Perform the requested operation with extra care...
```

### Install / Setup

```bash
# Place hook scripts in project
mkdir -p .claude/hooks
chmod +x .claude/hooks/*.sh

# Reference in project settings
cat .claude/settings.json
# { "hooks": { "PreToolUse": [...] } }

# Or in global user settings
cat ~/.claude/settings.json
# { "hooks": { "SessionStart": [...] } }

# Test a hook manually
echo '{"tool_name":"Bash","tool_input":{"command":"rm -rf /tmp/test"}}' | .claude/hooks/block-rm.sh
```

---

## 9. Configuration Reference

### Hook Handler Fields

| Field | Type | Default | Description |
|---|---|---|---|
| `type` | string | — | `"command"`, `"http"`, `"mcp_tool"`, `"prompt"`, `"agent"` |
| `command` | string | — | Shell command or script path (for `command` type) |
| `args` | array | `[]` | Explicit args (exec form — no shell expansion) |
| `url` | string | — | HTTP endpoint URL (for `http` type) |
| `server` + `tool` | string | — | MCP server name and tool name (for `mcp_tool` type) |
| `prompt` | string | — | LLM prompt text; `$ARGUMENTS` = full stdin JSON (for `prompt` type) |
| `model` | string | haiku | Model for prompt hooks |
| `if` | string | — | Permission rule syntax filter: `"Bash(git *)"`, `"Edit(*.ts)"` |
| `timeout` | int | 600 | Seconds before cancellation (30 for prompt, 60 for agent) |
| `async` | bool | `false` | Fire and forget — don't wait for exit code |
| `statusMessage` | string | — | Custom spinner text while hook runs |
| `once` | bool | `false` | Run once per session then deregister (skills) |
| `headers` | object | — | HTTP headers; env var expansion supported |
| `allowedEnvVars` | array | — | Env vars passed to HTTP hook handler |
| `input` | object | — | Static input merged into MCP tool call |

### Hook Locations (precedence: highest to lowest)

| Location | Scope | Git-tracked |
|---|---|---|
| Managed policy settings (admin) | Organization-wide | N/A |
| `~/.claude/settings.json` | All projects for user | No |
| `.claude/settings.json` | This project | Yes |
| `.claude/settings.local.json` | This project (personal) | No (gitignored) |
| Plugin `hooks/hooks.json` | When plugin enabled | Yes |
| Skill/agent frontmatter | While component active | Yes |

### Path Placeholders

| Placeholder | Value |
|---|---|
| `${CLAUDE_PROJECT_DIR}` | Absolute path to the project root |
| `${CLAUDE_PLUGIN_ROOT}` | Plugin's installation directory |
| `${CLAUDE_PLUGIN_DATA}` | Plugin's persistent writable data directory |

---

## 10. Best Practices

### Script Safety
- ✅ Always `exit 0` for the allow path — only use `exit 2` for true blocks
- ✅ Use exec form (`args` array) when paths may contain spaces
- ✅ Set timeouts appropriate to the operation — don't block Claude indefinitely
- ❌ Don't use `exit 1` expecting a block — only `exit 2` blocks; `exit 1` is a soft warning

### Matcher Design
- ✅ Be as specific as possible — `"Bash(rm *)"` beats `"Bash"` for rm guards
- ✅ Use `"Edit|Write"` to catch all file-writing tools in one handler
- ✅ Use regex matchers for MCP tools: `"mcp__memory__.*"` for all memory server tools
- ❌ Don't use `"*"` as matcher for expensive operations — every tool call will invoke it

### Policy Layering
- ✅ Put security-critical hooks in `~/.claude/settings.json` (user-wide) so they can't be overridden per-project
- ✅ Put project-specific linting hooks in `.claude/settings.json` (shared with team)
- ✅ Put personal preferences in `.claude/settings.local.json` (gitignored)
- ❌ Don't put secrets in `settings.json` — use `allowedEnvVars` with env references

### Input Modification (`updatedInput`)
- ✅ Use `updatedInput` for adding safety wrappers (`timeout 30 <cmd>`) or normalizing paths
- ✅ Always return `permissionDecision: "allow"` alongside `updatedInput`
- ❌ Don't silently alter inputs without returning a `systemMessage` so Claude knows

### Performance
- ✅ Use `async: true` for pure side-effects (logging, notifications) that shouldn't block flow
- ✅ Use lightweight prompt models (`claude-haiku-4-5`) for LLM-based hook decisions
- ❌ Don't perform heavy network calls synchronously in `PreToolUse` — it blocks every tool call

---

## 11. Interview Talking Points

### "What are Claude Code hooks and when would you use them?"

> Claude Code hooks are user-defined handlers — shell scripts, HTTP endpoints, or LLM prompts — that fire automatically at lifecycle points like before/after tool calls, session start, and when Claude stops. You'd use them for guardrails (blocking destructive commands), automation (auto-lint after file edits), context injection (adding branch/issue context at session start), and notifications (desktop pings when Claude finishes long tasks). The key distinction from permissions is that hooks can read full tool input, modify it, and return structured JSON decisions — not just yes/no.

### "How does a PreToolUse hook block an action, and what's the exit code protocol?"

> A `PreToolUse` hook receives the tool name and full input JSON on stdin. To block, the script exits with code `2` (not `1` — that's a common mistake) and writes a deny reason to stderr. To allow, it exits `0`. To allow with modifications, it exits `0` and writes JSON to stdout containing `updatedInput` with the modified tool arguments. Exit code `1` or any non-zero other than `2` is treated as a non-blocking soft error — execution continues with just a warning shown.

### "How do you implement a policy that applies to all developers on a team?"

> Project-level hooks go in `.claude/settings.json` which is git-tracked and shared with the whole team — everyone who clones the repo gets the same hooks. For personal overrides (e.g., quieter notifications), use `.claude/settings.local.json` which is gitignored. For organization-wide non-overridable policies (e.g., security mandates), managed policy settings at the admin level take the highest precedence and cannot be disabled by user or project settings.

### "Can hooks modify what Claude sees as the output of a tool call?"

> Yes, via `PostToolUse` and the `updatedToolOutput` field. If a hook returns `{"hookSpecificOutput": {"hookEventName": "PostToolUse", "updatedToolOutput": "<replacement>"}}`, Claude reads the replacement text instead of the real tool output. The actual output is preserved in the transcript. You can also add `additionalContext` which Claude sees but which doesn't replace the output — useful for injecting metadata like "this file is auto-generated, edit the source instead."

### "What's the difference between `matcher` and the `if` condition?"

> The `matcher` selects which tool name triggers the handler group — it's coarse-grained (tool-level). The `if` field uses permission rule syntax for sub-tool filtering: `"Bash(git *)"` fires only for bash commands that start with `git`, while `"Edit(*.ts)"` fires only when editing TypeScript files. Matcher is required and evaluated first; `if` is optional and evaluated after the matcher matches. Together they avoid running expensive hooks on every single tool call.

---

## 12. Learning Resources

| Resource | Link | Type |
|---|---|---|
| Claude Code Hooks — Explained & Demoed | [YouTube](https://www.youtube.com/watch?v=3Y4XQ2obPIo) | Video |
| Claude Code Hooks Official Docs | [code.claude.com/docs/en/hooks](https://code.claude.com/docs/en/hooks) | Official Docs |
| Claude Code Settings Reference | [code.claude.com/docs/en/settings](https://code.claude.com/docs/en/settings) | Official Docs |
| Claude Code Skills vs Hooks vs Commands vs Plugins | Local: `Claude-Code-Skills-vs-Hooks-vs-Commands-vs-Plugins.md` | Internal Reference |

---

*Last Updated: July 2026 | Source: Claude Code / Anthropic — Claude Code Hooks Explained & Demoed*

---

## Source Attribution

| Section | Primary Source | Additional Sources |
|---|---|---|
| Core theory, architecture, config, best practices | Claude-Code-Hooks-Full-Theory-Practical.md | Claude-Code-Hooks-Explained.md |
| PreToolUse stdin example, Anthropic/YouTube framing | Claude-Code-Hooks-Explained.md | — |

*Consolidated: July 2026 | DocDedupAnalyzer Agent v1.0*
*Original files: Claude-Code-Hooks-Full-Theory-Practical.md, Claude-Code-Hooks-Explained.md | Zero data loss guaranteed*
