using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [Serializable]
    public class BoardInfoData
    {
        public CellImageType CellImageType => _cellImageType == CellImageType.Blue ||
                                              _cellImageType == CellImageType.Purple
                                                    ? CellImageType.Green
                                                    : _cellImageType;
        public CellType CellType => _cellType;
        public GameObject Prefab => _prefab;
        public BlockType BlockType => _blockType;
        public ObstacleCellType ObstacleCellType => _obstacleCellType;
            
        [PreviewField] [HorizontalGroup("Row", Width = 100)] [SerializeField]
        private GameObject _prefab;

        [SerializeField] private CellType _cellType;
        [SerializeField] private CellImageType _cellImageType;
        [SerializeField] private BlockType _blockType;
        [SerializeField] private ObstacleCellType _obstacleCellType;

        public BoardInfoData(GameObject prefab, CellType cellType, ObstacleCellType obstacleCellType, CellImageType cellImageType)
        {
            _prefab = prefab;
            _cellType = cellType;
            _cellImageType = cellImageType;
            _obstacleCellType = obstacleCellType;
            _blockType = _cellType == CellType.None ? BlockType.None : BlockType.Normal;
        }
    }
}