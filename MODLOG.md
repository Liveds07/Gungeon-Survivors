# Development journal

## 2026-10-09

User design: Enter the Gungeon host; actual Vampire Survivors characters,
signature abilities, automatic weapons and evolutions from the player's copy.
Exploration with optional survival branches. First version emphasizes distinct
random dungeon runs, two survivors, two weapons, multiple challenges/rewards.
Solo required; local co-op desired if feasible. English communication preferred.

Melty connection tested successfully. Account has no existing mods.
Search found no existing mashup combining these two games.
Read game_info for both and mashup_info for marble-blast-gungeon.
The example uses secondary for its other game; do not copy that: our required
Vampire Survivors content must use companion and receive its folder at launch.

Selected intended route: BepInEx 5 host plugin, Melty-installed v5.4.23.5,
runtime reads of companion content; exact APIs and formats await inspection.
Melty notes are generic Unity guidance, not verified Gungeon API documentation.

Toolkit cloned to /workspace/tools/universal-modder. Read mod-any-game skill
and Unity reference. CLI runs using UM_NO_UV=1. KB has no Gungeon field note.
um scan --list and filesystem discovery found no installed games or Steam
library. No accessible mount of the user's PC, Windows execution or desktop.
No game versions, assemblies, asset IDs or runtime hooks have been established.
No game files modified, loader installed into a game, save touched or media captured.

Independent work: JSON design sheets, per-cell release preflight, and a seeded
branch-selection model. This model does not generate Gungeon dungeon geometry.
Pending: inspect installed games, map sheet rows to actual runtime data and APIs,
generate per-row C# definitions, implement and test the real host vertical slice.
Never build a release with unfinished cells. Never substitute this model for
gameplay. License, remix permissions and final author credit are unset; do not
invent permissions or publish until these are established.

Validation: design preflight passed with zero findings. Six unittest tests
passed; route invariants and variation exercised over 1,000 seeds. Release
preflight correctly failed with 95 unfilled/unverified cell findings. No
playable-build, game-install, co-op or screenshot checks have run.
Toolkit checkout revision: 8370faa8e114baf33acdb23079aff552a7728c4b.
