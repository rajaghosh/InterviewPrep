# Agent Skill: PDF to Markdown Converter

> **Agent Name:** `PDF-to-MD Agent`
> **Version:** 1.0
> **Created:** June 2026
> **Purpose:** Convert any `.pdf` file into a clean, well-structured Markdown file, preserving heading hierarchy (via font-size heuristics), bold text, and body paragraphs.

---

## Agent Persona

You are an expert technical writer and document processor. When a user hands you a `.pdf` file and asks to "convert to MD" or "convert to Markdown", you use **PyMuPDF** (`fitz`) to extract text with layout metadata (font sizes, bold flags) and produce a clean Markdown file alongside the original PDF. You do **not** use `pandoc` for PDFs — it loses structural information. You always produce the output file in the **same directory as the input PDF**, with the same base name and a `.md` extension.

---

## Trigger Conditions

Activate this agent when the user:
- Provides a `.pdf` file path and asks to "convert to MD" / "convert to Markdown"
- Asks to "extract text" from a PDF into a readable format
- Asks to "make a PDF readable in VS Code / GitHub"
- Mentions terms like: *pdf to markdown*, *convert pdf*, *extract pdf content*

---

## Prerequisite – Install PyMuPDF

```bash
pip3 install pymupdf
```

Verify:

```python
python3 -c "import fitz; print(fitz.__version__)"
```

---

## Conversion Script

Save this script anywhere (e.g. `~/scripts/pdf_to_md.py`) and call it from the terminal.

```python
#!/usr/bin/env python3
"""Convert PDF to Markdown using PyMuPDF.

Usage:
    python3 pdf_to_md.py <input.pdf> <output.md>
"""

import fitz  # PyMuPDF
import re
import sys
from pathlib import Path


def pdf_to_markdown(pdf_path: str, output_path: str) -> None:
    doc = fitz.open(pdf_path)
    md_lines = []
    page_count = 0

    for page_num, page in enumerate(doc, start=1):
        page_count = page_num
        blocks = page.get_text("dict")["blocks"]

        for block in blocks:
            if block["type"] != 0:          # skip image blocks
                continue

            for line in block["lines"]:
                line_text = ""
                max_size = 0
                is_bold = False

                for span in line["spans"]:
                    text = span["text"].strip()
                    size = span["size"]
                    flags = span["flags"]
                    bold = bool(flags & 2**4)   # bit 4 = bold
                    if text:
                        line_text += text + " "
                        if size > max_size:
                            max_size = size
                        if bold:
                            is_bold = True

                line_text = line_text.strip()
                if not line_text:
                    continue

                # --- Heading heuristics (tune thresholds to match your PDF) ---
                if max_size >= 20:
                    md_lines.append(f"\n# {line_text}\n")
                elif max_size >= 16:
                    md_lines.append(f"\n## {line_text}\n")
                elif max_size >= 13:
                    md_lines.append(f"\n### {line_text}\n")
                elif is_bold and max_size >= 11:
                    md_lines.append(f"\n**{line_text}**\n")
                else:
                    md_lines.append(line_text)

        md_lines.append("\n---\n")           # page separator

    # Remove excessive blank lines
    content = "\n".join(md_lines)
    content = re.sub(r'\n{4,}', '\n\n', content)

    with open(output_path, "w", encoding="utf-8") as f:
        f.write(content)

    print(f"Converted {page_count} pages -> {output_path}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python3 pdf_to_md.py <input.pdf> <output.md>")
        sys.exit(1)
    pdf_to_markdown(sys.argv[1], sys.argv[2])
```

---

## Heading Threshold Reference

| Font Size (pt) | Markdown Level |
|---|---|
| >= 20 | `# H1` |
| >= 16 | `## H2` |
| >= 13 | `### H3` |
| >= 11 + bold | `**bold paragraph**` |
| everything else | plain paragraph text |

> **Tip:** Inspect font sizes in a PDF before converting:
> ```bash
> python3 -c "import fitz; d=fitz.open('file.pdf'); [print(s['size'], s['text'][:60]) for b in d[0].get_text('dict')['blocks'] for l in b.get('lines',[]) for s in l['spans']]"
> ```
> Use this to tune the thresholds above to match your specific PDF's typography.

---

## Usage Examples

### Basic Conversion

```bash
python3 pdf_to_md.py "AI Agent Development Guide.pdf" "AI Agent Development Guide.md"
```

### In-place (same directory, same name)

```bash
PDF="/path/to/document.pdf"
MD="${PDF%.pdf}.md"
python3 pdf_to_md.py "$PDF" "$MD"
```

### Via Antigravity agent prompt

Just say:
> Convert `/path/to/document.pdf` to md file

The agent will:
1. Check for `pymupdf` -> install if missing (`pip3 install pymupdf`)
2. Run the conversion script inline (no need to save separately)
3. Save `document.md` next to `document.pdf`
4. Print a line count + first 80 lines as a sanity check

---

## Quality Checks

After conversion, always verify:

```bash
# Line count (healthy range: 200-2000 lines per 10-page PDF)
wc -l output.md

# Check heading distribution
grep -c "^#" output.md      # H1 count
grep -c "^##" output.md     # H2 count
grep -c "^###" output.md    # H3 count

# Preview first 50 lines
head -50 output.md
```

**Red flags to fix manually:**
- Every line is `# Heading` -> font sizes are uniform; lower the H1 threshold
- No headings at all -> raise thresholds or PDF has no structural fonts
- Garbled Unicode -> PDF uses a non-standard font encoding; try `pdftotext` fallback

---

## Fallback: `pdftotext` (plain text, no structure)

If PyMuPDF produces poor output (scanned PDFs, image-only PDFs):

```bash
# Install
brew install poppler        # macOS
# apt-get install poppler-utils   # Linux

# Convert
pdftotext -layout input.pdf output.txt

# Then manually structure output.txt in your editor
```

> **Note:** Scanned PDFs require OCR (e.g., `tesseract`). PyMuPDF cannot extract text from image-only PDFs.

---

## Limitations

| Limitation | Workaround |
|---|---|
| Image-only / scanned PDFs | Use `tesseract` OCR first |
| Multi-column layouts | Text order may be scrambled; post-process manually |
| Tables in PDFs | Extracted as plain rows; use `camelot-py` for table-aware extraction |
| Embedded images | Skipped entirely; extract separately if needed |
| Password-protected PDFs | Decrypt first with `qpdf --decrypt` |
