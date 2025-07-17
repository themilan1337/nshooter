using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class Inventory : MonoBehaviour
    {
        [HideInInspector]
        public bool inventoryIsOpen;

        public bool PauseGameWhileOpen = true;

        [Header("-- UI References --")]
        public InventoryUI _InventoryUI;

        WeaponHandler _WeaponHandler;
        ThrowablesHandler _ThrowablesHandler;

        public static Inventory Instance;

        private void Awake()
        {
            Instance = this;
            _InventoryUI.Initialize();
        }

        // Use this for initialization
        void Start()
        {
            _WeaponHandler = FindObjectOfType<WeaponHandler>();
            _ThrowablesHandler = FindObjectOfType<ThrowablesHandler>();

            OnCLick_Weapons();
        }

        private void Update()
        {
            inventoryIsOpen = ManoeuvreFPSController.Instance.Inputs.inventoryInput && !GameOver.Instance.isGameOver;

            //As long as we press Inventory Button
            //Inventory will remain open
            if (inventoryIsOpen)
            {
                if(PauseGameWhileOpen)
                    Time.timeScale = 00.0000000001f;

                _InventoryUI.InventoryCanvas.SetActive(true);
                _InventoryUI.InventoryCanvas.GetComponent<CanvasGroup>().alpha = 1;
            }
            else
            {
                if(PauseGameWhileOpen)
                    Time.timeScale = 1;

                _InventoryUI.InventoryCanvas.SetActive(false);
                _InventoryUI.InventoryCanvas.GetComponent<CanvasGroup>().alpha = 0;
            }

        }

        public void OnCLick_Weapons()
        {
            _InventoryUI.WeaponsButton.alpha = 1f;
            _InventoryUI.PowerupsButton.alpha = 0.5f;
            _InventoryUI.ThrowablesButton.alpha = 0.5f;

            UpdateInventorySlots_Weapons();

            //make sure right one is active and set as scroller
            _InventoryUI.SlotsContainer_Weapons.gameObject.SetActive(true);
            _InventoryUI.SlotsContainer_Powerups.gameObject.SetActive(false);
            _InventoryUI.SlotsContainer_Throwables.gameObject.SetActive(false);

            _InventoryUI.InventoryCanvas.GetComponentInChildren<ScrollRect>().content = _InventoryUI.SlotsContainer_Weapons;

        }

        public void OnCLick_Powerups()
        {
            _InventoryUI.WeaponsButton.alpha = 0.5f;
            _InventoryUI.PowerupsButton.alpha = 1f;
            _InventoryUI.ThrowablesButton.alpha = 0.5f;

            //make sure right one is active and set as scroller
            _InventoryUI.SlotsContainer_Weapons.gameObject.SetActive(false);
            _InventoryUI.SlotsContainer_Powerups.gameObject.SetActive(true);
            _InventoryUI.SlotsContainer_Throwables.gameObject.SetActive(false);

            _InventoryUI.InventoryCanvas.GetComponentInChildren<ScrollRect>().content = _InventoryUI.SlotsContainer_Powerups;
        }

        public void OnCLick_Throwables()
        {
            _InventoryUI.WeaponsButton.alpha = 0.5f;
            _InventoryUI.PowerupsButton.alpha = 0.5f;
            _InventoryUI.ThrowablesButton.alpha = 1f;

            //make sure right one is active and set as scroller
            _InventoryUI.SlotsContainer_Weapons.gameObject.SetActive(false);
            _InventoryUI.SlotsContainer_Powerups.gameObject.SetActive(false);
            _InventoryUI.SlotsContainer_Throwables.gameObject.SetActive(true);

            _InventoryUI.InventoryCanvas.GetComponentInChildren<ScrollRect>().content = _InventoryUI.SlotsContainer_Throwables;
        }

        public void UpdateInventorySlots_Weapons()
        {
            //resize the scroll UI
          _InventoryUI.ResizeSlotsContainer_Weapons(_WeaponHandler.Weapons.Count);

            //clear previous slots if any
            _InventoryUI.ClearSLots();
            
            //instantiate correct number of slots
            for (int i = 0; i< _WeaponHandler.Weapons.Count; i++)
            {
                //now Instantiate Slots
                GameObject slot = Instantiate(_InventoryUI.slotPrefab_Weapons) as GameObject;
                slot.transform.SetParent(_InventoryUI.SlotsContainer_Weapons);
                slot.transform.localScale = Vector3.one;
                slot.transform.localPosition = Vector3.zero;
                slot.transform.localEulerAngles = Vector3.zero;
                slot.name = _WeaponHandler.Weapons[i].WeaponName;

                slot.GetComponent<ItemIdentifier_Weapons>().Weapon_ID = _WeaponHandler.Weapons[i].Weapon_ID;
                slot.GetComponent<ItemIdentifier_Weapons>().SetUI(_WeaponHandler.Weapons[i].UIIcon);

                //add it in the list
                _InventoryUI.allSlots_Weapons.Add(slot);

            }
        }

        /// <summary>
        /// This is used to add item to inventory as soon as it has been picked up!
        /// </summary>
        /// <param name="Weapon_ID"></param>
        /// <param name="UIIcon"></param>
        public void AddInventorySlot_Weapons(int Weapon_ID, Sprite UIIcon, string wpnName)
        {
            GameObject slot = Instantiate(_InventoryUI.slotPrefab_Weapons) as GameObject;
            slot.transform.SetParent(_InventoryUI.SlotsContainer_Weapons);
            slot.transform.localScale = Vector3.one;
            slot.transform.localPosition = Vector3.zero;
            slot.transform.localEulerAngles = Vector3.zero;
            slot.name = wpnName;

            slot.GetComponent<ItemIdentifier_Weapons>().Weapon_ID = Weapon_ID;
            slot.GetComponent<ItemIdentifier_Weapons>().SetUI(UIIcon);

            //add it in the list
            _InventoryUI.allSlots_Weapons.Add(slot);

            //resize the scroll UI
            _InventoryUI.ResizeSlotsContainer_Weapons(_InventoryUI.allSlots_Weapons.Count);

        }

        public ItemIdentifier_Powerups AddInventorySlot_Powerups(PowerupType _PowerupType)
        {
            GameObject slot = Instantiate(_InventoryUI.slotPrefab_Powerups) as GameObject;
            slot.transform.SetParent(_InventoryUI.SlotsContainer_Powerups);
            slot.transform.localScale = Vector3.one;
            slot.transform.localPosition = Vector3.zero;
            slot.transform.localEulerAngles = Vector3.zero;
            slot.name = _PowerupType.ToString();

            //set the type of power being added
            slot.GetComponent<ItemIdentifier_Powerups>()._PowerupType = _PowerupType;

            //add it in the list
            _InventoryUI.allSlots_Powerups.Add(slot);

            //resize the scroll UI
            _InventoryUI.ResizeSlotsContainer_Powerups(_InventoryUI.allSlots_Powerups.Count);

            return slot.GetComponent<ItemIdentifier_Powerups>();
        }

        /// <summary>
        /// This is used to add item to inventory as soon as Game starts
        /// </summary>
        public void AddInventorySlot_Throwables(string _itemName, Sprite UIIcon)
        {
            GameObject slot = Instantiate(_InventoryUI.slotPrefab_Throwables) as GameObject;
            slot.transform.SetParent(_InventoryUI.SlotsContainer_Throwables);
            slot.transform.localScale = Vector3.one;
            slot.transform.localPosition = Vector3.zero;
            slot.transform.localEulerAngles = Vector3.zero;
            slot.name = _itemName;

            slot.GetComponent<ItemIdentifier_Throwables>().itemName = _itemName;
            slot.GetComponent<ItemIdentifier_Throwables>().SetUI(UIIcon);

            //add it in the list
            _InventoryUI.allSlots_Throwables.Add(slot);

            //resize the scroll UI
            _InventoryUI.ResizeSlotsContainer_Throwables(_InventoryUI.allSlots_Throwables.Count);

        }

        /// <summary>
        /// This will update the Selected item in Throwables Handler
        /// </summary>
        public void OnSelectThrowableItem(string _itemName)
        {
            _ThrowablesHandler.SelectThisItem(_itemName);
        }
    }

    [System.Serializable]
    public class InventoryUI
    {
        [HideInInspector]
        public GameObject InventoryCanvas;
        [Space(5)]
        [HideInInspector]
        public RectTransform SlotsContainer_Weapons;
        [HideInInspector]
        public RectTransform SlotsContainer_Powerups;
        [HideInInspector]
        public RectTransform SlotsContainer_Throwables;
        [Space(5)]
        [HideInInspector]
        public CanvasGroup WeaponsButton;
        [HideInInspector]
        public CanvasGroup PowerupsButton;
        [HideInInspector]
        public CanvasGroup ThrowablesButton;

        [Space(5)]
        public GameObject slotPrefab_Weapons;
        public GameObject slotPrefab_Powerups;
        public GameObject slotPrefab_Throwables;

        [HideInInspector]
        public List<GameObject> allSlots_Weapons = new List<GameObject>();

        [HideInInspector]
        public List<GameObject> allSlots_Powerups = new List<GameObject>();

        [HideInInspector]
        public List<GameObject> allSlots_Throwables = new List<GameObject>();

        public void Initialize()
        {
            InventoryCanvas = GameObject.Find("Inventory");

            SlotsContainer_Weapons = InventoryCanvas.transform.Find("ItemContainer/SlotsContainer_Weapons").GetComponent<RectTransform>();
            SlotsContainer_Powerups = InventoryCanvas.transform.Find("ItemContainer/SlotsContainer_Powerups").GetComponent<RectTransform>();
            SlotsContainer_Throwables = InventoryCanvas.transform.Find("ItemContainer/SlotsContainer_Throwables").GetComponent<RectTransform>();

            WeaponsButton = InventoryCanvas.transform.Find("WeaponsButton").GetComponent<CanvasGroup>();
            PowerupsButton = InventoryCanvas.transform.Find("PowerupsButton").GetComponent<CanvasGroup>();
            ThrowablesButton = InventoryCanvas.transform.Find("ThrowablesButton").GetComponent<CanvasGroup>();

            WeaponsButton.GetComponent<Button>().onClick.AddListener(Inventory.Instance.OnCLick_Weapons);
            PowerupsButton.GetComponent<Button>().onClick.AddListener(Inventory.Instance.OnCLick_Powerups);
            ThrowablesButton.GetComponent<Button>().onClick.AddListener(Inventory.Instance.OnCLick_Throwables);

            InventoryCanvas.SetActive(false);

        }

        public void ResizeSlotsContainer_Weapons(int slotsCount)
        {
            float newWidth = (float)(slotsCount * (SlotsContainer_Weapons.GetComponent<GridLayoutGroup>().cellSize.x + 20));

            SlotsContainer_Weapons.sizeDelta = new Vector2(newWidth, SlotsContainer_Weapons.sizeDelta.y);
        }

        public void ResizeSlotsContainer_Powerups(int slotsCount)
        {
            float newWidth = (float)(slotsCount * (SlotsContainer_Powerups.GetComponent<GridLayoutGroup>().cellSize.x + 20));

            SlotsContainer_Powerups.sizeDelta = new Vector2(newWidth, SlotsContainer_Powerups.sizeDelta.y);
        }

        public void ResizeSlotsContainer_Throwables(int slotsCount)
        {
            float newWidth = (float)(slotsCount * (SlotsContainer_Throwables.GetComponent<GridLayoutGroup>().cellSize.x + 20));

            SlotsContainer_Throwables.sizeDelta = new Vector2(newWidth, SlotsContainer_Throwables.sizeDelta.y);
        }

        public void ClearSLots()
        {
            for(int i = 0;i< allSlots_Weapons.Count; i++)
            {
                GameObject.Destroy(allSlots_Weapons[i].gameObject);
            }

            allSlots_Weapons.Clear();
        }
    }

}