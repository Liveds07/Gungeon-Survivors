using UnityEngine;
namespace GungeonSurvivors {
// The installed VampiricaProjectile uses a main slash and two offset tinted ghosts.
public sealed class BloodyTearVisual : MonoBehaviour {
    SpriteRenderer main,red,purple;
    Material additive;
    float began,duration;
    public void Initialize(SpriteRenderer slash,float seconds) {
        main=slash; duration=seconds; began=Time.time;
        red=Ghost("GSBloodyTearRed"); purple=Ghost("GSBloodyTearPurple");
        Shader shader=Shader.Find("Particles/Additive");
        if(shader!=null) { additive=new Material(shader); additive.mainTexture=slash.sprite.texture; red.sharedMaterial=additive; purple.sharedMaterial=additive; }
    }
    SpriteRenderer Ghost(string name) {
        GameObject go=new GameObject(name); go.transform.SetParent(transform,false); go.layer=gameObject.layer;
        SpriteRenderer renderer=go.AddComponent<SpriteRenderer>(); renderer.sprite=main.sprite; return renderer;
    }
    void LateUpdate() {
        if(main==null) return;
        float alpha=Mathf.Clamp01(1-(Time.time-began)/duration);
        main.color=new Color(17f/255,0,17f/255,alpha);
        red.color=new Color(1,0,0,alpha); purple.color=new Color(.4f,0,.4f,alpha);
        red.flipX=purple.flipX=main.flipX; red.sortingOrder=main.sortingOrder+1; purple.sortingOrder=main.sortingOrder+2;
        red.transform.localPosition=new Vector3(.25f,.125f,-.001f); purple.transform.localPosition=new Vector3(-.25f,-.125f,-.002f);
    }
    void OnDestroy() { if(additive!=null) Destroy(additive); }
}
}
