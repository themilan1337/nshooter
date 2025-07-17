using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : Editor
    {
        CameraController _cameraController;
        bool _showBobProperties;

        private void OnEnable()
        {
            _cameraController = (CameraController)target;
        }

        public override void OnInspectorGUI()
        {
            //Controller texture
            Texture t = (Texture)Resources.Load("EditorContent/Camera-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            DrawCameraController();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawPlayerStateBasedBobbing();

         //   DrawDefaultInspector();
        }

        void DrawCameraController()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Change the below Camera Controller properties to suit your needs accordingly!", MessageType.Info);
            Camera WeaponCamera = EditorGUILayout.ObjectField("Weapon Camera", _cameraController.weaponCamera, typeof(Camera)) as Camera;

            float LookSensitivity = EditorGUILayout.Slider("Look Sensitivity", _cameraController.lookSensitivity, 1f, 15f);
            float LookSmooth = EditorGUILayout.Slider("Look Smoothing", _cameraController.lookSmoth, 0.05f, 1f);


            float minAngle = EditorGUILayout.Slider("Minimum Angle", _cameraController.MinMaxAngle.x, -360, 360);
            float maxAngle = EditorGUILayout.Slider("Maximum Angle", _cameraController.MinMaxAngle.y, -360, 360);

            EditorGUILayout.BeginVertical("Box");
            bool hideCursor = EditorGUILayout.Toggle("Hide Cursor", _cameraController.hideCursor);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "camera controller");

                _cameraController.hideCursor = hideCursor;
                _cameraController.lookSensitivity = LookSensitivity;
                _cameraController.lookSmoth = LookSmooth;
                _cameraController.weaponCamera = WeaponCamera;
                _cameraController.MinMaxAngle.x = minAngle;
                _cameraController.MinMaxAngle.y = maxAngle;

            }
        }

        void DrawPlayerStateBasedBobbing()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Camera Headbob states based on the Current Player State : \n" +
                                    "> Idle  \n" +
                                    "> Crouching  \n" +
                                    "> Walking  \n" +
                                    "> Running ", MessageType.Info);

            EditorGUILayout.EndVertical();

            string s1 = _showBobProperties ? "Hide" : "Show";
            _showBobProperties = GUILayout.Toggle(_showBobProperties, s1, "Button");

            if (!_showBobProperties)
                return;

            EditorGUILayout.BeginVertical("box");

            bool enableBobbing = EditorGUILayout.Toggle("Enable Bobbing", _cameraController._cameraHeadBob.enableBobbing);
            AnimationCurve BobCurve = EditorGUILayout.CurveField("Bob Curve", _cameraController._cameraHeadBob.animationCurve);

            EditorGUILayout.EndVertical();

            

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GetIdleBobbingState();

            EditorGUILayout.Space();

            GetCrouchingBobbingState();

            EditorGUILayout.Space();

            GetWalkingBobbingState();

            EditorGUILayout.Space();

            GetRunningBobbingState();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "CameraBobbing");

                _cameraController._cameraHeadBob.enableBobbing = enableBobbing;
                _cameraController._cameraHeadBob.animationCurve = BobCurve;
            }

        }

        void GetIdleBobbingState()
        {

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Idle", EditorStyles.centeredGreyMiniLabel);

            float HorizontalBob = 0;
            float VerticalBob = 0;
            float speed = 0;
            float Interval = 0;


            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _cameraController._cameraHeadBob.bobStates[3].horizontalFactor, 0.01f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _cameraController._cameraHeadBob.bobStates[3].verticalFactor, 0.01f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _cameraController._cameraHeadBob.bobStates[3].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _cameraController._cameraHeadBob.bobStates[3].interval, 0.1f, 10f);


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "IdleBased");
                _cameraController._cameraHeadBob.bobStates[3].horizontalFactor = HorizontalBob;
                _cameraController._cameraHeadBob.bobStates[3].verticalFactor = VerticalBob;
                _cameraController._cameraHeadBob.bobStates[3].speed = speed;
                _cameraController._cameraHeadBob.bobStates[3].interval = Interval;

            }
        }

        void GetCrouchingBobbingState()
        {

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Crouching", EditorStyles.centeredGreyMiniLabel);

            float HorizontalBob = 0;
            float VerticalBob = 0;
            float speed = 0;
            float Interval = 0;
            
            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _cameraController._cameraHeadBob.bobStates[1].horizontalFactor, 0.01f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _cameraController._cameraHeadBob.bobStates[1].verticalFactor, 0.01f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _cameraController._cameraHeadBob.bobStates[1].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _cameraController._cameraHeadBob.bobStates[1].interval, 0.1f, 10f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "CrouchingBased");
                _cameraController._cameraHeadBob.bobStates[1].horizontalFactor = HorizontalBob;
                _cameraController._cameraHeadBob.bobStates[1].verticalFactor = VerticalBob;
                _cameraController._cameraHeadBob.bobStates[1].speed = speed;
                _cameraController._cameraHeadBob.bobStates[1].interval = Interval;

            }
        }

        void GetWalkingBobbingState()
        {

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Walking", EditorStyles.centeredGreyMiniLabel);

            float HorizontalBob = 0;
            float VerticalBob = 0;
            float speed = 0;
            float Interval = 0;

            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _cameraController._cameraHeadBob.bobStates[0].horizontalFactor, 0.01f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _cameraController._cameraHeadBob.bobStates[0].verticalFactor, 0.01f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _cameraController._cameraHeadBob.bobStates[0].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _cameraController._cameraHeadBob.bobStates[0].interval, 0.1f, 10f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "WalkingBased");
                _cameraController._cameraHeadBob.bobStates[0].horizontalFactor = HorizontalBob;
                _cameraController._cameraHeadBob.bobStates[0].verticalFactor = VerticalBob;
                _cameraController._cameraHeadBob.bobStates[0].speed = speed;
                _cameraController._cameraHeadBob.bobStates[0].interval = Interval;

            }
        }

        void GetRunningBobbingState()
        {

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Running", EditorStyles.centeredGreyMiniLabel);

            float HorizontalBob = 0;
            float VerticalBob = 0;
            float speed = 0;
            float Interval = 0;

            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _cameraController._cameraHeadBob.bobStates[2].horizontalFactor, 0.01f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _cameraController._cameraHeadBob.bobStates[2].verticalFactor, 0.01f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _cameraController._cameraHeadBob.bobStates[2].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _cameraController._cameraHeadBob.bobStates[2].interval, 0.1f, 10f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "RunningBased");
                _cameraController._cameraHeadBob.bobStates[2].horizontalFactor = HorizontalBob;
                _cameraController._cameraHeadBob.bobStates[2].verticalFactor = VerticalBob;
                _cameraController._cameraHeadBob.bobStates[2].speed = speed;
                _cameraController._cameraHeadBob.bobStates[2].interval = Interval;

            }
        }

    }
}