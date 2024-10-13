using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Entity;

namespace ThreeMatch.InGame
{
    [Serializable]
    public class BoardInfo
    {
        /*
         *  1. 블록 정보
         *  2. 셀 정보
         */
    }
    
    [Serializable]
    public class MissionData
    {
        public MissionType missionType;
        public CellImageType cellImageType;
        public int removeCount;
    }
    
    public class StageBuilder
    {

        public List<MissionData> _missionDataList = new List<MissionData>()
        {
            new()
            {
                missionType = MissionType.RemoveNormalBlueCell,
                removeCount = 10,
            },
            new()
            {
                missionType = MissionType.RemoveNormalGreenCell,
                removeCount = 7,
            },
            new()
            {
                missionType = MissionType.RemoveNormalPurpleCell,
                removeCount = 5,
            },
        };

        public int[,] test_board_block = new[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
        };
        
        public int[,] test_board_cell = new[,]
        {
            { 8, 8, 8, 8, 8, 8, 8, },
            { 1, 1, 1, 1, 1, 1, 5, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
        };

        public int remainingMoveCount = 20;

        public Stage LoadStage(int stageLevel)
        {
            Stage stage = new Stage(test_board_block, test_board_cell, _missionDataList, remainingMoveCount);
            return stage;
        }
    }
}
