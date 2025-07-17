using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(WeaponMelee))]
    public class WeaponMeleeEditor : Editor
    {
        WeaponMelee _wm;

        private void OnEnable()
        {
            _wm = (WeaponMelee)target;

            _wm._MeleeAttackRange._mainCamera = Camera.main;
        }

        public override void OnInspectorGUI()
        {

            //weapon texture
            Texture t = (Texture)Resources.Load("EditorContent/Melee-icon");

            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawDefaultInspector();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawPropertyTabs();
        }

        void DrawPropertyTabs()
        {
            _wm.tabCount = GUILayout.Toolbar(_wm.tabCount, new string[] {"Attack", "Animations", "Range", "Hit Particles" });

            switch (_wm.tabCount)
            {
                case 0:

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    DrawAttackProperties();
                    break;

                case 01:
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    DrawAnimationsProperties();
                    break;

                case 02:
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    DrawRangeProperties();
                    break;

                case 03:
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    DrawHitParticlesProperties();
                    break;
            }

        }

        void DrawAttackProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Random between max and min Damage will be given each time Player Attacks", EditorStyles.helpBox);

            int maxDamage = EditorGUILayout.IntSlider("Maximum Damage", _wm._MeleeAttack.maxDamage, 1, 100);
            int minDamage = EditorGUILayout.IntSlider("Minimum Damage", _wm._MeleeAttack.minDamage, 1, 100);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "WM_Attack Properties");

                _wm._MeleeAttack.maxDamage = maxDamage;
                _wm._MeleeAttack.minDamage = minDamage;

            }
        }

        void DrawAnimationsProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Animation component of the weapon", EditorStyles.helpBox);
            Animation _weaponAnimation = (Animation)EditorGUILayout.ObjectField("Weapon Animation", _wm._MeleeAnimations._weaponAnimation, typeof(Animation));

            EditorGUILayout.LabelField("If true, Random attack animation will be played each time, else they will be played in order they are added.", EditorStyles.helpBox);
            bool playRandom = EditorGUILayout.Toggle("Play Random", _wm._MeleeAnimations.playRandom);

            EditorGUILayout.EndVertical();

            DrawMeleeAnimationsList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "MW_ Attack Animations");

                _wm._MeleeAnimations._weaponAnimation = _weaponAnimation;
                _wm._MeleeAnimations.playRandom = playRandom;
            }
        }

        void DrawMeleeAnimationsList()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Write all the \n\t> Attack Animations Name \n\t> Assign the attack audio clips \n\t> Approximate time when they attack.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                _wm._MeleeAnimations._MeleeAnimations.Add(new AttackAnimationClassification());
            }

            if (GUILayout.Button("Clear"))
            {
                _wm._MeleeAnimations._MeleeAnimations.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i =0;i < _wm._MeleeAnimations._MeleeAnimations.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();

                string animationClip = EditorGUILayout.TextField("Animation Clip", _wm._MeleeAnimations._MeleeAnimations[i].animationClip);

                if(GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _wm._MeleeAnimations._MeleeAnimations.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                AudioClip attackSound = (AudioClip)EditorGUILayout.ObjectField("Attack Sound", _wm._MeleeAnimations._MeleeAnimations[i].attackSound, typeof(AudioClip));
                float attackStartTime = EditorGUILayout.FloatField("Attack Start Time", _wm._MeleeAnimations._MeleeAnimations[i].attackStartTime);

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "WM_ Melee animations list");

                    _wm._MeleeAnimations._MeleeAnimations[i].animationClip = animationClip;
                    _wm._MeleeAnimations._MeleeAnimations[i].attackSound = attackSound;
                    _wm._MeleeAnimations._MeleeAnimations[i].attackStartTime = attackStartTime;
                }
            }
            EditorGUILayout.EndVertical();

        }

        void DrawRangeProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("All Layers this Weapon can hit.", EditorStyles.helpBox);
            LayerMask hitMask = LayerMaskUtility.LayerMaskField("Hit Mask", _wm._MeleeAttackRange.hitMask);

            EditorGUILayout.LabelField("All Layers this Weapon can not hit", EditorStyles.helpBox);
            LayerMask obstacleMask = LayerMaskUtility.LayerMaskField("Obstacle Mask", _wm._MeleeAttackRange.obstacleMask);

            EditorGUILayout.LabelField("From how far this weapon can hit.", EditorStyles.helpBox);
            float AttackDistance = EditorGUILayout.Slider("Attack Distance", _wm._MeleeAttackRange.AttackDistance, 0, 15);

            EditorGUILayout.LabelField("Targets can only be hit if they enter this Angle.", EditorStyles.helpBox);
            float Angle = EditorGUILayout.Slider("Angle", _wm._MeleeAttackRange.Angle, 0, 360);
            EditorGUILayout.EndVertical();

            DrawHitTagsList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "WM_ Melee Range");

                _wm._MeleeAttackRange.hitMask = hitMask;
                _wm._MeleeAttackRange.obstacleMask = obstacleMask;
                _wm._MeleeAttackRange.AttackDistance = AttackDistance;
                _wm._MeleeAttackRange.Angle = Angle;
            }
        }

        void DrawHitTagsList()
        {

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("These Tags will be identified as targets that this weapon can hit.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                string s = "";
                _wm._MeleeAttackRange.hitTags.Add(s);
            }

            if (GUILayout.Button("Clear"))
            {
                _wm._MeleeAttackRange.hitTags.Clear();
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i< _wm._MeleeAttackRange.hitTags.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField((i+1).ToString(), GUILayout.Width(15));
                string hitTags = EditorGUILayout.TextField(_wm._MeleeAttackRange.hitTags[i]);

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _wm._MeleeAttackRange.hitTags.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck()) {

                    Undo.RecordObject(target, "WM_ HiTags List");

                    _wm._MeleeAttackRange.hitTags[i] = hitTags;

                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawHitParticlesProperties()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add the Respective Particle FX for the Tag. This Particle FX will be seen when this weapon hit this Tag.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                _wm._MeleeHitParticles.ParticleFX.Add(new HitParticle());
            }

            if (GUILayout.Button("Clear"))
            {
                _wm._MeleeHitParticles.ParticleFX.Clear();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            for (int i =0;i<_wm._MeleeHitParticles.ParticleFX.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();
                string Tag = EditorGUILayout.TextField("Tag", _wm._MeleeHitParticles.ParticleFX[i].Tag);

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35))) {

                    _wm._MeleeHitParticles.ParticleFX.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                ParticleSystem _particle = (ParticleSystem)EditorGUILayout.ObjectField("Particle", _wm._MeleeHitParticles.ParticleFX[i]._particle, typeof(ParticleSystem));

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "WM_ Particle FX");

                    _wm._MeleeHitParticles.ParticleFX[i].Tag = Tag;
                    _wm._MeleeHitParticles.ParticleFX[i]._particle = _particle;

                }
            }
        }

        private void OnSceneGUI()
        {
            DrawAttackGizmos();
        }

        void DrawAttackGizmos()
        {
            MeleeAttackFOVProperties _r = _wm._MeleeAttackRange;

            Handles.color = Color.red;
            Handles.DrawWireArc(_wm._MeleeAttackRange._mainCamera.transform.position, Vector3.up, _wm._MeleeAttackRange._mainCamera.transform.forward, 360, _r.AttackDistance);

            Color c = Color.red;
            c.a = 0.35f;
            Handles.color = c;
            Handles.DrawSolidArc(_wm._MeleeAttackRange._mainCamera.transform.position, _wm._MeleeAttackRange._mainCamera.transform.up, _wm._MeleeAttackRange._mainCamera.transform.forward, _r.Angle / 2, _r.AttackDistance);
            Handles.DrawSolidArc(_wm._MeleeAttackRange._mainCamera.transform.position, _wm._MeleeAttackRange._mainCamera.transform.up, _wm._MeleeAttackRange._mainCamera.transform.forward, -_r.Angle / 2, _r.AttackDistance);

            Vector3 AngleA = _r.DirFromAngle(-_r.Angle / 2, false, _wm._MeleeAttackRange._mainCamera);
            Vector3 AngleB = _r.DirFromAngle(_r.Angle / 2, false, _wm._MeleeAttackRange._mainCamera);

            Handles.color = Color.green;
            Handles.DrawLine(_wm._MeleeAttackRange._mainCamera.transform.position, _wm._MeleeAttackRange._mainCamera.transform.position + AngleA * _r.AttackDistance);
            Handles.DrawLine(_wm._MeleeAttackRange._mainCamera.transform.position, _wm._MeleeAttackRange._mainCamera.transform.position + AngleB * _r.AttackDistance);

            foreach (Transform t in _r.visibleTargets)
            {
                Handles.DrawLine(_wm._MeleeAttackRange._mainCamera.transform.position, t.position);
            }
        }
    }
}