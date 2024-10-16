using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "Cell Data", menuName = "ThreeMatch/Cell Data")]
    public class CellData : ScriptableObject
    {
        [Serializable]
        public class CellInfo
        {
            public CellType cellType;
            public CellImageType cellImageType;
            public GameObject prefab;
        }

        public List<CellInfo> CellInfoList => _cellInfoList;

        [SerializeField] private List<CellInfo> _cellInfoList;
    }
}