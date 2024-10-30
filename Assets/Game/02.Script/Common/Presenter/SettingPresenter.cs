using ThreeMatch.Common.Popup;
using ThreeMatch.Common.View;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.Common.Presenter
{
    public class SettingPresenter : BasePresenter
    {
        private SettingView _view;

        public void Initialize(SettingView view)
        {
            _view = view;
            _view.Initialize(OnSfxToggled, OnBgmToggled, OnOpenTutorialPopup, OnCloseGame);
        }

        private void OnCloseGame()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == SceneType.StageLevel.ToString())
            {
                Application.Quit();
            }
            else if (scene.name == SceneType.InGame.ToString())
            {
                SceneManager.LoadScene(SceneType.StageLevel.ToString());
            }
        }

        private void OnOpenTutorialPopup()
        {
            var popup = PopupManager.Instance.GetPopup<TutorialPopup>();
            if (popup != null)
            {
                popup.OpenPopup();
            }
        }

        private void OnBgmToggled()
        {
            bool isOn = PlayerPrefs.GetInt(nameof(PlayerPrefsKeys.Sound)) == 0;
            PlayerPrefs.SetInt(nameof(PlayerPrefsKeys.Sound), isOn ? 1 : 0);
        }
        
        private void OnSfxToggled()
        {
            bool isOn = PlayerPrefs.GetInt(nameof(PlayerPrefsKeys.SFX)) == 0;
            PlayerPrefs.SetInt(nameof(PlayerPrefsKeys.SFX), isOn ? 1 : 0);
        }
        
    }
}