using System;
using System.Collections.Generic;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Interface;
using ThreeMatch.OutGame.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Popup
{
    public class DailyPopup : MonoBehaviour, IPopup
    {
        [SerializeField] private List<DailyRewardElement> _rewardElementList;
        [SerializeField] private Button _getButton;

        private Action _onGetRewardAction;
        
        private void Start()
        {
            _getButton.onClick.AddListener(OnGetReward);
        }

        private void OnGetReward()
        {
            _onGetRewardAction.Invoke();
        }

        public void Initialize(List<DailyRewardData> popupDataList, bool isPossibleGetReward,
            int possibleGetRewardItemFirstIndex, Action<int> onGetRewardAction, Action onGetReward)
        {
            _onGetRewardAction = onGetReward;
            for (var i = 0; i < _rewardElementList.Count; i++)
            {
                DailyRewardData data = popupDataList[i];
                bool isPossibleGet = isPossibleGetReward && possibleGetRewardItemFirstIndex == i;
                _rewardElementList[i].Initialize(data.ItemId, data.IsGetReward, data.RewardValue, isPossibleGet,
                    onGetRewardAction);
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}