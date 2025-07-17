using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(DestructibleProps))]
    public class DestructiblePropsEditor : Editor
    {
        DestructibleProps _dp;


        private void OnEnable()
        {
            _dp = (DestructibleProps) target;
        }

        public override void OnInspectorGUI()
        {

            DrawNewInspector();

            //DrawDefaultInspector();
        }

        void DrawNewInspector()
        {
            EditorGUI.BeginChangeCheck();

            //weapon texture
            Texture t = (Texture)Resources.Load("EditorContent/Destructible-icon");

            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //draw properties
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Total health of this Destructible Prop.", EditorStyles.helpBox);
            int Health = EditorGUILayout.IntSlider("Health", _dp.Health, 1, 200);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Amount of force to be added to child objects.", EditorStyles.helpBox);
            float _destructionForce = EditorGUILayout.Slider("Destruction Force", _dp._destructionForce, 10, 500);

            EditorGUILayout.LabelField("Explosion Radius i.e how far the child objects can fly.", EditorStyles.helpBox);
            float range = EditorGUILayout.Slider("Range", _dp.range, 0, 50);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Explosion Sound.", EditorStyles.helpBox);
            AudioClip destructionSFX = (AudioClip)EditorGUILayout.ObjectField("Destruction SFX", _dp.destructionSFX, typeof(AudioClip));

            EditorGUILayout.LabelField("Explosion Particle FX.", EditorStyles.helpBox);
            ParticleSystem destructionFX = (ParticleSystem)EditorGUILayout.ObjectField("Destruction FX", _dp.destructionFX, typeof(ParticleSystem));

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("If true, the child meshes will be faded with cool effect.", EditorStyles.helpBox);
            bool fadeMesh = EditorGUILayout.Toggle("Fade Mesh", _dp.fadeMesh);
            float fadeMeshDelay = _dp.fadeMeshDelay;
            float fadeMeshDuration = _dp.fadeMeshDuration;
            Material faderMaterial = _dp.faderMaterial;

            if (_dp.fadeMesh)
            {
                EditorGUILayout.LabelField("How long to wait before starting the fading effect.", EditorStyles.helpBox);
                fadeMeshDelay = EditorGUILayout.Slider("Fade Mesh Delay", _dp.fadeMeshDelay, 0.1f, 5f);

                EditorGUILayout.LabelField("Fade Effect's Total Duration.", EditorStyles.helpBox);
                fadeMeshDuration = EditorGUILayout.Slider("Fade Mesh Duration", _dp.fadeMeshDuration, 0.1f, 2.5f);

                EditorGUILayout.LabelField("Material whose shader properties are being used for effect.", EditorStyles.helpBox);
                faderMaterial = (Material)EditorGUILayout.ObjectField("Fader Material", _dp.faderMaterial, typeof(Material));

            }

            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Destructible Props");

                _dp.Health = Health;
                _dp._destructionForce = _destructionForce;
                _dp.range = range;
                _dp.fadeMesh = fadeMesh;
                _dp.destructionSFX = destructionSFX;
                _dp.destructionFX = destructionFX;
                _dp.fadeMeshDelay = fadeMeshDelay;
                _dp.fadeMeshDuration = fadeMeshDuration;
                _dp.faderMaterial = faderMaterial;

            }
        }

    }
}