using System;
using System.Collections.Generic;
using ThreeMatch.Firebase.Data;
using ThreeMatch.InGame.Interface;
using UniRx;

namespace ThreeMatch.OutGame.Data
{
    public class DailyRewardModel : IModel
    {
        public readonly ReactiveProperty<List<DailyRewardData>> dailyRewardItemDataList = new();
        public DateTime lastReceivedRewardTime;
        
        public void CreateDailyRewardItemList(DailyRewardHistoryData dailyRewardHistoryData)
        {
            dailyRewardItemDataList.SetValueAndForceNotify(dailyRewardHistoryData.DailyRewardDataList);
            // dailyRewardItemDataList.Value = new List<DailyRewardData>();
            // foreach (DailyRewardData rewardData in dailyRewardHistoryData.DailyRewardDataList)
            // {
            //     DailyRewardData itemData = new DailyRewardData()
            //     {
            //         ItemId = rewardData.ItemId,
            //         IsGetReward = rewardData.IsGetReward,
            //         RewardValue = rewardData.RewardValue
            //     };
            //     
            //     dailyRewardItemDataList.Value.Add(itemData);
            // }

            lastReceivedRewardTime = dailyRewardHistoryData.LastReceivedRewardTime;
        }
    }
}