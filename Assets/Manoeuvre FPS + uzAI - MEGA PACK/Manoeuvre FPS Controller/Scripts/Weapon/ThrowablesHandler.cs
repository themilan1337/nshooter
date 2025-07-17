using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class ThrowablesHandler : MonoBehaviour
    {
        public int _currentEquipped = 0;
        public GameObject PickupTextPrefab;
        
        public List<Throwables> AllThrowables = new List<Throwables>();

        public UnityEvent OnThrowEvent;

        ManoeuvreFPSController _Controller;
        gc_AmmoManager _ammoManager;

        Transform ThrowablesPool;

        //UI Variables
        Image ThrowablesIcon;
        Text ThrowableItemQuantityText;
        GameObject ThrowablesHUD;

        //UI Canvas element
        GameObject PickupMessagesContainer;

        public static ThrowablesHandler Instance;
        Inventory _inventory;

        public void Awake()
        {
            ///reset equipped ID
            _currentEquipped = 0;

            Instance = this;

            ThrowablesPool = new GameObject().transform;
            ThrowablesPool.name = "ThrowablesPool";
            ThrowablesPool.position = Vector3.zero;
            ThrowablesPool.eulerAngles = Vector3.zero;

            //get UI reference
            PickupMessagesContainer = GameObject.Find("PickupMessagesContainer");
            ThrowablesHUD = GameObject.Find("ThrowablesHUD");
            ThrowablesIcon = GameObject.Find("ThrowablesIcon").GetComponent<Image>();
            ThrowableItemQuantityText = GameObject.Find("ThrowableItemQuantityText").GetComponent<Text>();

        }

        private void Start()
        {
            //get refrences
            _Controller = GetComponent<ManoeuvreFPSController>();
            _ammoManager = gc_AmmoManager.Instance;
            _inventory = FindObjectOfType<Inventory>();

            foreach (Throwables wt in AllThrowables)
            {
                //disable gameobject
                wt.Throwable.gameObject.SetActive(false);
            }

            //init inventory
            InitializeInventory();
        }

        void InitializeInventory()
        {

            for (int i = 0; i < AllThrowables.Count; i++)
            {
                //if we want player to start with this weapon
                if (AllThrowables[i].AddToInventory)
                {
                    //add its slot as well in inventory
                    _inventory.AddInventorySlot_Throwables(AllThrowables[i]._ThrowableItem.ItemName, AllThrowables[i]._ItemIcon);
                }
                else if (!AllThrowables[i].AddToInventory)
                {
                    //make sure it's Quantity is ZERO!
                    AllThrowables[i]._ItemQuantity = 0;
                }
            }

            //now we equip the one we have first in Handler
            for (int i = 0; i < AllThrowables.Count; i++)
            {
                if (AllThrowables[i].AddToInventory)
                {
                    //Select
                    SelectThisItem(AllThrowables[i]._ThrowableItem.ItemName);

                    break; //exit ASAP
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            //if no throwables
            if (AllThrowables.Count < 1)
                return; //exit

            //if not in the inventory
            if (!AllThrowables[_currentEquipped].AddToInventory)
                return;

            //if inventory is open, block throws
            if (_Controller.Inputs.inventoryInput)
                return; //exit

            //if weapon is being reloaded OR being equipped OR already Throwing
            if (_ammoManager.isReloading || _ammoManager.isEquipping || _ammoManager.isThrowing)
                return; //exit
            
            //handle throwing input
            if (_Controller.Inputs.throwItemInput)
                ThrowCurrentEquippedThrowable();

        }

        /// <summary>
        /// Main Method which is used to throw an Item
        /// </summary>
        void ThrowCurrentEquippedThrowable()
        {
            //we can't perform any throw if we have thrown all!
            if (AllThrowables[_currentEquipped]._ItemQuantity < 1)
                return;

            ///consume 1
            AllThrowables[_currentEquipped]._ItemQuantity--;

            //enable the correct throwable object
            AllThrowables[_currentEquipped].Throwable.gameObject.SetActive(true);
            
            //Start the Throw Routine of that object
            AllThrowables[_currentEquipped].Throwable.Throw();

            //throw in reality
            Invoke("SpawnItemInWorldSpace", AllThrowables[_currentEquipped].Throwable.AnimationNormalizedTime);

            //update UI
            UpdateCurrentSelectedThrowableUI(); 

            //invoke event 
            OnThrowEvent.Invoke();
        }

        /// <summary>
        /// This is called after the normalized time of the animation has been passed!
        /// </summary>
        void SpawnItemInWorldSpace()
        {
            //disable renderer as well
            if(AllThrowables[_currentEquipped].Throwable.ItemRenderer)
                AllThrowables[_currentEquipped].Throwable.ItemRenderer.SetActive(false);

            //Instantiate throwable item prefab
            GameObject _ThrowableItem = Instantiate(AllThrowables[_currentEquipped]._ThrowableItem.gameObject, Camera.main.transform.position, Camera.main.transform.rotation);

            //set this as controller
            AllThrowables[_currentEquipped]._ThrowableItem._myController = GetComponent<ManoeuvreFPSController>();

            //make it a child of pool to clear heirarchy
            _ThrowableItem.transform.SetParent(ThrowablesPool);
        
            //throw it!
            Rigidbody _rbody = _ThrowableItem.GetComponent<Rigidbody>();
            _rbody.AddForce(Camera.main.transform.forward * AllThrowables[_currentEquipped]._ThrowForce, ForceMode.VelocityChange);

}

        /// <summary>
        /// Pass an item here and it will be added
        /// </summary>
        /// <param name="_item"></param>
        public void AddItemOnPickup(string _itemName)
        {
            for (int i = 0; i < AllThrowables.Count; i++)
            {
                if (AllThrowables[i]._ThrowableItem.ItemName == _itemName)
                {
                    AllThrowables[i]._ItemQuantity++;
                    ShowPickupMessage(_itemName);
                    UpdateCurrentSelectedThrowableUI();

                    if (!AllThrowables[i].AddToInventory)
                    {
                        AllThrowables[i].AddToInventory = true;
                        //add its slot as well in inventory
                        _inventory.AddInventorySlot_Throwables(AllThrowables[i]._ThrowableItem.ItemName, AllThrowables[i]._ItemIcon);
                    }

                    break;

                }
            }
        }

        /// <summary>
        /// Pass the item name and it will be selected!
        /// </summary>
        /// <param name="_itemName"></param>
        public void SelectThisItem(string _itemName)
        {
            for(int i = 0; i < AllThrowables.Count; i++)
            {
                if (AllThrowables[i]._ThrowableItem.ItemName == _itemName)
                {
                    //just make sure hud alpha is 1
                    ThrowablesHUD.GetComponent<CanvasGroup>().alpha = 1;

                    //set the current equipped index as that
                    _currentEquipped = i;

                    //update ui
                    UpdateCurrentSelectedThrowableUI();
                }
            }
        }

        void UpdateCurrentSelectedThrowableUI()
        {
            //update icon as well
            ThrowablesIcon.sprite = AllThrowables[_currentEquipped]._ItemIcon;

            //update item quantity
            ThrowableItemQuantityText.text = AllThrowables[_currentEquipped]._ItemQuantity.ToString();
        }

        void ShowPickupMessage(string ItemName)
        {
            //show pickup message
            GameObject msg = Instantiate(PickupTextPrefab);
            msg.GetComponent<UnityEngine.UI.Text>().text = "Picked " + ItemName;

            //init scale and pos
            msg.transform.SetParent(PickupMessagesContainer.transform);
            msg.transform.localPosition = Vector3.zero;
            msg.transform.localScale = Vector3.one;
            msg.transform.localEulerAngles = Vector3.zero;

            //destroy msg
            Destroy(msg, 1f);

        }

    }

    [System.Serializable]
    public class Throwables
    {
        public string ItemName;
        public WeaponThrowable Throwable;
        public ThrowableItem _ThrowableItem;
        public float _ThrowForce = 25f;
        public int _ItemQuantity = 10;
        public Sprite _ItemIcon;

        public bool AddToInventory = true;
    }
}