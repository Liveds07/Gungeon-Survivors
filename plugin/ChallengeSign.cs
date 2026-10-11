using UnityEngine;
using Dungeonator;
namespace GungeonSurvivors {
// A physical sign using the host's interaction binding and native wood-panel UI.
public sealed class ChallengeSign : MonoBehaviour,IPlayerInteractable {
    public string rules;
    public RoomHandler room;
    SpriteRenderer board;
    bool inRange;
    Texture2D texture; Sprite picture;
    public void Build(string title) {
        texture=new Texture2D(48,40,TextureFormat.RGBA32,false); texture.filterMode=FilterMode.Point;
        for(int y=0;y<40;y++) for(int x=0;x<48;x++) {
            Color c=Color.clear;
            // Wall-mounted board has no floor post.
            if(y>=15 && y<=37 && x>=1 && x<=46) {
                bool edge=y==15 || y==37 || x==1 || x==46;
                c=edge?new Color(.055f,.027f,.01f):new Color(.16f+(y%6==0?.015f:0),.085f,.028f);
            }
            if((x==4 || x==43) && (y==18 || y==34)) c=new Color(.8f,.66f,.38f);
            texture.SetPixel(x,y,c);
        }
        texture.Apply(); picture=Sprite.Create(texture,new Rect(0,0,48,40),new Vector2(.5f,0),16);
        board=gameObject.AddComponent<SpriteRenderer>(); board.sprite=picture; board.sortingOrder=3;
        GameObject caption=new GameObject("Sign lettering"); caption.transform.SetParent(transform,false);
        caption.transform.localPosition=new Vector3(0,1.66f,-.02f);
        TextMesh text=caption.AddComponent<TextMesh>(); text.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
        text.text=title.Replace(" ","\n"); text.fontSize=64; text.characterSize=.09f; text.anchor=TextAnchor.MiddleCenter; text.alignment=TextAlignment.Center; text.color=new Color(1,.88f,.57f);
        text.GetComponent<MeshRenderer>().sortingOrder=4;

    }
    // Screen-space text stays legible regardless of the host camera projection/depth.
    public void DrawPrompt() {
        if(!inRange || TextBoxManager.HasTextBox(transform)) return;
        Rect box=new Rect((Screen.width-340)/2,Screen.height-125,340,40);
        Color previous=GUI.color;
        GUI.color=new Color(.025f,.025f,.06f,.95f); GUI.DrawTexture(box,Texture2D.whiteTexture);
        GUI.color=Color.white;
        GUIStyle style=new GUIStyle(GUI.skin.label); style.fontSize=18; style.alignment=TextAnchor.MiddleCenter; style.normal.textColor=Color.white;
        GUI.Label(box,"Interact to read challenge rules",style);
        GUI.color=previous;
    }
    public float GetDistanceToPoint(Vector2 point) { return Vector2.Distance(point,(Vector2)transform.position+new Vector2(0,-.8f)); }
    public float GetOverrideMaxDistance() { return 3f; }
    public void OnEnteredRange(PlayerController player) { if(board!=null) board.color=new Color(1.2f,1.2f,1.2f); inRange=true; }
    public void OnExitRange(PlayerController player) { if(board!=null) board.color=Color.white; inRange=false; Close(); }
    public string GetAnimationState(PlayerController player,out bool shouldBeFlipped) { shouldBeFlipped=false; return string.Empty; }
    public void Interact(PlayerController player) {
        if(TextBoxManager.HasTextBox(transform)) { Close(); return; }
        GameUIRoot.Instance.HideCoreUI("GS_ChallengeSign");
        TextBoxManager.ShowWoodPanel(transform.position+new Vector3(0,2.5f,0),transform,-1,rules,true,false);
        Debug.Log("GS_SIGN_INTERACT nativeWoodPanel=true");
    }
    void Close() {
        TextBoxManager.ClearTextBox(transform);
        if(GameUIRoot.HasInstance) GameUIRoot.Instance.ShowCoreUI("GS_ChallengeSign");
    }
    void OnDestroy() {
        Close(); if(room!=null) room.DeregisterInteractable(this);
        if(picture!=null) Destroy(picture); if(texture!=null) Destroy(texture);
    }
}
}
