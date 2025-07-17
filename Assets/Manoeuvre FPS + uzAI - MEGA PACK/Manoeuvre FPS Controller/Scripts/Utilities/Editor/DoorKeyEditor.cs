using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(DoorKey))]
    public class DoorKeyEditor : Editor
    {
        DoorKey _dk;

        SerializedObject so_dk;
        SerializedProperty OnPickup;

        private void OnEnable()
        {
            _dk = (DoorKey)target;

            so_dk = new SerializedObject(_dk);
            OnPickup = so_dk.FindProperty("OnPickup");

        }

        public override void OnInspectorGUI()
        {
            DrawNewInspector();

            //DrawDefaultInspector();

        }

        void DrawNewInspector()
        {
            Texture t = (Texture)Resources.Load("EditorContent/DoorKey-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            string InteractionText = EditorGUILayout.TextField("Interaction Text", _dk.InteractionText);

            AudioClip pickupSound = EditorGUILayout.ObjectField("Pickup Sound", _dk.pickupSound, typeof(AudioClip)) as AudioClip;

            bool autoPickup = EditorGUILayout.Toggle("Auto Pickup", _dk.autoPickup);

            bool rotateKey = EditorGUILayout.Toggle("Rotate Key", _dk.rotateKey);

            float rotateSpeed = _dk.rotateSpeed;

            if (rotateKey)
            {
                rotateSpeed = EditorGUILayout.FloatField("Rotate Speed", _dk.rotateSpeed);
            }

            EditorGUILayout.EndVertical();

            DrawCorrespondingSwitches();

            //event
            EditorGUILayout.PropertyField(OnPickup);

            so_dk.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Door Key");

                _dk.InteractionText = InteractionText;

                _dk.pickupSound = pickupSound;

                _dk.autoPickup = autoPickup;

                _dk.rotateKey = rotateKey;
                _dk.rotateSpeed = rotateSpeed;
            }

        }

        void DrawCorrespondingSwitches()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Corresponding Switches", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Add all the switches/doors that can be opened using this DoorKey.", EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Add"))
            {
                DoorAction _nda = new DoorAction();
                _dk.CorrespondingSwitches.Add(_nda);
            }

            if (GUILayout.Button("Clear"))
            {
                _dk.CorrespondingSwitches.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _dk.CorrespondingSwitches.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("Box");

                DoorAction cs_dk = EditorGUILayout.ObjectField(_dk.CorrespondingSwitches[i], typeof(DoorAction)) as DoorAction;

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _dk.CorrespondingSwitches.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Corresponding Switches");

                    _dk.CorrespondingSwitches[i] = cs_dk;
                }
            }

            EditorGUILayout.EndVertical();

        }
    }
}