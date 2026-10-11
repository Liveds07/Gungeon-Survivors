using System.Collections.Generic;
namespace GungeonSurvivors {
public sealed class ChestRewardLedger {
    readonly HashSet<int> rewarded=new HashSet<int>();
    public bool TryOpen(int id,bool previouslyUnavailable,bool opened,bool broken) {
        return !previouslyUnavailable && opened && !broken && rewarded.Add(id);
    }
    public bool TryBreak(int id,bool opened,bool broken) {
        return !opened && broken && rewarded.Add(id);
    }
    public void Clear() { rewarded.Clear(); }
}
}
