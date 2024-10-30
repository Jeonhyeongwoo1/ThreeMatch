using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Entity
{
    public class AchievementElement : MonoBehaviour
    {
        public Vector3 GoldSpawnPosition => _getButton.transform.position;
        
        [SerializeField] private Image _progressbarImage;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _getButton;
        [SerializeField] private TextMeshProUGUI _rewardAmountText;
        [SerializeField] private TextMeshProUGUI _prograssText;
        [SerializeField] private GameObject _rewardObj;

        private Action<int, AchievementElement> _onRewardGetAction;
        private int _itemId;
        
        private void Start()
        {
            _getButton.onClick.AddListener(()=> _onRewardGetAction.Invoke(_itemId, this));
        }

        public void Initialize(Action<int, AchievementElement> onRewardGetAction, int itemId)
        {
            _onRewardGetAction = onRewardGetAction;
            _itemId = itemId;
        }

        public void UpdateUI(string description, int currentAmount, int aimAmount, int rewardAmount, bool isGet)
        {
            if (isGet)
            {
                gameObject.SetActive(false);
                return;
            }
            
            _descriptionText.text = description;
            bool isPossibleGet = currentAmount >= aimAmount;
            float ratio = (float)currentAmount / aimAmount;
            _progressbarImage.fillAmount = ratio;
            _getButton.gameObject.SetActive(isPossibleGet);
            _rewardObj.SetActive(!isPossibleGet);
            _prograssText.text = $"{currentAmount} / {aimAmount}";
            _rewardAmountText.text = rewardAmount.ToString();
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }
}