using ThreeMatch.InGame.Manager;
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

            GameManager.onInGameItemUsagePendingAction += OnItemUsagePendingAction;
        }

        public void Build(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab)
        {
            _board.Build(centerPosition, blockPrefab, cellPrefab);
        }

        public void Ready()
        {
        }

        public void OnItemUsagePendingAction(InGameItemType inGameItemType)
        {
            _board.SetPendingUseInGameItemType(inGameItemType);
        }
    }   
}