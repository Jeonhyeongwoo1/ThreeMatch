using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Entity
{
    [RequireComponent(typeof(Button))]
    public class OnClickScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Vector3 _onEnterScale = new Vector3(0.85f,0.85f,0.85f);
        [SerializeField] private Vector3 _onExitScale = new Vector3(1, 1, 1);
        [SerializeField] private float _duration = 0.15f; 
        
        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(_onEnterScale, _duration);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(_onExitScale, _duration);
        }
    }
}
