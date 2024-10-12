using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace ThreeMatch.InGame.Manager
{
    public class InputPanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
    {
        public static Action<Vector2> OnPointerDownAction;
        public static Action<Vector2> OnDragAction;
        public static Action<Vector2> OnEndDragAction;
        public static Action<Vector2> OnPointerUpAction;
        
        private Vector3 _pointerDownPosition;
        private Camera _camera;
        private bool _isBlocked = false;
        
        private void Start()
        {
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            StageManager.onUseInGameItem += OnUseInGameItem;
        }

        private void OnDisable()
        {
            StageManager.onUseInGameItem -= OnUseInGameItem;
        }

        private void OnUseInGameItem(bool isOn)
        {
            _isBlocked = isOn;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isBlocked)
            {
                return;
            }
            
            _pointerDownPosition = eventData.position;
            Vector2 position = _camera.ScreenToWorldPoint(_pointerDownPosition);
            OnPointerDownAction?.Invoke(position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_isBlocked)
            {
                return;
            }

            Vector2 position = _camera.ScreenToWorldPoint(eventData.position);
            OnDragAction?.Invoke(position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isBlocked)
            {
                return;
            }

            Vector2 beginPos = _camera.ScreenToWorldPoint(_pointerDownPosition);
            Vector2 endPos = _camera.ScreenToWorldPoint(eventData.position);
            Vector2 delta = new Vector2(Mathf.Abs(endPos.x) - Mathf.Abs(beginPos.x),
                Mathf.Abs(endPos.y) - Mathf.Abs(beginPos.y));

            OnEndDragAction?.Invoke(endPos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isBlocked)
            {
                return;
            }

            OnPointerUpAction?.Invoke(_camera.ScreenToWorldPoint(eventData.position));           
        }
    }
}