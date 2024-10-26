using System;
using System.Collections.Generic;
using Firebase.Firestore;

namespace ThreeMatch.Firebase.Data
{
    [FirestoreData]
    public class DailyRewardHistoryData
    {
        [FirestoreProperty] public List<DailyRewardData> DailyRewardDataList { get; set; }
        [FirestoreProperty] public DateTime LastReceivedRewardTime { get; set; } = DateTime.UtcNow;
    }

    [FirestoreData]
    public class DailyRewardData
    {
        [FirestoreProperty] public int ItemId { get; set; }
        [FirestoreProperty] public bool IsGetReward { get; set; }
        [FirestoreProperty] public int RewardValue { get; set; }
    }
}