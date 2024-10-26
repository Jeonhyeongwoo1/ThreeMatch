using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.Core;
using ThreeMatch.Firebase;
using ThreeMatch.Firebase.Data;
using ThreeMatch.InGame.Model;
using ThreeMatch.OutGame.Data;
using ThreeMatch.Server;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.Title.Controller
{
    public class TitleSceneController : MonoBehaviour
    {
        private async void Start()
        {
            await Initialize();
        }

        private async UniTask Initialize()
        {
            var firebaseController = new FirebaseController();
            bool isSuccess = await firebaseController.FirebaseInit();
            if (!isSuccess)
            {
                Debug.LogError("Failed firebase init");
                return;
            }

            if (!firebaseController.HasUserId)
            {
                await firebaseController.SignInAnonymously();
            }

            ServerHandlerFactory.InitializeServerHandlerRequest(firebaseController);
            
            var response = await ServerHandlerFactory.Get<ServerUserRequestHandler>().LoadUserDataRequest(new UserRequest());
            if (response.responseCode != ServerErrorCode.Success)
            {
                Debug.LogError("failed get user data");
                return;
            }
            
            var userModel = ModelFactory.CreateOrGet<UserModel>();
            userModel.heart.Value = response.userData.Heart;
            userModel.money.Value = response.userData.Money;

            var ingameItemModel = ModelFactory.CreateOrGet<InGameItemModel>();
            foreach (InGameItemData itemData in response.inGameItemDataList)
            {
                ingameItemModel.SetInGameItemValue((InGameItemType) itemData.ItemId, itemData.ItemCount);
            }

            var dailyRewardModel = ModelFactory.CreateOrGet<DailyRewardModel>();
            dailyRewardModel.CreateDailyRewardItemList(response.dailyRewardHistoryData);
        }

        public void MoveToMemu()
        {
            StartCoroutine(Wait(2, () => SceneManager.LoadScene(SceneType.StageLevel.ToString())));
        }

        private IEnumerator Wait(float wait, Action action)
        {
            yield return new WaitForSeconds(wait);
            action?.Invoke();
        }
    }
}