using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    [RequireComponent(typeof(SphereCollider))]
    public class uzAIZombieFood : MonoBehaviour
    {

        public enum FoodAvailability { Available, UnAvailable }

        [Header("Is Food Available")]
        public FoodAvailability _availability = FoodAvailability.Available;

        public uzAIZombieStateManager _Zombie;

        public void ToggleFoodAvailability(FoodAvailability _a)
        {
            _availability = _a;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(_availability == FoodAvailability.UnAvailable)
            {
                if (other.tag == "uzAIZombie")
                    _Zombie = other.GetComponent<uzAIZombieStateManager>();
            }
        }

        //private void OnTriggerExit(Collider other)
        //{
        //    if (_availability == FoodAvailability.UnAvailable)
        //    {
        //        if (other.tag == "uzAIZombie")
        //        {
        //            _availability = FoodAvailability.Available;
        //            if (_Zombie)
        //            {
        //                _Zombie.eatingBehaviour.currentFoodSource = null;
        //                _Zombie = null;
        //            }
        //        }
        //    }
        //}

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
        }

    }
}