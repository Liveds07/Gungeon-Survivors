using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BepInEx;
using UnityEngine;
using Dungeonator;

namespace GungeonSurvivors {
[Serializable] public class Level { public int level; public float power, growth, interval, amount, area, penetrating, speed, critChance, critMul; }
[Serializable] public class Survivor { public string id, name, weaponId; public string[] walkFrames; public float power, growth; public Level[] levels; }
[Serializable] public class Weapon { public string id, name, evolutionName, requiredPassive; public string[] effectFrames; public float repeatInterval; public Level[] levels; public Level evolution; }
[Serializable] public class AccessoryLevel { public float power, area, amount, growth, cooldown, maxHp; }
[Serializable] public class Accessory { public string id, dataKey, name, description; public AccessoryLevel[] levels; }
[Serializable] public class Settings { public float damage_scale, xp_per_kill, xp_per_level, pixels_per_unit, projectile_speed, projectile_range, whip_reach, whip_half_height, minimum_attack_interval; public int max_weapon_rank, max_weapon_slots, max_accessory_slots, level_choice_count; public float bloody_tear_heal, bloody_tear_heal_cooldown; public string selection_key, challenge_key, neutral_gun_match; public float walking_fps, effect_fps, projectile_visual_scale, whip_flash_seconds, fallback_heal; public bool hide_gun_graphics, remove_starting_items, selection_before_first_kill, whip_destroy_hostile_projectiles; public float starting_health, starting_armor; public float xp_level_linear, xp_level_quadratic, xp_enemy_health_reference, xp_enemy_min, xp_enemy_max, xp_boss_reward; }
[Serializable] public class UiData { public string id, sprite_selector, choice_policy; public bool freeze_world; public string navigation_actions, confirm_action, keyboard_navigation; public float input_grace_seconds, stick_engage_threshold, stick_release_threshold, navigation_cooldown_seconds; }
[Serializable] public class Content { public UiData ui; public Survivor[] survivors; public Weapon[] weapons; public Accessory[] accessories; public Settings settings; public string source, sourceHash; }

[BepInPlugin("local.gungeon-survivors", "Gungeon Survivors", "0.2.8")]
public class Plugin : BaseUnityPlugin {
    Content content;
    string cache, status = "Reading your Vampire Survivors copy...";
    int highlightedChoice; float choiceInputReady; MenuNavigation menuNavigation;
    int selected, level = 1, kills, shots, pendingChoices, runSeed = int.MinValue;
    class Slot { public Weapon weapon; public int rank = 1; public float timer; public bool evolved; }
    readonly List<Slot> owned = new List<Slot>();
    class AccessorySlot { public Accessory accessory; public int rank=1; }
    class Choice { public Weapon weapon; public Accessory accessory; public bool evolution; }
    readonly List<AccessorySlot> accessories=new List<AccessorySlot>();
    readonly List<Choice> choices=new List<Choice>();
    readonly List<Weapon> bossEvolutions=new List<Weapon>();
    bool evolutionChoice;
    StatModifier healthModifier; PlayerController healthOwner; float lastHeal=-100f;
    bool choosing, wasFoyer, prepared;
    float walkClock, rollClock, rollDuration = 0.5f, rollSign = -1f;
    Vector2 previousCenter;
    Projectile wandTemplate;
    const string OverrideKey = "GungeonSurvivors";
    float xp, power, growth;
    bool ready;
    PlayerController player;
    GameObject skinObject;
    SpriteRenderer skin;
    Renderer originalRenderer;
    Color originalColor;
    Texture2D[] portraits;
    Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    Process reader;
    Survivor Current { get { return content.survivors[selected]; } }
    Weapon CurrentWeapon { get { foreach (Weapon w in content.weapons) if (w.id == Current.weaponId) return w; throw new InvalidDataException("Unknown weapon reference"); } }

    void Awake() { StartCoroutine(ReadCompanion()); }
    IEnumerator ReadCompanion() {
        string companion = Config.Bind<string>("Content", "CompanionFolder", "", "Installed Vampire Survivors folder; supplied by the launcher.").Value;
        string python = Config.Bind<string>("Content", "PythonExecutable", "", "Local Python with UnityPy; no game content ships with the plugin.").Value;
        string tools = Config.Bind<string>("Content", "ToolsFolder", "", "Folder containing read_companion.py.").Value;
        string sheets = Config.Bind<string>("Content", "DesignSheets", "", "JSON source of truth.").Value;
        string[] args = Environment.GetCommandLineArgs();
        for (int i=0; i+1<args.Length; i++) if (args[i] == "--gs-companion") companion = args[i+1];
        cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GungeonSurvivors/content");
        if (!File.Exists(python) || !File.Exists(Path.Combine(tools,"read_companion.py")) || !File.Exists(sheets) || !File.Exists(Path.Combine(companion,"VampireSurvivors.exe"))) {
            status = "Set the local companion and reader paths in BepInEx config."; Logger.LogError(status); yield break;
        }
        try {
            ProcessStartInfo start = new ProcessStartInfo(python, Quote(Path.Combine(tools,"read_companion.py")) + " --companion " + Quote(companion) + " --sheets " + Quote(sheets) + " --output " + Quote(cache));
            start.UseShellExecute = false; start.CreateNoWindow = true;
            reader = Process.Start(start);
        } catch (Exception e) { status = "Companion reader failed: " + e.Message; Logger.LogError(status); yield break; }
        float deadline = Time.realtimeSinceStartup + 90f;
        while (!reader.HasExited && Time.realtimeSinceStartup < deadline) yield return null;
        if (!reader.HasExited) { reader.Kill(); reader.Dispose(); reader=null; status="Companion reader timed out; slice disabled."; Logger.LogError(status); yield break; }
        int exit = reader.ExitCode; reader.Dispose(); reader=null;
        if (exit != 0) { status="Installed companion content could not be read; slice disabled."; Logger.LogError(status); yield break; }
        try {
            FullSerializer.fsData parsed = FullSerializer.fsJsonParser.Parse(File.ReadAllText(Path.Combine(cache,"content.json")));
            new FullSerializer.fsSerializer().TryDeserialize<Content>(parsed,ref content).AssertSuccess();
            if (content == null || content.survivors == null || content.weapons == null || content.accessories == null || content.settings == null) throw new InvalidDataException("Companion parser did not populate required fields");
            if (content.survivors.Length != 2 || content.weapons.Length != 2) throw new InvalidDataException("Expected two sheet-defined survivors and weapons");
            if (Path.GetFullPath(content.source) != Path.GetFullPath(companion)) throw new InvalidDataException("Companion source mismatch");
            if(content.settings.max_weapon_slots!=6 || content.settings.max_accessory_slots!=6 || content.settings.level_choice_count<1) throw new InvalidDataException("Invalid inventory configuration");
            portraits = new Texture2D[content.survivors.Length];
            for (int i=0;i<content.survivors.Length;i++) { Logger.LogInfo("GS_SPRITE_READ id="+content.survivors[i].id); Sprite s=ReadSprite(content.survivors[i].id); if(s==null) throw new InvalidDataException("Unity Sprite.Create returned null"); portraits[i]=s.texture; }
            foreach (Weapon w in content.weapons) { Logger.LogInfo("GS_WEAPON_SPRITE id="+w.id); ReadSprite(w.id); ReadSprite(w.id+"_evolved"); }
            foreach(Accessory a in content.accessories) ReadSprite(a.id);
            foreach(Survivor c in content.survivors) foreach(string id in c.walkFrames) ReadSprite(id);
            foreach(Weapon w in content.weapons) foreach(string id in w.effectFrames) ReadSprite(id);
            ReadSprite("ui_frame");
            ready=true; ResetRun(); status="F6: choose survivor before combat. Automatic companion weapons enabled.";
            Logger.LogInfo("GS_CONTENT_READY sourceHash=" + content.sourceHash + " survivors=2 weapons=2 sprites="+sprites.Count);
        } catch (Exception e) { status="Companion validation failed: " + e.ToString(); Logger.LogError(status); }
    }
    static string Quote(string value) { if (value.IndexOf('"')>=0 || value.IndexOf('\n')>=0 || value.IndexOf('\r')>=0) throw new ArgumentException("Invalid path"); return "\""+value+"\""; }
    Sprite ReadSprite(string id) {
        Texture2D t = new Texture2D(2,2,TextureFormat.RGBA32,false);
        if (!ImageConversion.LoadImage(t,File.ReadAllBytes(Path.Combine(cache,id+".png")))) throw new InvalidDataException("Unreadable sprite");
        t.filterMode=FilterMode.Point;
        Sprite s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(0.5f,0.5f),content.settings.pixels_per_unit);
        s.name=id; sprites[id]=s; return s;
    }

    void ResetRun() {
        CloseChoice(); level=1; xp=0; kills=0; shots=0; pendingChoices=0;
        power=Current.power; growth=Current.growth; owned.Clear(); accessories.Clear(); bossEvolutions.Clear(); lastHeal=-100f; ApplyAccessoryHealth();
        owned.Add(new Slot { weapon=CurrentWeapon }); walkClock=0;
    }
    void CloseChoice() {
        if(!choosing) return;
        choosing=false; BraveTime.ClearMultiplier(gameObject);
        if(player!=null) player.ClearInputOverride(OverrideKey);
    }
    void Detach() {
        CloseChoice();
        if(player!=null) {
            player.OnKilledEnemyContext-=EnemyKilled; player.OnRollStarted-=RollStarted; player.PostProcessProjectile-=AimedProjectile;
            player.ToggleGunRenderers(true,OverrideKey); player.ToggleHandRenderers(true,OverrideKey);
            SpriteOutlineManager.ToggleOutlineRenderers(player.sprite,true); player.sprite.color=originalColor;
        }
        if(originalRenderer!=null) originalRenderer.enabled=true;
        if(skinObject!=null) Destroy(skinObject);
        player=null; skin=null; originalRenderer=null;
    }
    void Attach(PlayerController p) {
        Detach(); player=p;
        GameManager gm=GameManager.Instance;
        if(gm.IsFoyer) wasFoyer=true;
        else if(wasFoyer || runSeed!=gm.CurrentRunSeed) {
            runSeed=gm.CurrentRunSeed; wasFoyer=false; prepared=false; wandTemplate=null; ResetRun();
        }
        player.OnKilledEnemyContext+=EnemyKilled; player.OnRollStarted+=RollStarted; player.PostProcessProjectile+=AimedProjectile;
        skinObject=new GameObject("GungeonSurvivorsSkin"); skin=skinObject.AddComponent<SpriteRenderer>();
        skin.sprite=sprites[Current.walkFrames[0]]; skin.sortingOrder=1; skinObject.layer=p.sprite.gameObject.layer;
        originalRenderer=p.sprite.GetComponent<Renderer>(); originalColor=p.sprite.color; previousCenter=p.CenterPosition;
        ApplyAccessoryHealth();
        Logger.LogInfo("GS_PLAYER_ATTACHED survivor="+Current.id+" level="+level+" weapons="+owned.Count);
    }
    void PrepareLoadout() {
        if(prepared || player.CurrentInputState.ToString()!="AllInput") return;
        // Only replace an initial entrance loadout, never strip a mid-run room inventory.
        if(player.CurrentRoom==null || player.CurrentRoom.GetRoomName()!="Gungeon Entrance") { prepared=true; return; }
        Gun neutral=null;
        foreach(PickupObject item in PickupObjectDatabase.Instance.Objects) {
            Gun gun=item as Gun;
            if(gun!=null && (gun.name+" "+gun.gunName).ToLowerInvariant().Contains(content.settings.neutral_gun_match.ToLowerInvariant())) { neutral=gun; break; }
        }
        if(neutral==null || neutral.DefaultModule==null || neutral.DefaultModule.projectiles.Count==0) {
            prepared=true; Logger.LogError("Neutral starter selector not found; loadout left intact."); return;
        }
        wandTemplate=neutral.DefaultModule.projectiles[0];
        if(content.settings.remove_starting_items) { player.RemoveAllActiveItems(); player.RemoveAllPassiveItems(); }
        player.ForceZeroHealthState=false;
        player.stats.SetBaseStatValue(PlayerStats.StatType.Health,content.settings.starting_health,player);
        player.healthHaver.SetHealthMaximum(content.settings.starting_health,null,false);
        player.healthHaver.ForceSetCurrentHealth(content.settings.starting_health);
        player.healthHaver.Armor=content.settings.starting_armor;
        player.ArmorlessAnimations=true;
        player.inventory.DestroyAllGuns(); player.inventory.AddGunToInventory(neutral,true);
        prepared=true;
        ApplyAccessoryHealth();
        Logger.LogInfo("GS_LOADOUT neutral="+neutral.name+" guns="+player.inventory.AllGuns.Count+" passives="+player.passiveItems.Count+" actives="+player.activeItems.Count+" armor="+player.healthHaver.Armor+" maxHealth="+player.healthHaver.GetMaxHealth());
    }
    void RollStarted(PlayerController p, Vector2 direction) {
        rollClock=0f; rollDuration=Math.Max(0.01f,p.rollStats.GetModifiedTime(p));
        rollSign=direction.x<0f?1f:-1f;
        Logger.LogInfo("GS_HOST_DODGE preserved=true barrelRoll=true duration="+rollDuration);
    }
    float XpRequired { get { return Progression.Required(level,content.settings.xp_per_level,content.settings.xp_level_linear,content.settings.xp_level_quadratic); } }
    void AimedProjectile(Projectile p, float scalar) { Logger.LogInfo("GS_HOST_AIMED_FIRE preserved=true"); }
    void EnemyKilled(PlayerController p, HealthHaver enemy) {
        if(p!=player) return;
        float reward=Progression.Reward(enemy==null?content.settings.xp_enemy_health_reference:enemy.GetMaxHealth(),enemy!=null && enemy.IsBoss,content.settings.xp_enemy_health_reference,content.settings.xp_enemy_min,content.settings.xp_enemy_max,content.settings.xp_boss_reward)*content.settings.xp_per_kill*(growth+AccessoryStats().growth);
        kills++; xp+=reward; Logger.LogInfo("GS_KILL count="+kills+" reward="+reward+" xp="+xp+" required="+XpRequired);
        while(xp>=XpRequired) {
            xp-=XpRequired; level++; pendingChoices++;
            foreach(Level l in Current.levels) if(l.level==level) { power+=l.power; growth+=l.growth; }
            Logger.LogInfo("GS_LEVEL_UP level="+level+" choices="+pendingChoices);
        }
        if(enemy!=null && enemy.IsBoss) {
            foreach(Slot s in owned) if(CanEvolve(s) && !bossEvolutions.Contains(s.weapon)) bossEvolutions.Add(s.weapon);
            Logger.LogInfo("GS_BOSS_EVOLUTION ready="+bossEvolutions.Count);
        }
    }
    Slot FindSlot(Weapon w) { return owned.Find(delegate(Slot s) { return s.weapon.id==w.id; }); }
    AccessorySlot FindAccessory(Accessory a) { return accessories.Find(delegate(AccessorySlot s) { return s.accessory.id==a.id; }); }
    bool CanEvolve(Slot s) {
        bool required=accessories.Exists(delegate(AccessorySlot a) { return a.accessory.dataKey==s.weapon.requiredPassive; });
        return InventoryRules.CanEvolve(s.rank,Math.Min(content.settings.max_weapon_rank,s.weapon.levels.Length),s.evolved,required);
    }
    AccessoryLevel AccessoryStats() {
        AccessoryLevel result=new AccessoryLevel();
        foreach(AccessorySlot s in accessories) for(int i=0;i<s.rank && i<s.accessory.levels.Length;i++) {
            AccessoryLevel r=s.accessory.levels[i]; result.power+=r.power; result.area+=r.area;
            result.amount+=r.amount; result.growth+=r.growth; result.cooldown+=r.cooldown; result.maxHp+=r.maxHp;
        }
        return result;
    }
    void RemoveAccessoryHealth() {
        if(healthOwner!=null && healthModifier!=null) { healthOwner.ownerlessStatModifiers.Remove(healthModifier); healthOwner.stats.RecalculateStats(healthOwner,false,false); }
        healthOwner=null; healthModifier=null;
    }
    void ApplyAccessoryHealth() {
        if(player==null || content==null) return;
        if(healthOwner!=player) { RemoveAccessoryHealth(); healthOwner=player; }
        float amount=content.settings.starting_health*AccessoryStats().maxHp;
        if(healthModifier==null) {
            if(amount==0) return;
            healthModifier=new StatModifier { statToBoost=PlayerStats.StatType.Health,modifyType=StatModifier.ModifyMethod.ADDITIVE,amount=amount,ignoredForSaveData=true };
            player.ownerlessStatModifiers.Add(healthModifier);
        } else { if(Math.Abs(healthModifier.amount-amount)<0.0001f) return; healthModifier.amount=amount; }
        player.stats.RecalculateStats(player,false,false);
    }
    void BeginChoice() {
        choices.Clear(); evolutionChoice=bossEvolutions.Count>0;
        if(evolutionChoice) {
            foreach(Weapon w in bossEvolutions) { Slot s=FindSlot(w); if(s!=null && CanEvolve(s)) choices.Add(new Choice { weapon=w,evolution=true }); }
            if(choices.Count==0) { bossEvolutions.Clear(); evolutionChoice=false; }
        }
        if(!evolutionChoice) {
            foreach(Weapon w in content.weapons) {
                Slot s=FindSlot(w);
                if(InventoryRules.CanOffer(s!=null,s==null?0:s.rank,Math.Min(content.settings.max_weapon_rank,w.levels.Length),owned.Count,content.settings.max_weapon_slots,s!=null && s.evolved)) choices.Add(new Choice { weapon=w });
            }
            foreach(Accessory a in content.accessories) {
                AccessorySlot s=FindAccessory(a);
                if(InventoryRules.CanOffer(s!=null,s==null?0:s.rank,a.levels.Length,accessories.Count,content.settings.max_accessory_slots,false)) choices.Add(new Choice { accessory=a });
            }
            System.Random rng=new System.Random(unchecked(runSeed+level*7919+pendingChoices*31));
            for(int i=choices.Count-1;i>0;i--) { int j=rng.Next(i+1); Choice swap=choices[i]; choices[i]=choices[j]; choices[j]=swap; }
            if(choices.Count>content.settings.level_choice_count) choices.RemoveRange(content.settings.level_choice_count,choices.Count-content.settings.level_choice_count);
        }
        highlightedChoice=0; choiceInputReady=Time.realtimeSinceStartup+content.ui.input_grace_seconds; menuNavigation=new MenuNavigation(content.ui.stick_engage_threshold,content.ui.stick_release_threshold,content.ui.navigation_cooldown_seconds); menuNavigation.Reset(choiceInputReady); choosing=true; player.SetInputOverride(OverrideKey);
        if(content.ui.freeze_world) BraveTime.SetTimeScaleMultiplier(0f,gameObject);
        Logger.LogInfo("GS_CHOICE_OPEN choices="+choices.Count+" evolution="+evolutionChoice);
    }
    void Choose(int index) {
        if(!choosing || index<0 || index>=Math.Max(1,choices.Count)) return;
        if(choices.Count==0) { player.healthHaver.ApplyHealing(content.settings.fallback_heal); Logger.LogInfo("GS_CHOICE healing"); }
        else {
            Choice c=choices[index];
            if(c.accessory!=null) {
                AccessorySlot s=FindAccessory(c.accessory);
                if(s==null) { if(accessories.Count>=content.settings.max_accessory_slots) return; s=new AccessorySlot { accessory=c.accessory }; accessories.Add(s); } else if(s.rank<s.accessory.levels.Length) s.rank++; else return;
                ApplyAccessoryHealth(); Logger.LogInfo("GS_ACCESSORY id="+s.accessory.id+" rank="+s.rank+" owned="+accessories.Count);
            } else {
                Slot s=FindSlot(c.weapon);
                if(c.evolution) {
                    if(s==null || !CanEvolve(s)) return;
                    s.evolved=true; s.timer=0; Logger.LogInfo("GS_EVOLUTION weapon="+c.weapon.id+" into="+c.weapon.evolutionName);
                } else {
                    if(s==null) { if(owned.Count>=content.settings.max_weapon_slots) return; s=new Slot { weapon=c.weapon }; owned.Add(s); }
                    else if(!s.evolved && s.rank<Math.Min(content.settings.max_weapon_rank,s.weapon.levels.Length)) s.rank++; else return;
                    Logger.LogInfo("GS_CHOICE weapon="+c.weapon.id+" rank="+s.rank+" owned="+owned.Count);
                }
            }
        }
        if(evolutionChoice) bossEvolutions.Clear(); else pendingChoices--;
        CloseChoice();
    }
    void Update() {
        if(!ready || !GameManager.HasInstance) return;
        GameManager gm=GameManager.Instance;
        if(Input.GetKeyDown(KeyCode.F8) && Config.Bind<bool>("Testing","EnableTestCommands",false,"Allow fresh test runs only after saving backups.").Value && !gm.IsLoadingLevel && gm.PrimaryPlayer!=null && !gm.IsFoyer) { gm.QuickRestart(new QuickRestartOptions()); return; }
        if(gm.IsLoadingLevel) { if(player!=null) Detach(); return; }
        if(gm.IsFoyer) { wasFoyer=true; if(player!=null) Detach(); return; }
        PlayerController p=gm.PrimaryPlayer;
        if(p==null || p.healthHaver==null || p.healthHaver.IsDead) { if(p!=null && p.healthHaver!=null && p.healthHaver.IsDead) runSeed=int.MinValue; if(player!=null) Detach(); return; }
        if(gm.SecondaryPlayer!=null) { Detach(); return; }
        if(player!=p) Attach(p);
        if(gm.IsFoyer) wasFoyer=true;
        if(Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode),content.settings.selection_key)) && !choosing && !p.IsInCombat && (!content.settings.selection_before_first_kill || kills==0)) {
            selected=(selected+1)%content.survivors.Length; ResetRun(); skin.sprite=sprites[Current.walkFrames[0]];
            Logger.LogInfo("GS_SELECTION survivor="+Current.id);
        }
        if(!gm.IsFoyer) PrepareLoadout();
        if(choosing) {
            if(Time.realtimeSinceStartup<choiceInputReady) return;
            BraveInput input=BraveInput.GetInstanceForPlayer(player.PlayerIDX);
            GungeonActions actions=input.ActiveActions;
            if(actions==null) return;
            int count=Math.Max(1,choices.Count);
            float menuAxis=actions.SelectUp.Value-actions.SelectDown.Value;
            float moveAxis=actions.Up.Value-actions.Down.Value;
            float axis=Math.Abs(menuAxis)>=Math.Abs(moveAxis)?menuAxis:moveAxis;
            int step=menuNavigation.Step(axis,Time.realtimeSinceStartup);
            if(Input.GetKeyDown(KeyCode.UpArrow)) step=-1;
            else if(Input.GetKeyDown(KeyCode.DownArrow)) step=1;
            if(step!=0) {
                highlightedChoice=(highlightedChoice+step+count)%count;
                Logger.LogInfo("GS_CHOICE_FOCUS index="+highlightedChoice);
            }
            InControl.InputDevice device=actions.ActiveDevice ?? InControl.InputManager.ActiveDevice;
            bool deviceConfirm=device!=null && device.GetControl(GungeonActions.LocalizedMenuSelectAction).WasPressed;
            bool confirm=Input.GetKeyDown(KeyCode.Return) || input.MenuInteractPressed || deviceConfirm;
            if(Input.GetKeyDown(KeyCode.Alpha1)) Choose(0);
            else if(Input.GetKeyDown(KeyCode.Alpha2)) Choose(1);
            else if(Input.GetKeyDown(KeyCode.Alpha3)) Choose(2);
            else if(confirm) { Logger.LogInfo("GS_CHOICE_CONFIRM bound="+input.MenuInteractPressed+" device="+deviceConfirm); Choose(highlightedChoice); }
            return;
        }
        if(gm.IsPaused || gm.IsFoyer || p.CurrentRoom==null) return;
        if(pendingChoices>0 || bossEvolutions.Count>0) { if(p.CurrentInputState.ToString()=="AllInput") BeginChoice(); return; }
        foreach(Slot slot in owned) slot.timer-=BraveTime.DeltaTime;
        List<AIActor> enemies=p.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
        if(enemies==null || enemies.Count==0) return;
        foreach(Slot slot in owned.ToArray()) {
            if(slot.timer>0) continue;
            Level stats=WeaponStats(slot);
            slot.timer=Math.Max(content.settings.minimum_attack_interval,stats.interval/1000f);
            if(slot.weapon.id=="whip") StartCoroutine(WhipBurst(enemies,stats,slot.weapon.repeatInterval,slot.evolved)); else Wand(enemies,stats,slot);
        }
    }
    Level RankStats(Weapon weapon,int rank) {
        Level result=new Level();
        for(int i=0;i<rank && i<weapon.levels.Length;i++) { Level row=weapon.levels[i]; result.power+=row.power; result.interval+=row.interval; result.amount+=row.amount; result.area+=row.area; result.penetrating+=row.penetrating; result.speed+=row.speed; }
        return result;
    }
    Level WeaponStats(Slot slot) {
        Level b=slot.evolved?slot.weapon.evolution:RankStats(slot.weapon,slot.rank);
        AccessoryLevel a=AccessoryStats();
        return new Level { power=b.power*(1+a.power),area=b.area*(1+a.area),amount=b.amount+a.amount,
            interval=b.interval*Math.Max(0.1f,1+a.cooldown),penetrating=b.penetrating,speed=b.speed>0?b.speed:1,
            critChance=b.critChance,critMul=b.critMul };
    }
    IEnumerator WhipBurst(List<AIActor> enemies,Level stats,float repeatInterval,bool evolved) {
        RoomHandler room=player.CurrentRoom;
        float firstDirection=player.sprite.FlipX?-1f:1f;
        int count=Math.Max(1,(int)stats.amount);
        for(int index=0;index<count;index++) {
            if(player==null || player.healthHaver.IsDead || choosing || player.CurrentRoom!=room) yield break;
            Vector2 center=player.CenterPosition;
            float direction=index%2==0?firstDirection:-firstDirection;
            StartCoroutine(WhipStrike(center,direction,stats,room,index+1,count,evolved));
            if(index+1<count) yield return new WaitForSeconds(repeatInterval);
        }
    }
    IEnumerator WhipStrike(Vector2 center,float dir,Level stats,RoomHandler room,int strike,int count,bool evolved) {
        HitRectangle box=HitRectangle.Whip(center.x,center.y,dir,content.settings.whip_reach*stats.area,content.settings.whip_half_height*stats.area);
        GameObject go=new GameObject("GSWhipSlash"); SpriteRenderer r=go.AddComponent<SpriteRenderer>(); r.sprite=sprites["whip_effect"];
        go.transform.position=new Vector3((box.MinX+box.MaxX)/2,(box.MinY+box.MaxY)/2,player==null?0:player.transform.position.z-0.2f);
        go.transform.localScale=new Vector3((box.MaxX-box.MinX)/r.sprite.bounds.size.x,(box.MaxY-box.MinY)/r.sprite.bounds.size.y,1);
        r.flipX=dir<0; r.sortingOrder=3; if(evolved) r.color=new Color(1f,0.3f,0.4f,1f);
        Vector3 pivotOffset=r.sprite.bounds.center; Vector3 scale=go.transform.localScale;
        go.transform.position-=new Vector3((r.flipX?-pivotOffset.x:pivotOffset.x)*scale.x,pivotOffset.y*scale.y,0);
        HashSet<AIActor> damaged=new HashSet<AIActor>(); int cleared=0;
        float end=Time.time+content.settings.whip_flash_seconds;
        while(Time.time<end && player!=null && !player.healthHaver.IsDead && player.CurrentRoom==room) {
            if(!choosing && !GameManager.Instance.IsPaused) {
                if(content.settings.whip_destroy_hostile_projectiles) {
                    foreach(Projectile bullet in new List<Projectile>(StaticReferenceManager.AllProjectiles)) {
                        if(bullet==null || !bullet.collidesWithPlayer || bullet.Owner is PlayerController || bullet.specRigidbody==null) continue;
                        PixelCollider bulletCollider=bullet.specRigidbody.PrimaryPixelCollider;
                        Vector2 lower=bulletCollider==null?bullet.specRigidbody.UnitBottomLeft:bulletCollider.UnitBottomLeft;
                        Vector2 upper=bulletCollider==null?bullet.specRigidbody.UnitTopRight:bulletCollider.UnitTopRight;
                        if(!box.Overlaps(lower.x,lower.y,upper.x,upper.y)) continue;
                        bullet.DieInAir(true,false,false,true); cleared++;
                    }
                }
                List<AIActor> enemies=room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
                if(enemies!=null) foreach(AIActor enemy in enemies.ToArray()) {
                    if(enemy==null || enemy.healthHaver==null || enemy.healthHaver.IsDead || damaged.Contains(enemy)) continue;
                    Vector2 lower,upper;
                    PixelCollider collider=enemy.specRigidbody==null?null:enemy.specRigidbody.HitboxPixelCollider;
                    if(collider!=null) { lower=collider.UnitBottomLeft; upper=collider.UnitTopRight; }
                    else if(enemy.specRigidbody!=null) { lower=enemy.specRigidbody.UnitBottomLeft; upper=enemy.specRigidbody.UnitTopRight; }
                    else { lower=upper=enemy.CenterPosition; }
                    if(!box.Overlaps(lower.x,lower.y,upper.x,upper.y)) continue;
                    damaged.Add(enemy);
                    bool critical=evolved && UnityEngine.Random.value<stats.critChance;
                    float damage=stats.power*power*content.settings.damage_scale*(critical?Math.Max(1,stats.critMul):1);
                    float healthBefore=enemy.healthHaver.GetCurrentHealth();
                    enemy.healthHaver.ApplyDamage(damage,new Vector2(dir,0),"Gungeon Survivors Whip",CoreDamageTypes.None,DamageCategory.Normal,false,null,false);
                    if(critical && enemy.healthHaver.GetCurrentHealth()<healthBefore && Time.time-lastHeal>=content.settings.bloody_tear_heal_cooldown) {
                        player.healthHaver.ApplyHealing(content.settings.bloody_tear_heal); lastHeal=Time.time;
                    }
                    player.OnDidDamage(damage,enemy.healthHaver.IsDead,enemy.healthHaver);
                }
            }
            r.color=new Color(1,evolved?0.3f:1,evolved?0.4f:1,Mathf.Clamp01((end-Time.time)/content.settings.whip_flash_seconds)); yield return null;
        }
        Destroy(go); shots++;
        if(damaged.Count>0 || cleared>0 || shots<=6 || count>1) Logger.LogInfo("GS_WHIP attacks="+shots+" targets="+damaged.Count+" strike="+strike+"/"+count+" cleared="+cleared);
    }
    void Wand(List<AIActor> enemies,Level stats,Slot slot) {
        AIActor target=null; float distance=float.MaxValue;
        foreach(AIActor enemy in enemies) {
            if(enemy==null || enemy.healthHaver==null || enemy.healthHaver.IsDead) continue;
            float d=(enemy.CenterPosition-player.CenterPosition).sqrMagnitude;
            if(d<distance) { target=enemy; distance=d; }
        }
        if(target==null || wandTemplate==null) return;
        StartCoroutine(WandBurst(target,stats,slot));
    }
    IEnumerator WandBurst(AIActor target,Level stats,Slot slot) {
        for(int i=0;i<Math.Max(1,(int)stats.amount);i++) {
            if(player==null || target==null || target.healthHaver.IsDead || choosing) yield break;
            Vector2 direction=(target.CenterPosition-player.CenterPosition).normalized;
            float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
            GameObject go=SpawnManager.SpawnProjectile(wandTemplate.gameObject,new Vector3(player.CenterPosition.x,player.CenterPosition.y,0),Quaternion.Euler(0,0,angle),true);
            if(go!=null) {
                Projectile projectile=go.GetComponent<Projectile>(); projectile.Owner=player; projectile.Shooter=player.specRigidbody;
                projectile.collidesWithPlayer=false; projectile.collidesWithEnemies=true;
                projectile.baseData=new ProjectileData { damage=stats.power*power*content.settings.damage_scale,speed=content.settings.projectile_speed*stats.speed,range=content.settings.projectile_range,force=0f };
                projectile.RuntimeUpdateScale(stats.area);
                int extraPenetration=Math.Max(0,(int)stats.penetrating-1);
                PierceProjModifier pierce=go.GetComponent<PierceProjModifier>();
                if(pierce==null && extraPenetration>0) pierce=go.AddComponent<PierceProjModifier>();
                if(pierce!=null) pierce.penetration=extraPenetration;
                projectile.UpdateSpeed(); projectile.UpdateCollisionMask(); projectile.SendInDirection(direction,true,true);
                CompanionProjectileVisual visual=go.AddComponent<CompanionProjectileVisual>();
                visual.Initialize(projectile,new Sprite[] { sprites["wand_effect_1"],sprites["wand_effect_2"] },content.settings.effect_fps,content.settings.projectile_visual_scale);
                shots++; if(shots<=3 || shots%20==0) Logger.LogInfo("GS_WAND projectile="+shots+" rank="+slot.rank+" damage="+projectile.baseData.damage+" extraPenetration="+extraPenetration+" evolved="+slot.evolved);
            }
            yield return new WaitForSeconds(slot.weapon.repeatInterval);
        }
    }
    void LateUpdate() {
        if(player==null || skin==null) return;
        Vector2 center=player.CenterPosition;
        bool moving=(center-previousCenter).sqrMagnitude>0.000001f;
        if(moving && !choosing && !GameManager.Instance.IsPaused) walkClock+=BraveTime.DeltaTime; else if(!moving) walkClock=0;
        skin.sprite=sprites[Current.walkFrames[((int)(walkClock*content.settings.walking_fps))%Current.walkFrames.Length]];
        skin.transform.position=new Vector3(center.x,center.y,player.transform.position.z-0.1f); skin.flipX=player.sprite.FlipX;
        if(player.IsDodgeRolling) {
            if(!choosing && !GameManager.Instance.IsPaused) rollClock+=BraveTime.DeltaTime;
            skin.sprite=sprites[Current.walkFrames[0]];
            skin.transform.rotation=Quaternion.Euler(0,0,rollSign*360f*Mathf.Clamp01(rollClock/rollDuration));
        } else { rollClock=0; skin.transform.rotation=Quaternion.identity; }
        skin.color=Color.white; previousCenter=center;
        if(originalRenderer!=null) originalRenderer.enabled=true; player.sprite.color=new Color(1,1,1,0);
        SpriteOutlineManager.ToggleOutlineRenderers(player.sprite,false);
        if(content.settings.hide_gun_graphics) { player.ToggleGunRenderers(false,OverrideKey); player.ToggleHandRenderers(false,OverrideKey); }
    }
    void Frame(Rect rect) {
        Texture2D tex=sprites["ui_frame"].texture;
        float border=12f; float uv=0.25f;
        for(int y=0;y<3;y++) for(int x=0;x<3;x++) {
            float dx=x==0?rect.x:x==1?rect.x+border:rect.xMax-border;
            float dy=y==0?rect.y:y==1?rect.y+border:rect.yMax-border;
            float dw=x==1?rect.width-2*border:border, dh=y==1?rect.height-2*border:border;
            GUI.DrawTextureWithTexCoords(new Rect(dx,dy,dw,dh),tex,new Rect(x==0?0:x==1?uv:1-uv,y==0?1-uv:y==1?uv:0,x==1?1-2*uv:uv,y==1?1-2*uv:uv),true);
        }
    }
    void OnGUI() {
        if(!ready) { GUI.Box(new Rect(300,12,450,65),status); return; }
        if(player==null) return;
        GUIStyle label=new GUIStyle(GUI.skin.label); label.fontSize=16; label.normal.textColor=Color.white;
        float width=450, left=(Screen.width-width)/2;
        Rect hud=new Rect(left,12,width,166); Frame(hud);
        GUI.DrawTexture(new Rect(left+14,24,42,42),portraits[selected],ScaleMode.ScaleToFit,true);
        GUI.Label(new Rect(left+65,22,370,25),Current.name+"   Level "+level+"   XP "+xp.ToString("0.0")+" / "+XpRequired.ToString("0.0"),label);
        GUIStyle small=new GUIStyle(label); small.fontSize=11; small.alignment=TextAnchor.LowerRight;
        for(int row=0;row<2;row++) {
            GUI.Label(new Rect(left+14,59+row*45,82,25),row==0?"Weapons":"Accessories",GUI.skin.label);
            for(int i=0;i<6;i++) {
                Rect cell=new Rect(left+100+i*54,53+row*45,46,40); Frame(cell);
                string id=null,rank="";
                if(row==0 && i<owned.Count) { Slot s=owned[i]; id=s.weapon.id+(s.evolved?"_evolved":""); rank=s.evolved?"E":""+s.rank; }
                if(row==1 && i<accessories.Count) { AccessorySlot s=accessories[i]; id=s.accessory.id; rank=""+s.rank; }
                if(id!=null) { GUI.DrawTexture(new Rect(cell.x+5,cell.y+4,30,30),sprites[id].texture,ScaleMode.ScaleToFit,true); GUI.Label(new Rect(cell.x+25,cell.y+22,18,17),rank,small); }
            }
        }
        Rect bar=new Rect(left+15,145,width-30,8);
        GUI.color=new Color(0.12f,0.12f,0.2f); GUI.DrawTexture(bar,Texture2D.whiteTexture);
        GUI.color=new Color(0.2f,0.65f,1f); GUI.DrawTexture(new Rect(bar.x,bar.y,bar.width*Mathf.Clamp01(xp/XpRequired),bar.height),Texture2D.whiteTexture); GUI.color=Color.white;
        if(kills==0 && !choosing) GUI.Label(new Rect(left+15,178,width-30,22),"F6: select survivor before the first kill",GUI.skin.label);
        if(!choosing) return;
        int count=Math.Max(1,choices.Count); float panelWidth=620, height=100+count*100;
        float px=(Screen.width-panelWidth)/2, py=(Screen.height-height)/2;
        GUI.color=new Color(0,0,0,0.75f); GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture); GUI.color=Color.white;
        Frame(new Rect(px,py,panelWidth,height));
        GUIStyle title=new GUIStyle(label); title.fontSize=26; title.alignment=TextAnchor.MiddleCenter; title.normal.textColor=new Color(1,0.85f,0.3f);
        GUI.Label(new Rect(px+20,py+15,panelWidth-40,45),(evolutionChoice?"BOSS REWARD!  Choose an evolution":"LEVEL UP!  Choose an upgrade"),title);
        for(int i=0;i<count;i++) {
            string text; Texture2D icon=portraits[selected];
            if(choices.Count==0) text="Recover half a heart";
            else {
                Choice c=choices[i];
                if(c.accessory!=null) {
                    AccessorySlot s=FindAccessory(c.accessory); icon=sprites[c.accessory.id].texture;
                    text=(s==null?"NEW: ":"UPGRADE: ")+c.accessory.name+(s==null?"  Rank 1":"  Rank "+s.rank+" -> "+(s.rank+1))+"\n"+c.accessory.description;
                } else {
                    Slot s=FindSlot(c.weapon); icon=sprites[c.weapon.id+(c.evolution?"_evolved":"")].texture;
                    text=c.evolution?"EVOLVE: "+c.weapon.name+" -> "+c.weapon.evolutionName:
                        (s==null?"NEW: ":"UPGRADE: ")+c.weapon.name+(s==null?"  Rank 1":"  Rank "+s.rank+" -> "+(s.rank+1));
                }
            }
            Rect row=new Rect(px+24,py+75+i*100,panelWidth-48,82); Frame(row);
            if(i==highlightedChoice) {
                GUI.color=new Color(1f,0.85f,0.3f);
                GUI.DrawTexture(new Rect(row.x,row.y,row.width,2),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(row.x,row.yMax-2,row.width,2),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(row.x,row.y,2,row.height),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(row.xMax-2,row.y,2,row.height),Texture2D.whiteTexture);
                GUI.color=Color.white;
            }
            GUI.DrawTexture(new Rect(row.x+16,row.y+15,48,48),icon,ScaleMode.ScaleToFit,true);
            GUI.Label(new Rect(row.x+80,row.y+5,row.width-95,43),(i+1)+". "+text,label);
            if(GUI.Button(new Rect(row.x+80,row.y+48,row.width-95,24),"Choose")) { Choose(i); break; }
        }
        GUI.Label(new Rect(px+25,py+height-28,panelWidth-50,24),"Up / Down: navigate     Confirm / Enter: choose     1 / 2 / 3 or click: direct choice",GUI.skin.label);
    }
    void OnDestroy() { Detach(); RemoveAccessoryHealth(); if(reader!=null) { if(!reader.HasExited) reader.Kill(); reader.Dispose(); } foreach(Sprite s in sprites.Values) { Destroy(s.texture); Destroy(s); } }
}
public class CompanionProjectileVisual : MonoBehaviour {
    Projectile projectile; SpriteRenderer visual; Sprite[] frames; float fps; Color originalColor;
    public void Initialize(Projectile p,Sprite[] sequence,float rate,float scale) {
        projectile=p; frames=sequence; fps=rate; originalColor=p.sprite==null?Color.white:p.sprite.color;
        GameObject go=new GameObject("GSCompanionProjectile"); go.layer=p.gameObject.layer; go.transform.SetParent(transform,false);
        go.transform.localPosition=new Vector3(0,0,-0.1f); go.transform.localScale=new Vector3(scale,scale,1);
        visual=go.AddComponent<SpriteRenderer>(); visual.sprite=frames[0]; visual.sortingOrder=2;
    }
    void LateUpdate() {
        if(projectile==null || visual==null) return;
        if(projectile.sprite!=null) { projectile.sprite.color=new Color(1,1,1,0); SpriteOutlineManager.ToggleOutlineRenderers(projectile.sprite,false); }
        visual.sprite=frames[((int)(Time.time*fps))%frames.Length];
        Vector2 direction=projectile.Direction;
        if(direction.sqrMagnitude>0.000001f) visual.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
    }
    void OnDisable() { PierceProjModifier pierce=GetComponent<PierceProjModifier>(); if(pierce!=null) pierce.penetration=0; if(projectile!=null && projectile.sprite!=null) { projectile.sprite.color=originalColor; SpriteOutlineManager.ToggleOutlineRenderers(projectile.sprite,true); } if(visual!=null) Destroy(visual.gameObject); Destroy(this); }
}
}


