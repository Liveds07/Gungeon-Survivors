using System;
using GungeonSurvivors;
class InventoryRegression {
 static int checks;
 static void Check(bool ok,string message) { checks++; if(!ok) throw new Exception(message); }
 static int Main() {
  Check(InventoryRules.CanOffer(false,0,8,5,6,false),"Sixth weapon allowed");
  Check(!InventoryRules.CanOffer(false,0,8,6,6,false),"Seventh weapon blocked");
  Check(InventoryRules.CanOffer(true,7,8,6,6,false),"Full inventory still permits owned upgrade");
  Check(!InventoryRules.CanOffer(true,8,8,6,6,false),"Max rank excluded");
  Check(!InventoryRules.CanOffer(true,8,8,2,6,true),"Evolved weapon excluded");
  Check(InventoryRules.CanOffer(false,0,5,5,6,false),"Sixth accessory allowed");
  Check(!InventoryRules.CanOffer(false,0,5,6,6,false),"Seventh accessory blocked");
  Check(InventoryRules.CanOffer(true,1,2,6,6,false),"Duplicator second rank allowed with full slots");
  Check(!InventoryRules.CanOffer(true,2,2,6,6,false),"Duplicator stops at installed two ranks");
  Check(!InventoryRules.CanEvolve(7,8,false,true),"Rank seven cannot evolve");
  Check(!InventoryRules.CanEvolve(8,8,false,false),"Required accessory must be owned");
  Check(InventoryRules.CanEvolve(8,8,false,true),"Max weapon and owned accessory are eligible");
  Check(!InventoryRules.CanEvolve(8,8,true,true),"Evolution cannot repeat");
  // Separate cap counts: a full weapon inventory leaves accessory acquisition open.
  Check(InventoryRules.CanOffer(false,0,5,0,6,false),"Accessory slots independent of weapons");
  Console.WriteLine("Inventory: "+checks+" cap, rank and evolution checks passed."); return 0;
 }
}
