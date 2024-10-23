using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.Interface;
using UnityEngine;

namespace ThreeMatch.Manager
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PopupManager>();
                }

                return _instance;
            }
        }

        private static PopupManager _instance;
        private Dictionary<Type, IPopup> _popupDict = new();

        public T GetPopup<T>() where T : IPopup, new ()
        {
            if (_popupDict.TryGetValue(typeof(T), out var view))
            {
                return (T)view;
            }
            
            view = GetComponentInChildren<T>(true);
            _popupDict.Add(typeof(T), view);
            return (T)view;
        }

        public void ClearViewDict()
        {
            _popupDict.Clear();
        }

    }
}