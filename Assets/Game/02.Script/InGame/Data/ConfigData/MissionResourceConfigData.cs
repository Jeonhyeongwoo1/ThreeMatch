using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "MissionResourceConfigData", menuName = "ThreeMatch/MissionResourceConfigData", order = 1)]
    public class MissionResourceConfigData : ScriptableObject
    {
        [Serializable]
        public struct MissionResourceData
        {
            public MissionType missionType;
            public Sprite sprite;
        }

        [SerializeField] private List<MissionResourceData> _missionResourceDataList;

        public Sprite GetMissionSpriteByMissionType(MissionType missionType)
        {
            MissionResourceData data = _missionResourceDataList.Find(v => missionType == v.missionType);
            if (data.Equals(default))
            {
                Debug.LogError($"failed get mission resource data {missionType}");
                return null;
            }

            return data.sprite;
        }
    }
}