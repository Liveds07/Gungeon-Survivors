using System;
using System.Collections.Generic;
using UnityEngine;
using Dungeonator;
namespace GungeonSurvivors {
public partial class Plugin {
    class XpGem { public GameObject visual; public RoomHandler room; public float amount; public bool attracted; }
    readonly List<XpGem> xpGems=new List<XpGem>();
    GameObject garlicVisual;
    SpriteRenderer garlicRenderer;
    float regenBank,lastSoulHeal=-100f,observedHealth,observedMaximum;
    int auraHitsLogged;
    float BaseSurvivorHealth { get { return SurvivorTuning.Health(content.settings.starting_health,Current.maxHp,content.normalHealth); } }
    bool CanRankWeapon(Slot s) { return !s.evolved && s.rank<Math.Min(content.settings.max_weapon_rank,s.weapon.levels.Length); }
    void QueueChestWeaponReward(int count) {
        List<Weapon> eligible=new List<Weapon>();
        foreach(Slot s in owned) if(CanEvolve(s)) eligible.Add(s.weapon);
        if(eligible.Count>0) evolutionRewards.Enqueue(new EvolutionReward { weapons=eligible,source="CHEST",remainingRandomRanks=count-1 });
        else ApplyRandomChestRanks(count);
        Logger.LogInfo("GS_CHEST_WEAPON_REWARD slots="+count+" evolutionEligible="+eligible.Count+" characterLevel="+level+" xp="+xp);
    }
    static System.Random FreshRandom() {
        return new System.Random(Guid.NewGuid().GetHashCode());
    }
    System.Random chestRandom;
    string chestNotice; float chestNoticeUntil;
    void ApplyRandomChestRanks(int count) {
        if(count<=0) return;
        if(chestRandom==null) chestRandom=FreshRandom();
        List<string> grants=new List<string>();
        for(int i=0;i<count;i++) {
            List<Slot> eligible=owned.FindAll(delegate(Slot s) { return CanRankWeapon(s); });
            List<AccessorySlot> eligibleAccessories=accessories.FindAll(delegate(AccessorySlot a) { return a.rank<a.accessory.levels.Length; });
            int total=eligible.Count+eligibleAccessories.Count;
            if(total==0) break;
            int pick=chestRandom.Next(total);
            if(pick>=eligible.Count) {
                AccessorySlot accessory=eligibleAccessories[pick-eligible.Count]; accessory.rank++; ApplyAccessoryHealth();
                grants.Add(accessory.accessory.name+" "+accessory.rank);
                Logger.LogInfo("GS_CHEST_RANDOM accessory="+accessory.accessory.id+" rank="+accessory.rank+" level="+level+" xp="+xp);
                continue;
            }
            Slot slot=eligible[pick]; slot.rank++; slot.timer=0;
            grants.Add(slot.weapon.name+" "+slot.rank);
            Logger.LogInfo("GS_CHEST_RANDOM weapon="+slot.weapon.id+" rank="+slot.rank+" level="+level+" xp="+xp);
        }
        chestNotice=grants.Count==0?"All owned weapons and accessories are at maximum rank.":string.Join("  /  ",grants.ToArray());
        chestNoticeUntil=Time.realtimeSinceStartup+5;
        Logger.LogInfo("GS_CHEST_RANDOM_TOTAL requested="+count+" granted="+grants.Count+" characterLevelUnchanged=true");
    }
    void ApplySurvivorHealth(bool fill) {
        if(player==null || content==null) return;
        float old=player.healthHaver.GetCurrentHealth();
        player.stats.SetBaseStatValue(PlayerStats.StatType.Health,BaseSurvivorHealth,player);
        player.stats.RecalculateStats(player,false,false);
        player.healthHaver.ForceSetCurrentHealth(fill?player.healthHaver.GetMaxHealth():Math.Min(old,player.healthHaver.GetMaxHealth()));
        observedHealth=player.healthHaver.GetCurrentHealth(); observedMaximum=player.healthHaver.GetMaxHealth();
        Logger.LogInfo("GS_SURVIVOR_STATS id="+Current.id+" baseHealth="+BaseSurvivorHealth+" magnet="+Current.magnet);
    }
    void HealthChanged(float health,float maximum) {
        if(preservingAccessoryHealth) return;
        float recovered=health-observedHealth;
        if(recovered>0 && Math.Abs(maximum-observedMaximum)<.001f) foreach(Slot slot in owned) if(slot.weapon.id=="garlic" && slot.evolved) {
            slot.soulPower=Math.Min(content.settings.soul_eater_max_power_bonus,slot.soulPower+recovered*content.settings.soul_eater_growth_per_healed_heart);
            Logger.LogInfo("GS_SOUL_GROWTH recovered="+recovered+" bonus="+slot.soulPower);
        }
        observedHealth=health; observedMaximum=maximum;
    }
    void ClearSurvivorWorld() {
        ClearRunes();
        foreach(XpGem gem in xpGems) if(gem.visual!=null) Destroy(gem.visual);
        xpGems.Clear(); if(garlicVisual!=null) Destroy(garlicVisual); garlicVisual=null; garlicRenderer=null;
        lastSoulHeal=-100f; auraHitsLogged=0;
        foreach(Slot slot in owned) slot.auraHits.Clear();
    }
    void DropXpGem(HealthHaver enemy,float reward) {
        if(player==null || player.CurrentRoom==null) return;
        AIActor actor=enemy==null?null:enemy.GetComponent<AIActor>();
        Vector2 position=actor==null || (enemy!=null && enemy.IsBoss)?player.CenterPosition:actor.CenterPosition;
        if(xpGems.Count>=content.settings.xp_gem_limit) {
            XpGem nearest=null; float distance=float.MaxValue;
            foreach(XpGem gem in xpGems) if(gem.room==player.CurrentRoom && gem.visual!=null) {
                float d=((Vector2)gem.visual.transform.position-position).sqrMagnitude;
                if(d<distance) { nearest=gem; distance=d; }
            }
            if(nearest!=null) { nearest.amount+=reward; return; }
            // Keep rewards rather than discard a different room's gems when the cap is full.
            CollectXp(xpGems[0].amount); if(xpGems[0].visual!=null) Destroy(xpGems[0].visual); xpGems.RemoveAt(0);
        }
        GameObject go=new GameObject("GSXpGem"); go.layer=player.gameObject.layer;
        SpriteRenderer visual=go.AddComponent<SpriteRenderer>(); visual.sprite=sprites["xp_gem"]; visual.sortingOrder=1;
        go.transform.position=new Vector3(position.x,position.y,player.transform.position.z-.2f);
        go.transform.localScale=Vector3.one*content.settings.xp_gem_scale;
        xpGems.Add(new XpGem { visual=go,room=player.CurrentRoom,amount=reward });
        Logger.LogInfo("GS_XP_DROP amount="+reward+" position="+position);
    }
    void CollectXp(float reward) {
        float gained=reward*(growth+AccessoryStats().growth); xp+=gained; ResolveXpLevels();
        Logger.LogInfo("GS_XP_COLLECT amount="+gained+" xp="+xp+" level="+level);
    }
    void TickXpGems() {
        float radius=SurvivorTuning.PickupRadius(content.settings.xp_pickup_radius,Current.magnet);
        for(int i=xpGems.Count-1;i>=0;i--) {
            XpGem gem=xpGems[i];
            if(gem.visual==null) { xpGems.RemoveAt(i); continue; }
            if(gem.room!=player.CurrentRoom) continue;
            Vector2 position=gem.visual.transform.position;
            float distance=(position-player.CenterPosition).magnitude;
            if(distance<=content.settings.xp_collect_radius) { CollectXp(gem.amount); Destroy(gem.visual); xpGems.RemoveAt(i); continue; }
            if(distance<=radius) gem.attracted=true;
            if(gem.attracted) {
                Vector2 next=Vector2.MoveTowards(position,player.CenterPosition,content.settings.xp_gem_speed*BraveTime.DeltaTime);
                gem.visual.transform.position=new Vector3(next.x,next.y,gem.visual.transform.position.z);
            }
        }
    }
    void TickRegeneration() {
        float missing=player.healthHaver.GetMaxHealth()-player.healthHaver.GetCurrentHealth();
        float rate=SurvivorTuning.RegenRate(Current.regen+AccessoryStats().regen,content.settings.starting_health,content.normalHealth);
        float heal=SurvivorTuning.RegenStep(ref regenBank,rate,BraveTime.DeltaTime,missing,content.settings.regen_heart_step,player.IsInCombat || (SurvivalRunning && player.CurrentRoom==survivalRoom));
        if(heal>0) {
            float before=player.healthHaver.GetCurrentHealth(); player.healthHaver.ApplyHealing(heal);
            Logger.LogInfo("GS_REGEN combat="+player.IsInCombat+" recovered="+(player.healthHaver.GetCurrentHealth()-before)+" rate="+rate);
        }
    }
    Slot GarlicSlot { get { return owned.Find(delegate(Slot s) { return s.weapon.id=="garlic"; }); } }
    float GarlicRadius(Level stats) { return content.settings.garlic_radius*Mathf.Sqrt(Math.Max(0,stats.area)); }
    void UpdateGarlicVisual() {
        Slot slot=GarlicSlot;
        if(slot==null) { if(garlicVisual!=null) Destroy(garlicVisual); garlicVisual=null; garlicRenderer=null; return; }
        if(garlicVisual==null) {
            garlicVisual=new GameObject("GSGarlicAura"); garlicVisual.layer=player.gameObject.layer;
            garlicRenderer=garlicVisual.AddComponent<SpriteRenderer>(); garlicRenderer.sortingOrder=0;
        }
        garlicRenderer.sprite=sprites[(slot.evolved?slot.weapon.evolvedEffectFrames:slot.weapon.effectFrames)[0]];
        float diameter=2*GarlicRadius(WeaponStats(slot));
        garlicVisual.transform.position=new Vector3(player.CenterPosition.x,player.CenterPosition.y,player.transform.position.z+.1f);
        garlicVisual.transform.localScale=new Vector3(diameter/garlicRenderer.sprite.bounds.size.x,diameter/garlicRenderer.sprite.bounds.size.y,1);
        garlicRenderer.color=slot.evolved?new Color(0,0,0,.6f):new Color(1,1,1,.45f);
    }
    void Garlic(List<AIActor> enemies,Slot slot) {
        Level stats=WeaponStats(slot); float radius=GarlicRadius(stats);
        float interval=Math.Max(content.settings.minimum_attack_interval,stats.interval/1000f);
        foreach(AIActor enemy in enemies.ToArray()) {
            if(enemy==null || enemy.healthHaver==null || enemy.healthHaver.IsDead) continue;
            Vector2 target=enemy.CenterPosition;
            if(enemy.specRigidbody!=null && enemy.specRigidbody.HitboxPixelCollider!=null) {
                PixelCollider box=enemy.specRigidbody.HitboxPixelCollider;
                target=new Vector2(Mathf.Clamp(player.CenterPosition.x,box.UnitBottomLeft.x,box.UnitTopRight.x),Mathf.Clamp(player.CenterPosition.y,box.UnitBottomLeft.y,box.UnitTopRight.y));
            }
            if((target-player.CenterPosition).sqrMagnitude>radius*radius) continue;
            int id=enemy.GetInstanceID(); float until;
            if(slot.auraHits.TryGetValue(id,out until) && Time.time<until) continue;
            slot.auraHits[id]=Time.time+interval;
            float damage=stats.power*power*content.settings.damage_scale;
            float before=enemy.healthHaver.GetCurrentHealth();
            enemy.healthHaver.ApplyDamage(damage,(enemy.CenterPosition-player.CenterPosition).normalized,"Gungeon Survivors Garlic",CoreDamageTypes.None,DamageCategory.Normal,false,null,false);
            bool damaged=enemy.healthHaver.GetCurrentHealth()<before;
            if(damaged && slot.evolved && Time.time-lastSoulHeal>=content.settings.soul_eater_heal_cooldown && UnityEngine.Random.value<content.settings.soul_eater_heal_chance && player.healthHaver.GetCurrentHealth()<player.healthHaver.GetMaxHealth()) {
                player.healthHaver.ApplyHealing(content.settings.soul_eater_heal); lastSoulHeal=Time.time;
                Logger.LogInfo("GS_SOUL_HEAL hp="+player.healthHaver.GetCurrentHealth());
            }
            if(damaged) player.OnDidDamage(damage,enemy.healthHaver.IsDead,enemy.healthHaver);
            if(auraHitsLogged++<12) Logger.LogInfo("GS_GARLIC_HIT enemy="+id+" time="+Time.time+" interval="+interval+" damage="+damage+" damaged="+damaged+" evolved="+slot.evolved);
        }
    }
}
}

