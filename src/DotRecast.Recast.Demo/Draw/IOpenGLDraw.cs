using DotRecast.Core;
using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public interface IOpenGLDraw
{
    void Init();

    void Clear();

    void Begin(DebugDrawPrimitives prim, float size);

    void End();

    void Vertex(float x, float y, float z, int color);

    void Vertex(float[] pos, int color);
    void Vertex(RcVec3f pos, int color);

    void Vertex(RcVec3f pos, int color, RcVec2f uv);

    void Vertex(float x, float y, float z, int color, float u, float v);

    void Fog(bool state);

    void DepthMask(bool state);

    void Texture(GLCheckerTexture g_tex, bool state);

    void ProjectionMatrix(float[] projectionMatrix);

    void ViewMatrix(float[] viewMatrix);

    void Fog(float start, float end);
}