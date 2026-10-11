using System;
namespace GungeonSurvivors {
public static class SurvivorTuning {
    public static float Health(float hostNormal,float installedHealth,float installedNormal) {
        return Math.Max(.5f,(float)Math.Floor(hostNormal*installedHealth/installedNormal*2f)/2f);
    }
    public static float PickupRadius(float baseRadius,float magnet) { return baseRadius*Math.Max(0,1+magnet); }
    public static float RegenRate(float installedRegen,float hostNormal,float installedNormal) { return Math.Max(0,installedRegen)*hostNormal/installedNormal; }
    public static float RegenStep(ref float bank,float rate,float delta,float missing,float step,bool combat) {
        if(!combat || missing<=0 || rate<=0) { bank=0; return 0; }
        bank+=rate*Math.Max(0,delta);
        if(bank<step) return 0;
        float heal=Math.Min(missing,(float)Math.Floor(bank/step)*step); bank-=heal;
        if(heal>=missing) bank=0;
        return heal;
    }
}
}
