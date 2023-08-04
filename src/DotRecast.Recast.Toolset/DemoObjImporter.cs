using System;
using System.IO;
using DotRecast.Recast.Toolset.Geom;

namespace DotRecast.Recast.Toolset
{
    public static class DemoObjImporter
    {
        public static DemoInputGeomProvider Load(byte[] chunk)
        {
            var context = ObjImporter.LoadContext(chunk);
            return new DemoInputGeomProvider(context.vertexPositions, context.meshFaces);
        }
    }
}