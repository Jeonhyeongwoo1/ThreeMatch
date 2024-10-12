using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public class InGameItemView : MonoBehaviour
    {
        [Serializable]
        public struct InGameItemElement
        {
            public InGameItemType itemType;
            public Button button;
        }

        [SerializeField] private List<InGameItemElement> _inGameItemElemntList;
        
        private void Start()
        {
            // _inGameItemElemntList.ForEach(v =>
            // {
            //     v.button.onClick.AddListener(()=> );
            // });        
        }
    }
}