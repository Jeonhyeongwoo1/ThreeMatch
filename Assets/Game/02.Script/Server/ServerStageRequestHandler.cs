using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using ThreeMatch.Firebase;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Interface;
using ThreeMatch.Keys;
using ThreeMatch.Shared;

namespace ThreeMatch.Server
{
    public class ServerStageRequestHandler : ServerRequestHandler, IStageClientSender
    {
        public ServerStageRequestHandler(FirebaseController firebaseController) : base(firebaseController)
        {
        }

        public async UniTask<StageResponse> LoadStageDataRequest()
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);

            StageData stageData = null;
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.TryGetValue(nameof(StageData), out stageData))
                {
                    //Initialize
                    stageData = new StageData();
                    stageData.StageLevelDataList = new List<StageLevelData>(Const.MaxStageLevel);
                    for (int i = 0; i < Const.MaxStageLevel; i++)
                    {
                        StageLevelData stageLevelData = new StageLevelData();
                        stageLevelData.Level = i;
                        stageLevelData.IsLock = i != 0;
                        stageLevelData.StarCount = 0;

                        stageData.StageLevelDataList.Add(stageLevelData);
                    }

                    var stageDict = new Dictionary<string, StageData>()
                    {
                        { nameof(StageData), stageData }
                    };
                    
                    await docRef.SetAsync(stageDict, SetOptions.MergeAll);
                }
            }
            catch (Exception e)
            {
                return new StageResponse()
                {
                    responseCode = ServerErrorCode.FailedGetData,
                    errorMessage = e.ToString()
                };
            }

            return new StageResponse()
            {
                stageLevelDataList = stageData.StageLevelDataList
            };
        }

        public async UniTask<StageResponse> StageClearOrFailRequest(int stageLevel, int starCount, bool isClear,
            List<InGameItemData> inGameItemDataList)
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);

            StageData stageData = null;
            DocumentSnapshot snapshot = null;
            try
            {
                snapshot = await docRef.GetSnapshotAsync();
            }
            catch (Exception e)
            {
                return new StageResponse()
                {
                    responseCode = ServerErrorCode.FailedFirebaseError,
                    errorMessage = e.ToString()
                };
            }

            if (!snapshot.TryGetValue(nameof(StageData), out stageData))
            {
                return new StageResponse()
                {
                    responseCode = ServerErrorCode.FailedGetStageData
                };
            }

            var list = stageData.StageLevelDataList;
            var stageLevelData = list.Find(v => v.Level == stageLevel);
            if (stageLevelData.StarCount < starCount && isClear)
            {
                stageLevelData.StarCount = starCount;
            }

            if (isClear)
            {
                var nextStageLevelData = list.Find(v => v.Level == stageLevel + 1);
                nextStageLevelData.IsLock = false;
            }

            var stageDict = new Dictionary<string, object>()
            {
                { nameof(StageData), stageData }
            };

            if (snapshot.TryGetValue(DBFields.InGameItemDataList, out List<InGameItemData> dbItemDataList))
            {
                dbItemDataList = inGameItemDataList;
                stageDict.Add(DBFields.InGameItemDataList, dbItemDataList);
            }

            try
            {
                await docRef.SetAsync(stageDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                return new StageResponse()
                {
                    responseCode = ServerErrorCode.FailedFirebaseError,
                    errorMessage = e.ToString()
                };
            }
            
            return new StageResponse()
            {
                stageLevelDataList = stageData.StageLevelDataList
            };
        }
        
        public async UniTask<UserResponse> RemoveHeartRequest()
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
                return new UserResponse()
                {
                    responseCode = ServerErrorCode.FailedFirebaseError,
                    errorMessage = e.ToString()
                };
            }
            
            if (!snapshot.TryGetValue(nameof(UserData), out UserData userData))
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
                    responseCode = ServerErrorCode.NotEnoughHeart
                };
            }

            userData.Heart--;
            Dictionary<string, UserData> userDict = new Dictionary<string, UserData>()
            {
                { nameof(UserData), userData }
            };
            try
            {
                await docRef.SetAsync(userDict, SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                return new UserResponse()
                {
                    responseCode = ServerErrorCode.FailedFirebaseError,
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