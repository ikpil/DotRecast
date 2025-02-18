using DotRecast.Core.Numerics;

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

    void ProjectionMatrix(ref RcMatrix4x4f projectionMatrix);

    void ViewMatrix(ref RcMatrix4x4f viewMatrix);

    void Fog(float start, float end);
}