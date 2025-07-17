using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    [RequireComponent(typeof(BoxCollider))]
    public class AIDamageRegion : MonoBehaviour
    {

        public int DamagePerSecond = 2;

        float _timer = 1;

        private void Start()
        {
            //init Timer
            _timer = 1;
        }

        private void OnTriggerStay(Collider other)
        {
            
            if(other.tag == "uzAIZombie")
            {
                if (other.GetComponent<uzAIZombieStateManager>())
                    GiveDamage(other.GetComponent<uzAIZombieStateManager>().ZombieHealthStats);
                //other.GetComponent<uzAIZombieStateManager>().ZombieHealthStats.onDamage(DamagePerSecond);
            }

        }

        void GiveDamage(uzAIZombieHealth Health)
        {
            if (_timer >= 1)
            {
                Health.onDamage(DamagePerSecond);
                _timer = 0;
            }
            else
                _timer += Time.deltaTime;
        }

    }
}