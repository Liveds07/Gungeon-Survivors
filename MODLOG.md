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

## 2026-10-10 - 0.2.9 character picker and native action fixes

JSON slice policies updated before implementation. Added Antonio/Imelda picker
before a run in the Breach, with dungeon-entrance fallback and F6 reopening
before combat/first kill. Installed companion portraits and weapon names; native
controller actions, stick hysteresis, Up/Down/Enter, 1/2 and mouse confirmation.
Own input override and time multiplier are cleared on selection/loading/shutdown.

Inspected the installed host's actual PlayerController roll, slide, item-get,
queued-animation completion, chest interaction and GameManager restart code.
Forcing ArmorlessAnimations previously selected nonexistent *_armorless clips on
some host characters. Missing item-get/slide clips could retain queued animations
and input locks; missing dodge clips lost native frame invulnerability. Preserve
host armorless capability instead, keep native animator updating while visually
hidden, restore its prior setting on detach. No replacement of native animation
libraries, forced invulnerability, or clearing unrelated input overrides.

Upgrade/picker opening waits for native ready input, no roll, slide, queued
animation, pause or loading. Seven independent action-gate checks pass.
Harmony prefix on actual QuickRestart resets run/loadout/selection state even
when CurrentRunSeed is unchanged. Starter stripping waits for ready input and
actual Dungeon.data.Entrance rather than treating missing room state as done.
Build references the installed BepInEx 5 core Harmony assembly.

Actual gameplay checks performed with explicit user authorization for game input:
- Breach picker rendered; Enter selected Antonio. Dungeon fallback rendered;
  F6 reopened, Down/Enter chose Imelda; the 2 shortcut and mouse also confirmed.
- Marine-backed host had armorlessCapability=False. Runtime normal dodge/item-get
  clips existed, corresponding armorless clips were absent after entering run.
  Antonio and post-F6 Imelda each rolled four directions: 22 sampled invulnerable
  frames per direction, then AllInput, rolling=False, queued=False.
- A host with supported armorless clips retained its native capability. Native
  ApplyDamage during each of four dodge directions left HP 3 -> 3, including
  four further checks after F6 swapping to Imelda. These were real damage calls,
  not setting health or invulnerability to manufacture a pass.
- Native TriggerItemAcquisition completed and returned to AllInput/queued=False.
  Real locked Chest_Wood_Two_Items opened through Interact; normal input returned
  afterward on both tested host animation capabilities.
- Actual generated table slides and flips returned to AllInput/queued=False.
  Dense fixture moved two existing tables adjacent at runtime (no game-file edit):
  second right edge x=128.5625, player final x=129.1875, sawSlide=True,
  rolling=False, queued=False. This is a controlled chained-slide check.
- Native quick restart reopened picker and cleared loadout again: one
  Rusty_Sidearm, zero passives/active items, armor zero, level one, kills zero.
- Real automatic wand/whip attacks and enemy kills continued during these checks.

Nine Python tests and design preflight pass; independent action gate (7), menu
navigation (17), inventory (14), progression, whip collision (10) checks pass.
Physical 8BitDo input was not driven in this automated pass. Accessories,
evolutions, rank-seven wand penetration and full floor persistence still require
separate gameplay checks. No complete first-playable/release claim.

Temporary diagnostic helper stayed outside source and was removed from lab after
game exit. Native menus needed mouse selection and adequate transition waits;
an early helper floor load skipped Foyer.OnDepartedFoyer and was corrected before
valid run checks. Private evidence: modding-tools/picker-actions-pass1/2/3.log.
Pretest snapshot: gungeon-survivors-before-picker-actions/20261010-125923.zip.
Restored immediately pretest saves after exit; backup diff shows no changed or
missing files, only three added Unity analytics events. Pre-restore snapshot also
retained. Original game installation untouched; installed companion content
remains private. No publication. Installed DLL SHA256:
9AA9E5A07D2E9F0079239B0FD235D41A6F951A665275301DF9B7423F9A70E90F.


## 2026-10-10 - 0.2.10 native survivor scenery depth; queued content recon

Updated JSON slice depth policy before implementation. Survivor visuals now use
private companion sprite geometry in a MeshRenderer with a runtime clone of the
native player material, native sorting layer/order, native body world depth and
the installed tk2dBaseSprite.SetPositions perpendicular vertex-depth projection.
The old flat SpriteRenderer stays only as the current-frame/flip/rotation source.
Native sprite/animator remain intact for roll/slide/pickup behavior. Owned visual
mesh and material are destroyed on detach. No native material or game assets
written into source. Runtime inspected: perpendicular=True, Brave/PlayerShader,
queue=3000, native top vertices project height into negative local Z.

Actual visual tests exposed that world Z/layer changes alone were insufficient;
flat/default-material art still hid behind the altar while the native character
at the identical position appeared in front. Final native geometry + material
renders Antonio in front correctly. Compared native and survivor at the same
position at the starting altar and generated merchant counter. Antonio and Imelda
both rendered correctly at the counter. Post-F6 Imelda and Antonio four-direction
native roll checks rejected actual damage, HP 3 -> 3, returned AllInput with no
queued roll state. Final Unity log search found no NullReferenceException,
MissingMethodException or ArgumentException. Walking input also exercised.
Private screenshots/logs in modding-tools/depth-*, outside source; not published.
Compilation, nine Python safety/design tests and design preflight pass.

Poe/Garlic/Pummarola/Soul Eater explicitly deferred under the user's permission
to leave additions for later. Inspected installed resources.assets, not web data:
CHARACTER_DATA/CAMILLO is Poe Ratcho; WEAPON_DATA/GARLIC, REGEN (Pummarola),
VORTEX (Soul Eater). Poe has lower max HP and positive pickup range. Also found
Antonio's installed health baseline differs from Imelda's; future character
health adaptation must read each value rather than assume both are identical.
Current XP is awarded immediately on kills, so implementing meaningful magnet
requires collectible XP or another explicitly designed pickup adaptation.
Persistent aura damage must use per-enemy hit cooldowns. Regen conversion should
be gradual, capped by missing health, paused with gameplay, with no banked heal.
These are proposals requiring new real-game checks, not enabled features.

Cultist ending remains unchanged/deferred: installed CoopPastController.Update
and both intro/outro coroutine bodies directly access SecondaryPlayer alongside
PrimaryPlayer. A solo-safe version needs adaptation and complete ending tests;
do not change characterIdentity globally or load the two-player past in solo.
Recon selectors, constraints and proposed tuning saved in design/next-content.json
(status queued_not_implemented), separate from active design/sheets.json.

Pretest save snapshot: gungeon-survivors-before-depth/20261010-133407.zip.
After final game exit, restored that snapshot. Diff: no changed/missing files,
only three new Unity analytics events. Pre-restore snapshot retained. Diagnostic
plugin removed; game closed. Installed DLL matches build SHA256:
30FC7B1AAAB639D0C37E1DF7A70FD5F04BF4620807C94729E40CE07AE7E1A268.
No game files redistributed, original installations untouched, no publication.

## 2026-10-10 - 0.2.11 compact picker and chest rewards

Updated JSON policies first. Installed companion reader now supplies full character
names and descriptions privately. Picker uses small portrait tiles with a shared
description below; hover and horizontal/vertical keyboard/native controller actions
change focus. Stationary mouse does not override controller focus. Actual screenshots
confirm Antonio and Imelda descriptions; mouse hover and keyboard confirm passed.
Physical 8BitDo picker input still awaits player verification.

Inspected installed Chest.Open(PlayerController), Interact, OnBroken, PresentItem
and PlayerController.OnChestBroken. Harmony observes successful unopened-to-open
transitions, excluding failed locks, mimic transformation and already opened/broken
chests. Successful open grants one survivor XP level, XP resets to zero, ordinary
choice queues; eligible evolutions snapshot and queue first. Credited boss rewards
retain their behavior. Exact-once ledger clears with run reset. Native chest break
animation/cleanup/event retained; PresentItem suppressed for unopened broken chests.
Break grants exactly 1 XP without growth multiplication and no evolution.

Real isolated dungeon checks: locked chest without key stayed closed, level1 XP0.
MajorBreakable.Break on actual chest yielded broken=True, XP1, level1, no choices,
with native loot suppression logged. Another actual chest Interact opened, level2
XP0, three normal upgrade choices; confirm restored AllInput. Repeated Interact
left level/XP unchanged. Runtime-only test fixture supplied Whip rank8 and Hollow
Heart rank1, max host HP3.5. Actual silver chest opened, guaranteed Bloody Tear
choice appeared, selecting it replaced Whip in the same slot; regular choice then
appeared and confirmed. Final AllInput, timeScale1, no queued animation, no pending
choices. No NullReferenceException/MissingMethodException/ArgumentException in
Unity evidence. This verifies selection, not evolved combat/healing. Private
screenshots and chest-rewards-pass.log/chest-unity-pass.log remain outside source.
Compilation, 12 chest-transition regression checks, 9 Python safety/design tests
and design preflight pass. No publication or redistribution.

Automatic-test save backup: gungeon-survivors-before-chest-rewards/20261010-140836.zip.
After closing the test game, restored all 15 snapshot files with zero mismatches;
three added Unity analytics events retained. Pre-restore snapshot retained.
Installed/build SHA256: 409C37C367D30190C60018A404C2EF46BE35ED587EAD120F4B85B7ED123AC07A.
User requested a fresh live Whip8/HollowHeart1 test fixture; separate save snapshot
before that run. Normal starts unchanged. Endings explicitly on hold; new queued
content remains deferred. Next session should preserve the user's subsequent progress.

Live user handoff: Antonio Whip8/HollowHeart1, HP3.5, key supplied, unopened green
chest at reward-room position(38.1,66.4), AllInput, level1, no pending menus.
User acknowledged starter equipment and took control. Diagnostic helper disabled
(GS_TEST_HANDOFF). Removal denied while DLL loaded by the game; hidden cleanup
helper waits for game exit then removes only that DLL and diagnostic command file.
Verify next session. No further gameplay inputs sent.
Separate pre-user snapshot: before-user-evolution-test/20261010-141240.zip.


## 0.2.12: Poe, combat-only regeneration and corrected chests (2026-10-10)

Read README/MODLOG and JSON first; updated design before implementation. Installed
VS selectors CAMILLO/GARLIC/REGEN/VORTEX provide Poe, Garlic, Pummarola and Soul Eater.
Native IL2CPP analysis remains private outside source. Visible Garlic uses Rings3;
Soul Eater uses circle. Native VampiricaProjectile builds three offset slash
layers (dark main, red/purple additive ghosts), rather than a single red tint.
Implemented that structure using privately read installed art.

In-game: Poe had two hearts and Garlic. Native door entry activated combat;
Garlic damaged a Gun Nut and killed a weak enemy, whose XP gem was collected.
Pummarola5 recovered half-heart chunks during combat at converted rate0.03 hearts/s;
20 seconds outside combat recovered nothing. A controlled gem 2.05 units away
remained uncollected with Imelda's 1.8 radius, then collected with Poe's 2.25 radius.
Opening a real chest upgraded Garlic1 to2, keeping survivor level1 and XP1.5.
Repeat interaction gave no reward. Breaking another chest gave exactly1XP with
native item loot suppressed. A fresh real chest with Garlic8/Pummarola1 offered
Soul Eater: level1 and XP0.5 stayed unchanged, native input resumed, no follow-up
ordinary upgrade. Soul Eater damaged an actual Bullet Kin for8 then8.4 after
recovering0.5 heart and gaining0.05 power bonus. Healing chance was forced to1
only in private test memory to deterministically exercise this path; source
and final installed build retain0.1. No enemy AI flags changed in this test.

Bloody Tear fixture logged three renderers and additive shader available. Capture
was invalid as the game exited; appearance is not yet visually verified. Soul
Eater particles and Garlic resistance effects are incomplete; balance and physical
controller selection of Poe require player testing. Endings remain on hold.

Validation: compiled against installed Gungeon assemblies; nine Python tests and
design preflight passed. ChestRewardRegression and SurvivorTuningRegression each
passed12 checks earlier in this update. Final local DLL installed and hash matched.
Both automated sessions used pre-test snapshot
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-before-poe\20261010-150928.zip.
Game closed; preserved private logs and a pre-restore snapshot, restored every
backup member and verified byte equality. New analytics retained. Diagnostic DLL
and command file removed. Nothing published or game-derived files added to source.


## 0.2.13: wand balance, enemy density, boss XP and cinematic UI (2026-10-10)

Player completed an Imelda run around level11; wand/evolution felt far too strong.
Updated JSON tuning before implementing: Wand and Holy Wand bullet damage x0.25,
XP gem scale0.45->0.9, one additional counterpart per eligible regular enemy,
boss XP10 times native CurrentFloor (minimum1), hide UI during native sequences.
Challenge rooms deferred until the user supplies their gameplay design.

Inspected actual installed Assembly-CSharp signatures and IL for RoomHandler
AddSpecificEnemyToRoomProcedurally/RegisterEnemy, AIActor.Spawn, GameManager
CurrentFloor, native text boxes, Pixelator intro getter and GameUIRoot UI overrides.
Native procedural enemy spawning checks clearance and available floor cells,
registers enemies for room clear and uses reinforcement entrance. A scoped
RegisterEnemy hook marks copies immediately to prevent recursive duplication.
Regular NORMAL rooms only: excludes bosses, boss rooms, companions, harmless
enemies and enemies ignored for room clear. Already handled originals stay
marked until dungeon changes; subsequent reinforcement enemies are eligible.
Placement failure can yield fewer copies; logs report actual additions.

Boss XP now uses exact floor reward before collection growth:10/20/30/40/50
on main floors1..5. Unknown/negative side-floor indices default to10; side-floor
balance remains provisional. Boss gems spawn at the player's feet to avoid
unreachable boss positions. Existing regular enemy health-weighted XP and level
curve remain unchanged; more eligible enemies provide more collectible XP.

Native UI visibility reads the actual private CoreUIHidden override value and
Pixelator intro getter, visible text boxes, conversation options, boss-intro
flag, pause/loading and native input lock. Upgrade choices close without consuming
their reward and reopen when native control returns. Automatic attacks do not
start during native sequences; wand bursts stop and whip hits defer. Death still
detaches/reset-cleans even when the native sequence gate is active.

Real checks: first isolated room had5 originals plus5 counterparts, no recursion;
all10 died and the room cleared. Actual wand bullets logged damage1 (previous4),
Holy Wand3 (previous12), retained penetration. Larger blue XP gems were visible
in inspected screenshots. Native HideCoreUI hid the mod HUD; restoring native UI
restored eligibility. A real TextBoxManager dialogue screenshot showed dialogue
without mod HUD. An XP choice suspended during dialogue retained pending1, then
reopened and confirmed with native input restored. Controlled native boss-intro
flag toggles hid/restored UI; a complete boss cinematic was not replayed.

The initial direct-load fixture bypassed native session setup and left enemy AI
untargeted; do not hand that fixture to the user. A fresh run through the modern
MainMenuFoyerController Play reveal and native Marine selection initialized the
Breach correctly. Native door entry then produced3 originals plus3 counterparts,
all6 acquired PlayerTarget and pursued/attacked. Wand dealt actual reduced damage.
The idle automated player died to a Bullet Kin; native death UI showed no mod HUD.
Legacy MainMenuController quick-start lookup was inapplicable to this menu; use
the modern native Play/selection flow for future testing.

Validation: compiled final0.2.13 against installed game references;9 Python tests,
design preflight and extended ProgressionRegression passed (boss floors1..5 and
unknown-floor fallback included). No targeted runtime exception signatures in
the final native-session Unity log. This is a targeted runtime check, not a full
new balance playthrough. Reinforcement-wave/exotic-room coverage and multi-floor
boss-kill rewards still need broader in-game checks; no completion claim for them.

Latest completed-run save snapshot made before any tests:
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-before-balance\20261010-180513.zip
Game closed; private logs and pre-restore state archived outside source. Restored
every backup member and verified byte equality, retained new analytics, removed
new test-generated root Slot files absent from baseline after archiving them.
BalanceTestDriver.dll and command file removed. Final build/installed DLL hashes
matched. No older pre-Poe snapshot restored over this user's completed run.
Nothing published or installed game files redistributed.

## 2026-10-10 — Challenge-room development (0.2.14 candidate)

Source-of-truth survival_rooms sheet: 600 gameplay seconds, cap 20, replacement
5 seconds after death, 32x26 arena, 14x7 corridor, XP-only enemies, unlocked
native S chest. Native LoopDungeonGenerator flow inspection and cloned side
branch prototype implemented; original flow node lists preserved.
Latest save backup before testing:
C:\Users\Propietario\.universal-modder\backups\gungeon-survivors-before-survival\20261010-191011.zip
First native generation exposed missing RuntimeInjectionFlags; corrected.
Candidate compiles. Nine Python tests and standalone SurvivalClockRegression
passed. Runtime validation in progress; this entry is not a completion claim.

## Startup crash isolation (2026-10-10)
User reported a crash during the latest candidate restart. Archived native output privately as modding-tools/survival-startup-crash.log. Native log shows dfAtlas Dictionary ArrayTypeMismatchException followed by a Unity/Mono native crash, before GS_CONTENT_READY or dungeon generation. Disabled the temporary BalanceTestDriver plugin and removed its pending command. A fresh launch with only Gungeon Survivors reached the main menu and GS_CONTENT_READY with no Exception/crash matches. This is one successful isolation check, not proof of the root cause. The two-cell doorway candidate and remaining floor/reward checks still need validation; do not call 0.2.14 finished or publish it. Save baseline remains before-survival/20261010-191011.zip and final restoration remains pending.


## 0.2.14: five-minute survival arenas and random treasures (2026-10-10)

Supersedes the 600-second/S-only candidate above. Updated JSON first: 300
gameplay seconds, cap20, replacement5, arena32x26, corridor14x7, two exit cells,
four wall rectangles, two column islands, four installed flippable tables.
Main-floor unlocked rewards D/C/B/A/S on1/2/3/4/5. Sign is a physical wooden
board, built from original procedural pixels in code, with IPlayerInteractable
and native ShowWoodPanel rules; no permanently displayed rules HUD. Countdown
appears only while a challenge is running. Native assets remain local only.

Inspected installed IPlayerInteractable/NoteDoer, room cell/prototype APIs,
native cover placement/occupancy and chest LootData/RewardManager metadata/IL.
Use native wall cells and DungeonPlaceableUtility for tables. Flow copies add
two nodes while retaining original nodes/edges/loops and parent exit demand.
GetRandomFlow also injects native generation retries. Exact Foyer Flow exclusion
avoids accidentally skipping main-floor BossFoyer flows. Arena sealing waits
until five tiles inside so players are not pushed onto the wrong side.

Chest rewards read new chest_rewards sheet: D/C/B1, A3, S5 random ranks.
Each slot selects an owned weapon that is neither evolved nor max rank;
eligibility is rebuilt after each grant. Eligible evolution at opening gets
a choice, consumes one slot, then remaining slots grant random ranks. Stale
chest evolution queues fall back to ranks; stale boss-only queues do not award
invented chest ranks. No character XP/level change. Break/deduplication policy
is unchanged. Native later item pickups remain usable as in the existing slice.

Real duration evidence: survival-real-five-minutes.log / -unity.log in private
modding-tools. Actual clock reached300 without time acceleration and awarded
one unlocked D chest; native pause held elapsed82.76983 across three probes.
Twenty initial enemies, replacements after5 gameplay seconds, real pursuit/
shooting, credited-kill XP gems and collection were observed. Only private
health invulnerability was used to complete the unattended duration test.
Native sign interaction opened/dismissed wooden rules with HUD hidden/restored.

Real chest fixtures in that session: zero-key D/C/B each gave one rank; A gave
three; S gave five from rank1 and stopped at8 when only one rank remained.
Magic Wand8 + Empty Tome in an A chest offered Holy Wand; confirmation applied
that evolution and two random Whip ranks. A later S gave Whip3->8. Character
level1 and XP7.865735 remained unchanged throughout. Normal chests did not
open the mod choice UI. Native opening animations finished without a softlock.

Found and fixed a real currency loophole: native AdditionalSingleCoinDropChance
can bypass CanDropCurrency. Clear that bonus and suppress HandleLootPinata
only for arena enemies. Actual separate player kill events still grant XP.
Final test deliberately forced99 coins and bonus chance1 on enemies: native
wand kills awarded XP and reached character level2 while currency stayed0.
This fixture was private memory only. Native table interaction ran; the room's
walls/columns and horizontal tables were visible. Door traversal passed after
aligning the off-center warp fixture; opening fits native door width.

Final main-floor evidence: survival-final-floor-sweep.log / -unity.log outside
source. READY and cover placement on all five floors, with pools16/13/23/17/18.
Completion awarded exactly one unlocked D/C/B/A/S chest respectively. C/B/A/S
each opened with keys0. Floor sweep completion was accelerated, distinct from
the preceding real full-duration run. Surviving actors/projectiles cleared and
alive count reset0. Source's final cosmetic change fits sign lettering inside
its board. Secret/side floors and a full new balance playthrough remain pending.

Crash investigation: original native dfAtlas ArrayTypeMismatchException and
Unity/Mono crash happened before content readiness/generation. Multiple clean
and replacement-helper starts passed, as did the five-floor session. A controlled
recheck with the old helper initialized successfully and did not reproduce the
original crash. Some delayed terminal checks initially looked like a startup
hang; returned logs did not support that diagnosis. Old helper permanently
removed. Exact crash root cause remains unconfirmed; no definitive fix claim.
Archive native log immediately before relaunch if the player sees a recurrence.
Initial crash and comparison evidence are private survival-startup-crash.log
and survival-old-helper-comparison-final*.log. Private fixture-only lookup
errors were recorded separately; they are not production exceptions.

Validation: final DLL compiled against installed game/loader references;
10 Python tests and design preflight passed, including reachable cover layout.
SurvivalClockRegression passed300-second cap/replacement/pause/retry/abort;
ChestRewardRegression passed12 duplicate/open/break transition checks.
Game-derived assets/assemblies/dumps were not added to source; only the mod DLL
exists in ignored build/. No publication or Melty account writes.

Player save baseline before-survival/20261010-191011.zip restored and every
native backup member byte-verified after tests, with test saves archived first.
Both diagnostic DLLs and command file removed; final source/installed DLL SHA256
matched. Final clean startup check is recorded separately below.

Final clean startup: only GungeonSurvivors.dll installed; GS_CONTENT_READY reached, main-menu screenshot inspected, no Exception/ArrayTypeMismatch/crash matches in native log. Saved privately as survival-final-clean-startup*.log and survival-final-clean-menu.png. Installed SHA256 verified against final build. Game left at the native main menu for player handoff; no automation remains running. Original intermittent startup crash was not reproduced, and remains an unconfirmed cause rather than a proven fix.



## 0.2.15 ? challenge rebalance and varied upgrades (2026-10-10)

User correction supersedes earlier caps: main floors use 12/14/16/18/20 enemies,
with 10 gameplay seconds between death and scheduled replacement. JSON sheets
updated first. Completion chest is A with S chances 0/25/50/75/100%; native
RewardManager.FullHeartIdPrefab and LootEngine.SpawnItem (inspected in installed
Assembly-CSharp metadata) supply two full hearts on each side, four total.

Fixed offspring suppression: old RegisterEnemy prefix destroyed every enemy
not spawned by the challenge scheduler, preventing native slime splitting and
summoners. Retain native actors; adopt into a vacant scheduled replacement slot
or hold inactive at cap. Admit pending children in arrival order before normal
replacement waves; cleanup also removes pending children. This keeps actual
active enemy count within the floor cap. Native SpawnEnemyOnDeath code inspected
locally; no decompiled files stored in source.

Sign is mounted higher on corridor north wall beside door, floor post removed,
interaction distance targets reachable floor below it. In-range read prompt and
highlight added; native binding still calls IPlayerInteractable.Interact and
opens/dismisses wooden rules. Controller uses the host interaction binding;
physical controller test of this repositioned sign remains pending.

Chests uniformly pick from combined eligible owned weapon/accessory slots,
rechecking caps each grant. Eligible evolutions retain priority and consume one
slot. Level offers use Fisher-Yates on eligible choices with persistent per-run
RNG seeded from a new GUID, also used by chest ranks. Removed reseeding from
fixed native seed/level indices. No forced Empty Tome exclusion. OS crypto RNG
Dispose/Clear APIs were unavailable in host Mono references; Guid.NewGuid works
in actual host. New GS_CHOICE_OFFERS log records offered IDs and native seed.

Real 0.2.15 session: floor1 started exactly12; native Blobuloid split produced
Blobulin offspring that waited/adopted without surpassing12. Native XP collection
observed, currency0. Native E interaction displayed updated sign rules; wall
placement/rules screenshots inspected. Completion floor1 gave A plus four
FullHeart(Clone) objects, visibly two each side. Zero-key A opened and granted
Empty Tome2, Empty Tome3, Candelabrador2, character level1/XP0.8164966 unchanged.
Fresh first-offer fixtures with same native seed differed: Garlic/Spinach/
Pummarola versus Spinach/Pummarola/Candelabrador. Fixtures reset only mod run
state; not a full natural quick-restart playthrough.

Native floor sweep caps14/16/18/20 and completion chestsS/A/S/S on floors2?5;
four hearts logged each. Rewards were tested with accelerated completion, not
five uninterrupted minutes per floor. Guaranteed floor5 S with Whip8+Hollow
Heart offered Bloody Tear; confirmation gave evolution plus four random grants:
Hollow Heart2/3/4 and Magic Wand2. Character level1/XP0 unchanged.
Private evidence: rebalance-five-floors.log, rebalance-final-native.log and
rebalance-final-unity.log; inspected screenshots rebalance-wall-sign.png,
rebalance-sign.png, rebalance-reward.png, rebalance-offer1/2.png.

Final compile against installed references passed. Ten Python tests and design
preflight passed. SurvivalClockRegression covers all five caps, 10-second
deadlines, native-child adoption consuming replacement, duplicate deaths,
paused clock, completion, placement retry and abort. Final refinement changes
pending admission to FIFO; clean startup checked separately. Full new balance
playthrough, exhaustive enemy summoner variants and side floors remain pending.

Current player saves backed up before tests to before-rebalance/20261010-213347.zip;
test state archived, native backup files restored and byte-verified. This is the
new baseline; never reuse older backups over later player progress. Diagnostics
and command file removed. Installed final build hash matches source:
60117C9BC1FD39B86A507BF2AFD972DBFFA4ADDDD9013CD34A144E9108F5C078.
No publication, game-file redistribution or account writes.

Final clean startup: only GungeonSurvivors.dll loaded, version 0.2.15 and GS_CONTENT_READY observed. Native menu screenshot rebalance-final-clean-menu.png inspected. No Exception/ArrayTypeMismatch/crash log matches. Final source/installed SHA256 matches. Game left at native main menu; controller handed back, no automation pending.

Heart reward correction: user clarified one full heart per side, two total. JSON hearts_per_side changed to1; sign now reads its quantity from JSON. Build pending installation while player game is running. Summoned/split enemies remain included in active enemy cap and wait inactive at capacity; no exemption added. Previous four-heart evidence is historical, not verification of the corrected quantity.

User changed offspring policy: native summons/splits now spawn immediately, exempt from the scheduled-enemy cap. Separate offspring tracking preserves cleanup and XP/no-money behavior; their deaths neither schedule replacements nor consume reserved wave slots. HUD and sign explain scheduled versus summoned counts. Two-heart correction retained. Compilation/testing below; installation still pending because game is running.

Pending heart/summon update installed and tested after player closed the game.
New save baseline before-exempt-summons/20261010-215450.zip backed up before
testing, restored and byte-verified afterward. No diagnostic DLL/command remains.
Installed build SHA256 matches source: ac35a0f55d7781bfa923cf1a660e9459c184e90f9386e7d48dc27f164c822b07.
Targeted native Blobuloid fixture: active immediately; its natural death spawned
two Blobulins at time34.48375 while scheduledAlive stayed12. Probe active2 and
HUD scheduled12/12, summoned2; native kills still produced XP, currency0.
A scheduled death at5.466863 was replaced at15.48322, consistent with10 gameplay
seconds. Offspring never consume scheduled slots or schedule replacement waves.
First random floor pool contained no native splitter, so targeted second session
loaded the previously inspected installed Blobuloid GUID directly in private
fixture. No production prefab mutation. Private screenshot/exempt-final-native.log
and exempt-final-unity.log retain evidence. Completion fixture accelerated to300:
A chest, exactly two FullHeart(Clone) pickups on opposite sides; completion
counts scheduled0 and offspring0. No full new balance run claimed.
Game closed after tests; updated mod is ready on next launch. No publication.

2026-10-11: player reports interaction hint appears as a black/white pixel line. World-space TextMesh hint was tiny and placed below board where player/depth could obscure it. Replaced with 340x40 screen-space panel, 18px white text, shown only in interaction range. Integrated through existing HUD native-sequence/pause/picker guards; suppressed during upgrade choice and open sign rules. Native interaction and board/title unchanged. JSON sign_policy updated before code and in-game verification cleared. Prepared source/build only while player checks current version; new visual has not been tested or installed.

2026-10-11: user closed game and explicitly requested immediate installation with their own visual test. Installed readable sign-prompt build; source/installed SHA256 verified: B5253504DC3EF502D7F9FFBE7C6C84C3C6A0BE7034391C07B823592E41C4A057. Game left closed, saves untouched, no diagnostic helper installed. Visual verification awaits player report.


## Player notes ? queued next update (2026-10-11)

User asked to note these down for now. No implementation or installed-build change
performed for this report. Address these before expanding the character roster;
update JSON design sheets before implementing each accepted behavior.

- Hollow Heart: increases maximum health capacity only; acquiring/ranking it must
  preserve current HP, including chest-granted ranks. Investigate native stat
  recalculation healing before changing it. Spare capacity must be filled normally.
- Remove custom UI stick sensitivity/dead-zone/hysteresis changes for now. User
  suspects intermittent movement loss; this is a hypothesis, not a confirmed cause.
  Preserve working native controller confirmation and ensure closing any mod menu
  releases its input override and time-scale override. Verify normal movement after
  character selection, level-up, quick restart and native interaction.
- Disable controller rumble during survivor selection and upgrade/evolution UI.
  Quick restart followed by character selection currently vibrates heavily. Inspect
  actual installed vibration APIs and avoid restarting stale vibration on UI exit.
- Replace the sign's long rules text with exactly:
  "Survive for 5 minutes against endless hordes of enemies and obtain great rewards in exchange."
  Keep the physical sign interactive and the readable proximity prompt.
- Entrance bug: user encountered challenge through the RIGHT door instead of the
  advertised upper door, causing unannounced combat. Inspect generated flow/exit
  orientation across layouts; sign must identify the actual challenge doorway,
  and ordinary hall exits must never unexpectedly trigger it. Preserve optional
  branch and main route. Reproduce using real generated floors before claiming fix.
- Challenge HUD: show only scheduled population, label "Enemies". Remove displayed
  summoned count; summons and slime offspring remain exempt from the floor cap.

After these corrections, next proposed character is Pasqualina with Runetracer,
including its shooting-star-like particle trail; read stats/sprites/effects from
installed Vampire Survivors. Add Armor accessory with user's mashup proposal:
armor grants a temporary bullet shield / extra HP bar instead of subtracting
incoming damage. Shield size, refill trigger/cooldown, stacking and damage-type
coverage are not specified yet; resolve tuning before implementation. Do not
silently replace this request with Gungeon's ordinary permanent armor hearts.
Solo remains required; no publication or game-file redistribution.

2026-10-11 player clarification: retain Antonio health bonus; test Hollow Heart scaling next with Imelda. Antonio should also start with one bullet shield. Existing starting_armor remains zero; dedicated armor/shield integration is pending and must not be claimed present. Hollow Heart rank changes must preserve current HP exactly.


## 0.2.16: health, native armor, Clover and menu corrections (2026-10-11)

Read README/MODLOG before work. Retained installed EtG2.1.9a/Unity2017.4.27f1 references and supported BepInEx5.4.23.5 private route. Read actual installed VS resources.assets; sourceSHA remains c930da284e079870242b367f809ba737499ec19b8f53c51e9a03d8e082b61ffe. Installed ANTONIO base maxHp120/armor1, IMELDA100/armor0; Clover selector LUCK, sprite Clover, five rows each luck0.1. User mapping deliberately grants one native Coolness per row, not the VS percentage. Native Cecil inspection confirms PlayerStats.StatType.Coolness and armor API already used by loadout. JSON updated before code; game data and sprites remain exclusively in private cache.

Hollow Heart native stat recalculation now preserves captured currentHP, suppressing transient heal observation/Soul Eater growth. Real Imelda test: maximum3.5,4,5,5.5,6 at ranks1..5 with currentHP1.5 every rank; native half-heart rounding retained. Antonio native starting loadout has armor1/max3.5; Imelda armor0/max3. Character picker grants installed base armor and ordinary floor attachment does not refill it.

Clover uses one separate additive ownerless Coolness modifier, ignored for save data, with cleanup on player detachment. Rank changes/removal preserve HP. Native tests logged Coolness1..5 from baseline0 and removal to0, currentHP1.5 throughout. Real level-up offered crown/pummarola/clover; virtual controller focused index1 then2 and confirmed Clover, owned accessories6. Native S chest ranked Clover2/3/4/5, stopped at max (four eligible grants from five reward slots), characterlevel1/XP0 unchanged. Native sprite/description/HUD screenshots inspected. Private fixtures temporarily maximized other inventory to isolate the new accessory; production offers remain uniformly shuffled, with no guaranteed Clover.

Both mod menus now use native directional PlayerAction.WasPressed without custom thresholds/latch/cooldown. Menu close clears its own BraveTime owner even if flags were already lost, preserving other owners; input cleanup clears only mod override keys and captured player owners. Native BraveInput vibration timers depend on paused BraveTime.DeltaTime; custom menus can otherwise retain timed rumble. Hooks clear native queued/sustained rumble and force motor requests zero while a mod menu is active. Virtual known Xbox-style pad proved queued0/motors0,0 in survivor picker, upgrade choice and quick-restart picker; native button confirmation restored AllInput/timeScale1. Former private unknown-device fixture lost its state and failed reflection; corrected fixture uses actual known controller metadata and public LateUpdate. No production exception attributed to those diagnostic failures.

Native death-page Update gates pad select/cancel on IsKeyboardAndMouse/LastInputType. Scope-limited hook permits physical pad buttons in that death-page update only. Test logged keyboardBeforeTrue/padSelectTrue with no stick movement, then native GS_QUICK_RESTART. Fresh loadout guns1/passives0/actives0, Imelda armor0/max3 and required picker observed. Physical 8BitDo validation remains separate. Exact cause of intermittent walking loss remains unconfirmed; custom menu sensitivity did not directly modify global walking input.

Exit filtering now spans native GetExitPairsSimple/ForNode/PreferDistance/GetNumberOfIdealExitPairs, including fallback paths. Real connectedRoomsByExit verified NORTH->GS Challenge Arena and EAST/WEST->ordinary rooms on floors1..5; a second first-floor layout also generated. Mines needed multiple native generation retries before completing; no guarantee of generation latency. Short exact sign rules displayed in native wood panel; GS HUD uses Enemies only and hides offspring count, with offspring still exempt from scheduled cap. No new complete five-minute endurance run claimed.

Evidence outside source: clover-health-five-floors.log, imelda-six-hearts.png, clover-offer.png, clover-chest-rank5.png, menu-short-sign.png and menu-death-before-button.png in private modding-tools. Ten existing Python checks and design preflight passed; compilation used installed game references. Tests backed by before-menu-fixes/20261011-020117.zip; each session archived and baseline restored/byte-verified before later tests. Final cleanup/install recorded below. No publication or game asset redistribution.

Final HUD native screenshot menu-enemies-hud.png inspected: SURVIVAL 04:51, Enemies:12/12, no displayed offspring count. Test invulnerability was private fixture only and will be removed with diagnostic cleanup.

Final0.2.16 cleanup: before-menu-fixes/20261011-020117.zip restored and byte-verified. Diagnostic helpers/command removed; only production GungeonSurvivors.dll remains in lab plugins. Source/installed SHA256 95a6ffc720bf293b3c7a05ad239684cc304ff6aeee783634534adab225ddc20c. Clover sheet cells and Antonio/Imelda armor_policy marked in_game using native evidence above; this is not blanket release verification.

Clean handoff startup: only one plugin, Gungeon Survivors0.2.16, GS_CONTENT_READY with39 sprites. Clean screenshot clover-clean-menu.png actually shows native Breach; game left there for player control. No diagnostic helper or pending command remains, and no further automated input is scheduled.

0.2.17 implementation started: Pasqualina/Runetracer/NO FUTURE/Armor. JSON defines native Armor rank grants once per run, without regeneration. Installed code metadata DiamondProjectile/CarnageProjectile exposes trail, bounce and expiration; sharedassets3 prefab child renderers point to resources Diamond2. NO FUTURE weapon bundle contains Screeno_0..29. MonoBehaviour type-tree reading reports a layout mismatch, so effect inspection changed to serialized child SpriteRenderer/TrailRenderer and existing private IL2CPP metadata. New save backup before-pasqualina/20261011-030216.zip captures the latest player progress; do not restore older snapshots. Private BepInEx5 route retained; no publication.

0.2.17 completed local vertical slice: Pasqualina/Runetracer/Armor/NO FUTURE.
Installed tables and native Diamond/Carnage metadata inspected before hooks;
all artwork stays in private runtime cache. No game asset/code redistribution.
Pasqualina native3 hearts/armor0, projectile speed1.1,1.2,1.3,1.4 at1/5/10/15;
level20 remained1.4. Character selection supports fourth compact portrait.
DIAMOND native tile bounces, finite installed2250/3250ms rank lifetime, installed
amount/speed/power and500ms swept actor hits; quarter wand-equivalent host damage.
A fading native TrailRenderer follows installed Diamond2. Evolved ROCHER adds
Screeno_0..29 native-installed explosion artwork, bounce AOEs and OnDamaged
retaliation. Actual host OnDamaged passes currentHP/maxHP, not damage amount;
native armor branch sets damage0 but still fires that event after losing armor.
Five Armor ranks each granted1 shield; five forced native hits consumed them
without changingHP1.5. Repeated ApplyAccessoryHealth never refilled shields.
Owned Armor with0 shields still qualified rank8 Runetracer for evolution.
Native arena combat proved rank1 speed19.8/lifetime2.25s/damage1; rank8 speed27.72,
three projectiles/lifetime3.25s/damage3. Actual credited kills dropped XP.
Native unlocked A chest with0 keys offered NO FUTURE; virtual known-controller
button confirmed, remaining slots ranked owned inventory; XP/level unchanged.
Bounce explosions hit enemies for3; native armor hit triggered retaliation3.6
with owned Armor rank2. Floor2 attachment retained evolved inventory and did not
grant another Armor rank. Installed duration-dependent evolution interval uses
lifetime plus interval before global cooldown conversion.
Final controlled native test: stationary crystal overlapped two native BulletKin;
each lost3HP across1250ms active lifetime with500ms delay;800ms real-time menu
pause caused no damage; expiry left0 active flights. Sweep geometry regression
covers fast/reverse/vertical hits, parallel misses, stationary overlap and edges.
Build and10 Python tests/design preflight passed. Final source/lab SHA256
cf605bcf434044c5c60e14c95213dcab65155e00cd0a097f6760ede09feed681.
Evidence kept privately: pasqualina-combat-native/unity.log, pasqualina-final-native/
unity.log and pasqualina-runetracer/nofuture screenshots. Main-menu injected
Windows input did not activate Play; diagnostic switched to native menu action,
then native Breach departure. No production input changes made for that fixture.
Save snapshot before-pasqualina/20261011-030216.zip restored and byte-verified;
diagnostic DLLs and commands removed. Game closed, no test state handed off.
No full balance run claimed; physical8BitDo/new saved-run persistence remain to
validate. Supported BepInEx5 private route retained; no publication.

0.2.18: user requested restored menu sensitivity filter and reported brief
screen-wide Runetracer spawn trail. Read README/MODLOG first; native host API
inspection confirmed TrailRenderer.Clear/GetPositions; GungeonActions native
directional action values remain available. JSON updated before implementation.
Restored prior0.7 engage,0.25 release and0.25s menu-local cooldown via existing
MenuNavigation. Both menus require centering before another tilt; keyboard
arrows bypass stick filtering, native confirmation/rumble hooks unchanged.
No global input/aim/walking changes. Native choice ignored virtual0.5 tilt,
full tilt focused1, holding did not repeat; center/fresh tilt focused2 and native
confirm selected Crown. AllInput/timeScale1 restored after choice.
Private forced picker opened then closed before navigation; do not claim a new
native picker navigation pass. Shared filter unit checks passed17 cases.
Trail spawn anchor previously read specRigidbody.UnitCenter, potentially stale
on pooled spawn. New flight starts sweep at actual projectile transform. Trail
configured inactive, then actual post-physics position assigned and Clear called
before activation/rendering. This also avoids a stale initial damage sweep.
Real repeated distant-room spawn test:12 shots,2927 sampled native trail points,
max distance7.630633 units from player, no long streak. Live trail still draws.
Build,10 Python tests and design preflight passed. Fresh backup
before-trail-menu/20261011-033340.zip restored and byte-verified; diagnostics and
command file removed. Final source/lab hash b4e1c3ed45600536a09482d4835f021edc0b4f4618877e9617d7d20c6b7c3be8.
Game closed, no automation pending, no publication. Physical8BitDo testing and
existing full-run content balance validation remain separate.
