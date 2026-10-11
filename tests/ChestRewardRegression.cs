using System;
using GungeonSurvivors;
class ChestRewardRegression {
 static int checks;
 static void Check(bool value,string name) { checks++; if(!value) throw new Exception(name); }
 static int Main() {
  var ledger=new ChestRewardLedger();
  Check(!ledger.TryOpen(1,false,false,false),"Locked or unsuccessful opening gives nothing");
  Check(ledger.TryOpen(1,false,true,false),"Later successful opening gives reward");
  Check(!ledger.TryOpen(1,false,true,false),"Repeated opening cannot double reward");
  Check(!ledger.TryBreak(1,true,true),"Breaking opened chest gives no XP");
  Check(!ledger.TryOpen(2,false,false,false),"Mimic transformation does not count as opening");
  Check(!ledger.TryBreak(2,false,false),"Failed break gives nothing");
  Check(ledger.TryBreak(2,false,true),"Unopened broken chest gives break reward");
  Check(!ledger.TryBreak(2,false,true),"Repeated break gives nothing");
  Check(!ledger.TryOpen(2,false,true,false),"Broken chest cannot later grant opening reward");
  Check(!ledger.TryOpen(3,true,true,false),"Previously open chest is excluded");
  Check(!ledger.TryOpen(4,false,true,true),"Broken state cannot grant opening reward");
  ledger.Clear(); Check(ledger.TryOpen(1,false,true,false),"New run resets deduplication");
  Console.WriteLine("PASS: "+checks+" chest reward transition checks"); return 0;
 }
}
