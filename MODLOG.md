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
## Local Windows inspection and first slice (2026-10-09)

Command access now works. Read README.md and MODLOG.md before implementation.
Project is C:\Users\Propietario\Gungeon-Survivors-main (downloaded source directory,
no .git directory). Installed host is D:\Epic Games\EnterTheGungeon: Epic catalog
2.1.9a, x64, Unity 2017.4.27f1, Mono. Actual managed API metadata and selected IL
inspected using Unity's installed Cecil; no decompiled source stored in project.
Companion is D:\Steam\steamapps\common\Vampire Survivors: Steam build 25016043,
Unity 6000.0.62f1, IL2CPP metadata version 31. Native metadata confirms projectile
types including MagicMissileProjectile and WhipProjectile; native method bodies
have not been decompiled. No companion loader installed.

Universal-modder cloned outside project to C:\Users\Propietario\modding-tools\universal-modder.
Read its mod-any-game skill, Unity reference and safety reference. um scans found
no anti-cheat or preinstalled loader in either game; KB has no Gungeon note.
Isolated Python 3.12.10 is in modding-tools\python312 with UnityPy 1.25.4.
An attempted uv download was blocked by ESET; did not disable or bypass ESET.
Python obtained from python.org instead. BepInEx 5.4.23.5 downloaded from its
official GitHub release. Installed Melty app's catalog inspected read-only:
Gungeon lists bepinex-5 and that loader downloads the official x64 5.x release.
No Melty MCP tools are exposed in this session; recipe integration remains pending.
No Melty account credentials read or publication attempted.

Backups, all made with um backup before launching the test:
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-etg-saves\20261009-224432.zip
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-vs-profile\20261009-224432.zip
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-vs-steam-107358158\20261009-224432.zip
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-vs-steam-134462008\20261009-224433.zip
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-vs-legacy\20261009-224433.zip
Restore with the isolated Python: -m um backup restore <name> --snapshot <zip> --yes.
Restore overwrites existing save files; do so only with the game closed and user
authorization. The lab game shares the normal LocalLow saves, so backups matter.

Selected route: BepInEx 5 host plugin plus a separate asynchronous reader of the
installed companion. Unity 6 bundles cannot be loaded by Unity 2017 directly.
read_companion.py reads actual resources.assets TextAssets CHARACTER_DATA and
WEAPON_DATA and selected Sprite objects with UnityPy. Imelda sprite comes from
vs_local_assets_character_imelda.bundle. Runtime cache, including extracted PNGs
and selected data, is private in AppData\Local\GungeonSurvivors\content, outside
source and both game folders. Cache is regenerated on plugin startup, never shipped.
Actual selectors: ANTONIO/WHIP/VAMPIRICA; IMELDA/MAGIC_MISSILE/HOLY_MISSILE.
Evolution passives are read from those records. JSON sheets updated before plugin.
Sheet-defined host damage/XP conversion is provisional balancing, not VS values.

Plugin.cs compiles against installed host references using tools\build.ps1 and
Windows csc with /noconfig /nostdlib+. Game assemblies are referenced, never copied
into build. Local DLL is ignored by .gitignore. Testing uses a private game copy
at C:\Users\Propietario\modding-lab\GungeonSurvivors, with BepInEx and plugin.
Original installed games untouched. The private lab must never be packaged.

Initial runtime issue: Unity JsonUtility silently returned missing DTO fields.
After diagnostic checks established that problem, changed parser approach to
FullSerializer already present in inspected host assembly. BepInEx log then
confirmed GS_CONTENT_READY with two survivors, two weapons and six sprites.
F6 selection to Imelda confirmed via log and reviewed screenshot in an actual
Gungeon dungeon. Rendering and combat checks continue; not yet a completed slice.
Survival branches and evolutions remain unimplemented. Co-op disabled in prototype.

Checks so far: existing six design tests pass; design preflight passes; um publish
check (local scan only, no upload) found no game files/assets in source. No full
release verification, install recipe check, gameplay recording or publishing.
Additional runtime diagnostics: host gunfire and dodge events were observed.
A test-only driver DLL lives solely in the private lab (source in modding-tools),
not in the mod project. It reports player input state and can warp a test player
into an existing enemy room; it does not generate substitute enemies or kills.
Diagnostics established that the ordinary entrance player remained NoInput.
Disabling the host renderer prevented the entrance animation from completing.
Changed replacement skin to keep the host renderer enabled with transparent
sprite color, and hide its separate outline renderers. Original color and
outlines are restored on detach. Native input-state transition is being retested.
A first attempt at the F8 debug restart ran before a host player/session was
ready and failed in native floor generation. Guarded it against menu/foyer/loading
and absent player; no compatibility claim derives from that failed attempt.
Current source has provisional sheet-defined host scales and reads burst timing
from installed WEAPON_DATA. Nine tests pass, including three cache safety tests.
Release preflight still fails with 88 unfinished/unverified findings. No cells
are being marked verified merely because a selector was found or a build compiles.

### Real combat checks and session cleanup (2026-10-09)

Keeping the native renderer enabled with transparent color resolved the stalled
entrance animation. The test probe observed NoInput -> AllInput. Actual movement
changed player center from (86.3,80.6) to (86.3,85.9). F6 selected Imelda.

In an existing seven-enemy Castle_Green_Dragon room, the automatic wand fired
native projectiles at sheet-defined damage 4. No aimed firing was sent during
that room check. A reviewed private gameplay screenshot showed two kills and
2.0 XP. Native dodge event also logged. The player later died; no level-up or
room-clear success is claimed from that run.

Antonio's automatic whip damaged and killed a real enemy: runtime logged four
attacks with targets=1 and GS_KILL count=1 xp=1. This was a controlled range check:
the private test driver kept the player one unit beside an existing enemy for
seven seconds. The driver did not alter enemy health, invoke kills, add XP or
make the player invulnerable. A separate mixed combat check logged native aimed
fire and showed kill XP, but does not establish whip-only kills. Early range
checks missed because the helper confused the transform origin with body center;
the corrected helper accounts for that offset. Natural whip combat balance and
level-up progression still need testing.

The test process was closed by exact PID. Removed the test driver from the lab's
plugin folder (kept its authored source and DLL under modding-tools), and disabled
EnableTestCommands in the local config. Main plugin remains in the private lab.
Source, runtime cache, loader tooling and lab remain separate. Neither installed
game was patched; no package, screenshot, content cache or listing was published.

The lab shares normal Gungeon saves, so its test runs did change that save folder.
Preserved post-test state and logs in backup gungeon-survivors-etg-post-test /
20261009-232024.zip, then restored the original pre-test Gungeon snapshot using
its full path. Restore also made its own pre-restore backup. No clean deletion
was requested. Vampire Survivors was never launched and its backups remain.

Current evidence supports a narrow solo combat prototype with companion skins,
two automatic attacks, kill XP, native movement, aimed guns and dodge rolls.
It does not establish the full target: level-up behavior, survival branches,
evolutions, animated skins, compatibility with arbitrary gun prefabs, co-op,
Melty recipe wiring and ordinary installation remain unfinished. The current
single-floor attach resets progression on floor transitions; cross-floor run
persistence remains unfinished. Full release preflight must continue to fail.


### Player-feedback iteration: 0.2.0 / 0.2.1

User requested walking frames, actual attack art, longer whip range, upgrade
choices, removal of inherited starter equipment, hidden gun graphics with aimed
shooting, controller menu navigation and staggered additional whips. Updated
JSON design rows before implementation. No companion content added to source.

Inspected installed CHARACTER_DATA.walkingFrames=4 and actual character bundles
for newAntonio_01..04 and newImelda_01..04. Read four frames per survivor into the
private cache. Installed slash (147x22) visually matches the horizontal whip;
ProjectileBlue1/2 (28x23 / 32x22) match the blue missile. These sprite names are
observed assets; no IL2CPP native projectile bodies were decompiled to prove
prefab binding. UI frameB_blue (24x24) is read privately and drawn in nine parts.
No full original VS menu layout is claimed. Cache now contains 18 loaded sprites.

Used inspected host APIs ToggleGunRenderers / ToggleHandRenderers with a named
renderer override, restored on detach. Neutral starter is selected from the host
PickupObjectDatabase.Objects by sheet match `rusty`; runtime found Rusty_Sidearm.
Initial entrance loadout logged guns=1 passives=0 actives=0. Found equipment is
kept. Native non-item character traits are not normalized yet. Wand physics use
the same neutral projectile template regardless of the currently held gun;
private blue art overlays it. Native projectile colors restore before pool reuse.

Owned weapons have separate ranks and timers. Kill XP queues choices rather than
automatically increasing weapon rank. Choice UI uses the inspected BraveTime
SetTimeScaleMultiplier / ClearMultiplier and PlayerController input overrides.
A test-only injected pending choice verified menu rendering and new-weapon choice,
not kill-XP progression: selecting Wand produced owned=2, timeScale=0 -> 1 and
NoInput -> AllInput. The player's subsequent normal playtest logged a genuine
GS_LEVEL_UP level=2, GS_CHOICE_OPEN and Whip rank=2, followed by more kills. Thus
natural XP progression was also observed. A reviewed live screenshot showed a
different Antonio walking frame, hidden gun and companion blue projectile.

0.2.1 adds navigation using inspected GungeonActions SelectUp/Down, Up/Down and
MenuSelectAction with a highlighted row and unscaled input grace time. Keyboard
arrows/Enter remain supported along with numbers and clicking. Physical gamepad
verification remains pending. UI support is not a claim that F6 survivor selection
already works from controller. Additional whip attacks now run as a burst with
installed WEAPON_DATA.repeatInterval, rather than a simultaneous two-side hit.
Character rows and independent weapon stats stay sourced from the installed copy.

User's original playtest was not interrupted. Waited for them to close the game
before installing each new build. Backups before feedback tests:
gungeon-survivors-before-feedback-v2 / 20261009-233227.zip and
before controller/whip tests: gungeon-survivors-before-controller-whip /
20261009-234445.zip. Normal user saves are preserved separately from test artifacts.

### Level-up controller regression fix: 0.2.3

User physical-controller test reported inability to confirm a level-up choice
and overly sensitive stick navigation. Scope is the level-up menu only; movement
and aiming bindings/sensitivity are unchanged. Re-inspected actual host IL:
BraveInput.MenuInteractPressed checks InteractAction.WasPressed OR
MenuSelectAction.WasPressed. The old implementation checked only the latter.
Use the native combined check, plus the host's localized confirm control from
the active device when action bindings do not report it. No hardcoded Xbox/PS
button index and no host binding mutations.

JSON UI rows now specify stick engagement 0.7, release 0.25 and navigation
cooldown 0.25 s. MenuNavigation is a small engine-independent latch: one change
per intentional tilt, requiring center release, with no held-stick repeat.
A stick already held when the menu opens must be released first. Keyboard arrows,
Enter, 1/2 and mouse choices remain available. Plugin compiles against the installed
host references. Seventeen regression checks pass for opening-menu holds, small
tilts, holding, noise, reversal, cooldown and fresh tilts. All nine Python tests
and design preflight also pass. This is not physical-controller confirmation;
the replacement DLL awaits a clean handoff and restart for the player's retest.

Also recorded 0.2.2 lobby fix: survivor skin/HUD only activate outside IsFoyer;
native Breach presentation avoids the reported Marine/Antonio overlap. A private
reviewed lobby screenshot showed one native Marine, and probe showed foyer=True,
skin=none. Additional whip attack spacing is implemented using the installed
100 ms repeat interval; no clean in-game timing measurement is yet recorded.

An input-control handoff was ambiguous during the 0.2.2 test and agent/user input
overlapped. Agent stopped input and handed control to the player. No further
keyboard/mouse automation is permitted until a new explicit handoff. Reading
logs/screenshots and source work continue independently.

### Wand art and control handoff: 0.2.4 installed

User reported grey-looking wand orbs facing the wrong direction and requested
blue four-point-star art. Privately inspected a labeled contact sheet from their
installed resources.assets. ProjectileHoly1/2 (27x14 / 27x15) match that description;
ProjectileBlue1/2 were the old grey-looking orbs. Updated JSON effect selectors
before implementing the change. Visual scale increased to 0.75 for the smaller
star. Inspected host Projectile.Direction getter and align the overlay's world
rotation with its live direction each LateUpdate. A symmetric star still has
an asymmetric trail, so rotation is relevant. Native prefab mapping remains
unproven; this records a visual match to user feedback, not an IL2CPP code claim.

Compiled 0.2.4 with the level-up controller fixes from 0.2.3 and generated the
private content cache from the installed copy. Copied only authored plugin DLL
to the private lab. Removed the test driver from the lab's plugin folder, keeping
its authored code/DLL in modding-tools. Latest user saves were backed up as
gungeon-survivors-before-controller-star / 20261009-235804.zip. No save restore
was performed during this installation; current player progress was retained.
No game install was patched and no source game assets were added or published.

The user will perform the controller and visual retest. They explicitly asked
the agent to launch the updated lab, which was done; keyboard/mouse/controller
inputs remain exclusively with the user until they explicitly hand control back.
In-game confirmation of controller fix, star facing and whip spacing is pending.

### Whip visual/collision alignment: 0.2.5 (2026-10-10)

Player confirmed that the blue four-point-star wand art looks correct. This is
player verification of the chosen visual, not a claim of native prefab mapping.
They reported that whip hits did not cover its art, especially its upper area.
The old damage code tested enemy center points only and tested at strike start;
body overlap and entering the visible slash later could be missed.

Updated JSON collision policy before implementation. Whip damage and rendered
slash now use the same rectangle with sprite pivot compensation. Inspected host
PixelCollider.UnitBottomLeft / UnitTopRight and SpeculativeRigidbody.HitboxPixelCollider.
Use enemy hitbox overlap instead of a center point, with rigidbody bounds fallback.
Damage is checked throughout the visible flash and each enemy can be damaged
only once per individual strike. Additional strikes retain installed repeat timing.
Pause/upgrade UI suspends damage; room change, death and detach end the strike.

Ten standalone geometry regression checks pass, including upper overlap with
center outside, visible-edge contact, beyond-edge rejection, tip overlap, left
mirroring and increased area. Plugin compiles and design preflight passes. This
is not yet a gameplay confirmation of the new collision behavior. Installed
0.2.5 only after read-only process checks showed the game closed. Took a new
before-whip-hitbox save snapshot first. No game launch or game inputs were sent;
next in-game check remains with the player. No game content committed/published.

### Starter armor and whip bullet clearing: 0.2.6 (2026-10-10)

Player confirmed the 0.2.5 whip feels fairer. They reported Marine's helmet still
being retained and requested whip destruction of enemy projectiles. Prior log
proved zero starter active/passive items, but code still retained innate starting
armor. Marine's armor-loss path references helmet debris; his armor is separate
from passive inventory. Normalized initial dungeon entrance health to three hearts
and armor to zero, and requested armorless host animation. Armor-only health state
is cleared and base health set so an armor-only character can survive this starter
conversion. This affects the starting entrance loadout; found armor/items remain.
The native Breach presentation still deliberately shows Gungeon characters there.
Other non-item character traits are not all normalized.

Inspected actual HealthHaver Armor setter, health setters, PlayerStats base-stat
setter and Projectile.DieInAir parameter names before hooks. Armor setter only
updates armor/UI; it does not invoke armor-loss blank or helmet-debris effects.
Whip checks live hostile-projectile collider overlap using the same slash rectangle
and active window. Player-owned shots and bullets that cannot collide with the
player are excluded. DieInAir suppresses effects and actor/projectile spawning;
clearing a bullet does not grant kill XP. Beam attacks are not regular projectiles
and are not covered by this implementation. This is an explicitly requested
mashup mechanic; no claim that the installed native VS whip destroys bullets has
been established from guest native code.

Changed JSON settings before implementation. Plugin compiles and design preflight
passes. Installed only after the game process was absent, with a new save backup
named gungeon-survivors-before-armor-bullet. No game launched or game inputs sent.
Gameplay checks for starter armor removal, armor-only characters, controller
confirmation and bullet clearing remain with the player; no new in-game pass is
claimed from compilation. No game content committed or published.

### Progression and survivor barrel rolls: 0.2.7 (2026-10-10)

Player requested leaving the intermittent 8BitDo input issue alone because it
stopped. No input/config/controller changes made. Read-only logs had shown device
switches; their cause was not established.

Installed companion tables already supplied additive weapon rank changes to amount,
power, interval and area. Wand rank seven penetration was parsed but unused; added
native PierceProjModifier for extra targets (installed penetrating count minus
one initial target). Accessories/evolutions remain unimplemented. This is not the
complete Vampire Survivors progression system.

Changed JSON before code. XP requirement is 8 + 4*(level-1) + .5*(level-1)^2.
Enemy reward uses square root of max health / 15, clamped .5..5; bosses have a
10 XP floor. Survivor growth and xp_per_kill multiply this reward. XP overflow
is retained, and HUD shows current requirement. This is mashup balance, not a
claim of matching VS's XP curve. Inspected installed OnKilledEnemyContext and
OnDidDamage before replacing the context-free subscription; both whip and native
projectile kills use the existing credited-kill path, without duplicate listeners.

Survivor sprite performs one full rotation over actual rollStats.GetModifiedTime,
with direction determined by roll direction. Uses scaled host time and returns
upright at roll end; native movement/invulnerability remain in control. Inspected
actual roll API before implementation. No companion sprites modified or shipped.

Plugin compiles against installed host. Design preflight passes; nine Python tests
and progression regression (rising costs, weak/strong rewards, caps, boss floor,
and multi-level overflow) pass. First unittest invocation used the wrong working
directory and was rerun successfully from the project. Current saves backed up as
gungeon-survivors-before-progression-roll. Game process absent before installing.
No game launch/input sent. Barrel roll and penetration require next user gameplay
verification; no new in-game pass claimed. Nothing published.

### Accessories, capped inventory and boss evolutions: 0.2.8 (2026-10-10)

Player reported substantially smoother gameplay in 0.2.7. Wand rank-seven
penetration remains unconfirmed; player intends to report when reached. Requested
accessories, independent six-weapon/six-accessory limits, evolutions and inventory
HUD. Updated sheets before implementation. Read installed WEAPON_DATA directly
with UnityPy, verifying six passive selectors and their additive rank tables:
MAXHEALTH, COOLDOWN, POWER, AREA, AMOUNT and GROWTH. Imported icons to private cache
only. Duplicator has two ranks; the other five have five in this installed copy.
Reader validates evolution partners are present. No assets/data tables copied
into source, no publication.

Six companion weapon and six accessory slots are independent; the neutral aimed
gun is outside the companion cap. Caps checked both when offering and acquiring.
Full inventories retain owned rank upgrades, while maxed/evolved items are
excluded. Level menus sample up to three choices with a run/level seed and retain
native controller/keyboard/mouse controls (third number key added). HUD displays
two rows of six icon/rank cells and XP bar; evolved icons replace base icons.

Accessories apply installed additive stats to automatic companion weapons and XP.
Hollow Heart uses an unsaved mod-owned native ownerless health modifier instead
of overwriting found-item stats; this is retained across same-player floor attach,
updated/reset with accessory inventory and removed when plugin shuts down.
Actual StatModifier fields/RecalculateStats/Projectile.RuntimeUpdateScale inspected
before hooks. Area adjusts whip rectangle and native wand projectile scale.
Pierce count is reset on pooled automatic projectile spawn/disable so lower-rank
shots do not inherit it. Physical-controller selection was seen in prior player
logs; the expanded three-choice UI still needs a real gameplay check.

Credited boss kills offer one ready evolution when weapon is at its installed max
(rank eight) and its required accessory is owned at any rank. Reward selection
waits until native input is AllInput, avoiding interrupting boss cutscene input.
Evolution replaces its slot and does not consume an ordinary level-up. No banked
reward if no weapon was ready when boss died. If two are eligible, choose one;
next qualifying boss can evolve the other. This is explicitly Gungeon boss-reward
adaptation, not a claim of native VS chest timing.

Bloody Tear and Holy Wand use installed evolution base statistics (not additive
on top of old weapon ranks). Bloody Tear uses installed 10% critical chance and
2x multiplier, and heals half a host heart on a damaging critical at most once
per second; healing scale/cooldown are sheet-defined mashup tuning. Invulnerable
hits do not heal. Red-tinted existing slash is a placeholder for evolved VFX.
Holy Wand uses installed faster interval, speed and penetration. Accessory effects
on aimed gun stats are deliberately outside current scope.

Installed-content extraction succeeds; plugin compiles against actual host.
Nine Python tests pass, design preflight passes, 14 inventory cap/rank/evolution
checks pass, progression checks pass, and existing 17 controller-navigation and
10 whip-collision checks pass. No new in-game accessory/evolution pass claimed.
Game was relaunched during work; installation held until player explicitly
reported closed and process absence confirmed. New save snapshot:
gungeon-survivors-before-accessories-evolutions/20261010-004809.zip.
Installed DLL SHA256 2921509E08562096DCC9D3E810708A700AF6DED075E3EA5F10B896E62552C7A6.
No game launch/focus/input sent. LOCAL_DEVELOPMENT.md updated to remove obsolete
flat-XP, translucent-roll and no-accessory/evolution status. NEXT_SESSION.md records
continuation and honest gameplay verification gaps.
