using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    public class ShooterAIWizardEditor : EditorWindow
    {
        int tabCount = 0;

        //shooter AI properties
        string ShooterAIName = "Shooter AI";
        ManoeuvreFPSController Player;
        GameObject ShooterAIModel;
        AIType _AIType;

        //shooter weapon properties
        string ShooterAIWeaponName = "Shooter AI Weapon";
        ShooterAIStateManager _ShooterAI;
        GameObject WeaponModel;

        [MenuItem("Manoeuvre/Create/Shooter AI", false, 1)]
        public static void CreateShooterAI()
        {
            GetWindow<ShooterAIWizardEditor>("Shooter AI");
            GetWindow<ShooterAIWizardEditor>("Shooter AI").maxSize = new Vector2(510,625);
            GetWindow<ShooterAIWizardEditor>("Shooter AI").minSize = new Vector2(510,624);
        }

        private void OnGUI()
        {
            Texture t = Resources.Load("EditorContent/ShooterAIWizard") as Texture;
            if (GUILayout.Button(t))
            {
                Application.OpenURL("http://u3d.as/14KR");
            }

            EditorGUILayout.BeginHorizontal("Box");

            string helpboxText = tabCount == 0 ? "Create Shooter AI by assigning the Humanoid Model in the Below 'Shooter AI Model' field." : "Create Shooter Weapon by assigning the Weapon Model. ";

            EditorGUILayout.HelpBox(helpboxText, MessageType.Info);

            if(GUILayout.Button(Resources.Load("EditorContent/YouTube-icon") as Texture, GUILayout.Width(35), GUILayout.Height(38)))
            {
                Application.OpenURL("http://www.youtube.com");
            }

            EditorGUILayout.EndHorizontal();

            DrawTabs();

        }

        void DrawTabs()
        {

            tabCount = GUILayout.Toolbar(tabCount, new Texture[] { Resources.Load("EditorContent/ShooterAI-icon") as Texture, Resources.Load("EditorContent/Weapon-icon") as Texture });

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            switch (tabCount)
            {
                case 0:
                    DrawShooterAICreator();
                    break;

                case 1:
                    DrawShooterWeaponTab();
                    break;
            }

        }

        void DrawShooterAICreator()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Shooter AI Wizard", EditorStyles.centeredGreyMiniLabel);

            _AIType = (AIType)EditorGUILayout.EnumPopup("AI Type", _AIType);

            if(_AIType == AIType.Companion)
            {

                Player = (ManoeuvreFPSController)EditorGUILayout.ObjectField("Player", Player, typeof(ManoeuvreFPSController));

                if (!Player) EditorGUILayout.HelpBox("First Please Assign the Player before proceeding.", MessageType.Warning);

                if (Player)
                {
                    ShooterAIName = EditorGUILayout.TextField("Shooter AI Name", ShooterAIName);

                    ShooterAIModel = (GameObject)EditorGUILayout.ObjectField("Shooter AI Model", ShooterAIModel, typeof(GameObject));
                }
            }
            else
            {
                ShooterAIName = EditorGUILayout.TextField("Shooter AI Name", ShooterAIName);

                ShooterAIModel = (GameObject)EditorGUILayout.ObjectField("Shooter AI Model", ShooterAIModel, typeof(GameObject));
            }

            EditorGUILayout.EndVertical();

            if(ShooterAIModel != null)
            {
                if (ShooterAIModel.GetComponent<Animator>())
                {
                    if(ShooterAIModel.GetComponent<Animator>().isHuman)
                    {
                        //show create button
                        DrawCreateButton();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This ain't Humanoid. Please Assign a Humanoid Model!", MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Please Assign a Humanoid Model!", MessageType.Warning);
                }
            }
            
        }

        void DrawCreateButton()
        {
            if(GUILayout.Button("Create Shooter AI"))
            {
                GameObject _model = Instantiate(ShooterAIModel);

                GameObject _AI = Instantiate(Resources.Load("EditorContent/Shooter AI/Shooter AI")) as GameObject;
                _AI.GetComponent<Animator>().avatar = _model.GetComponent<Animator>().avatar;

                _model.transform.SetParent(_AI.transform);
                _model.transform.localPosition = Vector3.zero;
                _model.transform.localEulerAngles = Vector3.zero;


                //set LH IK Parent
                _AI.GetComponent<ShooterAIStateManager>().AimIK.LeftHandIK.SetParent(_model.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand));
                _AI.GetComponent<ShooterAIStateManager>().AimIK.LeftHandAimIK.SetParent(_model.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand));
                DestroyImmediate(_model.GetComponent<Animator>());

                //reset AI Properties   
                if (_AIType == AIType.Companion)
                    _AI.GetComponent<ShooterAIStateManager>().Player = Player;

                _AI.GetComponent<ShooterAIStateManager>()._AIType = _AIType;
                _AI.transform.position = Vector3.zero;
                _AI.transform.rotation = Quaternion.identity;
                _AI.name = ShooterAIName;

                
                Debug.Log("Shooter AI Created Successfully!!");
            }
        }

        void DrawShooterWeaponTab()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Shooter AI Weapon Wizard", EditorStyles.centeredGreyMiniLabel);

            _ShooterAI = (ShooterAIStateManager)EditorGUILayout.ObjectField("Shooter AI ", _ShooterAI, typeof(ShooterAIStateManager));

            if (_ShooterAI)
            {
                ShooterAIWeaponName = EditorGUILayout.TextField("Weapon Name", ShooterAIWeaponName);
                WeaponModel = (GameObject)EditorGUILayout.ObjectField("Weapon Model", WeaponModel, typeof(GameObject));
            }
            else
            {
                EditorGUILayout.HelpBox("Please Assign the Shooter AI whose weapon you want to Create.", MessageType.Warning);
            }

            if(_ShooterAI && WeaponModel)
            {
                //draw create Button
                if(GUILayout.Button("Create Weapon"))
                {
                    //weapon
                    GameObject _wm = Instantiate(WeaponModel);
                    _wm.name = ShooterAIWeaponName;

                    //muzzle
                    Transform MuzzleFlashLoc = new GameObject().transform;
                    MuzzleFlashLoc.gameObject.name = "Muzzle Flash Location";

                    MuzzleFlashLoc.SetParent(_wm.transform);
                    MuzzleFlashLoc.transform.localEulerAngles = Vector3.zero;
                    MuzzleFlashLoc.transform.localPosition = Vector3.zero;

                    _wm.transform.SetParent(_ShooterAI.AimIK.LeftHandIK.parent);

                    //reset wm
                    _wm.transform.localEulerAngles = Vector3.zero;
                    _wm.transform.localPosition = Vector3.zero;

                    //assign weapon
                    _ShooterAI.WeaponBehaviour.weaponObject = _wm.transform;
                    _ShooterAI.WeaponBehaviour.muzzleLocation = MuzzleFlashLoc;

                    Debug.Log("Weapon Created Successfully !!!");
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}