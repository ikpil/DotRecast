using System;
using System.IO;
using DotRecast.Recast.Demo.Geom;

namespace DotRecast.Recast.Demo.Tools;

public static class DemoObjImporter
{
    public static DemoInputGeomProvider load(byte[] chunk)
    {
        var context = ObjImporter.loadContext(chunk);
        return new DemoInputGeomProvider(context.vertexPositions, context.meshFaces);
    }
}