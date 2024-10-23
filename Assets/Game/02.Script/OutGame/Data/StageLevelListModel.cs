using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using ThreeMatch.OutGame.Data;
using UniRx;
using UnityEngine;

namespace ThreeMatch.OutGame.Data
{
    public class StageLevelListModel : IModel
    {
        public ReactiveProperty<List<StageLevelModel>> stageLevelModelList = new();
    }
}