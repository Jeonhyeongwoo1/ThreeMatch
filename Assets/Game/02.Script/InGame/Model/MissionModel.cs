using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using ThreeMatch.InGame.Interface;
using UniRx;
using UnityEngine;

namespace ThreeMatch.InGame.Model
{
    public class MissionModel : IModel
    {
        public ReactiveProperty<List<MissionInfoData>> missionDataList = new();
    }
}