using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "StageLevel", menuName = "ThreeMatch/StageLevel")]
    public class StageLevel : SerializedScriptableObject
    {
        #if UNITY_EDITOR
        [TableMatrix(HorizontalTitle = "column", VerticalTitle = "row", RowHeight = 50, SquareCells = true,
            DrawElementMethod = nameof(DrawElement))]
        #endif
        public BoardInfoData[,] boardInfoDataArray;

        public List<MissionInfoData> missionInfoDataList;
        public int remainingMoveCount;

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

        public void Initialize(BoardInfoData[,] boardInfoDataArray, List<MissionInfoData> missionInfoDataList, int remainingMoveCount)
        {
            this.boardInfoDataArray = boardInfoDataArray;
            this.missionInfoDataList = missionInfoDataList;
            this.remainingMoveCount = remainingMoveCount;
        }

#if UNITY_EDITOR
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
#endif
    }
}