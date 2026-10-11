using System;
using GungeonSurvivors;
class ActionGateRegression {
 static void Check(bool value,string name) { if(!value) throw new Exception(name); }
 static int Main() {
  Check(ActionGate.CanOpenMenu(true,false,false,false,false,false),"Idle ready player can select");
  Check(!ActionGate.CanOpenMenu(false,false,false,false,false,false),"Native input lock waits");
  Check(!ActionGate.CanOpenMenu(true,true,false,false,false,false),"Dodge roll waits");
  Check(!ActionGate.CanOpenMenu(true,false,true,false,false,false),"Table slide waits");
  Check(!ActionGate.CanOpenMenu(true,false,false,true,false,false),"Chest/table queued animation waits");
  Check(!ActionGate.CanOpenMenu(true,false,false,false,true,false),"Paused game waits");
  Check(!ActionGate.CanOpenMenu(true,false,false,false,false,true),"Loading game waits");
  Console.WriteLine("PASS: 7 native action/menu timing checks"); return 0;
 }
}
