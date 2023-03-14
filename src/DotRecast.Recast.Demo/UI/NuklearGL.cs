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

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;
using Microsoft.DotNet.PlatformAbstractions;

namespace DotRecast.Recast.Demo.UI;


public class NuklearGL {

    private static readonly int BUFFER_INITIAL_SIZE = 4 * 1024;
    private static readonly int MAX_VERTEX_BUFFER = 512 * 1024;
    private static readonly int MAX_ELEMENT_BUFFER = 128 * 1024;
    private static readonly int FONT_BITMAP_W = 1024;
    private static readonly int FONT_BITMAP_H = 1024;
    private static readonly int FONT_HEIGHT = 15;

    private readonly NuklearUI context;
    // private readonly NkDrawNullTexture null_texture = NkDrawNullTexture.create();
    // private readonly NkBuffer cmds = NkBuffer.create();
    // private readonly NkUserFont default_font;
    private readonly int program;
    private readonly int uniform_tex;
    private readonly int uniform_proj;
    private readonly int vbo;
    private readonly int ebo;
    private readonly int vao;
    //private readonly Buffer vertexLayout;

    public NuklearGL(NuklearUI context) {
        this.context = context;
        // nk_buffer_init(cmds, context.allocator, BUFFER_INITIAL_SIZE);
        // vertexLayout = NkDrawVertexLayoutElement.create(4)//
        //         .position(0).attribute(NK_VERTEX_POSITION).format(NK_FORMAT_FLOAT).offset(0)//
        //         .position(1).attribute(NK_VERTEX_TEXCOORD).format(NK_FORMAT_FLOAT).offset(8)//
        //         .position(2).attribute(NK_VERTEX_COLOR).format(NK_FORMAT_R8G8B8A8).offset(16)//
        //         .position(3).attribute(NK_VERTEX_ATTRIBUTE_COUNT).format(NK_FORMAT_COUNT).offset(0)//
        //         .flip();
        // string NK_SHADER_VERSION = Platform.get() == Platform.MACOSX ? "#version 150\n" : "#version 300 es\n";
        // string vertex_shader = NK_SHADER_VERSION + "uniform mat4 ProjMtx;\n" + "in vec2 Position;\n"
        //         + "in vec2 TexCoord;\n" + "in vec4 Color;\n" + "out vec2 Frag_UV;\n" + "out vec4 Frag_Color;\n"
        //         + "void main() {\n" + "   Frag_UV = TexCoord;\n" + "   Frag_Color = Color;\n"
        //         + "   gl_Position = ProjMtx * vec4(Position.xy, 0, 1);\n" + "}\n";
        // string fragment_shader = NK_SHADER_VERSION + "precision mediump float;\n" + "uniform sampler2D Texture;\n"
        //         + "in vec2 Frag_UV;\n" + "in vec4 Frag_Color;\n" + "out vec4 Out_Color;\n" + "void main(){\n"
        //         + "   Out_Color = Frag_Color * texture(Texture, Frag_UV.st);\n" + "}\n";
        //
        // program = glCreateProgram();
        // int vert_shdr = glCreateShader(GL_VERTEX_SHADER);
        // int frag_shdr = glCreateShader(GL_FRAGMENT_SHADER);
        // glShaderSource(vert_shdr, vertex_shader);
        // glShaderSource(frag_shdr, fragment_shader);
        // glCompileShader(vert_shdr);
        // glCompileShader(frag_shdr);
        // if (glGetShaderi(vert_shdr, GL_COMPILE_STATUS) != GL_TRUE) {
        //     throw new IllegalStateException();
        // }
        // if (glGetShaderi(frag_shdr, GL_COMPILE_STATUS) != GL_TRUE) {
        //     throw new IllegalStateException();
        // }
        // glAttachShader(program, vert_shdr);
        // glAttachShader(program, frag_shdr);
        // glLinkProgram(program);
        // if (glGetProgrami(program, GL_LINK_STATUS) != GL_TRUE) {
        //     throw new IllegalStateException();
        // }
        //
        // uniform_tex = glGetUniformLocation(program, "Texture");
        // uniform_proj = glGetUniformLocation(program, "ProjMtx");
        // int attrib_pos = glGetAttribLocation(program, "Position");
        // int attrib_uv = glGetAttribLocation(program, "TexCoord");
        // int attrib_col = glGetAttribLocation(program, "Color");
        //
        // // buffer setup
        // vbo = glGenBuffers();
        // ebo = glGenBuffers();
        // vao = glGenVertexArrays();
        //
        // glBindVertexArray(vao);
        // glBindBuffer(GL_ARRAY_BUFFER, vbo);
        // glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        //
        // glEnableVertexAttribArray(attrib_pos);
        // glEnableVertexAttribArray(attrib_uv);
        // glEnableVertexAttribArray(attrib_col);
        //
        // glVertexAttribPointer(attrib_pos, 2, GL_FLOAT, false, 20, 0);
        // glVertexAttribPointer(attrib_uv, 2, GL_FLOAT, false, 20, 8);
        // glVertexAttribPointer(attrib_col, 4, GL_UNSIGNED_BYTE, true, 20, 16);
        //
        // // null texture setup
        // int nullTexID = glGenTextures();
        //
        // null_texture.texture().id(nullTexID);
        // null_texture.uv().set(0.5f, 0.5f);
        //
        // glBindTexture(GL_TEXTURE_2D, nullTexID);
        // try (MemoryStack stack = stackPush()) {
        //     glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, 1, 1, 0, GL_RGBA, GL_UNSIGNED_INT_8_8_8_8_REV,
        //             stack.ints(0xFFFFFFFF));
        // }
        // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
        // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
        //
        // glBindTexture(GL_TEXTURE_2D, 0);
        // glBindBuffer(GL_ARRAY_BUFFER, 0);
        // glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        // glBindVertexArray(0);
        // default_font = setupFont();
        // nk_style_set_font(context.ctx, default_font);
    }

    private long setupFont() {
        return 0;
        // NkUserFont font = NkUserFont.create();
        // ByteBuffer ttf;
        // try {
        //     ttf = IOUtils.toByteBuffer(Loader.ToBytes("fonts/DroidSans.ttf"), true);
        // } catch (IOException e) {
        //     throw new RuntimeException(e);
        // }
        //
        // int fontTexID = glGenTextures();
        //
        // STBTTFontinfo fontInfo = STBTTFontinfo.malloc();
        // STBTTPackedchar.Buffer cdata = STBTTPackedchar.create(95);
        //
        // float scale;
        // float descent;
        //
        // try (MemoryStack stack = stackPush()) {
        //     stbtt_InitFont(fontInfo, ttf);
        //     scale = stbtt_ScaleForPixelHeight(fontInfo, FONT_HEIGHT);
        //
        //     int[] d = stack.mallocInt(1);
        //     stbtt_GetFontVMetrics(fontInfo, null, d, null);
        //     descent = d[0] * scale;
        //
        //     ByteBuffer bitmap = memAlloc(FONT_BITMAP_W * FONT_BITMAP_H);
        //
        //     STBTTPackContext pc = STBTTPackContext.mallocStack(stack);
        //     stbtt_PackBegin(pc, bitmap, FONT_BITMAP_W, FONT_BITMAP_H, 0, 1);
        //     stbtt_PackSetOversampling(pc, 1, 1);
        //     stbtt_PackFontRange(pc, ttf, 0, FONT_HEIGHT, 32, cdata);
        //     stbtt_PackEnd(pc);
        //
        //     // Convert R8 to RGBA8
        //     ByteBuffer texture = memAlloc(FONT_BITMAP_W * FONT_BITMAP_H * 4);
        //     for (int i = 0; i < bitmap.capacity(); i++) {
        //         texture.putInt((bitmap[i] << 24) | 0x00FFFFFF);
        //     }
        //     texture.flip();
        //
        //     glBindTexture(GL_TEXTURE_2D, fontTexID);
        //     glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, FONT_BITMAP_W, FONT_BITMAP_H, 0, GL_RGBA,
        //             GL_UNSIGNED_INT_8_8_8_8_REV, texture);
        //     glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
        //     glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        //
        //     memFree(texture);
        //     memFree(bitmap);
        // }
        // int[] cache = new int[1024];
        // try (MemoryStack stack = stackPush()) {
        //     int[] advance = stack.mallocInt(1);
        //     for (int i = 0; i < 1024; i++) {
        //         stbtt_GetCodepointHMetrics(fontInfo, i, advance, null);
        //         cache[i] = advance[0];
        //     }
        // }
        // font.width((handle, h, text, len) => {
        //     float text_width = 0;
        //     try (MemoryStack stack = stackPush()) {
        //         int[] unicode = stack.mallocInt(1);
        //
        //         int glyph_len = nnk_utf_decode(text, memAddress(unicode), len);
        //         int text_len = glyph_len;
        //
        //         if (glyph_len == 0) {
        //             return 0;
        //         }
        //
        //         int[] advance = stack.mallocInt(1);
        //         while (text_len <= len && glyph_len != 0) {
        //             if (unicode[0] == NK_UTF_INVALID) {
        //                 break;
        //             }
        //
        //             /* query currently drawn glyph information */
        //             // stbtt_GetCodepointHMetrics(fontInfo, unicode[0], advance, null);
        //             // text_width += advance[0] * scale;
        //
        //             text_width += cache[unicode[0]] * scale;
        //             /* offset next glyph */
        //             glyph_len = nnk_utf_decode(text + text_len, memAddress(unicode), len - text_len);
        //             text_len += glyph_len;
        //         }
        //     }
        //     return text_width;
        // }).height(FONT_HEIGHT).query((handle, font_height, glyph, codepoint, next_codepoint) => {
        //     try (MemoryStack stack = stackPush()) {
        //         float[] x = stack.floats(0.0f);
        //         float[] y = stack.floats(0.0f);
        //
        //         STBTTAlignedQuad q = STBTTAlignedQuad.mallocStack(stack);
        //         // int[] advance = stack.mallocInt(1);
        //
        //         stbtt_GetPackedQuad(cdata, FONT_BITMAP_W, FONT_BITMAP_H, codepoint - 32, x, y, q, false);
        //         // stbtt_GetCodepointHMetrics(fontInfo, codepoint, advance, null);
        //
        //         NkUserFontGlyph ufg = NkUserFontGlyph.create(glyph);
        //
        //         ufg.width(q.x1() - q.x0());
        //         ufg.height(q.y1() - q.y0());
        //         ufg.offset().set(q.x0(), q.y0() + (FONT_HEIGHT + descent));
        //         // ufg.xadvance(advance[0] * scale);
        //         ufg.xadvance(cache[codepoint] * scale);
        //         ufg.uv(0).set(q.s0(), q.t0());
        //         ufg.uv(1).set(q.s1(), q.t1());
        //     }
        // }).texture().id(fontTexID);
        // return font;
    }

    void render(long win, int AA) {
        // int width;
        // int height;
        // int display_width;
        // int display_height;
        // try (MemoryStack stack = stackPush()) {
        //     int[] w = stack.mallocInt(1);
        //     int[] h = stack.mallocInt(1);
        //
        //     glfwGetWindowSize(win, w, h);
        //     width = w[0];
        //     height = h[0];
        //
        //     glfwGetFramebufferSize(win, w, h);
        //     display_width = w[0];
        //     display_height = h[0];
        // }
        //
        // try (MemoryStack stack = stackPush()) {
        //     // setup global state
        //     glEnable(GL_BLEND);
        //     glBlendEquation(GL_FUNC_ADD);
        //     glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        //     glDisable(GL_CULL_FACE);
        //     glDisable(GL_DEPTH_TEST);
        //     glEnable(GL_SCISSOR_TEST);
        //     glActiveTexture(GL_TEXTURE0);
        //
        //     glUseProgram(program);
        //     // setup program
        //     glUniform1i(uniform_tex, 0);
        //     glUniformMatrix4fv(uniform_proj, false, stack.floats(2.0f / width, 0.0f, 0.0f, 0.0f, 0.0f, -2.0f / height,
        //             0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, -1.0f, 1.0f, 0.0f, 1.0f));
        //     glViewport(0, 0, display_width, display_height);
        // }
        // // allocate vertex and element buffer
        // glBindVertexArray(vao);
        // glBindBuffer(GL_ARRAY_BUFFER, vbo);
        // glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        //
        // glBufferData(GL_ARRAY_BUFFER, MAX_VERTEX_BUFFER, GL_STREAM_DRAW);
        // glBufferData(GL_ELEMENT_ARRAY_BUFFER, MAX_ELEMENT_BUFFER, GL_STREAM_DRAW);
        //
        // // load draw vertices & elements directly into vertex + element buffer
        // ByteBuffer vertices = glMapBuffer(GL_ARRAY_BUFFER, GL_WRITE_ONLY, MAX_VERTEX_BUFFER, null);
        // ByteBuffer elements = glMapBuffer(GL_ELEMENT_ARRAY_BUFFER, GL_WRITE_ONLY, MAX_ELEMENT_BUFFER, null);
        //
        // try (MemoryStack stack = stackPush()) {
        //     // fill convert configuration
        //     NkConvertConfig config = NkConvertConfig.callocStack(stack).vertex_layout(vertexLayout).vertex_size(20)
        //             .vertex_alignment(4).null_texture(null_texture).circle_segment_count(22).curve_segment_count(22)
        //             .arc_segment_count(22).global_alpha(1f).shape_AA(AA).line_AA(AA);
        //
        //     // setup buffers to load vertices and elements
        //     NkBuffer vbuf = NkBuffer.mallocStack(stack);
        //     NkBuffer ebuf = NkBuffer.mallocStack(stack);
        //
        //     nk_buffer_init_fixed(vbuf, vertices/* , max_vertex_buffer */);
        //     nk_buffer_init_fixed(ebuf, elements/* , max_element_buffer */);
        //     nk_convert(context.ctx, cmds, vbuf, ebuf, config);
        // }
        // glUnmapBuffer(GL_ELEMENT_ARRAY_BUFFER);
        // glUnmapBuffer(GL_ARRAY_BUFFER);
        //
        // // iterate over and execute each draw command
        // float fb_scale_x = (float) display_width / (float) width;
        // float fb_scale_y = (float) display_height / (float) height;
        //
        // long offset = NULL;
        // for (NkDrawCommand cmd = nk__draw_begin(context.ctx, cmds); cmd != null; cmd = nk__draw_next(cmd, cmds,
        //         context.ctx)) {
        //     if (cmd.elem_count() == 0) {
        //         continue;
        //     }
        //     glBindTexture(GL_TEXTURE_2D, cmd.texture().id());
        //     glScissor((int) (cmd.clip_rect().x() * fb_scale_x),
        //             (int) ((height - (int) (cmd.clip_rect().y() + cmd.clip_rect().h())) * fb_scale_y),
        //             (int) (cmd.clip_rect().w() * fb_scale_x), (int) (cmd.clip_rect().h() * fb_scale_y));
        //     glDrawElements(GL_TRIANGLES, cmd.elem_count(), GL_UNSIGNED_SHORT, offset);
        //     offset += cmd.elem_count() * 2;
        // }
        // nk_clear(context.ctx);
        // glUseProgram(0);
        // glBindTexture(GL_TEXTURE_2D, 0);
        // glBindBuffer(GL_ARRAY_BUFFER, 0);
        // glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        // glBindVertexArray(0);
        // glDisable(GL_BLEND);
        // glDisable(GL_SCISSOR_TEST);
    }
}
