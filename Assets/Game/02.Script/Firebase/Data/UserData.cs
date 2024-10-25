using System;
using Firebase.Firestore;

[FirestoreData]
public class UserData
{
    [FirestoreProperty]
    public string UserId { get; set; }
    
    [FirestoreProperty]
    public DateTime LastLoginTime { get; set; }
    
    [FirestoreProperty]
    public int Heart { get; set; }
    
    [FirestoreProperty]
    public long Money { get; set; }
}
