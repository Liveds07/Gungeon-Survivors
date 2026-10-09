# Gungeon Survivors

Explore Enter the Gungeon as a Vampire Survivors character, combining aimed
guns with evolving automatic weapons and optional survival branches.

## Status

Early source-only development. There is no playable mod or release yet.
The route planner is tested separately; it does not modify or run either game.
No game files, sprites, assemblies, or decompiled code belong in this repository.

## First playable target

- Enter the Gungeon is the host; Vampire Survivors is a required companion.
- Two survivors and their weapons, provisionally Antonio/Whip and
  Imelda/Magic Wand. Names, data identifiers, sprites, abilities and evolution
  data must be verified against the installed Vampire Survivors version.
- Preserve Gungeon exploration, aimed gunplay and dodge rolls.
- Offer optional survival branches with distinct modifiers and rewards.
  Branches rejoin the exploration route and never block the ordinary path.
- Use seeded variation so runs differ but failures can be reproduced.
- Solo is required. Local co-op is desired, contingent on real two-player tests;
  it is not implemented or advertised as supported.

## Development

Python 3.10+ is sufficient for the independent design tooling:

```sh
python tools/preflight.py --scope design
python -m unittest discover -s tests -v
python tools/preflight.py --scope release
```

Release preflight currently fails intentionally: the actual game integrations
have not been inspected or verified. Passing design checks is not a playable
build, an install test, or permission to upload.

JSON sheets are the source of truth. Change them before implementation.
Every cell has a verification status; release preflight lists unfinished cells
and broken cross-sheet references. No game API names are assumed.

## Required next access

Continue on an agent-accessible machine with the user's installed copies of
both games. This Linux cloud machine cannot see the user's PC or Steam library.
Inspect the actual engines, versions, managed assemblies and asset containers;
then implement a BepInEx 5 host plugin and read companion content at runtime
from the Vampire Survivors folder supplied by Melty. Do not ship game content.

Melty's catalog supports automatic BepInEx 5 v5.4.23.5 installation for Gungeon.
Do not infer compatibility with the user's uninspected game version from that.
No companion loader is required unless runtime inspection establishes a need.

## Publication

No listing, package, screenshot or release has been submitted. Before uploading:
verify all sheet cells, compile against actual host references, test in Gungeon,
test installation, capture real gameplay, run Melty package/recipe/one-click
checks, and establish the author's license, credits and remix permissions.
The final publication requires the user's explicit approval.
