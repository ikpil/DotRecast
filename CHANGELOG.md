# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased] - yyyy-mm-dd

### Added
- Added RcCircularBuffer<T> [@ikpil](https://github.com/ikpil)
- Added struct DtCrowdScopedTimer to avoid allocations in scoped timer calls. [@wrenge](https://github.com/wrenge)
 
### Fixed

### Changed
- Changed DtPathCorridor.Init(int maxPath) function to allow setting the maximum path [@ikpil](https://github.com/ikpil)
- Changed from List<T> to RcCyclicBuffer in DtCrowdTelemetry execution timing sampling [@wrenge](https://github.com/wrenge)

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

