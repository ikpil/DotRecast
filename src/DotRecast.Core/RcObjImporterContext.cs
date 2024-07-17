using System.Collections.Generic;

namespace DotRecast.Core
{
    public class RcObjImporterContext
    {
        public List<float> vertexPositions;
        public List<int> meshFaces;

        public RcObjImporterContext() : this(1024 * 1000)
        { }

        public RcObjImporterContext(int capcatiy)
        {
            vertexPositions = new List<float>(capcatiy * 3);
            meshFaces = new List<int>(capcatiy);
        }
    }
}