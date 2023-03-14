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

using System.Runtime.InteropServices.JavaScript;

namespace DotRecast.Recast.Demo.UI;

public class NuklearUIHelper {

    // public static void nk_color_rgb(IWindow ctx, NkColorf color) {
    //     try (MemoryStack stack = stackPush()) {
    //         if (nk_combo_begin_color(ctx, nk_rgb_cf(color, NkColor.mallocStack(stack)),
    //                 NkVec2.mallocStack(stack).set(nk_widget_width(ctx), 400))) {
    //             nk_layout_row_dynamic(ctx, 120, 1);
    //             nk_color_picker(ctx, color, NK_RGB);
    //             nk_layout_row_dynamic(ctx, 20, 1);
    //             color.r(nk_propertyf(ctx, "#R:", 0, color.r(), 1f, 0.01f, 0.005f));
    //             color.g(nk_propertyf(ctx, "#G:", 0, color.g(), 1f, 0.01f, 0.005f));
    //             color.b(nk_propertyf(ctx, "#B:", 0, color.b(), 1f, 0.01f, 0.005f));
    //             nk_combo_end(ctx);
    //         }
    //     }
    // }
    //
    // public static <T extends Enum<?>> T nk_radio(IWindow ctx, T[] values, T currentValue, JSType.Function<T, string> nameFormatter) {
    //     try (MemoryStack stack = stackPush()) {
    //         foreach (T v in values) {
    //             nk_layout_row_dynamic(ctx, 20, 1);
    //             if (nk_option_text(ctx, nameFormatter.apply(v), currentValue == v)) {
    //                 currentValue = v;
    //             }
    //         }
    //     }
    //     return currentValue;
    // }
    //
    // public static <T extends Enum<?>> T nk_combo(IWindow ctx, T[] values, T currentValue) {
    //     try (MemoryStack stack = stackPush()) {
    //         if (nk_combo_begin_label(ctx, currentValue.toString(), NkVec2.mallocStack(stack).set(nk_widget_width(ctx), 200))) {
    //             nk_layout_row_dynamic(ctx, 20, 1);
    //             foreach (T v in values) {
    //                 if (nk_combo_item_label(ctx, v.toString(), NK_TEXT_LEFT)) {
    //                     currentValue = v;
    //                 }
    //             }
    //             nk_combo_end(ctx);
    //         }
    //     }
    //     return currentValue;
    // }
    //
    // public static NkColorf nk_colorf(int r, int g, int b) {
    //     NkColorf color = NkColorf.create();
    //     color.r(r / 255f);
    //     color.g(g / 255f);
    //     color.b(b / 255f);
    //     return color;
    // }
}
