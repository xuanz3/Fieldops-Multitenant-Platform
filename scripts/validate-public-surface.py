#!/usr/bin/env python3

from pathlib import Path
import re
import subprocess
import sys

repository = (
    Path(__file__)
    .resolve()
    .parents[1]
)

tracked = subprocess.run(
    [
        "git",
        "ls-files",
    ],
    cwd=repository,
    text=True,
    capture_output=True,
    check=True,
).stdout.splitlines()

text_extensions = {
    ".md",
    ".yml",
    ".yaml",
    ".json",
    ".cs",
    ".ts",
    ".tsx",
    ".css",
    ".sh",
    ".command",
    ".py",
    ".mjs",
    ".conf",
    ".http",
    ".xml",
}

ignored_names = {
    "package-lock.json",
    "FieldOps.sln",
}

ignored_relatives = {
    "scripts/validate-public-surface.py",
}

patterns = {
    "ChatGPT":
        re.compile(
            r"\bchatgpt\b",
            re.IGNORECASE,
        ),
    "OpenAI":
        re.compile(
            r"\bopenai\b",
            re.IGNORECASE,
        ),
    "Copilot":
        re.compile(
            r"\bcopilot\b",
            re.IGNORECASE,
        ),
    "AI-generated":
        re.compile(
            r"\bai[- ]generated\b",
            re.IGNORECASE,
        ),
    "portfolio":
        re.compile(
            r"\bportfolio\b",
            re.IGNORECASE,
        ),
    "resume":
        re.compile(
            r"\bresume\b",
            re.IGNORECASE,
        ),
    "interview":
        re.compile(
            r"\binterview\b",
            re.IGNORECASE,
        ),
    "numbered phase":
        re.compile(
            r"\bphase[- _]?\d+\b",
            re.IGNORECASE,
        ),
    "checkpoint":
        re.compile(
            r"\bcheckpoint\b",
            re.IGNORECASE,
        ),
    "presentation generator":
        re.compile(
            r"capture-final-evidence|"
            r"generate\s+portfolio|"
            r"screenshot[- ]generation|"
            r"screen[- ]generation",
            re.IGNORECASE,
        ),
}

errors = []

for relative in tracked:
    if relative in ignored_relatives:
        continue

    path = repository / relative
    lower_name = relative.casefold()

    if (
        "portfolio" in lower_name
        or "checkpoint" in lower_name
        or re.search(
            r"phase[- _]?\d+",
            lower_name,
        )
        or "capture-final-evidence"
        in lower_name
    ):
        errors.append(
            f"process-oriented path: {relative}"
        )

    if (
        path.name in ignored_names
        or path.suffix.lower()
        not in text_extensions
    ):
        continue

    try:
        text = path.read_text(
            encoding="utf-8"
        )
    except UnicodeDecodeError:
        continue

    for label, pattern in patterns.items():
        for match in pattern.finditer(text):
            line_number = (
                text.count(
                    "\n",
                    0,
                    match.start(),
                )
                + 1
            )
            errors.append(
                f"{relative}:{line_number}: "
                f"{label}: {match.group(0)}"
            )

if errors:
    for error in errors:
        print(
            f"ERROR: {error}",
            file=sys.stderr,
        )
    raise SystemExit(1)

print("Public-surface validation passed.")
