using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ThreeMatch.InGame.Interface;
using TMPro;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class ComboBehaviour : MonoBehaviour, IPoolable
    {
        public PoolKeyType PoolKeyType { get; set; }

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _comboCountText;
        
        public T Get<T>() where T : MonoBehaviour
        {
            return this as T;
        }
        
        public void SetComboCount(string comboCount)
        {
            _comboCountText.text = comboCount;
        }

        public void Spawn(Transform spawner, Action callback = null)
        {
            transform.position = spawner.position;
            transform.SetParent(spawner);
            transform.localScale = Vector3.one;
            RectTransform rect = transform.GetComponent<RectTransform>();
            gameObject.SetActive(true);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOAnchorPosY(80, 1f));
            sequence.Join(_canvasGroup.DOFade(0, 1));
            sequence.OnComplete(() => ((IPoolable)this).Sleep());
        }
    }
}