#!/usr/bin/env python3

from argparse import ArgumentParser
from pathlib import Path
import sys

parser = ArgumentParser()
parser.add_argument(
    "--expected-count",
    type=int,
)
arguments = parser.parse_args()

REPOSITORY = Path(__file__).resolve().parents[1]
FINAL_DIRECTORY = (
    REPOSITORY /
    "docs" /
    "evidence" /
    "final"
)

IMAGE_EXTENSIONS = {
    ".png",
    ".jpg",
    ".jpeg",
    ".webp",
    ".gif",
    ".bmp",
    ".tif",
    ".tiff",
    ".heic",
    ".avif",
    ".svg",
}

IGNORED_DIRECTORY_NAMES = {
    ".git",
    ".fieldops-runtime",
    ".cache",
    "node_modules",
    "bin",
    "obj",
    "dist",
    "coverage",
    "playwright-report",
    "test-results",
    "__pycache__",
}


def is_generated_or_dependency_file(
    path: Path,
) -> bool:
    relative_parts = (
        path
        .relative_to(REPOSITORY)
        .parts
    )

    return any(
        part in IGNORED_DIRECTORY_NAMES
        for part in relative_parts
    )


images = sorted(
    path
    for path in REPOSITORY.rglob("*")
    if path.is_file()
    and not is_generated_or_dependency_file(
        path
    )
    and path.suffix.lower()
        in IMAGE_EXTENSIONS
)

outside_final = []

for image in images:
    try:
        image.resolve().relative_to(
            FINAL_DIRECTORY.resolve()
        )
    except ValueError:
        outside_final.append(image)

print(
    f"Repository image count: {len(images)}"
)
print(
    "Images outside docs/evidence/final: "
    f"{len(outside_final)}"
)

for image in images:
    print(
        "  - "
        f"{image.relative_to(REPOSITORY)}"
    )

errors = []

if len(images) > 15:
    errors.append(
        "The repository contains "
        f"{len(images)} images; "
        "the maximum is 15."
    )

if outside_final:
    errors.append(
        "Images may exist only inside "
        "docs/evidence/final."
    )

if (
    arguments.expected_count
    is not None
    and len(images)
    != arguments.expected_count
):
    errors.append(
        "Expected exactly "
        f"{arguments.expected_count} images "
        f"but found {len(images)}."
    )

if errors:
    for error in errors:
        print(
            f"ERROR: {error}",
            file=sys.stderr,
        )
    raise SystemExit(1)

print("Image policy passed.")
