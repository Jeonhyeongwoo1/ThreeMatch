using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [Serializable]
    public class ObjectPoolItemData
    {
        public int poolCount;
        public PoolKeyType poolKeyType;
        public string parentName;
        public GameObject prefab;
    }
    
    [CreateAssetMenu(fileName = "ObjectPoolConfigData", menuName = "ThreeMatch/ObjectPoolConfigData", order = 1)]
    public class ObjectPoolConfigData : ScriptableObject
    {
        public List<ObjectPoolItemData> ObjectPoolDataList => _objectPoolDataList;
        
        [SerializeField] private List<ObjectPoolItemData> _objectPoolDataList;
    }
}