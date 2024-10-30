using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.Common.View
{
    public class SettingView : MonoBehaviour, IView
    {
        [SerializeField] private GameObject _settingCircleObj;
        [SerializeField] private Button _settingCircelOpenButton;
        [SerializeField] private Button _sfxButton;
        [SerializeField] private Button _bgmButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _sfxOnImage;
        [SerializeField] private Image _bgmOnImage;

        private Action _onSfxToggledAction;
        private Action _onBgmToggleAction;
        private Action _onOpenTutorialPopupAction;
        private Action _onCloseGameAction;

        private void Awake()
        {
            _settingCircelOpenButton.onClick.AddListener(()=> _settingCircleObj.SetActive(true));
            _sfxButton.onClick.AddListener(OnClickSfxButton);
            _bgmButton.onClick.AddListener(OnClickBGMButton);
            _infoButton.onClick.AddListener(()=> _onOpenTutorialPopupAction.Invoke());
            _closeButton.onClick.AddListener(()=> _onCloseGameAction.Invoke());
        }

        private void OnClickSfxButton()
        {
            _sfxOnImage.enabled = !_sfxOnImage.enabled;
            _onSfxToggledAction.Invoke();
        }

        private void OnClickBGMButton()
        {
            _bgmOnImage.enabled = !_bgmOnImage.enabled;
            _onBgmToggleAction.Invoke();
        }

        public void Initialize(Action onSfxToggledAction, Action onBgmToggleAction, Action onOpenTutorialPopupAction,
            Action onCloseGameAction)
        {
            _onSfxToggledAction = onSfxToggledAction;
            _onBgmToggleAction = onBgmToggleAction;
            _onOpenTutorialPopupAction = onOpenTutorialPopupAction;
            _onCloseGameAction = onCloseGameAction;
        }
    }
}