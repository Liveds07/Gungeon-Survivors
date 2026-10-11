# Gungeon Survivors

Explore Enter the Gungeon as a Vampire Survivors character, combining aimed
guns with evolving automatic weapons and optional survival branches.

## Windows prototype download

[Download the self-contained Windows x64 package](https://github.com/Liveds07/Gungeon-Survivors/raw/refs/heads/main/distributions/Gungeon-Survivors-0.2.18-Windows-x64.zip).
Extract the entire ZIP and double-click `Launch.cmd`. It includes the supplied
0.2.18 DLL, companion reader, design sheets, BepInEx, and a private Python
runtime with sprite/data dependencies. No separate modding-tools installation
is required. It finds Steam/Epic game folders or asks you to select them,
creates a private Gungeon copy, backs up saves, configures paths and launches.
Both installed games are required; no game content is bundled.

The Windows launcher and bundled Windows runtime have not yet been executed
in the cloud. Treat this as an installation candidate awaiting a real PC test,
not a verified portable or Melty release. See the ZIP's `START-HERE.txt`.
Packaging source is under `packaging/` and `tools/package_runtime.py`.

## Status

Version 0.2.18 is a local solo BepInEx 5 playable prototype in a private copy of the installed Gungeon. Antonio, Imelda, Poe and Pasqualina use content read from the installed Vampire Survivors copy. Whip, Magic Wand, Garlic and Runetracer have damaged real enemies; their evolutions and nine accessories are implemented. Optional five-minute challenge arenas have an interactive entrance sign, native cover, 12/14/16/18/20 enemies by main floor and ten-second replacements. Completion grants an unlocked A chest, with S chances of 0/25/50/75/100% by main floor, plus one full heart on each side. Native splits and summons spawn immediately and are exempt from the scheduled-enemy cap. Chests grant random owned weapon or accessory ranks (one for D/C/B, three for A, five for S), with eligible evolutions selectable, without changing character XP or level. Wand balance, regular-room enemy doubling, floor-scaled boss XP and cinematic HUD suppression remain in place. There is no published release; broader content, co-op and installation recipe verification remain unfinished.
Hollow Heart increases capacity without changing current HP; Imelda reaches six hearts at rank five. Antonio starts with one native armor shield. Clover grants +1 native Coolness per rank, up to +5. Menu stick filtering uses a deliberate tilt and return to center; menu rumble is suppressed and controller death-menu confirmation no longer requires stick movement.
Pasqualina starts with Runetracer and +10% projectile speed, increasing to +20/+30/+40% at levels 5/10/15. Runetracer passes through enemies and bounces off native walls with a fading trail. Armor grants one native shield per new rank, up to five, without refilling spent shields. Rank-eight Runetracer plus owned Armor unlocks NO FUTURE, which explodes on bounces and received hits.
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
- Keep dungeon variation seeded; upgrade offers use a fresh random sequence each run.
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





