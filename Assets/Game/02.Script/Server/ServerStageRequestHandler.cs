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

        public async UniTask<StageResponse> UpdateStageLevelRequest(int stageLevel, int starCount)
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            DocumentReference docRef = db.Collection(DBKeys.UserDB).Document(userID);

            StageData stageData = null;
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.TryGetValue(nameof(StageData), out stageData))
                {
                    var list = stageData.StageLevelDataList;
                    var stageLevelData = list.Find(v => v.Level == stageLevel);
                    stageLevelData.StarCount = starCount;

                    var nextStageLevelData = list.Find(v => v.Level == stageLevel + 1);
                    nextStageLevelData.IsLock = false;
                    
                    var stageDict = new Dictionary<string, StageData>()
                    {
                        { nameof(StageData), stageData }
                    };
                    
                    await docRef.SetAsync(stageDict, SetOptions.MergeAll);
                }
                else
                {
                    return new StageResponse()
                    {
                        responseCode = ServerErrorCode.FailedGetStageData
                    };
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
    }
}