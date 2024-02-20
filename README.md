<h1 align="center">DotRecast</h1>
<p align="center">
<i>DotRecast is C# Recast & Detour, a port of <a href="https://github.com/recastnavigation/recastnavigation">recastnavigation</a> and <a href="https://github.com/ppiastucki/recast4j">recast4j</a> to the C# language.</i><br />
<i>If you'd like to support the project, we'd appreciate starring(⭐) our repos on Github for more visibility.</i> <p>
</p>

---

<p align="center">
<img alt="![GitHub License]" src="https://img.shields.io/github/license/ikpil/DotRecast?style=for-the-badge">
<img alt="Languages" src="https://img.shields.io/github/languages/top/ikpil/DotRecast?style=for-the-badge">
<img alt="GitHub repo size" src="https://img.shields.io/github/repo-size/ikpil/DotRecast?style=for-the-badge">
<a href="https://github.com/ikpil/DotRecast"><img alt="GitHub Repo stars" src="https://img.shields.io/github/stars/ikpil/DotRecast?style=for-the-badge&logo=github"></a>
<a href="https://github.com/ikpil/DotRecast/actions/workflows/dotnet.yml"><img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/ikpil/DotRecast/dotnet.yml?style=for-the-badge&logo=github"></a>
<a href="https://github.com/ikpil/DotRecast/actions/workflows/codeql.yml"><img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/ikpil/DotRecast/codeql.yml?style=for-the-badge&logo=github&label=CODEQL"></a>
<a href="https://github.com/ikpil/DotRecast/commits"><img alt="GitHub commit activity" src="https://img.shields.io/github/commit-activity/m/ikpil/DotRecast?style=for-the-badge&logo=github"></a>
<a href="https://github.com/ikpil/DotRecast/issues"><img alt="GitHub issues" src="https://img.shields.io/github/issues-raw/ikpil/DotRecast?style=for-the-badge&logo=github&color=44cc11"></a>
<a href="https://github.com/ikpil/DotRecast/issues"><img alt="GitHub closed issues" src="https://img.shields.io/github/issues-closed-raw/ikpil/DotRecast?style=for-the-badge&logo=github&color=a371f7"></a>
<a href="https://www.nuget.org/packages/DotRecast.Core"><img alt="NuGet Version" src="https://img.shields.io/nuget/vpre/DotRecast.Core?style=for-the-badge&logo=nuget"></a>
<a href="https://www.nuget.org/packages/DotRecast.Core"><img alt="NuGet Downloads" src="https://img.shields.io/nuget/dt/DotRecast.Core?style=for-the-badge&logo=nuget"></a>
<a href="https://visitorbadge.io/status?path=ikpil%2FDotRecast"><img alt="Visitors" src="https://api.visitorbadge.io/api/daily?path=ikpil%2FDotRecast&countColor=%23263759"></a>
<a href="https://github.com/sponsors/ikpil"><img alt="GitHub Sponsors" src="https://img.shields.io/github/sponsors/ikpil?style=for-the-badge&logo=GitHub-Sponsors&link=https%3A%2F%2Fgithub.com%2Fsponsors%2Fikpil"></a>
</p>

---

[![demo](https://user-images.githubusercontent.com/313821/266750582-8cf67832-1206-4b58-8c1f-7205210cbf22.gif)](https://youtu.be/zIFIgziKLhQ)



## 🚀 Features
 
- 🤖 Automatic - Recast can generate a navmesh from any level geometry you throw at it
- 🏎️ Fast - swift turnaround times for level designers
- 🧘 Flexible - detailed customization options and modular design let you tailor functionality to your specific needs
- 🚫 Dependency-Free - building Recast & Detour only requires a .NET compiler
- 💪 Industry Standard - Recast powers AI navigation features in Unity, Unreal, Godot, O3DE and countless AAA and indie games and engines

Recast Navigation is divided into multiple modules, each contained in its own folder:

- [DotRecast.Core](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Core) : Core utils
- [DotRecast.Recast](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast) : Navmesh generation
- [DotRecast.Detour](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour) : Runtime loading of navmesh data, pathfinding, navmesh queries
- [DotRecast.Detour.TileCache](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.TileCache) : Navmesh streaming. Useful for large levels and open-world games
- [DotRecast.Detour.Crowd](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.Crowd) : Agent movement, collision avoidance, and crowd simulation
- [DotRecast.Detour.Dynamic](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.Dynamic) : robust support for dynamic nav meshes combining pre-built voxels with dynamic objects which can be freely added and removed
- [DotRecast.Detour.Extras](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.Extras) : simple tool to import navmeshes created with [A* Pathfinding Project](https://arongranberg.com/astar/)
- [DotRecast.Recast.Toolset](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast.Toolset) : all modules
- [DotRecast.Recast.Demo](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast.Demo) : Standalone, comprehensive demo app showcasing all aspects of Recast & Detour's functionality
- [Tests](https://github.com/ikpil/DotRecast/tree/main/test) : Unit tests

## ⚡ Getting Started
 
- To build or integrate into your own project, please check out [BuildingAndIntegrating.md](https://github.com/ikpil/DotRecast/tree/main/BuildingAndIntegrating.md)
- To create a NavMesh, please check out [RecastSoloMeshTest.cs](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Recast.Test/RecastSoloMeshTest.cs)
- To test pathfinding, please check out [FindPathTest.cs](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Detour.Test/FindPathTest.cs)
- To watch the demo play video, please check out [Demo Video](#-demo-video)

## ⚙ How it Works

Recast constructs a navmesh through a multi-step mesh rasterization process.

1. First Recast rasterizes the input triangle meshes into voxels.
2. Voxels in areas where agents would not be able to move are filtered and removed.
3. The walkable areas described by the voxel grid are then divided into sets of polygonal regions.
4. The navigation polygons are generated by re-triangulating the generated polygonal regions into a navmesh.

You can use Recast to build a single navmesh, or a tiled navmesh.
Single meshes are suitable for many simple, static cases and are easy to work with.
Tiled navmeshes are more complex to work with but better support larger, more dynamic environments.  Tiled meshes enable advance Detour features like re-baking, heirarchical path-planning, and navmesh data-streaming.

## 📚 Documentation & Links

- DotRecast Links
  - [DotRecast/issues](https://github.com/ikpil/DotRecast/issues)
 
- Official Links
  - [recastnavigation/discussions](https://github.com/recastnavigation/recastnavigation/discussions)
  - [recastnav.com](https://recastnav.com)

## 🅾 License

DotRecast is licensed under ZLib license, see [LICENSE.txt](https://github.com/ikpil/DotRecast/tree/main/LICENSE.txt) for more information.

## 📹 Demo Video

[![demo](https://img.youtube.com/vi/zIFIgziKLhQ/0.jpg)](https://youtu.be/zIFIgziKLhQ)

[![demo](https://img.youtube.com/vi/CPvc19gNUEk/0.jpg)](https://youtu.be/CPvc19gNUEk)

[![demo](https://img.youtube.com/vi/pe5jpGUNPRg/0.jpg)](https://youtu.be/pe5jpGUNPRg)

