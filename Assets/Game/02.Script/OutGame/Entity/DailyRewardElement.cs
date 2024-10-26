using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Entity
{
    public class DailyRewardElement : MonoBehaviour
    {
        [SerializeField] private GameObject _checkObj;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Button _getButton;

        private Action<int> _onGetRewardAction;
        private int _itemId;
        
        private void Start()
        {
            _getButton.onClick.AddListener(() => _onGetRewardAction.Invoke(_itemId));
        }

        public void Initialize(int itemId, bool isGetReward, int rewardValue, bool isPossibleGet, Action<int> onGetRewardAction)
        {
            _itemId = itemId;
            _getButton.interactable = isPossibleGet && !isGetReward;
            _checkObj.SetActive(isGetReward);
            _rewardText.text = rewardValue.ToString();
            _onGetRewardAction = onGetRewardAction;
        }
    }
}