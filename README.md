## 목차
[1.프로젝트 개요](#프로젝트-개요)<br/>
[2.프로젝트 아키텍처](#프로젝트-아키텍처)<br/>
[3.트러블 슈팅](#트러블-슈팅)<br/>
[4.MVP 패턴](#MVP-패턴)<br/>
[5.팩토리 패턴](#팩토리-패턴)<br/>
[6.Stage 생성 Sequence Diagram](#Stage-생성)<br/>
[7.에디터 툴](#에디터-툴)<br/>
[8.콘텐츠 설명](#콘텐츠-설명)<br/>

## 프로젝트 개요

- 개발 기간
    - 2024.10.09 ~10.29
- 게임 장르
    - 하이퍼 캐주얼
- 타겟 플랫폼
    - 모바일
- 목표
    - 캔디 크러쉬 사가, 애니팡과 같은 3 매치 퍼즐 게임을 구현한다.
    - 세부 구현 목표
        - 3 ~ 5 개의 기본 매칭 시스템 구현
        - 4개 이상 매칭시 특수 블록 생성
        - 매칭 시 상단에서 블록을 떨어뜨리는 시스템 구현
        - 스테이지 구현
            - 스테이지 별 승리, 패배 조건 추가
        - 이동 가능한 블록이 없을 경우에 블록 셔플 기능 추가
        - 스와이프 기능 구현
- 목적
    - 쓰리 매치 퍼즐을 직접 구현함으로써 이해도를 높인다.
- Github 주소
    - 클라이언트 - https://github.com/Jeonhyeongwoo1/ThreeMatch
- 사용 기술
    - Unity, Firebase
- 게임 설명
    - 3 Match 캐주얼 게임
    - 각 스테이지마다 주어진 횟수안에 미션을 클리어하면 스테이지 성공 방식
    - 인 게임에 필요한 아이템을 판매 및 구매를 할 수 있음
- 게임 화면
  - Title
    - <img width="544" alt="Image" src="https://github.com/user-attachments/assets/6cd9c95d-1b89-49c4-917e-63a8137b1265" />
  - Lobby
    - <img width="540" alt="Image" src="https://github.com/user-attachments/assets/7409366a-4b8e-41db-a2b4-c32fbd9803db" />
  - Game
    - https://github.com/user-attachments/assets/0f5b9f77-b882-44b6-856c-4b1286bdd7fc

## 프로젝트 아키텍처
<div align="center">
    <img width="1180" alt="Image" src="https://github.com/user-attachments/assets/b966d5ee-3e7f-40c5-8180-ad460d93b13c" />
</div>

- 클라이언트 (Unity)
    - Firebase Auth를 이용한 로그인/회원가입
    - Firestore를 활용한 데이터 저장 및 조회
    - 게임 내에서 데이터를 실시간 동기화
- Firebase
    - Auth (사용자 로그인 및 인증)
    - Firestore(게임 데이터 저장 및 관리)
    - 클라이언트 요청에 대한 응답 처리

## 트러블 슈팅
- 매칭된 셀의 모양의 조합이 T, L, 십자가 형태일 때 셀이 정상적으로 없어지지 않는 현상
   - https://github.com/Jeonhyeongwoo1/ThreeMatch/issues/1
 
## MVP 패턴

<div align="center">
    <img width="445" alt="Image" src="https://github.com/user-attachments/assets/96eaec8c-900c-43db-b52b-07ae81b4580d" />
</div>

- Model, View, Presenter간의 명확한 분리를 통해 코드의 구조 개선
- 각 모듈이 분류되어서 구현되었기 때문에 서로간의 결합도가 낮고 확장성과 유연성이 높아지므로 코드 관리가 쉬움
- View - Presenter 1 : 1 관계
  
## 팩토리 패턴
- Model, Presenter는 전역적으로 접근해야할 수 있으므로 한 곳에서 관리하지 않으면 코드 관리가 어려워지므로 Factory내에서 관리 및 생성
- 모든 Model, Presenter들을 중앙에서 관리 및 생성하여 중앙 집중화
```csharp
public static class ModelFactory
{
    private static readonly Dictionary<Type, IModel> _modelDict = new();

    public static T CreateOrGetModel<T>() where T : IModel, new ()
    {
        if (!_modelDict.TryGetValue(typeof(T), out var model))
        {
            model = new T();
            _modelDict.Add(typeof(T), model);
            return (T) model;
        }

        return (T)model;
    }

    public static void ClearCachedDict()
    {
        _modelDict.Clear();
    }
}
```

## Stage 생성 
### Sequence Diagram

---

- 목적
    - 유저가 Stage 단계를 선택한 이후에, 스테이지를 로드 및 생성하고 보드를 생성하는 과정에 대해서 주요프로세스를 나타낸다.
      
<div align="center">
    <img width="1195" alt="Image" src="https://github.com/user-attachments/assets/04201664-ade4-498f-a187-3d6817632f05" />
</div>

- 주요 프로세스 설명
    - StageManager가 LoadStage()를 호출하여 유저가 선택한 스테이지 레벨을 바탕으로 StageBuilder에게 Stage 구성 요청 및 반환
    - StageManager가 BuildAsync()를 실행하여 스테이지 내 블록 및 셀을 생성하는 비동기 프로세스 수행.
    - Board는 블록 및 셀을 생성하며, 개별적으로 CreateBlockBehaviour() 및 CreateCellBehaviour()를 호출하여 각각 Block, Cell 게임 오브젝트 생성
    - 스테이지 빌드가 완료되면, BuildAfterProcessAsync()를 실행하여 게임 시작 전 데이터 정리 및 초기화 진행.
    - PostSwapProcess()를 통해 매칭 검사를 수행하고, CheckingMatchingCell()이 루프를 돌면서 연속 매칭을 감지.
    - 최종적으로, StartHitProcess()를 호출하여 유저 힌트 비동기 프로세스 수행.

## 에디터 툴

- 목표 : 에디터 툴을 개발하여 코드 없이도 다른 개발자들이 편리하게 스테이지 및 보드 생성을 할 수 있게 하여 생산성을 높인다.
- 사용법
    1. Tools → Level Editor 클릭
    2. 프로젝트 내에서 StageEditor 검색 후 씬 선택
    - 보드 구성 방법
    - 스테이지 생성 버튼 클릭
    - <div align="center">
        <img width="330" alt="Image" src="https://github.com/user-attachments/assets/73e173e5-f046-4b44-aa67-e9335796bb3a" />
      </div>
    - 생성된 보드에 "보드에서 사용가능한 에셋"에 있는 셀을 활용하여 보드판의 원하는 위치에 드래그앤 드랍
    - <div align="center">
        <img width="327" alt="Image" src="https://github.com/user-attachments/assets/81fb350e-2e45-4818-98a9-f17f08025315" />
      </div>
    - 완료된 스테이지 저장
    - <div align="center">
        <img width="329" alt="Image" src="https://github.com/user-attachments/assets/ef7a49a3-333c-402f-bf1d-8ea46c86899c" />
      </div>
    - 저장 경로 : Resources/StageLevel/StageLevel_{}.asset
- 주의 사항
  - 미션 및 이동 가능한 횟수와 목표 점수를 반드시 추가해야함.
- 테스트 실행 방법
  - StageEditor 실행 버튼 클릭


## 콘텐츠 설명

---

### 아이템

- 인게임 아이템
    - 구현 목적 : 유저가 게임을 진행하는데에 있어서 쉽게 게임을 클리어하는데 도움을 주기 위함.
    - 설명
        - 대포
            - 선택한 블록의 세로줄을 모두 없앤다
        - 화살표
            - 선택한 블록의 가로줄을 모두 없앤다
        - 해머
            - 선택한 블록을 없앤다.(특수 블록도 가능)
        - 광대 모자
            - 보드 내의 모든 셀을 셔플한다.(특수 블록은 제외)
- 보드 내 아이템 설명
    - 구현 목적 : 각 매칭되는 상황에 따라서 아이템을 생성하도록하여 다양한 유저 경험을 주기 위함.
    - 설명
        - 로켓(Rocket)
            - 매칭 방법: 같은 색상의 블록을 4개 일렬로 맞추면 생성됩니다.
            - 효과: 로켓은 매칭된 방향에 따라 가로 또는 세로로 이동하며 해당 줄의 모든 블록을 제거합니다.
            - 효과 발동 조건 : 스왑하는 다른 셀이 매칭이되어야지 발동
        - 폭탄(Bomb)
            - 매칭 방법: 같은 색상의 블록을 L 또는 T 모양으로 5개 매칭하면 생성됩니다.
            - 효과: 폭탄은 주변의 블록들을 폭발시키며, 범위 내 모든 블록을 제거합니다.
        - 종(Bell)
            - 매칭 방법: 같은 색상의 블록을 5개 일렬로 맞추면 생성됩니다.
            - 효과: 종은 해당 색상의 모든 블록을 한 번에 제거하며, 강력한 보드 청소 효과를 줍니다.
        - 각 아이템끼리의 조합
            - 로켓 + 폭탄 조합: 로켓과 폭탄을 교환하면 가로와 세로로 동시에 폭발하여 대량의 블록을 제거합니다.
                - 효과 : 타일 주변의 3개 행과 3개 열을 비우는 3x3 줄무늬 캔디를 만듭니다.
            - 로켓 + 종 조합: 로켓과 종을 교환하면 종이 모든 블록을 로켓으로 바꾸며, 이 로켓들이 연속적으로 터집니다.
            - 폭탄 + 종 조합: 폭탄과 종을 교환하면 종이 모든 블록을 폭탄으로 바꾸며, 폭탄들이 동시에 폭발하여 보드 전체를 뒤덮습니다.
            - 로켓 + 로켓: 두 개의 로켓을 교환하면, 교차하여 보드의 가로 세로 줄을 모두 제거합니다.
                - 효과 : 로켓이 결합된 행과 열을 지웁니다. 효과는 줄무늬 방향과 무관합니다.
            - 폭탄 + 폭탄:  폭탄 + 폭탄은 블럭을 중심지에서 5x5 규모로 없앤다.
            - 종 + 종 조합 : 모든 블록을 없앤다.
        - 왕관(Crown)
            - 특별 아이템: 특정 레벨에서는 왕관이 등장하며, 이를 수집하는 것이 목표가 됩니다. 왕관은 주로 보드의 장애물 속에 감춰져 있으며, 매칭을 통해 장애물을 제거하면서 왕관을 얻어야 합니다.

### 셀

- 구현 목적 :
    - 특정 장애물을 제거하거나 특수 블록과 상호작용하는 미션을 만드는 다양한 새로운 목표를 제공한다.
    - 단순한 매칭이아니라 장애물을 제거하거나 특정 블록과의 상호작용을 이용해서 게임 플레이에 전략성을 강화한다.
- 제너레이터 셀
    - 클락 : 부딪치면 스타를 스폰함
- 블록 장애물
    - 박스 : 한번 부딪치면 없어짐
    - 얼음 : 여러번 부딪치면 없어짐
    - 케이지 : 케이지 안에 셀이 존재하고 여러번 부딪치면 케이지가 없어지고 안에 셀을 부술수 있음

### 승리 미션

- 승리 조건(미션)
    - 장애물 제거
        - 맵 내에 존재하는 장애물들을 모두 제거한다.
    - 정해진 블록 수 제거
        - 일반 셀(노란색, 보라색 등)을 일정 수 이상 제거한다.
    - 특정 아이템 수집
        - 블록에 있는 특정 아이템을 일정 이상 수집한다.
- 점수
    - 특정 조건에 의하여 점수를 모을 수 있고 별을 획득 가능함.
    - 별 1 ~ 3개를 모을 수 있으며 목표 점수를 기준으로 0.2 → 1개 0.5 → 2개 1 → 3개
    - 특정 조건 리스트
        - **매칭된 캔디 수**: 기본적으로, 플레이어는 세 개 이상의 동일한 캔디를 매칭하여 점수를 얻습니다.
            - 3개 이상 `30점`
        - **콤보와 연속 매칭**: 연속적으로 매칭을 수행할 경우, 추가 점수를 받을 수 있습니다. 콤보가 증가할수록 점수가 더 높아집니다.
            - 콤보 + `10점`
            - 콤보는 캔디가 매칭되었을 때에만 적용됨
            - `기본 점수 + (콤보 갯수 * 콤보 점수)`
            - ex) 30 + (5 * 10) = 80
        - **특수 캔디**: 특정 조합으로 생성된 특수 캔디(예: 스트라이프 캔디, 폭탄 캔디 등)를 사용할 때는 기본 매칭보다 더 높은 점수를 부여받습니다.
            - 특수 캔디 발동 시 점수 → `50점`
            - 특수 캔디 콤비 조합 발동 시 점수
                - **로켓 + 폭탄 조합**: `200점`
                - **로켓 + 종 조합**: `200점`
                - **폭탄 + 종 조합**:  `200점`
                - **로켓 + 로켓**:  `150점`
                - **폭탄 + 폭탄**: `250점`
                - 종 + 종 조합 : `250점`

