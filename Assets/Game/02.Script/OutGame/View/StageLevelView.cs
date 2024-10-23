using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using ThreeMatch.InGame.Manager;
using ThreeMatch.OutGame.Entity;
using UnityEditor;
using UnityEngine;

namespace ThreeMatch.OutGame.View
{
    public class StageLevelView : MonoBehaviour, IView
    {
        [SerializeField] private GameObject _lockObj;
        [SerializeField] private GameObject[] _starObjArray;
        [SerializeField] private GameObject _pathPivotObj;

        public void UpdateUI(bool isLock, int starCount, int level)
        {
            _lockObj.SetActive(isLock);
            for (int i = 0; i < _starObjArray.Length; i++)
            {
                _starObjArray[i].SetActive(i < starCount);
            }

            if (!isLock)
            {
                var levelText = ObjectPoolManager.Instance.GetPool(PoolKeyType.StageLevelText);
                var stageLevelUI = levelText.Get<StageLevelUI>();
                stageLevelUI.Spawn(transform);
                stageLevelUI.UpdateLevelText(level.ToString());
            }
        }
        
        // private void OnValidate()
        // {
        //     _lockObj = null;
        //     _pathPivotObj = null;
        //     _starObjArray = null;
        //     Transform[] childs = GetComponentsInChildren<Transform>(true);
        //     
        //     _starObjArray = new GameObject [3];
        //
        //     int index = 0;
        //     foreach (Transform child in childs)
        //     {
        //         if (child.name == "Star1" || child.name == "Star2" || child.name == "Star3")
        //         {
        //             _starObjArray[index] = child.gameObject;
        //             index++;
        //         }
        //
        //         if (child.name == "Lock")
        //         {
        //             _lockObj = child.gameObject;
        //         }
        //
        //         if (child.name == "PathPivot")
        //         {
        //             _pathPivotObj = child.gameObject;
        //         }
        //     }
        //     
        //     EditorUtility.SetDirty(this);
        // }
    }
}