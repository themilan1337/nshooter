using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Manoeuvre
{
    public class ItemIdentifier_Throwables : MonoBehaviour
    {
        public string itemName;

        Inventory _Inventory;

        // Use this for initialization
        void Start()
        {
            _Inventory = FindObjectOfType<Inventory>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// We will select which is the current selected throwable item and also update its UI icon
        /// </summary>
        public void OnItemSelect()
        {
            _Inventory.OnSelectThrowableItem(itemName);
        }


        public void SetUI(Sprite icon)
        {
            Image Icon = transform.Find("Icon").GetComponent<Image>();
            Icon.sprite = icon;
        }

    }

}