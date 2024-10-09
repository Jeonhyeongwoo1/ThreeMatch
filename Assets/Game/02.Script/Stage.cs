using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class Stage
    {
        private Board _board;

        public Stage(int[,] boardInfoArray)
        {
            Board board = new Board(boardInfoArray);
            _board = board;
        }

        public void Build(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab)
        {
            _board.Build(centerPosition, blockPrefab, cellPrefab);
        }
    }   
}