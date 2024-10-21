using System;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "CellConfigData", menuName = "ThreeMatch/CellConfigData", order = 1)]
    public class CellConfigData : ScriptableObject
    {
        [Serializable]
        public struct CellImageTypeSpriteData
        {
            public CellImageType cellImageType;
            public Sprite normalSprite;
        }

        public Sprite[] IceBoxSpriteArray => _iceBoxSpriteArray;
        
        [SerializeField] private CellImageTypeSpriteData[] _cellImageTypeSpriteDataArray;
        [SerializeField] private Sprite[] _iceBoxSpriteArray;

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