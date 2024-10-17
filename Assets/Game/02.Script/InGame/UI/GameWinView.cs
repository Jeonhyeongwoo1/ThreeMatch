using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.UI
{
    public class GameWinView : MonoBehaviour, IView
    {
        [SerializeField] private GameObject _completeLevelObj;
        [SerializeField] private GameObject _preCompletedMissionPopupObj;
        [SerializeField] private MissionView _missionView;
        [SerializeField] private InGameMenuPopup _inGameWinMenuPopup;

        private UniTaskCompletionSource _finishedPreCompletedMissionAnimationTask;

        public void ShowPreCompletedMissionPopupObj(UniTaskCompletionSource finishedPreCompletedMissionAnimationTask,
            List<MissionView.Data> missionDataList = null)
        {
            if (missionDataList != null)
            {
                _missionView.Initialize(missionDataList);
            }

            _finishedPreCompletedMissionAnimationTask = finishedPreCompletedMissionAnimationTask;
            _preCompletedMissionPopupObj.SetActive(true);
        }

        public void ShowAndHideCompleteLevelObj(bool isActive)
        {
            _completeLevelObj.SetActive(isActive);
        }

        public void ShowAndHideInGameWinMenuPopup(bool isActive)
        {
            if (isActive)
            {
                _inGameWinMenuPopup.Show();
            }
            else
            {
                _inGameWinMenuPopup.Hide();
            }
        }
        
        public void OnFinishedPreCompletedMissionAnimation()
        {
            _finishedPreCompletedMissionAnimationTask?.TrySetResult();
        }
    }
}