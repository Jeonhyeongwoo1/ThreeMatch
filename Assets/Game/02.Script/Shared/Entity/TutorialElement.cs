using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.Shared.Entity
{
    public class TutorialElement : MonoBehaviour
    {
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeButton;
        
        public void Initialize(Action<int, TutorialElement> onChangePageAction, Action closeAction)
        {
            _prevButton?.onClick.RemoveAllListeners();
            _prevButton?.onClick.AddListener(()=> onChangePageAction.Invoke(-1, this));
            
            _nextButton?.onClick.RemoveAllListeners();
            _nextButton?.onClick.AddListener(()=> onChangePageAction.Invoke(1, this));
            
            _closeButton?.onClick.RemoveAllListeners();
            _closeButton?.onClick.AddListener(closeAction.Invoke);
        }

        public void Activate(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
