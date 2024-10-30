using System;

namespace ThreeMatch.Shared.Data
{
    [Serializable]
    public class GameResultData
    {
        public int starCount;
        public int removeCellCount;
        public int[] usedItemCountArray;
    }
}