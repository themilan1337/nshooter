using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(Powerups_Pickup))]
    public class Powerups_PickupEditor : Editor
    {
        Powerups_Pickup _Powerups_Pickup;

        private void OnEnable()
        {
            _Powerups_Pickup = (Powerups_Pickup) target;
        }

        public override void OnInspectorGUI()
        {

            Texture t = (Texture)Resources.Load("EditorContent/Pickups-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Select the power-up type and define it's respective properties!", MessageType.Info);

            EditorGUILayout.BeginVertical("Box");
            _Powerups_Pickup._PowerupType = (PowerupType) EditorGUILayout.EnumPopup("Powerup Type", _Powerups_Pickup._PowerupType);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            _Powerups_Pickup.PickupTextPrefab = (GameObject)EditorGUILayout.ObjectField("Pickup Text Prefab", _Powerups_Pickup.PickupTextPrefab, typeof(GameObject));
            _Powerups_Pickup.pickupSound = (AudioClip)EditorGUILayout.ObjectField("Pickup Sound", _Powerups_Pickup.pickupSound, typeof(AudioClip));

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginVertical("box");

            DrawPickupProperties();

            EditorGUILayout.EndVertical();


            //DrawDefaultInspector();
        }

        void DrawPickupProperties()
        {
            switch (_Powerups_Pickup._PowerupType)
            {
                case PowerupType.Healthkit:

                    if(FindObjectOfType<PowerupsManager>()._HealthKit.icon == null)
                        EditorGUILayout.HelpBox("There's no icon assigned for healthkit." +
                                                " Please assign an Icon in the Poweups Manager to remove any conflictions.", MessageType.Error);

                    EditorGUILayout.HelpBox("This much amount of health will be " +
                        "added to player's current health on consuming this powerup!",MessageType.Info );


                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Health Amount", EditorStyles.helpBox);
                    _Powerups_Pickup.healthAmount = EditorGUILayout.IntField(_Powerups_Pickup.healthAmount, EditorStyles.helpBox);

                    EditorGUILayout.EndHorizontal();
                    break;

                case PowerupType.Invincibility:

                    if (FindObjectOfType<PowerupsManager>()._Invincibility.icon == null)
                        EditorGUILayout.HelpBox("There's no icon assigned for Invincibility." +
                                                " Please assign an Icon in the Poweups Manager to remove any conflictions.", MessageType.Error);

                    EditorGUILayout.HelpBox("Player will remain invincible to attacks for duration of '" + _Powerups_Pickup.InvincibilityDuration.ToString() + " seconds' " +
                        "after consuming this powerup!", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Invincibility Duration", EditorStyles.helpBox);
                    _Powerups_Pickup.InvincibilityDuration = EditorGUILayout.FloatField(_Powerups_Pickup.InvincibilityDuration, EditorStyles.helpBox);

                    EditorGUILayout.EndHorizontal();

                    break;
                case PowerupType.Speedboost:

                    if (FindObjectOfType<PowerupsManager>()._SpeedBoost.icon == null)
                        EditorGUILayout.HelpBox("There's no icon assigned for Speedboost." +
                                                " Please assign an Icon in the Poweups Manager to remove any conflictions.", MessageType.Error);

                    EditorGUILayout.HelpBox("To the overall speeds defined under locomotion properties of the controller, " +
                        "an amount of " + _Powerups_Pickup.SpeedBoostAmount + " will be added!", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("SpeedBoost Amount", EditorStyles.helpBox);
                    _Powerups_Pickup.SpeedBoostAmount = EditorGUILayout.FloatField(_Powerups_Pickup.SpeedBoostAmount, EditorStyles.helpBox);

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.HelpBox("This increased speed will remain for duration of '" + _Powerups_Pickup.SpeedBoostDuration.ToString() + " seconds' " +
                        "after consuming this powerup!", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("SpeedBoost Duration", EditorStyles.helpBox);
                    _Powerups_Pickup.SpeedBoostDuration = EditorGUILayout.FloatField(_Powerups_Pickup.SpeedBoostDuration, EditorStyles.helpBox);

                    EditorGUILayout.EndHorizontal();

                    break;

                case PowerupType.DamageMultiplier:

                    if (FindObjectOfType<PowerupsManager>()._DamageMultiplier.icon == null)
                        EditorGUILayout.HelpBox("There's no icon assigned for DamageMultiplier." +
                                                " Please assign an Icon in the Poweups Manager to remove any conflictions.", MessageType.Error);


                    EditorGUILayout.HelpBox("To the overall damage defined under shooter properties of the weapon, " +
                        "an amount of " + _Powerups_Pickup.DamageMultiplierAmount + " will be Multiplied!", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("DamageMultiplier Amount ", EditorStyles.helpBox);
                    _Powerups_Pickup.DamageMultiplierAmount = EditorGUILayout.IntField(_Powerups_Pickup.DamageMultiplierAmount, EditorStyles.helpBox);

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.HelpBox("This increased damage will remain for duration of '" + _Powerups_Pickup.DamageMultiplierDuration.ToString() + " seconds' " +
                        "after consuming this powerup!", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("DamageMultiplier Duration", EditorStyles.helpBox);
                    _Powerups_Pickup.DamageMultiplierDuration = EditorGUILayout.FloatField(_Powerups_Pickup.DamageMultiplierDuration, EditorStyles.helpBox);

                    EditorGUILayout.EndHorizontal();

                    break;

                case PowerupType.InfiniteAmmo:

                    if (FindObjectOfType<PowerupsManager>()._InfiniteAmmo.icon == null)
                        EditorGUILayout.HelpBox("There's no icon assigned for InfiniteAmmo." +
                                                " Please assign an Icon in the Poweups Manager to remove any conflictions.", MessageType.Error);

                    EditorGUILayout.HelpBox("For duration of '" + _Powerups_Pickup.InfiniteAmmoDuration.ToString() + " seconds' " +
                        "the equipped weapon will be having infinite ammo after consuming this powerup!", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("InfiniteAmmo Duration", EditorStyles.helpBox);
                    _Powerups_Pickup.InfiniteAmmoDuration = EditorGUILayout.FloatField(_Powerups_Pickup.InfiniteAmmoDuration, EditorStyles.helpBox);

                    EditorGUILayout.EndHorizontal();

                    break;
            }
        }

    }
}