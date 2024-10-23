using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using TMPro;
using UnityEngine;

namespace ThreeMatch.OutGame.Entity
{
    public class StageLevelUI : MonoBehaviour, IPoolable
    {
        public PoolKeyType PoolKeyType { get; set; }

        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        public T Get<T>() where T : MonoBehaviour
        {
            return this as T;
        }

        private void Start()
        {
            if (!_canvas.worldCamera)
            {
                _canvas.worldCamera = Camera.main;
            }
        }

        public void UpdateLevelText(string level)
        {
            _levelText.text = level;
        }
        
        public void Spawn(Transform spawner, Action callback = null)
        {
            transform.position = spawner.position;
            gameObject.SetActive(true);
        }
    }
}