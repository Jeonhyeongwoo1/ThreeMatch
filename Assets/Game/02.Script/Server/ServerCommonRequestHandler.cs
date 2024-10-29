using System;
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
    public class ServerCommonRequestHandler : ServerRequestHandler, ICommonClientSender
    {
        public ServerCommonRequestHandler(FirebaseController firebaseController) : base(firebaseController)
        {
        }
        
        public async UniTask<CommonResponse> GetConstDataRequest()
        {
            string userID = _firebaseController.UserId;
            FirebaseFirestore db = _firebaseController.DB;
            
            DocumentReference docRef = db.Collection(DBKeys.ConstDB).Document(userID);
            FBCommonData fbCommonData = null;
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.TryGetValue(nameof(FBCommonData), out fbCommonData))
                {
                    return new CommonResponse()
                    {
                        responseCode = ServerErrorCode.FailedGetData
                    };
                }
            }
            catch (Exception e)
            {
                Debug.LogError("failed get data :" + e);
                return new CommonResponse()
                {
                    responseCode = ServerErrorCode.FailedGetData
                };
            }

            return new CommonResponse()
            {
                responseCode = ServerErrorCode.Success,
                fbCommonData = fbCommonData,
            };
        }
    }
}