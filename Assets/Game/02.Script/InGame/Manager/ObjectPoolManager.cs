using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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

        private ConcurrentDictionary<PoolKeyType, ConcurrentQueue<IPoolable>> _poolDict = new();
        private List<Transform> poolParentList;
        private readonly Object _lock = new();

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
            string itemType = itemData.sceneType.ToString();
            Scene scene = SceneManager.GetActiveScene();
            for (int i = 0; i < count; i++)
            {
                if (!itemData.isInit || String.Compare(itemType, scene.name, StringComparison.Ordinal) != 0)
                {
                    break;
                }

                GameObject obj = Instantiate(prefab, parent);
                obj.name = prefab.name;
                    
                if (!obj.TryGetComponent(out IPoolable poolable))
                {
                    Debug.LogWarning($"failed get pool component {obj.name}");
                    continue;
                }

                poolable.PoolKeyType = poolKeyType;
                obj.gameObject.SetActive(false);
                if (_poolDict.TryGetValue(poolKeyType, out ConcurrentQueue<IPoolable> list))
                {
                    list.Enqueue(poolable);
                    // list.Add(poolable);
                    continue;
                }

                list = new ConcurrentQueue<IPoolable>();
                list.Enqueue(poolable);
                if (!_poolDict.TryAdd(poolKeyType, list))
                {
                    Debug.LogError("failed pool dict : " + poolKeyType);
                    break;
                }
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

        public void Sleep(IPoolable poolable)
        {
            lock (_lock)
            {
                // _poolDict[poolable.PoolKeyType].Enqueue(poolable);
            }
        }

        public IPoolable GetPool(PoolKeyType poolKeyType)
        {
            lock (_lock)
            {
                IPoolable poolable = null;
                if (!_poolDict.TryGetValue(poolKeyType, out ConcurrentQueue<IPoolable> queue))
                {
                    Debug.LogWarning("pool key is not setting" + poolKeyType);
                    if (!TryCreatePoolItemData(poolKeyType))
                    {
                        Debug.LogError($"failed create pool {poolKeyType}");
                        return null;
                    }

                    _poolDict[poolKeyType].TryDequeue(out poolable);
                    if (poolable == null)
                    {
                        Debug.LogError($"failed get pool {poolKeyType}");
                        return null;
                    }

                    return poolable;
                }

                queue.TryDequeue(out poolable);
                if (poolable != null)
                {
                    return poolable;
                }
                
                if (!TryCreatePoolItemData(poolKeyType))
                {
                    Debug.LogError($"failed create pool {poolKeyType}");
                    return null;
                }
                
                _poolDict[poolKeyType].TryDequeue(out poolable);
                if (poolable != null)
                {
                    return poolable;
                }
                
                Debug.LogError($"failed get pool {poolKeyType}");
                return null;
            }
        }
    }
}

























