using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(DynamicBarricades))]
    public class DynamicBarricadesEditor : Editor
    {
        DynamicBarricades _db;
        bool childList;

        private void OnEnable()
        {
            _db = (DynamicBarricades) target;    
        }

        public override void OnInspectorGUI()
        {
            DrawNewInspector();

          //  DrawDefaultInspector();
        }

        void DrawNewInspector()
        {
            Texture t = Resources.Load("EditorContent/Barricades-icon") as Texture;
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("If true, all the barricades will be disabled.", EditorStyles.helpBox);
            bool startDisabled = EditorGUILayout.Toggle("Start Disabled", _db.startDisabled);

            EditorGUILayout.LabelField("Total Time Needed to Contruct all the Barricades. Experiment with this value to increase or decrease accordingly.", EditorStyles.helpBox);
            float totalContructionLength = EditorGUILayout.Slider("Total Contruction Length", _db.totalContructionLength, 0.1f, 10f);

            EditorGUILayout.LabelField("Layermask of the Barricades", EditorStyles.helpBox);
            LayerMask BarricadeLayer = LayerMaskUtility.LayerMaskField("Barricade Layer", _db.BarricadeLayer);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Subtle Particle Effect which will be played when a barricade is added.", EditorStyles.helpBox);
            ParticleSystem AddFx = (ParticleSystem) EditorGUILayout.ObjectField("Add Fx", _db.AddFx, typeof(ParticleSystem));

            EditorGUILayout.LabelField("Audio Clip which will be played when barricades will be added.", EditorStyles.helpBox);
            AudioClip AddSFX = (AudioClip)EditorGUILayout.ObjectField("Add SFX", _db.AddSFX, typeof(AudioClip));

            EditorGUILayout.LabelField("Audio Clip which will be played when the LAST barricade will be added.", EditorStyles.helpBox);
            AudioClip CompletionSFX = (AudioClip)EditorGUILayout.ObjectField("Completion SFX", _db.CompletionSFX, typeof(AudioClip));

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawChildBarricades();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Dynamic Barricades");
                _db.totalContructionLength = totalContructionLength;
                _db.startDisabled = startDisabled;
                _db.BarricadeLayer = BarricadeLayer;
                _db.AddFx = AddFx;
                _db.AddSFX = AddSFX;
                _db.CompletionSFX = CompletionSFX;
            }

        }

        void DrawChildBarricades()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add all the Child Barricades that this object will be having below.", MessageType.Info);

            string s = childList ? "Collapse" : "Expand";
            childList = GUILayout.Toggle(childList,s, "Button");

            if (childList)
            {
                EditorGUILayout.LabelField("Total Barricades : " +  _db.ChildBarricades.Count , EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.HelpBox("Based upon the Total Construction Time above, Construction of Single Barricade will take : " + _db.totalContructionLength / _db.ChildBarricades.Count + " s", MessageType.Info);

                EditorGUILayout.BeginHorizontal("Box");

                if (GUILayout.Button("Add"))
                {
                    Transform t = new GameObject().transform;

                    _db.ChildBarricades.Add(t);

                    DestroyImmediate(t.gameObject);
                }

                if (GUILayout.Button("Clear"))
                {
                    _db.ChildBarricades.Clear();
                }

                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < _db.ChildBarricades.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.BeginHorizontal("Box");

                    Transform ChildBarricades = (Transform)EditorGUILayout.ObjectField(_db.ChildBarricades[i], typeof(Transform));

                    if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                    {
                        _db.ChildBarricades.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Child Barricades List");

                        _db.ChildBarricades[i] = ChildBarricades;

                    }
                }

            }
            
            EditorGUILayout.EndVertical();

        }
    }
}