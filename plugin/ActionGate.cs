namespace GungeonSurvivors {
public static class ActionGate {
    public static bool CanOpenMenu(bool inputReady,bool rolling,bool sliding,bool queuedAnimation,bool paused,bool loading) {
        return inputReady && !rolling && !sliding && !queuedAnimation && !paused && !loading;
    }
}
}
