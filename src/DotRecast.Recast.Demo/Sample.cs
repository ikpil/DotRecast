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

using System.Collections.Generic;
using DotRecast.Detour;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using DotRecast.Recast.Demo.Settings;

namespace DotRecast.Recast.Demo;

public class Sample
{
    private DemoInputGeomProvider inputGeom;
    private NavMesh navMesh;
    private NavMeshQuery navMeshQuery;
    private readonly RcSettingsView _settingsView;
    private IList<RecastBuilderResult> recastResults;
    private bool changed;

    public Sample(DemoInputGeomProvider inputGeom, IList<RecastBuilderResult> recastResults, NavMesh navMesh,
        RcSettingsView settingsView, RecastDebugDraw debugDraw)
    {
        this.inputGeom = inputGeom;
        this.recastResults = recastResults;
        this.navMesh = navMesh;
        _settingsView = settingsView;
        setQuery(navMesh);
        changed = true;
    }

    private void setQuery(NavMesh navMesh)
    {
        navMeshQuery = navMesh != null ? new NavMeshQuery(navMesh) : null;
    }

    public DemoInputGeomProvider getInputGeom()
    {
        return inputGeom;
    }

    public IList<RecastBuilderResult> getRecastResults()
    {
        return recastResults;
    }

    public NavMesh getNavMesh()
    {
        return navMesh;
    }

    public RcSettingsView getSettingsUI()
    {
        return _settingsView;
    }

    public NavMeshQuery getNavMeshQuery()
    {
        return navMeshQuery;
    }

    public bool isChanged()
    {
        return changed;
    }

    public void setChanged(bool changed)
    {
        this.changed = changed;
    }

    public void update(DemoInputGeomProvider geom, IList<RecastBuilderResult> recastResults, NavMesh navMesh)
    {
        inputGeom = geom;
        this.recastResults = recastResults;
        this.navMesh = navMesh;
        setQuery(navMesh);
        changed = true;
    }
}