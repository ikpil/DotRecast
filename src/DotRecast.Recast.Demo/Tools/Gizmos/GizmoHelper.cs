using System;
using DotRecast.Recast.Demo.Draw;
using static DotRecast.Detour.DetourCommon;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class GizmoHelper
{
    private static readonly int SEGMENTS = 16;
    private static readonly int RINGS = 8;

    private static float[] sphericalVertices;

    public static float[] generateSphericalVertices()
    {
        if (sphericalVertices == null)
        {
            sphericalVertices = generateSphericalVertices(SEGMENTS, RINGS);
        }

        return sphericalVertices;
    }

    private static float[] generateSphericalVertices(int segments, int rings)
    {
        float[] vertices = new float[6 + 3 * (segments + 1) * (rings + 1)];
        // top
        int vi = 0;
        vertices[vi++] = 0;
        vertices[vi++] = 1;
        vertices[vi++] = 0;
        for (int r = 0; r <= rings; r++)
        {
            double theta = Math.PI * (r + 1) / (rings + 2);
            vi = generateRingVertices(segments, vertices, vi, theta);
        }

        // bottom
        vertices[vi++] = 0;
        vertices[vi++] = -1;
        vertices[vi++] = 0;
        return vertices;
    }

    public static float[] generateCylindricalVertices()
    {
        return generateCylindricalVertices(SEGMENTS);
    }

    private static float[] generateCylindricalVertices(int segments)
    {
        float[] vertices = new float[3 * (segments + 1) * 4];
        int vi = 0;
        for (int r = 0; r < 4; r++)
        {
            vi = generateRingVertices(segments, vertices, vi, Math.PI * 0.5);
        }

        return vertices;
    }

    private static int generateRingVertices(int segments, float[] vertices, int vi, double theta)
    {
        double cosTheta = Math.Cos(theta);
        double sinTheta = Math.Sin(theta);
        for (int p = 0; p <= segments; p++)
        {
            double phi = 2 * Math.PI * p / segments;
            double cosPhi = Math.Cos(phi);
            double sinPhi = Math.Sin(phi);
            vertices[vi++] = (float)(sinTheta * cosPhi);
            vertices[vi++] = (float)cosTheta;
            vertices[vi++] = (float)(sinTheta * sinPhi);
        }

        return vi;
    }

    public static int[] generateSphericalTriangles()
    {
        return generateSphericalTriangles(SEGMENTS, RINGS);
    }

    private static int[] generateSphericalTriangles(int segments, int rings)
    {
        int[] triangles = new int[6 * (segments + rings * (segments + 1))];
        int ti = generateSphereUpperCapTriangles(segments, triangles, 0);
        ti = generateRingTriangles(segments, rings, triangles, 1, ti);
        generateSphereLowerCapTriangles(segments, rings, triangles, ti);
        return triangles;
    }

    public static int generateRingTriangles(int segments, int rings, int[] triangles, int vertexOffset, int ti)
    {
        for (int r = 0; r < rings; r++)
        {
            for (int p = 0; p < segments; p++)
            {
                int current = p + r * (segments + 1) + vertexOffset;
                int next = p + 1 + r * (segments + 1) + vertexOffset;
                int currentBottom = p + (r + 1) * (segments + 1) + vertexOffset;
                int nextBottom = p + 1 + (r + 1) * (segments + 1) + vertexOffset;
                triangles[ti++] = current;
                triangles[ti++] = next;
                triangles[ti++] = nextBottom;
                triangles[ti++] = current;
                triangles[ti++] = nextBottom;
                triangles[ti++] = currentBottom;
            }
        }

        return ti;
    }

    private static int generateSphereUpperCapTriangles(int segments, int[] triangles, int ti)
    {
        for (int p = 0; p < segments; p++)
        {
            triangles[ti++] = p + 2;
            triangles[ti++] = p + 1;
            triangles[ti++] = 0;
        }

        return ti;
    }

    private static void generateSphereLowerCapTriangles(int segments, int rings, int[] triangles, int ti)
    {
        int lastVertex = 1 + (segments + 1) * (rings + 1);
        for (int p = 0; p < segments; p++)
        {
            triangles[ti++] = lastVertex;
            triangles[ti++] = lastVertex - (p + 2);
            triangles[ti++] = lastVertex - (p + 1);
        }
    }

    public static int[] generateCylindricalTriangles()
    {
        return generateCylindricalTriangles(SEGMENTS);
    }

    private static int[] generateCylindricalTriangles(int segments)
    {
        int circleTriangles = segments - 2;
        int[] triangles = new int[6 * (circleTriangles + (segments + 1))];
        int vi = 0;
        int ti = generateCircleTriangles(segments, triangles, vi, 0, false);
        ti = generateRingTriangles(segments, 1, triangles, segments + 1, ti);
        int vertexCount = (segments + 1) * 4;
        ti = generateCircleTriangles(segments, triangles, vertexCount - segments, ti, true);
        return triangles;
    }

    private static int generateCircleTriangles(int segments, int[] triangles, int vi, int ti, bool invert)
    {
        for (int p = 0; p < segments - 2; p++)
        {
            if (invert)
            {
                triangles[ti++] = vi;
                triangles[ti++] = vi + p + 1;
                triangles[ti++] = vi + p + 2;
            }
            else
            {
                triangles[ti++] = vi + p + 2;
                triangles[ti++] = vi + p + 1;
                triangles[ti++] = vi;
            }
        }

        return ti;
    }

    public static int getColorByNormal(float[] vertices, int v0, int v1, int v2)
    {
        float[] e0 = new float[3], e1 = new float[3];
        float[] normal = new float[3];
        for (int j = 0; j < 3; ++j)
        {
            e0[j] = vertices[v1 + j] - vertices[v0 + j];
            e1[j] = vertices[v2 + j] - vertices[v0 + j];
        }

        normal[0] = e0[1] * e1[2] - e0[2] * e1[1];
        normal[1] = e0[2] * e1[0] - e0[0] * e1[2];
        normal[2] = e0[0] * e1[1] - e0[1] * e1[0];
        RecastVectors.normalize(normal);
        float c = clamp(0.57735026f * (normal[0] + normal[1] + normal[2]), -1, 1);
        int col = DebugDraw.duLerpCol(DebugDraw.duRGBA(32, 32, 0, 160), DebugDraw.duRGBA(220, 220, 0, 160),
            (int)(127 * (1 + c)));
        return col;
    }
}