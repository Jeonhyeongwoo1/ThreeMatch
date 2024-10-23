using System;
using UniRx;

namespace ThreeMatch.OutGame.Data
{
    [Serializable]
    public class StageLevelModel
    {
        public int level;
        public bool isLock = true;
        public int starCount;
    }
}
