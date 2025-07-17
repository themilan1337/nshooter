using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace uzAI
{
    public class WizardEditor : EditorWindow
    {

        string _name = "uzAI Zombie";
        GameObject _mesh;

        [MenuItem("uzAI/Create Zombie", false, 0)]
        public static void OpenWizard()
        {
            GetWindow<WizardEditor>("uzAI Wizard");
            GetWindow<WizardEditor>("uzAI Wizard").maxSize = new Vector2(517, 450);
            GetWindow<WizardEditor>("uzAI Wizard").minSize = new Vector2(517, 449);
        }

        [MenuItem("uzAI/Create/Create Wave Spawner")]
        public static void CreateWaveSpawner()
        {
            //create wp
            GameObject _ws = Instantiate(Resources.Load("EditorContent/Prefabs/WaveSpawner")) as GameObject;
            _ws.name = "Wave Spawner";
            _ws.transform.position = Vector3.zero;
        }

        [MenuItem("uzAI/Create/Create Waypoints Path")]
        public static void CreateWaypointsPath()
        {
            //create wp
            GameObject _wp = Instantiate(Resources.Load("EditorContent/Prefabs/WaypointsPath")) as GameObject;
            _wp.name = "Waypoints Path";
            _wp.transform.position = Vector3.zero;
        }

        [MenuItem("uzAI/Create/Create Audio Target")]
        public static void CreateAudioTarget()
        {
            GameObject _aTarget = (GameObject)Instantiate(Resources.Load("EditorContent/Prefabs/AudioTarget"));
            _aTarget.name = "Audio Target";
            _aTarget.transform.position = Vector3.zero;
        }

        [MenuItem("uzAI/Create/Props/Dynamic Barricades", false, 1)]
        public static void Barricades()
        {
            GameObject Projectile = Instantiate(Resources.Load("EditorContent/Prefabs/Dynamic Barricades Prototype")) as GameObject;
            Projectile.name = "Dynamic Barricades Prototype";
            Projectile.transform.position = Vector3.zero;
            Projectile.transform.rotation = Quaternion.identity;
        }

        [MenuItem("uzAI/Create/Props/Empty Room", false, 1)]
        public static void EmptyRoom()
        {
            GameObject Projectile = Instantiate(Resources.Load("EditorContent/Prefabs/Empty Room Prototype")) as GameObject;
            Projectile.name = "Empty Room Prototype";
            Projectile.transform.position = Vector3.zero;
            Projectile.transform.rotation = Quaternion.identity;
        }

        [MenuItem("uzAI/Documentation")]
        public static void OpenDocumentation()
        {
            //open asset
            Application.OpenURL("http://u3d.as/14G8");

            //open docs
            Application.OpenURL("https://drive.google.com/open?id=1j--MTvI-891LbqBEe_BwSY__f-ju95uX");

        }

        private void OnGUI()
        {
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            Texture t0 = (Texture)Resources.Load("EditorContent/uzai-logo");

            if (GUILayout.Button(t0, GUILayout.Height(233), GUILayout.Width(513)))
            {
                Application.OpenURL("http://u3d.as/14G8");
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.HelpBox("Please Select the correct model which is of Humanoid Rig.", MessageType.Info);

            //YouTube texture
            Texture t4 = (Texture)Resources.Load("EditorContent/YouTube-icon");

            //tut button
            if (GUILayout.Button(t4, GUILayout.Width(40), GUILayout.Height(38)))
            {
                Application.OpenURL("https://www.youtube.com/channel/UCpX1xXvpG6uYiHq18Ju0R6g");
            }

            EditorGUILayout.EndHorizontal();

            DrawZombieCreator();
        }

        void DrawZombieCreator()
        {
            EditorGUILayout.BeginVertical("Box");

            _name = EditorGUILayout.TextField("Zombie Name", _name);
            _mesh = (GameObject)EditorGUILayout.ObjectField("Zombie Model", _mesh, typeof(GameObject));

            //if mesh and animator added
            if(_mesh )
            {
                Animator _animator = _mesh.GetComponent<Animator>();

                if (_animator)
                {
                    if (_animator.isHuman)
                    {
                        if (GUILayout.Button("Create uzAI Zombie"))
                        {
                            CreateZombie(_name, _animator.avatar, _mesh);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Please add a Humanoid Model of Zombie.", MessageType.Error);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Please add a Humanoid Model of Zombie", MessageType.Error);
                }
            }
            

            EditorGUILayout.EndVertical();
        }

        void CreateZombie(string nm, Avatar _avatar, GameObject model)
        {

            GameObject mainZombieInstance = (GameObject) Instantiate(Resources.Load("EditorContent/Prefabs/uzAIZombie"));
            GameObject modelMesh = (GameObject) Instantiate(model);
            mainZombieInstance.name = nm;
            mainZombieInstance.GetComponent<Animator>().avatar = _avatar;

            DestroyImmediate(modelMesh.GetComponent<Animator>());

            modelMesh.transform.SetParent(mainZombieInstance.transform);
            modelMesh.transform.localPosition = Vector3.zero;
            modelMesh.transform.localEulerAngles = Vector3.zero;
            mainZombieInstance.transform.localPosition = Vector3.zero;
            mainZombieInstance.transform.localEulerAngles = Vector3.zero;

            //create wp
            GameObject _wp = Instantiate(Resources.Load("EditorContent/Prefabs/WaypointsPath")) as GameObject;
            _wp.name = "Waypoints Path";
            _wp.transform.position = Vector3.zero;

        }

        void DrawYoutubeTab()
        {

        }

    }
}