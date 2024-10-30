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
            if (_missionElementList.Count > 0)
            {
                _missionElementList.Clear();
                MissionElement[] childs = _missionContainer.GetComponentsInChildren<MissionElement>();
                foreach (MissionElement element in childs)
                {
                    DestroyImmediate(element.gameObject);
                }
            }
            
            missionDataList.ForEach(missionData =>
            {
                MissionElement element = Instantiate(_missionElementPrefab, _missionContainer);
                element.Initialize(missionData.missionType, missionData.count);
                _missionElementList.Add(element);
            });
        }

        public Vector3 GetMissionElementPositionAndUpdateUI(MissionType missionType, string count)
        {
            var missionElement = _missionElementList.Find(v => v.MissionType == missionType);
            if (missionElement == null)
            {
                Debug.LogError($"Failed mission element {missionType}");
                return Vector3.zero;
            }
            
            missionElement.UpdateCountText(count);
            return missionElement.Position;
        }
    }
}