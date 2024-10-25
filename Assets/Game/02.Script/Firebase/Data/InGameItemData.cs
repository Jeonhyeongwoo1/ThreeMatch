using System.Collections.Generic;
using Firebase.Firestore;

namespace ThreeMatch.Firebase.Data
{
    [FirestoreData]
    public class InGameItemData
    {
        [FirestoreProperty] public int ItemId { get; set; }
        [FirestoreProperty] public int ItemCount { get; set; }
    }
}