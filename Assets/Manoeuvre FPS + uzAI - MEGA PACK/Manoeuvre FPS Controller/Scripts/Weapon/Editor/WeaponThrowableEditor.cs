using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(WeaponThrowable))]
    public class WeaponThrowableEditor : Editor
    {
        WeaponThrowable _wt;

        private void OnEnable()
        {
            _wt = (WeaponThrowable) target;
        }

        public override void OnInspectorGUI()
        {

            DrawNewInspector();

            //DrawDefaultInspector();

        }

        void DrawNewInspector()
        {
            Texture t = (Texture)Resources.Load("EditorContent/WeaponThrowable-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("The Throwable Item name, this has to be UNIQUE.", EditorStyles.helpBox);
            string ItemName = EditorGUILayout.TextField("Item Name", _wt.ItemName);

            EditorGUILayout.LabelField("The Weapon Mesh Renderer  - Should be already assigned.", EditorStyles.helpBox);
            GameObject WeaponObject = EditorGUILayout.ObjectField("Weapon Object", _wt.WeaponObject, typeof(GameObject)) as GameObject;

            EditorGUILayout.LabelField("This is the child object of hand which is just a renderer (Optional)", EditorStyles.helpBox);
            GameObject ItemRenderer = EditorGUILayout.ObjectField("Item Renderer", _wt.ItemRenderer, typeof(GameObject)) as GameObject;

            EditorGUILayout.LabelField("The Animation Component which will be playing Throw Animation.", EditorStyles.helpBox);
            Animation _Animation = EditorGUILayout.ObjectField("Animation Component", _wt._Animation, typeof(Animation)) as Animation;

            EditorGUILayout.LabelField("Throw Animation Name.", EditorStyles.helpBox);
            string throwAnimation = EditorGUILayout.TextField("Throw Animation", _wt.throwAnimation);

            EditorGUILayout.LabelField("Throw Animation Speed.", EditorStyles.helpBox);
            float AnimationSpeed = EditorGUILayout.FloatField("Animation Speed", _wt.AnimationSpeed);
            _wt.AnimationSpeed = Mathf.Clamp(_wt.AnimationSpeed, 0.01f, _wt.AnimationSpeed);

            EditorGUILayout.LabelField("The time at which the Throw Method will be called i.e real object will be spawned in the World!.", EditorStyles.helpBox);
            float AnimationNormalizedTime = EditorGUILayout.FloatField("Animation Normalized Time", _wt.AnimationNormalizedTime);
            _wt.AnimationNormalizedTime = Mathf.Clamp(_wt.AnimationNormalizedTime, 0.01f, _wt.AnimationNormalizedTime);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Weapon Throwable");

                _wt.ItemName = ItemName;
                _wt.WeaponObject = WeaponObject;
                _wt.ItemRenderer = ItemRenderer;
                _wt._Animation = _Animation;
                _wt.throwAnimation = throwAnimation;
                _wt.AnimationSpeed = AnimationSpeed;
                _wt.AnimationNormalizedTime = AnimationNormalizedTime;
            }
        }

    }
}