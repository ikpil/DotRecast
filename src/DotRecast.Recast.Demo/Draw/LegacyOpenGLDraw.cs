using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public class LegacyOpenGLDraw : OpenGLDraw
{
    private GL _gl;

    public void fog(bool state)
    {
        // if (state) {
        //     _gl.Enable(GL_FOG);
        // } else {
        //     _gl.Disable(GL_FOG);
        // }
    }

    public void init(GL gl)
    {
        _gl = gl;

        // // Fog.
        // float fogDistance = 1000f;
        // float fogColor[] = { 0.32f, 0.31f, 0.30f, 1.0f };
        // glEnable(GL_FOG);
        // glFogi(GL_FOG_MODE, GL_LINEAR);
        // glFogf(GL_FOG_START, fogDistance * 0.1f);
        // glFogf(GL_FOG_END, fogDistance * 1.25f);
        // glFogfv(GL_FOG_COLOR, fogColor);
        // glDepthFunc(GL_LEQUAL);
    }

    public void clear()
    {
        // glClearColor(0.3f, 0.3f, 0.32f, 1.0f);
        // glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        // glEnable(GL_BLEND);
        // glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        // glDisable(GL_TEXTURE_2D);
        // glEnable(GL_DEPTH_TEST);
        // glEnable(GL_CULL_FACE);
    }

    public void projectionMatrix(float[] matrix)
    {
        // glMatrixMode(GL_PROJECTION);
        // glLoadMatrixf(matrix);
    }

    public void viewMatrix(float[] matrix)
    {
        // glMatrixMode(GL_MODELVIEW);
        // glLoadMatrixf(matrix);
    }

    public void begin(DebugDrawPrimitives prim, float size)
    {
        // switch (prim) {
        // case POINTS:
        //     glPointSize(size);
        //     glBegin(GL_POINTS);
        //     break;
        // case LINES:
        //     glLineWidth(size);
        //     glBegin(GL_LINES);
        //     break;
        // case TRIS:
        //     glBegin(GL_TRIANGLES);
        //     break;
        // case QUADS:
        //     glBegin(GL_QUADS);
        //     break;
        // }
    }

    public void vertex(float[] pos, int color)
    {
        // glColor4ubv(color);
        // glVertex3fv(pos);
    }

    public void vertex(float x, float y, float z, int color)
    {
        // glColor4ubv(color);
        // glVertex3f(x, y, z);
    }

    public void vertex(float[] pos, int color, float[] uv)
    {
        // glColor4ubv(color);
        // glTexCoord2fv(uv);
        // glVertex3fv(pos);
    }

    public void vertex(float x, float y, float z, int color, float u, float v)
    {
        // glColor4ubv(color);
        // glTexCoord2f(u, v);
        // glVertex3f(x, y, z);
    }

    private void glColor4ubv(int color)
    {
        // glColor4ub((byte) (color & 0xFF), (byte) ((color >> 8) & 0xFF), (byte) ((color >> 16) & 0xFF),
        //         (byte) ((color >> 24) & 0xFF));
    }

    public void depthMask(bool state)
    {
        // glDepthMask(state);
    }

    public void texture(GLCheckerTexture g_tex, bool state)
    {
        // if (state) {
        //     glEnable(GL_TEXTURE_2D);
        //     g_tex.bind();
        // } else {
        //     glDisable(GL_TEXTURE_2D);
        // }
    }

    public void end()
    {
        // glEnd();
        // glLineWidth(1.0f);
        // glPointSize(1.0f);
    }

    public void fog(float start, float end)
    {
        // glFogf(GL_FOG_START, start);
        // glFogf(GL_FOG_END, end);
    }
}