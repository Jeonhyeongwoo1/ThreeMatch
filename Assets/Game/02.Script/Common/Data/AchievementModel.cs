using System.Collections.Generic;
using ThreeMatch.Firebase.Data;
using ThreeMatch.InGame.Interface;
using UniRx;

namespace ThreeMatch.Common.Data
{
    public class AchievementModel : IModel
    {
        public readonly ReactiveProperty<List<AchievementData>> achievementDataList = new();

        public void SetAchievementDataList(List<AchievementData> achievementDataList)
        {
            if (this.achievementDataList.Value == null)
            {
                this.achievementDataList.Value = new List<AchievementData>();
            }
            
            this.achievementDataList.Value = achievementDataList;
        }

        public List<AchievementData> GetAchievementDataList() => achievementDataList.Value;
    }
}