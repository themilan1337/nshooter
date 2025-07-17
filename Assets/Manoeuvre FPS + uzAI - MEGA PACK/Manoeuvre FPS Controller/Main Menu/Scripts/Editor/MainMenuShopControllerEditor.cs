using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(MainMenuShopController))]
    public class MainMenuShopControllerEditor : Editor
    {
        MainMenuShopController _sc;
        int currentSelectedWeapon;
        bool _showUpgrades;

        private void OnEnable()
        {
            _sc = (MainMenuShopController)target;
        }

        public override void OnInspectorGUI()
        {
            //Controller texture
            Texture t = (Texture)Resources.Load("EditorContent/ShopController-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            DrawTopProperties();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawWeaponUpgradesList();

            //DrawDefaultInspector();
        }

        void DrawTopProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("UI References, after re-skinning, make sure you set these up properly.", MessageType.Info);

            GameObject AttributePrefab = (GameObject)EditorGUILayout.ObjectField("Attribute Prefab", _sc.AttributePrefab, typeof(GameObject));

            Text WeaponNameText = (Text)EditorGUILayout.ObjectField("Weapon Name Text", _sc.WeaponNameText, typeof(Text));
            Text WeaponCurrentLevelText = (Text)EditorGUILayout.ObjectField("Weapon Current Level Text", _sc.WeaponCurrentLevelText, typeof(Text));
            Text InGameCurrencyText = (Text)EditorGUILayout.ObjectField("In Game Currency Text", _sc.InGameCurrencyText, typeof(Text));

            Button NextWeaponBtn = (Button)EditorGUILayout.ObjectField("Next Weapon Btn", _sc.NextWeaponBtn, typeof(Button));
            Button PreviousWeaponBtn = (Button)EditorGUILayout.ObjectField("Previous Weapon Btn", _sc.PreviousWeaponBtn, typeof(Button));
            Button UpgradeWeaponBtn = (Button)EditorGUILayout.ObjectField("Upgrade Weapon Btn", _sc.UpgradeWeaponBtn, typeof(Button));

            Transform AttributesContainer = (Transform)EditorGUILayout.ObjectField("Attributes Container", _sc.AttributesContainer, typeof(Transform));
            Transform WeaponsMeshContainer = (Transform)EditorGUILayout.ObjectField("Weapons Mesh Container", _sc.WeaponsMeshContainer, typeof(Transform));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Main Menu Scene Name which is in Build Settings", MessageType.None);
            string MainMenuSceneName = EditorGUILayout.TextField("Main Menu Scene Name", _sc.MainMenuSceneName);

            EditorGUILayout.HelpBox("Sound FX for Button Click and Hover.", MessageType.None);
            string ButtonClickSFX = EditorGUILayout.TextField("Button Click SFX", _sc.ButtonClickSFX);
            string ButtonHoverSFX = EditorGUILayout.TextField("Button Hover SFX", _sc.ButtonHoverSFX);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_sc Top Properties");

                _sc.AttributePrefab = AttributePrefab;
                _sc.WeaponNameText = WeaponNameText;
                _sc.WeaponCurrentLevelText = WeaponCurrentLevelText;
                _sc.InGameCurrencyText = InGameCurrencyText;
                _sc.NextWeaponBtn = NextWeaponBtn;
                _sc.PreviousWeaponBtn = PreviousWeaponBtn;
                _sc.UpgradeWeaponBtn = UpgradeWeaponBtn;
                _sc.AttributesContainer = AttributesContainer;
                _sc.WeaponsMeshContainer = WeaponsMeshContainer;

                _sc.MainMenuSceneName = MainMenuSceneName;
                _sc.ButtonClickSFX = ButtonClickSFX;
                _sc.ButtonHoverSFX = ButtonHoverSFX;

            }
        }

        void DrawWeaponUpgradesList()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox("Add or Clear All the Weapon Upgrades.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add New"))
            {
                AttributesDefinition newAD = new AttributesDefinition();
                _sc.attributesDefinitions.Add(newAD);
            }

            if (GUILayout.Button("Clear All"))
            {
                _sc.attributesDefinitions.Clear();
            }

            EditorGUILayout.EndHorizontal();

            if (_sc.attributesDefinitions.Count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one Weapon to proceed.", MessageType.Warning);
                currentSelectedWeapon = 0;
                return;
            }

            EditorGUILayout.HelpBox("Easily Select Next or Previous Weapon that you have added in the List.", MessageType.Info);

            EditorGUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("<--", GUILayout.Height(35)))
            {
                if (currentSelectedWeapon > 0)
                    currentSelectedWeapon--;
                else
                    currentSelectedWeapon = _sc.attributesDefinitions.Count - 1;
            }

            EditorGUILayout.LabelField((currentSelectedWeapon + 1) + "/" + _sc.attributesDefinitions.Count, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150), GUILayout.Height(35));

            if (GUILayout.Button("--->", GUILayout.Height(35)))
            {
                if (currentSelectedWeapon < _sc.attributesDefinitions.Count - 1)
                    currentSelectedWeapon++;
                else
                    currentSelectedWeapon = 0;
            }

            EditorGUILayout.EndHorizontal();

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Remove This Weapon"))
            {
                _sc.attributesDefinitions.RemoveAt(currentSelectedWeapon);
                if (_sc.attributesDefinitions.Count > 0 && currentSelectedWeapon != 0)
                    currentSelectedWeapon--;
                else if (_sc.attributesDefinitions.Count < 1 && currentSelectedWeapon == 0)
                    _sc.attributesDefinitions.Clear();

                return;
            }

            GUI.backgroundColor = oldColor;

            string WeaponName = EditorGUILayout.TextField("Weapon Name", _sc.attributesDefinitions[currentSelectedWeapon].WeaponName);
            GameObject WeaponMesh = (GameObject)EditorGUILayout.ObjectField("Weapon Mesh", _sc.attributesDefinitions[currentSelectedWeapon].WeaponMesh, typeof(GameObject));
            Vector3 WeaponMeshScale = EditorGUILayout.Vector3Field("Weapon Mesh Scale", _sc.attributesDefinitions[currentSelectedWeapon].WeaponMeshScale);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "_sc Weapons upgrades List");

                _sc.attributesDefinitions[currentSelectedWeapon].WeaponName = WeaponName;
                _sc.attributesDefinitions[currentSelectedWeapon].WeaponMesh = WeaponMesh;
                _sc.attributesDefinitions[currentSelectedWeapon].WeaponMeshScale = WeaponMeshScale;

            }

            DrawCurrentWeaponsAttributesList();

            EditorGUILayout.EndVertical();
        }

        void DrawCurrentWeaponsAttributesList()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("All this Weapon's Attributes.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                UnitAttribute _ua = new UnitAttribute();
                _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes.Add(_ua);

                for(int i = 0; i < _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades.Count; i++)
                {
                    AttributeUpgrade _item = new AttributeUpgrade();

                    _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].AttributeUpgrade.Add(_item);
                }
            }

            if (GUILayout.Button("Clear All"))
            {
                _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                string AttributeName = EditorGUILayout.TextField("Attribute Name", _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].AttributeName);
                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    for (int k = 0; k < _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades.Count; k++)
                    {
                        for(int k1 = 0; k1< _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[k].AttributeUpgrade.Count; k1++)
                        {
                            if (_sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[k].AttributeUpgrade[k1].AttributeName == _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].AttributeName)
                                _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[k].AttributeUpgrade.RemoveAt(k1);
                        }
                    }

                    _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes.RemoveAt(i);


                    break;
                }
                EditorGUILayout.EndHorizontal();

                WeaponUpgradesTypes weaponUpgradesTypes = (WeaponUpgradesTypes)EditorGUILayout.EnumPopup("Weapon Upgrade Type", _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].weaponUpgradesTypes);

                EditorGUILayout.Space();

                float MaxValue = EditorGUILayout.FloatField("Max Value", _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].MaxValue);
                float MinValue = EditorGUILayout.FloatField("Min Value", _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].MinValue);

                float CurrentValue = EditorGUILayout.Slider("Current Value", _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].CurrentValue, MinValue, MaxValue);
                EditorGUILayout.EndVertical();

                //END CHANGE CHECK
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "_sc Current Weapons Attributes");
                    _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].AttributeName = AttributeName;
                    _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].weaponUpgradesTypes = weaponUpgradesTypes;

                    _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].MaxValue = MaxValue;
                    _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].MinValue = MinValue;

                    _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].CurrentValue = CurrentValue;

                }
            }

            string s1 = _showUpgrades ? "Hide Upgrades" : "Show Upgrades";
            _showUpgrades = GUILayout.Toggle(_showUpgrades, s1, "Button", GUILayout.Height(35));

            if (_showUpgrades)
                DrawAttributesUpgradesPrice();

            EditorGUILayout.EndVertical();
        }

        void DrawAttributesUpgradesPrice()
        {
            EditorGUILayout.HelpBox("All this Weapon's Upgrades.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Upgrade Level"))
            {
                AllAttributesCommonUpgrade _acu = new AllAttributesCommonUpgrade();

                for (int i = 0; i < _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes.Count; i++)
                {
                    AttributeUpgrade item = new AttributeUpgrade();
                    item.AttributeName = _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].AttributeName;
                    item.weaponUpgradesTypes = _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[i].weaponUpgradesTypes;

                    _acu.AttributeUpgrade.Add(item);
                }

                _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades.Add(_acu);

            }

            if (GUILayout.Button("Clear All"))
            {
                _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades.Clear();
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.LabelField("Upgrade Lvl : " + (i + 1), EditorStyles.centeredGreyMiniLabel);
                if(GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                int price = EditorGUILayout.IntField("Upgrade Price", _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].price);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "_sc price");
                    _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].price = price;
                }

                EditorGUILayout.Space();

                DrawAttributesSpecificUpgrades(i);

                EditorGUILayout.EndVertical();
            }
        }

        void DrawAttributesSpecificUpgrades(int i)
        {
            for (int _au = 0; _au < _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].AttributeUpgrade.Count; _au++)
            {
                EditorGUI.BeginChangeCheck();

                string AttributeName = EditorGUILayout.TextField("Attribute Name", _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].AttributeUpgrade[_au].AttributeName);
                int UpgradeValue = EditorGUILayout.IntField("Upgrade Value", _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].AttributeUpgrade[_au].UpgradeValue);

                if(_au < _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].AttributeUpgrade.Count-1)
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "_sc upgrade val");
                    _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].AttributeUpgrade[_au].AttributeName = AttributeName;
                    _sc.attributesDefinitions[currentSelectedWeapon].AttributeUpgrades[i].AttributeUpgrade[_au].UpgradeValue = UpgradeValue;
                }

            }
        }

        void ManageAttributesAndUpgrades()
        {
            List<string> AttributeName = new List<string>();
            List<WeaponUpgradesTypes> weaponUpgradesTypes = new List<WeaponUpgradesTypes>();

            for(int _wa = 0; _wa < _sc.attributesDefinitions[currentSelectedWeapon].AllAttributes.Count; _wa++)
            {
                AttributeName.Add(_sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[_wa].AttributeName);
                weaponUpgradesTypes.Add(_sc.attributesDefinitions[currentSelectedWeapon].AllAttributes[_wa].weaponUpgradesTypes);
            }
        }
    }
}