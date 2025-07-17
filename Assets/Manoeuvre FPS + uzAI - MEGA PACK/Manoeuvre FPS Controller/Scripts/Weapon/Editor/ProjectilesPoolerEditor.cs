using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(ProjectilesPooler))]
    public class ProjectilesPoolerEditor : Editor
    {
        ProjectilesPooler _p;

        private void OnEnable()
        {
           _p = (ProjectilesPooler) target;
                
        }

        public override void OnInspectorGUI()
        {
            DrawNewInspector();

            //DrawDefaultInspector();
        }

        void DrawNewInspector()
        {

            Texture t = Resources.Load("EditorContent/ProjectilesPool-icon") as Texture;
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("This is the List of all the Projectiles which can be used in Turrets and Weapons.", MessageType.Info);

            EditorGUILayout.HelpBox("Add a new Projectile and use it in any Turret or a Projectile Based Weapon.", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                _p.projectilePool.Add(new Pool());
            }

            if (GUILayout.Button("Clear"))
            {
                _p.projectilePool.Clear();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if(_p.projectilePool.Count < 1)
            {
                EditorGUILayout.HelpBox("No Projectiles to Show. Please add a new Projectile in the List by clicking the Add button.", MessageType.Error);

            }

            for (int i =0; i< _p.projectilePool.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal("Helpbox");

                string ProjectileName = EditorGUILayout.TextField("Projectile Name", _p.projectilePool[i].ProjectileName);
                if(GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _p.projectilePool.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                GameObject ProjectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", _p.projectilePool[i].ProjectilePrefab, typeof(GameObject));
                int PoolSize = EditorGUILayout.IntField("Pool Size", _p.projectilePool[i].PoolSize);

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Projectile Pool");

                    _p.projectilePool[i].ProjectileName = ProjectileName;
                    _p.projectilePool[i].ProjectilePrefab = ProjectilePrefab;
                    _p.projectilePool[i].PoolSize = PoolSize;

                }
            }

        }
    }

}