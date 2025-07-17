using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    [RequireComponent(typeof(Rigidbody))]
    public class WeaponProjectile : MonoBehaviour
    {
        public float forceAmt;
        public BulletHits bulletHitProperties;

        Rigidbody _rBody;
        Collider _col;
        WeaponShooter _shooterWeapon;
        Camera weaponCam;
        Transform muzzleFlashLoc;

        /// <summary>
        /// Init this Bullet
        /// </summary>
        public void Initialize(float force, BulletHits hits, WeaponShooter shooter)
        {
            forceAmt = force;
            bulletHitProperties = hits;

            _shooterWeapon = shooter;
            _rBody = GetComponent<Rigidbody>();
            _rBody.isKinematic = false;

            _col = GetComponent<Collider>();
            _col.isTrigger = true;

            weaponCam = shooter.weaponCam;
            muzzleFlashLoc = _shooterWeapon.muzzleProperties.muzzleFlashLocation;

            ///add back to queue in 5 seconds
            Invoke("AddBackToQueue", 5f);
        }

        private void OnTriggerEnter(Collider other)
        {
            //make sure we didn't hit ourself
            if (other.gameObject == _shooterWeapon.GetComponentInParent<ManoeuvreFPSController>().gameObject)
                return;

            //stop projectile there only
          //  _rBody.isKinematic = true;

            //draw ray 
            RaycastHit hit = new RaycastHit();
            if(Physics.Raycast(_shooterWeapon.muzzleProperties.muzzleFlashLocation.position, transform.position - _shooterWeapon.muzzleProperties.muzzleFlashLocation.position, out hit, 1000))
            {
                _shooterWeapon.OnHit(hit);
            }

            AddBackToQueue();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(!_rBody.isKinematic)
                //add force to this Bullet
                _rBody.AddForce(muzzleFlashLoc.forward * forceAmt);
        }

        void AddBackToQueue()
        {
            //reset pos and rotation
            transform.localEulerAngles = Vector3.zero;
            transform.localPosition = Vector3.zero;
            _rBody.isKinematic = true;

            //Add back to queue
            ProjectilesPooler.Instance.AddBackToQueue(this.gameObject, "Bullet");
        }
    }
}