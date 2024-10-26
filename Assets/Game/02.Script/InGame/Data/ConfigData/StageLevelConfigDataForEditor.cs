using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "StageLevelConfigDataForEditor", menuName = "ThreeMatch/StageLevelConfigDataForEditor", order = 1)]
    public class StageLevelConfigDataForEditor : SerializedScriptableObject
    {
        [TableMatrix(HorizontalTitle = "column", VerticalTitle = "row", RowHeight = 50, SquareCells = true,
            DrawElementMethod = nameof(DrawElement))]
        public BoardInfoData[,] boardInfoDataArray;

        public List<MissionInfoData> missionInfoDataList;
        public int remainingMoveCount;
        public int aimScore;

        public BoardInfoData[,] GetBoardInfoDataArray()
        {
            int row = boardInfoDataArray.GetLength(0);
            int column = boardInfoDataArray.GetLength(1);
            BoardInfoData[,] array = new BoardInfoData[column, row];
            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    array[i, j] = boardInfoDataArray[j, i];
                }
            }
            
            for (int i = 0; i < row / 2; i++) // 절반만 순회해서 앞뒤로 변경
            {
                for (int j = 0; j < column; j++)
                {
                    (array[i, j], array[row - i - 1, j]) = (array[row - i - 1, j], array[i, j]);
                }
            }

            return array;
        }
        
        public void Initialize(BoardInfoData[,] boardInfoDataArray, List<MissionInfoData> missionInfoDataList, int remainingMoveCount, int aimScore)
        {
            this.boardInfoDataArray = boardInfoDataArray;
            this.missionInfoDataList = missionInfoDataList;
            this.remainingMoveCount = remainingMoveCount;
            this.aimScore = aimScore;
        }
        
        private BoardInfoData DrawElement(Rect rect, BoardInfoData value)
        {
            // 셀 내에 오브젝트 이름 표시
            if (value != null && value.Prefab != null)
            {
                Texture2D texture2D = AssetPreview.GetAssetPreview(value.Prefab);
                // GUI.Label(rect, value.name); // Prefab 이름을 셀에 그리기
                GUI.DrawTexture(rect, texture2D, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Label(rect, "Empty");
            }

            return value;
        }
    }
}
#endif