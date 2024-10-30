using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.Common.Entity;
using ThreeMatch.Interface;
using UnityEngine;

namespace ThreeMatch.Common.Popup
{
    public class TutorialPopup : MonoBehaviour, IPopup
    {
        [SerializeField] private List<TutorialElement> _tutorialList;

        private void Start()
        {
            foreach (TutorialElement element in _tutorialList)
            {
                element.Initialize(OnChangePageAction, ClosePopup);
            }
        }

        public void OpenPopup()
        {
            if (_tutorialList.Count == 0)
            {
                return;
            }
            
            gameObject.SetActive(true);
            _tutorialList[0].Activate(true);
        }

        public void ClosePopup()
        {
            gameObject.SetActive(false);
            foreach (TutorialElement element in _tutorialList)
            {
                element.Activate(false);
            }
        }

        private void OnChangePageAction(int direction, TutorialElement element)
        {
            int index = _tutorialList.FindIndex(v => v == element);
            int targetIndex = index + direction;
            if (targetIndex < 0 || targetIndex >= _tutorialList.Count)
            {
                return;
            }

            element.Activate(false);
            _tutorialList[targetIndex].Activate(true);
        }
    }
}
