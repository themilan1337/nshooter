using UnityEditor;
using UnityEngine;

namespace Manoeuvre
{
    [CustomEditor(typeof(GameOver))]
    public class GameOverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Texture t = (Texture)Resources.Load("EditorContent/GameOver-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("A simple Gameover script Example which will enable the Game Over UI after the below defined amount of seconds when Player's health reaches 0 . ", MessageType.Info);

            base.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }
    }
}