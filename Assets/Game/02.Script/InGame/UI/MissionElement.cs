using ThreeMatch.InGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public class MissionElement : MonoBehaviour
    {
        public MissionType MissionType => _missionType;

        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }
        
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _missionIconImage;
        [SerializeField] private Image _missionIconImage2;
        [SerializeField] private GameObject _checkObj;
        [SerializeField] private GameObject _unCheckObj;
        [FormerlySerializedAs("_inGameResourcesConfigData")] [SerializeField] private GameResourcesConfigData gameResourcesConfigData;
        
        private MissionType _missionType;
        
        public void Initialize(MissionType missionType, int count)
        {
            bool isClear = count == 0;
            _missionType = missionType;
            var sprite = GetMissionSprite();
            Debug.Log($"mission : {missionType} / sprite {sprite}");
            _missionIconImage.sprite = sprite;
            
            if (missionType == MissionType.RemoveStarGeneratorCell)
            {
                _missionIconImage2.sprite = gameResourcesConfigData.StarSprite;
            }

            _countText.text = count.ToString();
            _countText.gameObject.SetActive(!isClear);
            _missionIconImage2.gameObject.SetActive(missionType == MissionType.RemoveStarGeneratorCell);
            _checkObj.SetActive(isClear);
        }

        private Sprite GetMissionSprite()
        {
            Sprite sprite = null;
            GameResourcesConfigData data = gameResourcesConfigData;
            switch (_missionType)
            {
                case MissionType.RemoveNormalRedCell:
                     sprite = data.GetCellImageTypeSpriteData(CellImageType.Red).normalSprite;
                     return sprite;
                case MissionType.RemoveNormalYellowCell:
                    sprite = data.GetCellImageTypeSpriteData(CellImageType.Yellow).normalSprite;
                    return sprite;
                case MissionType.RemoveNormalGreenCell:
                    sprite = data.GetCellImageTypeSpriteData(CellImageType.Green).normalSprite;
                    return sprite;
                case MissionType.RemoveNormalPurpleCell:
                    sprite = data.GetCellImageTypeSpriteData(CellImageType.Purple).normalSprite;
                    return sprite;
                case MissionType.RemoveNormalBlueCell:
                    sprite = data.GetCellImageTypeSpriteData(CellImageType.Blue).normalSprite;
                    return sprite;
                case MissionType.RemoveObstacleOneHitBoxCell:
                    sprite = data.ObstacleOneHitBoxSprite;
                    return sprite;
                case MissionType.RemoveObstacleHitableBoxCell:
                    sprite = data.ObstacleHitableBoxSprite;
                    return sprite;
                case MissionType.RemoveObstacleCageCell:
                    sprite = data.ObstacleCageSprite;
                    return sprite;
                case MissionType.RemoveStarGeneratorCell:
                    sprite = data.StarGeneratorSprite;
                    return sprite;
            }

            return null;
        }

        public void FailedMission()
        {
            _unCheckObj.SetActive(true);
            _checkObj.SetActive(false);
            _countText.gameObject.SetActive(false);
        }

        public void UpdateCountText(string count)
        {
            _countText.text = count;
        }
    }
}