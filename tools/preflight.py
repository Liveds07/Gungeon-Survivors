"""Validate sheet shape, every cell, and references before any build."""
import argparse
import json
from pathlib import Path

SHEETS = Path(__file__).resolve().parents[1] / "design" / "sheets.json"
DESIGN_SHEETS = {"rewards", "challenges", "systems", "survival_rooms", "chest_rewards"}


def validate(document, scope="release"):
    errors = []
    sheets = document["sheets"]
    selected = DESIGN_SHEETS if scope == "design" else set(sheets)
    for name in sorted(selected):
        sheet = sheets[name]
        seen = set()
        for row in sheet["rows"]:
            row_id = row.get("id", "<missing>")
            if row_id in seen:
                errors.append(f"{name}.{row_id}: duplicate id")
            seen.add(row_id)
            for extra in set(row) - set(sheet["columns"]):
                errors.append(f"{name}.{row_id}.{extra}: undeclared column")
            for column in sheet["columns"]:
                value = row.get(column)
                if value is None or value == "":
                    errors.append(f"{name}.{row_id}.{column}: unfilled")
                if scope == "release":
                    # In-game verification is per cell, never inferred from a
                    # passing design test or a whole-sheet status.
                    key = f"{name}.{row_id}.{column}"
                    if document.get("verified_cells", {}).get(key) != "in_game":
                        errors.append(f"{key}: not verified in game")
    for ref in document["references"]:
        if ref["sheet"] not in selected:
            continue
        target_ids = {r["id"] for r in sheets[ref["target"]]["rows"]}
        for row in sheets[ref["sheet"]]["rows"]:
            if row.get(ref["column"]) not in target_ids:
                errors.append(f"{ref['sheet']}.{row['id']}.{ref['column']}: unresolved reference")
    return errors


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--scope", choices=["design", "release"], default="release")
    args = parser.parse_args()
    errors = validate(json.loads(SHEETS.read_text()), args.scope)
    for error in errors:
        print(error)
    print(f"{args.scope} preflight: {len(errors)} unfinished cells/findings")
    raise SystemExit(bool(errors))
