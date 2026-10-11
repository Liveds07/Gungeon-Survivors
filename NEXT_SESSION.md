# Next session: Gungeon Survivors

Read README.md and MODLOG.md first, then LOCAL_DEVELOPMENT.md and JSON sheets.
English; no publication or redistribution of installed game files. Use
universal-modder and supported BepInEx 5.4.23.5. Melty recipe wiring is pending.
Solo only; endings remain deferred.

Current local build: 0.2.18. Launcher:
C:\Users\Propietario\modding-lab\GungeonSurvivors\EtG.exe
Project: C:\Users\Propietario\Gungeon-Survivors-main

Antonio/Whip, Imelda/Magic Wand, Poe/Garlic, Pasqualina/Runetracer; nine accessories, four evolutions,
six slots each for weapons/accessories. Pummarola works only during combat.
Wand/Holy damage remains quartered; eligible regular enemies double; boss XP
is 10 times the native floor. Mod UI hides for native dialogue/cinematics.

Optional survival arenas on each main floor have a corridor and wall-mounted
interactive wooden sign beside the door. Rules use native wood-panel dialogue; only the
countdown/enemy count appears during combat. 300 gameplay seconds, caps 12/14/16/18/20 by main floor,
replacement ten seconds after each death. Native splits/summons spawn immediately and are exempt from the scheduled cap.
They are tracked separately for cleanup and never affect scheduled replacement slots. 32x26 arena, four wall sections,
two column islands, four native flippable tables. Challenge enemy loot is
suppressed at AIActor.HandleLootPinata, preserving separate credited-kill XP.
CanDropCurrency alone was insufficient: native bonus coins bypass it.
Completion clears enemies/projectiles, unlocks the exit and grants one unlocked
A chest, with S probabilities 0/25/50/75/100% by main floor.
One full heart on each side of the chest. No key, mimic or fuse.

Chests grant random owned weapon/accessory ranks: one for D/C/B, three for A, five for S.
An evolution eligible at opening is selectable and consumes one slot; remaining
slots grant random ranks. Recheck rank eligibility per slot; stop at max rank.
Character XP/level do not change. Broken unopened chests retain exactly 1 XP
and no native loot. Later native Gungeon item pickups remain usable, as before.

0.2.15 checks: native caps on all five floors, four full hearts per completion,
wall sign native interaction, Blobuloid native offspring, accessory chest ranks,
S chest Bloody Tear plus four random ranks, and varied fresh first offers with
the same native seed. Completion sweep was accelerated; full new balance
playthrough remains pending. Choice/chest RNG is persistent per run, freshly
seeded from Guid.NewGuid; no forced item exclusions.

Historical 0.2.14 tests: a real full 300-second challenge completed; native pause froze its clock.
All five main floors generated arenas and correct unlocked chests. The floor
sweep used accelerated completion after the full-duration test. All rarities
opened with zero keys. A gave three ranks; S gave five and stopped at rank cap.
An A evolution offered Holy Wand, then gave two Whip ranks. Native table
interaction and doorway walking passed. Forced 99-coin/bonus-coin enemy flags
still gave zero currency while actual wand kills awarded XP. See MODLOG.md.

Startup crash: one native Unity/Mono crash before content readiness showed
dfAtlas Dictionary ArrayTypeMismatchException. Old BalanceTestDriver is retired.
Multiple clean/replacement-helper launches and the five-floor sweep passed.
A later old-helper comparison initialized and did not reproduce the crash.
Exact cause remains unconfirmed; do not claim a definitive diagnosis.
Archive output_log.txt before relaunch if it recurs.

Saves restored and byte-verified from before-exempt-summons/20261010-215450.zip after
tests; all diagnostic DLLs and pending commands removed. Never restore this
snapshot over later player progress. Back up again before automated testing.
User permits autonomous tests while not controlling; announce any handoff.
Use native Play/Marine selection and the survivor picker before entering from
the Breach. Direct main-menu-to-floor loading leaves enemy AI untargeted.

Generation retries need the branch through GetRandomFlow. Exclude exactly
Foyer Flow, not names containing foyer (BossFoyer is a main-floor flow).
Filter hall exits before native opposite-direction preference discards bent
pairs. Opening uses two contained cells (native exitLength 3). Warp fixtures
place the body off-center; align before testing straight traversal. Preserve
original flow nodes and parent exit demand; never mutate native assets.

Beyond this local slice: full new balance playthrough, side-floor coverage,
broader controller/VFX checks, saved mid-run persistence, Melty recipe/install
tests. Co-op is unimplemented. Release preflight still reports unfinished cells.
Nothing published. Native dumps/assets/references and fixtures stay outside source.

Latest correction installed: one full heart each side (two total); native summoned/split offspring active immediately and exempt from scheduled cap. Native Blobuloid -> two Blobulins with scheduled12 verified in private final session. Saves restored; game left closed, ready for next launch. See latest MODLOG evidence.

The readable screen-space interaction prompt and short native wood-panel rules are installed and visually verified in0.2.16. Earlier0.2.15 evidence above is historical; use the latest backup/hash below for this completed session, and create a fresh backup before any later test.

2026-10-11 completed update 0.2.16 (historical; superseded below):
- Imelda tested in game: base maximum3, Hollow Heart rank5 maximum6; currentHP1.5 preserved at every rank. Antonio installed data120HP/armor1 versus Imelda100HP/armor0; host Antonio3.5 hearts and one native armor shield.
- Clover added from installed WEAPON_DATA/LUCK and Sprite/Clover: five ranks, each grants +1 native Coolness via one ownerless additive stat modifier. Native tests0->1->2->3->4->5 and removal back0; normal choice/controller acquisition and S chest rank upgrades passed. Six accessory slots retained; chest XP/character level unchanged.
- Custom stick filtering removed from both menus; native directional WasPressed used. Virtual known Xbox-style pad moved focus once per press and confirmed picker/choice. Rumble queue/motors zero in picker, choice and quick-restart picker. Physical 8BitDo check remains for the player.
- Death-book virtual button test deliberately started with LastInputType keyboard and no stick movement; native QuickRestart triggered. New run removes inherited passives/actives and reopens picker.
- Closing menus clears only this plugin's BraveTime multiplier even if its ownership flags were already lost; native input overrides are still cleared by their own keys. Exact intermittent walking-stall cause remains unconfirmed; no claim that the former menu stick filter caused global movement issues.
- Interactive sign shows the exact requested short sentence; screenshot inspected. North challenge/east-west ordinary connections verified across all five main floors and a second first-floor layout. Mines fixture needed several native generation retries: investigate if players report long loading.
- HUD labels scheduled population Enemies; native summoned/split enemies still exempt from the cap. No new full five-minute endurance/balance run claimed.
- At this point Pasqualina/Runetracer and Armor were next content tasks. User clarification resolves shield type to native Gungeon armor.

Current pending work: Pasqualina/Runetracer and Armor accessory; further physical 8BitDo movement/death-UI playtest; investigate Mines generation retries if reproduced in normal runs. Armor means native Gungeon armor. Endings, co-op and publication remain deferred.
Final installed SHA256: 95a6ffc720bf293b3c7a05ad239684cc304ff6aeee783634534adab225ddc20c. Save baseline before-menu-fixes/20261011-020117.zip restored and byte-verified; only production GungeonSurvivors.dll remains in lab plugins.

Clean game left running in native Breach for the player, not in a diagnostic fixture. No automation pending. Do not interfere with player control; wait for them to close the game before installing further updates.

2026-10-11 completed update 0.2.17 ï¿½ CURRENT HANDOFF:
Pasqualina, Runetracer, Armor and NO FUTURE are implemented and installed.
Installed PASQUALINA.speed1.1; +0.1 at5/10/15, cap1.4 verified through20.
Native health3/armor0. Sprite/walking frames read from the installed addressable.
DIAMOND ranks use installed power/speed/amount/duration/hitBoxDelay; finite
native bouncing projectiles pass through actors using swept hitbox checks.
Native TrailRenderer draws a colored fading tail. Rank1 damage1/lifetime2.25s;
rank8 damage3/lifetime3.25s/amount3 before global stat bonuses. ROCHER uses
installed evolved stats and duration-dependent interval; its native bouncing
shots explode with actual Screeno_0..29 frames, and received HP/armor hits cause
retaliation. Armor grants one native shield once per new rank (five ranks),
never heals or changes maxHP, and retains evolution eligibility after spending.
Both six-slot caps remain. Effects/modifiers/callbacks are removed on pool reuse,
room changes, character swaps and detach. Pause ignores bounce damage; noisy
projectile diagnostics are limited per run.
Native evidence: pasqualina-combat-native.log, pasqualina-final-native.log,
pasqualina-runetracer.png and pasqualina-nofuture.png outside source in modding-tools.
Native speed thresholds/cap, actual shield hits/noRefill, credited rune kills/XP,
A chest evolution/controller confirmation, bounce/armor-hit explosions and floor
transition passed. Final native controlled flight: two actors each lost3HP,
500ms hit delay, no damage during menu pause, finite expiry, zero active flights.
Build+ten Python checks+design preflight+RuneGeometryRegression passed.
No full balance playthrough or new physical8BitDo test claimed.
Fresh latest-player snapshot before-pasqualina/20261011-030216.zip restored and
byte-verified. NEVER restore an older snapshot over future player progress.
Installed/source SHA256 cf605bcf434044c5c60e14c95213dcab65155e00cd0a097f6760ede09feed681.
Only production GungeonSurvivors.dll remains in lab plugins. Game closed;
no automation, invulnerable fixture or pending command is handed to the player.
Next: player Pasqualina balance feedback, broader controller/saved-run tests,
Mines generation latency if reported, supported Melty recipe/install validation.
Endings/co-op/publication remain deferred. No game files belong in source.

2026-10-11 completed0.2.18 — CURRENT HANDOFF:
User requested return of the former menu sensitivity adjustment; restored
menu-local0.7 engage/0.25 release/0.25s cooldown/neutral-before-next tilt in
choice and character picker. Global walking/aiming bindings are untouched.
Native choice fixture:0.5 tilt ignored, full tilt moved0->1 once, held did not
repeat, neutral+fresh moved1->2, confirm selected Crown and restored native
AllInput/timeScale1.17 menu regression checks passed. New picker fixture opened
but closed before navigation; physical controller/picker retest remains pending.
Runetracer spawn streak fixed: initialize sweep at actual spawn transform,
create inactive trail, set post-physics anchor and Clear before enabling it.
Actual12 alternating distant-room projectile respawns:2927 sampled trail points,
maxDistance7.630633; no distant cached points/long streak.10 Python checks and
design preflight passed; build uses installed native references as before.
Latest player baseline before-trail-menu/20261011-033340.zip restored and
byte-verified. Use this only for this session; BACK UP AGAIN after later progress.
Only production plugin installed; game closed; no automation pending.
Source/installed SHA256 b4e1c3ed45600536a09482d4835f021edc0b4f4618877e9617d7d20c6b7c3be8.
Evidence: private modding-tools/trail-menu-final-native.log and unity.log.
Nothing published or redistributed. Retain0.2.17 content/balance validation tasks.
