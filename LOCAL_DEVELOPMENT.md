# Local development slice

Use installed copies of both games. This is an unfinished development build,
not a release. Solo only; no co-op support or survival branches yet. Version 0.2.8 adds accessory and evolution prototypes awaiting gameplay verification.

Build with Windows PowerShell:

```powershell
.\tools\build.ps1 -GameFolder 'D:\Epic Games\EnterTheGungeon' -LoaderFolder 'C:\Users\Propietario\modding-tools\bepinex5'
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
a half-heart heal is offered. The current weapon pool remains Whip and Magic Wand.

Six accessories use installed names, icons and additive rank data: Hollow Heart
(maximum health), Empty Tome (automatic cooldown), Spinach (automatic damage),
Candelabrador (automatic area), Duplicator (automatic amount), Crown (XP growth).
Hollow Heart scales the three-heart starting health with a mod-owned, unsaved,
ownerless native health modifier; it does not replace health bonuses from found
Gungeon items. Automatic-weapon accessories do not buff aimed gun stats. Duplicator
has two installed ranks; the other five currently have five.

A credited boss kill offers one evolution if a rank-eight weapon and its matching
accessory are already owned: Whip + Hollow Heart -> Bloody Tear; Magic Wand +
Empty Tome -> Holy Wand. The accessory need not be maxed. One evolution replaces
its own weapon slot; there is no extra slot and no additional level-up consumed.
Boss evolution selection waits for native player input to be restored after a
cutscene. This boss reward is a mashup adaptation, not native VS treasure timing.
If both are eligible, choose one; another qualifying boss kill is needed for the
other. Bosses defeated before eligibility do not bank evolution rewards.
Evolved weapon statistics come from installed tables. Bloody Tear uses installed
critical chance/multiplier with half-heart healing on a damaging critical hit,
at most once per second. Healing scale/cooldown are mashup tuning. The slash uses
the existing installed art tinted red; evolved VFX are not fully replicated.

Upgrade-menu input uses Gungeon's own navigation and confirm actions, including
controller bindings. A highlighted row shows the current selection. Menu stick input
requires a deliberate tilt and center release before moving again. Confirmation
uses the host menu-interact check and its localized confirm button. Keyboard
Up/Down and Enter, number keys 1/2/3, and mouse buttons also work. Physical-controller confirmation was observed in player-test logs; new three-choice/accessory/evolution menus remain untested in game. Survivor selection itself still uses F6.

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
when the dungeon run starts; use F6 there before earning the first kill.


Wand art uses installed ProjectileHoly1/2, matching the blue four-point star.
Whip collision follows the drawn rectangle and clears hostile regular bullets,
excluding player shots and beams. Player confirmed wand art, whip fairness and
smoother progression. Wand rank-seven penetration remains unconfirmed in game.
Version 0.2.8 passes compilation, private installed-content extraction, Python
checks and inventory/progression/controller/hitbox regression checks. New
accessories, health upgrades, boss evolution rewards, evolved combat and expanded
HUD await real player testing. See NEXT_SESSION.md for the handoff.
