using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.Manager
{
    //UIManager 아래에 모든 캔버스들이 존재해야함.
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;

        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                }

                return _instance;
            }
        }
        
        private Dictionary<Type, IView> _viewDict = new();

        public T CreateOrGetView<T>() where T : IView, new ()
        {
            if (_viewDict.TryGetValue(typeof(T), out var view))
            {
                return (T)view;
            }
            
            view = GetComponentInChildren<T>(true);
            _viewDict.Add(typeof(T), view);
            return (T)view;
        }

        public void ClearViewDict()
        {
            _viewDict.Clear();
        }
        
    }
}
