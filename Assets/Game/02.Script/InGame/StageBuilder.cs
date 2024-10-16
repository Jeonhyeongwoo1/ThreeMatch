using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class StageBuilder
    {
        public Stage LoadStage()
        {
            StageLevel stageLevel = Resources.Load<StageLevel>("StageLevel/StageLevel_0");
            return new Stage(stageLevel.GetBoardInfoDataArray(), stageLevel.missionInfoDataList, stageLevel.remainingMoveCount);
        }

        public Stage LoadStage(BoardInfoData[,] boardInfoArray, List<MissionInfoData> missionInfoDataList, int remainingMoveCount)
        {
            return new Stage(boardInfoArray,  missionInfoDataList, remainingMoveCount);
        }
    }
}
