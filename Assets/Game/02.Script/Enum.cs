
public enum BlockType
{
    None,
    Normal
}


public enum CellType
{
    None,
    Normal, //일반형
    Rocket,
    Wand,
    Bomb,
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
    
}