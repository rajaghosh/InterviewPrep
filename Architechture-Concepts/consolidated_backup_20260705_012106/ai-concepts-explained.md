# AI Concepts Explained — Complete Guide
> **Consolidated From:** 20-AI-Concepts-Explained.md, 9-AI-Concepts-Explained.md
> **Topics Covered:** 20 core AI concepts, 9 additional AI concepts, ByteByteGo framing, concept interactions
> **Consolidation Date:** 2026-07-04
> **Original Documents:** 2 → **Content Preserved:** 100%

---

> **Source:** [YouTube — 20 AI Concepts Explained in 40 Minutes](https://www.youtube.com/watch?v=OYvlznJ4IZQ)
> **Channel/Event:** Gaurav Sen
> **Topic:** LLM, Tokenization, Vectorization, Attention, Transformer, RAG, Vector DB, MCP, Agents, Reasoning, Distillation, Quantization
> **Key Claim:** Shared AI vocabulary for engineers building AI systems — 20 foundational concepts in 40 minutes

---

## Table of Contents

1. [Overview](#1-overview)
2. [The 20 Concepts at a Glance](#2-the-20-concepts-at-a-glance)
3. [Core Concepts — Foundational Layer](#3-core-concepts--foundational-layer)
4. [Core Concepts — Architecture Layer](#4-core-concepts--architecture-layer)
5. [Core Concepts — Adaptation Layer](#5-core-concepts--adaptation-layer)
6. [Core Concepts — Retrieval & Memory Layer](#6-core-concepts--retrieval--memory-layer)
7. [Core Concepts — Agentic & Reasoning Layer](#7-core-concepts--agentic--reasoning-layer)
8. [Core Concepts — Efficiency Layer](#8-core-concepts--efficiency-layer)
9. [Architecture — The Modern AI Stack](#9-architecture--the-modern-ai-stack)
10. [How It Works — Token to Output Pipeline](#10-how-it-works--token-to-output-pipeline)
11. [Comparison Tables](#11-comparison-tables)
12. [Code Examples](#12-code-examples)
13. [Best Practices](#13-best-practices)
14. [Interview Talking Points](#14-interview-talking-points)
15. [Learning Resources](#15-learning-resources)

---

## 1. Overview

This video by Gaurav Sen covers the 20 foundational AI/ML concepts every software engineer needs to speak the language of modern AI systems. It is structured as a rapid-fire reference: not a deep-dive into any single topic, but a cohesive vocabulary guide that connects concepts — from raw text tokens all the way to quantized, distilled agents running on edge devices. The target audience is engineers who already code but need to reason, write, and discuss AI architecture fluently. Each concept is self-contained with a clear timestamp for quick review.

---

## 2. The 20 Concepts at a Glance

| # | Concept | Timestamp | Layer |
|---|---------|-----------|-------|
| 1 | Large Language Model | 00:28 | Foundation |
| 2 | Tokenization | 01:28 | Foundation |
| 3 | Vectorization | 02:53 | Foundation |
| 4 | Attention | 04:15 | Architecture |
| 5 | Self-Supervised Learning | 07:22 | Architecture |
| 6 | Transformer | 12:07 | Architecture |
| 7 | Fine-tuning | 14:32 | Adaptation |
| 8 | Few-shot Prompting | 17:05 | Adaptation |
| 9 | Retrieval Augmented Generation (RAG) | 18:11 | Retrieval & Memory |
| 10 | Vector Database | 20:33 | Retrieval & Memory |
| 11 | Model Context Protocol (MCP) | 23:03 | Retrieval & Memory |
| 12 | Context Engineering | 25:43 | Retrieval & Memory |
| 13 | Agents | 28:17 | Agentic & Reasoning |
| 14 | Reinforcement Learning | 29:19 | Agentic & Reasoning |
| 15 | Chain of Thought | 34:42 | Agentic & Reasoning |
| 16 | Reasoning Models | 35:55 | Agentic & Reasoning |
| 17 | Multi-modal Models | 36:36 | Agentic & Reasoning |
| 18 | Small Language Models | 38:21 | Efficiency |
| 19 | Distillation | 40:24 | Efficiency |
| 20 | Quantization | 41:47 | Efficiency |

---

## 3. Core Concepts — Foundational Layer

### Large Language Model (LLM)
A neural network with billions of parameters trained on massive text corpora to predict the next token given a sequence of tokens. LLMs are not rule-based; they learn statistical patterns across language. Key property: emergent capabilities (reasoning, coding, summarization) appear at scale without being explicitly trained.

### Tokenization
The process of splitting raw text into discrete units called **tokens** before feeding it to a model. Tokens are not always words — they are sub-word pieces (e.g., "unhappiness" → `["un", "happi", "ness"]`). The vocabulary size is fixed (GPT-4 uses ~100K tokens). Tokenization determines:
- What the model can represent
- How context length is measured (tokens, not characters)
- Cost of inference (billed per token)

**Key numbers:**
- ~4 chars ≈ 1 token (English)
- 1 page ≈ 500 tokens
- GPT-4 Turbo context: 128K tokens

### Vectorization (Embeddings)
Converting tokens into high-dimensional numerical vectors so the model can perform math on language. Each token maps to a vector in embedding space (e.g., 768 or 4096 dimensions). Semantically similar words cluster together — this is what enables meaning-aware operations.

```
"king" - "man" + "woman" ≈ "queen"  (classic word2vec demo)
```

The embedding space is learned during training. The model learns that "Paris" and "France" should be close in the same way "Berlin" and "Germany" are.

---

## 4. Core Concepts — Architecture Layer

### Attention
The core mechanism that allows a transformer to weigh how much each token should "pay attention" to every other token in the sequence. Attention solves the problem of long-range dependencies that RNNs couldn't handle.

**Scaled Dot-Product Attention:**

```
Attention(Q, K, V) = softmax(QK^T / √d_k) × V
```

- **Q (Query):** What am I looking for?
- **K (Key):** What do I have?
- **V (Value):** What do I return if I match?

**Multi-Head Attention** runs several attention operations in parallel, each learning different relationships (syntax, coreference, semantics).

```mermaid
flowchart LR
    Input["Input Tokens"] --> Q["Query\n(What do I need?)"]
    Input --> K["Key\n(What do I have?)"]
    Input --> V["Value\n(What to return)"]
    Q --> Score["Dot Product Score\nQ × K^T / √d_k"]
    K --> Score
    Score --> Softmax["Softmax\n(Normalize weights)"]
    Softmax --> Weighted["Weighted Sum\n× Values"]
    V --> Weighted
    Weighted --> Output["Attention Output"]

    style Input fill:#0078D4,color:#fff
    style Score fill:#5C2D91,color:#fff
    style Softmax fill:#5C2D91,color:#fff
    style Output fill:#107C10,color:#fff
```

### Self-Supervised Learning
Training a model on unlabeled data by deriving supervision signals from the data itself. For LLMs, the task is **next-token prediction** (Causal LM) or **masked token prediction** (BERT). No human labels required — the internet is the training set.

This is why LLMs can be trained at scale: no annotation bottleneck. The model sees a trillion tokens and learns by predicting what comes next.

### Transformer
The neural network architecture that replaced RNNs and LSTMs for sequence modeling. Key innovations:
1. **Parallel processing** — processes all tokens simultaneously (vs. sequential in RNNs)
2. **Self-attention** — every token attends to every other token
3. **Positional encoding** — since there's no recurrence, position must be injected

```mermaid
flowchart TD
    subgraph Encoder ["Encoder Block (BERT-style)"]
        E1["Input Embeddings\n+ Positional Encoding"]
        E2["Multi-Head\nSelf-Attention"]
        E3["Add & LayerNorm"]
        E4["Feed-Forward Network"]
        E5["Add & LayerNorm"]
        E1 --> E2 --> E3 --> E4 --> E5
    end

    subgraph Decoder ["Decoder Block (GPT-style)"]
        D1["Output Embeddings\n+ Positional Encoding"]
        D2["Masked Multi-Head\nSelf-Attention"]
        D3["Add & LayerNorm"]
        D4["Cross-Attention\n(Encoder-Decoder)"]
        D5["Add & LayerNorm"]
        D6["Feed-Forward Network"]
        D7["Add & LayerNorm"]
        D1 --> D2 --> D3 --> D4 --> D5 --> D6 --> D7
    end

    Encoder --> Decoder

    style Encoder fill:#EFF6FC,stroke:#0078D4
    style Decoder fill:#FFF4CE,stroke:#D83B01
```

---

## 5. Core Concepts — Adaptation Layer

### Fine-tuning
Continuing to train a pre-trained model on a smaller, task-specific dataset to adapt it to a new domain or behavior. The base model's weights are updated (vs. frozen in other techniques).

| Type | Description | When to Use |
|---|---|---|
| **Full fine-tuning** | Update all weights | Maximum quality, high compute |
| **LoRA** | Low-rank adapters only | Efficient, preserves base model |
| **RLHF** | Human feedback signal | Align with human preferences |
| **Instruction tuning** | Train on instruction-response pairs | ChatGPT-style behavior |

**LoRA intuition:** Instead of updating the full weight matrix W (d×d), learn two small matrices A (d×r) and B (r×d) where r << d. At inference, W' = W + AB.

### Few-shot Prompting
Providing the model with a small number of input-output examples in the prompt to demonstrate the desired behavior — no weight updates required. The model generalizes from the pattern.

```
Zero-shot:  "Classify sentiment: 'I love this product'"
            → Positive

Few-shot:   "Classify sentiment:
            'Great product!' → Positive
            'Terrible experience' → Negative
            'I love this product' → "
            → Positive
```

**Why it works:** LLMs learn in-context — the prompt IS the training data at inference time. This is called **in-context learning (ICL)**.

---

## 6. Core Concepts — Retrieval & Memory Layer

### Retrieval Augmented Generation (RAG)
Augmenting an LLM's response with relevant documents retrieved from an external knowledge base at inference time. Solves the two biggest LLM limitations: knowledge cutoffs and hallucinations.

```mermaid
sequenceDiagram
    participant U as User
    participant R as Retriever
    participant VDB as Vector DB
    participant LLM as LLM

    U->>R: Query: "What is our refund policy?"
    R->>VDB: Embed query → search nearest vectors
    VDB-->>R: Top-k relevant document chunks
    R->>LLM: Prompt = [Context: <chunks>] + [Question: <query>]
    LLM-->>U: Grounded answer citing retrieved docs
```

**RAG vs Fine-tuning:**
| Dimension | RAG | Fine-tuning |
|---|---|---|
| Knowledge update | Real-time (add docs) | Requires retraining |
| Cost | Low (retrieval only) | High (GPU compute) |
| Hallucination control | High (cited sources) | Medium |
| Private data | Easy (index it) | Risk of memorization |

### Vector Database
A database purpose-built for storing, indexing, and querying high-dimensional embedding vectors via approximate nearest neighbor (ANN) search.

**Core operations:**
1. **Upsert:** Store vector + payload metadata
2. **Query:** Find k-nearest vectors to a query vector
3. **Filter:** Metadata filtering alongside vector search

**Popular ANN algorithms:**
- **HNSW** (Hierarchical Navigable Small World) — graph-based, fast query
- **IVF** (Inverted File Index) — cluster-based, memory efficient
- **PQ** (Product Quantization) — compress vectors, faster but lossy

| Database | Open Source | Managed Cloud | Notes |
|---|---|---|---|
| Pinecone | No | Yes | SaaS only, production-grade |
| Weaviate | Yes | Yes | Built-in vectorization |
| Qdrant | Yes | Yes | Rust-based, high performance |
| Chroma | Yes | No | Lightweight, dev-friendly |
| pgvector | Yes | Yes | PostgreSQL extension |
| Azure AI Search | No | Azure | Hybrid vector + keyword |

### Model Context Protocol (MCP)
Anthropic's open protocol (2024) that standardizes how AI models connect to external tools, data sources, and services. Think of it as **USB-C for AI** — one universal interface instead of custom integrations per tool.

```mermaid
flowchart LR
    subgraph Host ["MCP Host (Claude Desktop, IDE)"]
        Client["MCP Client"]
    end

    subgraph Servers ["MCP Servers"]
        FS["File System\nServer"]
        DB["Database\nServer"]
        API["API / Web\nServer"]
        Git["GitHub\nServer"]
    end

    Client <--> FS
    Client <--> DB
    Client <--> API
    Client <--> Git

    style Host fill:#EFF6FC,stroke:#0078D4
    style Client fill:#0078D4,color:#fff
    style Servers fill:#FFF4CE,stroke:#D83B01
```

**MCP primitives:**
- **Tools** — functions the model can call (read file, query DB)
- **Resources** — data the model can read (files, documents)
- **Prompts** — reusable prompt templates

### Context Engineering
The discipline of deliberately designing what information goes into a model's context window to maximize output quality. As context windows grow (1M+ tokens), the challenge shifts from "what fits" to "what should be included and how."

**Context window components:**
```
System prompt       → Role, instructions, constraints
Retrieved docs      → RAG output (most relevant chunks)
Tool results        → Results from function calls
Conversation history → Prior turns (summarized if long)
User query          → The actual question
```

**Key principles:**
- **Primacy/recency bias** — models attend more to beginning and end of context
- **Needle-in-haystack problem** — critical info buried in the middle is often missed
- **Compression** — summarize older turns; don't dump raw history
- **Structured context** — use XML/JSON tags to delineate sections

---

## 7. Core Concepts — Agentic & Reasoning Layer

### Agents
AI systems that go beyond single-turn responses by using tools, maintaining state, and taking multi-step actions to complete a goal. An agent has a **perception → reasoning → action** loop.

```mermaid
flowchart TD
    Goal["🎯 User Goal"] --> Perceive["Perceive\n(Read context, tool results)"]
    Perceive --> Reason["Reason\n(LLM: what to do next?)"]
    Reason --> Act["Act\n(Call tool / write / search)"]
    Act --> Observe["Observe\n(Tool result)"]
    Observe --> Done{Goal\nAchieved?}
    Done -->|No| Perceive
    Done -->|Yes| Output["✅ Final Response"]

    style Goal fill:#0078D4,color:#fff
    style Reason fill:#5C2D91,color:#fff
    style Output fill:#107C10,color:#fff
```

**Agent patterns:**
- **ReAct** — Reason + Act in alternation
- **Plan-and-Execute** — plan all steps, then execute
- **Reflection** — agent critiques its own output
- **Multi-agent** — specialized sub-agents orchestrated by a supervisor

### Reinforcement Learning (RL)
Training a model through trial-and-error: the model takes actions and receives reward signals, learning to maximize cumulative reward. For LLMs, RL is used in:

- **RLHF** (Reinforcement Learning from Human Feedback) — human raters rank outputs, training a reward model, then fine-tuning the LLM with PPO
- **RLAIF** — AI provides the feedback instead of humans
- **GRPO / PPO** — optimization algorithms used to update LLM weights based on rewards

```mermaid
flowchart LR
    A["LLM\n(Policy)"] -->|"Generates response"| B["Human Rater\nor AI Judge"]
    B -->|"Scores output\n(reward signal)"| C["Reward Model"]
    C -->|"PPO gradient update"| A

    style A fill:#0078D4,color:#fff
    style B fill:#5C2D91,color:#fff
    style C fill:#D83B01,color:#fff
```

### Chain of Thought (CoT)
A prompting technique that instructs the model to produce intermediate reasoning steps before the final answer. Dramatically improves performance on multi-step math, logic, and coding problems.

```
Zero-shot CoT:  "Think step by step."

Few-shot CoT:
Q: "Roger has 5 balls. He buys 2 cans of 3 balls each. How many?"
A: "Roger starts with 5. He buys 2×3=6. Total: 5+6=11 balls."
Q: "Lisa has 8 apples. She eats 2 and gives 3 to a friend. How many left?"
A: [model follows the reasoning pattern]
```

**Why it works:** LLMs are trained on human text, which often includes worked examples. CoT activates the model's learned pattern of "show your work."

### Reasoning Models
Models trained specifically to reason before answering — not just generate fluent text. They produce a hidden "thinking" trace (scratchpad) and only output the final answer. Examples: OpenAI o1/o3, DeepSeek-R1, Claude 3.7 Sonnet.

| Property | Standard LLM | Reasoning Model |
|---|---|---|
| Output | Direct answer | Think → Answer |
| Latency | Low | High (more tokens) |
| Math/Logic | Weak | Strong |
| Cost | Low | High |
| Best for | Conversation, writing | Code, math, complex reasoning |

### Multi-modal Models
Models that process and generate across multiple data types (modalities): text, images, audio, video, code. The input encoder for each modality projects into a shared embedding space that the language model understands.

```mermaid
flowchart TD
    Text["📝 Text\nTokenizer + Embeddings"] --> Shared["Shared\nRepresentation Space"]
    Image["🖼️ Image\nVision Encoder"] --> Shared
    Audio["🔊 Audio\nAudio Encoder"] --> Shared
    Shared --> LLM["Transformer\n(LLM Core)"]
    LLM --> TOut["Text Output"]
    LLM --> IOut["Image Output\n(DALL-E, Imagen)"]
    LLM --> AOut["Audio Output\n(TTS)"]

    style Shared fill:#5C2D91,color:#fff
    style LLM fill:#0078D4,color:#fff
```

**Examples:** GPT-4o (text+image+audio), Gemini Ultra, Claude 3.5 Sonnet (text+image), Whisper (audio→text).

---

## 8. Core Concepts — Efficiency Layer

### Small Language Models (SLMs)
Compact models (1B–13B parameters) designed for specific tasks or constrained environments. Trade generality for speed, cost, and deployability. Can run on-device (phone, laptop) without cloud inference.

| Model | Params | Provider | Use Case |
|---|---|---|---|
| Phi-4 | 14B | Microsoft | Reasoning, coding |
| Llama 3.2 | 1B, 3B | Meta | Edge devices |
| Gemma 3 | 1B–27B | Google | On-device |
| Mistral 7B | 7B | Mistral | General purpose |
| Claude Haiku | ~13B est. | Anthropic | Fast, cheap API |

**When to choose SLM over LLM:**
- Latency < 100ms requirement
- On-device / offline use
- Domain-specific task (one thing, done well)
- Cost sensitivity at scale

### Distillation
Training a small **student model** to mimic the behavior of a large **teacher model**. The student learns from the teacher's soft probability outputs (logits), not just the hard labels — this carries richer signal about relationships between classes/tokens.

```mermaid
flowchart LR
    Data["Training Data"] --> Teacher["Large Teacher Model\n(e.g., GPT-4, 175B params)"]
    Teacher -->|"Soft probabilities\n(logits)"| Loss["Distillation Loss\nKL Divergence"]
    Data --> Student["Small Student Model\n(e.g., 7B params)"]
    Student --> Loss
    Loss -->|"Gradient update"| Student

    style Teacher fill:#5C2D91,color:#fff
    style Student fill:#107C10,color:#fff
    style Loss fill:#D83B01,color:#fff
```

**Key insight:** The teacher's distribution over all tokens (not just the correct one) teaches the student what the model considers "plausible" — far more information than a one-hot label.

**Examples:** DistilBERT (BERT→66% size, 97% quality), DeepSeek-R1 distilled variants.

### Quantization
Reducing the numerical precision of model weights and activations to decrease memory footprint and increase inference speed. The accuracy tradeoff is usually small for modern quantization techniques.

| Precision | Bits per Weight | Memory (7B model) | Quality Loss |
|---|---|---|---|
| FP32 | 32 | ~28 GB | 0% (baseline) |
| FP16 / BF16 | 16 | ~14 GB | Negligible |
| INT8 | 8 | ~7 GB | < 1% |
| INT4 | 4 | ~3.5 GB | 1-3% |
| INT2 | 2 | ~1.75 GB | Significant |

**Quantization types:**
- **Post-Training Quantization (PTQ)** — quantize after training, fast, some quality loss
- **Quantization-Aware Training (QAT)** — simulate quantization during training, better quality

**Popular formats:** GGUF (llama.cpp), GPTQ, AWQ, ExLlamaV2.

---

## 9. Architecture — The Modern AI Stack

```mermaid
flowchart TD
    subgraph Input ["Input Layer"]
        Raw["Raw Text / Image / Audio"]
        Tok["Tokenizer / Encoder"]
        Emb["Embedding Layer\n(Vectorization)"]
        Raw --> Tok --> Emb
    end

    subgraph Core ["Transformer Core"]
        Attn["Multi-Head\nSelf-Attention"]
        FFN["Feed-Forward\nNetwork"]
        Norm["Layer Norm\n+ Residual"]
        Attn --> FFN --> Norm
        Norm -.->|"N layers"| Attn
    end

    subgraph Adaptation ["Adaptation Layer"]
        SFT["Supervised\nFine-tuning"]
        RLHF["RLHF / RLAIF"]
        LoRA["LoRA Adapters"]
    end

    subgraph Inference ["Inference & Retrieval"]
        VDB["Vector DB\n(RAG)"]
        MCP2["MCP Tools\n(Agents)"]
        CtxEng["Context\nEngineering"]
        CoT["Chain of Thought\n/ Reasoning"]
    end

    subgraph Output ["Output & Efficiency"]
        Quant["Quantized Weights\n(INT4/INT8)"]
        Distill["Distilled\nStudent Model"]
        Final["Final Response\n(Text / Image / Audio)"]
    end

    Input --> Core
    Core --> Adaptation
    Adaptation --> Inference
    Inference --> Output

    style Input fill:#EFF6FC,stroke:#0078D4
    style Core fill:#FFF4CE,stroke:#D83B01
    style Adaptation fill:#EFF6FC,stroke:#0078D4
    style Inference fill:#FFF4CE,stroke:#D83B01
    style Output fill:#DFF6DD,stroke:#107C10
```

---

## 10. How It Works — Token to Output Pipeline

```mermaid
sequenceDiagram
    participant U as User Input
    participant T as Tokenizer
    participant E as Embedding Layer
    participant A as Attention Mechanism
    participant FF as Feed-Forward Layers
    participant D as Decoder / Sampling
    participant O as Output

    U->>T: "Explain quantum computing"
    T->>E: [1234, 567, 8901, 234] (token IDs)
    E->>A: 4 × 4096-dim vectors
    A->>A: Q·K^T / √d_k → softmax → weighted V
    A->>FF: Contextual representations
    FF->>D: Final hidden states → logits over vocabulary
    D->>O: Sample next token (temperature / top-p)
    O-->>T: Append token, feed back for next step
    Note over D,O: Autoregressive: repeat until EOS token
```

**Step-by-step:**
1. **Tokenize** — split prompt into sub-word tokens, map to integer IDs
2. **Embed** — look up each token ID in the embedding table → float vectors
3. **Positional encode** — add positional signal (sinusoidal or RoPE)
4. **Attend** — multi-head attention computes relationships across all tokens
5. **Feed-forward** — per-token non-linear transformation
6. **Repeat** — N transformer layers (32 for LLaMA 7B, 96 for GPT-4)
7. **Project** — final hidden state → logits over 100K+ vocabulary
8. **Sample** — select next token (greedy / temperature / nucleus sampling)
9. **Autoregress** — append new token, repeat until EOS

---

## 11. Comparison Tables

### Classic NLP vs Modern LLM

| Dimension | Classic NLP (pre-2017) | Modern LLM (Transformer) |
|---|---|---|
| Architecture | RNN, LSTM, n-gram | Transformer + attention |
| Training | Task-specific, labeled data | Self-supervised on internet scale |
| Vocabulary | Fixed, word-level | Sub-word BPE / SentencePiece |
| Context | Short (512 tokens) | Long (1M+ tokens) |
| Generalization | Single task | Zero-shot / few-shot on any task |
| Computation | Sequential (slow) | Parallel (GPU-friendly) |

### RAG vs Fine-tuning vs Prompting

| Approach | Cost | Knowledge Update | Hallucination | Best For |
|---|---|---|---|---|
| **Prompting** | Free | None | High | General tasks, no new knowledge |
| **RAG** | Low | Real-time | Low (cited) | Private docs, fresh data |
| **Fine-tuning** | High | Retraining | Medium | Tone, format, domain language |
| **RAG + Fine-tune** | Highest | Real-time | Lowest | Production enterprise AI |

### SLM vs LLM

| Property | SLM (1B-13B) | LLM (70B-405B+) |
|---|---|---|
| Inference cost | Very low | High |
| Latency | < 50ms (local) | 500ms-2s (cloud) |
| On-device | Yes | No |
| Complex reasoning | Weak-moderate | Strong |
| Domain-specific | Excellent (fine-tuned) | Good out-of-box |
| Privacy | High (local) | Low (cloud API) |

---

## 12. Code Examples

### Python — End-to-End RAG Pipeline

```python
from openai import OpenAI
import numpy as np

client = OpenAI()

def embed(text: str) -> list[float]:
    response = client.embeddings.create(
        input=text,
        model="text-embedding-3-small"
    )
    return response.data[0].embedding

def cosine_similarity(a: list, b: list) -> float:
    a, b = np.array(a), np.array(b)
    return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))

# Build a tiny in-memory vector store
docs = [
    "Our refund policy is 30 days from purchase.",
    "We support Visa, Mastercard, and PayPal.",
    "Shipping takes 3-5 business days.",
]
doc_embeddings = [embed(d) for d in docs]

def rag_query(question: str, top_k: int = 2) -> str:
    q_emb = embed(question)
    scores = [cosine_similarity(q_emb, d) for d in doc_embeddings]
    top_indices = sorted(range(len(scores)), key=lambda i: scores[i], reverse=True)[:top_k]
    context = "\n".join(docs[i] for i in top_indices)

    response = client.chat.completions.create(
        model="gpt-4o-mini",
        messages=[
            {"role": "system", "content": f"Answer using only this context:\n{context}"},
            {"role": "user", "content": question},
        ]
    )
    return response.choices[0].message.content

print(rag_query("What is your refund policy?"))
```

### Python — Few-shot Prompting

```python
from anthropic import Anthropic

client = Anthropic()

few_shot_prompt = """Classify sentiment as Positive, Negative, or Neutral.

Examples:
Input: "This product changed my life!" → Positive
Input: "Complete waste of money." → Negative
Input: "Package arrived on time." → Neutral
Input: "I've never been more disappointed." → Negative

Classify this:
Input: "{text}" →"""

def classify(text: str) -> str:
    response = client.messages.create(
        model="claude-haiku-4-5-20251001",
        max_tokens=10,
        messages=[{"role": "user", "content": few_shot_prompt.format(text=text)}]
    )
    return response.content[0].text.strip()

print(classify("Shipping was fast but the item was broken."))  # → Negative
```

### Python — Quantized Inference with llama.cpp (GGUF)

```bash
# Install llama-cpp-python with GPU support
pip install llama-cpp-python

# Download a quantized model (4-bit)
# e.g., Llama-3.2-3B-Instruct-Q4_K_M.gguf from HuggingFace
```

```python
from llama_cpp import Llama

# Load INT4 quantized model — runs on CPU/Mac M-series
llm = Llama(
    model_path="./Llama-3.2-3B-Instruct-Q4_K_M.gguf",
    n_ctx=4096,   # context window
    n_threads=8,  # CPU threads
)

output = llm.create_chat_completion(
    messages=[
        {"role": "system", "content": "You are a helpful assistant."},
        {"role": "user", "content": "Explain tokenization in one sentence."},
    ],
    max_tokens=100,
    temperature=0.7,
)
print(output["choices"][0]["message"]["content"])
```

### Python — Simple Agent Loop with Tool Use

```python
from anthropic import Anthropic
import json

client = Anthropic()

tools = [
    {
        "name": "web_search",
        "description": "Search the web for current information.",
        "input_schema": {
            "type": "object",
            "properties": {"query": {"type": "string"}},
            "required": ["query"],
        },
    }
]

def fake_web_search(query: str) -> str:
    return f"[Mock result for: {query}] — Latest AI news: Claude 4 released July 2025."

def run_agent(user_message: str) -> str:
    messages = [{"role": "user", "content": user_message}]

    while True:
        response = client.messages.create(
            model="claude-sonnet-4-6",
            max_tokens=1024,
            tools=tools,
            messages=messages,
        )

        if response.stop_reason == "end_turn":
            return response.content[0].text

        # Handle tool calls
        tool_results = []
        for block in response.content:
            if block.type == "tool_use":
                result = fake_web_search(block.input["query"])
                tool_results.append({
                    "type": "tool_result",
                    "tool_use_id": block.id,
                    "content": result,
                })

        messages.append({"role": "assistant", "content": response.content})
        messages.append({"role": "user", "content": tool_results})

print(run_agent("What are the latest AI releases in 2025?"))
```

---

## 13. Best Practices

### Tokenization & Context
- ✅ Measure input/output in tokens, not characters
- ✅ Use `tiktoken` (OpenAI) or the model's own tokenizer to count tokens before API calls
- ❌ Don't assume 1 word = 1 token — code and non-English text tokenize differently
- ❌ Don't stuff the context window — more tokens ≠ better results due to attention dilution

### RAG
- ✅ Chunk documents by semantic boundary (paragraph/section), not fixed character count
- ✅ Embed both document chunks and queries with the same model
- ✅ Use hybrid search (vector + BM25 keyword) for better recall
- ❌ Don't retrieve too many chunks — top-3 to top-5 is usually optimal
- ❌ Don't skip re-ranking — a cross-encoder re-ranker dramatically improves precision

### Agents
- ✅ Give the agent a minimal, well-defined toolset — fewer tools = fewer mistakes
- ✅ Add guardrails: max iterations, human-in-the-loop for destructive actions
- ✅ Log every tool call and result for debugging and auditing
- ❌ Don't let agents run unbounded loops in production without a circuit breaker
- ❌ Don't use RL/RLHF unless you have a reliable reward signal — noisy rewards break training

### Quantization & SLMs
- ✅ Start with INT8 for inference — near-lossless and halves memory vs FP16
- ✅ Use INT4 (GGUF Q4_K_M) for on-device or memory-constrained deployments
- ❌ Don't go below INT4 without benchmarking — quality drops sharply at INT2
- ✅ Use QAT over PTQ if you control the training pipeline — better accuracy at same bit-width

---

## 14. Interview Talking Points

### "What is the difference between RAG and fine-tuning? When would you use each?"

> RAG retrieves relevant documents at inference time and injects them into the prompt — no weight updates, knowledge stays fresh, hallucinations are traceable. Fine-tuning bakes knowledge into weights during training — better for changing the model's style, tone, or domain-specific language patterns. I'd use RAG when I need real-time or private data access, and fine-tuning when the task format itself (e.g., structured JSON extraction, medical coding) needs to be deeply learned. In production, I often combine both: fine-tune for behavior, RAG for knowledge.

### "Explain the Attention mechanism and why it replaced RNNs."

> Attention computes a weighted sum of all tokens' value vectors, where the weights are determined by the similarity between a query vector and each key vector — formalized as `softmax(QK^T / √d_k) × V`. This allows every token to directly attend to every other token in a single operation, regardless of distance. RNNs processed tokens sequentially and struggled to propagate gradients over long sequences (vanishing gradient problem). Transformers with attention process all tokens in parallel, enabling GPUs to be used efficiently and enabling long-range dependencies to be captured without degradation.

### "What is the Model Context Protocol and why does it matter?"

> MCP is Anthropic's open standard (2024) for connecting AI models to external tools and data sources. Before MCP, every AI application needed custom integration code for each tool — a database connector, a GitHub integration, a web search plugin. MCP standardizes this: any MCP-compatible server exposes Tools (callable functions), Resources (readable data), and Prompts (reusable templates), and any MCP-compatible host (Claude Desktop, IDE, custom app) can use them without bespoke wiring. It matters because it shifts AI integrations from O(N×M) custom code to O(N+M) standardized servers, enabling a composable ecosystem.

### "What is distillation and when would you use it over quantization?"

> Distillation trains a small student model to match the output distribution (soft probabilities) of a large teacher model — it transfers capability, not just compresses weights. Quantization reduces the numerical precision of existing weights (FP32 → INT4) to shrink memory and speed up inference without changing the architecture. I'd choose distillation when I need a permanently smaller model that can be deployed to edge devices and fine-tuned further. I'd choose quantization when I already have a trained model and need to deploy it faster or in less memory without retraining. Often both are applied: distill to 7B, then quantize to INT4.

### "How does Chain of Thought prompting work, and what are its limits?"

> Chain of Thought prompting instructs the model to produce explicit intermediate reasoning steps before the final answer. It works because LLMs are trained on human text that often shows worked reasoning (textbooks, forum posts, code comments), so CoT activates that learned pattern. It dramatically improves performance on arithmetic, logic, and multi-step reasoning. The limits: it increases token count and latency; it doesn't help if the model lacks the underlying knowledge; and "thinking" can be confidently wrong — generating plausible-sounding steps that lead to an incorrect conclusion. Reasoning models like o1 address this by training on verified CoT rather than just mimicking it.

### "What is Context Engineering and why is it not just prompt engineering?"

> Prompt engineering is about word choice within a single prompt. Context engineering is the higher-level discipline of architecting what goes into the full context window: how to structure the system prompt, what RAG chunks to retrieve and in what order, how to summarize conversation history to avoid degradation, and how to arrange tool results. As models support 1M-token contexts, naively filling that context causes the "lost in the middle" problem — models miss critical information sandwiched between many irrelevant tokens. Context engineering treats the context window as a cache to be managed: evict stale turns, prioritize high-signal chunks, use structured delimiters, and position critical instructions at the beginning or end.

---

## 15. Learning Resources

| Resource | Link | Type |
|---|---|---|
| 20 AI Concepts Explained in 40 Minutes | [YouTube](https://www.youtube.com/watch?v=OYvlznJ4IZQ) | Video |
| Attention Is All You Need (original paper) | [arXiv 1706.03762](https://arxiv.org/abs/1706.03762) | Paper |
| The Illustrated Transformer (Jay Alammar) | [jalammar.github.io](https://jalammar.github.io/illustrated-transformer/) | Blog |
| RAG — Azure AI Search Guide | [learn.microsoft.com](https://learn.microsoft.com/en-us/azure/search/retrieval-augmented-generation-overview) | Official Docs |
| Model Context Protocol | [modelcontextprotocol.io](https://modelcontextprotocol.io/) | Official Docs |
| RLHF — InstructGPT Paper | [arXiv 2203.02155](https://arxiv.org/abs/2203.02155) | Paper |
| llama.cpp (GGUF quantization) | [github.com/ggerganov/llama.cpp](https://github.com/ggerganov/llama.cpp) | GitHub |
| Anthropic Claude API Docs | [docs.anthropic.com](https://docs.anthropic.com) | Official Docs |
| Hugging Face — Quantization Guide | [huggingface.co/docs/transformers/quantization](https://huggingface.co/docs/transformers/quantization) | Official Docs |

---

*Last Updated: July 2026 | Source: Gaurav Sen — 20 AI Concepts Explained in 40 Minutes*

---

## Additional Material from 9-AI-Concepts-Explained.md

> Unique additions: ByteByteGo framing and concept-interactions.


> **Source:** [YouTube — 9 AI Concepts Explained in 7 minutes: AI Agents, RAGs, Tokenization, RLHF, Diffusion, LoRA...](https://www.youtube.com/watch?v=nVnxG10D5W0)  
> **Channel/Event:** ByteByteGo  
> **Topic:** Tokenization, Text Decoding, Prompt Engineering, AI Agents, RAG, RLHF, Diffusion Models, LoRA, A2A Protocol  
> **Key Claim:** 9 foundational AI/LLM concepts every AI engineer must know — from raw text to autonomous multi-agent systems

---

## Table of Contents

1. [Overview](#1-overview)
2. [Problem Statement](#2-problem-statement)
3. [Core Concepts — All 9 Defined](#3-core-concepts--all-9-defined)
4. [Architecture — The Modern AI Stack](#4-architecture--the-modern-ai-stack)
5. [Key Components](#5-key-components)
6. [How It Works — Concept Interactions](#6-how-it-works--concept-interactions)
7. [Comparison Tables](#7-comparison-tables)
8. [Code Examples](#8-code-examples)
9. [Best Practices](#9-best-practices)
10. [Interview Talking Points](#10-interview-talking-points)
11. [Learning Resources](#11-learning-resources)

---

## 1. Overview

This ByteByteGo reference covers the 9 most essential AI/LLM concepts for engineers building production AI systems. The concepts span the entire AI stack: from how text is converted into tokens a model can process (Tokenization), through alignment techniques that make models safe and useful (RLHF), efficient adaptation methods (LoRA), grounding techniques that reduce hallucination (RAG), generative models for images (Diffusion), and finally autonomous orchestration systems (AI Agents + A2A). Understanding these 9 concepts unlocks the ability to design, evaluate, and optimize end-to-end AI systems. Each concept addresses a specific gap between raw transformer capability and production-grade AI behavior. Together they form a complete mental model of the modern AI engineering stack.

---

## 2. Problem Statement

Raw transformer models trained on next-token prediction are powerful but insufficient for production use:

### Core Engineering Gaps

| Problem | Root Cause | Solution Concept |
|---|---|---|
| Models can't read text directly | Neural nets require numerical input | **Tokenization** |
| Deterministic outputs lack diversity | Greedy decoding picks most likely token | **Text Decoding strategies** |
| Generic prompts yield generic results | No task-specific guidance structure | **Prompt Engineering** |
| Models hallucinate facts | Knowledge frozen at training cutoff | **RAG** |
| Base models ignore instructions | Pure next-token prediction objective | **RLHF** |
| Full fine-tuning is prohibitively expensive | Updating billions of parameters | **LoRA** |
| LLMs can only generate text | No ability to act on the world | **AI Agents** |
| Agents cannot coordinate at scale | No standard communication protocol | **A2A Protocol** |
| Text-only architecture for multimodal tasks | Transformer outputs discrete tokens | **Diffusion Models** |

> **Key Insight:** "Without RLHF, an LLM would continue text in whatever direction seems statistically plausible, even when that direction is unhelpful or unsafe."

---

## 3. Core Concepts — All 9 Defined

### 1. Tokenization

Text is split into subword chunks (tokens) and mapped to integer IDs. A token is roughly 3/4 of a word in English. The model never sees raw text — it sees sequences of integers, which are then converted to dense vector embeddings before entering the transformer. Common algorithms: **BPE** (GPT family), **WordPiece** (BERT), **SentencePiece** (Llama/T5).

```
"Hello world" → ["Hello", " world"] → [9906, 1917] → [emb_9906, emb_1917]
```

Key implication: token count = API cost = context window consumption. Code and non-Latin scripts tokenize less efficiently than English prose.

### 2. Text Decoding

After the model computes logits (a score per vocabulary token), a decoding strategy selects the next token:

| Strategy | Method | Deterministic | Use Case |
|---|---|---|---|
| Greedy | Pick argmax token | Yes | Factual Q&A |
| Top-K sampling | Sample from K most likely | No | Creative writing |
| Top-P (nucleus) | Sample from tokens summing to P | No | Balanced tasks |
| Beam search | Explore B paths simultaneously | Yes | Translation |
| Temperature | Scale logits (low=sharp, high=flat) | Depends | Universal modifier |

### 3. Prompt Engineering

The art of structuring input to elicit desired model behavior without changing weights:

- **Zero-shot** — task described, no examples; relies on pretraining generalization.
- **Few-shot** — 2–5 examples embedded in the prompt demonstrate input-output format.
- **Chain-of-Thought (CoT)** — "Let's think step by step" forces intermediate reasoning; dramatically improves accuracy on math, logic, and multi-step tasks.
- **ReAct** — interleaves reasoning and tool-use traces; standard template for agent prompting.

### 4. AI Agents

Systems where an LLM acts as a reasoning engine that plans, calls tools, observes results, and iterates toward a goal. The core loop:

```
Perceive (input) → Think (reason) → Act (tool call) → Observe (result) → repeat
```

Tools can include: web search, code executors, database queries, API calls, file I/O. Memory types: in-context (conversation history), external (vector DB), episodic (past sessions).

### 5. RAG (Retrieval-Augmented Generation)

Before generating a response, the system retrieves relevant documents from an external knowledge base (vector DB) and injects them into the prompt as context. The model answers using retrieved facts rather than training memory alone — dramatically reducing hallucination on domain-specific, time-sensitive, or proprietary data.

Three retrieval modes:
- **Sparse** (keyword BM25): fast, exact term matching
- **Dense** (vector similarity): semantic matching via embeddings
- **Hybrid**: weighted combination of both; best recall in practice

### 6. RLHF (Reinforcement Learning from Human Feedback)

A post-training alignment technique using three sequential stages:

1. **Supervised Fine-Tuning (SFT)** — fine-tune on human-written demonstrations of ideal responses.
2. **Reward Model Training** — train a separate model to score outputs by learning from human preference pairs (A preferred over B).
3. **RL Optimization (PPO)** — use the reward model as a signal to further optimize the LLM via Proximal Policy Optimization, penalizing KL divergence from the SFT model to prevent mode collapse.

Result: models that follow instructions, refuse harmful requests, and maintain helpful, harmless tone.

### 7. Diffusion Models

Generative models that learn to reverse a noise-addition process:

- **Forward process (training)**: progressively add Gaussian noise to real data over T timesteps until data becomes pure noise. Model learns to predict the noise added at each step.
- **Reverse process (inference)**: start from pure Gaussian noise; the trained network (U-Net for images, DiT for latent diffusion) iteratively removes noise over T steps to produce a clean sample.
- **Conditioning**: text-to-image models inject text embeddings (CLIP) at each denoising step to guide generation.

Used in: Stable Diffusion, DALL-E 3, Midjourney, Sora (video).

### 8. LoRA (Low-Rank Adaptation)

A Parameter-Efficient Fine-Tuning (PEFT) technique. Key insight: weight changes during fine-tuning are intrinsically **low-rank** — they can be approximated by the product of two small matrices without losing significant quality.

**Mechanism**: freeze original weight matrix `W ∈ ℝ^(d×d)`, inject trainable decomposition `ΔW = B × A` where `B ∈ ℝ^(d×r)` and `A ∈ ℝ^(r×d)`, with rank `r << d`. Only A and B are trained.

- Typical ranks: r = 4, 8, 16 (lower = fewer params = more regularization)
- Parameter savings: 0.1%–1% of total model parameters
- At inference: merge adapters into base weights (zero overhead) or keep separate (swappable per task)

### 9. Agent-to-Agent (A2A) Protocol

A standardized open protocol (Google, 2025) enabling specialized AI agents to communicate, delegate subtasks, and coordinate asynchronously — without sharing internal state or model weights.

Key concepts:
- **Agent Card** (JSON manifest): declares agent capabilities, input/output types, authentication
- **Task lifecycle**: `submitted → working → completed/failed`
- **Streaming support**: agents can stream intermediate results back to orchestrators
- **Push notifications**: long-running tasks notify via webhook when complete

Enables: research agent + coding agent + review agent running in parallel, orchestrated by a planner agent.

---

## 4. Architecture — The Modern AI Stack

```mermaid
flowchart TD
    subgraph Foundation ["Foundation — Text Processing"]
        TOK["Tokenization\n(text to token IDs to embeddings)"]
        DEC["Text Decoding\n(greedy / top-P / temperature)"]
        PE2["Prompt Engineering\n(zero-shot / few-shot / CoT)"]
    end

    subgraph Adaptation ["Model Adaptation"]
        RLHF2["RLHF\n(SFT + reward model + PPO)"]
        LORA["LoRA\n(freeze weights + low-rank adapters)"]
    end

    subgraph Augmentation ["Knowledge Augmentation"]
        RAG2["RAG\n(retrieve + inject + generate)"]
        DIFF["Diffusion Models\n(noise to image generation)"]
    end

    subgraph Orchestration ["Task Orchestration"]
        AGENT["AI Agents\n(plan + act + observe loop)"]
        A2A2["A2A Protocol\n(agent-to-agent communication)"]
    end

    TOK --> DEC --> PE2
    PE2 --> RLHF2
    PE2 --> LORA
    RLHF2 --> RAG2
    LORA --> RAG2
    RAG2 --> AGENT
    AGENT --> A2A2
    TOK --> DIFF

    style TOK fill:#0078D4,color:#fff
    style DEC fill:#0078D4,color:#fff
    style PE2 fill:#0078D4,color:#fff
    style RLHF2 fill:#5C2D91,color:#fff
    style LORA fill:#5C2D91,color:#fff
    style RAG2 fill:#107C10,color:#fff
    style DIFF fill:#D83B01,color:#fff
    style AGENT fill:#107C10,color:#fff
    style A2A2 fill:#107C10,color:#fff
    style Foundation fill:#EFF6FC,stroke:#0078D4
    style Adaptation fill:#F3EFF8,stroke:#5C2D91
    style Augmentation fill:#DFF6DD,stroke:#107C10
    style Orchestration fill:#DFF6DD,stroke:#107C10
```

---

## 5. Key Components

| Concept | Category | Core Mechanism | Primary Trade-off |
|---|---|---|---|
| Tokenization | Pre-processing | BPE / WordPiece subword splitting | Vocabulary size vs OOV handling |
| Text Decoding | Inference | Logit scoring + sampling strategy | Determinism vs diversity |
| Prompt Engineering | Inference | Structured input templates + examples | Prompt length vs model capacity |
| RLHF | Post-training | SFT + reward model + PPO loop | Alignment quality vs reward hacking |
| LoRA | Fine-tuning | Low-rank weight decomposition | Rank vs parameter efficiency |
| RAG | Augmentation | Vector search + context injection | Retrieval recall vs context length |
| Diffusion Models | Generation | Iterative denoising via neural network | Steps vs quality vs speed |
| AI Agents | Orchestration | LLM + tool use + observe-iterate loop | Autonomy vs cost vs safety |
| A2A Protocol | Multi-agent | Agent Card discovery + task delegation | Specialization vs coordination overhead |

---

## 6. How It Works — Concept Interactions

### RAG Pipeline — End to End

```mermaid
sequenceDiagram
    participant User
    participant RAGSystem as "RAG System"
    participant VectorDB as "Vector DB"
    participant LLM

    User->>RAGSystem: Submit query
    RAGSystem->>VectorDB: Embed query with same embedding model
    VectorDB-->>RAGSystem: Return top-K similar document chunks
    RAGSystem->>LLM: System context + retrieved chunks + user query
    LLM-->>RAGSystem: Answer grounded in retrieved documents
    RAGSystem-->>User: Final response with source citations
```

### LoRA Fine-Tuning — Weight Decomposition

```mermaid
flowchart LR
    Input(["Input x"])

    subgraph Frozen ["Frozen Pre-trained Weights"]
        W["W\n(d x d — no gradient)"]
    end

    subgraph Adapters ["Trainable LoRA Adapters"]
        A["A\n(d x r — trained)"]
        B["B\n(r x d — trained)"]
    end

    Merge["Add: Wx + BAx\n(combined output)"]
    Output(["Output h"])

    Input --> W --> Merge
    Input --> A --> B --> Merge
    Merge --> Output

    style W fill:#EFF6FC,stroke:#0078D4
    style A fill:#5C2D91,color:#fff
    style B fill:#5C2D91,color:#fff
    style Merge fill:#107C10,color:#fff
    style Input fill:#0078D4,color:#fff
    style Output fill:#0078D4,color:#fff
```

### AI Agent Execution Loop

```mermaid
flowchart TD
    Start(["User Goal / Task"])
    Think["LLM: Think\n(reason about next step)"]
    Decide{Tool\nNeeded?}
    Act["Execute Tool\n(search / code / API / DB)"]
    Observe["Observe Result\n(parse and integrate tool output)"]
    Done{Goal\nAchieved?}
    End(["Return Final Answer"])

    Start --> Think --> Decide
    Decide -->|Yes| Act --> Observe --> Done
    Decide -->|No| Done
    Done -->|No| Think
    Done -->|Yes| End

    style Start fill:#0078D4,color:#fff
    style End fill:#107C10,color:#fff
    style Done fill:#D83B01,color:#fff
    style Decide fill:#5C2D91,color:#fff
```

### Diffusion Model — Forward and Reverse

```mermaid
flowchart LR
    subgraph Forward ["Forward Process (Training)"]
        I0["Clean Image x0"]
        I1["Noisy x1"]
        I2["Noisy x2"]
        IN["Pure Noise xT"]
        I0 -->|"add noise"| I1 -->|"add noise"| I2 -->|"..."| IN
    end

    subgraph Reverse ["Reverse Process (Inference)"]
        N0["Pure Noise xT"]
        N1["Denoised xt-1"]
        N2["Denoised xt-2"]
        NF["Generated Image x0"]
        N0 -->|"U-Net denoise"| N1 -->|"U-Net denoise"| N2 -->|"..."| NF
    end

    style I0 fill:#107C10,color:#fff
    style IN fill:#D83B01,color:#fff
    style N0 fill:#D83B01,color:#fff
    style NF fill:#107C10,color:#fff
    style Forward fill:#EFF6FC,stroke:#0078D4
    style Reverse fill:#DFF6DD,stroke:#107C10
```

---

## 7. Comparison Tables

### Fine-Tuning: Full vs LoRA

| Dimension | Full Fine-Tuning | LoRA |
|---|---|---|
| Parameters updated | 100% of weights | 0.1%–1% (adapters only) |
| GPU memory required | Very high (70B → 8× A100) | Low (fits single consumer GPU) |
| Training time | Days to weeks | Hours to days |
| Storage per task variant | Full model copy (50–140GB) | Small adapter file (5–50MB) |
| Output quality | Highest possible | Near full fine-tune quality |
| Multi-task support | Requires separate models | Swap adapter files at runtime |
| Merge at inference | N/A | Yes — merge for zero overhead |

### LLM Without RAG vs With RAG

| Dimension | Vanilla LLM | RAG-augmented LLM |
|---|---|---|
| Knowledge freshness | Frozen at training cutoff | Real-time via vector DB |
| Hallucination rate | High on domain-specific facts | Low — grounded in retrieved docs |
| Context utilization | Static context window | Scales with retrieval corpus |
| Latency | Generation only | Retrieval + generation |
| Data privacy | Training data exposure risk | Data stays in private vector DB |
| Best for | General world knowledge | Enterprise / proprietary / live data |

### RLHF Stages vs Objective

| Stage | Technique | What It Optimizes | Risk |
|---|---|---|---|
| SFT | Supervised learning | Instruction format adherence | Distribution shift from base model |
| Reward Model | Preference learning | Human preference score | Reward misspecification |
| PPO | Reinforcement learning | Expected reward signal | Reward hacking / mode collapse |

### Decoding Strategy Trade-offs

| Strategy | Determinism | Diversity | Best For |
|---|---|---|---|
| Greedy | Fully deterministic | None | Factual Q&A, classification |
| Top-K sampling | Non-deterministic | High | Creative writing |
| Top-P (nucleus) | Non-deterministic | Controlled | Balanced tasks |
| Beam search | Deterministic | Low | Translation, summarization |
| Temperature scaling | Adjustable | Adjustable | Universal modifier |

---

## 8. Code Examples

### Python — Tokenization with tiktoken

```python
import tiktoken

enc = tiktoken.encoding_for_model("gpt-4o")
text = "Hello, how are you today?"

tokens = enc.encode(text)
print(f"Token IDs:    {tokens}")         # [9906, 11, 1268, 527, 499, 3432, 30]
print(f"Token count:  {len(tokens)}")    # 7
print(f"Decoded:      {enc.decode(tokens)}")

# Count before API call to avoid surprises
MAX_TOKENS = 128_000
if len(tokens) > MAX_TOKENS:
    raise ValueError(f"Input too long: {len(tokens)} tokens")
```

### Python — Minimal RAG Pipeline

```python
from openai import OpenAI
import numpy as np

client = OpenAI()

def embed(text: str) -> list[float]:
    return client.embeddings.create(
        model="text-embedding-3-small", input=text
    ).data[0].embedding

def cosine_similarity(a, b):
    return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))

# Knowledge base (would be pre-indexed in production)
docs = [
    "Azure AI Search supports vector, keyword, and hybrid search.",
    "LoRA inserts low-rank adapters into transformer attention projection layers.",
    "RLHF uses a reward model trained on human preference comparison pairs.",
]
doc_embeddings = [embed(d) for d in docs]

def rag_query(question: str, top_k: int = 2) -> str:
    q_emb = embed(question)
    scores = [cosine_similarity(q_emb, d) for d in doc_embeddings]
    top_docs = [docs[i] for i in np.argsort(scores)[-top_k:][::-1]]

    context = "\n".join(f"- {d}" for d in top_docs)
    response = client.chat.completions.create(
        model="gpt-4o",
        messages=[
            {"role": "system", "content": f"Answer using ONLY this context:\n{context}"},
            {"role": "user", "content": question},
        ]
    )
    return response.choices[0].message.content

print(rag_query("What is LoRA?"))
```

### Python — LoRA Fine-Tuning Setup with PEFT

```python
from transformers import AutoModelForCausalLM, AutoTokenizer
from peft import LoraConfig, get_peft_model, TaskType

base_model = "meta-llama/Llama-3.2-3B"
model = AutoModelForCausalLM.from_pretrained(base_model)

lora_config = LoraConfig(
    task_type=TaskType.CAUSAL_LM,
    r=16,                                       # rank — lower = fewer params
    lora_alpha=32,                              # scaling factor (alpha/r = 2)
    target_modules=["q_proj", "v_proj"],        # attention projection layers
    lora_dropout=0.05,
    bias="none",
)

peft_model = get_peft_model(model, lora_config)
peft_model.print_trainable_parameters()
# trainable params: 4,194,304 || all params: 3,216,187,392 || trainable%: 0.13%

# Save adapter only (not full model)
peft_model.save_pretrained("./my-lora-adapter")

# Merge adapter into base at inference time (zero overhead)
merged = peft_model.merge_and_unload()
```

### Python — AI Agent with Tool Use (Anthropic SDK)

```python
from anthropic import Anthropic

client = Anthropic()

tools = [
    {
        "name": "web_search",
        "description": "Search the web for current information on a topic",
        "input_schema": {
            "type": "object",
            "properties": {
                "query": {"type": "string", "description": "Search query"}
            },
            "required": ["query"],
        },
    }
]

def execute_tool(name: str, inputs: dict) -> str:
    if name == "web_search":
        return f"[Search results for: {inputs['query']}]"
    return "Unknown tool"

def run_agent(task: str, max_iterations: int = 10) -> str:
    messages = [{"role": "user", "content": task}]

    for _ in range(max_iterations):
        response = client.messages.create(
            model="claude-sonnet-4-6",
            max_tokens=1024,
            tools=tools,
            messages=messages,
        )

        if response.stop_reason == "end_turn":
            return next(b.text for b in response.content if hasattr(b, "text"))

        tool_use = next(b for b in response.content if b.type == "tool_use")
        tool_result = execute_tool(tool_use.name, tool_use.input)

        messages.extend([
            {"role": "assistant", "content": response.content},
            {"role": "user", "content": [{
                "type": "tool_result",
                "tool_use_id": tool_use.id,
                "content": tool_result,
            }]},
        ])

    return "Max iterations reached"

print(run_agent("What are the latest developments in LoRA fine-tuning?"))
```

### Install / Setup

```bash
# Tokenization
pip install tiktoken

# RAG with OpenAI
pip install openai numpy

# LoRA / PEFT fine-tuning
pip install transformers peft accelerate bitsandbytes

# AI Agents (Anthropic SDK)
pip install anthropic

# Diffusion models
pip install diffusers transformers torch accelerate
```

---

## 9. Best Practices

### Tokenization
- ✅ Count tokens before API calls to avoid truncation surprises
- ✅ Use the same tokenizer for embedding model and LLM in a RAG pipeline
- ✅ Budget ~1.3 tokens/word for English; 2-4× more for code or non-Latin scripts
- ❌ Assume 1 token = 1 word — this breaks cost and context estimates

### RAG
- ✅ Chunk documents at semantic boundaries (paragraphs, not fixed character counts)
- ✅ Use hybrid search (vector + BM25 keyword) — better recall than dense alone
- ✅ Include source citations in retrieved context to enable attribution
- ✅ Re-rank top-K results with a cross-encoder before injection
- ❌ Embed full pages — long chunks dilute similarity scores
- ❌ Skip evaluation — measure recall@K and answer faithfulness separately

### LoRA
- ✅ Target attention projection layers (`q_proj`, `v_proj`) first; add `k_proj`/`o_proj` if underfitting
- ✅ Keep rank r ≤ 64; rank 8–16 is sufficient for most domain adaptation tasks
- ✅ Merge adapters into base model before deployment for zero inference overhead
- ✅ Use multiple task-specific adapters with the same frozen base (efficient multi-task)
- ❌ Use LoRA as a substitute for quality data — data quality dominates all adapter choices

### AI Agents
- ✅ Define strict JSON schemas for tools — models use exactly what they're given
- ✅ Cap the agent loop with a max-iterations guard to prevent runaway API costs
- ✅ Log every think/act/observe cycle for post-hoc debugging and evaluation
- ✅ Use structured output for agent decisions (JSON mode or tool forcing)
- ❌ Give agents irreversible tools (delete, send, publish) without human-in-the-loop confirmation

### Prompt Engineering
- ✅ Use chain-of-thought for any task requiring multi-step reasoning
- ✅ Few-shot examples should cover edge cases, not just the happy path
- ✅ Test prompts with adversarial inputs before production
- ❌ Add more instructions expecting better results — reduce and clarify instead

### RLHF
- ✅ Collect preference pairs from diverse annotators to reduce labeler bias
- ✅ Monitor KL divergence from SFT model during PPO — sharp spikes = instability
- ❌ Optimize reward score as an end goal — reward hacking degrades real quality

---

## 10. Interview Talking Points

### "What is tokenization and why does it matter for LLM performance?"

> Tokenization converts raw text into integer IDs the model can process — the model never sees characters or words directly. The tokenizer's vocabulary (built via BPE or WordPiece) determines encoding efficiency: English averages ~1.3 tokens per word, but code and non-Latin scripts can be 2–4× less efficient. Token count directly controls API cost, context window consumption, and latency. A critical production detail: the embedding model used in a RAG pipeline must share the same tokenizer as the LLM, or retrieved chunk boundaries won't align with model expectations.

---

### "When would you choose RAG over fine-tuning, and when would you combine both?"

> RAG is the right choice when the knowledge base changes frequently (product catalogues, policies, prices) or is proprietary and can't be included in training data — it externalizes facts to a searchable store the model queries at inference time. Fine-tuning is better for adopting a specific reasoning style, output format, or domain-specific vocabulary pattern: behaviors that can't be solved by injecting context. The best production systems combine both — a LoRA-fine-tuned model (for style, tone, and format adherence) layered with RAG (for fresh, grounded facts). Think of fine-tuning as teaching how to think; RAG as providing what to think about.

---

### "Explain LoRA as if you're interviewing for a senior ML engineering role."

> LoRA exploits an empirical observation: weight updates during fine-tuning are intrinsically low-rank, meaning the update matrix ΔW ∈ ℝ^(d×d) can be closely approximated by the product B×A where B ∈ ℝ^(d×r) and A ∈ ℝ^(r×d), with r << d. The original weights are frozen — only the two small adapter matrices are trained, reducing trainable parameters from 100% to 0.1%–1% of the model. A 7B model with rank-16 LoRA might train 4M parameters instead of 7B, making fine-tuning feasible on a single consumer GPU. At inference, adapters can be merged into the base weights for zero latency overhead, or kept separate for per-task hot-swapping.

---

### "What is RLHF and what specific problem does it solve that SFT alone cannot?"

> RLHF solves the alignment gap between statistical plausibility and human preference. SFT teaches the model to imitate human-written responses — it improves format adherence but can still produce confident-sounding harmful or unhelpful outputs. RLHF adds two additional stages: a reward model trained on human preference comparisons (A is better than B) learns to score outputs as humans would; then PPO optimizes the LLM against this reward signal while penalizing KL divergence from the SFT baseline to prevent mode collapse. The result is a model that reliably follows instructions, refuses unsafe requests, and maintains helpful tone — behaviors that next-token prediction alone cannot produce because they require modeling human intent, not just text distributions.

---

### "How do AI Agents differ from a simple LLM API call, and what role does A2A play?"

> A single LLM API call produces text from static input — it's a one-shot function. An AI Agent is a system where the LLM operates as a reasoning engine inside a continuous sense-plan-act loop: receive a goal, decide which tool to invoke, execute the tool (web search, code executor, database), observe the result, then decide whether to continue iterating or return a final answer. This loop enables multi-step task decomposition that no single prompt can handle. A2A extends this into multi-agent collaboration: each agent publishes an Agent Card (a JSON manifest declaring capabilities, I/O schema, and auth), allowing an orchestrator to discover specialized agents and delegate subtasks to them in parallel — a research agent, coding agent, and review agent can work concurrently without sharing model weights or internal state.

---

## 11. Learning Resources

| Resource | Link | Type |
|---|---|---|
| ByteByteGo — 9 AI Concepts | [YouTube](https://www.youtube.com/watch?v=nVnxG10D5W0) | Video |
| Hugging Face PEFT / LoRA Docs | [huggingface.co/docs/peft](https://huggingface.co/docs/peft) | Official Docs |
| Microsoft Learn — RAG Overview | [learn.microsoft.com](https://learn.microsoft.com/en-us/azure/search/retrieval-augmented-generation-overview) | Official Docs |
| Hugging Face Diffusers | [huggingface.co/docs/diffusers](https://huggingface.co/docs/diffusers) | Official Docs |
| Anthropic Tool Use Guide | [docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/tool-use) | Official Docs |
| Google A2A Protocol Spec | [google.github.io/A2A](https://google.github.io/A2A) | Spec |
| OpenAI Tokenizer (interactive) | [platform.openai.com/tokenizer](https://platform.openai.com/tokenizer) | Tool |
| InstructGPT Paper (RLHF) | [arxiv.org/abs/2203.02155](https://arxiv.org/abs/2203.02155) | Research Paper |
| LoRA Paper (Hu et al., 2021) | [arxiv.org/abs/2106.09685](https://arxiv.org/abs/2106.09685) | Research Paper |

---

*Last Updated: June 2026 | Source: ByteByteGo — 9 AI Concepts Explained in 7 Minutes*

---

## Source Attribution

| Section | Primary Source | Additional Sources |
|---|---|---|
| 20 core AI concepts | 20-AI-Concepts-Explained.md | 9-AI-Concepts-Explained.md |
| ByteByteGo framing, concept interactions | 9-AI-Concepts-Explained.md | — |

*Consolidated: July 2026 | DocDedupAnalyzer Agent v1.0*
*Original files: 20-AI-Concepts-Explained.md, 9-AI-Concepts-Explained.md | Zero data loss guaranteed*
