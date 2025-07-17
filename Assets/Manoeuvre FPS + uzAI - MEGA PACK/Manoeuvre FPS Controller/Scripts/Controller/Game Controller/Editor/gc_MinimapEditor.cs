using UnityEditor;
using UnityEngine;


namespace Manoeuvre
{
    [CustomEditor(typeof(gc_Minimap))]
    public class gc_MinimapEditor : Editor
    {
        gc_Minimap map;

        private void OnEnable()
        {
            map = (gc_Minimap) target;
        }

        public override void OnInspectorGUI()
        {

            Texture t = (Texture)Resources.Load("EditorContent/Minimap-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            DrawMinimapInspector();

            //DrawDefaultInspector();
        }

        void DrawMinimapInspector()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            GameObject MinimapIcon = (GameObject)EditorGUILayout.ObjectField("Minimap Icon Prefab", map.minimapIconPrefab, typeof(GameObject));
            float maxZoom = EditorGUILayout.Slider("Max Zoom", map.maxZoom, 15f, 25f);
            float minZoom = EditorGUILayout.Slider("Min Zoom", map.minZoom, 5f, 14f);
            float zoomAmount = EditorGUILayout.Slider("Zoom Amount", map.zoomAmount, 0.1f, 5f);
            float ZoomDuration = EditorGUILayout.Slider("Zoom Duration", map.ZoomDuration, 0.1f, 1.5f);

            EditorGUILayout.EndVertical();

            DrawMinimapIconList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Minimap");

                map.minimapIconPrefab = MinimapIcon;
                map.maxZoom = maxZoom;
                map.minZoom = minZoom;
                map.zoomAmount = zoomAmount;
                map.ZoomDuration = ZoomDuration;
            }
        }

        void DrawMinimapIconList()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Add tags to add more icons with different colors for minimap.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                map.MinimapIcons.Add(new MinimapIcon());
            }

            if (GUILayout.Button("Clear"))
            {
                map.MinimapIcons.Clear();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (map.MinimapIcons.Count == 0)
                return;

            EditorGUILayout.BeginVertical("window");

            for (int i = 0; i< map.MinimapIcons.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                string tag = EditorGUILayout.TextField("Tag", map.MinimapIcons[i].Tag);

                if (GUILayout.Button("X", EditorStyles.miniButtonRight))
                {
                    map.MinimapIcons.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                Color iconColor = EditorGUILayout.ColorField("Minimap Icon Color", map.MinimapIcons[i].minimapIconColor);
                float iconScale = EditorGUILayout.Slider("Icon Scale", map.MinimapIcons[i].iconScale , 1f, 3f);

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Minimaplist");

                    map.MinimapIcons[i].Tag = tag;
                    map.MinimapIcons[i].minimapIconColor = iconColor;
                    map.MinimapIcons[i].iconScale = iconScale;
                }
            }

            if (map.MinimapIcons.Count > 0)
            {

                EditorGUILayout.HelpBox("These tags will appear on the minimap with the defined colored icon as soon as game starts.", MessageType.Info);

            }

            EditorGUILayout.EndVertical();


        }
    }
}