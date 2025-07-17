using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(WeaponProceduralManoeuvre))]
    public class WeaponProceduralManoeuvreEditor : Editor
    {
        WeaponProceduralManoeuvre _wpm;
        WeaponType weaponType;

        private void OnEnable()
        {
             _wpm = (WeaponProceduralManoeuvre)target;
        }

        public override void OnInspectorGUI()
        {
            Texture t = (Texture) Resources.Load("EditorContent/WeaponProceduralManoeuvre-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));


            //Draw Debug Toggle
            DrawDebugToggle();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawTabs();

            DrawDefaultInspector();
        }

        void DrawDebugToggle()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", _wpm.weaponType);

            EditorGUILayout.HelpBox("If true, Weapon's Transform will enter into debug mode!", MessageType.Info);
            bool debugTransform = EditorGUILayout.Toggle("Debug Transform", _wpm.DebugTransform);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "DebugToggle");

                _wpm.DebugTransform = debugTransform;
                _wpm.weaponType = weaponType;

            }
        }

        void DrawTabs()
        {
            switch (weaponType)
            {
                case WeaponType.Shooter:
                    DrawShooterWeaponTabs();
                    break;

                case WeaponType.Melee:
                    DrawMeleeWeaponTabs();
                    break;
            }

            
        }

        void DrawShooterWeaponTabs()
        {
            _wpm.TabCount = GUILayout.Toolbar(_wpm.TabCount, new string[] { "Sway", "Bobbing", "Reload", "Equip" });

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            switch (_wpm.TabCount)
            {
                //Weapon Sway
                case 0:
                    DrawWeaponSway();
                    break;

                //Weapon Bobbing
                case 1:
                    DrawWeaponBobbing();
                    break;

                //Weapon Reload
                case 2:
                    DrawWeaponReload();
                    break;

                //Weapon Equip Sway
                case 3:
                    DrawEquipSway();
                    break;

            }
        }

        void DrawMeleeWeaponTabs()
        {
            _wpm.TabCount = GUILayout.Toolbar(_wpm.TabCount, new string[] { "Sway", "Bobbing", "Equip" });

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            switch (_wpm.TabCount)
            {
                //Weapon Sway
                case 0:
                    DrawWeaponSway();
                    break;

                //Weapon Bobbing
                case 1:
                    DrawWeaponBobbing();
                    break;

                //Weapon Equip Sway
                case 2:
                    DrawEquipSway();
                    break;

            }
        }

        void DrawWeaponSway()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Horizontal Axis used for mouse movement.", EditorStyles.helpBox);
            string MouseX = EditorGUILayout.TextField("Mouse X-Axis", _wpm._weaponSway.MouseX);

            EditorGUILayout.LabelField("Vertical Axis used for mouse movement.", EditorStyles.helpBox);
            string MouseY = EditorGUILayout.TextField("Mouse Y-Axis", _wpm._weaponSway.MouseY);

            EditorGUILayout.LabelField("How much movement is allowed in swaying?", EditorStyles.helpBox);
            float moveAmount = EditorGUILayout.Slider("Move Amount", _wpm._weaponSway.moveAmount, 0.1f, 5);

            EditorGUILayout.LabelField("At what speed the weapon sway back and forth?", EditorStyles.helpBox);
            float moveSpeed = EditorGUILayout.Slider("Move Speed", _wpm._weaponSway.moveSpeed , 0.1f, 5);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Weapon Sway");

                _wpm._weaponSway.MouseX = MouseX;
                _wpm._weaponSway.MouseY = MouseY;
                _wpm._weaponSway.moveAmount = moveAmount;
                 _wpm._weaponSway.moveSpeed = moveSpeed;
            }
        }

        void DrawWeaponBobbing()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            AnimationType _weaponBobType = (AnimationType)EditorGUILayout.EnumPopup("Bobbing Type", _wpm._weaponBobType);
            bool enableBobbing = _wpm._weaponBob.enableBobbing;
            AnimationCurve BobCurve = _wpm._weaponBob.animationCurve;
            Animation WeaponAnimation = _wpm._animatedBob.WeaponAnimation;

            string IdleAnimation = _wpm._animatedBob.IdleAnimation;
            float IdleAnimationSpeed = _wpm._animatedBob.IdleAnimationSpeed;

            string WalkAnimation = _wpm._animatedBob.WalkAnimation;
            float WalkAnimationSpeed = _wpm._animatedBob.WalkAnimationSpeed;

            string RunAnimation = _wpm._animatedBob.RunAnimation;
            float RunAnimationSpeed = _wpm._animatedBob.RunAnimationSpeed;

            string CrouchAnimation = _wpm._animatedBob.CrouchAnimation;
            float CrouchAnimationSpeed = _wpm._animatedBob.CrouchAnimationSpeed;


            if (_weaponBobType == AnimationType.Procedural)
            {
                EditorGUILayout.HelpBox("Weapon Headbob states based on the Current Player State : \n" +
                                    "> Idle  \n" +
                                    "> Crouching  \n" +
                                    "> Walking  \n" +
                                    "> Running ", MessageType.Info);

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");

                enableBobbing = EditorGUILayout.Toggle("Enable Bobbing", _wpm._weaponBob.enableBobbing);
                BobCurve = EditorGUILayout.CurveField("Bob Curve", _wpm._weaponBob.animationCurve);

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

            }
            else if(_weaponBobType == AnimationType.Animation)
            {
                EditorGUILayout.HelpBox("Drag the Weapon Animation Component. Please note that you need to have at least one Idle Animation, otherwise use procedural Animations", MessageType.Info);

                WeaponAnimation =  EditorGUILayout.ObjectField("Weapon Animation", _wpm._animatedBob.WeaponAnimation, typeof(Animation)) as Animation;

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Idle Animation Settings (Required!)", EditorStyles.centeredGreyMiniLabel);

                IdleAnimation = EditorGUILayout.TextField("Idle Animation", _wpm._animatedBob.IdleAnimation);
                IdleAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", _wpm._animatedBob.IdleAnimationSpeed);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Walk Animation Settings", EditorStyles.centeredGreyMiniLabel);

                WalkAnimation = EditorGUILayout.TextField("Walk Animation", _wpm._animatedBob.WalkAnimation);
                WalkAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", _wpm._animatedBob.WalkAnimationSpeed);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Run Animation Settings", EditorStyles.centeredGreyMiniLabel);

                RunAnimation = EditorGUILayout.TextField("Run Animation", _wpm._animatedBob.RunAnimation);
                RunAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", _wpm._animatedBob.RunAnimationSpeed);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Crouch Animation Settings", EditorStyles.centeredGreyMiniLabel);

                CrouchAnimation = EditorGUILayout.TextField("Crouch Animation", _wpm._animatedBob.CrouchAnimation);
                CrouchAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", _wpm._animatedBob.CrouchAnimationSpeed);

                EditorGUILayout.EndVertical();

            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "WeaponBobbing");

                _wpm._weaponBobType = _weaponBobType;

                _wpm._weaponBob.enableBobbing = enableBobbing;
                _wpm._weaponBob.animationCurve = BobCurve;

                _wpm._animatedBob.WeaponAnimation = WeaponAnimation;

                _wpm._animatedBob.IdleAnimation = IdleAnimation;
                _wpm._animatedBob.IdleAnimationSpeed = IdleAnimationSpeed;

                _wpm._animatedBob.WalkAnimation = WalkAnimation;
                _wpm._animatedBob.WalkAnimationSpeed = WalkAnimationSpeed;

                _wpm._animatedBob.RunAnimation = RunAnimation;
                _wpm._animatedBob.RunAnimationSpeed = RunAnimationSpeed;

                _wpm._animatedBob.CrouchAnimation = CrouchAnimation;
                _wpm._animatedBob.CrouchAnimationSpeed = CrouchAnimationSpeed;

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


            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _wpm._weaponBob.bobStates[0].horizontalFactor, 0.001f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _wpm._weaponBob.bobStates[0].verticalFactor, 0.001f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _wpm._weaponBob.bobStates[0].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _wpm._weaponBob.bobStates[0].interval, 0.1f, 10f);


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "IdleBased");
                _wpm._weaponBob.bobStates[0].horizontalFactor = HorizontalBob;
                _wpm._weaponBob.bobStates[0].verticalFactor = VerticalBob;
                _wpm._weaponBob.bobStates[0].speed = speed;
                _wpm._weaponBob.bobStates[0].interval = Interval;

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

            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _wpm._weaponBob.bobStates[3].horizontalFactor, 0.001f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _wpm._weaponBob.bobStates[3].verticalFactor, 0.001f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _wpm._weaponBob.bobStates[3].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _wpm._weaponBob.bobStates[3].interval, 0.1f, 10f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "CrouchingBased");
                _wpm._weaponBob.bobStates[3].horizontalFactor = HorizontalBob;
                _wpm._weaponBob.bobStates[3].verticalFactor = VerticalBob;
                _wpm._weaponBob.bobStates[3].speed = speed;
                _wpm._weaponBob.bobStates[3].interval = Interval;

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

            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _wpm._weaponBob.bobStates[1].horizontalFactor, 0.001f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _wpm._weaponBob.bobStates[1].verticalFactor, 0.001f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _wpm._weaponBob.bobStates[1].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _wpm._weaponBob.bobStates[1].interval, 0.1f, 10f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "WalkingBased");
                _wpm._weaponBob.bobStates[1].horizontalFactor = HorizontalBob;
                _wpm._weaponBob.bobStates[1].verticalFactor = VerticalBob;
                _wpm._weaponBob.bobStates[1].speed = speed;
                _wpm._weaponBob.bobStates[1].interval = Interval;

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

            HorizontalBob = EditorGUILayout.Slider("Horizontal Bob", _wpm._weaponBob.bobStates[2].horizontalFactor, 0.001f, 0.1f);
            VerticalBob = EditorGUILayout.Slider("Vertical Bob", _wpm._weaponBob.bobStates[2].verticalFactor, 0.001f, 0.1f);
            speed = EditorGUILayout.Slider("Speed", _wpm._weaponBob.bobStates[2].speed, 0.1f, 15f);
            Interval = EditorGUILayout.Slider("Interval", _wpm._weaponBob.bobStates[2].interval, 0.1f, 10f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "RunningBased");
                _wpm._weaponBob.bobStates[2].horizontalFactor = HorizontalBob;
                _wpm._weaponBob.bobStates[2].verticalFactor = VerticalBob;
                _wpm._weaponBob.bobStates[2].speed = speed;
                _wpm._weaponBob.bobStates[2].interval = Interval;

            }
        }

        void DrawWeaponReload()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Do you want to use reload animation or procedural reload.", MessageType.Info);
            AnimationType _ReloadType = (AnimationType) EditorGUILayout.EnumPopup("Reload Type", _wpm._weaponReload._ReloadType);
            EditorGUILayout.EndVertical();

            Vector3 ReloadPositionOffset = Vector3.zero;
            Vector3 ReloadRotationOffset = Vector3.zero;
            float ReloadDuration = 0;
            Animation weaponAnimation = null;
            string reloadAnimationName = "Reload";

            if (_ReloadType == AnimationType.Procedural)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Weapon's Position offset while reloading.", MessageType.Info);
                ReloadPositionOffset = EditorGUILayout.Vector3Field("Position Reload Offset", _wpm._weaponReload.ReloadPositionOffset);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Weapon's Rotation offset while reloading.", MessageType.Info);
                ReloadRotationOffset = EditorGUILayout.Vector3Field("Rotation Reload Offset", _wpm._weaponReload.ReloadRotationOffset);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Weapon's Total Reload Duration.", MessageType.Info);
                ReloadDuration = EditorGUILayout.Slider("Reload Duration", _wpm._weaponReload.reloadDuration, 0.1f, 5f);
                EditorGUILayout.EndVertical();

            }else if(_ReloadType == AnimationType.Animation)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.HelpBox("Weapon's Animation Component.", MessageType.Info);
                weaponAnimation = (Animation) EditorGUILayout.ObjectField("Reload Animation Name", _wpm._weaponReload.weaponAnimation, typeof(Animation));


                EditorGUILayout.HelpBox("Weapon's Reload Animation name.", MessageType.Info);
                reloadAnimationName = EditorGUILayout.TextField("Reload Animation Name", _wpm._weaponReload.reloadAnimationName);

                EditorGUILayout.EndVertical();

            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Weapon Reload");

                _wpm._weaponReload._ReloadType = _ReloadType;
                _wpm._weaponReload.ReloadPositionOffset = ReloadPositionOffset;
                _wpm._weaponReload.ReloadRotationOffset = ReloadRotationOffset;
                _wpm._weaponReload.reloadDuration = ReloadDuration;
                _wpm._weaponReload.weaponAnimation = weaponAnimation;
                _wpm._weaponReload.reloadAnimationName = reloadAnimationName;
            }
        }

        void DrawEquipSway()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Weapon's Position offset while Equipping / Un - equipping.", MessageType.Info);
            Vector3 equipPositionOffset = EditorGUILayout.Vector3Field("Equip Position Offset", _wpm._equipSway.equipPositionOffset);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Weapon's Rotation offset while Equipping / Un - equipping.", MessageType.Info);
            Vector3 equipRotationOffset = EditorGUILayout.Vector3Field("Equip Rotation Offset", _wpm._equipSway.equipRotationOffset);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Weapon's Total Equipping / Un - equipping Duration.", MessageType.Info);
            float equipDuration = EditorGUILayout.Slider("Equip Duration", _wpm._equipSway.equipDuration, 0.1f, 1f);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "EquipSway");

                _wpm._equipSway.equipPositionOffset = equipPositionOffset;
                _wpm._equipSway.equipRotationOffset = equipRotationOffset;
                _wpm._equipSway.equipDuration = equipDuration;
            }

        }

    }
}