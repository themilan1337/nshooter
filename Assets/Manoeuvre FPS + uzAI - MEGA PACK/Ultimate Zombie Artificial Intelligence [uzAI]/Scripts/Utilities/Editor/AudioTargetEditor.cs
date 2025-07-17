using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace uzAI
{
    [CustomEditor(typeof(AudioTarget))]
    public class AudioTargetEditor : Editor
    {
        AudioTarget _at;

        private void OnEnable()
        {
            _at = (AudioTarget)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            // texture
            Texture t = (Texture)Resources.Load("EditorContent/audiotarget-icon");

            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("If true, this will be triggered as soon as this GameObject / script is enabled.", EditorStyles.helpBox);
            bool enableAtStart = EditorGUILayout.Toggle("Enable At Start", _at.enableAtStart);

            EditorGUILayout.LabelField("Define which audio clip should be played when alerting nearby Zombies.", EditorStyles.helpBox);
            AudioClip clipToPlay = (AudioClip)EditorGUILayout.ObjectField("Clip To Play", _at.clipToPlay, typeof(AudioClip));

            if (clipToPlay == null)
                EditorGUILayout.HelpBox("Please Assign an Audio Clip", MessageType.Error);

            EditorGUILayout.LabelField("From how far zombies can hear this Audio Target's Sound.", EditorStyles.helpBox);
            float AudioRange = EditorGUILayout.Slider("Audio Range", _at.AudioRange, 5f, 150f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzai audio target");

                _at.enableAtStart = enableAtStart;
                _at.AudioRange = AudioRange;
                _at.clipToPlay = clipToPlay;
            }

         //   DrawDefaultInspector();
        }

    }
}