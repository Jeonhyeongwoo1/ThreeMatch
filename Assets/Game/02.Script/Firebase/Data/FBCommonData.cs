using System.Collections.Generic;
using Firebase.Firestore;

namespace ThreeMatch.Firebase.Data
{
    [FirestoreData]
    public class FBCommonData
    {
        [FirestoreProperty] public string Key { get; set; }
        [FirestoreProperty] public string IV { get; set; }
    }

    [FirestoreData]
    public class AchievementCommonData
    {
        [FirestoreProperty] public List<AchievementCommonElementData> AchievementDataList { get; set; }
    }

    [FirestoreData]
    public class AchievementCommonElementData
    {
        [FirestoreProperty] public int AchievementId { get; set; }
        [FirestoreProperty] public string Description { get; set; }
        [FirestoreProperty] public int AchievementAimValue { get; set; }
        [FirestoreProperty] public int AchievementRewardAmount { get; set; }
    }
}