using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace uzAI
{
    [CustomEditor(typeof(WaypointsPath))]
    public class WaypointsPathEditor : Editor
    {
        WaypointsPath _wp;

        List<Transform> waypoints = new List<Transform>();

        int startPoint;
        int endPoint;

        int selectedMode;

        private void OnEnable()
        {
             _wp = (WaypointsPath) target;
        }

        public override void OnInspectorGUI()
        {
            Texture t = (Texture)Resources.Load("EditorContent/waypointsPath-icon");

            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            DrawToolbar();

           // DrawDefaultInspector();
        }

        void DrawToolbar()
        {

            selectedMode = GUILayout.Toolbar(selectedMode, new string[] {"Edit", "Debug" });

            GUILayout.Label("", GUI.skin.horizontalSlider);

            if(selectedMode == 0)
                AddWaypoint();
            else if(selectedMode == 1)
                DebugWaypoints();

        }

        void DebugWaypoints()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Debug Patrol Path from Start Point to End Point.", MessageType.Info);

            if (_wp.waypoints.Count > 1)
            {

                startPoint = EditorGUILayout.IntSlider("Start Point", startPoint, 0, _wp.waypoints.Count - 1);
                endPoint = EditorGUILayout.IntSlider("End Point", endPoint, 0, _wp.waypoints.Count - 1);
            }
            else
                EditorGUILayout.HelpBox("Add at least 2 points to debug a patrol route.", MessageType.Error);

            EditorGUILayout.EndVertical();


        }

        void AddWaypoint()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Add a New Waypoint for the Patrol Path", MessageType.Info);

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            if (GUILayout.Button("Add"))
            {

                Transform newWP = new GameObject().transform;
                newWP.SetParent(_wp.transform);
                _wp.waypoints.Add(newWP);

            }

            EditorGUILayout.EndVertical();

            waypoints = _wp.waypoints;

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Total Waypoints : " + _wp.waypoints.Count, EditorStyles.centeredGreyMiniLabel);

            if (waypoints.Count == 0)
            {
                EditorGUILayout.HelpBox("Add at least 1 point to the patrol route.", MessageType.Error);
                return;
            }
            else
            {
                EditorGUILayout.BeginVertical();

                for (int i = 0; i < _wp.waypoints.Count; i++)
                {

                    if (waypoints[i] == null)
                    {
                        _wp.waypoints.RemoveAt(i);
                        break;
                    }


                    //if (i > 0)
                    //    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.BeginHorizontal("box");

                    waypoints[i] = (Transform)EditorGUILayout.ObjectField(_wp.waypoints[i], typeof(Transform));
                    waypoints[i].name = "Waypoint : " + i;

                    if (GUILayout.Button("X", EditorStyles.miniButtonRight))
                    {
                        DestroyImmediate(_wp.waypoints[i].gameObject);
                        _wp.waypoints.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void OnSceneGUI()
        {
            Handles.Label(_wp.transform.position, _wp.transform.name);

            Color c = Color.green;
            c.a = 0.5f;
            Handles.color = c;

            Handles.DrawWireDisc(_wp.transform.position, Vector3.up, 2f);

            if (selectedMode == 0)
                DrawPositionHandles();
            else if (selectedMode == 1)
                DrawPath();
        }

        private void DrawPositionHandles()
        {
            if (_wp.waypoints.Count < 1)
                return;

            for (int i = 0; i < _wp.waypoints.Count; i++)
            {
                if (_wp.waypoints[i] == null)
                    break;

                _wp.waypoints[i].transform.position = Handles.PositionHandle(_wp.waypoints[i].transform.position, _wp.waypoints[i].transform.rotation);
                Color c1 = Color.red;
                c1.a = 0.5f;
                Handles.color = c1;
                Handles.Label(_wp.waypoints[i].transform.position, _wp.waypoints[i].transform.name);
                Handles.SphereHandleCap(0, _wp.waypoints[i].transform.position, _wp.waypoints[i].transform.rotation, 1f, EventType.Repaint);

            }
        }

        void DrawPath()
        {

            for (int i = 0; i < _wp.waypoints.Count; i++)
            {
                if (_wp.waypoints[i] == null)
                    break;

                Color c1 = Color.red;
                c1.a = 0.5f;
                Handles.color = c1;
                Handles.Label(_wp.waypoints[i].transform.position, _wp.waypoints[i].transform.name);
                Handles.SphereHandleCap(0, _wp.waypoints[i].transform.position, _wp.waypoints[i].transform.rotation, 1f, EventType.Repaint);

            }

            if (_wp.waypoints.Count < 2)
                return;

            if (_wp.waypoints[startPoint] != null && _wp.waypoints[endPoint] != null && _wp.waypoints.Count > 1)
            {

                NavMeshPath path = new NavMeshPath();
                Vector3 startPos = _wp.waypoints[startPoint].position;
                Vector3 endPos = _wp.waypoints[endPoint].position;
                NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path);
                Handles.color = Color.green;
                Handles.DrawPolyLine(path.corners);
            }

        }

    }
}