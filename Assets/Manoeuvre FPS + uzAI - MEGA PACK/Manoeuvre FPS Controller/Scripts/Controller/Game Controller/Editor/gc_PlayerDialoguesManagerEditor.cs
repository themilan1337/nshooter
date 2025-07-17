using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(gc_PlayerDialoguesManager))]
    public class gc_PlayerDialoguesManagerEditor : Editor
    {
        gc_PlayerDialoguesManager _dm;

        bool togglePickupsList = false;
        bool toggleKillsList = false;

        private void OnEnable()
        {
            _dm = (gc_PlayerDialoguesManager) target;

        }

        public override void OnInspectorGUI()
        {
            DrawNewInspector();

            //DrawDefaultInspector();
        }

        void DrawNewInspector()
        {
            EditorGUI.BeginChangeCheck();

            Texture t = Resources.Load("EditorContent/Dialogues-icon") as Texture;

            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("If true, player will say dialogues on picking up items and killing target objects such as Turrets, uzAI Zombie, Destructible props, etc", MessageType.Info);
            bool enableDialogues = EditorGUILayout.Toggle("Enable Dialogues", _dm.enableDialogues);

            EditorGUILayout.EndVertical();

            float pickupDialogueFrequency = _dm.pickupDialogueFrequency;
            float killsDialogueFrequency = _dm.killsDialogueFrequency;
            float DialoguePitch = _dm.DialoguePitch;
            float DialogueVolume = _dm.DialogueVolume;

            if (_dm.enableDialogues)
            {

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Set Dialogues frequency i.e 0 means no dialogues, 1 means everytime", EditorStyles.helpBox);

                pickupDialogueFrequency = EditorGUILayout.Slider("Pickup Dialogue Frequency", _dm.pickupDialogueFrequency, 0, 1);
                killsDialogueFrequency = EditorGUILayout.Slider("Kills Dialogue Frequency", _dm.killsDialogueFrequency, 0, 1);


                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Set Dialogues Volume and Pitch.", EditorStyles.helpBox);

                DialoguePitch = EditorGUILayout.Slider("Dialogue Pitch", _dm.DialoguePitch, 0, 1);
                DialogueVolume = EditorGUILayout.Slider("Dialogue Volume", _dm.DialogueVolume, 0, 1);

                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                DrawPickupDialoguesList();

                DrawKillsDialoguesList();

            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "gc_Dialogues Manager");

                _dm.enableDialogues = enableDialogues;
                _dm.pickupDialogueFrequency = pickupDialogueFrequency;
                _dm.killsDialogueFrequency = killsDialogueFrequency;

                _dm.DialogueVolume = DialogueVolume;
                _dm.DialoguePitch = DialoguePitch;
            }
        }

        void DrawPickupDialoguesList() {

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Pickups Dialogue List", EditorStyles.centeredGreyMiniLabel);

            string s = togglePickupsList ? "Collapse" : "Expand";
            togglePickupsList = GUILayout.Toggle(togglePickupsList, s,  "Button");

            if (togglePickupsList)
            {
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Total Dialogues : " + _dm.PickupDialoguesList.Count, EditorStyles.centeredGreyMiniLabel);

                if (GUILayout.Button("Add"))
                {
                    AudioClip newAC = null;
                    _dm.PickupDialoguesList.Add(newAC);
                }

                for (int i = 0; i < _dm.PickupDialoguesList.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.BeginHorizontal("Box");
                    AudioClip clip = (AudioClip)EditorGUILayout.ObjectField("", _dm.PickupDialoguesList[i], typeof(AudioClip));

                    if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                    {
                        _dm.PickupDialoguesList.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "PickupDialoguesList");
                        _dm.PickupDialoguesList[i] = clip;
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
            }
            else
                EditorGUILayout.EndVertical();
        }

        void DrawKillsDialoguesList() {

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Kills Dialogue List", EditorStyles.centeredGreyMiniLabel);

            string s = toggleKillsList ? "Collapse" : "Expand";
            toggleKillsList = GUILayout.Toggle(toggleKillsList, s,  "Button");

            if (toggleKillsList)
            {
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Total Dialogues : " + _dm.KillsDialoguesList.Count, EditorStyles.centeredGreyMiniLabel);

                if (GUILayout.Button("Add"))
                {
                    AudioClip newAC = null; 
                    _dm.KillsDialoguesList.Add(newAC);
                }

                for (int i = 0; i < _dm.KillsDialoguesList.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.BeginHorizontal("Box");
                    AudioClip clip = (AudioClip)EditorGUILayout.ObjectField("", _dm.KillsDialoguesList[i], typeof(AudioClip));

                    if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                    {
                        _dm.KillsDialoguesList.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "KillsDialoguesList");
                        _dm.KillsDialoguesList[i] = clip;
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
            }
            else
                EditorGUILayout.EndVertical();
        }

    }
}