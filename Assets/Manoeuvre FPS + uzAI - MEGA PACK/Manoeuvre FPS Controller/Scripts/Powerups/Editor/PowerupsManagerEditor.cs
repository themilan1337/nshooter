using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(PowerupsManager))]
    public class PowerupsManagerEditor : Editor
    {
        PowerupsManager _pm;

        private void OnEnable()
        {
            _pm = (PowerupsManager) target;
        }

        public override void OnInspectorGUI()
        {

            EditorGUI.BeginChangeCheck();

            Texture t = (Texture)Resources.Load("EditorContent/Powerups-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Assign HUD Prefab", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
            GameObject PowerupsPrefab = (GameObject)EditorGUILayout.ObjectField("Powerups Prefab", _pm.PowerupsHUDPrefab, typeof(GameObject));

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Assign Respective Icons", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Healthkit Icon", GUILayout.Width(100));
            Sprite UIIcon_HealthKit = (Sprite)EditorGUILayout.ObjectField(_pm._HealthKit.icon, typeof(Sprite));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Invincibility Icon", GUILayout.Width(100));
            Sprite UIIcon_Invincibility = (Sprite)EditorGUILayout.ObjectField(_pm._Invincibility.icon, typeof(Sprite));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Speed Boost Icon", GUILayout.Width(100));
            Sprite UIIcon_SpeedBoost = (Sprite)EditorGUILayout.ObjectField(_pm._SpeedBoost.icon, typeof(Sprite));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Damage Multiplier Icon", GUILayout.Width(100));
            Sprite UIIcon_DamageMultiplier = (Sprite)EditorGUILayout.ObjectField(_pm._DamageMultiplier.icon, typeof(Sprite));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Infinite Ammo Icon", GUILayout.Width(100));
            Sprite UIIcon_InfiniteAmmo = (Sprite)EditorGUILayout.ObjectField(_pm._InfiniteAmmo.icon, typeof(Sprite));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Powerups Manager");

                _pm.PowerupsHUDPrefab = PowerupsPrefab;

                _pm._HealthKit.icon = UIIcon_HealthKit;
                _pm._Invincibility.icon = UIIcon_Invincibility;
                _pm._SpeedBoost.icon = UIIcon_SpeedBoost;
                _pm._DamageMultiplier.icon = UIIcon_DamageMultiplier;
                _pm._InfiniteAmmo.icon = UIIcon_InfiniteAmmo;
            }

            //DrawDefaultInspector();
        }

    }
}