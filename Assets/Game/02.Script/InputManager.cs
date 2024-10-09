using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace ThreeMatch.InGame.Manager
{
    public class InputManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerUpHandler
    {
        public static Action<Vector2> OnPointerDownAction;
        public static Action<Vector2Int> OnBeginDragAction;
        public static Action<Vector2> OnDragAction;
        public static Action<Vector2> OnEndDragAction;
        public static Action<Vector2> OnPointerUpAction;
        
        private Vector3 _pointerDownPosition;
        private Camera _camera;
        
        private void Start()
        {
            _camera = Camera.main;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownPosition = eventData.position;
            Vector2 position = _camera.ScreenToWorldPoint(_pointerDownPosition);
            OnPointerDownAction?.Invoke(position);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            // OnBeginDragAction?.Invoke(Vector2Int.RoundToInt(_pointerDownPosition));
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position = _camera.ScreenToWorldPoint(eventData.position);
            OnDragAction?.Invoke(position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 beginPos = _camera.ScreenToWorldPoint(_pointerDownPosition);
            Vector2 endPos = _camera.ScreenToWorldPoint(eventData.position);
            Vector2 delta = new Vector2(Mathf.Abs(endPos.x) - Mathf.Abs(beginPos.x),
                Mathf.Abs(endPos.y) - Mathf.Abs(beginPos.y));

            OnEndDragAction?.Invoke(endPos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpAction?.Invoke(_camera.ScreenToWorldPoint(eventData.position));           
        }
    }
}