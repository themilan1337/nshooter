using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(DoorAction))]
    public class DoorActionEditor : Editor
    {

        DoorAction _da;

        SerializedObject so_da;
        SerializedProperty OnOpen;
        SerializedProperty OnClose;
        SerializedProperty OnLocked;

        private void OnEnable()
        {
            _da = (DoorAction)target;

            so_da = new SerializedObject(_da);
            OnOpen = so_da.FindProperty("OnOpen");
            OnClose = so_da.FindProperty("OnClose");
            OnLocked = so_da.FindProperty("OnLocked");
        }

        public override void OnInspectorGUI()
        {

            DrawNewInspector();

            //DrawDefaultInspector();

        }

        void DrawNewInspector()
        {
            Texture t = (Texture)Resources.Load("EditorContent/DoorAction-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Door Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("Select whether this door should be Animated or Procedural.", EditorStyles.helpBox);
            DoorActionType _DoorActionType = (DoorActionType) EditorGUILayout.EnumPopup("Door Action Type", _da._DoorActionType);

            EditorGUILayout.LabelField("If true, this door will be locked by default and you have to create a DoorKey for this door to be opened.", EditorStyles.helpBox);
            bool needsKey = EditorGUILayout.Toggle("Needs Key", _da.needsKey);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("UI Texts", EditorStyles.centeredGreyMiniLabel);

            string InteractionText_Open = EditorGUILayout.TextField("Interaction Text : Open", _da.InteractionText_Open);
            string InteractionText_Close = EditorGUILayout.TextField("Interaction Text : Close", _da.InteractionText_Close);
            string InteractionText_Locked = EditorGUILayout.TextField("Interaction Text : Locked", _da.InteractionText_Locked);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            Animation DoorAnimation = _da.DoorAnimation;
            string CloseAnimation = _da.CloseAnimation;
            string OpenAnimation = _da.OpenAnimation;

            float LerpDuration = _da.LerpDuration;

            Vector3 FromPosition = _da.FromPosition;
            Vector3 ToPosition = _da.ToPosition;

            Vector3 FromRotation = _da.FromRotation;
            Vector3 ToRotation = _da.ToRotation;

            switch (_DoorActionType)
            {
                case DoorActionType.Animation:

                    EditorGUILayout.LabelField("Animation", EditorStyles.centeredGreyMiniLabel);

                    DoorAnimation = EditorGUILayout.ObjectField("Door Animation", _da.DoorAnimation, typeof(Animation)) as Animation;
                    OpenAnimation = EditorGUILayout.TextField("Open Animation", _da.OpenAnimation);
                    CloseAnimation = EditorGUILayout.TextField("Close Animation", _da.CloseAnimation);

                    break;

                case DoorActionType.LerpPosition:

                    EditorGUILayout.LabelField("Lerp Position", EditorStyles.centeredGreyMiniLabel);

                    EditorGUILayout.LabelField("Door's Position will be lerped from the 'From Position' to 'To Position' in 'Lerp Duration' seconds.", EditorStyles.helpBox);

                    LerpDuration = EditorGUILayout.FloatField("Lerp Duration", _da.LerpDuration);
                    LerpDuration = Mathf.Clamp(LerpDuration, 0, LerpDuration);

                    FromPosition = EditorGUILayout.Vector3Field("From Position", _da.FromPosition);
                    ToPosition = EditorGUILayout.Vector3Field("To Position", _da.ToPosition);

                    break;

                case DoorActionType.LerpRotation:

                    EditorGUILayout.LabelField("Lerp Rotation", EditorStyles.centeredGreyMiniLabel);

                    EditorGUILayout.LabelField("Door's Rotation will be lerped from the 'From Rotation' to 'To Rotation' in 'Lerp Duration' seconds.", EditorStyles.helpBox);

                    LerpDuration = EditorGUILayout.FloatField("Lerp Duration", _da.LerpDuration);
                    LerpDuration = Mathf.Clamp(LerpDuration, 0, LerpDuration);

                    FromRotation = EditorGUILayout.Vector3Field("From Rotation", _da.FromRotation);
                    ToRotation = EditorGUILayout.Vector3Field("To Rotation", _da.ToRotation);

                    break;

                case DoorActionType.LerpPositionRotation:

                    EditorGUILayout.LabelField("Lerp Position Rotation", EditorStyles.centeredGreyMiniLabel);

                    EditorGUILayout.LabelField("Door's Position and Rotation will be lerped.", EditorStyles.helpBox);

                    LerpDuration = EditorGUILayout.FloatField("Lerp Duration", _da.LerpDuration);
                    LerpDuration = Mathf.Clamp(LerpDuration, 0, LerpDuration);

                    EditorGUILayout.BeginVertical("Box");
                    FromPosition = EditorGUILayout.Vector3Field("From Position", _da.FromPosition);
                    ToPosition = EditorGUILayout.Vector3Field("To Position", _da.ToPosition);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("Box");
                    FromRotation = EditorGUILayout.Vector3Field("From Rotation", _da.FromRotation);
                    ToRotation = EditorGUILayout.Vector3Field("To Rotation", _da.ToRotation);
                    EditorGUILayout.EndVertical();

                    break;
            }
           
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Door Sounds", EditorStyles.centeredGreyMiniLabel);

            AudioClip OpenSound = EditorGUILayout.ObjectField("Open Sound", _da.OpenSound, typeof(AudioClip)) as AudioClip;
            AudioClip CloseSound = EditorGUILayout.ObjectField("Close Sound", _da.CloseSound, typeof(AudioClip)) as AudioClip;
            AudioClip LockedSound = EditorGUILayout.ObjectField("Locked Sound", _da.LockedSound, typeof(AudioClip)) as AudioClip;

            EditorGUILayout.EndVertical();

            DrawCorrespondingSwitches();

            DrawEvents();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Door Action");

                _da._DoorActionType = _DoorActionType;
                _da.needsKey = needsKey;

                _da.InteractionText_Open = InteractionText_Open;
                _da.InteractionText_Close = InteractionText_Close;
                _da.InteractionText_Locked = InteractionText_Locked;

                _da.DoorAnimation = DoorAnimation;
                _da.CloseAnimation = CloseAnimation;
                _da.OpenAnimation = OpenAnimation;

                _da.LerpDuration = LerpDuration;

                _da.FromPosition = FromPosition;
                _da.ToPosition = ToPosition;

                _da.FromRotation = FromRotation;
                _da.ToRotation = ToRotation;

                _da.OpenSound = OpenSound;
                _da.CloseSound = CloseSound;
                _da.LockedSound = LockedSound;

            }
        }

        void DrawEvents()
        {

            EditorGUILayout.PropertyField(OnOpen);
            
            EditorGUILayout.PropertyField(OnClose);

            EditorGUILayout.PropertyField(OnLocked);

            so_da.ApplyModifiedProperties();

        }

        void DrawCorrespondingSwitches()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Corresponding Switches", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("If this door is operated using multiple Switches, add them all here below.", EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Add"))
            {
                DoorAction _nda = new DoorAction();
                _da.CorrespondingSwitches.Add(_nda);
            }

            if (GUILayout.Button("Clear"))
            {
                _da.CorrespondingSwitches.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i =0; i< _da.CorrespondingSwitches.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("Box");

                DoorAction cs_da = EditorGUILayout.ObjectField(_da.CorrespondingSwitches[i], typeof(DoorAction)) as DoorAction;

                if(GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _da.CorrespondingSwitches.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Corresponding Switches");

                    _da.CorrespondingSwitches[i] = cs_da;
                }
            }

            EditorGUILayout.EndVertical();

        }

    }
}