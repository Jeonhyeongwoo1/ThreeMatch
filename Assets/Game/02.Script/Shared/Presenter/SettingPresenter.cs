using System.Collections;
using System.Collections.Generic;
using ThreeMatch.Core;
using ThreeMatch.InGame.Manager;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;
using ThreeMatch.Shared.Popup;
using ThreeMatch.Shared.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.Shared.Presenter
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