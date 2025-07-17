using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Manoeuvre
{
    [CustomEditor(typeof(WeaponHandler))]
    public class WeaponHandlerEditor : Editor
    {
        //weapon handler
        WeaponHandler _weaponHandler;

        private void OnEnable()
        {
            _weaponHandler = (WeaponHandler) target;
        }

        public override void OnInspectorGUI()
        {

            Texture t = (Texture)Resources.Load("EditorContent/Handler-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.HelpBox("Add Weapon to Weapon Handler", MessageType.Info);


            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Total Weapons : " + _weaponHandler.Weapons.Count, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                AddElement();
            }

            if (GUILayout.Button("Clear"))
            {
                _weaponHandler.Weapons.Clear();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if(_weaponHandler.Weapons.Count > 0)
                EditorGUILayout.BeginVertical("window");

            for(int i = 0; i< _weaponHandler.Weapons.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField(i.ToString(), EditorStyles.helpBox);

                _weaponHandler.Weapons[i].WeaponObject = (Transform) EditorGUILayout.ObjectField(_weaponHandler.Weapons[i].WeaponObject, typeof(Transform));

                if (GUILayout.Button("X"))
                {
                    RemoveElement(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                WeaponType weaponType = (WeaponType) EditorGUILayout.EnumPopup("Weapon Type", _weaponHandler.Weapons[i].WeaponType);
                string WeaponName = EditorGUILayout.TextField("Weapon Name", _weaponHandler.Weapons[i].WeaponName);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Ammo", EditorStyles.helpBox);
                _weaponHandler.Weapons[i].Ammo = EditorGUILayout.IntField(_weaponHandler.Weapons[i].Ammo, EditorStyles.helpBox);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Capacity", EditorStyles.helpBox);
                _weaponHandler.Weapons[i].Capacity = EditorGUILayout.IntField(_weaponHandler.Weapons[i].Capacity, EditorStyles.helpBox);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("UI Icon", EditorStyles.helpBox);
                _weaponHandler.Weapons[i].UIIcon =  (Sprite) EditorGUILayout.ObjectField(_weaponHandler.Weapons[i].UIIcon, typeof(Sprite));

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Weapon Handler");
                    _weaponHandler.Weapons[i].WeaponType = weaponType;
                    _weaponHandler.Weapons[i].WeaponName = WeaponName;
                }

            }

            if (_weaponHandler.Weapons.Count > 0)
            {

                EditorGUILayout.EndVertical();

                EditorGUILayout.HelpBox("These weapons will be present with the Player As soon as Game Starts.", MessageType.Info);

            }


            //DrawDefaultInspector();

        }

        void AddElement()
        {
            _weaponHandler.Weapons.Add(new WeaponClassification());
        }

        void RemoveElement(int index)
        {
            _weaponHandler.Weapons.RemoveAt(index);
        }
    }
}