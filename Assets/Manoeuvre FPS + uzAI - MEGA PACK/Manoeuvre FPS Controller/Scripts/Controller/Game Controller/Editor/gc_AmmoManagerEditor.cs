using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(gc_AmmoManager))]
    public class gc_AmmoManagerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            Texture t = (Texture)Resources.Load("EditorContent/AmmoManager-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("This script always has the Current Equipped Weapon and it thus updates the HUD with current Ammo. \n" +
                                    " It is also managing the reload Manoeuvre of the Weapon once the Ammo reaches 0. ", MessageType.Info);

            base.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }
    }
}