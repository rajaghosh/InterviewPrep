# AI & Architecture Concepts: Complete Reference — Part 2 (.NET)

> Covers: LoRA · A2A Protocol & Agent Cards · AutoGen vs Semantic Kernel · RICEFWID · AI Agent 4-Part Loop · MAE vs MSE vs RMSE · Multi-Step LLM Workflows

---

## Table of Contents

7. [LoRA — Low-Rank Adaptation](#7-lora--low-rank-adaptation)
8. [A2A Protocol & Agent Cards](#8-a2a-protocol--agent-cards)
9. [AutoGen vs Semantic Kernel](#9-autogen-vs-semantic-kernel)
10. [RICEFWID Framework](#10-ricefwid-framework)
11. [AI Agent 4-Part Loop](#11-ai-agent-4-part-loop)
12. [MAE vs MSE vs RMSE — Error Regression Metrics](#12-mae-vs-mse-vs-rmse--error-regression-metrics)
13. [Multi-Step LLM Workflows](#13-multi-step-llm-workflows)

---

## 7. LoRA — Low-Rank Adaptation

### Overview

LoRA (Hu et al., 2021) solves the cost problem of full fine-tuning. Fine-tuning a 70B parameter model requires updating all 70B weights — this demands enormous GPU memory and compute. LoRA freezes all original model weights and injects small trainable **adapter matrices** into specific layers. The update to a weight matrix W is approximated as `ΔW = B × A`, where B (d×r) and A (r×k) are low-rank matrices with rank r ≪ min(d, k). Only A and B are trained — millions of parameters instead of billions. At inference, LoRA adapters can be merged into the base model (zero latency overhead) or swapped dynamically for multi-tenant serving.

---

### LoRA Architecture

```mermaid
flowchart LR
    INPUT(["Input x"]) --> FROZEN["Frozen Weight Matrix W\n(d × k)\n— NEVER updated —"]
    INPUT --> A["Adapter Matrix A\n(r × k) — trainable\nr = rank (e.g. 8, 16, 64)"]

    FROZEN --> ADD["Add outputs\nW·x + B·A·x × α/r"]
    A --> B_MAT["Adapter Matrix B\n(d × r) — trainable\nInitialised to zero"]
    B_MAT --> ADD
    ADD --> OUT(["Output h"])

    NOTE["Total trainable params:\nd×r + r×k\n(e.g. 4096×16 + 16×4096 = 131K\nvs 4096×4096 = 16.7M for full layer)"]

    style INPUT fill:#0f172a,color:#fff
    style OUT fill:#22c55e,color:#fff
    style FROZEN fill:#1e40af,color:#fff
    style A fill:#ef4444,color:#fff
    style B_MAT fill:#ef4444,color:#fff
    style ADD fill:#8b5cf6,color:#fff
    style NOTE fill:#f59e0b,color:#fff
```

---

### LoRA vs Full Fine-Tuning vs Prompt Tuning

```mermaid
flowchart TD
    GOAL(["Adapt LLM to new task"]) --> Q1{"Compute budget?"}

    Q1 -->|"Very limited\n(1–2 GPUs)"| PROMPT_T["Prompt Tuning / Prefix Tuning\nAdd trainable soft tokens\nto prompt prefix\nNo weight changes at all\nWeakest adaptation"]

    Q1 -->|"Moderate\n(4–8 GPUs)"| LORA["LoRA / QLoRA\nLow-rank adapter matrices\nFreeze base, train adapters\n~1% of base params\nStrong adaptation"]

    Q1 -->|"Large cluster\n(100+ GPUs)"| FULL["Full Fine-Tuning\nUpdate all weights\nBest quality\nHighest cost\nRisk of catastrophic forgetting"]

    LORA --> QLORA["QLoRA extension:\nQuantize base to 4-bit (NF4)\nRun adapters in bf16\nFit 70B model on 48GB GPU"]

    style PROMPT_T fill:#f59e0b,color:#fff
    style LORA fill:#22c55e,color:#fff
    style FULL fill:#0078D4,color:#fff
    style QLORA fill:#22c55e,color:#fff
    style GOAL fill:#0f172a,color:#fff
```

---

### LoRA Fine-Tuning Pipeline

```mermaid
sequenceDiagram
    participant DATA as Training Data
    participant BASE as Base Model (Frozen)
    participant ADP as LoRA Adapters (Trainable)
    participant MERGE as Merged Model
    participant PROD as Production

    DATA->>ADP: Forward pass: input → base(x) + B·A·x
    ADP->>ADP: Compute loss (cross-entropy on fine-tune dataset)
    ADP->>ADP: Backprop: update only A and B matrices
    Note over BASE: Base weights: NEVER updated

    loop Training steps
        DATA->>ADP: Next batch
        ADP->>ADP: Gradient update on A, B
    end

    ADP->>MERGE: W_merged = W_base + B·A (optional merge for zero inference overhead)
    MERGE->>PROD: Deploy merged model (same latency as base)
    Note over PROD: Or serve base + adapter separately\nfor hot-swappable multi-tenant LoRA
```

---

### Calling Azure AI Foundry Fine-Tuned Model from .NET

```csharp
// Azure AI Foundry hosts fine-tuned models (including LoRA-adapted) as deployments
// Once deployed, they are called identically to base models

public class FineTunedModelService(IConfiguration config)
{
    private readonly ChatClient _client = new AzureOpenAIClient(
        endpoint: new Uri(config["AzureOpenAI:Endpoint"]!),
        credential: new DefaultAzureCredential()
    ).GetChatClient(
        deploymentName: config["AzureOpenAI:FineTunedDeployment"]!  // e.g. "contoso-gpt4o-lora-v2"
    );

    public async Task<string> ClassifyAsync(string text, CancellationToken ct = default)
    {
        // Fine-tuned model has been LoRA-adapted to output a specific JSON schema
        var completion = await _client.CompleteChatAsync(
            messages: [new UserChatMessage(text)],
            options: new ChatCompletionOptions
            {
                Temperature = 0.0f,    // deterministic for classification tasks
                MaxOutputTokenCount = 100,
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            },
            cancellationToken: ct
        );
        return completion.Value.Content[0].Text;
    }
}
```

---

### Interview Talking Points — LoRA

| Question | Answer |
|---|---|
| Why does LoRA work? | The hypothesis (supported empirically) is that fine-tuning changes in weight matrices have a low intrinsic rank — the meaningful adaptation lives in a low-dimensional subspace. LoRA constrains updates to that subspace explicitly, discarding the rest with minimal quality loss. |
| What is the rank hyperparameter r? | The rank of the adapter matrices. Low r (4–8): very few parameters, fast training, less capacity. High r (64–128): more parameters, slower training, higher quality adaptation. Common default is r=16. There is no universal optimal; tune on a validation set. |
| What is QLoRA? | LoRA applied to a 4-bit quantized (NF4) base model. Quantization reduces VRAM by 4x (float32 → 4-bit int), enabling 70B models to fit on a 48GB A100. Adapters still run in bf16. Combined with paged optimizers for memory efficiency. |
| When does LoRA underperform full fine-tuning? | On tasks requiring dramatic behavior change (e.g., adapting an English-only model for fluent Chinese generation). LoRA adapts efficiently within the model's existing representational space — it cannot teach capabilities the base model entirely lacks. |
| Can multiple LoRA adapters be combined? | Yes — LoRA-Merge and LoRAHub allow linear combination of multiple adapter sets. You can blend a "coding" adapter and a "formal-tone" adapter. The merged model inherits both adaptations proportionally. |
| What is catastrophic forgetting and does LoRA prevent it? | Catastrophic forgetting is when fine-tuning overwrites previously learned capabilities. LoRA significantly reduces this risk because the frozen base weights retain all pretraining knowledge — only the small adapters change. |

---

## 8. A2A Protocol & Agent Cards

### Overview

A2A (Agent-to-Agent) is an open protocol specification (Google, 2025) for how AI agents discover, authenticate, and communicate with each other. As multi-agent systems proliferate, agents from different vendors and frameworks need a standard contract. A2A defines the **Agent Card** — a JSON metadata file (served at `/.well-known/agent.json`) that describes an agent's identity, capabilities, input/output schemas, and endpoint URL. Agents use Agent Cards to decide if another agent can help with a sub-task, then use the A2A protocol to delegate work, stream responses, and exchange structured results.

---

### A2A Discovery and Communication Flow

```mermaid
sequenceDiagram
    participant O as Orchestrator Agent
    participant R as Agent Registry
    participant A as Specialist Agent
    participant AC as Agent Card (.well-known/agent.json)

    O->>R: "I need an agent that can: process invoices"
    R-->>O: Candidate: https://invoice-agent.contoso.com

    O->>AC: GET https://invoice-agent.contoso.com/.well-known/agent.json
    AC-->>O: Agent Card (capabilities, schemas, auth requirements)

    O->>O: Verify: capabilities match, auth scheme supported
    O->>A: POST /tasks/send (A2A task envelope)
    Note over O,A: Includes: task_id, message, context, callback_url

    A-->>O: 202 Accepted + task_id
    A->>A: Process task asynchronously

    A->>O: POST callback_url (streaming updates or final result)
    O-->>O: Incorporate result into main plan
```

---

### Agent Card Structure

```mermaid
flowchart TD
    CARD["Agent Card\n(/.well-known/agent.json)"] --> ID["Identity\nname, version, description\ncontact, provider"]
    CARD --> CAP["Capabilities\nwhat this agent can do\nskills list"]
    CARD --> IN["Input Schema\nJSON Schema for accepted tasks\ncontent types"]
    CARD --> OUT["Output Schema\nJSON Schema for returned results"]
    CARD --> AUTH["Authentication\nOAuth2, API key, mTLS\ntoken endpoint"]
    CARD --> EP["Endpoints\ntasks/send, tasks/get\nsubscriptions/create"]

    style CARD fill:#0f172a,color:#fff
    style ID fill:#0078D4,color:#fff
    style CAP fill:#22c55e,color:#fff
    style IN fill:#8b5cf6,color:#fff
    style OUT fill:#8b5cf6,color:#fff
    style AUTH fill:#ef4444,color:#fff
    style EP fill:#1e40af,color:#fff
```

---

### Component — Exposing an A2A-Compatible Agent in ASP.NET Core

```csharp
// Agent Card model
public record AgentCard(
    string Name,
    string Version,
    string Description,
    string[] Capabilities,
    AgentEndpoints Endpoints,
    AgentAuth Auth
);

public record AgentEndpoints(string TasksSend, string TasksGet);
public record AgentAuth(string Scheme, string TokenEndpoint);

// Expose the agent card at the well-known URL
app.MapGet("/.well-known/agent.json", (IConfiguration config) =>
    Results.Ok(new AgentCard(
        Name: "InvoiceProcessingAgent",
        Version: "2.1.0",
        Description: "Extracts, validates, and routes invoice data from PDF, image, or structured text.",
        Capabilities: ["extract-invoice", "validate-invoice", "route-for-approval"],
        Endpoints: new AgentEndpoints(
            TasksSend: $"{config["BaseUrl"]}/tasks/send",
            TasksGet:  $"{config["BaseUrl"]}/tasks/{{taskId}}"
        ),
        Auth: new AgentAuth("oauth2", $"{config["Auth:TokenEndpoint"]}")
    ))
);

// A2A task envelope
public record A2ATaskEnvelope(
    string TaskId,
    string Skill,
    string Message,
    Dictionary<string, object>? Context,
    string? CallbackUrl
);

public record A2ATaskResult(
    string TaskId,
    string Status,   // "completed" | "failed" | "in_progress"
    object? Result,
    string? Error
);

// Task handler endpoint
app.MapPost("/tasks/send", async (
    A2ATaskEnvelope task,
    IInvoiceAgentService agent,
    ITaskStore taskStore,
    CancellationToken ct) =>
{
    await taskStore.CreateAsync(task.TaskId, "in_progress", ct);

    // Process asynchronously; callback when done
    _ = Task.Run(async () =>
    {
        try
        {
            var result = await agent.ProcessAsync(task.Message, task.Skill, ct);
            await taskStore.UpdateAsync(task.TaskId, "completed", result, ct);

            if (task.CallbackUrl is not null)
                await NotifyCallbackAsync(task.CallbackUrl,
                    new A2ATaskResult(task.TaskId, "completed", result, null), ct);
        }
        catch (Exception ex)
        {
            await taskStore.UpdateAsync(task.TaskId, "failed", null, ct);
            if (task.CallbackUrl is not null)
                await NotifyCallbackAsync(task.CallbackUrl,
                    new A2ATaskResult(task.TaskId, "failed", null, ex.Message), ct);
        }
    }, ct);

    return Results.Accepted($"/tasks/{task.TaskId}", new { taskId = task.TaskId });
});
```

---

### Component — Orchestrator Discovering and Calling an A2A Agent

```csharp
public class A2AAgentClient(HttpClient http, ILogger<A2AAgentClient> logger)
{
    public async Task<AgentCard> DiscoverAsync(string agentBaseUrl, CancellationToken ct = default)
    {
        var card = await http.GetFromJsonAsync<AgentCard>(
            $"{agentBaseUrl}/.well-known/agent.json", ct)
            ?? throw new InvalidOperationException("Agent card not found");

        logger.LogInformation("Discovered agent {Name} v{Version} with {Count} capabilities",
            card.Name, card.Version, card.Capabilities.Length);
        return card;
    }

    public async Task<string> DelegateTaskAsync(
        AgentCard card,
        string skill,
        string message,
        string callbackUrl,
        CancellationToken ct = default)
    {
        var taskId = Guid.NewGuid().ToString("N");
        var envelope = new A2ATaskEnvelope(taskId, skill, message, null, callbackUrl);

        var response = await http.PostAsJsonAsync(card.Endpoints.TasksSend, envelope, ct);
        response.EnsureSuccessStatusCode();

        logger.LogInformation("Delegated task {TaskId} to {Agent}", taskId, card.Name);
        return taskId;
    }
}
```

---

### Interview Talking Points — A2A

| Question | Answer |
|---|---|
| What problem does A2A solve? | Interoperability between agents from different vendors. Without A2A, a Semantic Kernel agent cannot easily delegate work to a LangGraph agent or a CrewAI agent. A2A provides a common discovery and communication contract — like REST for APIs but for agents. |
| What is an Agent Card and what must it contain? | A JSON file at `/.well-known/agent.json` describing: agent identity (name, version), capabilities (skill list), input/output schemas, endpoint URLs, and authentication requirements. Think of it as an OpenAPI spec for an agent. |
| How does A2A handle authentication between agents? | The Agent Card declares the auth scheme (OAuth2, API key, mTLS). The orchestrator fetches a bearer token from the declared token endpoint before calling `tasks/send`. In Azure, Managed Identity + Entra ID federated tokens is the preferred pattern. |
| What is the difference between A2A and MCP? | **MCP** (Model Context Protocol) is about connecting a *single agent to tools/resources* (databases, APIs, file systems). **A2A** is about *agent-to-agent delegation* — one agent orchestrating another agent. MCP is vertical (agent→tool); A2A is horizontal (agent→agent). |
| How does A2A support long-running tasks? | The `tasks/send` call returns 202 Accepted immediately. The orchestrator provides a `callbackUrl`. When the specialist agent completes, it POSTs the result to the callback URL. The orchestrator can also poll `tasks/{taskId}` for status. |

---

## 9. AutoGen vs Semantic Kernel

### Overview

Both are Microsoft frameworks for building multi-agent AI systems in .NET, but they solve different problems. **AutoGen** is a conversation-centric framework — agents are defined by their role and system prompt, communicate via chat messages in a shared conversation thread, and collaborate through natural language. It excels at flexible, exploratory, and research-style multi-agent workflows. **Semantic Kernel** is a plugin/tool-centric framework built for structured enterprise workflows — agents are equipped with typed plugins (functions with schemas), plan and invoke them via a kernel, and integrate deeply with Azure services. Most production enterprise AI in .NET uses Semantic Kernel.

---

### Architecture Comparison

```mermaid
flowchart LR
    subgraph AutoGen ["AutoGen — Conversation-Centric"]
        direction TB
        AG_USER["UserProxyAgent\n(Human or Test Harness)"]
        AG_ASST["AssistantAgent\n(LLM-backed)"]
        AG_CODE["CodeExecutionAgent\n(Runs code, returns output)"]
        AG_USER <-->|"Chat messages\nin GroupChat"| AG_ASST
        AG_ASST <-->|"Code block\nexchange"| AG_CODE
        AG_GC["GroupChatManager\n(Routes messages, ends conversation)"]
        AG_GC --> AG_USER
        AG_GC --> AG_ASST
        AG_GC --> AG_CODE
    end

    subgraph SK ["Semantic Kernel — Plugin-Centric"]
        direction TB
        SK_KERNEL["Kernel\n(DI container for AI + plugins)"]
        SK_PLUGIN["Plugins\n(Typed KernelFunctions\nwith JSON schemas)"]
        SK_AGENT["ChatCompletionAgent\n(Semantic Kernel Agent)"]
        SK_PLAN["Planner / FunctionChoiceBehavior\n(Auto tool selection)"]
        SK_KERNEL --> SK_PLUGIN
        SK_KERNEL --> SK_AGENT
        SK_AGENT --> SK_PLAN
        SK_PLAN --> SK_PLUGIN
    end

    style AG_USER fill:#0078D4,color:#fff
    style AG_ASST fill:#8b5cf6,color:#fff
    style AG_CODE fill:#22c55e,color:#fff
    style AG_GC fill:#0f172a,color:#fff
    style SK_KERNEL fill:#0f172a,color:#fff
    style SK_PLUGIN fill:#22c55e,color:#fff
    style SK_AGENT fill:#8b5cf6,color:#fff
    style SK_PLAN fill:#f59e0b,color:#fff
```

---

### When to Use Each

```mermaid
flowchart TD
    NEED(["Multi-Agent Need"]) --> Q1{"Is the workflow\nstructure known\nahead of time?"}

    Q1 -->|"Yes — structured\nenterprise pipeline"| SK_PATH["Use Semantic Kernel\nPlugin-based, typed tools\nStrong Azure integration\nProduction-grade telemetry"]

    Q1 -->|"No — exploratory,\ndynamic collaboration"| AG_PATH["Use AutoGen\nChat-based, role-playing\nFlexible group conversations\nGood for R&D and prototyping"]

    SK_PATH --> SK_EX["Examples:\nRAG pipeline, order processing\nCode review workflow\nDocument approval chain"]

    AG_PATH --> AG_EX["Examples:\nResearch synthesis with multiple experts\nCode generation + critique + test\nAutonomous problem-solving loops"]

    style NEED fill:#0f172a,color:#fff
    style SK_PATH fill:#0078D4,color:#fff
    style AG_PATH fill:#8b5cf6,color:#fff
    style SK_EX fill:#1e40af,color:#fff
    style AG_EX fill:#1e40af,color:#fff
```

---

### Feature Comparison Table

| Feature | AutoGen | Semantic Kernel |
|---|---|---|
| **Agent communication** | Chat message exchange (GroupChat) | Kernel function calls, typed plugins |
| **Planning** | Conversation-driven, ad-hoc | Planner (sequential, stepwise, auto) |
| **Tool/function integration** | Code execution, custom agent functions | `[KernelFunction]` with JSON schema |
| **Azure integration** | Basic | Deep (AI Foundry, AI Search, App Config) |
| **Memory** | Simple in-conversation history | Semantic Kernel Memory, external stores |
| **Human-in-loop** | `UserProxyAgent` with `human_input_mode` | `IAutoFunctionInvocationFilter` approval gate |
| **Primary language** | Python-first; .NET port available | .NET-first; Python port available |
| **Best for** | Research, exploration, code-gen + test | Enterprise workflows, production agents |
| **Telemetry** | Basic logging | Full OpenTelemetry + Azure Monitor |

---

### Semantic Kernel — Multi-Agent GroupChat (.NET)

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;

public class CodeReviewOrchestrator(Kernel kernel)
{
    public async Task<string> ReviewCodeAsync(string code, CancellationToken ct = default)
    {
        var author = new ChatCompletionAgent
        {
            Name = "CodeAuthor",
            Instructions = "You are a .NET developer. Write clean, production-ready C# code.",
            Kernel = kernel.Clone()
        };

        var reviewer = new ChatCompletionAgent
        {
            Name = "CodeReviewer",
            Instructions = """
                You are a senior .NET architect. Review code for:
                - Security vulnerabilities
                - Performance issues
                - Missing CancellationToken propagation
                - Poor naming or missing null checks
                Reply with "APPROVED" when satisfied, otherwise list issues.
                """,
            Kernel = kernel.Clone()
        };

        var chat = new AgentGroupChat(author, reviewer)
        {
            ExecutionSettings = new AgentGroupChatSettings
            {
                TerminationStrategy = new ApprovalTerminationStrategy(),
                SelectionStrategy = new SequentialSelectionStrategy()
            }
        };

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User,
            $"Review this code:\n```csharp\n{code}\n```"));

        var result = new List<string>();
        await foreach (var response in chat.InvokeAsync(ct))
            result.Add($"[{response.AuthorName}]: {response.Content}");

        return string.Join("\n\n", result);
    }
}

// Terminate when reviewer says APPROVED
public class ApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(
        Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken ct)
    {
        var last = history.LastOrDefault()?.Content ?? string.Empty;
        return Task.FromResult(last.Contains("APPROVED", StringComparison.OrdinalIgnoreCase));
    }
}
```

---

### Interview Talking Points — AutoGen vs Semantic Kernel

| Question | Answer |
|---|---|
| Core architectural difference? | AutoGen: agents collaborate via chat messages in a shared conversation — it's communication-first. Semantic Kernel: agents invoke typed plugin functions via a kernel — it's function-calling-first. Choose based on whether your workflow is dynamic conversation or structured execution. |
| Can they be used together? | Yes. A Semantic Kernel agent can be one participant in an AutoGen GroupChat. Alternatively, AutoGen agents can be wrapped as Semantic Kernel plugins. Microsoft is converging both under the Agent SDK umbrella. |
| How does Semantic Kernel handle agent termination? | Via `TerminationStrategy` — a custom class that inspects conversation history and returns `true` to stop. Common triggers: a specific keyword ("APPROVED"), a function call completing, a max turn limit, or a condition on the last message's content. |
| What is the Semantic Kernel Planner? | A system that takes a user goal and automatically selects and sequences the right kernel functions to achieve it. `FunctionChoiceBehavior.Auto()` is the most common setting — lets the LLM decide which tools to call in which order via OpenAI's function calling protocol. |
| What is an `AgentGroupChat` in Semantic Kernel? | A coordination primitive that manages multiple `ChatCompletionAgent` instances in a shared conversation. Agents take turns responding (based on `SelectionStrategy`) until `TerminationStrategy` signals completion. |

---

## 10. RICEFWID Framework

### Overview

RICEFWID is a structured evaluation framework for assessing whether an AI agent has what it needs to execute a given task autonomously. It stands for **Resources, Intent, Capabilities, Experience, Features, Workflows, Inputs, Data**. Originally a mental model for product managers evaluating AI automation feasibility, it is now used by AI architects to design agents and identify gaps — what does the agent have, and what is missing before it can reliably perform at production scale?

---

### RICEFWID Components

```mermaid
flowchart TD
    AGENT(["AI Agent Under\nEvaluation"]) --> R
    AGENT --> I
    AGENT --> C
    AGENT --> E
    AGENT --> F
    AGENT --> W
    AGENT --> IN
    AGENT --> D

    R["R — Resources\nCompute, memory, API quotas\nTool access, credentials\nBudget constraints"]
    I["I — Intent\nClear goal definition\nSuccess criteria\nTermination conditions"]
    C["C — Capabilities\nWhat skills/tools the agent has\nWhat it cannot do\nKnown failure modes"]
    E["E — Experience\nFew-shot examples\nPrior task memory\nFine-tuning on domain"]
    F["F — Features\nPrompt features: system prompt quality\nContext window management\nOutput format constraints"]
    W["W — Workflows\nStep sequence definition\nBranching / fallback logic\nHuman handoff triggers"]
    IN["I — Inputs\nInput format and schema\nValidation rules\nEdge case handling"]
    D["D — Data\nAccess to required data\nData freshness / accuracy\nPrivacy / access controls"]

    style AGENT fill:#0f172a,color:#fff
    style R fill:#0078D4,color:#fff
    style I fill:#22c55e,color:#fff
    style C fill:#8b5cf6,color:#fff
    style E fill:#f59e0b,color:#fff
    style F fill:#0078D4,color:#fff
    style W fill:#22c55e,color:#fff
    style IN fill:#8b5cf6,color:#fff
    style D fill:#1e40af,color:#fff
```

---

### RICEFWID Evaluation Checklist

```mermaid
flowchart TD
    START(["Agent Design Review"]) --> RICE

    subgraph RICE ["RICEFWID Scorecard"]
        R_Q{"Resources:\nAre compute + API limits\nadequate for expected load?"}
        I_Q{"Intent:\nIs success measurable\nand unambiguous?"}
        C_Q{"Capabilities:\nCan existing tools cover\nall required actions?"}
        E_Q{"Experience:\nAre few-shot examples\nor fine-tuning in place?"}
        F_Q{"Features:\nIs the system prompt\nprecise and tested?"}
        W_Q{"Workflows:\nAre all branches and\nfallbacks defined?"}
        IN_Q{"Inputs:\nAre all input formats\nvalidated and handled?"}
        D_Q{"Data:\nIs required data accessible\nand fresh enough?"}
    end

    RICE --> SCORE{"All 8 pass?"}
    SCORE -->|"Yes"| PROD(["Ready for production"])
    SCORE -->|"No"| GAP["Document gaps\nresolve before deployment"]

    style START fill:#0f172a,color:#fff
    style PROD fill:#22c55e,color:#fff
    style GAP fill:#ef4444,color:#fff
```

---

### Applying RICEFWID — Example: Invoice Processing Agent

| Dimension | Question | Example Gap | Resolution |
|---|---|---|---|
| **Resources** | Does the agent have API quota for peak load? | Azure OpenAI TPM limit too low for month-end invoice surge | Request quota increase; implement queue + rate limiter |
| **Intent** | Is "process invoice" clearly defined? | "Process" could mean extract, validate, route, or pay | Write explicit success criteria: extracted fields pass schema validation, routed to correct approver |
| **Capabilities** | Can the agent read scanned PDFs? | No OCR tool registered | Add `extractTextFromPdf` tool backed by Azure Document Intelligence |
| **Experience** | Does the agent know invoice schema formats? | No few-shot examples in system prompt | Add 3-5 annotated examples of correct JSON extraction |
| **Features** | Is the output format constrained? | Agent returns free text instead of JSON | Add `ResponseFormat = JSON` + schema validation in filter |
| **Workflows** | What happens if approval fails? | No fallback defined | Add escalation path: auto-route to finance manager after 48h |
| **Inputs** | Can the agent handle multi-page invoices? | Chunking not implemented for multi-page | Implement page-by-page chunking + aggregation step |
| **Data** | Is the vendor database accessible? | Vendor lookup returns stale cached data | Connect to live ERP API; add cache invalidation on vendor update |

---

### Interview Talking Points — RICEFWID

| Question | Answer |
|---|---|
| What is RICEFWID used for? | Structured pre-deployment checklist for AI agents. It forces explicit answers to: does this agent have what it needs (resources, data, capabilities) and is the task well-defined enough (intent, workflows) for autonomous execution? |
| Which dimension is most commonly missed? | **Intent** — teams build capable agents but forget to define measurable success criteria and termination conditions. An agent without a clear "done" state may loop indefinitely or stop prematurely. |
| How does RICEFWID differ from regular sprint planning? | RICEFWID is agent-centric, not feature-centric. It evaluates the *agent's readiness* for a task rather than the team's plan to build features. It is applied at agent design time, not project planning time. |
| How would you use it in a production agent design review? | Create an 8-row scorecard (one per dimension), assign red/amber/green, require all rows green before production deployment. For each amber/red, document the gap, the owner, and the resolution date. |

---

## 11. AI Agent 4-Part Loop

### Overview

The AI Agent 4-Part Loop (Perception → Reasoning → Action → Memory) is the fundamental control cycle that all autonomous AI agents implement, regardless of the framework. It mirrors the classic sense-plan-act cycle from robotics. Each iteration of the loop: the agent **perceives** its environment (reads inputs, tool outputs, user messages), **reasons** about what to do next (LLM planning step), **acts** by executing tools or producing output, and **stores** results in memory to inform the next iteration. LangGraph implements this as a stateful directed graph; Semantic Kernel's `AgentGroupChat` implements it implicitly through the conversation loop.

---

### The 4-Part Loop

```mermaid
flowchart LR
    subgraph Loop ["Agent Execution Loop"]
        P["1. PERCEPTION\nObserve environment:\n— User message\n— Tool call result\n— External event\n— API response"]
        R["2. REASONING\nLLM planning step:\n— What is my goal?\n— What do I know?\n— What should I do next?\n— Which tool?"]
        A["3. ACTION\nExecute decision:\n— Call a tool\n— Write to memory\n— Send a message\n— Terminate if done"]
        M["4. MEMORY\nStore results:\n— Update context\n— Write to vector store\n— Log to audit trail\n— Update task state"]

        P --> R
        R --> A
        A --> M
        M --> P
    end

    IN(["External Trigger\nUser / Event / Schedule"]) --> P
    A --> OUT(["Output to User\nor External System"])

    style P fill:#0078D4,color:#fff
    style R fill:#8b5cf6,color:#fff
    style A fill:#22c55e,color:#fff
    style M fill:#1e40af,color:#fff
    style IN fill:#0f172a,color:#fff
    style OUT fill:#22c55e,color:#fff
```

---

### Agent Loop with Guardrails

```mermaid
flowchart TD
    TRIGGER(["Trigger"]) --> GI["Input Guardrail\n(Validate, sanitize,\ncheck policy)"]
    GI -->|"Blocked"| REJECT(["Reject with\nexplanation"])
    GI -->|"Pass"| PERC["Perception\nBuild context from:\nhistory + memory + input"]
    PERC --> REASON["Reasoning\nLLM decides: plan, tool, or respond"]
    REASON --> TERM{"Is goal\nachieved?"}
    TERM -->|"Yes"| GO["Output Guardrail\n(Validate output format\npolicy, PII redaction)"]
    TERM -->|"No"| ACTION["Action\nExecute selected tool"]
    ACTION --> SAFE{"Tool safe\nto execute?"}
    SAFE -->|"No — approval required"| HUMAN["Human-in-Loop\nAwait approval"]
    HUMAN --> ACTION
    SAFE -->|"Yes"| RESULT["Tool Result"]
    RESULT --> MEM["Memory Update\nLog + store result"]
    MEM --> MAXLOOP{"Max iterations\nreached?"}
    MAXLOOP -->|"Yes"| ABORT(["Abort — return\npartial result + reason"])
    MAXLOOP -->|"No"| PERC
    GO --> RESP(["Final Response"])

    style TRIGGER fill:#0f172a,color:#fff
    style RESP fill:#22c55e,color:#fff
    style REJECT fill:#ef4444,color:#fff
    style ABORT fill:#ef4444,color:#fff
    style HUMAN fill:#f59e0b,color:#fff
    style GI fill:#8b5cf6,color:#fff
    style GO fill:#8b5cf6,color:#fff
```

---

### Component — Agent Loop in Semantic Kernel

```csharp
public class AgentLoopService(Kernel kernel, IMemoryStore memoryStore, ILogger<AgentLoopService> logger)
{
    private const int MaxIterations = 10;

    public async Task<string> RunAsync(string userGoal, string userId, CancellationToken ct = default)
    {
        // MEMORY: Load prior context
        var priorContext = await memoryStore.GetRecentAsync(userId, maxItems: 5, ct);
        var history = new ChatHistory();
        history.AddSystemMessage($"""
            You are an autonomous agent. Complete the user's goal step-by-step using available tools.
            When the goal is fully achieved, respond with "TASK COMPLETE: " followed by a summary.
            Prior context: {string.Join('\n', priorContext)}
            """);
        history.AddUserMessage(userGoal);

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            MaxTokens = 2000
        };

        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            logger.LogInformation("Agent loop iteration {I}/{Max}", iteration + 1, MaxIterations);

            // REASONING: LLM decides what to do
            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatService.GetChatMessageContentAsync(
                history, settings, kernel, ct);

            var content = response.Content ?? string.Empty;
            history.AddAssistantMessage(content);

            // MEMORY: Log each iteration
            await memoryStore.SaveAsync(userId, $"iteration-{iteration}", content, ct);

            // TERMINATION: Check if done
            if (content.Contains("TASK COMPLETE:", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Goal achieved in {I} iterations", iteration + 1);
                return content;
            }

            // PERCEPTION: Tool results are automatically injected by Semantic Kernel
            // via FunctionChoiceBehavior.Auto() — no manual handling needed
        }

        return "Max iterations reached. Partial result logged.";
    }
}
```

---

### Interview Talking Points — Agent 4-Part Loop

| Question | Answer |
|---|---|
| What is the purpose of the Perception step? | Aggregating all available information into the agent's current context: the user's original goal, conversation history, results from prior tool calls, retrieved memory, and any external events. This is what the LLM sees when it reasons. |
| What happens if the Reasoning step produces an incorrect tool call? | The tool returns an error or unexpected result, which becomes input to the next Perception step. A well-designed agent incorporates the error into its context and reasons about retrying differently or escalating to a human. This self-correction is what distinguishes agentic from simple chatbot behavior. |
| What is the max iteration limit and why is it critical? | An upper bound on how many times the loop runs. Without it, a confused agent loops indefinitely consuming tokens and API budget. Production agents must log when this limit is hit, return a partial result, and alert the operator — not silently fail. |
| How is memory different from context in the loop? | **Context** is what's in the current prompt (in-context, ephemeral per session). **Memory** is what persists across sessions (external store). The loop reads from memory during Perception (to build context) and writes to memory after Action (to persist results). |
| How does LangGraph implement the 4-part loop? | As a directed graph where each node is a step (perceive, reason, act, memory). Edges are conditional transitions. The graph is stateful — the shared `AgentState` object persists between nodes within and across loop iterations. Cycles implement the loop; conditional edges implement branching and termination. |

---

## 12. MAE vs MSE vs RMSE — Error Regression Metrics

### Overview

Regression error metrics measure how far model predictions deviate from actual values. The choice of metric affects what the model optimizes for and how outliers are penalized. **MAE** (Mean Absolute Error) is the average absolute difference — interpretable, outlier-tolerant. **MSE** (Mean Squared Error) squares errors — amplifies outliers, useful when large errors are disproportionately costly. **RMSE** (Root MSE) returns MSE to original units for interpretability while retaining the outlier-sensitivity of MSE. In AI systems, these metrics evaluate regression components: response latency prediction, cost estimation, RAG relevance scores, and time-series forecasting.

---

### Metric Comparison

```mermaid
flowchart LR
    PRED(["Model Predictions ŷ\nvs Actual Values y"]) --> MAE_B
    PRED --> MSE_B
    PRED --> RMSE_B

    subgraph MAE_B ["MAE — Mean Absolute Error"]
        MAE_F["Formula:\nMAE = (1/n) Σ |yᵢ - ŷᵢ|\n\nUnit: Same as target\nSensitive to outliers: No\nInterpretable: Yes"]
        MAE_U["Use when:\nAll errors equally important\nOutliers are noise to ignore\nEasy stakeholder explanation"]
    end

    subgraph MSE_B ["MSE — Mean Squared Error"]
        MSE_F["Formula:\nMSE = (1/n) Σ (yᵢ - ŷᵢ)²\n\nUnit: Squared target units\nSensitive to outliers: High\nInterpretable: Low"]
        MSE_U["Use when:\nLarge errors are catastrophic\nDifferentiable loss for training\n(smoother gradient than MAE)"]
    end

    subgraph RMSE_B ["RMSE — Root Mean Squared Error"]
        RMSE_F["Formula:\nRMSE = √MSE\n\nUnit: Same as target\nSensitive to outliers: High\nInterpretable: Yes"]
        RMSE_U["Use when:\nYou want MSE sensitivity\nbut need original-unit\nreadability for reporting"]
    end

    style PRED fill:#0f172a,color:#fff
    style MAE_F fill:#22c55e,color:#fff
    style MSE_F fill:#ef4444,color:#fff
    style RMSE_F fill:#f59e0b,color:#fff
    style MAE_U fill:#1e40af,color:#fff
    style MSE_U fill:#1e40af,color:#fff
    style RMSE_U fill:#1e40af,color:#fff
```

---

### Metric Selection Decision Tree

```mermaid
flowchart TD
    Q1{"Are large prediction\nerrors catastrophic\n(safety, financial)?"}
    Q1 -->|"Yes"| Q2{"Do you need results\nin original units\nfor reporting?"}
    Q1 -->|"No — outliers are noise"| MAE["Use MAE\nRobust, interpretable\nEasy to explain"]

    Q2 -->|"Yes"| RMSE["Use RMSE\nPunishes outliers\nSame units as data"]
    Q2 -->|"No — training loss only"| MSE["Use MSE\nDifferentiable everywhere\nOptimal for gradient descent"]

    MAE --> EX1["Examples:\nDelivery time estimation\nContent relevance scoring\nCost prediction with noisy data"]
    RMSE --> EX2["Examples:\nLatency SLA prediction\nRevenue forecasting\nStock price prediction"]
    MSE --> EX3["Examples:\nNeural network training loss\nLinear regression fitting"]

    style MAE fill:#22c55e,color:#fff
    style RMSE fill:#f59e0b,color:#fff
    style MSE fill:#ef4444,color:#fff
    style EX1 fill:#1e40af,color:#fff
    style EX2 fill:#1e40af,color:#fff
    style EX3 fill:#1e40af,color:#fff
```

---

### Computing Metrics in C#

```csharp
public static class RegressionMetrics
{
    public static double Mae(IReadOnlyList<double> actual, IReadOnlyList<double> predicted)
    {
        if (actual.Count != predicted.Count) throw new ArgumentException("Length mismatch");
        return actual.Zip(predicted, (a, p) => Math.Abs(a - p)).Average();
    }

    public static double Mse(IReadOnlyList<double> actual, IReadOnlyList<double> predicted)
    {
        if (actual.Count != predicted.Count) throw new ArgumentException("Length mismatch");
        return actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Average();
    }

    public static double Rmse(IReadOnlyList<double> actual, IReadOnlyList<double> predicted)
        => Math.Sqrt(Mse(actual, predicted));

    // R² — proportion of variance explained (bonus metric)
    public static double R2(IReadOnlyList<double> actual, IReadOnlyList<double> predicted)
    {
        var mean = actual.Average();
        var ssTot = actual.Sum(a => Math.Pow(a - mean, 2));
        var ssRes = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Sum();
        return 1.0 - ssRes / ssTot;
    }
}

// Usage in a latency prediction evaluation
var actual    = new[] { 120.0, 350.0, 1800.0, 95.0, 210.0 }; // ms
var predicted = new[] { 130.0, 340.0,  900.0, 100.0, 220.0 };

Console.WriteLine($"MAE:  {RegressionMetrics.Mae(actual, predicted):F1} ms");
Console.WriteLine($"MSE:  {RegressionMetrics.Mse(actual, predicted):F1} ms²");
Console.WriteLine($"RMSE: {RegressionMetrics.Rmse(actual, predicted):F1} ms");
Console.WriteLine($"R²:   {RegressionMetrics.R2(actual, predicted):F3}");
// MAE:  271.0 ms  (influenced by the 900ms miss)
// RMSE: 404.8 ms  (the 900ms outlier is punished harder)
```

---

### Interview Talking Points — MAE vs MSE vs RMSE

| Question | Answer |
|---|---|
| Why does squaring errors in MSE matter? | Squaring amplifies large errors disproportionately. An error of 10 contributes 100 to MSE; an error of 100 contributes 10,000. This makes MSE ideal when large errors are costly (a model that is rarely very wrong is preferred over one that is often a little wrong). |
| Why is RMSE often preferred over MSE for reporting? | MSE is in squared units (ms², dollars²) which is uninterpretable to stakeholders. RMSE returns the metric to original units (ms, dollars) while preserving MSE's outlier sensitivity. |
| When would you use MAE over RMSE? | When outliers are genuine noise you want to ignore (sensor glitches, corrupted data), or when the cost of all errors is equal regardless of magnitude. Median-based models align naturally with MAE; mean-based models align with MSE/RMSE. |
| What is R² (coefficient of determination)? | R² measures the proportion of variance in the target explained by the model. R²=1.0 means perfect prediction; R²=0 means the model is no better than predicting the mean; negative means worse than predicting the mean. |
| Which metric should you use for LLM response quality evaluation? | LLM output quality is not a regression problem — use classification-style metrics (ROUGE, BLEU for text overlap) or LLM-as-judge (score 1-5 on helpfulness, faithfulness). MAE/MSE are used for numeric outputs like latency, cost, or confidence score prediction. |

---

## 13. Multi-Step LLM Workflows

### Overview

A single LLM call ("zero-shot") fails on complex tasks requiring multiple reasoning steps, diverse data sources, or specialized subtask handling. Multi-step workflows decompose these tasks into a sequence of LLM calls, tool invocations, and conditional branches. The four canonical patterns are: **Prompt Chaining** (output of step N feeds step N+1), **Routing** (a classifier directs input to a specialist), **Orchestrator-Workers** (a planner delegates to independent sub-agents), and **Human-in-the-Loop** (a human reviewer gates or corrects LLM output mid-workflow).

---

### Pattern 1 — Prompt Chaining

```mermaid
flowchart LR
    IN(["Raw Contract PDF"]) --> S1["Step 1:\nExtract key clauses\n(LLM + Document Intelligence)"]
    S1 --> S2["Step 2:\nIdentify risk items\n(LLM reads extracted clauses)"]
    S2 --> S3["Step 3:\nGenerate risk summary\nin standard format\n(LLM)"]
    S3 --> S4["Step 4:\nTranslate to client's\npreferred language\n(LLM)"]
    S4 --> OUT(["Formatted Risk Report"])

    style IN fill:#0f172a,color:#fff
    style OUT fill:#22c55e,color:#fff
    style S1 fill:#0078D4,color:#fff
    style S2 fill:#8b5cf6,color:#fff
    style S3 fill:#8b5cf6,color:#fff
    style S4 fill:#22c55e,color:#fff
```

---

### Pattern 2 — Routing

```mermaid
flowchart TD
    Q(["User Query"]) --> ROUTER["Router LLM\n(Classifies intent)"]

    ROUTER -->|"Billing question"| BILLING["Billing Agent\n(ERP + invoice tools)"]
    ROUTER -->|"Technical support"| TECH["Tech Support Agent\n(KB search + ticket tools)"]
    ROUTER -->|"Product question"| PROD["Product Agent\n(RAG over product catalog)"]
    ROUTER -->|"Human needed"| HUMAN["Escalate to\nLive Agent"]

    BILLING --> RESP(["Response"])
    TECH --> RESP
    PROD --> RESP
    HUMAN --> RESP

    style Q fill:#0f172a,color:#fff
    style ROUTER fill:#8b5cf6,color:#fff
    style BILLING fill:#0078D4,color:#fff
    style TECH fill:#0078D4,color:#fff
    style PROD fill:#0078D4,color:#fff
    style HUMAN fill:#f59e0b,color:#fff
    style RESP fill:#22c55e,color:#fff
```

---

### Pattern 3 — Orchestrator-Workers

```mermaid
flowchart TD
    GOAL(["User Goal:\nPrepare due diligence report\nfor Acquisition Target X"]) --> ORCH["Orchestrator LLM\nDecomposes task\ninto parallel subtasks"]

    ORCH --> W1["Worker 1:\nFinancial Analysis\n(access ERP data)"]
    ORCH --> W2["Worker 2:\nLegal Risk Review\n(search contract DB)"]
    ORCH --> W3["Worker 3:\nMarket Position\n(RAG over analyst reports)"]
    ORCH --> W4["Worker 4:\nTechnology Assessment\n(GitHub + tech stack analysis)"]

    W1 --> AGG["Aggregator LLM\nSynthesizes worker outputs\ninto unified report"]
    W2 --> AGG
    W3 --> AGG
    W4 --> AGG

    AGG --> OUT(["Due Diligence Report"])

    style GOAL fill:#0f172a,color:#fff
    style OUT fill:#22c55e,color:#fff
    style ORCH fill:#8b5cf6,color:#fff
    style AGG fill:#8b5cf6,color:#fff
    style W1 fill:#0078D4,color:#fff
    style W2 fill:#0078D4,color:#fff
    style W3 fill:#0078D4,color:#fff
    style W4 fill:#0078D4,color:#fff
```

---

### Pattern 4 — Human-in-the-Loop (Evaluator-Optimizer)

```mermaid
sequenceDiagram
    participant U as User
    participant G as Generator LLM
    participant E as Evaluator (LLM or Human)
    participant O as Optimizer LLM

    U->>G: "Draft a contract clause for..."
    G-->>U: Draft v1

    alt Automated evaluation
        G->>E: Evaluate: legality, completeness, tone
        E-->>G: Score: 6/10. Issues: missing liability cap, passive voice
        G->>O: Revise with issues: [liability cap, passive voice]
        O-->>U: Draft v2 (improved)
    else Human review gate
        G-->>U: Draft v1 (sent for legal review)
        U->>U: Human reviewer approves or redlines
        U->>O: Apply redlines: [remove clause 3, strengthen clause 5]
        O-->>U: Final approved draft
    end
```

---

### Component — Prompt Chaining in Semantic Kernel

```csharp
public class ContractReviewPipeline(IChatCompletionService chat)
{
    public async Task<string> RunAsync(string contractText, CancellationToken ct = default)
    {
        // Step 1: Extract clauses
        var clauses = await StepAsync(
            system: "Extract all legally significant clauses from the contract. Return as numbered list.",
            user: contractText, ct);

        // Step 2: Identify risks (feeds on Step 1 output)
        var risks = await StepAsync(
            system: "Identify risk items in these clauses. For each risk: state the clause, the risk type, and severity (High/Medium/Low).",
            user: clauses, ct);

        // Step 3: Generate structured summary (feeds on Step 2 output)
        var summary = await StepAsync(
            system: "Produce a JSON risk summary with fields: riskCount, highRisks[], mediumRisks[], lowRisks[], recommendation.",
            user: risks, ct);

        return summary;
    }

    private async Task<string> StepAsync(string system, string user, CancellationToken ct)
    {
        var history = new ChatHistory(system);
        history.AddUserMessage(user);
        var result = await chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        return result.Content ?? string.Empty;
    }
}
```

---

### Interview Talking Points — Multi-Step LLM Workflows

| Question | Answer |
|---|---|
| Why not just use a single large prompt? | Single prompts fail on tasks requiring diverse tools, parallel data access, or conditional branching. Token limits constrain how much context a single call can hold. Multi-step workflows break the problem, handle failures per step, and enable partial result recovery. |
| What is "prompt chaining" and when does it fail? | Output of step N feeds step N+1. Fails when an early step produces incorrect output — errors compound. Mitigation: validate each step's output (schema check, confidence threshold) before passing downstream; implement retry per step. |
| What is the difference between routing and orchestration? | **Routing**: static classification — send input to one specialist (no planning). **Orchestration**: dynamic planning — an orchestrator decomposes a goal, assigns multiple workers in parallel, then synthesizes. Routing is fast and cheap; orchestration is flexible but expensive. |
| When is human-in-the-loop essential? | For irreversible actions (financial transactions, legal document signing, sending external communications), compliance-regulated decisions, and any case where LLM hallucination would cause measurable harm. The human gate should be inserted before, not after, the irreversible action. |
| How do you handle failures in a multi-step pipeline? | Per-step retry with Polly, step-level circuit breaker, fallback to a simpler model, partial result return with status, and dead-letter queue for async pipelines. Log every step's input + output for audit and debugging. |
| What frameworks support multi-step LLM workflows natively? | **LangGraph**: stateful graph-based (Python/JS). **Semantic Kernel**: `AgentGroupChat` + sequential planner (.NET-first). **CrewAI**: role-based multi-agent with task assignment. **Azure AI Foundry**: visual workflow designer for Azure-hosted agents. |

---

*Part 2 of 3 | Continues in Part 3: Tools vs Skills vs Hooks vs Agents vs MCP · GuardRails · LLMOps / MLOps · Context Rot · Tokenization · AI Deployment Patterns*
