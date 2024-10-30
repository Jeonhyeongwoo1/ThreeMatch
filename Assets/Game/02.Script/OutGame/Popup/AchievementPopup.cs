using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Interface;
using ThreeMatch.OutGame.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Popup
{
    public class AchievementPopup : MonoBehaviour, IPopup
    {
        public Vector3 GoldTextPosition => _goldAmountText.transform.position;
        
        [SerializeField] private List<AchievementElement> _achievementElementList;
        [SerializeField] private Button _closeButton;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private GameObject _goldViewObj;
        [SerializeField] private TextMeshProUGUI _goldAmountText;

        private Tweener _tweener;
        
        private void Start()
        {
            _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnDisable()
        {
            if (_tweener != null)
            {
                _tweener.Kill(true);
            }

            if (_goldViewObj.activeSelf)
            {
                ActiveGoldObj(false);
            }
        }

        public void Initialize(List<AchievementData> achievementDataList, Action<int, AchievementElement> onRewardGetAction)
        {
            if (_achievementElementList.Count == 0)
            {
                _achievementElementList = GetComponentsInChildren<AchievementElement>(true).ToList();
            }

            for (var i = 0; i < achievementDataList.Count; i++)
            {
                var achievement = achievementDataList[i];
                _achievementElementList[i].Initialize(onRewardGetAction, achievement.AchievementId);
            }
        }

        public void ActiveGoldObj(bool active)
        {
            _goldViewObj.SetActive(active);
        }

        public void UpdateUserGold(long result, long from = 0, bool isAnimation = true)
        {
            if (isAnimation)
            {
                _tweener = DOVirtual.Float(from, result, 1, (v) => _goldAmountText.text = v.ToString("0"));
            }
            else
            {
                _goldAmountText.text = result.ToString();
            }
        }

        public void UpdateAchievementElementList(List<AchievementData> achievementDataList)
        {
            for (var i = 0; i < achievementDataList.Count; i++)
            {
                var achievement = achievementDataList[i];
                _achievementElementList[i].UpdateUI(achievement.Description, achievement.CurrentAchievementValue,
                    achievement.AchievementAimValue, achievement.AchievementRewardAmount, achievement.IsGet);
            }

            int offset = 100;
            _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x,
                (_gridLayoutGroup.cellSize.y + _gridLayoutGroup.spacing.y) * achievementDataList.Count + offset);
        }

    }
}