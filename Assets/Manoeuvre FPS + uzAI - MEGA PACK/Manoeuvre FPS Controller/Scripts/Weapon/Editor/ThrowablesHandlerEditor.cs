using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(ThrowablesHandler))]
    public class ThrowablesHandlerEditor : Editor
    {

        ThrowablesHandler _th;
        SerializedObject _thSO;

        private void OnEnable()
        {
            _th = (ThrowablesHandler) target;
            _thSO = new SerializedObject(_th);

        }

        public override void OnInspectorGUI()
        {

            DrawNewInspector();

            //DrawDefaultInspector();

        }

        void DrawNewInspector()
        {
            //weapon texture
            Texture t = (Texture)Resources.Load("EditorContent/ThrowablesHandler-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("UI Pickup Text Prefab.", EditorStyles.helpBox);
            GameObject PickupTextPrefab = EditorGUILayout.ObjectField("Pickup Text Prefab", _th.PickupTextPrefab, typeof(GameObject)) as GameObject;

            EditorGUILayout.EndVertical();

            //draw event listener
            SerializedProperty OnThrowEvent = _thSO.FindProperty("OnThrowEvent");
            EditorGUILayout.PropertyField(OnThrowEvent);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawAllThrowables();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Throwables Handler");

                _th.PickupTextPrefab = PickupTextPrefab;
            }

        }

        void DrawAllThrowables()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                _th.AllThrowables.Add(new Throwables());

            }

            if (GUILayout.Button("Clear"))
            {
                _th.AllThrowables.Clear();

            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _th.AllThrowables.Count; i++)
            {

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal("Box");

                EditorGUILayout.LabelField("Item Icon");

                Sprite _ItemIcon = EditorGUILayout.ObjectField(_th.AllThrowables[i]._ItemIcon, typeof(Sprite)) as Sprite;

                if (GUILayout.Button("X", EditorStyles.miniButtonRight , GUILayout.Width(35)))
                {
                    _th.AllThrowables.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                string ItemName = EditorGUILayout.TextField("Item Name", _th.AllThrowables[i].ItemName);
                WeaponThrowable Throwable = EditorGUILayout.ObjectField("Throwable Weapon", _th.AllThrowables[i].Throwable, typeof(WeaponThrowable)) as WeaponThrowable;
                ThrowableItem _ThrowableItem = EditorGUILayout.ObjectField("Throwable Item", _th.AllThrowables[i]._ThrowableItem, typeof(ThrowableItem)) as ThrowableItem;

                float _ThrowForce = EditorGUILayout.FloatField("Throw Force", _th.AllThrowables[i]._ThrowForce);
                _ThrowForce = Mathf.Clamp(_ThrowForce, 0.1f, _ThrowForce);

                bool AddToInventory = EditorGUILayout.Toggle("Add To Inventory", _th.AllThrowables[i].AddToInventory);

                int _ItemQuantity = _th.AllThrowables[i]._ItemQuantity;
                _ItemQuantity = Mathf.Clamp(_ItemQuantity, 1, _ItemQuantity);

                if (AddToInventory)
                    _ItemQuantity = EditorGUILayout.IntField("Item Quantity", _th.AllThrowables[i]._ItemQuantity);

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "All Throwables List");

                    _th.AllThrowables[i].ItemName = ItemName;
                    _th.AllThrowables[i].Throwable = Throwable;
                    _th.AllThrowables[i]._ThrowableItem = _ThrowableItem;
                    _th.AllThrowables[i]._ThrowForce = _ThrowForce;
                    _th.AllThrowables[i].AddToInventory = AddToInventory;
                    _th.AllThrowables[i]._ItemQuantity = _ItemQuantity;
                    _th.AllThrowables[i]._ItemIcon = _ItemIcon;
                }

            }


        }

    }
}