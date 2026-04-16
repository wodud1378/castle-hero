# Unity C# 프로젝트 가이드라인

## 🛠 빌드 및 실행 명령
- **컴파일 확인**: 유니티 에디터 포커스 (또는 MCP 서버를 통한 `Check Compilation`)
- **CLI 컴파일 확인**: `dotnet build Assembly-CSharp.csproj --nologo` (전체) — 단, 이 명령은 모든 스크립트를 한 번에 빌드해서 asmdef 경계를 검증하지 못함. **반드시 asmdef 별 csproj 도 함께 빌드**:
  ```
  for f in CastleHero.Common.csproj CastleHero.Data.csproj CastleHero.Network.csproj CastleHero.Network.Impl.Backend.csproj CastleHero.Network.Impl.Local.csproj CastleHero.GamePlay.csproj CastleHero.View.csproj CastleHero.Editor.csproj; do
    dotnet build "$f" --nologo 2>&1 | grep -iE "(error CS|오류 CS)" | grep -v MSB
  done
  ```
- **csproj 동기화**: 새 .cs 파일 추가/삭제 시 `node sync_csproj.js` 로 csproj 의 `<Compile Include>` 항목을 파일 시스템과 동기화.
- **테스트 실행**: `Window > General > Test Runner`를 통한 실행
- **로그 확인**: 에디터 콘솔 창 또는 `tail -f ./Logs/Editor.log`

## 📜 코딩 스타일 규칙
- **명명 규칙 (Naming)**:
    - 클래스, 메서드, public 변수, 프로퍼티: `PascalCase`
    - private 필드: `_camelCase` (언더바 접두사 사용)
    - SerializeField 필드 : `camelCase`
    - 지역 변수 및 매개변수: `camelCase`
- **직렬화 (Serialization)**:
    - 외부 노출이 필요한 경우 `public` 대신 `[SerializeField] private` 사용을 권장.
- **Unity 모범 사례**:
    - `GetComponent` 대신 `TryGetComponent`를 사용하여 성능 및 안정성 확보.
    - `Update()` 내에서 `GameObject.Find`나 `SendMessage` 사용 엄격히 금지.
    - 문자열 참조 대신 `nameof()` 연산자 활용.
    - 이벤트 구독 시 반드시 `OnDestroy` 또는 `OnDisable`에서 해제.
- **Null 체크**: Unity 오브젝트는 생명주기 특성상 `is null` 대신 `obj == null` 방식을 사용.
- **SerializeField Null 체크**: 인스펙터에서 할당해야하는 필드는 없으면 예외가 발생하는 것이 맞으므로, 대부분의 상황에서는 있다고 가정하고 코드 작성
- **SerializeField 노출**: 외부 노출이 필요한 경우 [field:SerializeField]를 사용해 프로퍼티로 노출

## 🏗 프로젝트 아키텍처
- **네임스페이스**: `ProjectName.Folder.SubFolder` 구조 준수.
- **설계 원칙**: 상속보다는 컴포넌트 기반(Composition) 설계 선호.
- **데이터 관리**: 정적 데이터나 설정값은 `ScriptableObject`를 적극 활용.
- **UI 시스템**: 최신 프로젝트의 경우 `UI Toolkit`을 우선하되, 레거시는 `UGUI` 사용.
- **모듈화**: 시스템이 적절히 책임에 따라 분리되어야하고, 적절한 추상화로 확장성이 있어야함.

### 모듈 구조 (asmdef)
```
Common         — 패턴/유틸/사운드 추상 (다른 모듈 참조 없음)
Data           — DB/Model/Repository/Factory 인터페이스 (Common, Network 참조)
Network        — 네트워크 인터페이스 + Result + facade (Common, Data 참조)
Network.Impl.Backend — 뒤끝(BackendInit) 기반 구현체
Network.Impl.Local   — 로컬 인메모리 스텁
GamePlay       — 유닛/AOE/감지 등 게임 로직 MonoBehaviour 포함 (Common, Data, Network 참조, View 참조 금지)
View           — UI/이펙트 등 디스플레이 MonoBehaviour (Common, Data, Network, GamePlay 참조)
Editor         — 에디터 전용
```
- **계층 규칙**: GamePlay → View 참조는 금지. View → GamePlay 참조는 허용. GamePlay 가 View 를 호출해야 하면 GamePlay 에 인터페이스 두고 View 에 구현체 (예: `IUnitRenderer`/`IAnimationEventProvider`).
- **MonoBehaviour 위치**: View = 순수 디스플레이 (UI, Effect, Renderer). GamePlay = 게임플레이 로직 (UnitBehaviour, AOE 필드, 감지 트리거).

### 의존성 주입 / ServiceLocator
- **부트스트랩 단일 지점**: `Context.LoadAsync` (View) 와 `BackendBootService` (Network.Impl.Backend) 에서만 `ServiceLocator.Register<T>()` 호출. 다른 곳에서 Register 금지.
- **plain C# 클래스**: 생성자 주입 우선. `ServiceLocator.Get` 직접 호출 지양.
- **MonoBehaviour**: `Awake()` 또는 `Init()` 에서 1회 캐싱. `Update`/이벤트 핸들러 안에서 매번 `Get` 호출 금지.
- **OnRegistered 이벤트**: 확장 메서드처럼 lazy 초기화 필요한 정적 유틸은 `ServiceLocator.OnRegistered` 구독으로 캐시 갱신 (예: `LocalizeHelper`).

### 풀(Pool) 라이프사이클
- `AddressablePool` 은 **완전 동기**. 생성 시 `GameObject prefab` 을 받아 보관, `TryGet`/`Release` 모두 sync.
- `Preloader.PreloadAll(entries, PoolContainer)` 가 `Addressables.LoadAssetAsync` 로 비동기 로드 → `PoolContainer.RegisterWithHandle(path, prefab, handle, count)` 로 등록.
- **스테이지 진입**: `InGameBehaviour.OnLoaded` 에서 `Preloader.PreloadAll` 호출. `Loading.SceneReady` (UniTaskCompletionSource) 핸드셰이크로 Loading UI 가 프리로드 끝까지 표시됨.
- **스테이지 종료**: `InGameBehaviour.Dispose` 에서 `_poolContainer.Dispose()` → 풀 GameObject 전체 Destroy + Addressables 핸들 Release. 다음 씬 `Context.LoadAsync` 가 새 PoolContainer 를 ServiceLocator 에 덮어씀.
- 풀 사용처가 늘어나면 `StagePreloadPlanner.Plan(stageId)` 에 entry 추가 필요.

### 씬 전환
- 모든 씬 전환은 `Loading.NextScene = "Foo"` 로 시작. Loading 씬이 additive 로 로드되어 대상 씬 진행 상황을 가린 뒤 언로드.
- 씬 종료 직전 `Loading.Tasks.Add(task)` 로 비동기 작업을 큐잉하면 Loading 씬에서 await.
- 새 씬의 `Context.LoadAsync` 가 끝날 때까지 Loading 씬은 `Loading.SceneReady.Task` 로 대기.

### 코드 스타일 추가
- `async void` 금지. 항상 `async UniTask` + 호출부 `.Forget()` 또는 await.
- Reactive 외부 노출은 `IReadOnlyReactiveProperty<T>` / `IObservable<T>` 등 read-only 인터페이스로.
- DTO (Network.Shared) 와 Entity (Data.Model) 는 **분리 유지**. DTO 에 비즈니스 로직 추가 금지 → 확장 메서드(`Data/Shared/CurrencyExtensions.cs` 패턴) 로 분리.

## 🎯 작업별 세부 지침
- **물리(Physics)**: 물리 연산 및 Rigidbody 조작은 반드시 `FixedUpdate`에서 수행.
- **최적화**: `Update` 메서드 내에서 `new` 키워드를 통한 메모리 할당(GC) 지양. 반복 생성되는 객체는 `Object Pooling` 적용.
- **