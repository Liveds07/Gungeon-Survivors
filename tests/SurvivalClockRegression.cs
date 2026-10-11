using System;
using GungeonSurvivors;
class SurvivalClockRegression {
 static void Check(bool value,string message) {if(!value)throw new Exception(message);}
 static void Main() {
  var c=new SurvivalClock(300,20,10); c.Begin();
  for(int i=1;i<=20;i++)Check(c.ReserveSpawn()&&c.Register(i),"initial slot");
  Check(!c.ReserveSpawn()&&!c.Register(21),"hard cap");
  c.Defeated(1);c.Defeated(1);Check(c.Waiting==1,"duplicate kill");
  c.Advance(9.99f);Check(!c.ReserveSpawn(),"early replacement");
  c.Advance(.02f);Check(c.ReserveSpawn()&&c.Register(21)&&c.Alive==20,"replacement deadline");
  c.Advance(0);Check(c.Elapsed<11,"paused clock");
  c.Advance(289);Check(c.Running,"before completion");c.Advance(1);Check(c.Completed&&!c.Running&&!c.ReserveSpawn(),"completion");
  var d=new SurvivalClock(300,20,10);d.Begin();Check(d.ReserveSpawn(),"reserve");d.FailedSpawn();Check(d.Waiting==20,"failed placement retained");d.Cancel();d.Advance(300);Check(!d.Completed&&d.Alive==0&&!d.ReserveSpawn(),"abort no reward");
  foreach(int cap in new int[] {12,14,16,18,20}) {
   var family=new SurvivalClock(300,cap,10); family.Begin();
   for(int i=0;i<cap;i++) Check(family.ReserveSpawn()&&family.Register(i),"floor initial cap");
   Check(!family.ReserveSpawn(),"floor scheduled cap"); family.Defeated(0);
   family.Advance(9.99f); Check(!family.ReserveSpawn(),"floor replacement delay");
   family.Advance(.02f); Check(family.ReserveSpawn()&&family.Register(101)&&family.Alive==cap,"floor replacement deadline");
  }
  Console.WriteLine("Survival timing, cap, retry, duplicate death, pause and abort passed.");
 }
}
