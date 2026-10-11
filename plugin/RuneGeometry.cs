using System;
namespace GungeonSurvivors {
public static class RuneGeometry {
    public static bool Crosses(float x0,float y0,float x1,float y1,float minX,float minY,float maxX,float maxY) {
        float near=0,far=1;
        return Slab(x0,x1-x0,minX,maxX,ref near,ref far) && Slab(y0,y1-y0,minY,maxY,ref near,ref far);
    }
    static bool Slab(float origin,float direction,float min,float max,ref float near,ref float far) {
        if(Math.Abs(direction)<.000001f) return origin>=min && origin<=max;
        float a=(min-origin)/direction,b=(max-origin)/direction;
        if(a>b) { float swap=a; a=b; b=swap; }
        near=Math.Max(near,a); far=Math.Min(far,b); return near<=far;
    }
}
}
