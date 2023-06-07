using System;
using System.IO;
using DotRecast.Recast.DemoTool.Geom;

namespace DotRecast.Recast.Demo.Tools;

public static class DemoObjImporter
{
    public static DemoInputGeomProvider Load(byte[] chunk)
    {
        var context = ObjImporter.LoadContext(chunk);
        return new DemoInputGeomProvider(context.vertexPositions, context.meshFaces);
    }
}