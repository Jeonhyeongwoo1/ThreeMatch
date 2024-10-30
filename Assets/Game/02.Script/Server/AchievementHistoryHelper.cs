using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Keys;
using UnityEngine;

namespace ThreeMatch.Server
{
    public static class AchievementHistoryHelper
    {
        [Serializable]
        public class UpdateAchievementHistoryResultData
        {
            public bool isSuccess;
            public AchievementHistoryData achievementHistoryData;
        }
        
        [Serializable]
        public class UpdatableAchievementData
        {
            public int achievementId;
            public int acquiredAmount;
        }

        public static UpdateAchievementHistoryResultData TryUpdateAchievementHistoryData(
            AchievementHistoryData achievementHistoryData, params UpdatableAchievementData[] updatableAchievementDataList)
        {
            List<AchievementData> achievementDataList = achievementHistoryData.AchievementDataList;
            if (updatableAchievementDataList.Length == 0)
            {
                return new UpdateAchievementHistoryResultData()
                {
                    isSuccess = false,
                    achievementHistoryData = achievementHistoryData
                };
            }
            
            foreach (UpdatableAchievementData data in updatableAchievementDataList)
            {
                AchievementData achievementData = achievementDataList.Find(v => v.AchievementId == data.achievementId);
                if (achievementData == null)
                {
                    Debug.LogError($"Failed get achievement {data.achievementId}");
                    return null;
                }

                int totalAchievementValue = achievementData.CurrentAchievementValue + data.acquiredAmount;
                if (achievementData.IsGet || totalAchievementValue == achievementData.AchievementAimValue)
                {
                    continue;
                    // return new UpdateAchievementHistoryResultData()
                    // {
                    //     isSuccess = false,
                    //     achievementHistoryData = achievementHistoryData
                    // };
                }

                if (achievementData.AchievementAimValue < totalAchievementValue)
                {
                    totalAchievementValue = achievementData.AchievementAimValue;
                }
                
                achievementData.CurrentAchievementValue = totalAchievementValue;   
            }
            
            return new UpdateAchievementHistoryResultData()
            {
                isSuccess = true,
                achievementHistoryData = achievementHistoryData
            };
        }

        // public static async UniTask<UpdateAchievementHistoryResultData> TryUpdateAchievementHistoryDataAsync(FirebaseFirestore db, string userId, int achievementId, int acquiredAmount)
        // {
        //     DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userId);
        //     DocumentSnapshot snapshot = null;
        //     try
        //     {
        //         snapshot = await docRef.GetSnapshotAsync();
        //     }
        //     catch (Exception e)
        //     {
        //         return null;
        //     }
        //
        //     if (snapshot.Exists)
        //     {
        //         return null;
        //     }
        //
        //     if (snapshot.TryGetValue(nameof(AchievementHistoryData), out AchievementHistoryData achievementHistoryData))
        //     {
        //         return AchievementHistoryHelper.TryUpdateAchievementHistoryData(achievementHistoryData, achievementId, acquiredAmount);
        //     }
        //
        //     return null;
        // }
    }
}