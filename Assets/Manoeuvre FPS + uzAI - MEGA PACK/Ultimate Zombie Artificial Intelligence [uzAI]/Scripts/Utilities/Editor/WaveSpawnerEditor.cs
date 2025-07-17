using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace uzAI
{
    [CustomEditor(typeof(WaveSpawner))]
    public class WaveSpawnerEditor : Editor
    {
        WaveSpawner _ws;

        private void OnEnable()
        {
            _ws = (WaveSpawner)target;
        }

        public override void OnInspectorGUI()
        {

            DrawNewInspector();

            //DrawDefaultInspector();
        }

        void DrawNewInspector()
        {
            Texture t = Resources.Load("EditorContent/WaveSpawner-icon") as Texture;
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            Drawtabs();

        }

        void Drawtabs()
        {
            _ws.tabCount = GUILayout.Toolbar(_ws.tabCount, new string[] {"Waves", "Spawn Points" });

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            switch (_ws.tabCount)
            {
                case 0:
                    DrawWavesProperties();
                    break;

                case 1:
                    DrawSpawnPointsProperties();
                    break;
            }
        }

        void DrawWavesProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Target Transform which the Zombies will be chasing. This can be anything!", EditorStyles.helpBox);
            Transform Target = (Transform) EditorGUILayout.ObjectField("Target", _ws.Target, typeof(Transform));

            EditorGUILayout.LabelField("HUD Text UI Duration.", EditorStyles.helpBox);
            float UIDuration = EditorGUILayout.FloatField("UI Duration", _ws.UIDuration);
            UIDuration = Mathf.Clamp(UIDuration, 0, UIDuration);

            EditorGUILayout.LabelField("Delay between each wave.", EditorStyles.helpBox);
            float WaveStartDelay = EditorGUILayout.FloatField("Wave Start Delay", _ws.WaveStartDelay);
            WaveStartDelay = Mathf.Clamp(WaveStartDelay, 0, WaveStartDelay);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Add Sounds FX for Wave Start, Wave End and Waves Finished.", EditorStyles.helpBox);

            AudioClip WaveStartClip = (AudioClip) EditorGUILayout.ObjectField("Wave Start Clip", _ws._soundProperties.WaveStartClip, typeof(AudioClip));
            AudioClip WaveEndClip = (AudioClip) EditorGUILayout.ObjectField("Wave End Clip", _ws._soundProperties.WaveEndClip, typeof(AudioClip));
            AudioClip WaveFinishClip = (AudioClip) EditorGUILayout.ObjectField("Wave Finish Clip", _ws._soundProperties.WaveFinishClip, typeof(AudioClip));

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawWavesList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Waves Properties");

                _ws.Target = Target;
                _ws.UIDuration = UIDuration;
                _ws.WaveStartDelay = WaveStartDelay;

                _ws._soundProperties.WaveStartClip = WaveStartClip;
                _ws._soundProperties.WaveEndClip = WaveEndClip;
                _ws._soundProperties.WaveFinishClip = WaveFinishClip;
                
            }
        }

        void DrawWavesList()
        {

            EditorGUILayout.HelpBox("Specify the Number of Zombies in each wave and also the Types of Zombies.", MessageType.Info);

            if(GUILayout.Button("Add New Wave"))
            {
                _ws._Waves.Add(new Wave());
            }

            for(int i =0; i< _ws._Waves.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Wave " + (i+1) + " Properties", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal("Box");

                int NoOfZombies = EditorGUILayout.IntField("No Of Zombies", _ws._Waves[i].NoOfZombies);
                NoOfZombies = Mathf.Clamp(NoOfZombies, _ws._Waves[i]._ZombieTypes.Count, NoOfZombies);

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _ws._Waves.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                DrawTypesOfZombiesList(i);

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    _ws._Waves[i].NoOfZombies = NoOfZombies;

                    Undo.RecordObject(target, "Waves List");
                }

            }

        }

        void DrawTypesOfZombiesList(int index)
        {
            EditorGUILayout.BeginVertical("Helpbox"); 

            if(GUILayout.Button("Add New AI Type"))
            {
                _ws._Waves[index]._ZombieTypes.Add(new uzAIZombieStateManager());

            }

            for(int i = 0; i < _ws._Waves[index]._ZombieTypes.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Type : " + (i + 1), GUILayout.Width(75));

                uzAIZombieStateManager _ZombieTypes = (uzAIZombieStateManager) EditorGUILayout.ObjectField(_ws._Waves[index]._ZombieTypes[i], typeof(uzAIZombieStateManager));

                if(GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _ws._Waves[index]._ZombieTypes.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Types Of Zombies List");
                    _ws._Waves[index]._ZombieTypes[i] = _ZombieTypes;
                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawSpawnPointsProperties()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("If the Distance between target and spawn point is greater than this Max Spawn Distance, " +
               "the Spawn Point will be disabled and won't spawn any Zombie!", EditorStyles.helpBox);
            float maxSpawnDistance = EditorGUILayout.FloatField("Max Spawn Distance", _ws.maxSpawnDistance);
            maxSpawnDistance = Mathf.Clamp(maxSpawnDistance, 0, maxSpawnDistance);

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawSpawnPointsList();

            if (EditorGUI.EndChangeCheck()){

                Undo.RecordObject(target, "Wave Spawn Points Properties");
                _ws.maxSpawnDistance = maxSpawnDistance;

            }
        }

        void DrawSpawnPointsList()
        {
            EditorGUILayout.BeginVertical("Helpbox");

            EditorGUILayout.LabelField("Total SpawnPoints : " + _ws._SpawnPoints.Count, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.HelpBox("Add a new Spawn Point and Mention How many Maximum Zombies Are Allowed to be Spawned here at once.", MessageType.Info);

            if (GUILayout.Button("Add new Spawn Point"))
            {
                _ws._SpawnPoints.Add(new WaveSpawnPoint());
            }

            for(int i =0;i<_ws._SpawnPoints.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal("Helpbox");

                Transform SpawnPoint = (Transform)EditorGUILayout.ObjectField("Spawn Point", _ws._SpawnPoints[i].SpawnPoint, typeof(Transform));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _ws._SpawnPoints.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                int maxAllowed = EditorGUILayout.IntField("Max Allowed", _ws._SpawnPoints[i].maxAllowed);

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Waves Spawn Points List");

                    _ws._SpawnPoints[i].SpawnPoint = SpawnPoint;
                    _ws._SpawnPoints[i].maxAllowed = maxAllowed;

                }
            }

            EditorGUILayout.EndVertical();

        }

        private void OnSceneGUI()
        {
            if (_ws._SpawnPoints.Count < 1)
                return;

            for(int i =0;i< _ws._SpawnPoints.Count; i++)
            {
                if (_ws._SpawnPoints[i].SpawnPoint != null)
                {
                    Color c = Color.cyan;
                    c.a = 0.25f;
                    Handles.color = c;

                    Handles.SphereHandleCap(1,_ws._SpawnPoints[i].SpawnPoint.position, Quaternion.identity, 2f, EventType.Repaint);

                }
            }
        }

    }
}