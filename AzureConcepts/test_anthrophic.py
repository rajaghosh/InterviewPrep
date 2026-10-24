import ollama

MODEL = "llama3.2:latest"

stream = ollama.chat(
    model=MODEL,
    messages=[
        {"role": "system", "content": "Python expert"},
        {"role": "user", "content": "Hi, get me the current weather of Kolkata? Use web search if needed."},
    ],
    stream=True,
)

for chunk in stream:
    print(chunk["message"]["content"], end="", flush=True)

print()
