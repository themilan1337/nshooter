using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class WizardEditor : EditorWindow
    {
        int wizardTabCount;
        int pickupTabCount;
        int pickupQuantity_powerup;
        int pickupQuantity_weapon;

        Transform _weaponObject;
        ThrowableItem _throwableItemObject;

        Texture[] toolbarTextures = new Texture[3];

        WeaponType weaponType = WeaponType.Shooter;
        string weaponName;
        GameObject weaponObj;

        [MenuItem("Manoeuvre/Wizard", false , 0)]
        public static void OpenWizard()
        {
            GetWindow<WizardEditor>("Manoeuvre Wizard");
            GetWindow<WizardEditor>("Manoeuvre Wizard").maxSize = new Vector2(680, 600);
            GetWindow<WizardEditor>("Manoeuvre Wizard").minSize = new Vector2(679, 599);
        }

        [MenuItem("Manoeuvre/Create/Turret")]
        public static void CreateTurret()
        {
            GameObject Turret = Instantiate(Resources.Load("EditorContent/Addons/Turret")) as GameObject;
            Turret.name = "Turret";
            Turret.transform.position = Vector3.zero;
            Turret.transform.rotation = Quaternion.identity;

        }

        [MenuItem("Manoeuvre/Create/Projectiles Pool")]
        public static void ProjectilesPool()
        {
            GameObject Pool = Instantiate(Resources.Load("EditorContent/Addons/ProjectilesPool")) as GameObject;
            Pool.name = "ProjectilesPool";
            Pool.transform.position = Vector3.zero;
            Pool.transform.rotation = Quaternion.identity;
        }

        [MenuItem("Manoeuvre/Create/Projectile")]
        public static void Projectile()
        {
            GameObject Projectile = Instantiate(Resources.Load("EditorContent/Addons/Projectile")) as GameObject;
            Projectile.name = "Projectile";
            Projectile.transform.position = Vector3.zero;
            Projectile.transform.rotation = Quaternion.identity;
        }

        [MenuItem("Manoeuvre/Create/Props/Destructible")]
        public static void Destructible()
        {
            GameObject Projectile = Instantiate(Resources.Load("EditorContent/Addons/DestructibleCrate")) as GameObject;
            Projectile.name = "Destructible Crate";
            Projectile.transform.position = Vector3.zero;
            Projectile.transform.rotation = Quaternion.identity;
        }

        [MenuItem("Manoeuvre/Create/Props/Dynamic Barricades")]
        public static void Barricades()
        {
            GameObject Projectile = Instantiate(Resources.Load("EditorContent/Addons/Dynamic Barricades Prototype")) as GameObject;
            Projectile.name = "Dynamic Barricades Prototype";
            Projectile.transform.position = Vector3.zero;
            Projectile.transform.rotation = Quaternion.identity;
        }

        [MenuItem("Manoeuvre/Create/Props/Empty Room")]
        public static void EmptyRoom()
        {
            GameObject Projectile = Instantiate(Resources.Load("EditorContent/Addons/Empty Room Prototype")) as GameObject;
            Projectile.name = "Empty Room Prototype";
            Projectile.transform.position = Vector3.zero;
            Projectile.transform.rotation = Quaternion.identity;
        }

        [MenuItem("Manoeuvre/Documentation")]
        public static void Documentation()
        {
            //asset store link
            Application.OpenURL("http://u3d.as/14Cy");

            //documentation link
            Application.OpenURL("https://drive.google.com/open?id=12Ja2GcwiNls7hnJcwrc2Qm2qZZp5rWy5");
        }

        [MenuItem("Manoeuvre/Reset PlayerPrefs")]
        public static void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Cleared All Data!");
        }

        private void OnGUI()
        {
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            //YouTube texture
            Texture t0 = (Texture)Resources.Load("EditorContent/manoeuvre_Logo_l");
            //tut button
            if (GUILayout.Button(t0))
            {
                Application.OpenURL("http://u3d.as/14Cy");
            }

            EditorGUILayout.BeginHorizontal("box");

            switch (wizardTabCount)
            {
                //controller
                case 0:
                    EditorGUILayout.HelpBox("Create Manoeuvre Controller.", MessageType.Info);
                    break;

                case 01:
                    EditorGUILayout.HelpBox("Create Weapon.", MessageType.Info);
                    break;

                case 02:
                    EditorGUILayout.HelpBox("Create Pickups for Manoeuvre Controller and Weapon.", MessageType.Info);
                    break;

            }

            //YouTube texture
            Texture t4 = (Texture)Resources.Load("EditorContent/YouTube-icon");

            //tut button
            if (GUILayout.Button(t4, GUILayout.Width(40), GUILayout.Height(38)))
            {
                Application.OpenURL("https://www.youtube.com/channel/UCpX1xXvpG6uYiHq18Ju0R6g");
            }

            EditorGUILayout.EndHorizontal();

            //controller texture
            Texture t1 = (Texture)Resources.Load("EditorContent/Controller-icon");
            toolbarTextures[0] = t1;

            //weapon texture
            Texture t2 = (Texture)Resources.Load("EditorContent/Weapon-icon");
            toolbarTextures[1] = t2;

            //pickups texture
            Texture t3 = (Texture)Resources.Load("EditorContent/Pickups-icon");
            toolbarTextures[2] = t3;

            EditorGUILayout.BeginHorizontal("box");

            wizardTabCount = GUILayout.Toolbar(wizardTabCount, toolbarTextures);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            switch (wizardTabCount)
            {
                case 0:
                    OpenControllerCreationDialog();
                    break;

                case 01:
                    OpenWeaponCreationDialog();
                    break;

                case 02:
                    OpenPickupsCreationDialog();
                    break;

            }

        }

        void OpenControllerCreationDialog()
        {
            EditorGUILayout.HelpBox("Clicking the Create Button below will Add a New Manoeuvre FPS Rig, UI, and Game Controller. This is ideal if you want to start from scratch. " +
                "Please make sure you are doing this in an Empty Scene.", MessageType.Info);

            if(GUILayout.Button("Create New Controller"))
            {
                CreateNewController();
            }

            EditorGUILayout.HelpBox("This will create A Complete Controller with all the Weapons and Throwable items already Attached!" +
                "Please make sure you are doing this in an Empty Scene.", MessageType.Info);

            if (GUILayout.Button("Create Complete Controller"))
            {
                CreateCompleteController();
            }

        }

        void CreateNewController()
        {
            //Controller GameObject
            GameObject _controller = Instantiate(Resources.Load("EditorContent/Controller/Manoeuvre FPS Rig")) as GameObject;
            _controller.name = "Manoeuvre FPS Rig";
            _controller.transform.position = Vector3.zero;

            //UI
            GameObject _UI = Instantiate(Resources.Load("EditorContent/Controller/UICamera")) as GameObject;
            _UI.name = "UICamera";

            //Game Controller
            GameObject _gameController = Instantiate(Resources.Load("EditorContent/Controller/GameController")) as GameObject;
            _gameController.name = "GameController";

            Debug.Log("Conroller Created");

        }

        void CreateCompleteController()
        {
            //Controller GameObject
            GameObject _controller = Instantiate(Resources.Load("EditorContent/Controller/Manoeuvre FPS Rig Animated Weapons")) as GameObject;
            _controller.name = "Manoeuvre FPS Rig Animated Weapons";
            _controller.transform.position = Vector3.zero;

            //UI
            GameObject _UI = Instantiate(Resources.Load("EditorContent/Controller/UICamera")) as GameObject;
            _UI.name = "UICamera";

            //Game Controller
            GameObject _gameController = Instantiate(Resources.Load("EditorContent/Controller/GameController")) as GameObject;
            _gameController.name = "GameController";

            Debug.Log("Conroller Created");

        }

        void OpenWeaponCreationDialog()
        {

            EditorGUILayout.BeginVertical("box");

            weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);

            weaponName = EditorGUILayout.TextField("Weapon Name", weaponName);

            weaponObj = (GameObject)EditorGUILayout.ObjectField("Assign Weapon", weaponObj, typeof(GameObject));

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (weaponObj && !string.IsNullOrEmpty(weaponName))
            {
                if (GUILayout.Button("Create Weapon"))
                {
                    CreateWeapon();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please write a name and Assign a Weapon Object", MessageType.Info);
            }


        }

        void CreateWeapon()
        {

            switch (weaponType)
            {
                case WeaponType.Melee:
                    MakeMeleeWeapon();
                    break;

                case WeaponType.Shooter:
                    MakeShooterWeapon();
                    break;

                case WeaponType.Throwable:
                    MakeThrowableWeapon();
                    break;

                case WeaponType.ThrowableItem:
                    MakeThrowableItem();
                    break;
            }

        }

        void MakeMeleeWeapon()
        {
            //make this object weapon
            Transform selectedObject = Instantiate(weaponObj.transform) as Transform;

            foreach (Transform go in selectedObject.GetComponentsInChildren<Transform>())
            {
                go.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }

            GameObject Weapon = Instantiate(Resources.Load("Prefabs/MeleeWeapon")) as GameObject;

            Weapon.name = weaponName;
            Weapon.GetComponent<WeaponMelee>().WeaponName = weaponName;

            Weapon.transform.position = selectedObject.position;

            selectedObject.SetParent(Weapon.transform);
            selectedObject.localPosition = Vector3.zero;

            Debug.Log("Created Melee Weapon Successfully!");
        }

        void MakeShooterWeapon()
        {
            //make this object weapon
            Transform selectedObject = Instantiate(weaponObj.transform) as Transform;

            foreach (Transform go in selectedObject.GetComponentsInChildren<Transform>())
            {
                go.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }

            GameObject Weapon = Instantiate(Resources.Load("Prefabs/ShooterWeapon")) as GameObject;

            Weapon.name = weaponName;
            Weapon.GetComponent<WeaponShooter>().WeaponName = weaponName;

            Weapon.transform.position = selectedObject.position;

            selectedObject.SetParent(Weapon.transform);
            selectedObject.localPosition = Vector3.zero;

            Debug.Log("Created Shooter Weapon Successfully!");
        }

        void MakeThrowableWeapon()
        {
            //make this object weapon
            Transform selectedObject = Instantiate(weaponObj.transform) as Transform;

            foreach (Transform go in selectedObject.GetComponentsInChildren<Transform>())
            {
                go.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }

            GameObject Weapon = Instantiate(Resources.Load("Prefabs/ThrowableWeapon")) as GameObject;

            Weapon.name = weaponName;
            Weapon.GetComponent<WeaponThrowable>().WeaponObject = selectedObject.gameObject;

            Weapon.transform.position = selectedObject.position;

            selectedObject.SetParent(Weapon.transform);
            selectedObject.localPosition = Vector3.zero;

            Debug.Log("Created Throwable Weapon Successfully!");
        }

        void MakeThrowableItem()
        {
            //make this object weapon
            Transform selectedObject = Instantiate(weaponObj.transform) as Transform;

            foreach (Transform go in selectedObject.GetComponentsInChildren<Transform>())
            {
                go.gameObject.layer = LayerMask.NameToLayer("Default");
            }

            GameObject Weapon = Instantiate(Resources.Load("Prefabs/Item-Throwable")) as GameObject;

            Weapon.name = weaponName;
            Weapon.GetComponent<ThrowableItem>().ItemName = weaponName;

            Weapon.transform.position = selectedObject.position;

            selectedObject.SetParent(Weapon.transform);
            selectedObject.localPosition = Vector3.zero;

            Debug.Log("Created Throwable Item Successfully!");

        }

        void OpenPickupsCreationDialog()
        {
            pickupTabCount = GUILayout.Toolbar(pickupTabCount, new string[] { "Powerups", "Weapon Pickups", "Throwables Pickup" });

            GUILayout.Label("", GUI.skin.horizontalSlider);

            switch (pickupTabCount)
            {
                case 0:
                    CreatePowerupsPickup();
                    break;
                case 1:
                    CreateWeaponsPickup();
                    break;
                case 2:
                    CreateThrowablesPickup();
                    break;
            }
        }

        void CreatePowerupsPickup()
        {
            EditorGUILayout.HelpBox("Create upto 10 Power pickups. Just select the number in slider and hit Create Button! \n" +
                                    "You are advised, once you have created these pickups, modify their properties directly in the Inspector!", MessageType.Info);

            GUILayout.BeginHorizontal("box");

            GUILayout.Label("Number of Power pickups to Create");
            pickupQuantity_powerup = EditorGUILayout.IntSlider( pickupQuantity_powerup, 1, 10);

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Create " + pickupQuantity_powerup + " Powerups"))
            {

                for(int i =0; i< pickupQuantity_powerup; i++)
                {
                    GameObject pickup = Instantiate(Resources.Load("Prefabs/Powerup_Pickup")) as GameObject;

                    pickup.name = "Powerup Pickup";

                    Debug.Log("Pickup Created");
                }

            }

        }

        void CreateWeaponsPickup()
        {
            EditorGUILayout.HelpBox("Please Drag the weapon assigned under Weapon Camera whose pickup you want to create. \n" +
                                    "NOTE : If you haven't created any weapon then you need to create a weapon first by clicking Weapon Tab!", MessageType.Info);

            EditorGUILayout.BeginHorizontal("box");

            _weaponObject = (Transform)EditorGUILayout.ObjectField("Weapon", _weaponObject, typeof(Transform));

            if (_weaponObject)
            {
                if(GUILayout.Button("Create Weapon Pickup for " + _weaponObject.name))
                {
                    GameObject weaponPickup = (GameObject) Instantiate(Resources.Load("Prefabs/Weapon_pickup"));
                    weaponPickup.name = _weaponObject.name + " Pickup";

                    Transform _weaponObjectClone = (Transform) Instantiate(_weaponObject);

                    _weaponObjectClone.gameObject.SetActive(true);
                    _weaponObjectClone.SetParent(weaponPickup.transform);
                    _weaponObjectClone.localPosition = Vector3.zero;

                    weaponPickup.GetComponent<Weapon_Pickup>().WeaponObject = _weaponObject;
                    weaponPickup.layer = LayerMask.NameToLayer("Default");

                    DestroyImmediate(_weaponObjectClone.GetComponent<WeaponProceduralManoeuvre>());
                    DestroyImmediate(_weaponObjectClone.GetComponent<WeaponShooter>());
                    DestroyImmediate(_weaponObjectClone.GetComponent<WeaponMelee>());


                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void CreateThrowablesPickup()
        {
            EditorGUILayout.HelpBox("Please Drag and  Assign the Throwable Item prefab which you must have already created from the Weapons Tab \n" +
                                    "NOTE : If you haven't created any Throwable Item then you need to create it first by clicking Weapons Tab!", MessageType.Info);

            EditorGUILayout.BeginHorizontal("box");

            _throwableItemObject = (ThrowableItem)EditorGUILayout.ObjectField("Throwable Item", _throwableItemObject, typeof(ThrowableItem));

            if (_throwableItemObject)
            {
                if (GUILayout.Button("Create Throwable Pickup for " + _throwableItemObject.ItemName))
                {
                    GameObject ThrowablesPickup = (GameObject)Instantiate(Resources.Load("Prefabs/Throwable Pickup"));
                    ThrowablesPickup.name = _throwableItemObject.name + " Pickup";

                    Transform _ThrowablesPickupClone = (Transform)Instantiate(_throwableItemObject.transform);

                    _ThrowablesPickupClone.gameObject.SetActive(true);
                    _ThrowablesPickupClone.SetParent(ThrowablesPickup.transform);
                    _ThrowablesPickupClone.localPosition = Vector3.zero;

                    ThrowablesPickup.GetComponent<ThrowableItem_Pickup>().itemName = _throwableItemObject.ItemName;
                    ThrowablesPickup.layer = LayerMask.NameToLayer("Default");

                    DestroyImmediate(_ThrowablesPickupClone.GetComponent<ThrowableItem>());
                    if(_ThrowablesPickupClone.GetComponent<ThrowableItem_Pickup>())
                        DestroyImmediate(_ThrowablesPickupClone.GetComponent<ThrowableItem_Pickup>());
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}