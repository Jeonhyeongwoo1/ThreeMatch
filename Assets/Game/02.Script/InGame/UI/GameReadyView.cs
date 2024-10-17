using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.UI
{
    public class GameReadyView : MonoBehaviour, IView
    {
        [SerializeField] private MissionView _missionView;

        private UniTaskCompletionSource _taskCompletionSource;
        
        public void Show(List<MissionView.Data> missionViewDataList, UniTaskCompletionSource taskCompletionSource)
        {
            _missionView.Initialize(missionViewDataList);
            gameObject.SetActive(true);
            _taskCompletionSource = taskCompletionSource;
        }

        public void OnFinishedAnimation()
        {
            Debug.Log("SS" + _taskCompletionSource);
            if (_taskCompletionSource != null)
            {
                _taskCompletionSource.TrySetResult();
            }
            gameObject.SetActive(false);
        }
    }
}