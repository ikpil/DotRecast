# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased] - yyyy-mm-dd

### Added
- Added RcBinaryMinHeap ([@Sarofc](https://github.com/Sarofc))
- Added DotRecast.Benchmark ([@Sarofc](https://github.com/Sarofc))

### Fixed
- Fix raycast shortcuts ([@Sarofc](https://github.com/Sarofc)) [#72](https://github.com/ikpil/DotRecast/issues/72)
- Fix dynamic mesh bounds calculation ([@ppiastucki](https://github.com/ppiastucki)) [#77](https://github.com/ikpil/DotRecast/issues/77)
    - issuer : [@OhJeongrok](https://github.com/OhJeongrok)
- Fix Support non-tiled dynamic nav meshes ([@ppiastucki](https://github.com/ppiastucki))

### Changed
- Changed data structure of 'neis' from List<byte> to byte[] for optimized memory usage and improved access speed in `DtLayerMonotoneRegion`
- Changed new RcVec3f[3] to stackalloc RcVec3f[3] in DtNavMesh.GetPolyHeight() to reduce heap allocation
- Changed memory handling to use stackalloc in DtNavMeshQuery.GetPolyWallSegments for reducing SOH
- Changed DtNavMeshQuery.GetPolyWallSegments() to use Span<T> for enhanced performance, memory efficiency.
- Changed bmin/bmax from int[] to RcVec3i for improved memory efficiency

### Removed
- Nothing

### Special Thanks
- [@Doprez](https://github.com/Doprez)


## [2024.3.1] - 2024-07-09

### Added
- Nothing

### Fixed
- Fixed bug where the dynamic voxel save file browser doesn't appear in `Recast.Demo`

### Changed
- Changed to reuse samples and edges list in `BuildPolyDetail()`
- Changed `heights`, `areas`, `cons`, and `regs` arrays to byte arrays for uniformity and efficiency in `DtTileCacheLayer`
- Changed `reg`, `area` arrays to byte arrays for uniformity and efficiency in `DtTileCacheContour`
- Changed `RcChunkyTriMesh` to separate the function and variable.
- Changed to consolidate vector-related functions into one place.
- Changed stack handling from List to a fixed-size array with manual index management for optimization in `RcLayers.BuildHeightfieldLayers()`
- Changed to use Span<byte> and stackalloc for improved performance and memory management in `RcLayers.BuildHeightfieldLayers()`
- Changed vertCount and triCount to byte in `DtPolyDetail`
- Changed `new float[]` to `stackalloc float[]` in `DtConvexConvexIntersections.Intersect()`
- Changed agents management from list to dictionary in `DtCrowd`
- Changed to efficiently stack nearby DtCrowdAgents in `DtCrowd.GetNeighbours()`
- Changed to limit neighbor search to a maximum count and use array for memory efficiency in `DtCrowd.AddNeighbour()`

### Removed
- Removed RcMeshDetails.VdistSq2(float[], float[])
- Removed RcVecUtils.Dot()
- Removed RcVecUtils.Scale()
- Removed RcVecUtils.Subtract(RcVec3f i, float[] verts, int j)
- Removed RcVecUtils.Subtract(float[] verts, int i, int j)
- Removed RcVecUtils.Min(), RcVecUtils.Max()
- Removed RcVecUtils.Create(float[] values)
- Removed RcVecUtils.Dot2D(this RcVec3f @this, Span<float> v, int vi)

### Special Thanks
- [@Doprez](https://github.com/Doprez)

## [2024.2.3] - 2024-06-03

### Added
- Added `DtCollectPolysQuery` and `FindCollectPolyTest`

### Fixed
- Nothing

### Changed
- Changed `IDtPolyQuery` interface to make `Process()` more versatile
- Changed `PolyQueryInvoker` to `DtActionPolyQuery`
- Changed `DtTileCacheBuilder` to a static class
- Changed `DtTileCacheLayerHeaderReader` to a static class
- Changed `Dictionary<int, List<DtMeshTile>>` to `DtMeshTile[]` to optimize memory usage
- Changed `MAX_STEER_POINTS` from class constant to local. 
- Changed `List<DtStraightPath>` to `Span<DtStraightPath>` for enhanced memory efficiency
- Changed `DtWriter` to a static class and renamed it to `RcIO`
- Changed class `Trajectory` to interface `ITrajectory`

### Removed
- Nothing

### Special Thanks
- [@Doprez](https://github.com/Doprez)

 
## [2024.2.2] - 2024-05-18

### Added
- Added RcSpans UnitTest
 
### Fixed
- Nothing

### Changed
- Changed class name of static functions to RcRecast and DtDetour
- Changed DtLink class member variable type from int to byte
- Changed initialization in DtNavMesh constructor to Init() function.

### Removed
- Nothing
 
### Special Thanks
- [@Doprez](https://github.com/Doprez)


## [2024.2.1] - 2024-05-04

### Added
- Added RcCircularBuffer<T> [@ikpil](https://github.com/ikpil)
- Added struct DtCrowdScopedTimer to avoid allocations in scoped timer calls. [@wrenge](https://github.com/wrenge)
- Added struct RcScopedTimer to avoid allocations in RcContext scoped timer [@ikpil](https://github.com/ikpil)
- Added RcSpans [@ikpil](https://github.com/ikpil)
 
### Fixed
- SOH issue [#14](https://github.com/ikpil/DotRecast/issues/41)
- Optimization: reduce number of allocations on hot path. [@awgil](https://github.com/awgil)

### Changed
- Changed DtPathCorridor.Init(int maxPath) function to allow setting the maximum path [@ikpil](https://github.com/ikpil)
- Changed from List<T> to RcCyclicBuffer in DtCrowdTelemetry execution timing sampling [@wrenge](https://github.com/wrenge)
- RcCyclicBuffer<T> optimizations [@wrenge](https://github.com/wrenge)

### Removed

### Special Thanks
- [@Doprez](https://github.com/Doprez)
- [@Arctium](https://github.com/Arctium)


## [2024.1.3] - 2024-02-13

### Added
- Added DtNodeQueue UnitTest [@ikpil](https://github.com/ikpil)
- Added RcSortedQueue UnitTest [@ikpil](https://github.com/ikpil)
- Added IComparable interface to RcAtomicLong [@ikpil](https://github.com/ikpil)
- Added Menu bar in Demo [@ikpil](https://github.com/ikpil)
 
### Fixed

### Changed
- Update Microsoft.NET.Test.Sdk 17.8.0 to 17.9.0
- Enhanced ToString method of DtNode to provide more detailed information.
- Reuse DtNode in DtNodePool
 
### Removed

### Special Thanks
- [@Doprez](https://github.com/Doprez)
- [@Arctium](https://github.com/Arctium)
 
## [2024.1.2] - 2024-02-04

### Added
- Added DtNodePool tests [@ikpil](https://github.com/ikpil)
- Added WangHash() for DtNodePool [@ikpil](https://github.com/ikpil)
- Added avg, min, max, sampling updated times in CrowdAgentProfilingTool [@ikpil](https://github.com/ikpil)
 
### Fixed
- Fixed SOH issue in DtNavMeshQuery.Raycast [@ikpil](https://github.com/ikpil)
- Fixed SOH issue in DtProximityGrid.QueryItems [@ikpil](https://github.com/ikpil)

### Changed
- Upgrade NUnit.Analyzers 4.0.1

### Removed

### Special Thanks
- [@Doprez](https://github.com/Doprez)
- [@Arctium](https://github.com/Arctium)

## [2024.1.1] - 2024-01-05

### Fixed
- Fix typo ([#25](https://github.com/ikpil/DotRecast/pull/25)) [@c0nd3v](https://github.com/c0nd3v)
- Fix updated struct version ([#23](https://github.com/ikpil/DotRecast/pull/23)) [@c0nd3v](https://github.com/c0nd3v)
- Allow Radius 0 in Demo ([#22](https://github.com/ikpil/DotRecast/pull/22)) [@c0nd3v](https://github.com/c0nd3v)

### Changed
- [Upstream] Cleanup filter code and improved documentation ([#30](https://github.com/ikpil/DotRecast/pull/30)) [@ikpil](https://github.com/ikpil)
- [Upstream] Make detail mesh edge detection more robust ([#26](https://github.com/ikpil/DotRecast/pull/26)) [@ikpil](https://github.com/ikpil)
- [Upstream] 248275e - Fix: typo error (#153) ([#21](https://github.com/ikpil/DotRecast/pull/21)) [@ikpil](https://github.com/ikpil)
- Code cleanup and small optimizations in RecastFilter.cpp ([#29](https://github.com/ikpil/DotRecast/pull/29)) [@ikpil](https://github.com/ikpil)
- Added UI scaling feature based on monitor resolution in Demo ([#28](https://github.com/ikpil/DotRecast/pull/28)) [@ikpil](https://github.com/ikpil)

