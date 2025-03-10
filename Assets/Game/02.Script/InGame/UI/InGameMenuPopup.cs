using System;
using System.Collections.Generic;
using ThreeMatch.Core;
using ThreeMatch.InGame.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public class InGameMenuPopup : MonoBehaviour
    {
        [Serializable]
        public struct ButtonData
        {
            public InGameMenuPopupButtonType inGameMenuPopupButtonType;
            public Button button;
        }

        [SerializeField] private List<ButtonData> _buttonDataList;
        [SerializeField] private TextMeshProUGUI _stageLevelText;
        [SerializeField] private List<GameObject> _starObjList;

        private void Start()
        {
            var menuButtonModel = ModelFactory.CreateOrGet<InGameMenuButtonModel>();
            foreach (ButtonData data in _buttonDataList)
            {
                var callback = menuButtonModel.GetCallback(data.inGameMenuPopupButtonType);
                data.button.onClick.AddListener(() =>
                {
                    callback.Invoke();
                    switch (data.inGameMenuPopupButtonType)
                    {
                        case InGameMenuPopupButtonType.MoveToStageLevelScene:
                        case InGameMenuPopupButtonType.NextStage:
                        case InGameMenuPopupButtonType.ClosePopup:
                            gameObject.SetActive(false);
                            break;
                        case InGameMenuPopupButtonType.RestartGame:
                        case InGameMenuPopupButtonType.Share:
                        case InGameMenuPopupButtonType.ShowAd:
                        default:
                            break;
                    }
                });
            }
        }

        public void Show(int starCount, string stageLevel)
        {
            gameObject.SetActive(true);
            UpdateStarObj(starCount);
            UpdateStageLevel(stageLevel);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void UpdateStarObj(int targetIndex)
        {
            for (var i = 0; i < _starObjList.Count; i++)
            {
                _starObjList[i].SetActive(targetIndex - 1 >= i);
            }
        }

        private void UpdateStageLevel(string stageLevel)
        {
            _stageLevelText.text = stageLevel;
        }
    }
}