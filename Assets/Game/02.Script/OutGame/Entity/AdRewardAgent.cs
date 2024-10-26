using ThreeMatch.Core;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Data;
using ThreeMatch.Server;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Entity
{
    public class AdRewardAgent : MonoBehaviour
    {
        [SerializeField] private Animator _anim;
        [SerializeField] private Button _button;

        private readonly string _showAnimationName = "show";
        
        private void OnEnable()
        {
            _button.interactable = false;
            _button.gameObject.SetActive(false);
            _button.onClick.AddListener(ShowVideoAds);
            Invoke(nameof(Prepare), 2);
        }

        private void Prepare()
        {
            ShowButton();
        }

        private void ShowVideoAds()
        {
            AdManager.Instance.ShowRewardAd(GetReward, null);
        }

        private async void GetReward()
        {
            var response = await ServerHandlerFactory.Get<ServerUserRequestHandler>().GetAdRewardRequest();
            if (response.responseCode != ServerErrorCode.Success)
            {
                switch (response.responseCode)
                {
                    case ServerErrorCode.FailedGetUserData:
                    case ServerErrorCode.FailedFirebaseError:
                        SceneManager.LoadScene(SceneType.Title.ToString());
                        Debug.LogError("error : " + response.errorMessage);
                        return;
                }
            }

            gameObject.SetActive(false);
            var userModel = ModelFactory.CreateOrGet<UserModel>();
            userModel.money.Value += response.money;
        }

        private void ShowButton()
        {
            _button.gameObject.SetActive(true);
            _button.interactable = true;
            _anim.SetTrigger(_showAnimationName);
        }

        public void Hide()
        {
            _button.interactable = false;
        }
    }
}