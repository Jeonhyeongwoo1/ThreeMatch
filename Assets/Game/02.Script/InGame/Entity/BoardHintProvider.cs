using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public partial class Board
    {
        [Serializable]
        public class SimulationResultData
        {
            public Cell targetCell;
            public Cell specialOrNormalCell;
            public int estimatedScore;
            public MatchedSameImageCellInfo matchedSameImageCellInfo;

            public void StopPunchScale()
            {
                targetCell?.CellBehaviour?.StopPunchScale();
                specialOrNormalCell?.CellBehaviour?.StopPunchScale();

                if (matchedSameImageCellInfo.cellList != null)
                {
                    foreach (Cell cell in matchedSameImageCellInfo.cellList)
                    {
                        cell.CellBehaviour?.StopPunchScale();
                    }
                }
            }

            public void ShowPunchScale()
            {
                targetCell.CellBehaviour.ShowHintAnimation();
                if (targetCell.CellType == CellType.Normal)
                {
                    foreach (Cell cell in matchedSameImageCellInfo.cellList)
                    {
                        if (cell.CellBehaviour == null)
                        {
                            continue;
                        }

                        cell.CellBehaviour.ShowHintAnimation();
                    }
                }
                else
                {
                    specialOrNormalCell.CellBehaviour.ShowHintAnimation();
                }
            }
        }

        private readonly List<SimulationResultData> _simulationResultDataList = new();
        private SimulationResultData _currentSimulationResultData = new();
        private CancellationTokenSource _hintProcessCts;

        private async UniTaskVoid StartHintProcess()
        {
            _hintProcessCts = new CancellationTokenSource();
            try
            {
                await UniTask.WaitForSeconds(Const.ShowHintTime, cancellationToken: _hintProcessCts.Token);
            }
            catch (Exception e) when (!_hintProcessCts.Token.IsCancellationRequested)
            {
                Debug.LogError($"Hit process error {e.Message}");
                return;
            }

            if (_simulationResultDataList.Count > 0)
            {
                DisplaySimulationResult();
            }
        }

        private void RemoveSimulationResult()
        {
            _hintProcessCts.Cancel();
            if (_currentSimulationResultData != null)
            {
                _currentSimulationResultData.StopPunchScale();
                _currentSimulationResultData = null;
            }
        }
        
        private void StartSimulation()
        {
            _simulationResultDataList.Clear();
            Cell simulationCell = new Cell(0, 0, CellType.Normal);
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Block block = _blockArray[i, j];
                    Cell cell = _cellArray[i, j];
                    if (block.BlockType == BlockType.None || cell == null || IsObstacleOrGeneratorCellType(cell.CellType))
                    {
                        continue;
                    }

                    CheckAndAddHint(simulationCell, cell, i, j, 1, 0); //top
                    CheckAndAddHint(simulationCell, cell, i, j, -1, 0); //bottom
                    CheckAndAddHint(simulationCell, cell, i, j, 0, 1); //right
                    CheckAndAddHint(simulationCell, cell, i, j, 0, -1); //lefht
                }
            }
        }
        
        //우선 순위가 가장 높은 것 하나만 표시
        private void DisplaySimulationResult()
        {
            _simulationResultDataList.Sort((a, b) => b.estimatedScore.CompareTo(a.estimatedScore));
            _currentSimulationResultData = _simulationResultDataList[0];
            var resultData = _simulationResultDataList[0];
            resultData.ShowPunchScale();
        }

        private void CheckAndAddHint(Cell simulationCell, Cell cell, int row, int column, int rowOffset,
            int columnOffset)
        {
            int resultRow = row + rowOffset;
            int resultColumn = column + columnOffset;

            if (resultRow >= 0 && resultRow < _row && resultColumn >= 0 && resultColumn < _column)
            {
                simulationCell.UpdateRowAndColumn(resultRow, resultColumn);
                simulationCell.UpdateCellImageTypeForSimulation(cell.CellImageType);

                Cell c = _cellArray[resultRow, resultColumn];
                Block block = _blockArray[resultRow, resultColumn];
                if (block.BlockType == BlockType.None || c == null)
                {
                    return;
                }
                
                if (cell.CellType == CellType.Normal)
                {
                    MatchedSameImageCellInfo? matchedSameImageCellInfo =
                        GetMatchedSameImageCellInfo(simulationCell, cell);
                    if (matchedSameImageCellInfo != null && matchedSameImageCellInfo.HasValue)
                    {
                        matchedSameImageCellInfo.Value.cellList.Remove(simulationCell);
                        AddSimulationResultData(cell, null, matchedSameImageCellInfo.Value);
                    }
                }
                else
                {
                    //스페셜 셀 or 일반셀
                    if (IsSpecialCell(c.CellType) || c.CellType == CellType.Normal)
                    {
                        AddSimulationResultData(cell, c);
                    }
                }
            }
        }

        private bool IsSpecialCell(CellType cellType)
        {
            return cellType == CellType.Bomb || cellType == CellType.Rocket || cellType == CellType.Wand;
        }

        private int EvaluateScore(MatchedSameImageCellInfo matchedSameImageCellInfo, Cell specialOrNormalCell, Cell targetCell)
        {
            if (specialOrNormalCell != null)
            {
                if (specialOrNormalCell.CellType == CellType.Normal)
                {
                    switch (targetCell.CellType)
                    {
                        case CellType.Rocket:
                        case CellType.Wand:
                        case CellType.Bomb:
                            return Const.MatchedSpecialCellScore;
                        default:
                            Debug.LogError($"failed evaluate score {targetCell.CellType}");
                            return 0;
                    }
                }

                CellCombinationType cellCombinationType =
                    EvaluateCellCombinationType(specialOrNormalCell.CellType, targetCell.CellType);
                return cellCombinationType switch
                {
                    CellCombinationType.RocketAndBomb => Const.ActivateRocketAndBombScore,
                    CellCombinationType.RocketAndWand => Const.ActivateRocketAndWandScore,
                    CellCombinationType.RocketAndRocket => Const.ActivateRocketAndRocketScore,
                    CellCombinationType.BombAndWand => Const.ActivateBombAndWandScore,
                    CellCombinationType.BombAndBomb => Const.ActivateBombAndBombScore,
                    CellCombinationType.WandAndWand => Const.ActivateWandAndWandScore,
                    _ => 0
                };
            }

            //매칭 갯수에 의해서 차등 지급
            return matchedSameImageCellInfo.cellMatchedType switch
            {
                CellMatchedType.Three => Const.MatchedCellScore,
                CellMatchedType.Four => Const.MatchedCellScore + 1,
                CellMatchedType.Five => Const.MatchedCellScore + 2,
                CellMatchedType.Five_Shape => Const.MatchedCellScore + 3,
                CellMatchedType.Vertical_Four => Const.MatchedCellScore + 1,
                CellMatchedType.Horizontal_Four => Const.MatchedCellScore + 1,
                _ => 0
            };
        }

        private void AddSimulationResultData(Cell targetCell, Cell specialOrNormalCell = null,
            MatchedSameImageCellInfo matchedSameImageCellInfo = default)
        {
            SimulationResultData resultData = new SimulationResultData();
            resultData.targetCell = targetCell;
            resultData.matchedSameImageCellInfo = matchedSameImageCellInfo;
            resultData.specialOrNormalCell = specialOrNormalCell;
            resultData.estimatedScore = EvaluateScore(matchedSameImageCellInfo, specialOrNormalCell, targetCell);
            _simulationResultDataList.Add(resultData);
        }
    }
}