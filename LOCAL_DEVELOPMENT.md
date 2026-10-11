# Local development slice

Use installed copies of both games. This is an unfinished development build,
not a release. Solo only; no co-op support. Version 0.2.18 includes optional five-minute survival arenas, an interactive entrance sign, native cover and floor-scaled chest rewards, HP-preserving Hollow Heart, Antonio starting armor and five-rank Clover/Coolness, Pasqualina/Runetracer/NO FUTURE and five-rank native-shield Armor. See MODLOG.md for current test evidence; older version notes below are historical.

Build with Windows PowerShell:

```powershell
.\tools\build.ps1 -GameFolder 'D:\Epic Games\EnterTheGungeon' -LoaderFolder 'C:\Users\Propietario\modding-lab\GungeonSurvivors'
```

This compiles against the host's own references and BepInEx 5.4.23.5. It writes
only the mod DLL to ignored build/. It does not install into the game folder.
Local paths shown here describe this development machine, not portable defaults.

The test copy is outside the source project at
C:\Users\Propietario\modding-lab\GungeonSurvivors. It shares the player's normal
Gungeon save directory. Back up that directory before each testing session.
MODLOG.md records the initial backups and the restore command.

The plugin requires a local Python with UnityPy and four BepInEx settings under
[Content]: CompanionFolder, PythonExecutable, ToolsFolder and DesignSheets.
ToolsFolder contains tools/read_companion.py; DesignSheets points to
 design/sheets.json. The launcher can override CompanionFolder with the command
line pair --gs-companion <installed folder>. Melty folder-token wiring is still
pending verification; no recipe has been installed or published.

At startup the plugin starts the content reader asynchronously. It reads the
actual installed TextAssets and sprites, regenerating a private cache under
%LOCALAPPDATA%\GungeonSurvivors\content. Failure disables the slice instead of
silently using old cached content. Never package the cache, lab, game assemblies,
extracted sprites, decompiled code or screenshots in the source project.

Version 0.2.8 uses four actual walking frames for each survivor. F6 switches
survivor outside combat before the first kill. Dodge physics stay native; the
survivor sprite performs one full rotation over the native modified roll duration.
Gun and hand graphics are hidden with host renderer overrides;
aimed shooting remains functional.

At the first dungeon entrance, inherited starter guns, passives and active
items are removed and replaced with the same hidden Rusty Sidearm for everyone.
Items and guns found afterward remain usable. Existing mid-run room inventories
are left intact. Starting health is three hearts and starting armor is zero; other non-item
character traits are not all normalized. Neutral weapon identity is selected from the actual host database,
not a guessed pickup ID. The wand uses that stable starter projectile for physics,
with private companion projectile sprites overlaid; pooled native colors are
restored on disable. Whip slash art is scaled to its sheet-defined hit range.
Additional whips alternate sides with the installed weapon's repeat interval
(100 ms in this installed version), rather than hitting both sides at once.

Level costs rise as 8 + 4*(level-1) + .5*(level-1)^2. Credited kills award
sqrt(enemy max health / 15), clamped .5..5 XP; bosses award at least 10. Survivor
and Crown growth bonuses are additive. This is mashup balancing, not VS's XP curve.
The HUD displays an XP bar and six weapon slots above six accessory slots.
The native aimed gun does not occupy a companion weapon slot.

Level-up menus offer up to three seeded choices from available weapon and accessory
acquisitions/ranks. Both inventories have independent six-slot caps. Full slots
still allow upgrades; maxed/evolved entries are excluded. If nothing is eligible,
a half-heart heal is offered. The current weapon pool is Whip, Magic Wand and Garlic.

Six accessories use installed names, icons and additive rank data: Hollow Heart
(maximum health), Empty Tome (automatic cooldown), Spinach (automatic damage),
Candelabrador (automatic area), Duplicator (automatic amount), Crown (XP growth).
Hollow Heart scales the selected survivor starting health with a mod-owned, unsaved,
ownerless native health modifier; it does not replace health bonuses from found
Gungeon items. Automatic-weapon accessories do not buff aimed gun stats. Duplicator
has two installed ranks; the other five currently have five.

A credited boss kill offers one evolution if a rank-eight weapon and its matching
accessory are already owned: Whip + Hollow Heart -> Bloody Tear; Magic Wand +
Empty Tome -> Holy Wand. The accessory need not be maxed. One evolution replaces
its own weapon slot; there is no extra slot and no additional level-up consumed.
Evolution selection waits for native animations to finish and player input to return.
Successful chest opening grants random ranks to owned, non-maxed weapons and accessories: D/C/B
chests grant one, A grants three and S grants five. If an evolution is eligible
when the chest opens, choose one evolution; it consumes one slot and the remaining
slots grant random ranks. Ranks stop when no owned weapon or accessory can improve. Character
level and XP remain unchanged.
There is no ordinary level-up afterward and no dungeon floor transition.
Each qualifying boss kill also offers one evolution. If multiple weapons qualify,
choose one per reward; ineligible opens/kills never bank future evolutions.
Breaking an unopened chest grants exactly 1 XP without growth scaling or evolution,
and suppresses native item drops. Failed opens and repeat interaction grant nothing.
These chest rules are mashup adaptations. Real chest open/break and Bloody Tear
selection passed in-game in 0.2.11; evolved combat/healing still needs validation.
Evolved weapon statistics come from installed tables. Bloody Tear uses installed
critical chance/multiplier with half-heart healing on a damaging critical hit,
at most once per second. Healing scale/cooldown are mashup tuning. The slash uses
the installed slash with a dark main layer and offset red/purple additive ghosts,
matching the inspected native effect structure. Complete VFX parity is not claimed.

Upgrade-menu input uses Gungeon's own navigation and confirm actions, including
controller bindings. A highlighted row shows the current selection. Menu stick input
requires a deliberate tilt and center release before moving again. Confirmation
uses the host menu-interact check and its localized confirm button. Keyboard
Up/Down and Enter, number keys 1/2/3, and mouse buttons also work. Physical-controller confirmation was observed in player-test logs; new three-choice/accessory/evolution menus remain untested in game. Version 0.2.10 adds a pre-run survivor picker; F6 reopens it before combat/first kill.

Weapon rank and XP are retained across loading the next floor within the same
run seed; death, foyer/new-run transitions and pre-run survivor changes reset the
build. Full floor-transition testing and saved mid-run persistence are pending;
progress is currently in memory only. Accessory/evolution gameplay verification, survival branches and the Melty install recipe remain unfinished.

A development-only [Testing] EnableTestCommands=true setting enables F8 to restart
an already active dungeon using the host's QuickRestart API. It clears the active
run save like the game's restart command. Leave this off for ordinary play.
The test command is not permitted during menus, foyer or level loading.

Validation:

```powershell
& 'C:\Users\Propietario\modding-tools\python312\python.exe' -m unittest discover -s tests -v
& 'C:\Users\Propietario\modding-tools\python312\python.exe' tools\preflight.py --scope design
```

The companion safety tests require UnityPy. Existing route tests alone need
only Python. Release preflight must still fail until all cells have in-game
verification. Read MODLOG.md for what has actually been tested.

The Breach lobby uses native character graphics. Survivor skins and HUD activate
when the dungeon run starts. Choose a survivor in the Breach picker, or in the entrance fallback. F6 reopens the picker before combat/first kill.


Wand art uses installed ProjectileHoly1/2, matching the blue four-point star.
Whip collision follows the drawn rectangle and clears hostile regular bullets,
excluding player shots and beams. Player confirmed wand art, whip fairness and
smoother progression. Wand rank-seven penetration remains unconfirmed in game.
Version 0.2.10 passes compilation, private installed-content extraction, Python
checks and inventory/progression/controller/hitbox regression checks. New
accessories, health upgrades, boss evolution rewards, evolved combat and expanded
HUD await real player testing. See NEXT_SESSION.md for the handoff.


0.2.9 real-game validation covers pre-run selection, F6 swaps and native dodge
invulnerability/damage rejection, chest/pickup completion, table sliding/flipping
(including two tables placed adjacent at runtime), and quick-restart starter
cleanup. See MODLOG.md for exact evidence. Automated test saves were restored;
the diagnostic helper is removed. Physical controller picker input remains to
be checked; controller support uses the native menu bindings.



0.2.10 fixes survivor scenery overlap using the host material and native
per-vertex world-depth projection. Actual altar and merchant-counter comparisons
passed; F6 swaps and native roll damage rejection still pass. Poe/Garlic/
Pummarola/Soul Eater are implemented in 0.2.12; endings remain on hold.
See design/next-content.json for the inspected selectors and adaptation plan.


0.2.11 uses compact portrait tiles with full installed character descriptions below.
Mouse hover and keyboard/controller focus update the details. Mouse hover and
keyboard selection passed in-game; physical controller picker testing remains pending.
Endings are explicitly on hold. See the 0.2.12 notes below for the new content.


Current 0.2.12 details (supersede historical status above): Poe starts with Garlic,
two host hearts and +25% XP pickup radius. Antonio starts with 3.5 hearts; Imelda
with three. Installed health is converted to native half-heart units. Enemy XP
now drops as collectible gems; installed magnet and growth affect collection.
Pummarola is the seventh accessory. Regeneration only accrues during combat and
clears its partial bank outside combat or at full health. Rank1 recovers half a
heart after roughly 83.3 combat seconds; rank5 after 16.7, before other effects.
Garlic8 + Pummarola evolves into Soul Eater. Aura area and damage/cooldown use
installed data with sheet-defined host conversions. Soul Eater heals half a heart
with a provisional 10% damaging-hit chance and one-second shared cooldown;
actual healing increases its power, capped at +100%. Knockback/freeze resistance
reduction and full native particle effects remain unimplemented. Bloody Tear's
three layers and additive shader were confirmed instantiated, but final visual
appearance needs another capture because the game exited during that check.
Automated tests restored the latest pre-Poe saves; diagnostics are removed.


Current balance update0.2.13: Magic Wand/Holy Wand damage is25% of0.2.12 per bullet;
Whip/Garlic damage unchanged. Eligible regular-room enemies get one additional
same-prefab native spawn; bosses, special rooms and friendly/harmless actors are
excluded. Native clear-cell failures are logged, so exact doubling is conditional
on available placement. Boss base XP is10 per main-floor number before growth;
unknown side floors use10. Boss gems appear at the player's feet. XP gems are
twice their old width/height; collection radius unchanged. HUD/picker/upgrade UI
hide during native dialogue, boss intro, core-UI suppression, input locks, pause
and loading; deferred upgrade rewards are retained. Full new balance run and
reinforcement/exotic-room coverage remain pending. Challenge rooms await the
user's description. Latest pre-test completed-run saves restored and verified.

Current 0.2.17 additions: Pasqualina reads PASQUALINA from installed CHARACTER_DATA,
including base speed1.1 and +0.1 at levels5/10/15. This affects automatic
projectiles, not aimed starter-gun bullets. Native base health3/armor0 tested.
Runetracer reads DIAMOND, NO FUTURE reads ROCHER, Armor reads ARMOR. Native wall
bounces and swept enemy hitboxes use installed lifetime and500ms per-enemy delay.
A native TrailRenderer draws the fading crystal trail; evolved explosion frames
Screeno_0..29 are read from the installed NO FUTURE addressable bundle into the
private cache. The evolved duration-dependent cooldown includes lifetime plus
interval, then applies Empty Tome. No companion assets are shipped with source.
Armor has five ranks; each new rank grants one native armor shield once per run.
Spending shields never removes the evolution prerequisite and stat refresh does
not refill them. HP and maximumHP are unchanged by Armor.

Native checks: all Pasqualina speed thresholds/cap through20; five real armor
hits with no HP loss/refill; regular and rank-eight bouncing combat with credited
kills/XP drops; A chest NO FUTURE offer and virtual native controller confirmation;
bounce damage and retaliation on an armor hit; floor transition cleanup. A final
controlled native flight hit two enemies three times each at500ms intervals,
paused without damage during the choice UI, then expired and left zero flights.
Build, ten Python checks, design preflight and RuneGeometryRegression passed.
Long full-run balance and a new physical8BitDo test remain pending.
Latest player saves before-pasqualina/20261011-030216.zip restored and byte-verified.
All test DLLs/commands removed. Nothing published; game left closed.

0.2.18 restores the requested menu-local stick filter:0.7 engage,0.25 release,
0.25s cooldown, one selection per tilt, center required on menu opening.
Keyboard arrows remain immediate; confirmation, rumble and gameplay input are
unchanged. Choice and picker use the shared filter. The native level-choice
fixture verified ignored0.5 tilt, one full-tilt step, no held repeat, center/fresh
second step and controller confirmation returning AllInput/timeScale1.
A separate picker fixture opened then closed before navigation; no new native
picker-navigation pass claimed. Physical8BitDo checking remains for the player.
Runetracer starts its hit sweep at the actual spawned transform, not pooled
rigidbody coordinates. Its trail is configured inactive and positioned/cleared
on the first post-physics update before rendering. Native12 distant-room
respawns sampled2927 trail points, max distance7.630633 units from player;
no whole-screen streak.17 menu regression checks and10 Python checks passed.
Latest saves before-trail-menu/20261011-033340.zip restored and byte-verified;
all diagnostic helpers removed; game closed. Installed0.2.18 hash b4e1c3ed45600536a09482d4835f021edc0b4f4618877e9617d7d20c6b7c3be8.
