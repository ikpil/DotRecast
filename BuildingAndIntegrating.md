
## 🔨 Build
1. `DotRecast.Recast.Demo` uses [dotnet 8](https://dotnet.microsoft.com/) to build platform specific projects. Download it and make sure it's available on your path, or specify the path to it.
2. Open a command prompt, point it to a directory and clone DotRecast to it: `git clone https://github.com/ikpil/DotRecast.git`
3. Open `<DotRecastDir>\DotRecast.sln` with Visual Studio 2022 and build `DotRecast.Recast.Demo`
    - Optionally, you can run using the `dotnet run` command with `DotRecast.Recast.Demo.csproj`

1. 빌드 할 때, .NET 8 SDK 만 필요 하다
2. `DotRecast.Recast.Demo` 를 빌드하고 실행하면 전체적인 빌드 방법을 확인할 수 있다

## ▶️ Run
### Windows 에서 실행할 땐 
- need to install [microsoft visual c++ redistributable package](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist)
- 윈도우에서 실행 할 땐, 재배포 패키지가 설치 되어 있어야 한다
 
### Linux & MacOS & Windows
- 추가 종속성 관리 할 필요 없이  `DotRecast.Recast.Demo` 폴더로 이동 한 후 닷넷 커맨드로 `dotnet run` 을 수행하면 된다
- 비쥬얼 스튜디오 2022 에서 빌드 한다면, DotRecast.sln 을 열고, DotRecast.Recast.Demo 를 선택하여 실행하면 된다.
- `DotRecast.Recast.Demo` 폴더로 이동 한 후 닷넷 커맨드로 `dotnet run` 을 수행하면 된다

## 🛠️ Integration

### Source Code
- [DotRecast.Core](src/DotRecast.Core)
- [DotRecast.Recast](src/DotRecast.Recast)
- [DotRecast.Detour](src/DotRecast.Detour)
- [DotRecast.Detour.TileCache](src/DotRecast.Detour.TileCache)
- [DotRecast.Detour.Crowd](src/DotRecast.Detour.Crowd)
- [DotRecast.Detour.Dynamic](src/DotRecast.Detour.Dynamic)
- [DotRecast.Detour.Extras](src/DotRecast.Detour.Extras)
- [DotRecast.Recast.Toolset](src/DotRecast.Recast.Toolset)
- [DotRecast.Recast.Demo](src/DotRecast.Recast.Demo)

It is recommended to add the source directories
`DotRecast.Core`,
`DotRecast.Detour.Crowd`,
`DotRecast.Detour.Dynamic`,
`DotRecast.Detour.TitleCache`,
`DotRecast.Detour.Extras` and
`DotRecast.Recast`
into your own project depending on which parts of the project you need.
For example your level building tool could include `DotRecast.Core`, `DotRecast.Recast`,
and `DotRecast.Detour`, and your game runtime could just include `DotRecast.Detour`.

### Nuget

- [DotRecast.Core](https://www.nuget.org/packages/DotRecast.Core)
- [DotRecast.Recast](https://www.nuget.org/packages/DotRecast.Recast)
- [DotRecast.Detour](https://www.nuget.org/packages/DotRecast.Detour)
- [DotRecast.Detour.TileCache](https://www.nuget.org/packages/DotRecast.Detour.TileCache)
- [DotRecast.Detour.Crowd](https://www.nuget.org/packages/DotRecast.Detour.Crowd)
- [DotRecast.Detour.Dynamic](https://www.nuget.org/packages/DotRecast.Detour.Dynamic)
- [DotRecast.Detour.Extras](https://www.nuget.org/packages/DotRecast.Detour.Extras)
- [DotRecast.Recast.Toolset](https://www.nuget.org/packages/DotRecast.Recast.Toolset)
- [DotRecast.Recast.Demo](https://www.nuget.org/packages/DotRecast.Recast.Demo)

## 🚦Unit Test

- [DotRecast.Core.Test](test/DotRecast.Core.Test) : ...
- [DotRecast.Recast.Test](test/DotRecast.Recast.Test) : ...
- [DotRecast.Detour.Test](test/DotRecast.Detour.Test) : ...
- [DotRecast.Detour.TileCache.Test](test/DotRecast.Detour.TileCache.Test) : ...
- [DotRecast.Detour.Crowd.Test](test/DotRecast.Detour.Crowd.Test) : ...
- [DotRecast.Detour.Dynamic.Test](test/DotRecast.Detour.Dynamic.Test) : ...
- [DotRecast.Detour.Extras.Test](test/DotRecast.Detour.Extras.Test) : ...

### Windows

### With VS2022
- In Visual Studio 2022 go to the test menu and press `Run All Tests`
- 
### With CLI
- in the DotRecast folder open a command prompt and run `dotnet test`

