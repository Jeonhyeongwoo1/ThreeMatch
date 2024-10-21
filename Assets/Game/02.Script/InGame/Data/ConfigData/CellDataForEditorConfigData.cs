using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "CellDataForEditorConfigData", menuName = "ThreeMatch/CellDataForEditorConfigData")]
    public class CellDataForEditorConfigData : ScriptableObject
    {
        [Serializable]
        public class CellInfo
        {
            public CellType cellType;
            public CellImageType cellImageType;
            public ObstacleCellType obstacleCellType;
            public GameObject prefab;
        }

        public List<CellInfo> CellInfoList => _cellInfoList;

        [SerializeField] private List<CellInfo> _cellInfoList;
    }
}