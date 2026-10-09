using System;
using GungeonSurvivors;
class ProgressionRegression {
 static void Check(bool result) { if(!result) throw new Exception("Progression regression failed"); }
 static int Main() {
  Check(Progression.Required(1,8,4,.5f)==8);
  Check(Progression.Required(2,8,4,.5f)==12.5f);
  Check(Progression.Required(3,8,4,.5f)==18);
  for(int i=2;i<100;i++) Check(Progression.Required(i,8,4,.5f)>Progression.Required(i-1,8,4,.5f));
  Check(Progression.Reward(15,false,15,.5f,5,10)==1);
  Check(Progression.Reward(3,false,15,.5f,5,10)==.5f);
  Check(Progression.Reward(60,false,15,.5f,5,10)==2);
  Check(Progression.Reward(60000,false,15,.5f,5,10)==5);
  Check(Progression.Reward(60,true,15,.5f,5,10)==10);
  float xp=40; int level=1;
  while(xp>=Progression.Required(level,8,4,.5f)) xp-=Progression.Required(level++,8,4,.5f);
  Check(level==4 && xp==1.5f);
  Console.WriteLine("Progression: rising requirements, reward tiers, caps, boss floor and overflow passed."); return 0;
 }
}
