using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace GungeonSurvivors {
public partial class Plugin {
    static readonly FieldInfo QueuedAnimationField=typeof(PlayerController).GetField("m_handlingQueuedAnimation",BindingFlags.Instance|BindingFlags.NonPublic);
    static Plugin activePlugin;
    Harmony runHooks;
    bool pickerActive, foyerPickerShown, survivorConfirmed;
    int pickerHighlight;
    float pickerReadyAt;
    PlayerController pickerPlayer;
    Vector2 pickerMousePosition;
    const string PickerOverride="GungeonSurvivorsPicker";

    void InstallRunHooks() {
        activePlugin=this;
        runHooks=new Harmony("local.gungeon-survivors.run-lifecycle");
        runHooks.PatchAll(typeof(RestartHook).Assembly);
    }
    void RemoveRunHooks() {
        if(runHooks!=null) runHooks.UnpatchSelf();
        if(activePlugin==this) activePlugin=null;
    }
    [HarmonyPatch(typeof(GameManager),"QuickRestart",new Type[] { typeof(QuickRestartOptions) })]
    class RestartHook {
        [HarmonyPrefix] static void Prefix() { if(activePlugin!=null) activePlugin.BeforeQuickRestart(); }
    }
    void BeforeQuickRestart() {
        ClosePicker(); CloseChoice();
        forceFreshRun=true; prepared=false; survivorConfirmed=false; runSeed=int.MinValue;
        if(player!=null) Detach();
        Logger.LogInfo("GS_QUICK_RESTART newRun=true loadoutReset=true pickerRequired=true");
    }
    bool NativeMenuReady(PlayerController p,bool foyer) {
        if(p==null || p.spriteAnimator==null || QueuedAnimationField==null || !GameManager.HasInstance) return false;
        bool inputReady=p.CurrentInputState==PlayerInputState.AllInput || (foyer && p.CurrentInputState==PlayerInputState.FoyerInputOnly);
        return ActionGate.CanOpenMenu(inputReady,p.IsDodgeRolling,p.IsSlidingOverSurface,(bool)QueuedAnimationField.GetValue(p),GameManager.Instance.IsPaused,GameManager.Instance.IsLoadingLevel);
    }
    void BeginPicker(PlayerController p) {
        if(pickerActive || choosing) return;
        pickerPlayer=p; pickerActive=true; pickerHighlight=selected; foyerPickerShown=true;
        pickerReadyAt=Time.realtimeSinceStartup+content.ui.input_grace_seconds; ResetMenuNavigation(pickerReadyAt);
        pickerMousePosition=new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y);
        p.SetInputOverride(PickerOverride); BraveTime.SetTimeScaleMultiplier(0f,gameObject); ClearMenuVibration();
        Logger.LogInfo("GS_PICKER_OPEN foyer="+GameManager.Instance.IsFoyer+" selected="+Current.id);
    }
    void ClosePicker() {
        bool ownedPause=pickerActive || pickerPlayer!=null;
        if(pickerPlayer!=null) pickerPlayer.ClearInputOverride(PickerOverride);
        if(player!=null && player!=pickerPlayer) player.ClearInputOverride(PickerOverride);
        pickerActive=false; pickerPlayer=null;
        if(!choosing) BraveTime.ClearMultiplier(gameObject);
        if(ownedPause) ClearMenuVibration();
    }
    void ConfirmSurvivor(int index) {
        if(!pickerActive || Time.realtimeSinceStartup<pickerReadyAt || index<0 || index>=content.survivors.Length) return;
        selected=index; survivorConfirmed=true; ResetRun();
        if(player!=null) player.healthHaver.Armor=content.settings.starting_armor+Current.armor;
        if(skin!=null) { skin.sprite=sprites[Current.walkFrames[0]]; skin.transform.rotation=Quaternion.identity; }
        ClosePicker(); Logger.LogInfo("GS_SELECTION survivor="+Current.id+" picker=true nativeAnimationPreserved=true");
    }
    void UpdatePicker() {
        if(pickerPlayer==null || pickerPlayer.healthHaver==null || pickerPlayer.healthHaver.IsDead) { ClosePicker(); return; }
        if(Time.realtimeSinceStartup<pickerReadyAt) return;
        BraveInput input=BraveInput.GetInstanceForPlayer(pickerPlayer.PlayerIDX);
        GungeonActions actions=input.ActiveActions;
        if(actions==null) return;
        int step=MenuStep(actions,true);
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow)) step=-1;
        else if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow)) step=1;
        if(step!=0) { pickerHighlight=(pickerHighlight+step+content.survivors.Length)%content.survivors.Length; Logger.LogInfo("GS_PICKER_FOCUS index="+pickerHighlight); }
        InControl.InputDevice device=actions.Device ?? actions.ActiveDevice ?? InControl.InputManager.ActiveDevice;
        bool confirm=Input.GetKeyDown(KeyCode.Return) || input.MenuInteractPressed || (device!=null && device.GetControl(GungeonActions.LocalizedMenuSelectAction).WasPressed);
        if(Input.GetKeyDown(KeyCode.Alpha1)) ConfirmSurvivor(0);
        else if(Input.GetKeyDown(KeyCode.Alpha2)) ConfirmSurvivor(1);
        else if(Input.GetKeyDown(KeyCode.Alpha3)) ConfirmSurvivor(2);
        else if(Input.GetKeyDown(KeyCode.Alpha4)) ConfirmSurvivor(3);
        else if(confirm) ConfirmSurvivor(pickerHighlight);
    }
    void DrawPicker() {
        GUI.color=new Color(0,0,0,0.8f); GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture); GUI.color=Color.white;
        float width=Math.Min(520,Screen.width-36),tile=76,gap=10;
        int columns=Math.Max(1,Math.Min(5,(int)((width-48)/(tile+gap))));
        int rows=(content.survivors.Length+columns-1)/columns;
        float height=244+rows*(tile+gap);
        float x=(Screen.width-width)/2,y=(Screen.height-height)/2;
        Frame(new Rect(x,y,width,height));
        GUIStyle title=new GUIStyle(GUI.skin.label); title.fontSize=24; title.alignment=TextAnchor.MiddleCenter; title.normal.textColor=new Color(1,0.85f,0.3f);
        GUI.Label(new Rect(x+15,y+12,width-30,38),"CHOOSE YOUR SURVIVOR",title);
        Vector2 mouse=Event.current.mousePosition;
        bool mouseMoved=Event.current.type==EventType.Repaint && (mouse-pickerMousePosition).sqrMagnitude>1;
        for(int i=0;i<content.survivors.Length;i++) {
            int row=i/columns,col=i%columns;
            int count=Math.Min(columns,content.survivors.Length-row*columns);
            float rowWidth=count*tile+(count-1)*gap;
            Rect box=new Rect(x+(width-rowWidth)/2+col*(tile+gap),y+60+row*(tile+gap),tile,tile);
            if(mouseMoved && box.Contains(mouse)) pickerHighlight=i;
            bool clicked=GUI.Button(box,GUIContent.none);
            GUI.DrawTexture(new Rect(box.x+9,box.y+6,tile-18,tile-12),portraits[i],ScaleMode.ScaleToFit,true);
            if(i==pickerHighlight) {
                GUI.color=new Color(1,0.85f,0.3f);
                GUI.DrawTexture(new Rect(box.x,box.y,box.width,3),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(box.x,box.yMax-3,box.width,3),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(box.x,box.y,3,box.height),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(box.xMax-3,box.y,3,box.height),Texture2D.whiteTexture); GUI.color=Color.white;
            }
            if(clicked) { ConfirmSurvivor(i); break; }
        }
        if(Event.current.type==EventType.Repaint) pickerMousePosition=mouse;
        Survivor focus=content.survivors[pickerHighlight]; Weapon weapon=null;
        foreach(Weapon candidate in content.weapons) if(candidate.id==focus.weaponId) weapon=candidate;
        Rect detail=new Rect(x+24,y+66+rows*(tile+gap),width-48,112); Frame(detail);
        GUIStyle name=new GUIStyle(GUI.skin.label); name.fontSize=19; name.normal.textColor=new Color(1,0.85f,0.3f);
        GUIStyle description=new GUIStyle(GUI.skin.label); description.fontSize=15; description.wordWrap=true; description.normal.textColor=Color.white;
        GUI.Label(new Rect(detail.x+14,detail.y+8,detail.width-28,27),string.IsNullOrEmpty(focus.fullName)?focus.name:focus.fullName,name);
        GUI.Label(new Rect(detail.x+14,detail.y+37,detail.width-28,24),"Starting weapon: "+(weapon==null?focus.weaponId:weapon.name),GUI.skin.label);
        GUI.Label(new Rect(detail.x+14,detail.y+62,detail.width-28,44),focus.description,description);
        if(GUI.Button(new Rect(x+24,y+height-55,width-48,27),"Choose "+focus.name)) ConfirmSurvivor(pickerHighlight);
        GUI.Label(new Rect(x+24,y+height-26,width-48,23),"Arrows / stick: select     Confirm / Enter: choose     1 / 2 / 3 / 4",GUI.skin.label);
    }
}
}
