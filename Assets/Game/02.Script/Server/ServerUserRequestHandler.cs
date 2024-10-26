using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using ThreeMatch.Firebase;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Interface;
using ThreeMatch.Keys;
using ThreeMatch.Shared;
using UnityEngine;

namespace ThreeMatch.Server
{
    public class ServerUserRequestHandler : ServerRequestHandler, IUserClientSender
    {
        public ServerUserRequestHandler(FirebaseController firebaseController) : base(firebaseController)
        {
        }

        public async UniTask<UserResponse> SelectStageRequest()
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);
            
            UserData userData = null;
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.TryGetValue(nameof(UserData), out userData))
                {
                    return new UserResponse()
                    {
                        responseCode = ServerErrorCode.FailedGetUserData,
                    };
                }

                if (userData.Heart <= 0)
                {
                    return new UserResponse()
                    {
                        responseCode = ServerErrorCode.FailedGetData
                    };
                }

                userData.Heart--;
                Dictionary<string, UserData> userDict = new Dictionary<string, UserData>()
                {
                    { nameof(UserData), userData }
                };
                
                await docRef.SetAsync(userDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                return new UserResponse()
                {
                    responseCode = ServerErrorCode.FailedGetData,
                    errorMessage = e.ToString()
                };
            }

            return new UserResponse()
            {
                responseCode = ServerErrorCode.Success,
                userData = userData
            };
        }

        public async UniTask<UserResponse> LoadUserDataRequest(UserRequest request)
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);
            UserData userData = null;
            List<InGameItemData> inGameItemDataList = null;
            DailyRewardHistoryData dailyRewardHistoryData = null;
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.TryGetValue(nameof(UserData), out userData))
                {
                    //Init
                    userData = new UserData()
                    {
                        UserId = userID,
                        LastLoginTime = DateTime.UtcNow,
                        Heart = Const.DefaultHeartCount,
                        Money = Const.DefaultMoney
                    };
                }
                else
                {
                    userData.LastLoginTime = DateTime.UtcNow;
                }

                Dictionary<string, object> userDict = new Dictionary<string, object>();
                userDict.Add(nameof(UserData), userData);
                
                if (!snapshot.TryGetValue(DBFields.InGameItemDataList, out inGameItemDataList))
                {
                    int length = Enum.GetValues(typeof(InGameItemType)).Length;
                    inGameItemDataList = new List<InGameItemData>();
                    for (int i = 0; i < length; i++)
                    {
                        if (i == (int)InGameItemType.None)
                        {
                            continue;
                        }
                        
                        var data = new InGameItemData
                        {
                            ItemId = i,
                            ItemCount = Const.DefaultInGameItemCount
                        };
                
                        inGameItemDataList.Add(data);
                    }
                
                    userDict.Add(DBFields.InGameItemDataList, inGameItemDataList);
                }

                if (!snapshot.TryGetValue(nameof(DailyRewardHistoryData),
                        out  dailyRewardHistoryData))
                {
                    const int daily = 7;
                    dailyRewardHistoryData = new DailyRewardHistoryData
                    {
                        DailyRewardDataList = new List<DailyRewardData>(daily)
                    };
                    
                    for (int i = 0; i < 7; i++)
                    {
                        var data = new DailyRewardData
                        {
                            ItemId = i,
                            IsGetReward = false,
                            RewardValue = 10
                        };
                        
                        dailyRewardHistoryData.DailyRewardDataList.Add(data);
                    }
                    
                    userDict.Add(nameof(DailyRewardHistoryData), dailyRewardHistoryData);
                }

                await docRef.SetAsync(userDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                Debug.LogError("failed get data :" + e);
                return new UserResponse()
                {
                    responseCode = ServerErrorCode.FailedGetData
                };
            }

            return new UserResponse()
            {
                responseCode = ServerErrorCode.Success,
                userData = userData,
                inGameItemDataList = inGameItemDataList,
                dailyRewardHistoryData = dailyRewardHistoryData
            };
        }

        public async UniTask<UserResponse> ChargedHeartRequest()
        {
            int chargedCount = 1;
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);
            
            UserData userData = null;
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.TryGetValue(nameof(UserData), out userData))
                {
                    return new UserResponse()
                    {
                        responseCode = ServerErrorCode.FailedGetUserData,
                    };
                }

                if (userData.Heart >= Const.MaxUserHeartCount)
                {
                    return new UserResponse()
                    {
                        responseCode = ServerErrorCode.MaxHeartCount
                    };
                }

                userData.Heart += chargedCount;
                Dictionary<string, UserData> userDict = new Dictionary<string, UserData>()
                {
                    { nameof(UserData), userData }
                };
                
                await docRef.SetAsync(userDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                return new UserResponse()
                {
                    responseCode = ServerErrorCode.FailedGetData,
                    errorMessage = e.ToString()
                };
            }

            return new UserResponse()
            {
                responseCode = ServerErrorCode.Success,
                userData = userData
            };
        }

        public async UniTask<UserResponse> BuyHeartRequest()
        {
            int cost = 250;
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);
            
            UserData userData = null;
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.TryGetValue(nameof(UserData), out userData))
                {
                    return new UserResponse()
                    {
                        responseCode = ServerErrorCode.FailedGetUserData,
                    };
                }

                if (userData.Money < cost)
                {
                    return new UserResponse()
                    {
                        responseCode = ServerErrorCode.NotEnoughMoney
                    };
                }

                if (userData.Heart >= Const.MaxUserHeartCount)
                {
                    return new UserResponse()
                    {
                        responseCode = ServerErrorCode.MaxHeartCount
                    };
                }

                userData.Money -= cost;
                userData.Heart++;
                Dictionary<string, UserData> userDict = new Dictionary<string, UserData>()
                {
                    { nameof(UserData), userData }
                };
                
                await docRef.SetAsync(userDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                return new UserResponse()
                {
                    responseCode = ServerErrorCode.FailedGetData,
                    errorMessage = e.ToString()
                };
            }

            return new UserResponse()
            {
                responseCode = ServerErrorCode.Success,
                userData = userData
            };
        }

        public async UniTask<DailyRewardHistoryResponse> GetDailyRewardRequest(int itemId)
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);
            
            DocumentSnapshot snapshot = null;
            try
            {
                snapshot = await docRef.GetSnapshotAsync();
            }
            catch (Exception e)
            {
                return new DailyRewardHistoryResponse()
                {
                    responseCode = ServerErrorCode.FailedFirebaseError,
                };
            }
            
            if (!snapshot.TryGetValue(nameof(UserData), out UserData userData))
            {
                return new DailyRewardHistoryResponse()
                {
                    responseCode = ServerErrorCode.FailedGetUserData,
                };
            }

            if (!snapshot.TryGetValue(nameof(DailyRewardHistoryData),
                    out DailyRewardHistoryData dailyRewardHistoryData))
            {
                return new DailyRewardHistoryResponse()
                {
                    responseCode = ServerErrorCode.FailedGetData
                };
            }

            var rewardDataList = dailyRewardHistoryData.DailyRewardDataList;
            var dailyRewardData = rewardDataList.Find(v => v.ItemId == itemId);
            if (dailyRewardData == null)
            {
                return new DailyRewardHistoryResponse()
                {
                    responseCode = ServerErrorCode.FailedGetDailyReward
                };
            }

            dailyRewardHistoryData.LastReceivedRewardTime = DateTime.UtcNow;
            dailyRewardData.IsGetReward = true;
            
            int rewardValue = dailyRewardData.RewardValue;
            userData.Money += rewardValue;

            Dictionary<string, object> userDict = new Dictionary<string, object>();
            userDict.Add(nameof(UserData), userData);
            userDict.Add(nameof(DailyRewardHistoryData), dailyRewardHistoryData);
            try
            {
                await docRef.SetAsync(userDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                return new DailyRewardHistoryResponse()
                {
                    responseCode = ServerErrorCode.FailedFirebaseError,
                };
            }

            return new DailyRewardHistoryResponse()
            {
                dailyRewardHistoryData = dailyRewardHistoryData,
                userMoney = userData.Money
            };
        }

        public async UniTask<AdRewardResponse> GetAdRewardRequest()
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);
            
            DocumentSnapshot snapshot = null;
            try
            {
                snapshot = await docRef.GetSnapshotAsync();
            }
            catch (Exception e)
            {
                return new AdRewardResponse()
                {
                    errorMessage = e.ToString(),
                    responseCode = ServerErrorCode.FailedFirebaseError,
                };
            }
      
            if (!snapshot.TryGetValue(nameof(UserData), out UserData userData))
            {
                return new AdRewardResponse()
                {
                    errorMessage = "Failed get user data",
                    responseCode = ServerErrorCode.FailedGetUserData,
                };
            }

            const int reward = 250;
            userData.Money += reward;
            
            Dictionary<string, object> userDict = new Dictionary<string, object>();
            userDict.Add(nameof(UserData), userData);

            try
            {
                await docRef.SetAsync(userDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                return new AdRewardResponse()
                {
                    errorMessage = e.ToString(),
                    responseCode = ServerErrorCode.FailedFirebaseError
                };
            }

            return new AdRewardResponse()
            {
                money = reward
            };
        }
    }
}