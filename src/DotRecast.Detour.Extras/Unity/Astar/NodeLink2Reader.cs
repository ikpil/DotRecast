/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

using System.IO.Compression;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Unity.Astar
{
    public class NodeLink2Reader : ZipBinaryReader
    {
        public NodeLink2[] Read(ZipArchive file, string filename, int[] indexToNode)
        {
            ByteBuffer buffer = ToByteBuffer(file, filename);
            int linkCount = buffer.GetInt();
            NodeLink2[] links = new NodeLink2[linkCount];
            for (int i = 0; i < linkCount; i++)
            {
                long linkID = buffer.GetLong();
                int startNode = indexToNode[buffer.GetInt()];
                int endNode = indexToNode[buffer.GetInt()];
                int connectedNode1 = buffer.GetInt();
                int connectedNode2 = buffer.GetInt();
                Vector3f clamped1 = new Vector3f();
                clamped1.x = buffer.GetFloat();
                clamped1.y = buffer.GetFloat();
                clamped1.z = buffer.GetFloat();
                Vector3f clamped2 = new Vector3f();
                clamped2.x = buffer.GetFloat();
                clamped2.y = buffer.GetFloat();
                clamped2.z = buffer.GetFloat();
                bool postScanCalled = buffer.Get() != 0;
                links[i] = new NodeLink2(linkID, startNode, endNode, clamped1, clamped2);
            }

            return links;
        }
    }
}