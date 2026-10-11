using System;
using System.Collections;
using HarmonyLib;
namespace GungeonSurvivors {
[Serializable] public class ChestRewardDesign { public int D,C,B,A,S; }
public partial class Plugin {
    string ChestTier(Chest chest) {
        LootData loot=chest.lootTable;
        if(loot==null) return "D";
        float highest=loot.D_Chance; string tier="D";
        if(loot.C_Chance>highest) {highest=loot.C_Chance;tier="C";}
        if(loot.B_Chance>highest) {highest=loot.B_Chance;tier="B";}
        if(loot.A_Chance>highest) {highest=loot.A_Chance;tier="A";}
        if(loot.S_Chance>highest) tier="S";
        return tier;
    }
    int ChestRanks(string tier) {
        ChestRewardDesign r=content.chestRewards;
        return tier=="S"?r.S:tier=="A"?r.A:tier=="B"?r.B:tier=="C"?r.C:r.D;
    }
    bool ChestRunActive {
        get { return ready && player!=null && player.healthHaver!=null && !player.healthHaver.IsDead && GameManager.HasInstance && !GameManager.Instance.IsFoyer && !GameManager.Instance.IsLoadingLevel && GameManager.Instance.SecondaryPlayer==null; }
    }
    [HarmonyPatch(typeof(Chest),"Open",new Type[] { typeof(PlayerController) })]
    class ChestOpenHook {
        [HarmonyPrefix] static void Prefix(Chest __instance,ref bool __state) { __state=__instance.IsOpen || __instance.IsBroken; }
        [HarmonyPostfix] static void Postfix(Chest __instance,PlayerController __0,bool __state) {
            if(activePlugin!=null) activePlugin.ChestOpened(__instance,__0,__state);
        }
    }
    void ChestOpened(Chest chest,PlayerController opener,bool previouslyUnavailable) {
        if(!ChestRunActive || opener!=player || chest==null || !chestRewards.TryOpen(chest.GetInstanceID(),previouslyUnavailable,chest.IsOpen,chest.IsBroken)) return;
        string tier=ChestTier(chest); QueueChestWeaponReward(ChestRanks(tier));
        Logger.LogInfo("GS_CHEST_TIER tier="+tier+" slots="+ChestRanks(tier));
        Logger.LogInfo("GS_CHEST_OPEN id="+chest.GetInstanceID()+" level="+level+" xp="+xp+" characterLevelUnchanged=true weaponRewards="+evolutionRewards.Count);
    }
    void ChestBroken(PlayerController owner,Chest chest) {
        if(!ChestRunActive || owner!=player || chest==null || !chestRewards.TryBreak(chest.GetInstanceID(),chest.IsOpen,chest.IsBroken)) return;
        xp+=1f; ResolveXpLevels();
        Logger.LogInfo("GS_CHEST_BREAK id="+chest.GetInstanceID()+" xpReward=1 xp="+xp+" level="+level+" evolutionReward=false");
    }
    // Retain native break animation, occupancy cleanup and event; suppress only loot.
    [HarmonyPatch(typeof(Chest),"PresentItem",new Type[] {})]
    class BrokenChestLootHook {
        [HarmonyPrefix] static bool Prefix(Chest __instance,ref IEnumerator __result) {
            if(activePlugin==null || !activePlugin.ChestRunActive || !__instance.IsBroken || __instance.IsOpen) return true;
            __result=EmptyPresentation();
            activePlugin.Logger.LogInfo("GS_CHEST_BREAK_LOOT_SUPPRESSED id="+__instance.GetInstanceID());
            return false;
        }
        static IEnumerator EmptyPresentation() { yield break; }
    }
}
}
