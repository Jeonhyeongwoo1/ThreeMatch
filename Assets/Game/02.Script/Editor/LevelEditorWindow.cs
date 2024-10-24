using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Manager;
using ThreeMatch.InGame.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Stage = ThreeMatch.InGame.Entity.Stage;

namespace ThreeMatch.InGame.Editor
{
    public class LevelEditorWindow : OdinEditorWindow
    {
        private const string StageLevelPath = "Assets/Game/03.Resources/Resources/StageLevel/";
        private const string BoardContainerPrefabPath = "Assets/Game/04.Prefab/Board/BoardContainer.prefab";
        private const string CellPrefabPath = "Assets/Game/04.Prefab/Board/Cell.prefab";
        private const string BlockPrefabPath = "Assets/Game/04.Prefab/Board/Block.prefab";
        private const string CellDataPath = "Assets/Game/03.Resources/Resources/Data/CellDataForEditorConfigData.asset";
        private const string MissionResourceConfigDataPath = "Assets/Game/03.Resources/Resources/Data/MissionResourceConfigData.asset";
        private const string StageLevelConfigDataForEditorPath = "Assets/Game/03.Resources/Resources/StageLevelConfigDataForEditor.asset";
        private const string MissionElementPrefabPath = "Assets/Game/04.Prefab/UI/MissionElement.prefab";
        private const string StageLevelListPath = "Assets/Game/03.Resources/Resources/StageLevel";

        private const int BoardMatrixMaxValue = 8;
        private const int BoardMatrixMinValue = 0;

        [ReadOnly]
        [ShowIf(nameof(IsActiveStageEditor))]
        public GameObject boardContainerPrefab;
        [ReadOnly]
        [ShowIf(nameof(IsActiveStageEditor))]
        public GameObject cellPrefab;
        [ReadOnly]
        [ShowIf(nameof(IsActiveStageEditor))]
        public GameObject blockPrefab;
        [ReadOnly]
        [ShowIf(nameof(IsActiveStageEditor))]
        public MissionElement missionElementPrefab;

        [ShowIf(nameof(IsActiveStageEditor))]
        [Title("Board")] [Range(BoardMatrixMinValue, BoardMatrixMaxValue)]
        public int row;

        [ShowIf(nameof(IsActiveStageEditor))]
        [Range(BoardMatrixMinValue, BoardMatrixMaxValue)]
        public int column;

        [OnValueChanged(nameof(OnBoardInfoValueChanged))]
        [ShowIf(nameof(_isShowBoardCellTypeMatrix))]
        [TableMatrix(HorizontalTitle = "column", VerticalTitle = "row", SquareCells = true,
            DrawElementMethod = nameof(BoardInfoDrawElement))]
        [Title("보드판")]
        public BoardInfoData[,] boardInfoDataArray;//사용하는 board

        [Space]
        [ShowIf(nameof(_isShowBoardCellTypeMatrix))]
        [TableMatrix(HorizontalTitle = "column", VerticalTitle = "row", RowHeight = 50, SquareCells = true,
            DrawElementMethod = nameof(CellPrefabDrawElement))]
        [Title("보드에서 사용가능한 에셋")]
        public BoardInfoData[,] availableBoardAssetArray;
        
        [HideInInspector]
        [SerializeField] 
        private StageBuilder _stageBuilder;

        [HideInInspector]
        [SerializeField] 
        private Transform _boardContainer;
        
        [HideInInspector]     
        [SerializeField] 
        private Stage _stage;
        
        [HideInInspector]
        [SerializeField] 
        private bool _isShowBoardCellTypeMatrix;
        
        [HideInInspector]
        [SerializeField]
        private MissionResourceConfigData _missionResourceConfigData;
        private BoardInfoData _draggingBoardInfoData;

        [SerializeField] private StageLevelConfigDataForEditor _stageLevelConfigDataForEditor;
        
        public void OnBoardInfoValueChanged()
        {
            if (_boardContainer)
            {
                DestroyImmediate(_boardContainer.gameObject);
            }
            
            CreateCustomStage();
        }

        private BoardInfoData CellPrefabDrawElement(Rect rect, BoardInfoData value, int x, int y)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (value != null && value.Prefab != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    _draggingBoardInfoData = value;
                    DragAndDrop.objectReferences = new Object[] { value.Prefab  };
                    DragAndDrop.StartDrag("Dragging");
                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.DragUpdated && rect.Contains(Event.current.mousePosition))
            {
                value = _draggingBoardInfoData;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }

            // 셀 내에 오브젝트 이름 표시
            if (value != null && value.Prefab != null)
            {
                Texture2D texture2D = AssetPreview.GetAssetPreview(value.Prefab);
                // GUI.Label(rect, value.name); // Prefab 이름을 셀에 그리기
                if (texture2D != null)
                {
                    GUI.DrawTexture(rect, texture2D, ScaleMode.ScaleToFit);
                }
            }
            else
            {
                GUI.Label(rect, "Empty");
            }

            return value;
        }

        private BoardInfoData BoardInfoDrawElement(Rect rect, BoardInfoData value, int x, int y)
        {
            if (Event.current.type == EventType.DragUpdated && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
            }

            if (Event.current.type == EventType.DragExited && rect.Contains(Event.current.mousePosition))
            {
                // boardInfoDataArray[_dragStartValue.x, _dragStartValue.y] = null;
                BoardInfoData boardInfoData = new BoardInfoData(_draggingBoardInfoData.Prefab,
                    _draggingBoardInfoData.CellType, _draggingBoardInfoData.ObstacleCellType,
                    _draggingBoardInfoData.CellImageType);
                boardInfoDataArray[x, y] = boardInfoData;
                GUI.changed = true;
                Event.current.Use();
                value = _draggingBoardInfoData;
            }

            // 셀 내에 오브젝트 이름 표시
            if (value != null && value.Prefab != null)
            {
                Texture2D texture2D = AssetPreview.GetAssetPreview(value.Prefab);
                if (texture2D != null)
                {
                    GUI.DrawTexture(rect, texture2D, ScaleMode.ScaleToFit);
                }
            }
            else
            {
                GUI.Label(rect, "Empty");
            }
            
            if (rect.Contains(Event.current.mousePosition) && boardInfoDataArray[x, y] != null)
            {
                Rect buttonRect = new Rect(rect.xMax - 30, rect.yMin + 5, 30, 30);
                
                // GUIStyle 생성 및 색상 설정
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.hover.textColor = Color.white; // 텍스트 색상
                buttonStyle.hover.background = Texture2D.redTexture;

                if (GUI.Button(buttonRect, "X", buttonStyle))
                {
                    var boardInfoData = OnClickCellRemoveButton(x, y);
                    GUI.changed = true;
                    Event.current.Use();
                    return boardInfoData;
                }
            }

            return value;
        }
        
        private BoardInfoData OnClickCellRemoveButton(int x, int y)
        {
            boardInfoDataArray[x, y] =
                new BoardInfoData(null, CellType.None, ObstacleCellType.None, CellImageType.None);
            Debug.Log($"Removed BoardInfoData at position ({x}, {y})");
            // EditorUtility.SetDirty(this); // 에디터에 변경 사항 알림
            return boardInfoDataArray[x, y];
        }
        
        //--------------------------------------
        
        [MenuItem("Tools/Level editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<LevelEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
            window.Show();
        }

        private bool IsActiveStageEditor()
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.name == "StageEditor";
        }
        
        // 색상 텍스처 생성 함수
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();
        }
        
        protected override void OnEnable()
        {
            Debug.Log("OnEnable");
            base.OnEnable();
            
            boardContainerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BoardContainerPrefabPath);
            cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CellPrefabPath);
            blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BlockPrefabPath);
            missionElementPrefab = AssetDatabase.LoadAssetAtPath<MissionElement>(MissionElementPrefabPath);
            
            _missionResourceConfigData = AssetDatabase.LoadAssetAtPath<MissionResourceConfigData>(MissionResourceConfigDataPath);
            _stageLevelConfigDataForEditor =
                AssetDatabase.LoadAssetAtPath<StageLevelConfigDataForEditor>(StageLevelConfigDataForEditorPath);
            _missionPanel = GameObject.Find("MissionPanel");

            stageLevelList = new List<StageLevel>();
            var guids = AssetDatabase.FindAssets("", new[] { StageLevelListPath });
            foreach (string guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var stageLevelConfigData = AssetDatabase.LoadAssetAtPath<StageLevel>(assetPath);
                stageLevelList.Add(stageLevelConfigData);
            }
            
            if (_stage != null)
            {
                OnLoadStageByLevel();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            remainingMoveCount = 0;
            _stage = null;
            _boardContainer = null;
            _missionDataList = null;
            missionInfoDataList = null;
            _missionElementList = null;
        }

        private void InitializeBoardInfoDataArray()
        {
            CellDataForEditorConfigData cellDataForEditorConfigData = AssetDatabase.LoadAssetAtPath<CellDataForEditorConfigData>(CellDataPath);
            //매트릭스 기준
            boardInfoDataArray = new BoardInfoData[column, row];

            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    var cellList = cellDataForEditorConfigData.CellInfoList;
                    var cellInfoList = cellList.FindAll(v => v.cellType == CellType.Normal && (v.cellImageType == CellImageType.Yellow ||
                                                                         v.cellImageType == CellImageType.Green ||
                                                                         v.cellImageType == CellImageType.Blue ||
                                                                         v.cellImageType == CellImageType.Purple ||
                                                                         v.cellImageType == CellImageType.Red));
                    int random = Random.Range(0, cellInfoList.Count);
                    var cellInfo = cellInfoList[random];
                    BoardInfoData data = new BoardInfoData(cellInfo.prefab, cellInfo.cellType,
                        cellInfo.obstacleCellType, cellInfo.cellImageType);
                    boardInfoDataArray[j, i] = data;
                }
            }
        }

        private void InitializeMissionInfoDataList()
        {
            missionInfoDataList = new List<MissionInfoData>();
        }

        private void AddAvailableBoardAssetArray()
        {
            CellDataForEditorConfigData cellDataForEditorConfigData = AssetDatabase.LoadAssetAtPath<CellDataForEditorConfigData>(CellDataPath);
            var cellInfoList = cellDataForEditorConfigData.CellInfoList;

            int count = cellInfoList.Count;
            int maxColumn = 7;
            int row = count / maxColumn;
            int column = maxColumn;
            availableBoardAssetArray = new BoardInfoData[column + 1, row + 1];

            int rowIndex = 0;
            int columnIndex = 0;
            for (int i = 0; i < count; i++)
            {
                CellDataForEditorConfigData.CellInfo info = cellInfoList[i];
                BoardInfoData data = new BoardInfoData(info.prefab, info.cellType, info.obstacleCellType,
                    info.cellImageType);
                availableBoardAssetArray[columnIndex, rowIndex] = data;

                if (i != 0 && i % maxColumn == 0)
                {
                    rowIndex++;
                    columnIndex = 0;
                }
                else
                {
                    columnIndex++;
                }
            }
        }
        
        private void CreateCustomStage()
        {
            _boardContainer = Instantiate(boardContainerPrefab).transform;
            _boardContainer.transform.position = Vector3.zero;
            //미리 1로 할당
            // var array = Enumerable.Repeat(1, row * column).ToArray().To2DArray(row, column);

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
            
            _stageBuilder = new StageBuilder();
            _stage = _stageBuilder.LoadStage(array, missionInfoDataList, remainingMoveCount);

            Vector2 centerPosition =
                Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
            _stage.CustomBuild(centerPosition, blockPrefab, cellPrefab, _boardContainer);
            _isShowBoardCellTypeMatrix = true;
            _stageLevelConfigDataForEditor.boardInfoDataArray = boardInfoDataArray;
            EditorUtility.SetDirty(_stageLevelConfigDataForEditor);
        }

        [PropertyOrder(1)]
        [ShowIf(nameof(IsActiveStageEditor))]
        [Button(ButtonSizes.Medium), Title("보드 생성")]
        public void OnCreateBoard()
        {
            if (stageLevel == null)
            {
                DisplayDialog("에러", "새로운 스테이지를 생성해주세요.", "OK");
                Debug.Log("새로운 스테이지를 생성해주세요.");
                return;
            }
            if (row == 0 && column == 0)
            {
                DisplayDialog("에러", "row행과 column을 확인해주세요", "OK");
                Debug.LogWarning($"failed create board row {row} / column {column}");
                return;
            }

            if (_stage != null)
            {
                DisplayDialog("에러", "이미 스테이지가 있습니다.", "OK");
                return;
            }

            if (_boardContainer)
            {
                DestroyImmediate(_boardContainer.gameObject);
            }
            
            InitializeBoardInfoDataArray();
            AddStageLevelAssetData();
            AddAvailableBoardAssetArray();
            CreateCustomStage();
        }

        [PropertyOrder(2)]
        [ShowIf(nameof(IsActiveStageEditor))]
        [Button(ButtonSizes.Medium), Title("보드 제거")]
        public void OnDestroyBoard()
        {
            if (stageLevel == null)
            {
                DisplayDialog("에러", "스테이지가 없습니다.", "OK");
                Debug.Log("스테이지가 없습니다.");
                return;
            }

            if (_stage == null)
            {
                DisplayDialog("에러", "스테이지가 없습니다.", "OK");
                Debug.Log("스테이지가 없습니다.");
                return;
            }
            
            boardInfoDataArray = null;
            if (_boardContainer)
            {
                DestroyImmediate(_boardContainer.gameObject);
            }

            _stage = null;
            _isShowBoardCellTypeMatrix = false;
        }
        
        [PropertyOrder(3)]
        [ShowIf(nameof(IsActiveStageEditor))]
        [Button(ButtonSizes.Medium), Title("스테이지 데이터 저장")]
        public void OnSaveStageData()
        {
            if (stageLevel == null)
            {
                DisplayDialog("에러", "선택된 스테이지가 없습니다.", "OK");
                Debug.Log("기존 스테이지가 없습니다");
                return;
            }

            if (missionInfoDataList.Count == 0)
            {
                DisplayDialog("에러", "미션을 설정해야합니다.", "OK");
                Debug.Log("미션을 설정해야합니다.");
                return;
            }

            var mission = missionInfoDataList.Find(v => v.missionType == MissionType.None);
            if (mission != null)
            {
                DisplayDialog("에러", "미션 리스트중에서 미션 타입을 설정하지 않았습니다.", "OK");
                Debug.Log("미션 타입을 설정하지 않았습니다.");
                return;
            }

            if (remainingMoveCount == 0)
            {
                DisplayDialog("에러", "이동 가능한 횟수가 0입니다.", "OK");
                Debug.Log("이동 가능한 횟수가 0입니다");
                return;
            }
            
            StageEditorManager.stageLevel = stageLevel;
            stageLevel.Initialize(boardInfoDataArray, missionInfoDataList, remainingMoveCount);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            EditorUtility.SetDirty(stageLevel); // 에디터에 변경 사항 알림
            EditorUtility.SetDirty(_stageLevelConfigDataForEditor);
        }
        
        [PropertyOrder(4)]
        [ShowIf(nameof(IsActiveStageEditor))]
        [Button(ButtonSizes.Medium), Title("새로운 스테이지 데이터 생성")]
        public void OnCreateNewStageData()
        {
            if (row == 0 && column == 0)
            {
                DisplayDialog("에러", "row행과 column을 확인해주세요", "OK");
                Debug.LogWarning($"failed create board row {row} / column {column}");
                return;
            }

            if (_boardContainer)
            {
                DestroyImmediate(_boardContainer.gameObject);
            }

            InitializeMissionInfoDataList();
            InitializeBoardInfoDataArray();
            AddStageLevelAssetData();
            AddAvailableBoardAssetArray();
            CreateCustomStage();
            
            var stageLevel = CreateInstance<StageLevel>();
            stageLevel.Initialize(boardInfoDataArray, missionInfoDataList, remainingMoveCount);
            _stageLevelConfigDataForEditor.Initialize(boardInfoDataArray,
                missionInfoDataList, stageLevel.remainingMoveCount);
            int count = stageLevelList.Count;
            AssetDatabase.CreateAsset(stageLevel, $"{StageLevelPath}StageLevel_{count}.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();

            stageLevelList.Add(stageLevel);
            this.stageLevel = stageLevel;
           
            EditorUtility.SetDirty(stageLevel); // 에디터에 변경 사항 알림
            EditorUtility.SetDirty(_stageLevelConfigDataForEditor);
        }
        
        [PropertyOrder(5)]
        [BoxGroup("Stage level")]
        [Header("스테이지 리스트")]
        [Space]
        [ShowIf(nameof(IsActiveStageEditor))]
        public List<StageLevel> stageLevelList;

        [PropertyOrder(6)]
        [BoxGroup("Stage level")]
        [Header("현재 스테이지")]
        [ShowIf(nameof(IsActiveStageEditor))]
        [SerializeReference] public StageLevel stageLevel;
        
        [PropertyOrder(7)]
        [BoxGroup("Stage level")]
        [ShowIf(nameof(IsActiveStageEditor))]
        [Button(ButtonSizes.Medium), Title("스테이지 불러오기")]
        public void OnLoadStageByLevel()
        {
            if (stageLevel == null)
            {
                DisplayDialog("에러", "스테이지 리스트에서 리스트를 선택한 후에 현재 스테이지에 넣어주세요", "OK");
                return;
            }

            if (missionInfoDataList.Count > 0)
            {
                DestroyMission();
            }

            if (_stage != null)
            {
                OnDestroyBoard();
            }
            
            row = stageLevel.boardInfoDataArray.GetLength(0);
            column = stageLevel.boardInfoDataArray.GetLength(1);
            boardInfoDataArray = new BoardInfoData[row, column];
            
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    BoardInfoData infoData = stageLevel.boardInfoDataArray[i, j];
                    boardInfoDataArray[i, j] = new BoardInfoData(infoData.Prefab, infoData.CellType, infoData.ObstacleCellType, infoData.CellImageType);
                }
            }

            missionInfoDataList = new List<MissionInfoData>();
            foreach (MissionInfoData missionInfoData in stageLevel.missionInfoDataList)
            {
                missionInfoDataList.Add(new MissionInfoData()
                {
                    missionType = missionInfoData.missionType,
                    removeCount = missionInfoData.removeCount
                });
            }

            remainingMoveCount = stageLevel.remainingMoveCount;
            _stageLevelConfigDataForEditor.remainingMoveCount = remainingMoveCount;
            _stageLevelConfigDataForEditor.missionInfoDataList = missionInfoDataList;
            EditorUtility.SetDirty(_stageLevelConfigDataForEditor);
            CreateCustomStage();                
        }

        [OnValueChanged(nameof(OnMissionDataValueChanged))]
        [ListDrawerSettings, Title("미션 리스트 추가")]
        [PropertyOrder(8)]
        [ShowIf(nameof(IsActiveStageEditor))]
        public List<MissionInfoData> missionInfoDataList;
        private GameObject _missionPanel;

        [Space] 
        [PropertyOrder(9)]
        [OnValueChanged(nameof(OnChangeRemainingMoveCountValue))]
        [ShowIf(nameof(IsActiveStageEditor))]
        [Title("이동 가능한 횟수")]
        public int remainingMoveCount;

        private void OnChangeRemainingMoveCountValue()
        {
            _stageLevelConfigDataForEditor.remainingMoveCount = remainingMoveCount;
            EditorUtility.SetDirty(_stageLevelConfigDataForEditor);
        }
        
        [Serializable]
        public struct MissionData
        {
            public MissionInfoData missionInfoData;
            public MissionElement missionElement;
        }

        [HideInInspector]
        [SerializeField] private List<MissionData> _missionDataList = new();
        [HideInInspector]
        [SerializeField] private List<MissionElement> _missionElementList = new();

        private void DestroyMission()
        {
            if (!_missionPanel)
            {
                _missionPanel = GameObject.Find("MissionPanel");
            }
            
            MissionElement[] childs = _missionPanel.GetComponentsInChildren<MissionElement>();
            for (int i = 0; i < childs.Length; i++)
            {
                MissionElement tr = childs[i];
                if (tr.transform == _missionPanel.transform)
                {
                    continue;
                }
                
                DestroyImmediate(tr.gameObject);
            }
        }
        
        private void OnMissionDataValueChanged()
        {
            DestroyMission();
            _missionDataList.Clear();
            _missionElementList.Clear();
            foreach (MissionInfoData missionData in missionInfoDataList)
            {
                missionData.onChangeMissionInfoData = OnChangedMissionDataValue;
                MissionType missionType = missionData.missionType;
                int removeCount = missionData.removeCount;

                MissionElement missionElement = Instantiate(missionElementPrefab, _missionPanel.transform);
                missionElement.Initialize(missionType, removeCount);
                _missionElementList.Add(missionElement);

                MissionData data = new MissionData()
                {
                    missionInfoData = missionData,
                    missionElement = missionElement
                };
                
                _missionDataList.Add(data);
            }
            
            _stageLevelConfigDataForEditor.missionInfoDataList = missionInfoDataList;
            EditorUtility.SetDirty(_stageLevelConfigDataForEditor);
        }

        private void OnChangedMissionDataValue(MissionInfoData missionInfoData)
        {
            MissionData missionData = _missionDataList.Find(v => v.missionInfoData == missionInfoData);
            missionData.missionElement.Initialize(missionInfoData.missionType, missionInfoData.removeCount);
            _stageLevelConfigDataForEditor.missionInfoDataList = missionInfoDataList;
            EditorUtility.SetDirty(_stageLevelConfigDataForEditor);
        }
        
        private void AddStageLevelAssetData()
        {
            string[] guidArray = AssetDatabase.FindAssets("StageLevel_");
            foreach (string guid in guidArray)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                // Debug.Log($"{guid} / {assetPath}");
                StageLevel stageLevel = AssetDatabase.LoadAssetAtPath<StageLevel>(assetPath);
                if (!stageLevelList.Contains(stageLevel))
                {
                    stageLevelList.Add(stageLevel);
                }
            }
        }

        [Button(size: ButtonSizes.Large, Name = "StageEditor 씬 열기")]
        [HideIf(nameof(IsActiveStageEditor))]
        private void LoadScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string scenePath = "Assets/Game/01.Scene/StageEditor.unity"; // 열고자 하는 씬의 경로
                EditorSceneManager.OpenScene(scenePath);
            }
        }
        
        private void DisplayDialog(string title, string message, string ok)
        {
            EditorUtility.DisplayDialog(title, message, ok);
        }
    }
}

public static class Extensions
{
    public static T[,] To2DArray<T>(this T[] array, int rows, int columns)
    {
        var result = new T[rows, columns];
        for (int i = 0; i < array.Length; i++)
        {
            result[i / columns, i % columns] = array[i];
        }
        return result;
    }
}