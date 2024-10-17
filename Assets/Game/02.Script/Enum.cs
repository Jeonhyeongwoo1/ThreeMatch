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
        Obstacle_Box,
        Obstacle_IceBox,
        Obstacle_Cage,
        Generator
    }

    public enum MissionType
    {
        None,
        RemoveNormalBlueCell,
        RemoveNormalGreenCell,
        RemoveNormalPinkCell,
        RemoveNormalPurpleCell,
        RemoveNormalRedCell,
        RemoveObstacleBoxCell,
        RemoveObstacleIceBoxCell,
        RemoveObstacleCageCell,
        RemoveGeneratorCell
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
        CellDisappearParticle,
        CellDisappearLightEffect,
        WandLightEffect,
        RocketEffect,
        BombEffect,
    }

    public enum GameState
    {
        None,
        Ready,
        Start,
        End
    }
}