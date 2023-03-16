using System.Collections.Generic;
using DotRecast.Recast;
using static DotRecast.Recast.RecastConstants;


namespace DotRecast.Detour.Extras.Jumplink
{
    public class EdgeExtractor
    {
        public Edge[] extractEdges(PolyMesh mesh)
        {
            List<Edge> edges = new List<Edge>();
            if (mesh != null)
            {
                float[] orig = mesh.bmin;
                float cs = mesh.cs;
                float ch = mesh.ch;
                for (int i = 0; i < mesh.npolys; i++)
                {
                    if (i > 41 || i < 41)
                    {
                        //           continue;
                    }

                    int nvp = mesh.nvp;
                    int p = i * 2 * nvp;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (j != 1)
                        {
                            //                    continue;
                        }

                        if (mesh.polys[p + j] == RC_MESH_NULL_IDX)
                        {
                            break;
                        }

                        // Skip connected edges.
                        if ((mesh.polys[p + nvp + j] & 0x8000) != 0)
                        {
                            int dir = mesh.polys[p + nvp + j] & 0xf;
                            if (dir == 0xf)
                            {
                                // Border
                                if (mesh.polys[p + nvp + j] != RC_MESH_NULL_IDX)
                                {
                                    continue;
                                }

                                int nj = j + 1;
                                if (nj >= nvp || mesh.polys[p + nj] == RC_MESH_NULL_IDX)
                                {
                                    nj = 0;
                                }

                                int va = mesh.polys[p + j] * 3;
                                int vb = mesh.polys[p + nj] * 3;
                                Edge e = new Edge();
                                e.sp[0] = orig[0] + mesh.verts[vb] * cs;
                                e.sp[1] = orig[1] + mesh.verts[vb + 1] * ch;
                                e.sp[2] = orig[2] + mesh.verts[vb + 2] * cs;
                                e.sq[0] = orig[0] + mesh.verts[va] * cs;
                                e.sq[1] = orig[1] + mesh.verts[va + 1] * ch;
                                e.sq[2] = orig[2] + mesh.verts[va + 2] * cs;
                                edges.Add(e);
                            }
                        }
                    }
                }
            }

            return edges.ToArray();
        }
    }
}