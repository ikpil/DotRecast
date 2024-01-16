
## 🔨 Build
- 빌드에는 오직  .NET 8 SDK 만 요구 됩니다.
- 모든 모듈에 대한 빌드를 확인 하려면, DotRecast.Recast.Demo 빌드 해야 합니다. 
- 먼저 소스 코드를 clone 하여 준비 합니다.
- Open a command prompt, point it to a directory and clone DotRecast to it: `git clone https://github.com/ikpil/DotRecast.git`

### Building with Command Prompt
```shell
dotnet --version
dotnet build
```

### Building with an IDE
- Visual Studio 2022 & Visual Code & Rider 
- IDE 로 Open `<DotRecastDir>\DotRecast.sln` 을 오픈 한 뒤, 
- `DotRecast.Recast.Demo` 를 빌드

## ▶️ Run
### Windows 에서 실행할 땐 
- need to install [microsoft visual c++ redistributable package](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist)
- 윈도우에서 실행 할 땐, 재배포 패키지가 설치 되어 있어야 한다
 
### Linux & MacOS & Windows
- 추가 종속성 관리 할 필요 없이  `DotRecast.Recast.Demo` 폴더로 이동 한 후 닷넷 커맨드로 `dotnet run` 을 수행하면 된다
- 비쥬얼 스튜디오 2022 에서 빌드 한다면, DotRecast.sln 을 열고, DotRecast.Recast.Demo 를 선택하여 실행하면 된다.
- `DotRecast.Recast.Demo` 폴더로 이동 한 후 닷넷 커맨드로 `dotnet run` 을 수행하면 된다
- 
```shell
dotnet run src/DotRecast.Recast.Demo --proejct
```

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

