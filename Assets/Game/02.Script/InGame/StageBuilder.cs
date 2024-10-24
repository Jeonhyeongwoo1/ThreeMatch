using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class StageBuilder
    {
        public Stage LoadStage(int level)
        {
            string path = $"StageLevel_{level}";
            StageLevel stageLevel = Resources.Load<StageLevel>($"StageLevel/{path}");
            if (stageLevel == null)
            {
                Debug.LogError($"failed get stage {path}");
                path = $"StageLevel_{0}";
                stageLevel = Resources.Load<StageLevel>($"StageLevel/{path}");
                Debug.Log("Load stage level 0");
            }
            
            return new Stage(stageLevel.GetBoardInfoDataArray(), stageLevel.missionInfoDataList, stageLevel.remainingMoveCount);
        }

        public Stage LoadStage(BoardInfoData[,] boardInfoArray, List<MissionInfoData> missionInfoDataList, int remainingMoveCount)
        {
            return new Stage(boardInfoArray,  missionInfoDataList, remainingMoveCount);
        }
    }
}
