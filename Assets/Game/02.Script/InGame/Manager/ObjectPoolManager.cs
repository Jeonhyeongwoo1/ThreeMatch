using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.Manager
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ObjectPoolManager>();
                }

                return _instance;
            }
        }

        private static ObjectPoolManager _instance;

        [SerializeField] private ObjectPoolConfigData _poolConfigData;

        private Dictionary<PoolKeyType, Queue<IPoolable>> _poolDict = new();
        private List<Transform> poolParentList;

        private void Awake()
        {
            poolParentList = GetComponentsInChildren<Transform>().ToList();
            Initialize();
        }

        private void Initialize()
        {
            var poolDataList = _poolConfigData.ObjectPoolDataList;
            foreach (ObjectPoolItemData itemData in poolDataList)
            {
                CreatePool(itemData);
            }
        }

        private void CreatePool(ObjectPoolItemData itemData)
        {
            GameObject prefab = itemData.prefab;
            string parentName = itemData.parentName;
            Transform parent = poolParentList.Find(v => v.name == parentName);
            int count = itemData.poolCount;
            PoolKeyType poolKeyType = itemData.poolKeyType;
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                obj.name = prefab.name;
                    
                if (!obj.TryGetComponent(out IPoolable poolable))
                {
                    Debug.LogWarning($"failed get pool component {obj.name}");
                    continue;
                }

                poolable.PoolKeyType = poolKeyType;
                if (_poolDict.TryGetValue(poolKeyType, out Queue<IPoolable> queue))
                {
                    queue.Enqueue(poolable);
                    continue;
                }
                
                queue = new Queue<IPoolable>();
                queue.Enqueue(poolable);
                _poolDict.Add(poolKeyType, queue);
            }
        }

        private ObjectPoolItemData GetObjectPoolItemData(PoolKeyType poolKeyType)
        {
            return _poolConfigData.ObjectPoolDataList.Find(v => v.poolKeyType == poolKeyType);
        }

        private bool TryCreatePoolItemData(PoolKeyType poolKeyType)
        {
            ObjectPoolItemData itemData = GetObjectPoolItemData(poolKeyType);
            if (itemData == null)
            {
                Debug.LogError("There isn't pool item data " + poolKeyType);
                return false;
            }
            
            CreatePool(itemData);
            return true;
        }

        public bool Enqueue(IPoolable poolable)
        {
            if (!_poolDict.TryGetValue(poolable.PoolKeyType, out Queue<IPoolable> queue))
            {
                return false;
            }

            queue.Enqueue(poolable);
            return true;
        }

        public IPoolable DequeuePool(PoolKeyType poolKeyType)
        {
            if (!_poolDict.TryGetValue(poolKeyType, out Queue<IPoolable> queue))
            {
                Debug.LogWarning("key is not setting" + poolKeyType);
                if (!TryCreatePoolItemData(poolKeyType))
                {
                    return null;
                }
                
                return _poolDict[poolKeyType].Dequeue();
            }

            IPoolable iPoolable = null;
            bool isSuccess = queue.TryDequeue(out iPoolable);
            if (!isSuccess)
            {
                if (!TryCreatePoolItemData(poolKeyType))
                {
                    return null;
                }
                
                return _poolDict[poolKeyType].Dequeue();
            }

            return iPoolable;
        }
    }
}

























