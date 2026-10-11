using System;
using System.Collections.Generic;
namespace GungeonSurvivors {
public sealed class SurvivalClock {
    readonly float duration,delay;
    readonly int limit;
    readonly HashSet<int> alive=new HashSet<int>();
    readonly List<float> due=new List<float>();
    int reserved;
    public float Elapsed { get; private set; }
    public bool Started { get; private set; }
    public bool Running { get; private set; }
    public bool Completed { get; private set; }
    public int Alive { get { return alive.Count; } }
    public int Waiting { get { return due.Count; } }
    public SurvivalClock(float seconds,int maximum,float replacementDelay) {
        if(seconds<=0 || maximum<1 || replacementDelay<0) throw new ArgumentException("Invalid survival rules");
        duration=seconds; limit=maximum; delay=replacementDelay;
    }
    public void Begin() { if(Started) return; Started=Running=true; for(int i=0;i<limit;i++) due.Add(0); }
    public void Advance(float seconds) {
        if(!Running) return;
        Elapsed=Math.Min(duration,Elapsed+Math.Max(0,seconds));
        if(Elapsed>=duration) { Running=false; Completed=true; due.Clear(); reserved=0; }
    }
    public bool ReserveSpawn() {
        if(!Running || Alive+reserved>=limit) return false;
        int index=-1; float earliest=float.MaxValue;
        for(int i=0;i<due.Count;i++) if(due[i]<=Elapsed && due[i]<earliest) { index=i; earliest=due[i]; }
        if(index<0) return false;
        due.RemoveAt(index); reserved++; return true;
    }
    public bool Register(int id) {
        if(!Running || reserved<1 || alive.Count>=limit || !alive.Add(id)) return false;
        reserved--; return true;
    }
    public void FailedSpawn() { if(reserved>0) { reserved--; due.Add(Elapsed+.5f); } }
    public void Defeated(int id) { if(Running && alive.Remove(id)) due.Add(Elapsed+delay); }
    public void Cancel() { Running=false; due.Clear(); reserved=0; alive.Clear(); }
}
}
