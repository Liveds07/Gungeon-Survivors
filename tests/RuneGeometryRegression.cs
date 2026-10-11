using System;
using GungeonSurvivors;
class RuneGeometryRegression {
    static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
    static void Main() {
        Check(RuneGeometry.Crosses(-20,0,20,0,-1,-1,1,1),"Fast shot tunnels through enemy");
        Check(RuneGeometry.Crosses(20,0,-20,0,-1,-1,1,1),"Reverse fast shot missed");
        Check(RuneGeometry.Crosses(0,-20,0,20,-1,-1,1,1),"Vertical shot missed");
        Check(!RuneGeometry.Crosses(-20,2,20,2,-1,-1,1,1),"Parallel near miss hit");
        Check(!RuneGeometry.Crosses(-20,-20,-5,-5,-1,-1,1,1),"Segment outside hit");
        Check(RuneGeometry.Crosses(0,0,0,0,-1,-1,1,1),"Stationary overlap missed");
        Check(!RuneGeometry.Crosses(2,2,2,2,-1,-1,1,1),"Stationary miss hit");
        Check(RuneGeometry.Crosses(-2,1,2,1,-1,-1,1,1),"Edge contact missed");
        Console.WriteLine("Rune sweep geometry passed");
    }
}
