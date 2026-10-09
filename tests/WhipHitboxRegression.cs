using System;
using GungeonSurvivors;
class WhipHitboxRegression {
    static int checks;
    static void Check(bool condition,string description) { checks++; if(!condition) throw new Exception(description); }
    static int Main() {
        HitRectangle right=HitRectangle.Whip(0,0,1,4.5f,1f);
        Check(right.Overlaps(2f,0.9f,2.5f,2f),"Upper body overlap must hit even when the enemy center is outside");
        Check(right.Overlaps(2f,1f,2.5f,2f),"Touching the visible top edge must count");
        Check(!right.Overlaps(2f,1.01f,2.5f,2f),"A body above the visible top edge must miss");
        Check(right.Overlaps(4.4f,-0.2f,5.2f,0.2f),"Tip overlap must hit even with a center past the tip");
        Check(!right.Overlaps(4.51f,-0.2f,5.2f,0.2f),"A body beyond the tip must miss");
        Check(!right.Overlaps(-1f,-0.2f,-0.01f,0.2f),"Front strike must not hit an entirely rear body");
        HitRectangle left=HitRectangle.Whip(0,0,-1,4.5f,1f);
        Check(left.Overlaps(-2.5f,0.9f,-2f,2f),"Mirrored upper overlap must hit");
        Check(!left.Overlaps(0.01f,-0.2f,1f,0.2f),"Mirrored strike must exclude the opposite side");
        HitRectangle larger=HitRectangle.Whip(10,20,1,9f,2f);
        Check(larger.MinX==10 && larger.MaxX==19 && larger.MinY==18 && larger.MaxY==22,"Area and player translation must apply to the whole rectangle");
        Check(larger.Overlaps(18.9f,21.9f,20f,23f),"Scaled upper tip overlap must hit");
        Console.WriteLine("PASS: "+checks+" whip hitbox regression checks"); return 0;
    }
}
