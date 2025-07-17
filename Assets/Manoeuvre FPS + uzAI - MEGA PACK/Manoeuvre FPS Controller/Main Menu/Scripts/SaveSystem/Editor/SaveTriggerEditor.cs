using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(SaveTrigger))]
    public class SaveTriggerEditor : Editor
    {

        SaveTrigger _st;

        SerializedObject _SO_st;
        SerializedProperty OnResumeFromHere;

        private void OnEnable()
        {
            _st = (SaveTrigger)target;

            _SO_st = new SerializedObject(_st);
            OnResumeFromHere = _SO_st.FindProperty("OnResumeFromHere");
        }

        public override void OnInspectorGUI()
        {
            //weapon texture
            Texture t = (Texture)Resources.Load("EditorContent/SaveTrigger-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("This is the Text which will be displayed in the Slots of Load and Save Menu. \n" +
                "This can be anything from Scene Name or Percentage or Mission Name, it is just matter of preference and" +
                "type of game you are making.", MessageType.Info);

            string ProgressMessage = EditorGUILayout.TextField("Progress Message", _st.ProgressMessage);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            DrawPreviousSaveTriggersList();

            EditorGUILayout.EndVertical();

            EditorGUILayout.HelpBox("Please add here all the methods or events you want to Invoke whenever player Resumes / Load the game from this Save Trigger.", MessageType.Info);

            //draw on detonate event
            EditorGUILayout.PropertyField(OnResumeFromHere);

            _SO_st.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Save Trigger MFPS");

                _st.ProgressMessage = ProgressMessage;
            }

            //DrawDefaultInspector();

        }

        void DrawPreviousSaveTriggersList()
        {

            EditorGUILayout.HelpBox("This is where magic happens. \n" +
                "Just Add the Save Triggers which appears before this trigger in your Game. " +
                "So whenever your game is loaded from here, All the previous progress will be loaded which is confined to the Added Triggers. \n" +
                "If this is your first Trigger, leave this blank.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                SaveTrigger saveTrigger = new SaveTrigger();
                _st.PreviousSaveTriggers.Add(saveTrigger);
            }

            if (GUILayout.Button("Clear"))
            {
                _st.PreviousSaveTriggers.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _st.PreviousSaveTriggers.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                SaveTrigger trigger = (SaveTrigger) EditorGUILayout.ObjectField(_st.PreviousSaveTriggers[i], typeof(SaveTrigger));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _st.PreviousSaveTriggers.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Save triggers List");

                    _st.PreviousSaveTriggers[i] = trigger;
                }

            }
        }
    }
}