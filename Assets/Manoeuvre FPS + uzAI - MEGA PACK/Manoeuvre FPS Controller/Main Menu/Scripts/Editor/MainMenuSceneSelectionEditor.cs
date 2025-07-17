using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(MainMenuSceneSelection))]
    public class MainMenuSceneSelectionEditor : Editor
    {
        MainMenuSceneSelection _mms;
        int currentSelectedSO = 0;

        private void OnEnable()
        {
            _mms = (MainMenuSceneSelection) target;
        }

        public override void OnInspectorGUI()
        {
            //Controller texture
            Texture t = (Texture)Resources.Load("EditorContent/SceneSelectController-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            DrawTopProperties();

            EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);

            DrawSceneObjectsList();

            //DrawDefaultInspector();
        }

        void DrawTopProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Maximum number of allowed scene objects.", EditorStyles.helpBox);
            int MaxAllowedSceneObjects = EditorGUILayout.IntField("Max Allowed Scene Objects", _mms.MaxAllowedSceneObjects);

            EditorGUILayout.LabelField("All the Scene Objects Container.", EditorStyles.helpBox);
            Transform SceneObjectsContainer = (Transform)EditorGUILayout.ObjectField("Scene Objects Container", _mms.SceneObjectsContainer, typeof(Transform));

            EditorGUILayout.LabelField("Scene Object's Prefab which will be spawned.", EditorStyles.helpBox);
            GameObject SceneObjectPrefab = (GameObject)EditorGUILayout.ObjectField("Scene Object Prefab", _mms.SceneObjectPrefab, typeof(GameObject));

            EditorGUILayout.LabelField("Buttons for Next and Previous Scene Objects Scrolling.", EditorStyles.helpBox);
            Button NextButton = (Button)EditorGUILayout.ObjectField("Next Button", _mms.NextButton, typeof(Button));
            Button PreviousButton = (Button)EditorGUILayout.ObjectField("Previous Button", _mms.PreviousButton, typeof(Button));

            EditorGUILayout.LabelField("Buttons Click and Hover Sound FX.", EditorStyles.helpBox);
            string ButtonClickSFX = EditorGUILayout.TextField("Button Click SFX", _mms.ButtonClickSFX);
            string ButtonHoverSFX = EditorGUILayout.TextField("Button Hover SFX", _mms.ButtonHoverSFX);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_mms Top Properties");

                _mms.MaxAllowedSceneObjects = MaxAllowedSceneObjects;
                _mms.SceneObjectsContainer = SceneObjectsContainer;
                _mms.SceneObjectPrefab = SceneObjectPrefab;
                _mms.NextButton = NextButton;
                _mms.PreviousButton = PreviousButton;
                _mms.ButtonClickSFX = ButtonClickSFX;
                _mms.ButtonHoverSFX = ButtonHoverSFX;
            }
        }

        void DrawSceneObjectsList()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add or Clear All the Scene Objects.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("Add New")){
                MainMenuSceneObject newMMS = new MainMenuSceneObject();
                _mms.MainMenuSceneObject.Add(newMMS);
            }

            if(GUILayout.Button("Clear All")){
                _mms.MainMenuSceneObject.Clear();
            }
            
            EditorGUILayout.EndHorizontal();

            if(_mms.MainMenuSceneObject.Count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one Scene Object to proceed.", MessageType.Warning);
                currentSelectedSO = 0;
                return;
            }

            EditorGUILayout.HelpBox("Easily Select Next or Previous Scene Object that you have added in the Scene Objects List.", MessageType.Info);

            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("<--", GUILayout.Height(35)))
            {
                if (currentSelectedSO > 0)
                    currentSelectedSO--;
                else
                    currentSelectedSO = _mms.MainMenuSceneObject.Count - 1;
            }

            EditorGUILayout.LabelField((currentSelectedSO+1) + "/" + _mms.MainMenuSceneObject.Count, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150), GUILayout.Height(35));

            if (GUILayout.Button("--->", GUILayout.Height(35)))
            {
                if(currentSelectedSO < _mms.MainMenuSceneObject.Count-1)
                    currentSelectedSO++;
                else
                    currentSelectedSO = 0;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            string SceneNameToDisplay = EditorGUILayout.TextField("Scene Name To Display", _mms.MainMenuSceneObject[currentSelectedSO].SceneNameToDisplay);

            if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
            {
                _mms.MainMenuSceneObject.RemoveAt(currentSelectedSO);
                if (_mms.MainMenuSceneObject.Count > 0 && currentSelectedSO != 0)
                    currentSelectedSO--;
                else if (_mms.MainMenuSceneObject.Count < 1 && currentSelectedSO == 0)
                    _mms.MainMenuSceneObject.Clear();

                return;
            }

            EditorGUILayout.EndHorizontal();

            string SceneNameInBuildSettings = EditorGUILayout.TextField("Scene Name In Build Settings", _mms.MainMenuSceneObject[currentSelectedSO].SceneNameInBuildSettings);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene Image");
            Sprite SceneImage = (Sprite)EditorGUILayout.ObjectField( _mms.MainMenuSceneObject[currentSelectedSO].SceneImage, typeof(Sprite));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_mms Scene Objects List");
                _mms.MainMenuSceneObject[currentSelectedSO].SceneNameToDisplay = SceneNameToDisplay;
                _mms.MainMenuSceneObject[currentSelectedSO].SceneNameInBuildSettings = SceneNameInBuildSettings;
                _mms.MainMenuSceneObject[currentSelectedSO].SceneImage = SceneImage;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }
    }
}