using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.OutGame.View
{
    public class StageLevelListView : MonoBehaviour, IView
    {
        public List<StageLevelView> StageLevelViewList => _stageLevelViewList;
        
        [SerializeField] private List<StageLevelView> _stageLevelViewList;
    }
}