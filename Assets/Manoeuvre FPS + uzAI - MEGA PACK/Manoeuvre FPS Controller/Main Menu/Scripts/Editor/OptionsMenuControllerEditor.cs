using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(OptionsMenuController))]
    public class OptionsMenuControllerEditor : Editor
    {
        OptionsMenuController _omc;

        private void OnEnable()
        {
            _omc = (OptionsMenuController)target;
        }

        public override void OnInspectorGUI()
        {
            //Controller texture
            Texture t = (Texture)Resources.Load("EditorContent/OptionsMenuController-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            DrawOptionsControllerProperties();

            //DrawDefaultInspector();
        }

        void DrawOptionsControllerProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("All the UI References. There's nothing for you to edit in Inspector. The script handles everything automatically.", MessageType.Info);

            Text TextureQualityValue = (Text)EditorGUILayout.ObjectField("Texture Quality Value", _omc.TextureQualityValue, typeof(Text));
            Text AntiAliasingValue = (Text)EditorGUILayout.ObjectField("Anti Aliasing Value", _omc.AntiAliasingValue, typeof(Text));
            Text ShadowsValue = (Text)EditorGUILayout.ObjectField("Shadows Value", _omc.ShadowsValue, typeof(Text));
            Text VSyncValue = (Text)EditorGUILayout.ObjectField("VSync Value", _omc.VSyncValue, typeof(Text));
            Text ResolutionValue = (Text)EditorGUILayout.ObjectField("Resolution Value", _omc.ResolutionValue, typeof(Text));

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_omc Properties");

                _omc.TextureQualityValue = TextureQualityValue;
                _omc.AntiAliasingValue = AntiAliasingValue;
                _omc.ShadowsValue = ShadowsValue;
                _omc.VSyncValue = VSyncValue;
                _omc.ResolutionValue = ResolutionValue;
            }
        }
    }
}