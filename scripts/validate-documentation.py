#!/usr/bin/env python3

from pathlib import Path
from urllib.parse import unquote, urlsplit
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
        "*.md",
    ],
    cwd=repository,
    text=True,
    capture_output=True,
    check=True,
).stdout.splitlines()

link_pattern = re.compile(
    r"(!?)\[([^\]]*)\]\(([^)]+)\)"
)
heading_pattern = re.compile(
    r"^(#{1,6})\s+(.+?)\s*$"
)
task_pattern = re.compile(
    r"^\s*[-*+]\s+\[[ xX]\]\s+"
)
malformed_task_pattern = re.compile(
    r"^\s*[-*+]\s+\[[^\]]*\]"
)
empty_list_pattern = re.compile(
    r"^\s*[-*+]\s*$"
)
placeholder_pattern = re.compile(
    r"\b(TODO|TBD|FIXME)\b",
    re.IGNORECASE,
)

errors = []

for relative in tracked:
    path = repository / relative
    text = path.read_text(
        encoding="utf-8"
    )
    lines = text.splitlines()

    if not text.endswith("\n"):
        errors.append(
            f"{relative}: missing final newline"
        )

    if text.count("```") % 2:
        errors.append(
            f"{relative}: unbalanced fenced code block"
        )

    h1_count = 0
    previous_level = 0
    headings = set()

    for line_number, line in enumerate(
        lines,
        start=1,
    ):
        if line.rstrip() != line:
            errors.append(
                f"{relative}:{line_number}: trailing whitespace"
            )

        if "\t" in line:
            errors.append(
                f"{relative}:{line_number}: tab character"
            )

        if task_pattern.match(line):
            errors.append(
                f"{relative}:{line_number}: task-list checkboxes "
                "are not used in maintained documentation"
            )
        elif malformed_task_pattern.match(line):
            errors.append(
                f"{relative}:{line_number}: malformed task-list marker"
            )

        if empty_list_pattern.match(line):
            errors.append(
                f"{relative}:{line_number}: empty list item"
            )

        if placeholder_pattern.search(line):
            errors.append(
                f"{relative}:{line_number}: unresolved placeholder"
            )

        heading = heading_pattern.match(line)
        if heading:
            level = len(heading.group(1))
            title = heading.group(2).strip()

            if level == 1:
                h1_count += 1

            if (
                previous_level
                and level > previous_level + 1
            ):
                errors.append(
                    f"{relative}:{line_number}: heading level "
                    f"jumps from H{previous_level} to H{level}"
                )

            key = (
                level,
                title.casefold(),
            )
            if key in headings:
                errors.append(
                    f"{relative}:{line_number}: duplicate heading "
                    f"'{title}'"
                )
            headings.add(key)
            previous_level = level

    if h1_count != 1:
        errors.append(
            f"{relative}: expected one H1 heading, found {h1_count}"
        )

    for match in link_pattern.finditer(text):
        is_image = bool(match.group(1))
        label = match.group(2).strip()
        target = match.group(3).strip()

        if is_image and not label:
            errors.append(
                f"{relative}: image without alt text"
            )

        if (
            not target
            or target.startswith("#")
            or target.startswith("mailto:")
        ):
            continue

        parsed = urlsplit(target)

        if parsed.scheme in {
            "http",
            "https",
        }:
            continue

        clean_target = unquote(
            parsed.path
        )

        if not clean_target:
            continue

        resolved = (
            path.parent /
            clean_target
        ).resolve()

        try:
            resolved.relative_to(
                repository.resolve()
            )
        except ValueError:
            errors.append(
                f"{relative}: link escapes repository: {target}"
            )
            continue

        if not resolved.exists():
            errors.append(
                f"{relative}: broken relative link: {target}"
            )

if errors:
    for error in errors:
        print(
            f"ERROR: {error}",
            file=sys.stderr,
        )
    raise SystemExit(1)

print(
    f"Documentation validation passed for {len(tracked)} files."
)
