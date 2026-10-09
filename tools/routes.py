"""Host-independent branch selection; not a game or a host integration."""
import json
import random
from pathlib import Path


def plan(seed, document=None):
    if document is None:
        document = json.loads((Path(__file__).resolve().parents[1] / "design/sheets.json").read_text())
    sheets = document["sheets"]
    config = next(row for row in sheets["systems"]["rows"] if row["id"] == "branch_planner")
    challenges = sheets["challenges"]["rows"]
    count = config["choices_per_stage"]
    if not 1 <= count <= len(challenges):
        raise ValueError("Challenge choice count exceeds available unique challenges")
    if not config["preserve_normal_route"]:
        raise ValueError("The normal exploration route must remain available")
    rng = random.Random(seed)
    stages = []
    for stage in range(config["stages"]):
        options = rng.sample(challenges, count)
        stages.append({
            "stage": stage,
            "normal_route": True,
            "challenges": [dict(row) for row in options],
            "rejoin_stage": stage + 1,
        })
    return stages
