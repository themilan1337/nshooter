using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class ThrowableItem_Pickup : MonoBehaviour
    {
        public string itemName;
        public AudioClip pickupSound;

        bool isAdded;

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.tag == "Player")
            {
                AddItemToThrowablesHandler();
            }
        }

        void AddItemToThrowablesHandler()
        {
            if (isAdded)
                return;

            isAdded = true;
            ThrowablesHandler.Instance.AddItemOnPickup(itemName);

            //play sound fx
            if(pickupSound)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}