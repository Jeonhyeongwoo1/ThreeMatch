using System;
using ThreeMatch.InGame.Entity;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Interface
{
    public interface IPoolable
    {
        PoolKeyType PoolKeyType { get; set; }
        T Get<T>() where T : MonoBehaviour;
        public void Enqueue()
        {
            var mono = Get<MonoBehaviour>();
            if (mono.gameObject.activeSelf)
            {
                mono.gameObject.SetActive(false);
            }
            
            ObjectPoolManager.Instance.Enqueue(this);
        }

        public void Spawn(Transform spawner);
    }
}