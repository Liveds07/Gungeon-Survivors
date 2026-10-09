namespace GungeonSurvivors {
public static class InventoryRules {
    public static bool CanOffer(bool owned,int rank,int maxRank,int count,int cap,bool evolved) {
        return !evolved && (owned?rank<maxRank:count<cap);
    }
    public static bool CanEvolve(int rank,int maxRank,bool evolved,bool hasRequiredAccessory) {
        return rank>=maxRank && !evolved && hasRequiredAccessory;
    }
}
}
