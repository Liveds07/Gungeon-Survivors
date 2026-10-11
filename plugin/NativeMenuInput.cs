using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
namespace GungeonSurvivors {
public partial class Plugin {
    static readonly BindingFlags InputFields=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly FieldInfo TimedRumble=typeof(BraveInput).GetField("m_currentVibrations",InputFields);
    static readonly FieldInfo LargeRumble=typeof(BraveInput).GetField("m_sustainedLargeVibration",InputFields);
    static readonly FieldInfo SmallRumble=typeof(BraveInput).GetField("m_sustainedSmallVibration",InputFields);
    static readonly MethodInfo NativeSetVibration=typeof(BraveInput).GetMethod("SetVibration",InputFields);
    static bool deathMenuInputScope;
    MenuNavigation menuNavigation;
    void ResetMenuNavigation(float readyAt) {
        menuNavigation=new MenuNavigation(content.ui.stick_engage_threshold,content.ui.stick_release_threshold,content.ui.navigation_cooldown_seconds);
        menuNavigation.Reset(readyAt);
    }
    int MenuStep(GungeonActions actions,bool horizontal) {
        float y=Math.Max(actions.SelectUp.Value,actions.Up.Value)-Math.Max(actions.SelectDown.Value,actions.Down.Value);
        if(horizontal) {
            float x=Math.Max(actions.SelectRight.Value,actions.Right.Value)-Math.Max(actions.SelectLeft.Value,actions.Left.Value);
            if(Math.Abs(x)>Math.Abs(y)) y=-x;
        }
        return menuNavigation==null?0:menuNavigation.Step(y,Time.realtimeSinceStartup);
    }
    bool MuteMenuRumble { get { return content!=null && content.ui!=null && content.ui.mute_controller_rumble && (pickerActive || choosing); } }
    static void ClearVibration(BraveInput input) {
        if(input==null) return;
        IList queued=TimedRumble.GetValue(input) as IList; if(queued!=null) queued.Clear();
        LargeRumble.SetValue(input,0f); SmallRumble.SetValue(input,0f);
        NativeSetVibration.Invoke(input,new object[] {0f,0f});
    }
    void ClearMenuVibration() {
        if(!GameManager.HasInstance || content==null || content.ui==null || !content.ui.mute_controller_rumble) return;
        ClearVibration(BraveInput.GetInstanceForPlayer(0));
    }
    [HarmonyPatch(typeof(BraveInput),"LateUpdate")]
    class MenuRumbleQueueHook {
        [HarmonyPrefix] static void Prefix(BraveInput __instance) {
            if(activePlugin!=null && activePlugin.MuteMenuRumble) ClearVibration(__instance);
        }
    }
    [HarmonyPatch(typeof(BraveInput),"SetVibration",new Type[] {typeof(float),typeof(float)})]
    class MenuRumbleMotorHook {
        [HarmonyPrefix] static void Prefix(ref float __0,ref float __1) {
            if(activePlugin!=null && activePlugin.MuteMenuRumble) { __0=0f; __1=0f; }
        }
    }
    // Native death-page code gates pad confirmation on LastInputType. A pad
    // button itself is enough; the player need not move a stick first.
    [HarmonyPatch(typeof(AmmonomiconDeathPageController),"Update")]
    class DeathMenuScopeHook {
        [HarmonyPrefix] static void Prefix(ref bool __state) {
            __state=deathMenuInputScope;
            deathMenuInputScope=activePlugin!=null && activePlugin.ready && GameManager.HasInstance && GameManager.Instance.PrimaryPlayer!=null && GameManager.Instance.PrimaryPlayer.healthHaver!=null && GameManager.Instance.PrimaryPlayer.healthHaver.IsDead;
        }
        [HarmonyPostfix] static void Postfix(bool __state) { deathMenuInputScope=__state; }
        [HarmonyFinalizer] static void Finalizer(bool __state) { deathMenuInputScope=__state; }
    }
    [HarmonyPatch(typeof(BraveInput),"IsKeyboardAndMouse",new Type[] {typeof(bool)})]
    class DeathMenuControllerButtonHook {
        [HarmonyPrefix] static bool Prefix(ref bool __result) {
            if(!deathMenuInputScope) return true;
            InControl.InputDevice device=InControl.InputManager.ActiveDevice;
            if(device==null || (!BraveInput.WasSelectPressed(device) && !BraveInput.WasCancelPressed(device))) return true;
            __result=false; return false;
        }
    }
}
}

