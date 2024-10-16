using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.Effect
{
    public class BombEffect : MonoBehaviour, IPoolable
    {
        public PoolKeyType PoolKeyType { get; set; }

        [SerializeField] private GameObject _particle;
        
        public T Get<T>() where T : MonoBehaviour
        {
            return this as T;
        }

        public void Spawn(Transform spawner)
        {
            transform.position = spawner.position;
            gameObject.SetActive(true);
            DOVirtual.DelayedCall(0.25f, () => _particle.SetActive(true));
        }
    }
}