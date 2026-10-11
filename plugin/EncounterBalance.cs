using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Dungeonator;
namespace GungeonSurvivors {
public partial class Settings {
    public float wand_damage_multiplier;
    public int enemy_population_multiplier;
    public bool hide_ui_during_native_sequences;
}
public partial class Plugin {
    readonly HashSet<int> populationHandled=new HashSet<int>();
    Dungeon populationDungeon;
    bool spawningPopulation;
    static readonly PropertyInfo NativeIntroProperty=typeof(Pixelator).GetProperty("IsInIntro",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
    static readonly FieldInfo NativeUiHiddenField=typeof(GameUIRoot).GetField("CoreUIHidden",BindingFlags.Instance|BindingFlags.NonPublic);
    bool NativeSequenceActive {
        get {
            if(!GameManager.HasInstance || (content!=null && !content.settings.hide_ui_during_native_sequences)) return false;
            GameManager gm=GameManager.Instance;
            if(gm.IsLoadingLevel || gm.IsPaused || GameManager.IsBossIntro || TextBoxManager.ExtantTextBoxVisible) return true;
            if(Pixelator.Instance!=null && NativeIntroProperty!=null && (bool)NativeIntroProperty.GetValue(Pixelator.Instance,null)) return true;
            if(GameUIRoot.Instance!=null) {
                if(GameUIRoot.Instance.DisplayingConversationBar) return true;
                OverridableBool hidden=NativeUiHiddenField==null?null:NativeUiHiddenField.GetValue(GameUIRoot.Instance) as OverridableBool;
                if(hidden!=null && hidden.Value) return true;
            }
            PlayerController p=gm.PrimaryPlayer;
            return p!=null && !pickerActive && !choosing && !gm.IsFoyer && p.CurrentInputState!=PlayerInputState.AllInput;
        }
    }
    [HarmonyPatch(typeof(RoomHandler),"RegisterEnemy",new Type[] { typeof(AIActor) })]
    class PopulationRegistrationHook {
        [HarmonyPostfix] static void Postfix(AIActor __0) {
            if(activePlugin!=null && activePlugin.spawningPopulation && __0!=null) activePlugin.populationHandled.Add(__0.GetInstanceID());
        }
    }
    void TickEnemyPopulation() {
        if(content.settings.enemy_population_multiplier<2 || !player.IsInCombat) return;
        Dungeon dungeon=GameManager.Instance.Dungeon;
        if(populationDungeon!=dungeon) { populationHandled.Clear(); populationDungeon=dungeon; }
        RoomHandler room=player.CurrentRoom;
        if(room==null || IsSurvivalRoom(room) || room.area==null || room.area.PrototypeRoomCategory!=PrototypeDungeonRoom.RoomCategory.NORMAL) return;
        List<AIActor> enemies=room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
        if(enemies==null) return;
        foreach(AIActor enemy in enemies.ToArray()) {
            if(enemy==null || !enemy.IsNormalEnemy || enemy.IsHarmlessEnemy || enemy.IgnoreForRoomClear || enemy.CompanionOwner!=null || enemy.healthHaver==null || enemy.healthHaver.IsBoss || enemy.healthHaver.IsDead || !enemy.HasBeenEngaged || string.IsNullOrEmpty(enemy.EnemyGuid) || !populationHandled.Add(enemy.GetInstanceID())) continue;
            int before=room.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All);
            spawningPopulation=true;
            try {
                for(int i=1;i<content.settings.enemy_population_multiplier;i++) room.AddSpecificEnemyToRoomProcedurally(enemy.EnemyGuid,true,null);
            } catch(Exception e) { Logger.LogWarning("GS_POPULATION_FAILED enemy="+enemy.EnemyGuid+" reason="+e.Message); }
            finally { spawningPopulation=false; }
            int added=room.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All)-before;
            Logger.LogInfo("GS_POPULATION enemy="+enemy.EnemyGuid+" added="+added+" floor="+GameManager.Instance.CurrentFloor+" room="+room.GetRoomName());
        }
    }
}
}
