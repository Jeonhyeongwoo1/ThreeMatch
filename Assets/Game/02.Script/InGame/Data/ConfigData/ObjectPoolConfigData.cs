using System;
using System.Collections.Generic;
using UnityEditor;
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
        private static ObjectPoolConfigData _instance;

        public static ObjectPoolConfigData Instance
        {
            get
            {
                if (!_instance)
                {
                    ObjectPoolConfigData configData =
                        AssetDatabase.LoadAssetAtPath<ObjectPoolConfigData>(Const.ObjectPoolConfigDataPath);
                    _instance = configData;
                }

                return _instance;
            }
        }
        
        public List<ObjectPoolItemData> ObjectPoolDataList => _objectPoolDataList;
        
        [SerializeField] private List<ObjectPoolItemData> _objectPoolDataList;
    }
}