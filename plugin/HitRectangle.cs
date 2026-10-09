using System;
namespace GungeonSurvivors {
public struct HitRectangle {
    public float MinX, MinY, MaxX, MaxY;
    public static HitRectangle Whip(float x,float y,float direction,float reach,float halfHeight) {
        return new HitRectangle { MinX=x+(direction<0?-reach:0), MaxX=x+(direction<0?0:reach), MinY=y-halfHeight, MaxY=y+halfHeight };
    }
    public bool Overlaps(float minX,float minY,float maxX,float maxY) {
        return minX<=MaxX && maxX>=MinX && minY<=MaxY && maxY>=MinY;
    }
}
}
