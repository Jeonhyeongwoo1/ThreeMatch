using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "InGameResourcesConfigData", menuName = "ThreeMatch/InGameResourcesConfigData", order = 1)]
    public class InGameResourcesConfigData : ScriptableObject
    {
        [Serializable]
        public struct CellImageTypeSpriteData
        {
            public CellImageType cellImageType;
            public Sprite normalSprite;
        }

        public Sprite[] HitableBoxSpriteArray => _hitableBoxSpriteArray;
        public Sprite ObstacleOneHitBoxSprite => _obstacle_OneHitBoxSprite;
        public Sprite ObstacleHitableBoxSprite => _obstacle_HitableBoxSprite;
        public Sprite ObstacleCageSprite => _obstacle_CageSprite;
        public Sprite StarGeneratorSprite => _starGeneratorSprite;
        public Sprite StarSprite => _starSprite;
        
        [SerializeField] private CellImageTypeSpriteData[] _cellImageTypeSpriteDataArray;
        [SerializeField] private Sprite[] _hitableBoxSpriteArray;
        [SerializeField] private Sprite _obstacle_OneHitBoxSprite;
        [SerializeField] private Sprite _obstacle_HitableBoxSprite;
        [SerializeField] private Sprite _obstacle_CageSprite;
        [SerializeField] private Sprite _starGeneratorSprite;
        [SerializeField] private Sprite _starSprite;
        
        public CellImageTypeSpriteData GetCellImageTypeSpriteData(CellImageType cellImageType)
        {
            foreach (CellImageTypeSpriteData data in _cellImageTypeSpriteDataArray)
            {
                if (data.cellImageType == cellImageType)
                {
                    return data;
                }
            }

            return default;
        }
    }
}