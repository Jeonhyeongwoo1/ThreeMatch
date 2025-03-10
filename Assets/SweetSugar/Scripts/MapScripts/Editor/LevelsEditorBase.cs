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
    public abstract class LevelsEditorBase : UnityEditor.Editor
    {
        protected List<MapLevel> GetMapLevels()
        {
            return FindObjectsOfType<MapLevel>().OrderBy(ml => ml.Number).ToList();
        }

        protected MapLevel CreateMapLevel(Vector3 position, int number, MapLevel mapLevelPrefab)
        {
            MapLevel mapLevel = PrefabUtility.InstantiatePrefab(mapLevelPrefab) as MapLevel;
            mapLevel.transform.position = position;
            return mapLevel;
        }

        protected void UpdateLevelsNumber(List<MapLevel> mapLevels)
        {
            for (int i = 0; i < mapLevels.Count; i++)
            {
                mapLevels[i].Number = i + 1;
                mapLevels[i].name = string.Format("Level{0:00}", i + 1);
            }
        }

        protected void UpdatePathWaypoints(List<MapLevel> mapLevels)
        {
            Path path = FindObjectOfType<Path>();
            path.Waypoints.Clear();
            foreach (MapLevel mapLevel in mapLevels)
                path.Waypoints.Add(mapLevel.PathPivot);
        }

        protected void SetAllMapLevelsAsDirty()
        {
            GetMapLevels().ForEach(EditorUtility.SetDirty);
        }

        protected void SetStarsEnabled(LevelsMap levelsMap, bool isEnabled)
        {
            levelsMap.SetStarsEnabled(isEnabled);
            if (isEnabled)
                levelsMap.SetStarsType(levelsMap.StarsType);
            EditorUtility.SetDirty(levelsMap);
            SetAllMapLevelsAsDirty();
        }
    }
}
