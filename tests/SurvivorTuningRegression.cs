using System;
using GungeonSurvivors;
class SurvivorTuningRegression {
 static int checks;
 static void Check(bool value,string name) { checks++; if(!value) throw new Exception(name); }
 static bool Near(float a,float b) { return Math.Abs(a-b)<.00001f; }
 static void Main() {
  Check(Near(SurvivorTuning.Health(3,70,100),2),"Poe health quantized");
  Check(Near(SurvivorTuning.Health(3,120,100),3.5f),"Antonio health quantized");
  Check(Near(SurvivorTuning.Health(3,100,100),3),"normal health");
  Check(Near(SurvivorTuning.PickupRadius(1.8f,.25f),2.25f),"Poe pickup bonus");
  Check(Near(SurvivorTuning.RegenRate(.2f,3,100),.006f),"rank1 regen");
  Check(Near(SurvivorTuning.RegenRate(1,3,100),.03f),"rank5 regen");
  float bank=.49f;
  Check(Near(SurvivorTuning.RegenStep(ref bank,.006f,2,1,.5f,true),.5f),"half-heart threshold");
  Check(bank<.01f,"remaining fraction only");
  bank=.4f;Check(SurvivorTuning.RegenStep(ref bank,1,20,1,.5f,false)==0 && bank==0,"no healing or banking outside combat");
  bank=.4f;Check(SurvivorTuning.RegenStep(ref bank,1,20,0,.5f,true)==0 && bank==0,"no bank at full health");
  bank=0;Check(SurvivorTuning.RegenStep(ref bank,1,0,1,.5f,true)==0 && bank==0,"pause does not advance");
  bank=0;Check(SurvivorTuning.RegenStep(ref bank,1,20,.5f,.5f,true)==.5f && bank==0,"overheal clamped and bank cleared");
  Console.WriteLine("PASS: "+checks+" survivor stat and regeneration checks");
 }
}
