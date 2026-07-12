# Open-Source AI Tools — Curated Reference List

> 68 essential open-source tools across 10 categories every AI practitioner should know.

---

## 01 · LLM INFERENCE
*run models locally. no api. no limits. no bill.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 01 | **ollama** | ★98K | Run llama, mistral, qwen, gemma locally with one command. Supports GPU acceleration, REST API, OpenAI-compatible endpoints. `github.com/ollama/ollama` |
| 02 | **llama.cpp** | ★72K | LLM inference in pure C++. Runs on CPU, GPU, Apple Silicon. If ollama is the car, llama.cpp is the engine. `github.com/ggml-org/llama.cpp` |
| 03 | **vllm** | ★44K | High-throughput LLM serving for production. Continuous batching, paged attention, OpenAI-compatible API. `github.com/vllm-project/vllm` |
| 04 | **lm studio** | ★28K | Desktop app for running local LLMs. Download from Hugging Face, get an OpenAI-compatible local server. Best for non-developers. `github.com/lmstudio-ai/lmstudio.js` |
| 05 | **jan** | ★26K | Open-source ChatGPT alternative that runs 100% offline. Clean UI, model management, local API. No data leaves your machine. `github.com/janhq/jan` |
| 06 | **text-gen-webui** | ★42K | Swiss army knife for local LLMs. Every model format, every backend. Character mode, notebook mode, API mode. `github.com/oobabooga/text-generation-webui` |
| 07 | **localai** | ★26K | Self-hosted OpenAI drop-in replacement. Same API, local models. Swap GPT/Claude without changing a line of code. `github.com/mudler/LocalAI` |

---

## 02 · RAG & KNOWLEDGE
*make AI answer from your own data.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 08 | **langchain** | ★98K | Most popular LLM framework. Chains, agents, retrievers, memory. Connects LLMs to any data source or tool. `github.com/langchain-ai/langchain` |
| 09 | **llamaindex** | ★38K | Data framework for LLM apps. Index PDF, SQL, Notion, Slack — query with natural language. Better than LangChain for pure RAG. `github.com/run-llama/llama_index` |
| 10 | **rag-anything** | ★12K | Multimodal RAG. Handles text, tables, images, charts, graphs. 6 lines to set up. `github.com/HKUDS/RAG-Anything` |
| 11 | **chroma** | ★16K | Open-source vector database. Store embeddings, search by similarity, filter by metadata. Simplest semantic search setup. `github.com/chroma-core/chroma` |
| 12 | **weaviate** | ★12K | Vector database with built-in ML models. Hybrid search, multi-tenancy, scales to billions of objects. `github.com/weaviate/weaviate` |
| 13 | **haystack** | ★18K | End-to-end NLP framework for RAG pipelines. Modular, production-ready, works with any LLM or vector DB. `github.com/deepset-ai/haystack` |
| 14 | **docling** | ★22K | Convert documents to structured markdown for AI. Handles PDFs with tables, figures, formulas. Built by IBM Research. `github.com/DS4SD/docling` |

---

## 03 · AI AGENTS
*let the model act, not just answer.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 15 | **autogen** | ★40K | Multi-agent conversation framework by Microsoft. Agents delegate tasks, write and execute code. `github.com/microsoft/autogen` |
| 16 | **crewai** | ★28K | Orchestrate role-playing AI agents. Define a crew, assign roles, set goals — agents collaborate like a team. `github.com/crewAIInc/crewAI` |
| 17 | **langgraph** | ★10K | Stateful multi-agent workflows as graphs. Handles complex logic, loops, human-in-the-loop. `github.com/langchain-ai/langgraph` |
| 18 | **agno** | ★22K | Fast multi-modal AI agents. Any LLM, any tool, memory, knowledge, storage. 10x faster than LangChain for simple agents. `github.com/agno-agi/agno` |
| 19 | **smolagents** | ★14K | Minimal agent framework by Hugging Face. Code agents that write and execute Python. 1000 lines total. The anti-LangChain. `github.com/huggingface/smolagents` |
| 20 | **openhands** | ★48K | Open-source Devin. AI engineer that writes code, runs tests, fixes bugs, deploys. Works with Claude, GPT-4, local models. `github.com/All-Hands-AI/OpenHands` |
| 21 | **superagi** | ★16K | Self-hosted autonomous agent infrastructure. Agent marketplace, performance telemetry, concurrent agents, graphical UI. `github.com/TransformerOptimus/SuperAGI` |

---

## 04 · PROMPTS & EVALS
*stop guessing. start measuring.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 29 | **dspy** | ★22K | Programming — not prompting — LLMs. DSPy optimizes prompts automatically. From Stanford NLP. `github.com/stanfordnlp/dspy` |
| 30 | **guidance** | ★20K | Control LLM output structure with code. Interleave generation with logic, force JSON schemas, constrain outputs. `github.com/guidance-ai/guidance` |
| 31 | **outlines** | ★11K | Structured text generation. Force valid JSON, regex patterns, specific formats. Guaranteed output structure. `github.com/dottxt-ai/outlines` |
| 32 | **promptfoo** | ★6K | Test and eval your prompts. Run automated tests, compare models, catch regressions. Unit tests for AI. `github.com/promptfoo/promptfoo` |
| 33 | **braintrust** | ★3K | Eval framework for LLM apps. Track quality across model versions, prompts, configurations. Because vibes aren't a metric. `github.com/brainlid/langchain` |
| 34 | **instructor** | ★9K | Structured outputs via Pydantic. Define a schema, get back a validated Python object. Works with OpenAI, Anthropic, Google. `github.com/instructor-ai/instructor` |

---

## 05 · FINE-TUNING
*make the model yours.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 35 | **unsloth** | ★24K | Fine-tune LLMs 2x faster, 80% less memory. Supports Llama, Mistral, Qwen, Gemma. Runs on a single GPU. `github.com/unslothai/unsloth` |
| 36 | **axolotl** | ★8K | Streamlined fine-tuning. YAML config, every dataset format, every training technique. The ops layer on top of Hugging Face. `github.com/axolotl-org/axolotl` |
| 37 | **llama-factory** | ★40K | Fine-tune 100+ LLMs with zero code. Web UI, supports LoRA, QLoRA, full fine-tuning. Most user-friendly tool available. `github.com/hiyouga/LLaMA-Factory` |
| 38 | **trl** | ★12K | Transformer Reinforcement Learning. RLHF, DPO, PPO — techniques used to align GPT-4 and Claude. By Hugging Face. `github.com/huggingface/trl` |
| 39 | **torchtune** | ★5K | PyTorch-native fine-tuning from Meta. Simple, hackable, well-documented. Reference implementation in pure PyTorch. `github.com/pytorch/torchtune` |
| 40 | **mergekit** | ★4K | Merge multiple fine-tuned models into one. SLERP, TIES, DARE, linear merge. No GPU needed. Create Frankenstein models that outperform. `github.com/arcee-ai/mergekit` |

---

## 06 · TOOLS & CONTEXT
*feed the model what it actually needs.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 41 | **markitdown** | ★38K | Convert any file to markdown. PDF, Word, Excel, PowerPoint, images, audio. Clean structured text for your LLM. By Microsoft. `github.com/microsoft/markitdown` |
| 42 | **files-to-prompt** | ★3K | Turn your entire codebase into one prompt. Respects .gitignore, recursive, filterable. By Simon Willison. `github.com/simonw/files-to-prompt` |
| 43 | **crawl4ai** | ★30K | Web scraping for AI. Extracts clean markdown from any URL, handles JS-heavy sites, structured data extraction. `github.com/unclecode/crawl4ai` |
| 44 | **firecrawl** | ★25K | Turn any website into LLM-ready data. Full site crawling, structured extraction, clean markdown output. `github.com/mendableai/firecrawl` |
| 45 | **playwright-mcp** | ★31K | Give Claude a real browser. Navigate, click, screenshot, read dynamic content. Analyze any site in 30 seconds. `github.com/microsoft/playwright-mcp` |
| 46 | **m-c-p** | ★11K | Standard for connecting Claude to external tools. Official Anthropic MCP. Plug in any API, database, service. `github.com/anthropics/model-context-protocol` |
| 47 | **mcp-servers** | ★27K | Ready-made MCP servers. GitHub, Slack, Notion, databases, browsers, finance. Every integration in one catalog. `github.com/punkpeye/awesome-mcp-servers` |

---

## 07 · DEPLOYMENT
*ship it. then scale it.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 49 | **litellm** | ★16K | One API for 100+ LLMs. OpenAI format, works with Claude, GPT, Gemini, local models. Load balancing, fallbacks, cost tracking. `github.com/BerriAI/litellm` |
| 50 | **bentoml** | ★7K | Build and deploy AI services. Package models, create APIs, deploy anywhere. From local to production Kubernetes. `github.com/bentoml/BentoML` |
| 51 | **ray serve** | ★34K | Distributed AI inference at scale. Serve multiple models, autoscale, handle millions of requests. Used by OpenAI. `github.com/ray-project/ray` |
| 52 | **triton inference** | ★8K | NVIDIA's production inference server. Maximum GPU utilization, dynamic batching, multi-model serving. `github.com/triton-inference-server/server` |
| 53 | **lorax** | ★3K | Serve hundreds of LoRA fine-tuned models on one GPU. One base model, hundreds of adapters. 10x cost reduction. `github.com/predibase/lorax` |
| 54 | **supabase** | ★73K | Default backend for AI apps. Open-source Firebase on Postgres. Real-time DB, auth, edge functions, vector search. `github.com/supabase/supabase` |

---

## 08 · CLAUDE-SPECIFIC
*if you use Claude, these are not optional.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 55 | **superpowers** | ★160K | Adds superpowers to Claude Code. Deep code analysis, auto-refactor, project-wide editing. Most popular Claude enhancement. `github.com/obra/superpowers` |
| 56 | **claude-skills** | official | Official Anthropic skills framework. skill.md patterns that teach Claude to handle documents, automations, workflows. `github.com/anthropics/claude-code-skills` |
| 57 | **free-claude** | ★2K | Run Claude Code completely free via GitHub Models API. Trending #1 on GitHub. $0 forever. `github.com/Alishahryar1/free-claude-code` |
| 58 | **claude-mem** | ★1K | Persistent memory for Claude. Auto-captures everything across sessions. Claude remembers who you are & what you're working on. `github.com/thedotmack/claude-mem` |

---

## 09 · DATA PREP
*garbage in, garbage out. fix the input.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 59 | **unstructured** | ★10K | Extract and transform unstructured data for LLMs. PDFs, HTML, Word, images, emails — clean chunks for RAG. `github.com/Unstructured-IO/unstructured` |
| 60 | **datatrove** | ★3K | Large-scale data processing for LLM training by Hugging Face. Deduplication, quality filtering, content classification. `github.com/huggingface/datatrove` |
| 61 | **trafilatura** | ★3K | Web content extraction for AI. Strips boilerplate, keeps content, outputs clean text or markdown. `github.com/adbar/trafilatura` |
| 62 | **semchunk** | ★1K | Semantic text chunking for RAG. Splits at natural boundaries instead of arbitrary token counts. Better chunks → better answers. `github.com/umarbutler/semchunk` |
| 63 | **datachain** | ★2K | AI-native dataset management. Version, query, transform multimodal datasets. Images, video, text, embeddings. `github.com/iterative/datachain` |

---

## 10 · VISION & MULTIMODAL
*beyond text.*

| # | Tool | Stars | Description |
|---|------|-------|-------------|
| 64 | **moondream** | ★10K | Tiny vision language model. 1.6B parameters. Describe images, answer visual questions, detect objects. Runs on a Raspberry Pi. `github.com/vikhyat/moondream` |
| 65 | **internvl** | ★7K | State of the art open-source vision model. Matches GPT-4V on most benchmarks. Understands images, charts, documents, screenshots. `github.com/OpenGVLab/InternVL` |
| 66 | **whisper** | ★74K | Open-source speech recognition by OpenAI. Transcribes audio in 99 languages. Handles accents, background noise, jargon. `github.com/openai/whisper` |
| 67 | **fast-whisper** | ★8K | Whisper but 10-20x faster. One command, automatic GPU optimization. Transcribe a 2-hour podcast in 2 minutes. `github.com/Vaibhavs10/insanely-fast-whisper` |
| 68 | **diffusion-webui** | ★143K | Browser interface for Stable Diffusion. Generate, edit, upscale images from text. Hundreds of extensions, ControlNet, inpainting. `github.com/AUTOMATIC1111/stable-diffusion-webui` |

---

## Quick Selection Guide

| Use Case | Top Pick | Why |
|----------|----------|-----|
| Run any LLM locally | **ollama** | One command, GPU support, REST API |
| Build a RAG pipeline | **llamaindex** + **chroma** | Best-in-class for document querying |
| Create AI agents | **langgraph** | Stateful, handles loops and HITL |
| Fine-tune on one GPU | **unsloth** | 2x faster, 80% less memory |
| Serve 100+ LLMs via one API | **litellm** | Drop-in OpenAI proxy for everything |
| Structured LLM output | **instructor** | Pydantic-validated, works everywhere |
| Eval your prompts | **promptfoo** | Automated test suites for prompts |
| Scrape web for AI | **crawl4ai** | Clean markdown from any URL |
| Speech-to-text | **whisper** | 99 languages, production-grade |
| Image generation UI | **diffusion-webui** | 143K stars, every extension imaginable |
