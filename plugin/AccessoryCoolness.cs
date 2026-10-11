using System;
namespace GungeonSurvivors {
public partial class Plugin {
    StatModifier coolnessModifier;
    PlayerController coolnessOwner;
    void RemoveAccessoryCoolness() {
        if(coolnessOwner!=null && coolnessModifier!=null) {
            float health=coolnessOwner.healthHaver.GetCurrentHealth(); preservingAccessoryHealth=true;
            coolnessOwner.ownerlessStatModifiers.Remove(coolnessModifier);
            try {
                coolnessOwner.stats.RecalculateStats(coolnessOwner,false,false);
                coolnessOwner.healthHaver.ForceSetCurrentHealth(health);
            } finally { preservingAccessoryHealth=false; }
        }
        coolnessOwner=null; coolnessModifier=null;
    }
    void ApplyAccessoryCoolness() {
        if(coolnessOwner!=player) { RemoveAccessoryCoolness(); coolnessOwner=player; }
        float amount=AccessoryStats().coolness;
        if(coolnessModifier==null) {
            if(amount==0) return;
            coolnessModifier=new StatModifier { statToBoost=PlayerStats.StatType.Coolness, modifyType=StatModifier.ModifyMethod.ADDITIVE, amount=amount, ignoredForSaveData=true };
            player.ownerlessStatModifiers.Add(coolnessModifier);
        } else {
            if(Math.Abs(coolnessModifier.amount-amount)<.0001f) return;
            coolnessModifier.amount=amount;
        }
        float health=player.healthHaver.GetCurrentHealth(); preservingAccessoryHealth=true;
        try {
            player.stats.RecalculateStats(player,false,false);
            player.healthHaver.ForceSetCurrentHealth(health);
        } finally { preservingAccessoryHealth=false; observedHealth=player.healthHaver.GetCurrentHealth(); observedMaximum=player.healthHaver.GetMaxHealth(); }
        Logger.LogInfo("GS_CLOVER bonus="+amount+" coolness="+player.stats.GetStatValue(PlayerStats.StatType.Coolness));
    }
}
}
