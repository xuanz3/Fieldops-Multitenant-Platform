#!/usr/bin/env python3

from pathlib import Path
import sys

REPOSITORY = Path(__file__).resolve().parents[1]
FINAL_DIRECTORY = REPOSITORY / "docs" / "evidence" / "final"

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

images = sorted(
    path
    for path in REPOSITORY.rglob("*")
    if path.is_file()
    and ".git" not in path.parts
    and path.suffix.lower() in IMAGE_EXTENSIONS
)

outside_final = []

for image in images:
    try:
        image.resolve().relative_to(FINAL_DIRECTORY.resolve())
    except ValueError:
        outside_final.append(image)

print(f"Repository image count: {len(images)}")
print(f"Images outside docs/evidence/final: {len(outside_final)}")

for image in images:
    print(f"  - {image.relative_to(REPOSITORY)}")

errors = []

if len(images) > 15:
    errors.append(
        f"The repository contains {len(images)} images; the maximum is 15."
    )

if outside_final:
    errors.append(
        "Images may exist only inside docs/evidence/final."
    )

if errors:
    for error in errors:
        print(f"ERROR: {error}", file=sys.stderr)
    raise SystemExit(1)

print("Image policy passed.")
