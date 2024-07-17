/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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
using System.Globalization;
using System.IO;
using System.Numerics;

namespace DotRecast.Core
{
    public static class RcObjImporter
    {
        [Obsolete("use 'LoadContext(Stream)' instead")]
        public static RcObjImporterContext LoadContext(byte[] chunk)
        {
            RcObjImporterContext context = new RcObjImporterContext();
            try
            {
                using StreamReader reader = new StreamReader(new MemoryStream(chunk));
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    ReadLine(line, context);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return context;
        }

        public static RcObjImporterContext LoadContext(Stream stream)
        {
            RcObjImporterContext context = new RcObjImporterContext();
            try
            {
                using StreamReader reader = new StreamReader(stream);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ReadLine(line, context);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return context;
        }


        public static void ReadLine(ReadOnlySpan<char> line, RcObjImporterContext context)
        {
            line = line.Trim();
            if (line.StartsWith("v"))
            {
                ReadVertex(line, context);
            }
            else if (line.StartsWith("f"))
            {
                ReadFace(line, context);
            }
        }

        private static void ReadVertex(ReadOnlySpan<char> line, RcObjImporterContext context)
        {
            if (line.StartsWith("v "))
            {
                var vert = ReadVector3f(line);
                context.vertexPositions.Add(vert.X);
                context.vertexPositions.Add(vert.Y);
                context.vertexPositions.Add(vert.Z);
            }
        }

        private static Vector3 ReadVector3f(ReadOnlySpan<char> line)
        {
            Span<Range> v = stackalloc Range[4];
            var n = line.Split(v, ' ', StringSplitOptions.RemoveEmptyEntries);
            if (n < 4)
            {
                throw new Exception("Invalid vector, expected 3 coordinates, found " + (n - 1));
            }

            // fix - https://github.com/ikpil/DotRecast/issues/7
            return new Vector3(
                float.Parse(line[v[1]], CultureInfo.InvariantCulture),
                float.Parse(line[v[2]], CultureInfo.InvariantCulture),
                float.Parse(line[v[3]], CultureInfo.InvariantCulture)
            );
        }

        private static void ReadFace(ReadOnlySpan<char> line, RcObjImporterContext context)
        {
            Span<Range> v = stackalloc Range[16]; // incase
            var n = line.Split(v, ' ', StringSplitOptions.RemoveEmptyEntries);
            if (n < 4)
            {
                throw new Exception("Invalid number of face vertices: 3 coordinates expected, found " + n);
            }

            for (int j = 0; j < n - 3; j++)
            {
                context.meshFaces.Add(ReadFaceVertex(line[v[1]], context));
                for (int i = 0; i < 2; i++)
                {
                    context.meshFaces.Add(ReadFaceVertex(line[v[2 + j + i]], context));
                }
            }
        }

        private static int ReadFaceVertex(ReadOnlySpan<char> face, RcObjImporterContext context)
        {
            Span<Range> v = stackalloc Range[2];
            var n = face.Split(v, '/');
            return GetIndex(int.Parse(face[v[0]]), context.vertexPositions.Count);
        }

        private static int GetIndex(int posi, int size)
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