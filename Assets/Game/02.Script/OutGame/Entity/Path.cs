﻿using System.Collections.Generic;
using ThreeMatch.Utils;
using UnityEngine;

namespace ThreeMatch.OutGame.Entity
{
    public class Path : MonoBehaviour
    {
        // [HideInInspector]
        public List<Transform> Waypoints = new List<Transform>();

        public bool IsCurved;

        public Color GizmosColor = Color.white;

        public float GizmosRadius = 1.0f;

        public void OnDrawGizmos()
        {
            Gizmos.color = GizmosColor;
            for (var i = 0; i < Waypoints.Count; i++)
            {
                Gizmos.DrawSphere(Waypoints[i].transform.position, GizmosRadius);
                if (i < Waypoints.Count - 1)
                    DrawPart(i);
            }
        }

        private void DrawPart(int ind)
        {
            if (IsCurved)
            {
                var devidedPoints = GetDivededPoints(ind);
                for (var i = 0; i < devidedPoints.Length - 1; i++)
                    Gizmos.DrawLine(devidedPoints[i], devidedPoints[i + 1]);
            }
            else
            {
                Gizmos.DrawLine(Waypoints[ind].position, Waypoints[(ind + 1) % Waypoints.Count].position);
            }
        }

        private Vector2[] GetDivededPoints(int ind)
        {
            var points = new Vector2[11];
            var pointInd = 0;
            var indexes = GetSplinePointIndexes(ind, true);
            Vector2 a = Waypoints[indexes[0]].transform.position;
            Vector2 b = Waypoints[indexes[1]].transform.position;
            Vector2 c = Waypoints[indexes[2]].transform.position;
            Vector2 d = Waypoints[indexes[3]].transform.position;
            for (float t = 0; t <= 1.001f; t += 0.1f)
                points[pointInd++] = SplineCurve.GetPoint(a, b, c, d, t);
            return points;
        }

        public int[] GetSplinePointIndexes(int baseInd, bool isForwardDirection)
        {
            var dInd = isForwardDirection ? 1 : -1;
            return new[]
            {
                Mathf.Clamp(baseInd - dInd, 0, Waypoints.Count - 1),
                baseInd,
                Mathf.Clamp(baseInd + dInd, 0, Waypoints.Count - 1),
                Mathf.Clamp(baseInd + 2*dInd, 0, Waypoints.Count - 1)
            };
        }

        public float GetLength()
        {
            float length = 0;
            for (var i = 0; i < Waypoints.Count; i++)
            {
                Vector2 p1 = Waypoints[i].transform.position;
                Vector2 p2 = Waypoints[(i + 1) % Waypoints.Count].transform.position;
                length += Vector2.Distance(p1, p2);
            }
            return length;
        }

        public float GetLength(int startInd)
        {
            return Vector2.Distance(
                Waypoints[startInd].transform.position,
                Waypoints[(startInd + 1) % Waypoints.Count].transform.position);
        }

    }
}
