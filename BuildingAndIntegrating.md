
## 🔨 Build
- Building requires only .NET 8 SDK.

### 🔨 Building with Command Prompt

```shell
dotnet build -c Release
```

### 🔨 Building with an IDE

1. Open IDE: Launch your C# IDE (e.g., Visual Studio).
2. Open Solution: Go to the "File" menu and select "Open Solution."
3. Build: In the IDE menu, select "Build" > "Build Solution" or click the "Build" icon on the toolbar.

## ▶️ Run
- To verify the run for all modules, run [DotRecast.Recast.Demo](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast.Demo/DotRecast.Recast.Demo.csproj)
- on windows requirement : install to [Microsoft Visual C++ Redistributable Package](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) 

### ▶️ Running With Command Prompt

```shell
dotnet run --project src/DotRecast.Recast.Demo --framework net8.0 -c Release
```

### ▶️ Running With IDE (ex. Visual Studio 2022 or Rider ...)

1. Open your C# IDE (like Visual Studio).
2. Go to "File" in the menu.
3. Choose "Open Project" or "Open Solution."
4. Find and select [DotRecast.sln](DotRecast.sln), then click "Open."
5. Run to [DotRecast.Recast.Demo](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast.Demo/DotRecast.Recast.Demo.csproj)

## 🧪 Running Unit Test

- [DotRecast.Core.Test](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Core.Test) : ...
- [DotRecast.Recast.Test](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Recast.Test) : ...
- [DotRecast.Detour.Test](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Detour.Test) : ...
- [DotRecast.Detour.TileCache.Test](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Detour.TileCache.Test) : ...
- [DotRecast.Detour.Crowd.Test](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Detour.Crowd.Test) : ...
- [DotRecast.Detour.Dynamic.Test](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Detour.Dynamic.Test) : ...
- [DotRecast.Detour.Extras.Test](https://github.com/ikpil/DotRecast/tree/main/test/DotRecast.Detour.Extras.Test) : ...

### 🧪 Testing With Command Prompt

```shell
 dotnet test --framework net8.0 -c Release
```

### 🧪 Testing With IDE 

- Refer to the manual for your IDE.

## 🛠️ Integration

There are a few ways to integrate [DotRecast.Recast](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast) and [DotRecast.Detour](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour) into your project. 
Source integration is the most popular and most flexible, and is what the project was designed for from the beginning.

### 🛠️ Source Integration

It is recommended to add the source directories 
[DotRecast.Core](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Core), 
[DotRecast.Recast](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast),
[DotRecast.Detour](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour),
[DotRecast.Detour.Crowd](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.Crowd),
[DotRecast.Detour.TileCache](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.TileCache) 
and directly into your project depending on which parts of the project you need. 

For example your level building tool could include
[DotRecast.Core](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Core),
[DotRecast.Recast](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast),
[DotRecast.Detour](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour) 
and your game runtime could just include
[DotRecast.Detour](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour)

- [DotRecast.Core](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Core) : Core Utils
- [DotRecast.Recast](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Recast) : Core navmesh building system.
- [DotRecast.Detour](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour) : Runtime navmesh interface and query system.
- [DotRecast.Detour.TileCache](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.TileCache) : Runtime movement, obstacle avoidance, and crowd simulation systems.
- [DotRecast.Detour.Crowd](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.Crowd) : Runtime navmesh dynamic obstacle and re-baking system.
- [DotRecast.Detour.Dynamic](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.Dynamic) : robust support for dynamic nav meshes combining pre-built voxels with dynamic objects which can be freely added and removed
- [DotRecast.Detour.Extras](https://github.com/ikpil/DotRecast/tree/main/src/DotRecast.Detour.Extras) : simple tool to import navmeshes created with [A* Pathfinding Project](https://arongranberg.com/astar/)


### 🛠️ Installation through Nuget

- Nuget link : [DotRecast.Core](https://www.nuget.org/packages/DotRecast.Core)
- Nuget link : [DotRecast.Recast](https://www.nuget.org/packages/DotRecast.Recast)
- Nuget link : [DotRecast.Detour](https://www.nuget.org/packages/DotRecast.Detour)
- Nuget link : [DotRecast.Detour.TileCache](https://www.nuget.org/packages/DotRecast.Detour.TileCache)
- Nuget link : [DotRecast.Detour.Crowd](https://www.nuget.org/packages/DotRecast.Detour.Crowd)
- Nuget link : [DotRecast.Detour.Dynamic](https://www.nuget.org/packages/DotRecast.Detour.Dynamic)
- Nuget link : [DotRecast.Detour.Extras](https://www.nuget.org/packages/DotRecast.Detour.Extras)
- Nuget link : [DotRecast.Recast.Toolset](https://www.nuget.org/packages/DotRecast.Recast.Toolset)
- Nuget link : [DotRecast.Recast.Demo](https://www.nuget.org/packages/DotRecast.Recast.Demo)

