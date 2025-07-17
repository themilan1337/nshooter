using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Manoeuvre
{
    [CustomEditor(typeof(MainMenuController))]
    public class MainMenuControllerEditor : Editor
    {
        MainMenuController _mmc;

        int propertyTab;

        private void OnEnable()
        {
            _mmc = (MainMenuController)target;
        }

        public override void OnInspectorGUI()
        {
            //Controller texture
            Texture t = (Texture)Resources.Load("EditorContent/MMController-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.HelpBox("Toggle the Cursor Visibility while navigating the Main Menu.", MessageType.Info);
            bool HideCursor = EditorGUILayout.Toggle("Hide Cursor",_mmc.HideCursor);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "cursor");
                _mmc.HideCursor = HideCursor;
            }

            DrawMainMenuPropertiesTab();

            //DrawDefaultInspector();
        }

        void DrawMainMenuPropertiesTab()
        {
            propertyTab = GUILayout.Toolbar(propertyTab, new string[] {"Hover Texts", "Animations", "Scenes", "Buttons SFX" });

            switch (propertyTab)
            {
                case 0:
                    DrawHoverTextsTab();
                    break;

                case 01:
                    DrawAnimationsTab();
                    break;

                case 02:
                    DrawScenesTab();
                    break;

                case 03:
                    DrawButtonsSFXTab();
                    break;
            }
        }

        void DrawHoverTextsTab()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("These texts will appear whenever the player Hover Over / Select the respective buttons.", MessageType.Info);

            Text ButtonInfoText = (Text) EditorGUILayout.ObjectField("Button Info Text", _mmc.ButtonInfoText, typeof(Text));
            EditorGUILayout.Space();
            string StartBtnHoverText = EditorGUILayout.TextField("Start Btn Hover Text", _mmc.StartBtnHoverText);
            string LoadBtnHoverText = EditorGUILayout.TextField("Load Btn Hover Text", _mmc.LoadBtnHoverText);
            string ShopBtnHoverText = EditorGUILayout.TextField("Shop Btn Hover Text", _mmc.ShopBtnHoverText);
            string OptionsBtnHoverText = EditorGUILayout.TextField("Options Btn Hover Text", _mmc.OptionsBtnHoverText);
            string QuitBtnHoverText = EditorGUILayout.TextField("Quit Btn Hover Text", _mmc.QuitBtnHoverText);
            string LoadSlotBtnHoverText = EditorGUILayout.TextField("Load Slot Btn Hover Text", _mmc.LoadSlotBtnHoverText);
            string TextureQualityText = EditorGUILayout.TextField("Texture Quality Text", _mmc.TextureQualityText);
            string AntiAliasingText = EditorGUILayout.TextField("Anti Aliasing Text", _mmc.AntiAliasingText);
            string ShadowsText = EditorGUILayout.TextField("Shadows Text", _mmc.ShadowsText);
            string VSyncText = EditorGUILayout.TextField("VSync Text", _mmc.VSyncText);
            string ResolutionText = EditorGUILayout.TextField("Resolution Text", _mmc.ResolutionText);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_mmc Hover Texts");

                _mmc.ButtonInfoText = ButtonInfoText;
                _mmc.StartBtnHoverText = StartBtnHoverText;
                _mmc.LoadBtnHoverText = LoadBtnHoverText;
                _mmc.ShopBtnHoverText = ShopBtnHoverText;
                _mmc.OptionsBtnHoverText = OptionsBtnHoverText;
                _mmc.QuitBtnHoverText = QuitBtnHoverText;
                _mmc.LoadSlotBtnHoverText = LoadSlotBtnHoverText;
                _mmc.TextureQualityText = TextureQualityText;
                _mmc.AntiAliasingText = AntiAliasingText;
                _mmc.ShadowsText = ShadowsText;
                _mmc.VSyncText = VSyncText;
                _mmc.ResolutionText = ResolutionText;
            }
        }

        void DrawAnimationsTab()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Animation names and the Animator Controller itself. So you just have to change the Animations, and write their names below..!", MessageType.Info);

            Animator _animator = (Animator)EditorGUILayout.ObjectField("Animator", _mmc._animator, typeof(Animator));
            string OptionsMenu_OpenAnimation = EditorGUILayout.TextField("Options Menu_Open Animation", _mmc.OptionsMenu_OpenAnimation);
            string OptionsMenu_CloseAnimation = EditorGUILayout.TextField("Options Menu_Close Animation", _mmc.OptionsMenu_CloseAnimation);
            string LoadMenu_OpenAnimation = EditorGUILayout.TextField("Load Menu_Open Animation", _mmc.LoadMenu_OpenAnimation);
            string LoadMenu_CloseAnimation = EditorGUILayout.TextField("Load Menu_Close Animation", _mmc.LoadMenu_CloseAnimation);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_mmc Animation Tabs");

                _mmc._animator = _animator;
                _mmc.OptionsMenu_OpenAnimation = OptionsMenu_OpenAnimation;
                _mmc.OptionsMenu_CloseAnimation = OptionsMenu_CloseAnimation;
                _mmc.LoadMenu_OpenAnimation = LoadMenu_OpenAnimation;
                _mmc.LoadMenu_CloseAnimation = LoadMenu_CloseAnimation;
            }
        }

        void DrawScenesTab()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Scene Names of Scene Select Menu and Shop Menu.", MessageType.Info);

            string SceneSelectName = EditorGUILayout.TextField("Scene Select Name", _mmc.SceneSelectName);
            string ShopSelectName = EditorGUILayout.TextField("Shop Select Name", _mmc.ShopSelectName);


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_mmc Scene Tab");

                _mmc.SceneSelectName = SceneSelectName;
                _mmc.ShopSelectName = ShopSelectName;
            }
        }

        void DrawButtonsSFXTab()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("SFX names of Button Click and Hover sounds.", MessageType.Info);

            string ButtonClickSFX = EditorGUILayout.TextField("Button Click SFX", _mmc.ButtonClickSFX);
            string ButtonHoverSFX = EditorGUILayout.TextField("Button Hover SFX", _mmc.ButtonHoverSFX);


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_mmc Button SFX");

                _mmc.ButtonClickSFX = ButtonClickSFX;
                _mmc.ButtonHoverSFX = ButtonHoverSFX;
            }
        }
    }
}