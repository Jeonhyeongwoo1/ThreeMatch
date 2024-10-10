using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame
{
    [Serializable]
    public class BoardInfo
    {
    }
    
    public class StageBuilder
    {

        private int[,] test_board = new[,]
        {
            { 0, 1, 1, 1, 1, 1, 1, },
            { 1, 0, 1, 1, 1, 1, 1, },
            { 1, 1, 0, 0, 1, 1, 1, },
            { 1, 1, 1, 0, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
        };

        public Stage LoadStage(int stageLevel)
        {
            Stage stage = new Stage(test_board);
            return stage;
        }
    }
}
