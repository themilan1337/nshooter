using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Manoeuvre
{

    public class ThrowableItem : MonoBehaviour
    {
        public string ItemName;
        public float affectRadius = 15f;

        public LayerMask _hitMask;
        public LayerMask _obstacleMask;

        public ParticleSystem DetonateFX;
        public AudioClip DetonateSFX;
        public AudioClip AttractSoundSFX;
        public float DetonateDelay = 5f;
        public float forceAmtOnRigidbodies = 25f;
        public int damage = 100;

        //if true, kills on detonate else, apply defined damage
        public bool KillAllNearby = true;
        //stick to surface on collision
        public bool isSticky;
        //attract nearby AIs
        public bool isSoundAttractor;
        //should this explode too?
        public bool shouldDetonate = true;
        //destroy on detonate?
        public bool destroyObjectOnDetonate;
        //can we hit ourselves
        public bool canHitOurself = true;

        [HideInInspector]
        public ManoeuvreFPSController _myController;

        /// <summary>
        /// Handy On Detonate Event
        /// </summary>
        public UnityEvent OnDetonateEvent;

        bool hasStucked;
        bool hasMadeSound;
        Transform collidedWith = null;
        ConfigurableJoint _cj;
     

        // Use this for initialization
        void Start()
        {
            //we don't detonate this on start if this is sticky item
            if(!isSticky && shouldDetonate )
                Invoke("OnDetonate", DetonateDelay);

            //we will destroy this object anyway in 20 seconds
            Destroy(gameObject, DetonateDelay+20);

            //add dont go through things utility Script
            gameObject.AddComponent<DontGoThroughThings>();

        }

        /// <summary>
        /// If this item is sticky, it will stick to whatever it collides with!
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            if (isSticky)
                StickyItem(collision);

            if (isSoundAttractor)
                SoundAttractorItem(collision);

        }

        void StickyItem(Collision collision)
        {
            if (hasStucked)
            {
                GetComponent<Rigidbody>().isKinematic = false;
                return;
            }

            if (collision.transform.tag != "Player")
                collidedWith = collision.transform;

            if (collidedWith)
            {
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Collider>().isTrigger = true;

                if (collidedWith.GetComponent<Rigidbody>())
                {
                    AddConfigurableJoint();
                }
                else
                {
                    collidedWith.gameObject.AddComponent<Rigidbody>().isKinematic = true;

                    AddConfigurableJoint();

                }

                GetComponent<Rigidbody>().isKinematic = false;

                //now we invoke our method
                if(shouldDetonate)
                    Invoke("OnDetonate", DetonateDelay);

                hasStucked = true;
                gameObject.layer = LayerMask.NameToLayer("Default");

                if (GetComponentInChildren<ParticleSystem>())
                    GetComponentInChildren<ParticleSystem>().Stop();
            }
        }

        void SoundAttractorItem(Collision collision)
        {
            if (hasMadeSound)
                return;

            if (collision.transform.tag != "Player")
                collidedWith = collision.transform;

            if (collidedWith)
            {
                //enable the flag
                hasMadeSound = true;
                
                //call an audio target
                gameObject.AddComponent<uzAI.AudioTarget>().clipToPlay = AttractSoundSFX;
                GetComponent<uzAI.AudioTarget>().AudioRange = affectRadius;
                GetComponent<uzAI.AudioTarget>().EnableAudioTarget();

                //now we invoke our method
                if (shouldDetonate)
                    Invoke("OnDetonate", DetonateDelay);

                gameObject.layer = LayerMask.NameToLayer("Default");

                if (GetComponentInChildren<ParticleSystem>())
                    GetComponentInChildren<ParticleSystem>().Stop();
            }
        }

        void OnDetonate()
        {
            //detach this item if it was stucked to destructible prop
            if (collidedWith)
            {
                if (collidedWith.GetComponent<DestructibleProps>() || collidedWith.GetComponent<uzAI.uzAIZombieStateManager>() || collidedWith.GetComponent<ShooterAIStateManager>())
                {
                    //enable collider
                    GetComponent<Collider>().isTrigger = false; 

                    //enable physics
                    GetComponent<Rigidbody>().isKinematic = false;
                    
                    //detach
                    transform.SetParent(null);
                    
                    //destroy joint
                    if(_cj)
                        Destroy(_cj);
                }
            }

            //play p fx
            GameObject fx = null;
            if (DetonateFX)
                fx = Instantiate(DetonateFX.gameObject, transform.position, transform.rotation);

            //play sfx
            if (DetonateSFX)
            {
                if (fx)
                {
                    fx.gameObject.AddComponent<AudioSource>().maxDistance = affectRadius;
                    fx.GetComponent<AudioSource>().PlayOneShot(DetonateSFX);
                }
                else
                    AudioSource.PlayClipAtPoint(DetonateSFX, transform.position, 1);

            }

            //get nearby colliders
            Collider[] allNearbyColliders = Physics.OverlapSphere(transform.position, affectRadius, _hitMask);

            for (int i = 0; i < allNearbyColliders.Length; i++)
            {
                if (!allNearbyColliders[i].GetComponent<ThrowableItem>())
                {
                    //add force to rigidbodies
                    if (allNearbyColliders[i].GetComponent<Rigidbody>())
                        allNearbyColliders[i].GetComponent<Rigidbody>().AddExplosionForce(forceAmtOnRigidbodies, transform.position, affectRadius);

                    if (allNearbyColliders[i].GetComponent<DestructibleProps>())
                        allNearbyColliders[i].GetComponent<DestructibleProps>().OnExplosion(forceAmtOnRigidbodies, transform.position, affectRadius);

                    //check for uzAI
                    if (allNearbyColliders[i].GetComponent<uzAI.uzAIZombieStateManager>())
                    {
                        //we get the direction
                        Vector3 dirToTarget = (allNearbyColliders[i].transform.position - transform.position).normalized;

                        //we see the distance of this target
                        //from our position
                        float distToTarget = Vector3.Distance(transform.position, allNearbyColliders[i].transform.position);

                        //if there's no obstacle in between
                        if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, _obstacleMask))
                        {
                            //we apply damage to the zombie
                            if (KillAllNearby)
                                allNearbyColliders[i].GetComponent<uzAI.uzAIZombieStateManager>().PerformRagdollDeath(forceAmtOnRigidbodies, transform.position, affectRadius);
                            else
                                allNearbyColliders[i].GetComponent<uzAI.uzAIZombieStateManager>().ZombieHealthStats.onDamage(damage); 
                        }

                    }

                    //check for Shooter AI
                    if (allNearbyColliders[i].GetComponent<ShooterAIStateManager>())
                    {
                        //else
                        //we get the direction
                        Vector3 dirToTarget = (allNearbyColliders[i].transform.position - transform.position).normalized;

                        //we see the distance of this target
                        //from our position
                        float distToTarget = Vector3.Distance(transform.position, allNearbyColliders[i].transform.position);

                        //if there's no obstacle in between
                        if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, _obstacleMask))
                        {
                            //we apply damage to the AI
                            if (KillAllNearby)
                                allNearbyColliders[i].GetComponent<ShooterAIStateManager>().Die();
                            else
                                allNearbyColliders[i].GetComponent<ShooterAIStateManager>().Health.onDamage(damage);

                        }

                    }

                    //check for main controller
                    if (allNearbyColliders[i].GetComponent<ManoeuvreFPSController>())
                    {
                        //make sure we don't apply damage to ourselves
                        if (_myController != allNearbyColliders[i].GetComponent<ManoeuvreFPSController>() || canHitOurself)
                        {
                            //else
                            //we get the direction
                            Vector3 dirToTarget = (allNearbyColliders[i].transform.position - transform.position).normalized;

                            //we see the distance of this target
                            //from our position
                            float distToTarget = Vector3.Distance(transform.position, allNearbyColliders[i].transform.position);

                            //if there's no obstacle in between
                            if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, _obstacleMask))
                            {
                                //we apply damage to the zombie
                                allNearbyColliders[i].GetComponent<ManoeuvreFPSController>().Die();

                            }

                        }

                    }
                }
               
            }

            //call any event that has been added by user
            OnDetonateEvent.Invoke();

            Destroy(fx, 2f);

            //destroy gameobject
            if (destroyObjectOnDetonate)
                Destroy(gameObject);

            if (_cj)
                Destroy(_cj, DetonateDelay+20);

        }

        void AddConfigurableJoint()
        {
            _cj = collidedWith.gameObject.AddComponent<ConfigurableJoint>();
            _cj.xMotion = ConfigurableJointMotion.Locked;
            _cj.yMotion = ConfigurableJointMotion.Locked;
            _cj.zMotion = ConfigurableJointMotion.Locked;
            _cj.angularXMotion = ConfigurableJointMotion.Locked;
            _cj.angularYMotion = ConfigurableJointMotion.Locked;
            _cj.angularZMotion = ConfigurableJointMotion.Locked;
            _cj.projectionMode = JointProjectionMode.PositionAndRotation;
            _cj.projectionDistance = 0;
            _cj.projectionAngle = 0;

            _cj.connectedBody = transform.GetComponent<Rigidbody>();

        }

        public void SetGlobalScale(Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, affectRadius);
        }

    }
}