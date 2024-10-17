using DG.Tweening;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.UI
{
    public class GameFailedView : MonoBehaviour, IView
    {
        [SerializeField] private GameObject _failAlarmTextObj;
        [SerializeField] private InGameMenuPopup _inGameMenuPopup;
        
        public void ShowAndHideFailTextObj(bool isActive)
        {
            _failAlarmTextObj.SetActive(isActive);
        }

        public void ShowAndHideFailPopupObj(bool isActive, bool isShowAdsObj)
        {
            _inGameMenuPopup.Show();
        }

        public void CloseFailPopup()
        {
            _inGameMenuPopup.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
            {
                _inGameMenuPopup.Hide();
                _inGameMenuPopup.transform.localScale = Vector3.one;
            });
        }
    }
}