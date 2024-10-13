using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Entity;
using UnityEngine;

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
    
    public class StageBuilder
    {

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
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 5, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
            { 1, 1, 1, 1, 1, 1, 1, },
        };


        public Stage LoadStage(int stageLevel)
        {
            Stage stage = new Stage(test_board_block, test_board_cell);
            return stage;
        }
    }
}
