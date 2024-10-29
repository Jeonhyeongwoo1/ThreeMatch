using Firebase.Firestore;

namespace ThreeMatch.Firebase.Data
{
    [FirestoreData]
    public class FBCommonData
    {
        [FirestoreProperty] public string Key { get; set; }
        [FirestoreProperty] public string IV { get; set; }
    }
}