## 핵심 메시지 (TL;DR)

이 프로젝트에서 `struct` 파라미터 전달 최적화는 다음 순서로 효과가 큽니다.

- **큰 struct(대략 32B 이상)**: `in`(읽기 전용) 또는 `ref`(수정/누적)로 바꾸면 **복사 비용 감소 효과가 확실**합니다.
- **작지만 매우 자주 호출되는 struct(예: `RcVec3f`)**: 핫루프/쿼리/기하 함수에서 `in` 전환이 **누적 비용을 줄일 가능성**이 큽니다.

---

## 왜 이 문서가 필요한가

이 코드베이스는 Recast/Detour 계열 알고리즘 특성상 “작은 기하 타입”이 **경로탐색/레이캐스트/교차 테스트**에서 매우 많이 호출됩니다. C#에서 `struct`를 by-value로 넘기면 호출 지점마다 복사가 발생할 수 있으므로,

- **큰 struct**는 `in/ref`로,
- **핫한 작은 struct**도 (특정 구간에서) `in`으로

바꿨을 때 유의미한 성능 개선이 나올 수 있습니다.

---

## 빠른 선택 규칙 (실무 기준)

| 상황 | 권장 | 이유 |
|---|---|---|
| 파라미터를 함수에서 **읽기만** 한다 | `in T` | 복사 방지 + 읽기 전용 의도 명확 |
| 파라미터를 함수에서 **수정/누적** 해야 한다 | `ref T` | 호출자 데이터 변경 |
| 파라미터를 함수에서 **채워서 반환**한다 | `out T` (또는 `ref` 반환) | 결과값 출력 |
| 타입이 매우 작다(예: 8~12B) + 호출이 적다 | 현행 유지 | 가독성/호환성 유지가 더 중요 |

> 참고: `in`은 내부적으로 “readonly ref”이며, JIT/런타임/코드 패턴에 따라 이득이 달라질 수 있습니다. 이 문서는 “후보 목록”입니다.

---

## 우선순위 (효과 큰 순)

| 우선순위 | 타입 | 대략 크기 감 | 코멘트 |
|---:|---|---:|---|
| 1 | `RcMatrix4x4f` | **64B** | 큰 struct. by-value면 손해가 큼 |
| 2 | `DtTileCacheParams` | **50B+** | 설정 struct가 큼 |
| 3 | `DtNavMeshParams` | **30B대** | 내부에 `RcVec3f` 포함 |
| 4 | `NavMeshSetHeader` | **더 큼** | 내부에 `DtNavMeshParams` 포함 |
| 5 | `RcVec3f` | **12B** | 크기는 작지만 **핫 루프에서 호출이 매우 많음** |
| 6 | `RcAreaModification` | **8B** | 이득 작음(일관성 목적이면 `in` 가능) |

---

## `in` 전환 “강력 후보” 목록

아래 항목은 **현재 by-value로 `RcVec3f`/큰 struct를 받는 메서드 시그니처**를 기준으로 뽑았습니다.

### 1) Detour 쿼리 (핫스팟) — `RcVec3f` by-value → `in RcVec3f` 후보

파일: `src/DotRecast.Detour/DtNavMeshQuery.cs`

- `FindRandomPointAroundCircle(..., RcVec3f centerPos, ...)`
- `FindRandomPointWithinCircle(..., RcVec3f centerPos, ...)`
- `ClosestPointOnPoly(long refs, RcVec3f pos, ...)`
- `ClosestPointOnPolyBoundary(long refs, RcVec3f pos, ...)`
- `GetPolyHeight(long refs, RcVec3f pos, ...)`
- `FindNearestPoly(RcVec3f center, RcVec3f halfExtents, ...)`
- `QueryPolygonsInTile(..., RcVec3f qmin, RcVec3f qmax, ...)`
- `QueryPolygons(RcVec3f center, RcVec3f halfExtents, ...)` (오버로드 포함)
- `FindPath(..., RcVec3f startPos, RcVec3f endPos, ...)`
- `InitSlicedFindPath(..., RcVec3f startPos, RcVec3f endPos, ...)`
- `FindStraightPath(RcVec3f startPos, RcVec3f endPos, ...)`
- `MoveAlongSurface(..., RcVec3f startPos, RcVec3f endPos, ...)`
- `Raycast(..., RcVec3f startPos, RcVec3f endPos, ...)` (오버로드 포함)
- `FindPolysAroundCircle(..., RcVec3f centerPos, ...)`
- `FindLocalNeighbourhood(..., RcVec3f centerPos, ...)`
- `FindDistanceToWall(..., RcVec3f centerPos, ...)`

보조 함수(역시 후보):

- `AppendVertex(RcVec3f pos, ...)`
- `AppendPortals(..., RcVec3f endPos, ...)`
- `GetEdgeIntersectionPoint(RcVec3f fromPos, ...)`

### 2) Detour 유틸/기하 — `RcVec3f` by-value → `in RcVec3f` 후보

파일: `src/DotRecast.Detour/DtUtils.cs`

- `OverlapBounds(RcVec3f amin, RcVec3f amax, RcVec3f bmin, RcVec3f bmax)`
- `TriArea2D(RcVec3f a, RcVec3f b, RcVec3f c)`
- `ClosestHeightPointTriangle(RcVec3f p, RcVec3f a, RcVec3f b, RcVec3f c, ...)`
- `ProjectPoly(RcVec3f axis, ...)`
- `PointInPolygon(RcVec3f pt, ...)`
- `DistancePtPolyEdgesSqr(RcVec3f pt, ...)`
- `DistancePtSegSqr2D(RcVec3f pt, RcVec3f p, RcVec3f q, ...)` (오버로드 포함)
- `IntersectSegmentPoly2D(RcVec3f p0, RcVec3f p1, ...)`
- `IntersectSegSeg2D(RcVec3f ap, RcVec3f aq, RcVec3f bp, RcVec3f bq, ...)`

### 3) Recast Rasterization — `RcVec3f` by-value → `in RcVec3f` 후보

파일: `src/DotRecast.Recast/RcFilledVolumeRasterization.cs`

- `RasterizeSphere(..., RcVec3f center, ...)`
- `RasterizeCapsule(..., RcVec3f start, RcVec3f end, ...)`
- `RasterizeCylinder(..., RcVec3f start, RcVec3f end, ...)`
- `RasterizeBox(..., RcVec3f center, ...)`
- 내부 helper들(`Intersect*`, `Ray*`, `OverlapBounds(RcVec3f ...)`)도 전반적으로 후보

### 4) Recast 기본 유틸 — `RcVec3f` by-value → `in RcVec3f` 후보

파일: `src/DotRecast.Recast/RcRecast.cs`

- `CalcGridSize(RcVec3f bmin, RcVec3f bmax, ...)`
- `CalcTileCount(RcVec3f bmin, RcVec3f bmax, ...)`
- `CalcTriNormal(RcVec3f v0, RcVec3f v1, RcVec3f v2, ref RcVec3f norm)`
  - `v0/v1/v2`는 `in` 후보, `norm`은 **출력/수정**이므로 `ref` 유지가 자연스러움

### 5) Core 교차/컨벡스 — `RcVec3f` by-value → `in RcVec3f` 후보

파일: `src/DotRecast.Core/RcIntersections.cs`

- `IntersectSegmentTriangle(RcVec3f sp, RcVec3f sq, RcVec3f a, RcVec3f b, RcVec3f c, ...)`
- `IsectSegAABB(RcVec3f sp, RcVec3f sq, RcVec3f amin, RcVec3f amax, ...)`

파일: `src/DotRecast.Core/RcConvexUtils.cs`

- `Cmppt(RcVec3f a, RcVec3f b)`
- `Left(RcVec3f a, RcVec3f b, RcVec3f c)`

---

## 큰 struct: `in/ref` 전환 “확실 후보”

### 1) `DtTileCacheParams` (큰 설정 struct) — by-value → `in DtTileCacheParams` 후보

- `src/DotRecast.Detour.TileCache/DtTileCache.cs`
  - `DtTileCache(DtTileCacheParams option, ...)`
- `src/DotRecast.Detour.TileCache/Io/DtTileCacheWriter.cs`
  - `WriteCacheParams(..., DtTileCacheParams option, ...)`

### 2) `DtNavMeshParams` — by-value → `in DtNavMeshParams` 후보

- `src/DotRecast.Detour/DtNavMesh.cs`
  - `Init(DtNavMeshParams param, ...)`
- `src/DotRecast.Detour/Io/DtNavMeshParamWriter.cs`
  - `Write(..., DtNavMeshParams option, ...)`
- `src/DotRecast.Detour/Io/DtMeshSetReader.cs`
  - `Convert32BitRef(..., DtNavMeshParams option)`

### 3) `RcMatrix4x4f` — by-value 발견 시 우선 `in/ref` 검토

이미 큰 struct에 대한 `ref` 사용이 존재합니다.

- `src/DotRecast.Core/Numerics/RcMatrix4x4f.cs`
  - `Mul(ref RcMatrix4x4f left, ref RcMatrix4x4f right)`

추가 개선 후보(예: 64B를 값으로 받는 케이스):

- `src/DotRecast.Recast.Toolset/Tools/RcDynamicUpdateTool.cs`
  - `MulMatrixVector(ref RcVec3f resultvector, RcMatrix4x4f matrix, RcVec3f pvector)`
    - `matrix`는 **`in RcMatrix4x4f`** 후보
    - `pvector`도 **`in RcVec3f`** 후보

---

## “굳이 안 바꿔도 됨” (효과가 작은 편)

### `RcAreaModification` (8B)

파일: `src/DotRecast.Recast/RcAreaModification.cs`

- `int` 2개(대략 8B) 수준이라 `in`으로 바꿔도 되지만 복사 비용 이득은 작습니다.
- 단, **API 일관성(다른 큰 설정들이 in이 되면 같이 맞추기)** 목적으로는 `in`으로 바꿀 수 있습니다.

---

## 적용 시 주의사항

- `in`으로 바꾸면 호출자/오버로드 해석이 바뀔 수 있어 **공개 API**라면 영향 범위를 확인해야 합니다.
- `in` 파라미터는 “readonly”라서, 내부에서 실수로 수정하려 하면 컴파일 에러로 잡힙니다(의도 명확화 장점).
- 너무 작은 struct까지 무차별 `in` 전환은 가독성 저하 + JIT 상황에 따라 이득이 미미할 수 있습니다.

---

## 다음 액션(원하면)

- `DtNavMeshQuery`/`DtUtils`/`RcFilledVolumeRasterization`의 `RcVec3f` 파라미터를 일괄 `in`으로 바꾸고,
  - 호출자 수정 포함하여 빌드/테스트까지 통과시키는 변경을 만들 수 있습니다.


