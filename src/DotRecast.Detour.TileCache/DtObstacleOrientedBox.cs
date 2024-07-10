using System.Numerics;

namespace DotRecast.Detour.TileCache
{
    public class DtObstacleOrientedBox
    {
        public Vector3 center;
        public Vector3 extents;
        public readonly float[] rotAux = new float[2]; // { Cos(0.5f*angle)*Sin(-0.5f*angle); Cos(0.5f*angle)*Cos(0.5f*angle) - 0.5 } 
    }
}