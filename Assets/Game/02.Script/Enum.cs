
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
    Obstacle_Cage
}

public enum CellImageType
{
    None = -1,
    Blue,
    Green,
    Pink,
    Purple,
    Red
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