/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using DotRecast.Recast.Geom;

namespace DotRecast.Recast
{
    public static class ObjImporter
    {
        public class ObjImporterContext
        {
            public List<float> vertexPositions = new List<float>();
            public List<int> meshFaces = new List<int>();
        }

        public static InputGeomProvider load(byte[] chunck)
        {
            var context = loadContext(chunck);
            return new SimpleInputGeomProvider(context.vertexPositions, context.meshFaces);
        }

        public static ObjImporterContext loadContext(byte[] chunck)
        {
            ObjImporterContext context = new ObjImporterContext();
            try
            {
                using StreamReader reader = new StreamReader(new MemoryStream(chunck));
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    readLine(line, context);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return context;
        }


        public static void readLine(string line, ObjImporterContext context)
        {
            if (line.StartsWith("v"))
            {
                readVertex(line, context);
            }
            else if (line.StartsWith("f"))
            {
                readFace(line, context);
            }
        }

        private static void readVertex(string line, ObjImporterContext context)
        {
            if (line.StartsWith("v "))
            {
                float[] vert = readVector3f(line);
                foreach (float vp in vert)
                {
                    context.vertexPositions.Add(vp);
                }
            }
        }

        private static float[] readVector3f(string line)
        {
            string[] v = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (v.Length < 4)
            {
                throw new Exception("Invalid vector, expected 3 coordinates, found " + (v.Length - 1));
            }

            return new float[] { float.Parse(v[1]), float.Parse(v[2]), float.Parse(v[3]) };
        }

        private static void readFace(string line, ObjImporterContext context)
        {
            string[] v = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (v.Length < 4)
            {
                throw new Exception("Invalid number of face vertices: 3 coordinates expected, found " + v.Length);
            }

            for (int j = 0; j < v.Length - 3; j++)
            {
                context.meshFaces.Add(readFaceVertex(v[1], context));
                for (int i = 0; i < 2; i++)
                {
                    context.meshFaces.Add(readFaceVertex(v[2 + j + i], context));
                }
            }
        }

        private static int readFaceVertex(string face, ObjImporterContext context)
        {
            string[] v = face.Split("/");
            return getIndex(int.Parse(v[0]), context.vertexPositions.Count);
        }

        private static int getIndex(int posi, int size)
        {
            if (posi > 0)
            {
                posi--;
            }
            else if (posi < 0)
            {
                posi = size + posi;
            }
            else
            {
                throw new Exception("0 vertex index");
            }

            return posi;
        }
    }
}
