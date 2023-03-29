using DotRecast.Core;
using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public interface OpenGLDraw
{
    void init();

    void clear();

    void begin(DebugDrawPrimitives prim, float size);

    void end();

    void vertex(float x, float y, float z, int color);

    void vertex(float[] pos, int color);
    void vertex(Vector3f pos, int color);
    
    void vertex(float[] pos, int color, float[] uv);

    void vertex(float x, float y, float z, int color, float u, float v);

    void fog(bool state);

    void depthMask(bool state);

    void texture(GLCheckerTexture g_tex, bool state);

    void projectionMatrix(float[] projectionMatrix);

    void viewMatrix(float[] viewMatrix);

    void fog(float start, float end);
}