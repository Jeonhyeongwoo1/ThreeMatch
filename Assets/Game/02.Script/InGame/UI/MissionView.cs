using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.UI
{
    public class MissionView : MonoBehaviour, IView
    {
        [SerializeField] private MissionElement _missionElementPrefab;
        [SerializeField] private Transform _missionContainer;
        
        [Serializable]
        public struct Data
        {
            public MissionType missionType;
            public int count;
        }
        
        private List<MissionElement> _missionElementList = new();

        public void Initialize(List<Data> missionDataList)
        {
            missionDataList.ForEach(missionData =>
            {
                MissionElement element = Instantiate(_missionElementPrefab, _missionContainer);
                element.Initialize(missionData.missionType, missionData.count.ToString());
                _missionElementList.Add(element);
            });
        }

        public void UpdateUI(MissionType missionType, string count)
        {
            var missionElement = _missionElementList.Find(v => v.MissionType == missionType);
            if (missionElement == null)
            {
                Debug.LogError($"Failed mission element {missionType}");
                return;
            }
            
            missionElement.UpdateCountText(count);
        }
    }
}