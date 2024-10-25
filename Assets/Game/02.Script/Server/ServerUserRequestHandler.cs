using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using ThreeMatch.Firebase;
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
            try
            {
                var snapshot =  await docRef.GetSnapshotAsync();
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
                    responseCode = ServerErrorCode.FailedGetData
                };
            }

            return new UserResponse()
            {
                responseCode = ServerErrorCode.Success,
                userData = userData
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
                Debug.Log($" {userData.Heart} / {chargedCount}");
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
    }
}