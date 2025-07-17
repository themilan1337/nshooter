using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(Inventory))]
    public class InventoryEditor : Editor
    {
        Inventory _i;

        private void OnEnable()
        {
            _i = (Inventory) target;
        }
        
        public override void OnInspectorGUI()
        {
            Texture t = (Texture)Resources.Load("EditorContent/Inventory-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Inventory class which will hold all the items (bWeapon, Powerups, Throwables) which are " +
                "with the Player with respective quantity. ", MessageType.Info);

            GameObject slotPrefab_Weapons = (GameObject)EditorGUILayout.ObjectField("Weapon's Slot Prefab", _i._InventoryUI.slotPrefab_Weapons, typeof(GameObject));
            GameObject slotPrefab_Powerups = (GameObject)EditorGUILayout.ObjectField("Powerup's Slot Prefab", _i._InventoryUI.slotPrefab_Powerups, typeof(GameObject));
            GameObject slotPrefab_Throwables = (GameObject)EditorGUILayout.ObjectField("Throwable's Slot Prefab", _i._InventoryUI.slotPrefab_Throwables, typeof(GameObject));

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Pauses the game while the inventory is Open", EditorStyles.helpBox);
            bool PauseGameWhileOpen = EditorGUILayout.Toggle("Pause Game While Open", _i.PauseGameWhileOpen);
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            GUILayout.Label("All Weapons : " + _i._InventoryUI.allSlots_Weapons.Count  , EditorStyles.centeredGreyMiniLabel) ;

            for(int i = 0; i<_i._InventoryUI.allSlots_Weapons.Count; i++)
            {

                EditorGUILayout.BeginHorizontal("box");

                GUILayout.Label(i.ToString(), EditorStyles.helpBox);
                GameObject slot_Weapons = (GameObject)EditorGUILayout.ObjectField(_i._InventoryUI.allSlots_Weapons[i], typeof(GameObject));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("All Powerups : " + _i._InventoryUI.allSlots_Powerups.Count, EditorStyles.centeredGreyMiniLabel);

            for (int i = 0; i < _i._InventoryUI.allSlots_Powerups.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("box");

                GUILayout.Label(i.ToString(), EditorStyles.helpBox);
                GameObject slot_Weapons = (GameObject)EditorGUILayout.ObjectField(_i._InventoryUI.allSlots_Powerups[i], typeof(GameObject));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("All Throwables : " + _i._InventoryUI.allSlots_Throwables.Count, EditorStyles.centeredGreyMiniLabel);

            for (int i = 0; i < _i._InventoryUI.allSlots_Throwables.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("box");

                GUILayout.Label(i.ToString(), EditorStyles.helpBox);
                GameObject slot_Weapons = (GameObject)EditorGUILayout.ObjectField(_i._InventoryUI.allSlots_Throwables[i], typeof(GameObject));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "INVENTORY Slots");

                _i.PauseGameWhileOpen = PauseGameWhileOpen;
                _i._InventoryUI.slotPrefab_Weapons = slotPrefab_Weapons;
                _i._InventoryUI.slotPrefab_Powerups = slotPrefab_Powerups;
                _i._InventoryUI.slotPrefab_Throwables = slotPrefab_Throwables;

            }

            EditorGUILayout.EndVertical();

           // base.OnInspectorGUI();

        }
    }
}