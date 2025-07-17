using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(Weapon_Pickup))]
    public class Weapon_PickupEditor : Editor
    {
        Weapon_Pickup _wp;
        private void OnEnable()
        {
            _wp = (Weapon_Pickup) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            Texture t = (Texture)Resources.Load("EditorContent/Pickups-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("box");

            int Ammo = 0;
            bool equipOnPickup = false;
            Transform WeaponObject = null;
            int Capacity = 0;
            Sprite UIIcon = null;
            GameObject PickupTextPrefab = null;

            EditorGUILayout.HelpBox("Handy On Pickup event in case you want to trigger someething when player picks up this item.", MessageType.Info);

            DrawDefaultInspector();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");

            if (FindWeapon())
            {
                if (WeaponObject)
                {
                    if (WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                        EditorGUILayout.HelpBox("This Weapon is assigned in the Weapon Handler, hence Ammo (" + _wp.Ammo + ") will be added " +
                        "to this Weapon On Pick Up!", MessageType.Info);
                    else
                        EditorGUILayout.HelpBox("This Weapon is assigned in the Weapon Handler", MessageType.Info);
                }
                WeaponObject = (Transform)EditorGUILayout.ObjectField("Weapon Object", _wp.WeaponObject, typeof(Transform));

                if(WeaponObject)
                {
                    if(WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                         Ammo = EditorGUILayout.IntField("Ammo", _wp.Ammo);
                }

                PickupTextPrefab = (GameObject)EditorGUILayout.ObjectField("Pickup Text Prefab", _wp.PickupTextPrefab, typeof(GameObject));

                equipOnPickup = EditorGUILayout.Toggle("equipOnPickup", _wp.equipOnPickup);

                _wp.pickupSound = (AudioClip)EditorGUILayout.ObjectField("Pickup Sound", _wp.pickupSound, typeof(AudioClip));

            }
            else
            {
                if (WeaponObject)
                {
                    if (WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                        EditorGUILayout.HelpBox("This Weapon is not assigned in the Weapon Handler, hence Ammo (" + _wp.Ammo + ") will be given " +
                        "to this Weapon On Pick Up! \n" +
                        "Also, this weapon will be having the Capacity of " + _wp.Capacity, MessageType.Info);
                    else
                        EditorGUILayout.HelpBox("This Weapon is not assigned in the Weapon Handler", MessageType.Info);
                }
                 WeaponObject = (Transform)EditorGUILayout.ObjectField("Weapon Object", _wp.WeaponObject, typeof(Transform));

                if (WeaponObject)
                {
                    if (WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    {
                        Ammo = EditorGUILayout.IntField("Ammo", _wp.Ammo);
                        Capacity = EditorGUILayout.IntField("Capacity", _wp.Capacity);
                    }
                }
                PickupTextPrefab = (GameObject) EditorGUILayout.ObjectField("Pickup Text Prefab", _wp.PickupTextPrefab, typeof(GameObject));
                 equipOnPickup = EditorGUILayout.Toggle("Equip On Pickup", _wp.equipOnPickup);
                _wp.pickupSound = (AudioClip)EditorGUILayout.ObjectField("Pickup Sound", _wp.pickupSound, typeof(AudioClip));

                if (!_wp.UIIcon)
                    EditorGUILayout.HelpBox("Please Assign an Icon for this Weapon to Display in HUD and Inventory.", MessageType.Error);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UI Icon", GUILayout.Width(100));
                UIIcon = (Sprite)EditorGUILayout.ObjectField(_wp.UIIcon, typeof(Sprite));
                EditorGUILayout.EndHorizontal();


            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Weapon_pickup");

                _wp.WeaponObject = WeaponObject;
                _wp.equipOnPickup = equipOnPickup;
                _wp.Ammo = Ammo;
                _wp.Capacity = Capacity ;
                _wp.UIIcon = UIIcon;
                _wp.PickupTextPrefab = PickupTextPrefab;
            }



        }

        bool FindWeapon()
        {
            for (int i = 0; i < FindObjectOfType<WeaponHandler>().Weapons.Count; i++)
            {
                //if we have this pickup in the Handler
                if (FindObjectOfType<WeaponHandler>().Weapons[i].WeaponObject == _wp.WeaponObject)
                {
                    return true;
                }

            }

            return false;
        }
    }
}