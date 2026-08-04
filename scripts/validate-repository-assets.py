#!/usr/bin/env python3

from pathlib import Path
import sys

repository = (
    Path(__file__)
    .resolve()
    .parents[1]
)

screen_directory = (
    repository /
    "docs" /
    "screens"
)

expected_files = {
    "dashboard.png",
    "work-orders.png",
    "create-work-order.png",
    "work-order-details.png",
    "dispatch-board.png",
    "technician-workspace.png",
    "completion-files.png",
    "client-review.png",
    "customers.png",
    "role-access.png",
    "audit-log.png",
    "operations-report.png",
}

ignored_directories = {
    ".git",
    ".fieldops-runtime",
    ".cache",
    "node_modules",
    "bin",
    "obj",
    "dist",
    "coverage",
    "test-results",
    "__pycache__",
}

image_extensions = {
    ".png",
    ".jpg",
    ".jpeg",
    ".webp",
    ".gif",
    ".svg",
}


def is_generated(path: Path) -> bool:
    return any(
        part in ignored_directories
        for part in (
            path
            .relative_to(repository)
            .parts
        )
    )


images = sorted(
    path
    for path in repository.rglob("*")
    if path.is_file()
    and not is_generated(path)
    and path.suffix.lower()
        in image_extensions
)

errors = []

for image in images:
    try:
        image.resolve().relative_to(
            screen_directory.resolve()
        )
    except ValueError:
        errors.append(
            "Image outside docs/screens: "
            f"{image.relative_to(repository)}"
        )

actual_files = {
    path.name
    for path in screen_directory.glob("*.png")
}

missing = sorted(
    expected_files -
    actual_files
)
unexpected = sorted(
    actual_files -
    expected_files
)

if missing:
    errors.append(
        "Missing application screens: "
        + ", ".join(missing)
    )

if unexpected:
    errors.append(
        "Unexpected application screens: "
        + ", ".join(unexpected)
    )

if len(images) != len(expected_files):
    errors.append(
        "Expected exactly "
        f"{len(expected_files)} repository images, "
        f"found {len(images)}."
    )

print(
    f"Application screen count: {len(images)}"
)

if errors:
    for error in errors:
        print(
            f"ERROR: {error}",
            file=sys.stderr,
        )
    raise SystemExit(1)

print("Repository asset validation passed.")
