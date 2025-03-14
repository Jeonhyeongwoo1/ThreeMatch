﻿// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SweetSugar.Scripts.MapScripts.Editor
{
    [CustomEditor(typeof(MapLevel))]
    public class MapLevelEditor : LevelsEditorBase
    {
        private MapLevel _mapLevel;

        private static GameObject _pendingDeletedGameObject;

        public void OnEnable()
        {
            _mapLevel = target as MapLevel;
            DeletePendingGameObject();
        }
        void OnSceneGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyUp && FindObjectsOfType(typeof(MapLevel)).Count() == _mapLevel.Number)
            {
                if (e.keyCode == KeyCode.G)
                    AddAfter();

            }
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Space(5);

            if (GUILayout.Button("Insert before"))
            {
                List<MapLevel> mapLevels = GetMapLevels();
                int ind = mapLevels.IndexOf(_mapLevel);
                InsertMapLevel(ind, mapLevels);
            }

            if (GUILayout.Button("Insert after"))
            {
                AddAfter();
            }

            if (GUILayout.Button("Delete"))
            {
                Delete();
            }

            UpdateSceneName();

            GUILayout.Space(5);
            GUILayout.EndVertical();

            base.OnInspectorGUI();
        }

        private void AddAfter()
        {
            List<MapLevel> mapLevels = GetMapLevels();
            int ind = mapLevels.IndexOf(_mapLevel);
            InsertMapLevel(ind + 1, mapLevels);
        }

        private void UpdateSceneName()
        {
            string oldSceneName = _mapLevel.SceneName;
            string newSceneName = _mapLevel.LevelScene == null ? null : _mapLevel.LevelScene.name;
            if (oldSceneName != newSceneName)
            {
                _mapLevel.SceneName = newSceneName;
                EditorUtility.SetDirty(_mapLevel);
            }
        }

        private void InsertMapLevel(int ind, List<MapLevel> mapLevels)
        {
            Vector2 position = GetInterpolatedPosition(ind, mapLevels);
            LevelsMap levelsMap = FindObjectOfType<LevelsMap>();
            MapLevel mapLevel = CreateMapLevel(position, ind, levelsMap.MapLevelPrefab);
            mapLevel.transform.parent = _mapLevel.transform.parent;
            mapLevel.transform.SetSiblingIndex(ind);
            mapLevels.Insert(ind, mapLevel);
            UpdateLevelsNumber(mapLevels);
            UpdatePathWaypoints(mapLevels);
            SetStarsEnabled(levelsMap, levelsMap.StarsEnabled);
            Selection.activeGameObject = mapLevel.gameObject;
        }

        private Vector2 GetInterpolatedPosition(int ind, List<MapLevel> mapLevels)
        {
            Vector3 startPosition = mapLevels[Mathf.Max(0, ind - 1)].transform.position;
            Vector3 finishPosition = mapLevels[Mathf.Min(ind, mapLevels.Count - 1)].transform.position;

            if (ind == 0 && mapLevels.Count > 1)
                finishPosition = startPosition + (startPosition - mapLevels[1].transform.position);

            if (ind == mapLevels.Count && mapLevels.Count > 1)
                finishPosition = startPosition + (startPosition - mapLevels[ind - 2].transform.position);

            return (startPosition + finishPosition) / 2;
        }

        private void Delete()
        {
            List<MapLevel> mapLevels = GetMapLevels();
            int ind = mapLevels.IndexOf(_mapLevel);
            mapLevels.Remove(_mapLevel);
            UpdateLevelsNumber(mapLevels);
            UpdatePathWaypoints(mapLevels);
            LevelsMap levelsMap = FindObjectOfType<LevelsMap>();
            Selection.activeGameObject =
                mapLevels.Any()
                    ? mapLevels[Mathf.Max(0, ind - 1)].gameObject
                    : levelsMap.gameObject;
            SetStarsEnabled(levelsMap, levelsMap.StarsEnabled);
            _pendingDeletedGameObject = _mapLevel.gameObject;
        }

        private void DeletePendingGameObject()
        {
            if (_pendingDeletedGameObject != null)
            {
                DestroyImmediate(_pendingDeletedGameObject);
                _pendingDeletedGameObject = null;
            }
        }
    }
}
