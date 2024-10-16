using System;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "CellConfigData", menuName = "ThreeMatch/CellConfigData", order = 1)]
    public class CellConfigData : ScriptableObject
    {
        public Sprite BombSprite => _bombSprite;
        public Sprite WandSprite => _wandSprite;
        public Sprite BoxSprite => _boxSprite;
        public Sprite[] IceBoxSpriteArray => _iceBoxSpriteArray;
        public Sprite CageSprite => _cageSprite;
        public Sprite GeneratorSprite => _generatorSprite;
        public GameObject StarPrefab => _starPrefab;
        public GameObject WandIdleParticlePrefab => _wandIdleParticlePrefab;
        
        [Serializable]
        public struct CellImageTypeSpriteData
        {
            public CellImageType cellImageType;
            public Sprite normalSprite;
            public Sprite verticalSprite;
            public Sprite horizontalSprite;
        }

        [SerializeField] private CellImageTypeSpriteData[] _cellImageTypeSpriteDataArray;
        [SerializeField] private Sprite _bombSprite;
        [SerializeField] private Sprite _wandSprite;

        [SerializeField] private Sprite _boxSprite;
        [SerializeField] private Sprite[] _iceBoxSpriteArray;
        [SerializeField] private Sprite _cageSprite;
        [SerializeField] private Sprite _generatorSprite;

        [SerializeField] private GameObject _starPrefab;

        [SerializeField] private GameObject _wandIdleParticlePrefab;

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