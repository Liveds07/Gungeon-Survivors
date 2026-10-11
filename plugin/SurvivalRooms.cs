using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Dungeonator;
namespace GungeonSurvivors {
[Serializable] public class CoverRect { public int x,y,width,height; }
[Serializable] public class CoverPoint { public int x,y; }
[Serializable] public class SurvivalDesign {
    public bool enabled;
    public float duration_seconds,replacement_delay_seconds;
    public int maximum_enemies,arena_width,arena_height,corridor_width,corridor_height,minimum_per_floor,doorway_cells;
    public string sign_text,sign_rules_text,fallback_enemy_guid;
    public string[] reward_tiers;
    public int[] enemy_limits; public float[] reward_s_chances; public int hearts_per_side;
    public CoverRect[] wall_rectangles,column_rectangles;
    public CoverPoint[] table_positions;
}
public partial class Plugin {
    const string ArenaName="GS Challenge Arena",HallName="GS Challenge Corridor";
    static readonly FieldInfo AssignedFlow=typeof(LoopDungeonGenerator).GetField("m_assignedFlow",BindingFlags.Instance|BindingFlags.NonPublic);
    static readonly MethodInfo ShallowClone=typeof(object).GetMethod("MemberwiseClone",BindingFlags.Instance|BindingFlags.NonPublic);
    PrototypeDungeonRoom arenaPrototype,hallPrototype;
    Dungeon survivalDungeon;
    RoomHandler survivalRoom,survivalHall;
    SurvivalClock survivalClock;
    readonly Dictionary<int,AIActor> survivalActors=new Dictionary<int,AIActor>();
    readonly List<AIActor> survivalOffspring=new List<AIActor>();
    string survivalRewardTier;
    readonly List<string> survivalPool=new List<string>();
    readonly List<UnityEngine.Object> survivalFlowObjects=new List<UnityEngine.Object>();
    System.Random survivalRandom;
    bool spawningSurvival,survivalRewardGiven; float nextSurvivalRewardAttempt;
    int survivalLootLogged;
    GameObject challengeSign;
    bool IsSurvivalRoom(RoomHandler room) { return room!=null && (room==survivalRoom || room.GetRoomName()==ArenaName); }
    bool SurvivalRunning { get { return survivalClock!=null && survivalClock.Running; } }
    [HarmonyPatch(typeof(LoopDungeonGenerator),MethodType.Constructor,new Type[] { typeof(Dungeon),typeof(int) })]
    class SurvivalFlowHook {
        [HarmonyPostfix] static void Postfix(LoopDungeonGenerator __instance,Dungeon __0) { if(activePlugin!=null) activePlugin.InjectSurvivalFlow(__instance,__0); }
    }
    [HarmonyPatch(typeof(SemioticDungeonGenSettings),"GetRandomFlow")]
    class SurvivalRetryFlowHook {
        [HarmonyPostfix] static void Postfix(ref DungeonFlow __result) { if(activePlugin!=null) __result=activePlugin.AddSurvivalFlow(__result); }
    }
    [HarmonyPatch]
    class SurvivalExitHook {
        static IEnumerable<MethodBase> TargetMethods() {
            foreach(MethodInfo method in typeof(LoopBuilderComposite).GetMethods(BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
                if(method.Name=="GetExitPairsSimple" || method.Name=="GetExitPairsForNode" || method.Name=="GetExitPairsPreferDistance" || method.Name=="GetNumberOfIdealExitPairs") yield return method;
        }
        static PrototypeDungeonRoom Hall(BuilderFlowNode first,BuilderFlowNode second) {
            if(first.assignedPrototypeRoom!=null && first.assignedPrototypeRoom.name==HallName) return first.assignedPrototypeRoom;
            if(second.assignedPrototypeRoom!=null && second.assignedPrototypeRoom.name==HallName) return second.assignedPrototypeRoom;
            return null;
        }
        [HarmonyPrefix] static void Prefix(BuilderFlowNode __0,BuilderFlowNode __1,ref List<PrototypeRoomExit> __state) {
            PrototypeDungeonRoom hall=Hall(__0,__1); if(hall==null) return;
            PrototypeDungeonRoom other=(__0.assignedPrototypeRoom==hall?__1:__0).assignedPrototypeRoom; if(other==null) return;
            bool arena=other.name==ArenaName; __state=hall.exitData.exits;
            // Filter before native opposite-exit preference discards bent pairs.
            hall.exitData.exits=__state.FindAll(delegate(PrototypeRoomExit exit) { return (exit.exitDirection==DungeonData.Direction.NORTH)==arena; });
        }
        [HarmonyPostfix] static void Postfix(BuilderFlowNode __0,BuilderFlowNode __1,List<PrototypeRoomExit> __state) { if(__state!=null) Hall(__0,__1).exitData.exits=__state; }
        [HarmonyFinalizer] static void Finalizer(BuilderFlowNode __0,BuilderFlowNode __1,List<PrototypeRoomExit> __state) { if(__state!=null) Hall(__0,__1).exitData.exits=__state; }
    }
    void InjectSurvivalFlow(LoopDungeonGenerator generator,Dungeon dungeon) {
        if(dungeon==null || dungeon.name.ToLowerInvariant().Contains("foyer")) return;
        AssignedFlow.SetValue(generator,AddSurvivalFlow(AssignedFlow.GetValue(generator) as DungeonFlow));
    }
    DungeonFlow AddSurvivalFlow(DungeonFlow original) {
        if(!ready || content.survival==null || !content.survival.enabled || original==null || string.Equals(original.name,"Foyer Flow",StringComparison.OrdinalIgnoreCase)) return original;
        foreach(DungeonFlowNode n in original.AllNodes) if(n.overrideExactRoom!=null && n.overrideExactRoom.name==ArenaName) return original;
        try {
            DungeonFlow copy=Instantiate(original);
            List<DungeonFlowNode> nodes=new List<DungeonFlowNode>();
            foreach(DungeonFlowNode source in original.AllNodes) {
                DungeonFlowNode node=(DungeonFlowNode)ShallowClone.Invoke(source,null);
                node.flow=copy; node.childNodeGuids=new List<string>(source.childNodeGuids); nodes.Add(node);
            }
            typeof(DungeonFlow).GetField("m_nodes",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(copy,nodes);
            List<string> guids=new List<string>(); foreach(DungeonFlowNode n in nodes) guids.Add(n.guidAsString);
            typeof(DungeonFlow).GetField("m_nodeGuids",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(copy,guids);
            DungeonFlowNode parent=null,continuation=null;
            foreach(DungeonFlowNode candidate in nodes) {
                if(candidate.nodeType!=DungeonFlowNode.ControlNodeType.ROOM || candidate.isSubchainStandin) continue;
                foreach(string childGuid in candidate.childNodeGuids) {
                    DungeonFlowNode child=nodes.Find(delegate(DungeonFlowNode n) {return n.guidAsString==childGuid;});
                    if(child!=null && child.nodeType==DungeonFlowNode.ControlNodeType.ROOM && !child.isSubchainStandin) {parent=candidate;continuation=child;break;}
                }
                if(parent!=null && parent.roomCategory==PrototypeDungeonRoom.RoomCategory.NORMAL) break;
            }
            if(parent==null) { Destroy(copy); Logger.LogError("GS_SURVIVAL_FLOW_NO_EDGE flow="+original.name); return original; }
            if(arenaPrototype==null) BuildSurvivalPrototypes();
            DungeonFlowNode hall=BranchNode(copy,hallPrototype),arena=BranchNode(copy,arenaPrototype);
            int before=original.AllNodes.Count;
            // Insert the three-exit hall along one existing edge, preserving the
            // parent room's exit demand and every original room and loop link.
            parent.childNodeGuids.Remove(continuation.guidAsString);
            copy.AddNodeToFlow(hall,parent); copy.AddNodeToFlow(arena,hall);
            hall.childNodeGuids.Add(continuation.guidAsString); continuation.parentNodeGuid=hall.guidAsString;
            hall.minChildrenToBuild=hall.maxChildrenToBuild=2;
            survivalFlowObjects.Add(copy);
            Logger.LogInfo("GS_SURVIVAL_FLOW flow="+original.name+" originalNodes="+before+" unchanged="+(original.AllNodes.Count==before)+" branchNodes="+copy.AllNodes.Count);
            return copy;
        } catch(Exception e) { Logger.LogError("GS_SURVIVAL_FLOW_FAILED "+e); return original; }
    }
    DungeonFlowNode BranchNode(DungeonFlow flow,PrototypeDungeonRoom room) {
        return new DungeonFlowNode(flow) { nodeType=DungeonFlowNode.ControlNodeType.ROOM,roomCategory=room.category,overrideExactRoom=room,percentChance=1,priority=DungeonFlowNode.NodePriority.MANDATORY,minChildrenToBuild=room==hallPrototype?1:0,maxChildrenToBuild=room==hallPrototype?1:0 };
    }
    PrototypeDungeonRoom EmptyPrototype(string name,int width,int height,PrototypeDungeonRoom.RoomCategory category) {
        PrototypeDungeonRoom room=ScriptableObject.CreateInstance<PrototypeDungeonRoom>(); room.name=name;
        typeof(PrototypeDungeonRoom).GetField("m_width",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(room,width);
        typeof(PrototypeDungeonRoom).GetField("m_height",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(room,height);
        typeof(PrototypeDungeonRoom).GetMethod("InitializeArray",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(room,new object[] { width,height }); room.category=category; room.PreventMirroring=true;
        room.GUID=Guid.NewGuid().ToString(); room.QAID=name; room.injectionFlags=new RuntimeInjectionFlags(); room.overrideRoomVisualType=-1;
        room.exitData=new PrototypeRoomExitData { exits=new List<PrototypeRoomExit>() };
        room.placedObjects=new List<PrototypePlacedObjectData>(); room.placedObjectPositions=new List<Vector2>();
        room.additionalObjectLayers=new List<PrototypeRoomObjectLayer>(); room.runtimeAdditionalObjectLayers=new List<PrototypeRoomObjectLayer>();
        room.pits=new List<PrototypeRoomPitEntry>(); room.paths=new List<SerializedPath>(); room.roomEvents=new List<RoomEventDefinition>();
        room.eventTriggerAreas=new List<PrototypeEventTriggerArea>(); room.prerequisites=new List<DungeonPrerequisite>();
        room.excludedOtherRooms=new List<PrototypeDungeonRoom>(); room.rectangularFeatures=new List<PrototypeRectangularFeature>();
        room.usesProceduralLighting=true; room.usesProceduralDecoration=false; room.allowFloorDecoration=false; room.allowWallDecoration=false;
        foreach(PrototypeDungeonRoomCellData cell in room.FullCellData) cell.appearance=new PrototypeDungeonRoomCellAppearance();
        UnityEngine.Random.State savedRandom=UnityEngine.Random.state;
        room.UpdatePrecalculatedData(); UnityEngine.Random.state=savedRandom;
        return room;
    }
    void BuildSurvivalPrototypes() {
        SurvivalDesign s=content.survival;
        hallPrototype=EmptyPrototype(HallName,s.corridor_width,s.corridor_height,PrototypeDungeonRoom.RoomCategory.HUB);
        arenaPrototype=EmptyPrototype(ArenaName,s.arena_width,s.arena_height,PrototypeDungeonRoom.RoomCategory.NORMAL);
        AddCoverCells(s.wall_rectangles); AddCoverCells(s.column_rectangles);
        UnityEngine.Random.State savedRandom=UnityEngine.Random.state;
        arenaPrototype.UpdatePrecalculatedData(); UnityEngine.Random.state=savedRandom;
        for(int i=0;i<s.doorway_cells;i++) {
            hallPrototype.exitData.AddExitPosition(-1,s.corridor_height/2-1+i,hallPrototype);
            hallPrototype.exitData.AddExitPosition(s.corridor_width,s.corridor_height/2-1+i,hallPrototype);
            hallPrototype.exitData.AddExitPosition(s.corridor_width/2-1+i,s.corridor_height,hallPrototype);
            arenaPrototype.exitData.AddExitPosition(s.arena_width/2-1+i,-1,arenaPrototype);
        }
    }
    void AddCoverCells(CoverRect[] blocks) {
        if(blocks==null) return;
        foreach(CoverRect block in blocks) for(int y=block.y;y<block.y+block.height;y++) for(int x=block.x;x<block.x+block.width;x++) {
            if(x<2 || y<6 || x>=arenaPrototype.Width-2 || y>=arenaPrototype.Height-2) throw new InvalidOperationException("Cover overlaps arena boundary/entry");
            arenaPrototype.GetCellDataAtPoint(x,y).state=CellType.WALL;
        }
    }
    void PlaceSurvivalTables() {
        FlippableCover prefab=null;
        foreach(FlippableCover candidate in Resources.FindObjectsOfTypeAll<FlippableCover>()) {
            if(candidate==null || candidate.gameObject.scene.IsValid() || !candidate.name.ToLowerInvariant().Contains("table")) continue;
            prefab=candidate; break;
        }
        if(prefab==null) { Logger.LogWarning("GS_SURVIVAL_TABLE_PREFAB_MISSING"); return; }
        int placed=0;
        foreach(CoverPoint point in content.survival.table_positions) {
            GameObject obj=DungeonPlaceableUtility.InstantiateDungeonPlaceable(prefab.gameObject,survivalRoom,new IntVector2(point.x,point.y),false,AIActor.AwakenAnimationType.Default,false);
            if(obj==null) continue;
            obj.transform.SetParent(survivalDungeon.transform,true);
            FlippableCover table=obj.GetComponent<FlippableCover>();
            if(table!=null) survivalRoom.RegisterInteractable(table);
            placed++;
        }
        Logger.LogInfo("GS_SURVIVAL_COVER walls="+content.survival.wall_rectangles.Length+" columns="+content.survival.column_rectangles.Length+" tables="+placed+" prefab="+prefab.name);
    }
    int SurvivalFloorIndex { get { return Math.Max(0,Math.Min(4,GameManager.Instance.CurrentFloor-1)); } }
    int SurvivalLimit { get { return content.survival.enemy_limits[SurvivalFloorIndex]; } }
    string SurvivalTier { get { return survivalRewardTier ?? content.survival.reward_tiers[SurvivalFloorIndex]; } }
    Chest SurvivalChestPrefab {
        get { RewardManager r=GameManager.Instance.RewardManager; string t=SurvivalTier; return t=="S"?r.S_Chest:t=="A"?r.A_Chest:t=="B"?r.B_Chest:t=="C"?r.C_Chest:r.D_Chest; }
    }
    void EnsureSurvivalFloor() {
        Dungeon dungeon=GameManager.Instance.Dungeon;
        if(dungeon==null || dungeon.data==null || survivalDungeon==dungeon || content.survival==null || !content.survival.enabled) return;
        CancelSurvival(); if(challengeSign!=null) Destroy(challengeSign);
        survivalDungeon=dungeon; survivalRoom=survivalHall=null; survivalRewardGiven=false; nextSurvivalRewardAttempt=0; survivalPool.Clear(); survivalLootLogged=0; survivalRewardTier=null;
        foreach(RoomHandler room in dungeon.data.rooms) {
            if(room.GetRoomName()==ArenaName) survivalRoom=room;
            if(room.GetRoomName()==HallName) survivalHall=room;
            List<AIActor> enemies=room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if(enemies==null) continue;
            foreach(AIActor enemy in enemies) if(enemy!=null && enemy.IsNormalEnemy && !enemy.IsHarmlessEnemy && !enemy.IgnoreForRoomClear && enemy.CompanionOwner==null && enemy.healthHaver!=null && !enemy.healthHaver.IsBoss && !enemy.IsMimicEnemy && !enemy.SetsFlagOnDeath && !enemy.SetsCharacterSpecificFlagOnDeath && !string.IsNullOrEmpty(enemy.EnemyGuid) && !survivalPool.Contains(enemy.EnemyGuid)) survivalPool.Add(enemy.EnemyGuid);
        }
        if(survivalRoom==null || survivalHall==null) { Logger.LogError("GS_SURVIVAL_MISSING floor="+GameManager.Instance.CurrentFloor); return; }
        survivalRoom.PreventStandardRoomReward=true; survivalHall.PreventStandardRoomReward=true;
        survivalClock=new SurvivalClock(content.survival.duration_seconds,SurvivalLimit,content.survival.replacement_delay_seconds);
        if(survivalPool.Count==0) survivalPool.Add(content.survival.fallback_enemy_guid);
        survivalRandom=new System.Random(unchecked(runSeed*397+GameManager.Instance.CurrentFloor*41));
        PlaceSurvivalTables();
        Vector2 position=survivalHall.area.basePosition.ToVector2()+new Vector2(content.survival.corridor_width/2+3,content.survival.corridor_height-1.5f);
        challengeSign=new GameObject("GS Challenge Room Sign"); challengeSign.transform.SetParent(dungeon.transform,false);
        challengeSign.transform.position=new Vector3(position.x,position.y,-1);
        ChallengeSign sign=challengeSign.AddComponent<ChallengeSign>(); sign.room=survivalHall;
        sign.rules=content.survival.sign_rules_text;
        sign.Build(content.survival.sign_text); survivalHall.RegisterInteractable(sign);
        Logger.LogInfo("GS_SURVIVAL_READY floor="+GameManager.Instance.CurrentFloor+" pool="+survivalPool.Count+" hall="+survivalHall.area.UnitCenter+" arena="+survivalRoom.area.UnitCenter+" duration="+content.survival.duration_seconds);
    }
    void TickSurvival() {
        EnsureSurvivalFloor();
        if(survivalClock==null || survivalRoom==null) return;
        if(player.CurrentRoom==survivalRoom && !survivalClock.Started && player.CenterPosition.y>=survivalRoom.area.basePosition.y+5) {
            survivalClock.Begin(); survivalRoom.SealRoom();
            Logger.LogInfo("GS_SURVIVAL_BEGIN duration="+content.survival.duration_seconds+" cap="+SurvivalLimit);
        }
        if(survivalClock.Completed && !survivalRewardGiven) { if(Time.realtimeSinceStartup>=nextSurvivalRewardAttempt) CompleteSurvival(); return; }
        if(!survivalClock.Running) return;
        if(player.CurrentRoom!=survivalRoom) { CancelSurvival(); return; }
        survivalClock.Advance(BraveTime.DeltaTime);
        if(survivalClock.Completed) { CompleteSurvival(); return; }
        foreach(int id in new List<int>(survivalActors.Keys)) {
            AIActor enemy=survivalActors[id];
            if(enemy==null || enemy.healthHaver==null || enemy.healthHaver.IsDead) {
                survivalActors.Remove(id); survivalClock.Defeated(id);
                Logger.LogInfo("GS_SURVIVAL_DEFEAT time="+survivalClock.Elapsed+" replacementAt="+(survivalClock.Elapsed+content.survival.replacement_delay_seconds));
            }
        }
        if(!survivalRoom.IsSealed) survivalRoom.SealRoom();
        survivalOffspring.RemoveAll(delegate(AIActor child) { return child==null || child.healthHaver==null || child.healthHaver.IsDead; });
        while(survivalClock.ReserveSpawn()) {
            int before=survivalActors.Count; spawningSurvival=true;
            try { survivalRoom.AddSpecificEnemyToRoomProcedurally(survivalPool[survivalRandom.Next(survivalPool.Count)],true,null); }
            catch(Exception e) { Logger.LogWarning("GS_SURVIVAL_SPAWN_FAILED "+e.Message); }
            finally { spawningSurvival=false; }
            if(survivalActors.Count==before) survivalClock.FailedSpawn();
        }
    }
    [HarmonyPatch(typeof(RoomHandler),"RegisterEnemy",new Type[] { typeof(AIActor) })]
    class SurvivalRegisterHook {
        [HarmonyPrefix] static bool Prefix(RoomHandler __instance,AIActor __0) {
            Plugin mod=activePlugin;
            if(mod==null || !mod.SurvivalRunning || __instance!=mod.survivalRoom || __0==null || __0.CompanionOwner!=null || __0.IsHarmlessEnemy) return true;
            int id=__0.GetInstanceID();
            if(mod.survivalActors.ContainsKey(id) || mod.survivalOffspring.Contains(__0)) return true;
            bool scheduled=mod.spawningSurvival && mod.survivalClock.Register(id);
            __0.CanDropCurrency=false; __0.AssignedCurrencyToDrop=0; __0.AdditionalSingleCoinDropChance=0; __0.CanDropItems=false; __0.CustomLootTable=null; __0.CustomChestTable=null; __0.ChanceToDropCustomChest=0;
            if(scheduled) mod.survivalActors.Add(id,__0);
            else mod.survivalOffspring.Add(__0);
            mod.Logger.LogInfo((scheduled?"GS_SURVIVAL_SPAWN":"GS_SURVIVAL_OFFSPRING_EXEMPT")+" time="+mod.survivalClock.Elapsed+" scheduledAlive="+mod.survivalClock.Alive+" offspring="+mod.survivalOffspring.Count+" guid="+__0.EnemyGuid+" currency=false");
            return true;
        }
    }
    [HarmonyPatch(typeof(AIActor),"HandleLootPinata",new Type[] { typeof(int) })]
    class SurvivalLootHook {
        [HarmonyPrefix] static bool Prefix(AIActor __instance) {
            Plugin mod=activePlugin;
            if(mod==null || !mod.IsSurvivalRoom(__instance.ParentRoom)) return true;
            // Native bonus-coin and loot effects can bypass CanDropCurrency.
            // Credited-kill XP is awarded by the separate player event.
            if(mod.survivalLootLogged++<3) mod.Logger.LogInfo("GS_SURVIVAL_LOOT_BLOCKED guid="+__instance.EnemyGuid+" XPEventPreserved=true");
            return false;
        }
    }
    [HarmonyPatch(typeof(RoomHandler),"UnsealRoom",new Type[] {})]
    class SurvivalUnsealHook {
        [HarmonyPrefix] static bool Prefix(RoomHandler __instance) { return activePlugin==null || !activePlugin.SurvivalRunning || __instance!=activePlugin.survivalRoom; }
    }
    [HarmonyPatch(typeof(RoomHandler),"HandleRoomClearReward",new Type[] {})]
    class SurvivalClearRewardHook {
        [HarmonyPrefix] static bool Prefix(RoomHandler __instance) { return activePlugin==null || !activePlugin.IsSurvivalRoom(__instance); }
    }
    void CleanupSurvivalEnemies() {
        foreach(Projectile bullet in new List<Projectile>(StaticReferenceManager.AllProjectiles)) if(bullet!=null && bullet.Owner is AIActor && ((AIActor)bullet.Owner)!=null && ((AIActor)bullet.Owner).ParentRoom==survivalRoom) bullet.DieInAir(true,false,false,true);
        foreach(AIActor actor in new List<AIActor>(survivalActors.Values)) if(actor!=null) actor.EraseFromExistence(true);
        foreach(AIActor child in survivalOffspring) if(child!=null) child.EraseFromExistence(true); survivalOffspring.Clear();
        survivalActors.Clear();
    }
    void CancelSurvival() {
        if(survivalClock!=null && survivalClock.Running) { survivalClock.Cancel(); CleanupSurvivalEnemies(); if(survivalRoom!=null && survivalDungeon!=null && survivalDungeon==GameManager.Instance.Dungeon) survivalRoom.UnsealRoom(); Logger.LogInfo("GS_SURVIVAL_ABORT noReward=true"); }
    }
    void CompleteSurvival() {
        CleanupSurvivalEnemies(); survivalClock.Cancel(); survivalRoom.UnsealRoom();
        if(survivalRewardGiven) return;
        IntVector2 position=survivalRoom.GetBestRewardLocation(new IntVector2(3,3),RoomHandler.RewardLocationStyle.CameraCenter,true);
        if(survivalRewardTier==null) survivalRewardTier=survivalRandom.NextDouble()<content.survival.reward_s_chances[SurvivalFloorIndex]?"S":"A";
        Chest chest=Chest.Spawn(SurvivalChestPrefab,position,survivalRoom,true);
        if(chest==null) { nextSurvivalRewardAttempt=Time.realtimeSinceStartup+1; Logger.LogError("GS_SURVIVAL_CHEST_FAILED retry=true"); return; }
        chest.PreventFuse=true; chest.ForceUnlock(); survivalRewardGiven=true;
        GameObject heart=PickupObjectDatabase.GetById(GameManager.Instance.RewardManager.FullHeartIdPrefab).gameObject;
        for(int side=-1;side<=1;side+=2) for(int i=0;i<content.survival.hearts_per_side;i++)
            LootEngine.SpawnItem(heart,position.ToVector2()+new Vector2(side*(2.5f+i*1.25f),.5f),Vector2.zero,0f,false,true,false);
        Logger.LogInfo("GS_SURVIVAL_COMPLETE elapsed="+survivalClock.Elapsed+" quality="+SurvivalTier+" locked="+chest.IsLocked+" rewardCount=1");
    }
    void DestroySurvivalResources() {
        if(challengeSign!=null) Destroy(challengeSign);
        foreach(UnityEngine.Object obj in survivalFlowObjects) if(obj!=null) Destroy(obj); survivalFlowObjects.Clear();
        if(arenaPrototype!=null) Destroy(arenaPrototype); if(hallPrototype!=null) Destroy(hallPrototype);
    }
    void DrawSurvivalStatus() {
        if(!SurvivalRunning || player==null || player.CurrentRoom!=survivalRoom) return;
        int left=(int)Math.Ceiling(content.survival.duration_seconds-survivalClock.Elapsed);
        string label="SURVIVAL  "+(left/60).ToString("00")+":"+(left%60).ToString("00")+"    Enemies: "+survivalClock.Alive+" / "+SurvivalLimit;
        Rect plate=new Rect((Screen.width-640)/2,Screen.height-76,640,42);
        Color savedColor=GUI.color; GUI.color=new Color(.02f,.02f,.06f,.85f); GUI.DrawTexture(plate,Texture2D.whiteTexture); GUI.color=savedColor;
        GUIStyle style=new GUIStyle(GUI.skin.label); style.alignment=TextAnchor.MiddleCenter; style.fontSize=survivalClock.Started?18:13; style.normal.textColor=Color.white;
        GUI.Label(plate,label,style);
    }
}
}

