using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(ThrowableItem))]
    public class ThrowableItemEditor : Editor
    {
        ThrowableItem _ti;
        SerializedObject _SO_ti;
        SerializedProperty OnDetonateEvent;

        private void OnEnable()
        {
            _ti = (ThrowableItem)target;

            _SO_ti = new SerializedObject(_ti);

            OnDetonateEvent = _SO_ti.FindProperty("OnDetonateEvent");
        }


        public override void OnInspectorGUI()
        {

            DrawNewInspector();

            //DrawDefaultInspector();
        }

        void DrawNewInspector()
        {
            //weapon texture
            Texture t = (Texture)Resources.Load("EditorContent/ThrowableItem-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Item Name - This is the Unique Identifier, hence, every item should have different item name!", EditorStyles.helpBox);
            string ItemName = EditorGUILayout.TextField("Item Name", _ti.ItemName);

            EditorGUILayout.LabelField("All the colliders within this range will be affected when this item will be detonated!", EditorStyles.helpBox);
            float affectRadius = EditorGUILayout.FloatField("Affect Radius", _ti.affectRadius);
            affectRadius = Mathf.Clamp(affectRadius, 0.1f, affectRadius);

            EditorGUILayout.LabelField("What this item can affect should be from hit layers defined below!", EditorStyles.helpBox);
            LayerMask _hitMask = LayerMaskUtility.LayerMaskField("Hit Mask", _ti._hitMask);
            EditorGUILayout.LabelField("What this item can NOT affect should be from Obstacle layers defined below!", EditorStyles.helpBox);
            LayerMask _obstacleMask = LayerMaskUtility.LayerMaskField("Obstacle Mask", _ti._obstacleMask);

            EditorGUILayout.LabelField("If true, it will stick to any object on Collision!", EditorStyles.helpBox);
            bool isSticky = EditorGUILayout.Toggle("Is Sticky", _ti.isSticky);
            EditorGUILayout.LabelField("If true, it will emit sound on collision which will attract nearby NPCs!", EditorStyles.helpBox);
            bool isSoundAttractor = EditorGUILayout.Toggle("Is Sound Attractor", _ti.isSoundAttractor);
            AudioClip AttractSoundSFX = _ti.AttractSoundSFX;

            if (isSoundAttractor)
            {
                AttractSoundSFX = EditorGUILayout.ObjectField("Attract Sound SFX", _ti.AttractSoundSFX, typeof(AudioClip)) as AudioClip;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Detonate Properties", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("If true, This object will Detonate on Collision!", EditorStyles.helpBox);
            bool shouldDetonate = EditorGUILayout.Toggle("Should Detonate", _ti.shouldDetonate);
            EditorGUILayout.EndVertical();
           
            //detonate related vars
            bool destroyObjectOnDetonate = _ti.destroyObjectOnDetonate;
            bool canHitOurself = _ti.canHitOurself;
            bool KillAllNearby = _ti.KillAllNearby;
            ParticleSystem DetonateFX = _ti.DetonateFX;
            AudioClip DetonateSFX = _ti.DetonateSFX;
            float DetonateDelay = _ti.DetonateDelay;
            float forceAmtOnRigidbodies = _ti.forceAmtOnRigidbodies;
            int damage = _ti.damage;

            if (shouldDetonate)
            {
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("If true, This object will be destroyed on Detonation!", EditorStyles.helpBox);
                destroyObjectOnDetonate = EditorGUILayout.Toggle("Destroy On Detonate", _ti.destroyObjectOnDetonate);

                EditorGUILayout.LabelField("If true, On Detonation Player also gets Damage if he is in Affect Radius!", EditorStyles.helpBox);
                canHitOurself = EditorGUILayout.Toggle("Can Hit Ourself", _ti.canHitOurself);

                EditorGUILayout.LabelField("If true, directly kill all the nearby NPCs!", EditorStyles.helpBox);
                KillAllNearby = EditorGUILayout.Toggle("Kill All Nearby", _ti.KillAllNearby);

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Particle and Sound FX which will be rendered on Detonate!", EditorStyles.helpBox);
                DetonateFX = EditorGUILayout.ObjectField("Detonate FX", _ti.DetonateFX, typeof(ParticleSystem)) as ParticleSystem;
                DetonateSFX = EditorGUILayout.ObjectField("Detonate SFX", _ti.DetonateSFX, typeof(AudioClip)) as AudioClip;

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Delay applied before Detonation of this object!", EditorStyles.helpBox);
                DetonateDelay = EditorGUILayout.FloatField("Detonate Delay", _ti.DetonateDelay);

                EditorGUILayout.LabelField("Force to be applied on the nearby Rigidbodies!", EditorStyles.helpBox);
                forceAmtOnRigidbodies = EditorGUILayout.FloatField("Force Applied", _ti.forceAmtOnRigidbodies);

                if (!KillAllNearby)
                {
                    EditorGUILayout.LabelField("Damage is only applied if we are NOT Killing nearby objects!", EditorStyles.helpBox);
                    damage = EditorGUILayout.IntField("Force Applied", _ti.damage);
                }

                EditorGUILayout.EndVertical();

                //draw on detonate event
                EditorGUILayout.PropertyField(OnDetonateEvent);

                _SO_ti.ApplyModifiedProperties();

            }



            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Throwable Item Properties");

                _ti.ItemName = ItemName;
                _ti.affectRadius = affectRadius;
                _ti._hitMask = _hitMask;
                _ti._obstacleMask = _obstacleMask;
                _ti.isSticky = isSticky;
                _ti.isSoundAttractor = isSoundAttractor;
                _ti.AttractSoundSFX = AttractSoundSFX;
                
                _ti.shouldDetonate = shouldDetonate;
                _ti.destroyObjectOnDetonate = destroyObjectOnDetonate;
                _ti.canHitOurself = canHitOurself;
                _ti.KillAllNearby = KillAllNearby;
                _ti.DetonateFX = DetonateFX;
                _ti.DetonateSFX = DetonateSFX;
                _ti.DetonateDelay = DetonateDelay;
                _ti.forceAmtOnRigidbodies = forceAmtOnRigidbodies;
                _ti.damage = damage;

            }

        }
    }
}