
public static class Const
{
    public static int RemovableMatchedCellCount = 3;
    public static float SwapAnimationDuration = 0.3f;
    public static float CellMoveAnimationDuration = 0.1f;
    public static float CellRemoveAnimationDuration = 0.2f;
    public static float CellConcatAnimationDuration = 0.2f;
    public static float CellMatchedWaitTime = 0.2f;
    public static float CellAlphaAnimation = 0.5f;

    public static int BombRange = 1;
    public static int BombAndBombCombinationRange = 2;

    public static string ObjectPoolConfigDataPath =
        "Assets/Game/03.Resources/Resources/Data/ObjectPoolConfigData.asset";
    
    public static string InGameResourcesConfigData =
        "Assets/Game/03.Resources/Resources/Data/InGameResourcesConfigData.asset";

    public static int MaxUserHeartCount = 5;
    public static int HeartChargeMinute = 1;
    public static int HeartPurchaseCost = 250;

    public static int DefaultHeartCount = 5;
    public static int DefaultMoney = 0;
    public static int MaxStageLevel = 100;
    public static int DefaultInGameItemCount = 3;
    public static int ShowHintTime = 3;

    public static int MatchedCellScore = 30;
    public static int MatchedSpecialCellScore = 50;
    public static int ActivateRocketAndBombScore = 200;
    public static int ActivateRocketAndRocketScore = 150;
    public static int ActivateRocketAndWandScore = 200;
    public static int ActivateBombAndWandScore = 200;
    public static int ActivateBombAndBombScore = 250;
    public static int ActivateWandAndWandScore = 250;
    public static int ComboAddScore = 10;
}