using System;
namespace GungeonSurvivors {
public static class Progression {
    public static float BossReward(int floor,float perFloor) { return Math.Max(1,floor)*perFloor; }
    public static float Required(int level,float initial,float linear,float quadratic) {
        float n=Math.Max(0,level-1);
        return Math.Max(1f,initial+linear*n+quadratic*n*n);
    }
    public static float Reward(float health,bool boss,float reference,float minimum,float maximum,float bossReward) {
        float reward=Math.Max(minimum,Math.Min(maximum,(float)Math.Sqrt(Math.Max(0f,health)/Math.Max(1f,reference))));
        return boss?Math.Max(reward,bossReward):reward;
    }
}
}
