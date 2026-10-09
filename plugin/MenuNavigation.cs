using System;
namespace GungeonSurvivors {
// Menu-local hysteresis: an intentional tilt must end before another selection.
public sealed class MenuNavigation {
    readonly float engage, release, cooldown;
    bool armed;
    float readyAt, nextAllowed;
    public MenuNavigation(float engageThreshold,float releaseThreshold,float cooldownSeconds) {
        if(releaseThreshold<0 || engageThreshold<=releaseThreshold || engageThreshold>1 || cooldownSeconds<0) throw new ArgumentException("Invalid menu navigation thresholds");
        engage=engageThreshold; release=releaseThreshold; cooldown=cooldownSeconds;
    }
    public void Reset(float inputReadyAt) { armed=false; readyAt=inputReadyAt; nextAllowed=inputReadyAt; }
    public int Step(float vertical,float now) {
        if(now<readyAt) return 0;
        float magnitude=Math.Abs(vertical);
        if(magnitude<=release) { armed=true; return 0; }
        if(!armed || magnitude<engage) return 0;
        armed=false;
        if(now<nextAllowed) return 0;
        nextAllowed=now+cooldown;
        return vertical>0?-1:1;
    }
}
}
