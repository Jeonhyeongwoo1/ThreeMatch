using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Interface;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public enum ButtonType
    {
        NextStage,
        Share,
        MoveToMap,
        RestartGame,
        ClosePopup,
        ShowAd
    }
    
    public class InGameMenuModel : IModel
    {
        [Serializable]
        public struct MenuButtonData
        {
            public ButtonType buttonType;
            public Action callback;
        }
        
        public ReactiveProperty<List<MenuButtonData>> _menuButtonDataList = new();
    
        public InGameMenuModel()
        {
            int length = Enum.GetValues(typeof(ButtonType)).Length;
            for (int i = 0; i < length; i++)
            {
                MenuButtonData data = new MenuButtonData();
                data.buttonType = (ButtonType)i;
                data.callback = null;
            }
        }

        public Action GetCallback(ButtonType buttonType)
        {
            if (!_menuButtonDataList.HasValue)
            {
                Debug.LogError("menu button data list is null");
                return null;
            }

            var data = _menuButtonDataList.Value;
            if (data == null)
            {
                Debug.LogError("menu button data list is null");
                return null;
            }

            return data.Find(v => v.buttonType == buttonType).callback;
        }

        public void AddCallback(ButtonType buttonType, Action callback)
        {
            if (!_menuButtonDataList.HasValue)
            {
                _menuButtonDataList = new ReactiveProperty<List<MenuButtonData>>();
                MenuButtonData buttonData = new MenuButtonData();
                buttonData.buttonType = buttonType;
                buttonData.callback = callback;
                _menuButtonDataList.Value.Add(buttonData);
            }
            else
            {
                _menuButtonDataList.Value.ForEach(v =>
                {
                    if (v.buttonType == buttonType)
                    {
                        v.callback = callback;
                    }
                });
            }
        }
    }
    
    public class InGameMenuPopup : MonoBehaviour
    {
        
        [Serializable]
        public struct ButtonData
        {
            public ButtonType buttonType;
            public Button button;
        }

        [SerializeField] private List<ButtonData> _buttonDataList;
        [SerializeField] private TextMeshProUGUI _stageLevelText;
        [SerializeField] private List<GameObject> _starObjList;

        private void Start()
        {
            var menuButtonModel = ModelFactory.CreateOrGet<InGameMenuModel>();
            foreach (ButtonData data in _buttonDataList)
            {
                var callback = menuButtonModel.GetCallback(data.buttonType);
                data.button.onClick.AddListener(() =>
                {
                    callback?.Invoke();
                    switch (data.buttonType)
                    {
                        case ButtonType.ClosePopup:
                            gameObject.SetActive(false);
                            break;
                        case ButtonType.NextStage:
                        case ButtonType.Share:
                        case ButtonType.MoveToMap:
                        case ButtonType.RestartGame:
                        case ButtonType.ShowAd:
                        default:
                            break;
                    }
                });
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void UpdateStarObj(int targetIndex)
        {
            for (var i = 0; i < _starObjList.Count; i++)
            {
                _starObjList[i].SetActive(targetIndex < i);
            }
        }

        private void UpdateStageLevel(string stageLevel)
        {
            _stageLevelText.text = stageLevel;
        }
    }
}