using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [Serializable]
    public class MissionInfoData
    {
        [OnValueChanged(nameof(OnMissionDataValueChanged))]
        public MissionType missionType;

        [OnValueChanged(nameof(OnMissionDataValueChanged))]
        public int removeCount;

        public Action<MissionInfoData> onChangeMissionInfoData;
        
        private void OnMissionDataValueChanged()
        {
            onChangeMissionInfoData?.Invoke(this);
        }
    }
}