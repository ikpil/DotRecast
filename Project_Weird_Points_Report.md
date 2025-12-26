# DotRecast 프로젝트에서 “이상하다고 느껴지는 지점” 후보 정리 (전파용)

## 핵심 메시지 (TL;DR)

DotRecast는 **성능 최적화 지향(Span/stackalloc/메모리 재사용)** 이 강하게 반영된 고급 포팅 프로젝트입니다. 다만 그 과정에서 **(1) 타겟 프레임워크 전략(net10 포함), (2) 테스트 프로젝트 결합 방식, (3) TODO/FIXME로 남은 중복/부채, (4) 동시성/타이머 구현의 과설계 가능성** 같은 “이상 신호”가 보이며, 팀/외부 사용자에게는 **유지보수·호환성·신뢰성 리스크**로 전파될 수 있습니다.

---

## 한 장 요약표 (바로 공유용)

| 구분 | 뭐가 이상한가(증상) | 근거(파일) | 왜 문제인가(리스크) | 권장 액션 |
|---|---|---|---|---|
| **TFM/배포** | 라이브러리가 `netstandard2.1;net8.0;net9.0;net10.0` 멀티타겟 | `DotRecast.Core.csproj` 등 | 팀/사용자 환경에 따라 **SDK/TFM 준비 비용 증가**, CI 매트릭스 복잡 | “최소 지원”을 문서화하고, `net10`은 **옵션/프리뷰**로 분리 검토 |
| **테스트 구조** | 테스트 프로젝트들이 `DotRecast.Core.Test`(테스트 프로젝트)를 참조 | `DotRecast.Detour.Test.csproj` 등 | 테스트 간 결합은 **의존성/빌드 시간/유지보수** 악화. 실제로 코드에서 `DotRecast.Core.Test` 타입 사용 흔적도 거의 없음(불필요 참조 가능성) | 공용 유틸이 필요하면 `TestUtils` 같은 **별도 프로젝트**로 분리, 아니면 참조 제거 |
| **테스트 신뢰도** | 테스트에 `TODO: check`, `FIXME` 남아있음 | `TileCacheNavigationTest.cs` 등 | “테스트가 진실인가?”에 의심이 생김 → 외부 기여/리팩토링의 브레이크 | TODO/FIXME를 이슈로 승격하고, 기대값/근거 주석 보강 |
| **중복/부채** | “중복 구현”, “다음 메이저에서 제거” 같은 TODO | `DtNavMesh.cs`, `RcRasterizations.cs` | 코드가 커질수록 **일관성 깨짐/버그 양산** (한쪽만 수정되는 문제) | 중복 제거(공유 함수/내부 헬퍼), API 정리 로드맵 명시 |
| **동시성/타이머** | 타이머 시작값을 ThreadLocal 딕셔너리에 `RcAtomicLong`로 저장 | `RcContext.cs` | per-thread 데이터에 atomic을 쓰는 건 **과설계/오버헤드** 가능. 또한 `StartTimer` 호출 순서/스레드가 어긋나면 예외 위험 | 시작값은 `long`으로 저장 + 안전가드(키 존재 검사) + 명세(스레드 규약) |
| **성능 최적화 편향** | `Span`, `stackalloc` 사용이 광범위 + 별도 최적화 후보 문서 존재 | `StructPassing_InRef_Candidates.md`, CHANGELOG | 장점도 크지만, API 변경(`in/ref`)은 **호환성/가독성** 비용. 리뷰/온보딩 난이도 상승 | “핫패스” 기준을 문서화하고, 공개 API는 신중(메이저 버전에서) |

---

## 근거 1) 타겟 프레임워크 전략이 “과하게 앞서감”으로 보일 수 있음

라이브러리들이 `net10.0`까지 포함해서 멀티타겟을 하고 있습니다.

```3:6:src/DotRecast.Core/DotRecast.Core.csproj
  <PropertyGroup>
    <TargetFrameworks>netstandard2.1;net8.0;net9.0;net10.0</TargetFrameworks>
    <PackageId>DotRecast.Core</PackageId>
    <PackageReadmeFile>README.md</PackageReadmeFile>
```

- **이상 신호로 보이는 이유**: 외부 사용자(특히 Unity/툴체인/서버팀)는 “최신 TFM까지 다 맞춰야 하나?”라는 부담을 느낍니다.
- **반면 장점**: 최신 JIT/런타임 최적화 수혜(성능 라이브러리 특성상 가치가 큼).
- **전파 포인트**: “지원 범위를 넓히는 선택”이지만, 운영 관점에선 **CI/패키징 비용이 올라갈 수 있다**.

참고로, 이 작업환경에서는 .NET 10 SDK가 설치되어 있어 빌드 자체는 가능한 상태였습니다.

```text
dotnet --list-sdks
8.0.416
9.0.307
10.0.101
```

---

## 근거 2) 테스트 프로젝트 간 결합이 이례적(그리고 불필요 참조로 보일 여지)

예: `DotRecast.Detour.Test`가 `DotRecast.Core.Test`(테스트 프로젝트)를 참조합니다.

```24:28:test/DotRecast.Detour.Test/DotRecast.Detour.Test.csproj
    <ItemGroup>
        <ProjectReference Include="..\DotRecast.Core.Test\DotRecast.Core.Test.csproj" />
        <ProjectReference Include="..\..\src\DotRecast.Detour\DotRecast.Detour.csproj" />
        <ProjectReference Include="..\..\src\DotRecast.Recast\DotRecast.Recast.csproj" />
    </ItemGroup>
```

- **이상 신호로 보이는 이유**: 일반적으로 “테스트”는 서로 참조하지 않고, 공용 코드가 필요하면 `TestUtils` 같은 **별도 라이브러리**를 둡니다.
- **추가 관찰**: 코드 검색 기준으로 `using DotRecast.Core.Test` 같은 직접 사용 흔적이 거의 없어서, “왜 참조하지?”라는 질문이 남습니다.  
  (즉, 현재 상태는 **불필요 참조**일 가능성도 있습니다.)

---

## 근거 3) 테스트 코드에 “검증 미완료” 표식이 남아 있음

예: 타일캐시 경로 테스트에 “TODO: check”가 남아 있습니다.

```94:102:test/DotRecast.Detour.TileCache.Test/TileCacheNavigationTest.cs
            var status = query.FindPath(startRef, endRef, startPos, endPos, filter, path.AsSpan(), out var npath, path.Length);
            Assert.That(status, Is.EqualTo(statuses[i]));
            Assert.That(npath, Is.EqualTo(results[i].Length));
            for (int j = 0; j < results[i].Length; j++)
            {
                Assert.That(path[j], Is.EqualTo(results[i][j])); // TODO: @ikpil, check
            }
```

- **이상 신호로 보이는 이유**: 테스트는 “신뢰의 기반”인데, TODO가 남아 있으면 외부 기여자가 리팩토링을 주저합니다.
- **전파 포인트**: “테스트가 있다”보다 “테스트가 확실하다”가 더 중요.

---

## 근거 4) 중복 구현/정리되지 않은 API 부채가 명시적으로 존재

예: `DtNavMesh` 내부에 “쿼리 쪽과 중복인데 필요해서 복제함”이라는 TODO가 있습니다.

```237:246:src/DotRecast.Detour/DtNavMesh.cs
        public ref readonly DtNavMeshParams GetParams()
        {
            return ref m_params;
        }


        // TODO: These methods are duplicates from dtNavMeshQuery, but are needed for off-mesh connection finding.
        /// Queries polygons within a tile.
        private int QueryPolygonsInTile(DtMeshTile tile, RcVec3f qmin, RcVec3f qmax, Span<long> polys, int maxPolys)
```

또 다른 예로, “다음 메이저에서 제거” 예정인 매개변수(현재 미사용)가 주석으로 남아 있습니다.

```455:476:src/DotRecast.Recast/RcRasterizations.cs
        public static void RasterizeTriangle(RcContext context, float[] verts, int v0, int v1, int v2, int areaID,
            RcHeightfield heightfield, int flagMergeThreshold)
        {
            using var timer = context.ScopedTimer(RcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
            // Rasterize the single triangle.
            float inverseCellSize = 1.0f / heightfield.cs;
            float inverseCellHeight = 1.0f / heightfield.ch;
            RasterizeTri(verts, v0, v1, v2, areaID, heightfield, heightfield.bmin, heightfield.bmax, heightfield.cs, inverseCellSize,
                inverseCellHeight, flagMergeThreshold);
        }
...
        /// @param[in]		numVerts			The number of vertices. (unused) TODO (graham): Remove in next major release
```

- **이상 신호로 보이는 이유**: 포팅 프로젝트에서 흔하지만, 중복/미사용 API가 누적되면 “한쪽만 수정되는 버그”가 생기기 쉽습니다.

---

## 근거 5) 동시성/타이머 구현의 “과설계” 또는 “규약 미명시” 가능성

`RcContext`는 타이머 시작 tick을 ThreadLocal 딕셔너리에 저장하면서 값 타입이 아닌 `RcAtomicLong`을 사용합니다.

```41:68:src/DotRecast.Core/RcContext.cs
    public class RcContext
    {
        private readonly ThreadLocal<Dictionary<string, RcAtomicLong>> _timerStart;
        private readonly ConcurrentDictionary<string, RcAtomicLong> _timerAccum;
...
        public void StartTimer(RcTimerLabel label)
        {
            _timerStart.Value[label.Name] = new RcAtomicLong(RcFrequency.Ticks);
        }
...
        public void StopTimer(RcTimerLabel label)
        {
            _timerAccum
                .GetOrAdd(label.Name, _ => new RcAtomicLong(0))
                .AddAndGet(RcFrequency.Ticks - _timerStart.Value?[label.Name].Read() ?? 0);
        }
```

- **이상 신호로 보이는 이유**
  - per-thread 저장소에 atomic 객체를 쓰는 건 비용/복잡도 대비 이득이 불명확합니다(시작값은 그냥 `long`이면 충분해 보임).
  - `StopTimer`는 사실상 “같은 스레드에서 `StartTimer`가 선행”이라는 규약을 전제합니다. 규약이 깨지면 딕셔너리 인덱서에서 예외 가능성이 있습니다.

---

## (보너스) “성능 최적화 드라이브” 자체가 신호인 이유

레포에 `StructPassing_InRef_Candidates.md` 같은 문서가 따로 있고, CHANGELOG에도 `Span`/`stackalloc` 중심의 성능 리팩토링이 반복적으로 등장합니다.

- **장점**: 네비메시/경로탐색은 핫루프가 많아서, 이런 최적화가 제품 가치로 직결됩니다.
- **이상 신호로 보이는 지점**: 최적화가 넓게 퍼질수록 API 변경(`in/ref`), 디버깅 난이도, 런타임별 동작 차이 같은 “운영 비용”이 같이 커집니다.

---

## 팀에 전파할 때의 “한 문장 요약” 템플릿

> “DotRecast는 Span/stackalloc 기반의 고성능 포팅이 강점이지만, `net10` 멀티타겟·테스트 간 결합·중복 TODO 같은 운영/유지보수 리스크 신호도 있어, ‘최소 지원 범위’와 ‘테스트/공용 유틸 구조’ 정리가 필요합니다.”

---

## 바로 실행 가능한 체크리스트(단계별)

1. **TFM 정책 확정**: “공식 지원”과 “실험 지원(net10)”을 문서로 분리.
2. **테스트 구조 정리**: `DotRecast.Core.Test`를 참조하는 테스트 프로젝트들을 점검해서
   - 실제 공용 코드가 필요하면 `DotRecast.TestUtils` 같은 프로젝트로 분리
   - 아니면 `ProjectReference` 제거
3. **TODO/FIXME 정리**: 테스트의 `TODO: check`는 이슈화하고 근거/기대값을 명확히.
4. **중복 제거 로드맵**: `DtNavMesh` vs `DtNavMeshQuery` 중복 함수는 내부 헬퍼로 통합(가능 범위부터).
5. **RcContext 규약 명시**: “Start/Stop은 동일 스레드에서 짝” 같은 전제를 주석/가드로 고정.


