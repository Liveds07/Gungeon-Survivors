using System;
using GungeonSurvivors;
class MenuNavigationRegression {
    static int checks;
    static void Expect(MenuNavigation nav,float axis,float time,int expected,string description) {
        int actual=nav.Step(axis,time); checks++;
        if(actual!=expected) throw new Exception(description+": expected "+expected+", got "+actual);
    }
    static int Main() {
        var nav=new MenuNavigation(0.7f,0.25f,0.25f); nav.Reset(0.2f);
        Expect(nav,0f,0.1f,0,"Menu grace does not arm the stick");
        Expect(nav,1f,0.21f,0,"Held movement when menu opens must not change selection");
        Expect(nav,0f,0.3f,0,"Returning to center arms navigation");
        Expect(nav,0.5f,0.4f,0,"Small tilt is ignored");
        Expect(nav,0.8f,0.5f,-1,"Deliberate upward tilt changes selection once");
        Expect(nav,1f,0.9f,0,"Holding the stick does not repeat");
        Expect(nav,0.3f,1f,0,"Near-center noise must not rearm");
        Expect(nav,0.85f,1.1f,0,"Threshold jitter must not repeat");
        Expect(nav,-1f,1.2f,0,"Reversing without returning to center must not repeat");
        Expect(nav,0.2f,1.3f,0,"Center release zone rearms navigation");
        Expect(nav,-0.8f,1.4f,1,"Deliberate downward tilt selects the next row");
        Expect(nav,0f,1.45f,0,"Center after selection");
        Expect(nav,1f,1.5f,0,"Rapid second tilt is debounced");
        Expect(nav,1f,2f,0,"A debounced held tilt must not fire later");
        Expect(nav,0f,2.1f,0,"Rearm after debounced tilt");
        Expect(nav,1f,2.2f,-1,"Fresh deliberate tilt works after cooldown");
        nav.Reset(3f);
        Expect(nav,1f,3.1f,0,"A new level-up resets the latch");
        Console.WriteLine("PASS: "+checks+" menu navigation regression checks"); return 0;
    }
}
