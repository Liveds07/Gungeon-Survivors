# Gungeon Survivors

Explore Enter the Gungeon as a Vampire Survivors character, combining aimed
guns with evolving automatic weapons and optional survival branches.

## Status

An early solo BepInEx 5 prototype runs in a private copy of the installed Gungeon. Companion sprites and weapon data come from the installed Vampire Survivors copy. Both automatic weapons have killed real enemies and awarded XP; the whip check used controlled positioning. The full first-playable target remains unfinished; there is no release.
See LOCAL_DEVELOPMENT.md for setup and MODLOG.md for evidence and limitations.
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
have not all been verified in game. Passing design checks is not a playable
build, an install test, or permission to upload.

JSON sheets are the source of truth. Change them before implementation.
Every cell has a verification status; release preflight lists unfinished cells
and broken cross-sheet references. No game API names are assumed.

## Local game integration

Both installed games have been inspected on the Windows development machine:
Gungeon 2.1.9a uses Unity 2017.4.27f1 / Mono; Vampire Survivors Steam build
25016043 uses Unity 6000.0.62f1 / IL2CPP. See MODLOG.md for the evidence.

Melty's local catalog supports BepInEx 5 v5.4.23.5 for Gungeon. That loader has
run the prototype locally. Melty companion-folder wiring and installation
recipe tests are still pending. No companion loader is currently required.

## Publication

No listing, package, screenshot or release has been submitted. Before uploading:
verify all sheet cells, compile against actual host references, test in Gungeon,
test installation, capture real gameplay, run Melty package/recipe/one-click
checks, and establish the author's license, credits and remix permissions.
The final publication requires the user's explicit approval.



