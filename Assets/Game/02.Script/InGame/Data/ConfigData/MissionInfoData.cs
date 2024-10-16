using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [Serializable]
    public class MissionInfoData
    {
        [OnValueChanged(nameof(OnMissionTypeValueChanged))]
        public MissionType missionType;

        [OnValueChanged(nameof(OnRemoveCountValueChanged))]
        public int removeCount;

        public Action<MissionInfoData> onChangeMissionInfoData;
        
        private void OnRemoveCountValueChanged()
        {
            Debug.Log(removeCount);
            onChangeMissionInfoData?.Invoke(this);
        }
        
        private void OnMissionTypeValueChanged()
        {
            Debug.Log("OnMissionTypeCahnge" + missionType);
            onChangeMissionInfoData?.Invoke(this);
        }
    }
}