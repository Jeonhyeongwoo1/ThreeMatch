using System.Collections.Generic;
using ThreeMatch.Firebase.Data;
using ThreeMatch.InGame.Interface;
using UniRx;

namespace ThreeMatch.OutGame.Data
{
    public class StageLevelListModel : IModel
    {
        public ReactiveProperty<List<StageLevelModel>> stageLevelModelList;
        public int selectedStageLevel;
        public bool openNewStage;

        public void AddStageLevelModelList(List<StageLevelData> stageLevelDataList)
        {
            stageLevelModelList = new ReactiveProperty<List<StageLevelModel>>();
            stageLevelModelList.Value = new List<StageLevelModel>(Const.MaxStageLevel);
            foreach (StageLevelData levelData in stageLevelDataList)
            {
                var stageLevelModel = new StageLevelModel
                {
                    level = levelData.Level,
                    isLock = levelData.IsLock,
                    starCount = levelData.StarCount
                };
                
                stageLevelModelList.Value.Add(stageLevelModel);
            }
        }
    }
}