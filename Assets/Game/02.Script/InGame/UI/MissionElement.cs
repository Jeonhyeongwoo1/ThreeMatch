using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public class MissionElement : MonoBehaviour
    {
        public MissionType MissionType => _missionType;
        
        [SerializeField] private MissionResourceConfigData _missionResourceData;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _missionIconImage;
        [SerializeField] private GameObject _checkObj;
        [SerializeField] private GameObject _unCheckObj;
        
        private MissionType _missionType;
        
        public void Initialize(MissionType missionType, int count)
        {
            bool isClear = count == 0;
            var sprite = _missionResourceData.GetMissionSpriteByMissionType(missionType);
            _missionIconImage.sprite = sprite;
            _missionType = missionType;

            if (isClear)
            {
                _countText.gameObject.SetActive(false);
            }
            else
            {
                _countText.text = count.ToString();
            }
            
            _checkObj.SetActive(isClear);
            _unCheckObj.SetActive(!isClear);
        }

        public void UpdateCountText(string count)
        {
            _countText.text = count;
        }
    }
}