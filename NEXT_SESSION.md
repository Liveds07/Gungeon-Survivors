# Next session: Gungeon Survivors

Read README.md and MODLOG.md first, then this file, LOCAL_DEVELOPMENT.md and
JSON design sheets. Continue in English. Never publish yet or commit/redistribute
installed game content. Use player's installed VS; private extraction only.
Use universal-modder and supported BepInEx 5 route. Melty install wiring remains
unverified. No autonomous game inputs: player currently controls testing. Launch
only on request. Verify closed before replacing DLL. Back up relevant saves first.

Current build: 0.2.8, installed in private lab:
C:\Users\Propietario\modding-lab\GungeonSurvivors\EtG.exe
Project: C:\Users\Propietario\Gungeon-Survivors-main
ETG original: D:\Epic Games\EnterTheGungeon (2.1.9a / Mono).
VS: D:\Steam\steamapps\common\Vampire Survivors (build 25016043 / IL2CPP).
Python: C:\Users\Propietario\modding-tools\python312\python.exe
Toolkit: C:\Users\Propietario\modding-tools\universal-modder
Current private content cache: %LOCALAPPDATA%\GungeonSurvivors\content.
All original installations remain untouched.

Player confirmed prior whip collision fairness, blue wand art, controller level-up
confirmation and much smoother progression after 0.2.7. Wand extra penetration
starts at WEAPON RANK 7, not survivor level; not yet confirmed by player.
Intermittent 8BitDo device switching stopped; user requested ignoring it.

0.2.8 implemented and compiled:
- Six automatic-weapon slots plus six accessory slots; aimed gun outside cap.
- Still two weapons only. Six installed-data accessories: Hollow Heart, Empty Tome,
  Spinach, Candelabrador, Duplicator, Crown; two Duplicator ranks, others five.
- Three seeded level-up offers, controller/Enter/1/2/3/mouse.
- Two inventory icon rows, ranks, evolved icon and XP bar.
- Max weapon + owned required accessory, then credited boss kill: one evolution.
  Whip + Hollow Heart -> Bloody Tear; Wand + Empty Tome -> Holy Wand.
  Replaces same slot; any accessory rank; if both ready choose one.
- Installed evolution stats, Bloody Tear critical chance/damage and adapted heal.
  Half-heart healing on damaging crit, at most once/second; tinted slash VFX.
- Prior rising XP curve and health-weighted kill rewards, native-duration barrel roll.

Validation: installed extraction, compilation, nine Python tests, design preflight,
14 inventory checks, progression checks, 17 menu navigation and 10 hitbox checks.
New gameplay remains unverified: do not describe these checks as in-game passes.
The player closed the game for install; no game launched afterward by agent.

Next real player checks:
1. Level-up three choices render and confirm on 8BitDo without oversensitivity.
2. Acquire each accessory; effects/ranks appear in HUD; Hollow Heart raises max
   health without erasing found host-item bonuses; across-floor health persists.
3. Weapon rank-seven wand penetrates a second enemy and low-rank/aimed shots do
   not inherit pooled pierce; no damage/scale regressions from pooled projectiles.
4. Max weapon + partner before killing boss offers evolution after cutscene;
   only chosen weapon replaces same slot, no extra ordinary rank choice consumed.
5. Bloody Tear heals on damaging criticals and Holy Wand fires faster; inspect logs
   GS_ACCESSORY, GS_BOSS_EVOLUTION, GS_EVOLUTION and GS_WAND extraPenetration.
6. Full-inventory real tests require more weapons; current six-slot cap checked
   in pure regression only. Mid-run save persistence, co-op, branches, evolved
   VFX parity, portable launcher/Melty installation remain pending.

Latest backup: %USERPROFILE%\.universal-modder\backups\
gungeon-survivors-before-accessories-evolutions\20261010-004809.zip.
Do not restore old saves over user progress unless explicitly needed/authorized.

Resume saved conversation in PowerShell with `codex resume --last` from
C:\Users\Propietario, or `codex resume --all` and select the Gungeon conversation.
The installed `codex resume --help` confirms both commands. If using a new chat,
ask it to read these project files and continue from this checkpoint.

IMPORTANT BUG FIXES TO FIX AFTER SOLO TEST RUNS:
-Dodgeroll doesn't properly add iframes after swapping character with f6
-When trying to roll on several tables at once, the character breaks and gets stuck looking one way without performing the slide through the tables. Flipping tables might also cause this direction.lock bug
-Sometimes gets softlocked after opening a treasure chest
-Gungeon-specific starting gear isn't properly deleted after a quick restart