### Building DotRecast.Recast.Demo

1. `DotRecast.Recast.Demo` uses [dotnet 8](https://dotnet.microsoft.com/) to build platform specific projects. Download it and make sure it's available on your path, or specify the path to it.
2. Open a command prompt, point it to a directory and clone DotRecast to it: `git clone https://github.com/ikpil/DotRecast.git`
3. Open `<DotRecastDir>\DotRecast.sln` with Visual Studio 2022 and build `DotRecast.Recast.Demo`
    - Optionally, you can run using the `dotnet run` command with `DotRecast.Recast.Demo.csproj`

#### Windows

- need to install [microsoft visual c++ redistributable package](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist)

#### Linux & macOS & Windows

- Navigate to the `DotRecast.Recast.Demo` folder and run `dotnet run`

### Running Unit tests

#### With VS2022

- In Visual Studio 2022 go to the test menu and press `Run All Tests`

#### With CLI

- in the DotRecast folder open a command prompt and run `dotnet test`

## Integrating with your game or engine

It is recommended to add the source directories `DotRecast.Core`, `DotRecast.Detour.Crowd`, `DotRecast.Detour.Dynamic`, `DotRecast.Detour.TitleCache`, `DotRecast.Detour.Extras` and `DotRecast.Recast` into your own project depending on which parts of the project you need. For example your level building tool could include `DotRecast.Core`, `DotRecast.Recast`, and `DotRecast.Detour`, and your game runtime could just include `DotRecast.Detour`.
