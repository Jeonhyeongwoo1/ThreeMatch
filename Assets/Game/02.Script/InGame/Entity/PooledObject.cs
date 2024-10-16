using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class PooledObject : MonoBehaviour, IPoolable
    {
        public PoolKeyType PoolKeyType { get; set; }

        [SerializeField] private bool _useLifeCycle = true;
        [SerializeField] private float _lifeCycle = 1;

        private float _elapsed = 0;

        private void OnEnable()
        {
            _elapsed = 0;
        }

        private void Update()
        {
            if (_useLifeCycle)
            {
                _elapsed += Time.deltaTime;
                if (_elapsed > _lifeCycle)
                {
                    ((IPoolable)this).Enqueue();
                }
            }
        }

        public T Get<T>() where T : MonoBehaviour
        {
            return this as T;
        }

        public void Spawn(Transform spawner)
        {
            Debug.Log("Spawn");
            transform.position = spawner.position;
            gameObject.SetActive(true);
        }
    }
}