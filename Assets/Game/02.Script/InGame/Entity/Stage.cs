using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Stage
    {
        private Board _board;
        

        public Stage(int[,] blockInfoArray, int[,] cellInfoArray)
        {
            Board board = new Board(blockInfoArray, cellInfoArray);
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