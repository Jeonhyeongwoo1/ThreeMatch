using System.Collections.Generic;
using Firebase.Firestore;

namespace ThreeMatch.Firebase.Data
{
    [FirestoreData]
    public class AchievementHistoryData
    {
        [FirestoreProperty] public List<AchievementData> AchievementDataList { get; set; }
    }
    
    [FirestoreData]
    public class AchievementData
    {
        [FirestoreProperty] public int AchievementId { get; set; }
        [FirestoreProperty] public string Description { get; set; }
        [FirestoreProperty] public int CurrentAchievementValue { get; set; }
        [FirestoreProperty] public int AchievementAimValue { get; set; }
        [FirestoreProperty] public int AchievementRewardAmount { get; set; }
        [FirestoreProperty] public bool IsGet { get; set; }
    }
}