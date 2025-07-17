using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(SaveComponentsHelper))]
    public class SaveComponentsHelperEditor : Editor
    {
        SaveComponentsHelper _helper;

        private void OnEnable()
        {
            _helper = (SaveComponentsHelper)target;

        }

        public override void OnInspectorGUI()
        {
            //weapon texture
            Texture t = (Texture)Resources.Load("EditorContent/SaveHelper-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            DrawShooterWeapons();

            DrawMeleeWeapons();

            DrawThrowableWeapons();

            //DrawDefaultInspector();
        }

        void DrawShooterWeapons()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add All the Shooter Weapons. \n" +
                "It doesn't matter they are present in your weapon shooter or not", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                WeaponShooter ws = new WeaponShooter();
                _helper.ShooterWeapons.Add(ws);
            }

            if (GUILayout.Button("Clear"))
            {
                _helper.ShooterWeapons.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0;i < _helper.ShooterWeapons.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                WeaponShooter _ws = (WeaponShooter) EditorGUILayout.ObjectField(_helper.ShooterWeapons[i], typeof(WeaponShooter));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _helper.ShooterWeapons.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Helper SW");
                    _helper.ShooterWeapons[i] = _ws;
                }
            }

            EditorGUILayout.EndVertical();

        }

        void DrawMeleeWeapons()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add All the Melee Weapons. \n" +
                "It doesn't matter they are present in your Weapon Handler or not", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                WeaponMelee wm = new WeaponMelee();
                _helper.MeleeWeapons.Add(wm);
            }

            if (GUILayout.Button("Clear"))
            {
                _helper.MeleeWeapons.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _helper.MeleeWeapons.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                WeaponMelee _wm = (WeaponMelee)EditorGUILayout.ObjectField(_helper.MeleeWeapons[i], typeof(WeaponMelee));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _helper.MeleeWeapons.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Helper MW");
                    _helper.MeleeWeapons[i] = _wm;
                }
            }

            EditorGUILayout.EndVertical();

        }

        void DrawThrowableWeapons()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add All the Throwable Weapons. \n" +
                "It doesn't matter they are present in your Throwable Handler or not", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                WeaponThrowable ti = new WeaponThrowable();
                _helper.ThrowableItems.Add(ti);
            }

            if (GUILayout.Button("Clear"))
            {
                _helper.ThrowableItems.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _helper.ThrowableItems.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                WeaponThrowable _ti = (WeaponThrowable)EditorGUILayout.ObjectField(_helper.ThrowableItems[i], typeof(WeaponThrowable));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _helper.ThrowableItems.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Helper TI");
                    _helper.ThrowableItems[i] = _ti;
                }
            }

            EditorGUILayout.EndVertical();

        }

    }
}