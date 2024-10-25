using System.Collections.Generic;
using Firebase.Firestore;

namespace ThreeMatch.Firebase.Data
{
    [FirestoreData]
    public class StageLevelData
    {
        [FirestoreProperty] public int Level { get; set; }
        [FirestoreProperty] public bool IsLock { get; set; }
        [FirestoreProperty] public int StarCount { get; set; }
    }

    [FirestoreData]
    public class StageData
    {
        [FirestoreProperty] public List<StageLevelData> StageLevelDataList { get; set; }
    }
}