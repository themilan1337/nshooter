using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class TurretProjectile : MonoBehaviour
    {
        Collider _col;
        Rigidbody _rbody;
        ParticleSystem explodefx;
        AudioClip explosionSFX;
        Transform Target;
        float force;
        float rotateAmount;
        int Damage;

        // Use this for initialization
        IEnumerator InitializeReferences()
        {
            _col = GetComponent<Collider>();
            _col.isTrigger = true;

            _rbody = GetComponent<Rigidbody>();


            //We will explode this Projectile in 10 Seconds if 
            //it goes up in sky or don't collide with anything
            yield return new WaitForSeconds(10f);
            Explode();

        }

        public void InitializeProjectile(float forwardForce, float rotateAmt, ParticleSystem _explodeFX, AudioClip _explodeSFX, int damageToGive, Transform target)
        {
            //Init references
            StartCoroutine(InitializeReferences());

            //Get Force in Forward Direction
            force = forwardForce;

            //Get Rotation Amount
            rotateAmount = rotateAmt;

            //Init the explode fx 
            if (_explodeFX)
            {
                explodefx = Instantiate(_explodeFX);
                explodefx.Stop();
                explodefx.transform.SetParent(this.transform);
                explodefx.transform.localPosition = Vector3.zero;
                explodefx.transform.localEulerAngles = Vector3.zero;
            }

            //init audio clip
            if(explosionSFX)
                explosionSFX = _explodeSFX;

            //init Damage
            Damage = damageToGive;

            //init target
            if(target)
                Target = target;

        }

        private void Update()
        {
            if (Target)
            {
                Quaternion destRotation = Quaternion.LookRotation(Target.position - transform.position, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, destRotation, rotateAmount);

            }

            //Adding Force in the Forward Direction
            _rbody.AddForce(transform.forward * force);

        }

        private void OnTriggerEnter(Collider other)
        {
            //We don't care what are we colliding with,
            //we just simply
            //EXPLODE!!!!
            Explode();

            //give damage
            if(other.tag == "Player")
            {
                other.GetComponent<ManoeuvreFPSController>().Health.OnDamage(Damage);
            }
            else if(other.tag == "uzAIZombie")
            {
                other.GetComponent<uzAI.uzAIZombieStateManager>().ZombieHealthStats.onDamage(Damage);
            }
			else if (other.tag == "ShooterAI") {

				other.GetComponent<ShooterAIStateManager>().Health.onDamage(Damage);
			}
        }

        /// <summary>
        /// Make this method public so that any one can explode this projectile
        /// e.g our bullet
        /// </summary>
        public void Explode()
        {

            //release explode fx
            if (explodefx)
            {
                explodefx.transform.SetParent(null);

                //play the fx
                explodefx.Play();

                //Destroy coroutine
                Destroy(explodefx.gameObject, 3f);
            }

            if(explosionSFX)
                //Play SFX
                AudioSource.PlayClipAtPoint(explosionSFX, transform.position);

           

            //Add back to queue
            ProjectilesPooler.Instance.AddBackToQueue(this.gameObject, "Rocket");
        }

    }
}