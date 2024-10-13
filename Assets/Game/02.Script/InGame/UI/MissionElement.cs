using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public class MissionElement : MonoBehaviour
    {
        public MissionType MissionType => _missionType;
        
        [SerializeField] private MissionResourceConfigData _missionResourceData;
        [SerializeField] private Text _countText;
        [SerializeField] private Image _missionIconImage;

        private MissionType _missionType;
        
        public void Initialize(MissionType missionType, string count)
        {
            var sprite = _missionResourceData.GetMissionSpriteByMissionType(missionType);
            _missionIconImage.sprite = sprite;
            _countText.text = count;
            _missionType = missionType;
        }

        public void UpdateCountText(string count)
        {
            _countText.text = count;
        }
    }
}