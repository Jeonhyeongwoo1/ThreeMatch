using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThreeMatch.InGame.Entity
{
    public class AnimationEvent : MonoBehaviour
    {
        [SerializeField] UnityEvent _event;

        public void OnFinished()
        {
            _event?.Invoke();
        }
    }
}