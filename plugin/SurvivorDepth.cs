using UnityEngine;
namespace GungeonSurvivors {
// Mirror the installed tk2dBaseSprite.SetPositions tilt using private-cache art.
// The native sprite and animation library stay intact for gameplay.
public sealed class SurvivorDepth : MonoBehaviour {
    SpriteRenderer source;
    tk2dBaseSprite native;
    Mesh geometry;
    Material visualMaterial;
    public MeshRenderer Visual { get; private set; }
    public void Initialize(SpriteRenderer sprite,tk2dBaseSprite body) {
        source=sprite; native=body;
        GameObject child=new GameObject("GSSurvivorDepthGeometry");
        child.transform.SetParent(transform,false); child.layer=gameObject.layer;
        geometry=new Mesh(); geometry.name="GSSurvivorDepth";
        child.AddComponent<MeshFilter>().sharedMesh=geometry;
        Visual=child.AddComponent<MeshRenderer>(); visualMaterial=new Material(body.GetComponent<Renderer>().sharedMaterial); Visual.sharedMaterial=visualMaterial;
        Refresh();
    }
    public void Refresh() {
        if(source==null || source.sprite==null || native==null || Visual==null) return;
        source.enabled=false;
        Sprite sprite=source.sprite;
        Vector2[] points=sprite.vertices;
        Vector3[] vertices=new Vector3[points.Length];
        Color[] colors=new Color[points.Length];
        float centerHeight=transform.position.y-native.transform.position.y;
        for(int i=0;i<points.Length;i++) {
            Vector3 vertex=new Vector3(source.flipX?-points[i].x:points[i].x,points[i].y,0);
            Vector3 worldOffset=transform.TransformVector(vertex);
            float height=worldOffset.y+centerHeight;
            float depth=native.IsPerpendicular?-height*(1f+native.AdditionalPerpForwardPercentage):height;
            vertex.z=depth/Mathf.Max(0.0001f,transform.lossyScale.z);
            vertices[i]=vertex; colors[i]=source.color;
        }
        ushort[] indices=sprite.triangles; int[] triangles=new int[indices.Length];
        for(int i=0;i<indices.Length;i++) triangles[i]=indices[i];
        geometry.Clear(); geometry.vertices=vertices; geometry.uv=sprite.uv;
        geometry.colors=colors; geometry.triangles=triangles; geometry.RecalculateNormals(); geometry.RecalculateBounds();
        Visual.sortingLayerID=source.sortingLayerID; Visual.sortingOrder=source.sortingOrder;
        MaterialPropertyBlock block=new MaterialPropertyBlock();
        block.SetTexture("_MainTex",sprite.texture); Visual.SetPropertyBlock(block);
    }
    void OnDestroy() { if(geometry!=null) Destroy(geometry); if(visualMaterial!=null) Destroy(visualMaterial); }
}
}


