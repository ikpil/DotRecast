/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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

namespace DotRecast.Recast;

public class AreaModification
{
    public readonly int RC_AREA_FLAGS_MASK = 0x3F;
    
    private readonly int value;
    private readonly int mask;

    /**
     * Mask is set to all available bits, which means value is fully applied
     *
     * @param value
     *            The area id to apply. [Limit: &lt;= #RC_AREA_FLAGS_MASK]
     */
    public AreaModification(int value)
    {
        this.value = value;
        mask = RC_AREA_FLAGS_MASK;
    }

    /**
     *
     * @param value
     *            The area id to apply. [Limit: &lt;= #RC_AREA_FLAGS_MASK]
     * @param mask
     *            Bitwise mask used when applying value. [Limit: &lt;= #RC_AREA_FLAGS_MASK]
     */
    public AreaModification(int value, int mask)
    {
        this.value = value;
        this.mask = mask;
    }

    public AreaModification(AreaModification other)
    {
        value = other.value;
        mask = other.mask;
    }

    public int getMaskedValue()
    {
        return value & mask;
    }

    public int apply(int area)
    {
        return ((value & mask) | (area & ~mask));
    }
}