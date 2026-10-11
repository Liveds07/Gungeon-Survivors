using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace GungeonSurvivors {
public partial class Settings {
    public float runetracer_damage_multiplier,runetracer_hit_radius,runetracer_trail_seconds,runetracer_trail_width;
    public float runetracer_explosion_radius,runetracer_explosion_cooldown_seconds,runetracer_explosion_visual_seconds;
    public float runetracer_retaliation_cooldown_seconds,runetracer_retaliation_armor_scale;
    public int runetracer_max_projectiles;
}
public partial class Plugin {
    readonly List<RuneFlight> runeFlights=new List<RuneFlight>();
    readonly List<GameObject> runeExplosions=new List<GameObject>();
    int runeGeneration; int runeEventsLogged;
    float runeRetaliationAt=-100;
    internal bool RunePaused { get { return choosing || pickerActive || NativeSequenceActive || !GameManager.HasInstance || GameManager.Instance.IsPaused; } }
    internal bool RuneOwnerValid(PlayerController owner,RoomHandler room) { return player==owner && owner!=null && !owner.healthHaver.IsDead && owner.CurrentRoom==room && !GameManager.Instance.IsLoadingLevel; }
    void ClearRunes() {
        runeGeneration++; runeRetaliationAt=-100; runeEventsLogged=0;
        foreach(RuneFlight flight in runeFlights.ToArray()) if(flight!=null) flight.End();
        runeFlights.Clear(); foreach(GameObject effect in runeExplosions) if(effect!=null) Destroy(effect); runeExplosions.Clear();
    }
    internal void RuneFinished(RuneFlight flight) { runeFlights.Remove(flight); }
    internal void RuneTrace(string message) { if(runeEventsLogged++<120) Logger.LogInfo(message); }
    IEnumerator RuneBurst(Level stats,Slot slot) {
        int generation=runeGeneration; PlayerController owner=player; RoomHandler room=player.CurrentRoom;
        int count=Math.Max(1,(int)stats.amount);
        for(int index=0;index<count;index++) {
            if(generation!=runeGeneration || !RuneOwnerValid(owner,room) || RunePaused || wandTemplate==null) yield break;
            if(runeFlights.Count>=content.settings.runetracer_max_projectiles) yield break;
            float angle=UnityEngine.Random.Range(0f,360f);
            Vector2 direction=new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad));
            GameObject go=SpawnManager.SpawnProjectile(wandTemplate.gameObject,new Vector3(owner.CenterPosition.x,owner.CenterPosition.y,0),Quaternion.Euler(0,0,angle),true);
            if(go!=null) {
                Projectile projectile=go.GetComponent<Projectile>(); projectile.Owner=owner; projectile.Shooter=owner.specRigidbody;
                projectile.collidesWithPlayer=false; projectile.collidesWithEnemies=false;
                float speed=content.settings.projectile_speed*stats.speed;
                projectile.baseData=new ProjectileData { damage=0,speed=speed,range=speed*stats.duration/1000f+10,force=0 };
                projectile.UpdateSpeed(); projectile.UpdateCollisionMask(); projectile.SendInDirection(direction,true,true);
                RuneFlight flight=go.AddComponent<RuneFlight>(); runeFlights.Add(flight);
                flight.Initialize(this,projectile,owner,room,stats,slot.evolved,
                    sprites[(slot.evolved?slot.weapon.evolvedEffectFrames:slot.weapon.effectFrames)[0]],content.settings);
                shots++; RuneTrace("GS_RUNE_FIRE rank="+slot.rank+" evolved="+slot.evolved+" speed="+speed+" duration="+stats.duration+" damage="+RuneDamage(stats)+" active="+runeFlights.Count);
            }
            if(index+1<count) yield return new WaitForSeconds(slot.weapon.repeatInterval);
        }
    }
    internal float RuneDamage(Level stats) { return stats.power*power*content.settings.damage_scale*content.settings.runetracer_damage_multiplier; }
    internal void RuneHit(AIActor enemy,float damage,Vector2 direction) {
        if(player==null || enemy==null || enemy.healthHaver==null || enemy.healthHaver.IsDead) return;
        float before=enemy.healthHaver.GetCurrentHealth();
        enemy.healthHaver.ApplyDamage(damage,direction,"Gungeon Survivors Runetracer",CoreDamageTypes.None,DamageCategory.Normal,false,null,false);
        float dealt=Math.Max(0,before-enemy.healthHaver.GetCurrentHealth());
        if(dealt>0) { player.OnDidDamage(dealt,enemy.healthHaver.IsDead,enemy.healthHaver); RuneTrace("GS_RUNE_HIT damage="+dealt+" killed="+enemy.healthHaver.IsDead); }
    }
    internal void RuneExplosion(Vector2 position,RoomHandler room,Level stats,string cause) {
        if(player==null || room!=player.CurrentRoom || RunePaused) return;
        float radius=content.settings.runetracer_explosion_radius*Math.Max(1,stats.area);
        float damage=RuneDamage(stats)*(cause=="retaliation"?1+AccessoryStats().armor*content.settings.runetracer_retaliation_armor_scale:1);
        int targets=0; List<AIActor> enemies=room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
        if(enemies!=null) foreach(AIActor enemy in enemies.ToArray()) {
            if(enemy==null || enemy.healthHaver==null || enemy.healthHaver.IsDead) continue;
            Vector2 lower,upper; RuneFlight.EnemyBounds(enemy,out lower,out upper);
            Vector2 nearest=new Vector2(Mathf.Clamp(position.x,lower.x,upper.x),Mathf.Clamp(position.y,lower.y,upper.y));
            if((nearest-position).sqrMagnitude>radius*radius) continue;
            RuneHit(enemy,damage,(enemy.CenterPosition-position).normalized); targets++;
        }
        Weapon weapon=Array.Find(content.weapons,delegate(Weapon w) { return w.id=="runetracer"; });
        StartCoroutine(RuneExplosionVisual(position,room,weapon.explosionFrames,radius));
        string message="GS_RUNE_EXPLOSION cause="+cause+" targets="+targets+" damage="+damage; if(cause=="retaliation") Logger.LogInfo(message); else RuneTrace(message);
    }
    IEnumerator RuneExplosionVisual(Vector2 position,RoomHandler room,string[] frames,float radius) {
        if(frames==null || frames.Length==0) yield break;
        GameObject go=new GameObject("GSNoFutureExplosion"); runeExplosions.Add(go);
        SpriteRenderer renderer=go.AddComponent<SpriteRenderer>(); renderer.sortingOrder=3;
        go.transform.position=new Vector3(position.x,position.y,player.transform.position.z-.2f);
        float elapsed=0,duration=content.settings.runetracer_explosion_visual_seconds;
        while(go!=null && player!=null && player.CurrentRoom==room && elapsed<duration) {
            if(!RunePaused) elapsed+=BraveTime.DeltaTime;
            renderer.sprite=sprites[frames[Math.Min(frames.Length-1,(int)(elapsed/duration*frames.Length))]];
            float size=radius*2; go.transform.localScale=new Vector3(size/renderer.sprite.bounds.size.x,size/renderer.sprite.bounds.size.y,1);
            yield return null;
        }
        runeExplosions.Remove(go); if(go!=null) Destroy(go);
    }
    void RuneRetaliation(float health,float maximum,CoreDamageTypes types,DamageCategory category,Vector2 direction) {
        if(player==null || player.healthHaver.IsDead || RunePaused || Time.time-runeRetaliationAt<content.settings.runetracer_retaliation_cooldown_seconds) return;
        Slot slot=owned.Find(delegate(Slot s) { return s.weapon.id=="runetracer" && s.evolved; });
        if(slot==null || player.CurrentRoom==null) return;
        runeRetaliationAt=Time.time; RuneExplosion(player.CenterPosition,player.CurrentRoom,WeaponStats(slot),"retaliation");
    }
}

// Each flight owns its added modifiers and visuals. Native pool reuse must never
// inherit unlimited bounces, hidden bullets or an old survivor's damage callback.
public sealed class RuneFlight : MonoBehaviour {
    Plugin plugin; Projectile projectile; PlayerController owner; RoomHandler room; Level stats; Settings settings;
    BounceProjModifier bounce; PierceProjModifier pierce; GameObject head,trailObject; TrailRenderer trail; Material material;
    Color nativeColor; Vector2 previous; float age,lastExplosion=-100; bool evolved,ended,trailReady;
    readonly Dictionary<AIActor,float> hitTimes=new Dictionary<AIActor,float>();
    public void Initialize(Plugin source,Projectile bullet,PlayerController player,RoomHandler currentRoom,Level attributes,bool evolution,Sprite sprite,Settings config) {
        plugin=source; projectile=bullet; owner=player; room=currentRoom; stats=attributes; evolved=evolution; settings=config; previous=(Vector2)bullet.transform.position;
        nativeColor=bullet.sprite.color; bullet.sprite.color=new Color(1,1,1,0); SpriteOutlineManager.ToggleOutlineRenderers(bullet.sprite,false);
        bounce=gameObject.AddComponent<BounceProjModifier>(); bounce.numberOfBounces=10000; bounce.chanceToDieOnBounce=0; bounce.percentVelocityToLoseOnBounce=0; bounce.damageMultiplierOnBounce=1; bounce.onlyBounceOffTiles=true; bounce.OnSpawned(); bounce.OnBounce+=Bounced;
        pierce=gameObject.AddComponent<PierceProjModifier>(); pierce.penetration=10000;
        head=new GameObject("GSRunetracerCrystal"); head.transform.SetParent(transform,false); SpriteRenderer renderer=head.AddComponent<SpriteRenderer>(); renderer.sprite=sprite; renderer.sortingOrder=2;
        head.transform.localScale=Vector3.one*settings.projectile_visual_scale*Math.Max(1,stats.area);
        trailObject=new GameObject("GSRunetracerTrail"); trailObject.SetActive(false); trailObject.transform.position=new Vector3(previous.x,previous.y,transform.position.z-.1f);
        trail=trailObject.AddComponent<TrailRenderer>(); trail.time=settings.runetracer_trail_seconds; trail.minVertexDistance=.04f; trail.startWidth=settings.runetracer_trail_width; trail.endWidth=0;
        material=new Material(Shader.Find("Sprites/Default")); trail.material=material; trail.sortingOrder=1;
        Gradient gradient=new Gradient(); gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(evolved?Color.magenta:Color.cyan,0),new GradientColorKey(Color.yellow,.5f),new GradientColorKey(Color.blue,1) },new GradientAlphaKey[] { new GradientAlphaKey(.8f,0),new GradientAlphaKey(0,1) }); trail.colorGradient=gradient;
    }
    void Bounced() {
        if(ended || plugin==null || plugin.RunePaused) return;
        plugin.RuneTrace("GS_RUNE_BOUNCE evolved="+evolved+" age="+age);
        Sweep(projectile.specRigidbody.UnitCenter);
        if(evolved && age-lastExplosion>=settings.runetracer_explosion_cooldown_seconds) { lastExplosion=age; plugin.RuneExplosion(previous,room,stats,"bounce"); }
    }
    void LateUpdate() {
        if(ended) return;
        if(projectile==null || !plugin.RuneOwnerValid(owner,room)) { End(); return; }
        if(plugin.RunePaused) return;
        age+=BraveTime.DeltaTime;
        if(age>=stats.duration/1000f) { plugin.RuneTrace("GS_RUNE_EXPIRED age="+age+" expected="+stats.duration/1000f); End(); return; }
        Vector2 position=projectile.specRigidbody.UnitCenter; Sweep(position);
        if(head!=null) { head.transform.position=new Vector3(position.x,position.y,transform.position.z-.1f); head.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(projectile.Direction.y,projectile.Direction.x)*Mathf.Rad2Deg); }
        if(trailObject!=null) {
            trailObject.transform.position=new Vector3(position.x,position.y,transform.position.z-.1f);
            if(!trailReady) { trailObject.SetActive(true); trail.Clear(); trailReady=true; }
        }
    }
    void Sweep(Vector2 position) {
        List<AIActor> enemies=room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
        if(enemies!=null) foreach(AIActor enemy in enemies.ToArray()) {
            if(enemy==null || enemy.healthHaver==null || enemy.healthHaver.IsDead) continue;
            float last; if(hitTimes.TryGetValue(enemy,out last) && age-last<stats.hitBoxDelay/1000f) continue;
            Vector2 lower,upper; EnemyBounds(enemy,out lower,out upper); float radius=settings.runetracer_hit_radius*Math.Max(1,stats.area);
            if(!RuneGeometry.Crosses(previous.x,previous.y,position.x,position.y,lower.x-radius,lower.y-radius,upper.x+radius,upper.y+radius)) continue;
            hitTimes[enemy]=age; plugin.RuneHit(enemy,plugin.RuneDamage(stats),projectile.Direction);
        }
        previous=position;
    }
    internal static void EnemyBounds(AIActor enemy,out Vector2 lower,out Vector2 upper) {
        PixelCollider collider=enemy.specRigidbody==null?null:enemy.specRigidbody.HitboxPixelCollider;
        if(collider!=null) { lower=collider.UnitBottomLeft; upper=collider.UnitTopRight; }
        else if(enemy.specRigidbody!=null) { lower=enemy.specRigidbody.UnitBottomLeft; upper=enemy.specRigidbody.UnitTopRight; }
        else { lower=upper=enemy.CenterPosition; }
    }
    public void End() { if(ended) return; Cleanup(); if(projectile!=null && projectile.gameObject.activeSelf) projectile.DieInAir(true,false,false,true); }
    void OnDisable() { Cleanup(); }
    void Cleanup() {
        if(ended) return; ended=true;
        if(bounce!=null) { bounce.OnBounce-=Bounced; bounce.numberOfBounces=0; Destroy(bounce); }
        if(pierce!=null) { pierce.penetration=0; Destroy(pierce); }
        if(projectile!=null && projectile.sprite!=null) { projectile.sprite.color=nativeColor; SpriteOutlineManager.ToggleOutlineRenderers(projectile.sprite,true); }
        if(head!=null) Destroy(head); if(trailObject!=null) Destroy(trailObject); if(material!=null) Destroy(material);
        if(plugin!=null) plugin.RuneFinished(this); hitTimes.Clear(); Destroy(this);
    }
}
}
