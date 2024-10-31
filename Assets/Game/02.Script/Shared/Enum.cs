namespace ThreeMatch
{
    public enum BlockType
    {
        None,
        Normal,
    }

    public enum CellType
    {
        None,
        Normal, //일반형
        Rocket,
        Wand,
        Bomb,
        Obstacle,
        Generator
    }

    public enum ObstacleCellType
    {
        None,
        OneHitBox,
        HitableBox,
        Cage
    }

    public enum GeneratorType
    {
        None,
        Star,
    }

    public enum MissionType
    {
        None,
        RemoveNormalRedCell,
        RemoveNormalYellowCell,
        RemoveNormalGreenCell,
        RemoveNormalPurpleCell,
        RemoveNormalBlueCell,
        RemoveObstacleOneHitBoxCell,
        RemoveObstacleHitableBoxCell,
        RemoveObstacleCageCell,
        RemoveStarGeneratorCell,
        RemoveNormalOrangeCell,
    }

    public enum CellImageType
    {
        None = -1,
        Red,
        Yellow,
        Green,
        Purple,
        Blue,
        Orange
    }

    public enum CellMatchedType
    {
        None = 0,
        Three = 3,
        Four = 4, //로켓
        Five = 5,
        Five_OneLine = Five, // 완드
        Five_Shape, // 폭탄 (십자가, T, L 모양일 경우)
        Vertical_Four,
        Horizontal_Four, 
    }

    public enum CellCombinationType
    {
        None = 0,
        RocketAndBomb,
        RocketAndWand,
        RocketAndRocket,
        BombAndWand,
        BombAndBomb,
        WandAndWand,
    }

    public enum InGameItemType
    {
        None = -1,
        Shuffle,
        OneCellRemover,
        VerticalLineRemover,
        HorizontalLineRemover
    }

    public enum BoardState
    {
        None,
        Building,
        CompleteBuild,
        Ready,
        Swapping,
        SwapPostProcess,
        PendingUseInGameItem,
        UseItem,
        ShuffleByUnmatchableCell,
    }

    public enum PoolKeyType
    {
        None = -1,
        CellDisappearParticle,
        CellDisappearLightEffect,
        WandLightEffect,
        RocketEffect,
        BombEffect,
        Cell_Normal,
        Cell_Bomb,
        Cell_Rocket,
        Cell_Wand,
        Cell_Obstacle_HitableBox,
        Cell_Obstacle_Cage,
        Cell_Obstacle_OneHitBox,
        Cell_Generator,
        StarObject,
        StageLevelText,
        ComboCountText
    }

    public enum GameState
    {
        None,
        Ready,
        Start,
        End
    }

    public enum SceneType
    {
        None,
        Title,
        StageLevel,
        InGame
    }
    
    public enum InGameMenuPopupButtonType
    {
        NextStage,
        Share,
        MoveToStageLevelScene,
        RestartGame,
        ClosePopup,
        ShowAd
    }

    public enum AchievementType
    {
        None,
        AttendanceCheck = 1000,
        RemoveCells = 1001,
        UseItem = 1002,
        UseShuffleItem = UseItem,
        UseHammerItem = 1003,
        UseRocketItem = 1004,
        UseBulldozerItem = 1005,
        ReachStage = 1006
    }

    public enum ServerErrorCode
    {
        Success = 100,
        FailedGetData = 200,
        FailedGetUserData = 201,
        NotEnoughHeart,
        MaxHeartCount,
        NotEnoughMoney,
        FailedGetStageData,
        FailedGetDailyReward,
        FailedFirebaseError,
        FailedGetAchievement,
        NotMatchedAchievementId,
        AlreadyAchievementId
    }
}