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
        Box,
        IceBox,
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
        None,
        Shuffle,
        Hammer,
        VerticalRocket,
        HorizontalRocket
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
        StageLevelText
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
}