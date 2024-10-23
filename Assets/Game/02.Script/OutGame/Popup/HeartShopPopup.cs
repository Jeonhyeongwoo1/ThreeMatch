using System;
using System.Threading;
using ThreeMatch.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Timer = ThreeMatch.OutGame.Entity.Timer;

namespace ThreeMatch.OutGame.Popup
{
    public class HeartShopPopup : MonoBehaviour, IPopup
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Timer _timer;
        [SerializeField] private Button _buyLiftButton;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _adsButton;

        private Action _onBuyLifeAction;
        private Action _onShowAdAction;
        
        private void Start()
        {
            _closeButton.onClick.AddListener(OnClickCloseButton);
            _buyLiftButton.onClick.AddListener(()=> _onBuyLifeAction.Invoke());
            _adsButton.onClick.AddListener(()=> _onShowAdAction.Invoke());
        }

        private void OnClickCloseButton()
        {
            Close();
        }

        public void Open(DateTime finishTime, string costText, Action done)
        {
            StarHeartChargeTimer(finishTime, done);
            _costText.text = costText;
            gameObject.SetActive(true);
        }
        
        public void Close()
        {
            StopHeartChargeTimer();
            gameObject.SetActive(false);
        }

        public void Initialize(Action onBuyLifeAction, Action onShowAdAction)
        {
            _onBuyLifeAction = onBuyLifeAction;
            _onShowAdAction = onShowAdAction;
        }

        public void StarHeartChargeTimer(DateTime finishTime, Action done)
        {
            _timer.StartTimer(finishTime, done);
        }

        public void StopHeartChargeTimer()
        {
            _timer.StopTimer();
        }
    }
}