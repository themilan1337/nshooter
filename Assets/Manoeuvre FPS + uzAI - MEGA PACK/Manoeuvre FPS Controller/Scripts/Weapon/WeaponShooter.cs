using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public enum WeaponState { Idle, Firing, Reloading }

    //[HelpURL("Watch Tutorial!")]
    public class WeaponShooter : MonoBehaviour
    {
        [Tooltip("This is the Identification of the Weapon, if you change the Weapon's name, you have to reassign it's position and rotation!")]
        public string WeaponName = "Weapon 1";

        public WeaponState weaponState = WeaponState.Idle;

        public ShootingType ShootingType = ShootingType.Raycast;

        [HideInInspector]
        public Camera weaponCam;

        [Space(5)]

        //[HideInInspector]
        public Shoot shooterProperties;
        [HideInInspector]
        public AddForceToRigidbodies addForceProperties;
        [HideInInspector]
        public MuzzleFlash muzzleProperties;
        //[HideInInspector]
        public BulletHits bulletHitProperties;
        [HideInInspector]
        public RecoilSpring recoilProperties;
        [HideInInspector]
        public WeaponSounds weaponSoundProperties;
        [HideInInspector]
        public IronSight ironSightProperties;
        // public WeaponShells weaponShellProperties;

        ProceduralReload _weaponReload;
        ProceduralEquipSway _equipSway;
        [HideInInspector]
        public int Weapon_ID;
        [HideInInspector]
        public WeaponHandler _weaponHandler;

        //Editor Variable
        [HideInInspector]
        public int propertyTab1;
        [HideInInspector]
        public int propertyTab2;
        [HideInInspector]
        public string propertyName;

        // Use this for initialization
        void Start()
        {
            //initialize
            muzzleProperties.SpawnMuzzleFlash();
            gameObject.AddComponent<AudioSource>();
            weaponSoundProperties.source = GetComponent<AudioSource>();
            ironSightProperties.Init(Camera.main.fieldOfView, Time.timeScale, this.transform);
            _weaponHandler = GetComponentInParent<WeaponHandler>();
            weaponCam = GetComponentInParent<Camera>();
            recoilProperties.Initialize(transform, this, GetComponent<WeaponProceduralManoeuvre>());


            //get procedural Reload ref
            _weaponReload = GetComponent<WeaponProceduralManoeuvre>()._weaponReload;
            _equipSway = GetComponent<WeaponProceduralManoeuvre>()._equipSway;
            _equipSway.equipSound = weaponSoundProperties.equipSound;
            _equipSway.weaponSoundProperties = weaponSoundProperties;

            //set hear range
            gc_StateManager.Instance.radiusWhileShooting = shooterProperties.HearRange;
        }

        void FixedUpdate()
        {
            //increment timer every frame
            shooterProperties._timer += Time.deltaTime;

            //if we don't have equip sway,
            //iterator means  above initialize method haven't been invoked yet!
            if (_equipSway == null)
                return;

            //if we are in the equipment sway coroutine
            if (_equipSway.isEquipSway)
                return; // exit

            //make sure gun is lerping back to original rotation
            recoilProperties.returnToRot_Shooter();

            //as soon as player presses shoot key
            if (ManoeuvreFPSController.Instance.Inputs.shootInput)
                //SHOOT
                Shoot();

            //ironsight if not reloading or changing weapon
            if (ironSightProperties.useIronsight)
            {
                StartCoroutine(ironSightProperties.tweenIronSight(ManoeuvreFPSController.Instance.Inputs.ironsightInput
                                                                  && !gc_AmmoManager.Instance.isReloading
                                                                  && !gc_AmmoManager.Instance.isEquipping
                                                                  && !ManoeuvreFPSController.Instance.Inputs.inventoryInput));
            }

            //Manage State
            SetWeaponState();
        }

        /// <summary>
        /// Make this weapon Shoot!
        /// </summary>
        void Shoot()
        {

            //if Inventory is open
            if (Inventory.Instance.inventoryIsOpen)
                return;

            //if we are currrently reloading
            if (_weaponReload.currentlyReloading)
                return;

            //if the timer less then the fire rate
            if (shooterProperties._timer < (1 / shooterProperties.fireRate))
                //don't proceed
                return;
            //else continue with the shoot code

            //check if we can shoot
            shooterProperties.canShoot = shooterProperties.currentAmmo > 0 ? true : false;

            //exit if we can't shoot
            if (!shooterProperties.canShoot)
                return;

            //as soon as weapon ammo count reaches 0
            if (shooterProperties.currentAmmo == 0)
            {
                //disable shoot for the moment
                shooterProperties.canShoot = false;

                //see if we can reload
                shooterProperties.currentAmmo = gc_AmmoManager.Instance.ReloadAmmo();

                //if we retrieve a value > 0
                //it means we can reload
                if (shooterProperties.currentAmmo > 0)
                {
                    //Start Reloading
                    gc_AmmoManager.Instance.ReloadCurrentWeapon();
                    StartCoroutine(ironSightProperties.tweenIronSight(false));

                    return;
                }

            }

            //reduce ammo count and retrieve current ammo
            shooterProperties.currentAmmo = gc_AmmoManager.Instance.ReduceAmmo();

            //reset timer
            shooterProperties._timer = 0;

            //play muzzle flash whenever player shooots
            muzzleProperties.PlayMuzzleFlash();

            //recoil gun
            WeaponRecoil();

            //play sound
            weaponSoundProperties.PlayAudio(weaponSoundProperties.fireSound, true, transform.position);

            //spawn shell
            // weaponShellProperties.SpawnShells();

            //SHOOT
            ShootWeapon();

        }

        void ShootWeapon()
        {
            switch (ShootingType)
            {
                case ShootingType.Raycast:
                    ShootVIARaycast();
                    break;

                case ShootingType.Projectile:
                    ShootVIAProjectile();
                    break;
            }
        }

        /// <summary>
        /// Shooting Weapon Based on Raycast
        /// </summary>
        void ShootVIARaycast()
        {
            //send a ray of specified ShootRange
            RaycastHit hit;
            if (Physics.Raycast(weaponCam.transform.position, weaponCam.transform.forward, out hit, shooterProperties.ShootRange, shooterProperties.hitMask))
            {
                OnHit(hit);

                //shake camera
                if (shooterProperties.shakeCamera)
                    StartCoroutine(Camera.main.GetComponent<CameraController>().ShakeCamera((1 / shooterProperties.fireRate),
                        shooterProperties.shakeAmount));
            }
        }

        /// <summary>
        /// Shooting Weapon Based on Projectiles
        /// </summary>
        void ShootVIAProjectile()
        {

            GameObject projectile = ProjectilesPooler.Instance.SpawnFromPool(shooterProperties.ProjectileName, muzzleProperties.muzzleFlashLocation.position, Quaternion.identity);
            //no spread if single projectile
            if(projectile.GetComponent<WeaponProjectile>())
                projectile.GetComponent<WeaponProjectile>().Initialize(shooterProperties.ProjectileForce, bulletHitProperties, this);

            if (projectile.GetComponent<TurretProjectile>())
                projectile.GetComponent<TurretProjectile>().InitializeProjectile(shooterProperties.ProjectileForce, 0, null, null, 500, null);
        }

        public void OnHit(RaycastHit hit)
        {
            //calling the serialized class method  
            //it requires following parameters
            //1. the tag of the object you just hit
            //2. the Vector3 location of the exact point where we hit i.e hit.point
            //3. to make it look good, we are sending perpendicular direction of the Hit point
            bulletHitProperties.SpawnBulletHits(hit.transform, hit.point, hit.normal);

            //also cause damage 
            bulletHitProperties.GiveDamage(hit.transform, this.transform);

            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if (rb && addForceProperties.applyForce)
            {
                rb.AddForce(weaponCam.transform.forward * addForceProperties.forceAmount, ForceMode.Force);
            }
        }

        void SetWeaponState(){

            //set weapon state
            weaponState = ManoeuvreFPSController.Instance.Inputs.shootInput ? WeaponState.Firing : WeaponState.Idle;

            //set reload state
            if (gc_AmmoManager.Instance.isReloading)
                weaponState = WeaponState.Reloading;

            //update state in game controller
            gc_StateManager.Instance.currentWeaponState = weaponState;

        }

        void WeaponRecoil()
        {
            switch (recoilProperties.RecoilType)
            {
                case AnimationType.Animation:
					//set animation speed
					recoilProperties.RecoilAnimation[recoilProperties.FireRecoilAnimationName].speed = recoilProperties.AnimationSpeed;
                    //play fire animation
                    recoilProperties.RecoilAnimation.CrossFadeQueued(recoilProperties.FireRecoilAnimationName/*, QueueMode.PlayNow*/, 0.05f, QueueMode.PlayNow);
                    //recoilProperties.RecoilAnimation.Play(recoilProperties.FireRecoilAnimationName);
                    //set fire delay equal to fire animation speed
					shooterProperties.fireRate = 1  *recoilProperties.AnimationSpeed / ((recoilProperties.RecoilAnimation.GetClip(recoilProperties.FireRecoilAnimationName).length ));
                    break;

                case AnimationType.Procedural:
                    recoilProperties.Recoil();
                    StartCoroutine(recoilProperties.returnToPos(recoilProperties.speed));
                    break;
            }
        }

        /// <summary>
        /// Drawing Muzzle Flash spawn point gizmo
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(muzzleProperties.muzzleFlashLocation.position, 0.01f);
        }

    }

    [System.Serializable]
    public class Shoot
    {
        
        [Tooltip("Which Layer we can shoot?")]
        public LayerMask hitMask;
        [Tooltip("How far you want to shoot.")]
        [Range(5f,200f)]
        public float ShootRange = 100;
        [Range(1f,100f)]
        [Tooltip("How far zombie can hear.")]
        public float HearRange = 20;

        [Tooltip("Bullets to fire per second.")]
        public float fireRate = 10;
        
        [Tooltip("If true, camera will shake while shooting.")]
        public bool shakeCamera = true;

        [Range(0.1f, 2f)]
        public float shakeAmount = 0.75f;

        /// <summary>
        /// Hidden Variables
        /// </summary>
        //This weapon's current ammo, will be init at game start
        //[HideInInspector]
        public int currentAmmo;

        //[HideInInspector]
        //How much Ammo this weapon can fire per round.
        public int ammoCapacity = 0;

        //[HideInInspector]
        //Total ammo to give to this weapon at Start.
        public int totalAmmo = 0;

        //If the Ammo capacity and total Ammo reaches zero, this will be false.
        [HideInInspector]
        public bool canShoot;

        [HideInInspector]
        public float _timer = 0;

        public string ProjectileName = "Bullet";
       // public int projectilesToShoot = 1;
        public float ProjectileForce = 100;
        public float Spread = 0.2f;
    }

    [System.Serializable]
    public class MuzzleFlash
    {
        public ParticleSystem muzzleFlash;
        public Transform muzzleFlashLocation;
        ParticleSystem flash;

        /// <summary>
        /// Spawns the muzzle flash as soon as called at the assigned location
        /// </summary>
        public void SpawnMuzzleFlash()
        {
            if (muzzleFlash == null || muzzleFlashLocation == null)
                return;

            flash = GameObject.Instantiate(muzzleFlash) as ParticleSystem;
            flash.gameObject.layer = LayerMask.NameToLayer("Weapon");
            flash.transform.SetParent(muzzleFlashLocation);
            flash.transform.localPosition = Vector3.zero;

            foreach(Transform t in flash.GetComponentInChildren<Transform>())
            {
                t.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }
        }

        public void PlayMuzzleFlash()
        {
            if (flash != null)
                flash.Play();
        }

    }

    [System.Serializable]
    public class BulletHits
    {
        [Tooltip("How much damage this weapon will cause per bullet to the uzAI Zombie.")]
        public int maxDamage = 5;
        public int minDamage = 1;

        public List<hitInfo> hits = new List<hitInfo>();

        /// <summary>
        /// Compare the tag and if the tag is found in our hits list, we instantiate the particle
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        public void SpawnBulletHits(Transform tag, Vector3 hitPoint, Vector3 hitNormal)
        {
            foreach (hitInfo h in hits)
            {
                if (h.hitTag == tag.tag && h.hitParticle != null)
                    //it requires 2 parameters
                    //1. exact point where we just hit
                    //2. perpendicular direction of the Hit point
                    h.onHit(tag, hitPoint, hitNormal);
            }
        }

        public void GiveDamage(Transform tag, Transform _myTransform)
        {
            int calculatedDamage = Random.Range(minDamage, maxDamage);

            //if Damage Multiplier is Active
            if (DamageMultiplier() != 0)
                //multiply total damage with it
                calculatedDamage *= DamageMultiplier();

            if (tag.tag == "uzAIZombie" && tag.GetComponent<uzAI.uzAIZombieStateManager>())
                tag.GetComponent<uzAI.uzAIZombieStateManager>().ZombieHealthStats.onDamage(calculatedDamage);

            if(tag.tag == "Turret" && tag.GetComponent<Turret>())
                tag.GetComponent<Turret>()._turretHealth.onDamage(calculatedDamage, _myTransform);

            if (tag.GetComponent<TurretProjectile>())
                tag.GetComponent<TurretProjectile>().Explode();

            if (tag.tag == "Player")
                tag.GetComponent<ManoeuvreFPSController>().Health.OnDamage(calculatedDamage);
            
            if(tag.tag == "Destructible")
                tag.GetComponent<DestructibleProps>().OnDamage(calculatedDamage);

            if (tag.tag == "ShooterAI")
                tag.GetComponent<ShooterAIStateManager>().Health.onDamage(calculatedDamage);

        }

        int DamageMultiplier()
        {
            int retVal = 0;

            if (PowerupsManager.Instance._DamageMultiplier.isActive)
                retVal = PowerupsManager.Instance._DamageMultiplier.DamageMultiplierAmount;

            return retVal;
        }
    }

    [System.Serializable]
    public class hitInfo
    {
        public string hitTag;
        public GameObject hitParticle;
        public List<AudioClip> hitAudioClip = new List<AudioClip>();

        /// <summary>
        /// Instantiate particles on hit.
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        public void onHit(Transform hitTransform, Vector3 hitPoint, Vector3 hitNormal)
        {
            GameObject hitObject = GameObject.Instantiate(hitParticle, hitPoint, Quaternion.LookRotation(hitNormal));
            hitObject.transform.SetParent(hitTransform);

            if (hitAudioClip.Count > 0)
                AudioSource.PlayClipAtPoint(hitAudioClip[Random.Range(0,hitAudioClip.Count)], hitPoint);

            GameObject.Destroy(hitObject, 1f);
        }
    }

    [System.Serializable]
    public class RecoilSpring
    {
        public AnimationType RecoilType = AnimationType.Animation;

        public float recoilPositionFactor = 1f;
        public float recoilRotationFactor = 1f;
        public float speed = 2f;

        public Animation RecoilAnimation;
        public string FireRecoilAnimationName = "Fire";
		public float AnimationSpeed = 1;

        Transform weapon;

        WeaponShooter _shooter;
        WeaponProceduralManoeuvre _wpm;

        Vector3 defPos;
        public Vector3 defRot;

        Vector3 ironSightRot;

        Vector3 recoilPos;
        Vector3 recoilRot;

        public void Initialize(Transform w, WeaponShooter shooter, WeaponProceduralManoeuvre wpm)
        {
            weapon = w;

            defPos = weapon.localPosition;
            
            _shooter = shooter;
            _wpm = wpm;

            if(_shooter)
             ironSightRot = _shooter.ironSightProperties.tweenRotation;

        }

        public void Recoil()
        {
            
            recoilPos = new Vector3(weapon.localPosition.x, weapon.localPosition.y, weapon.localPosition.z - recoilPositionFactor);
            recoilRot = new Vector3(weapon.localEulerAngles.x - recoilRotationFactor, weapon.localEulerAngles.y, weapon.localEulerAngles.z);

            weapon.localPosition = recoilPos;
            weapon.localEulerAngles = recoilRot;

        }

        public IEnumerator returnToPos(float time)
        {
            float elapsedTime = 0;
            while (elapsedTime < time)
            {
                weapon.localPosition = Vector3.Lerp(recoilPos, defPos, elapsedTime / time);
                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        public void returnToRot_Shooter()
        {
            //if we are not reloading and not in debug mode
            if (_shooter.weaponState != WeaponState.Reloading && !_wpm.DebugTransform && !ManoeuvreFPSController.Instance.Inputs.ironsightInput)
                weapon.localRotation = Quaternion.Lerp(Quaternion.Euler(weapon.localEulerAngles),
                                       Quaternion.Euler(defRot), Time.deltaTime * 10);
            else if(_shooter.weaponState != WeaponState.Reloading && !_wpm.DebugTransform && RecoilType == AnimationType.Procedural)
                weapon.localRotation = Quaternion.Lerp(Quaternion.Euler(weapon.localEulerAngles),
                                       Quaternion.Euler(ironSightRot), Time.deltaTime * 10);
        }

        public void returnToRot_Melee()
        {
            //if we are not reloading and not in debug mode
            if (!_wpm.DebugTransform)
                weapon.localRotation = Quaternion.Lerp(Quaternion.Euler(weapon.localEulerAngles),
                                       Quaternion.Euler(defRot), Time.deltaTime * 10);
        }
    }

    [System.Serializable]
    public class WeaponSounds
    {
        public AudioClip equipSound;
        public AudioClip reloadSound;
        public AudioClip fireSound;
        [HideInInspector]
        public AudioSource source;

        [Range(-3f,3f)]
        public float maxPitchVariation = 0.2f;
        [Range(-3f,3f)]
        public float minPitchVariation = 1.2f;

        /// <summary>
        /// Plays the audio clip paassed into this method
        /// </summary>
        /// <param name="clip"></param>
        public void PlayAudio(AudioClip clip, bool causeVariation, Vector3 pos)
        {

            if (causeVariation && source)
            {
                source.PlayOneShot(clip);
                source.pitch = Random.Range(maxPitchVariation, minPitchVariation);
            }
            else
                AudioSource.PlayClipAtPoint(clip, pos);
        }
    }   


    [System.Serializable]
    public class WeaponShake
    {
        public bool shake = true;

        public float shakeDuration = 0.1f;
        public float shakeAmount = 0.2f;
        public float cooldown = 0.3f;

        Vector3 defPos;
        float currentShakeDuration;

        public IEnumerator Shake(Transform t) {

            if (shake)
                yield return null;

            defPos = t.localPosition;
            currentShakeDuration = shakeDuration;

            while(currentShakeDuration > 0)
            {
                t.position = defPos + Random.insideUnitSphere * shakeAmount;
                currentShakeDuration -= Time.deltaTime * cooldown;

                yield return null;
            }

            t.position = defPos;
        }

    }

    [System.Serializable]
    public class IronSight
    {
        public bool useIronsight = true;
        [HideInInspector]
        public bool canTween = true;

        public Transform vignetteImage;

        public Vector3 tweenPosition;
        public Vector3 tweenRotation;

        public float tweenFOVTo = 50f;
        public float tweenVignetteScaleTo = 1f;
        public float tweenTimescaleTo = 0.5f;
        public float ironsightTweenDuration = 0.25f;

        //cache
        float cacheFov;
        float cacheVignetteScale;
        float cacheTimescale;
        Vector3 cachePos;
        Quaternion cacheRot;
        Transform _weaponObject;

        public void Init(float fov, float timescale, Transform wpnObject)
        {

            vignetteImage = GameObject.Find("ScreenVignett").transform;
            cacheFov = fov;
            cacheVignetteScale = vignetteImage.transform.localScale.x;
            cacheTimescale = timescale;

            _weaponObject = wpnObject;
          
        }

        public IEnumerator tweenIronSight(bool ironSightInput)
        {

            if(cachePos == Vector3.zero)
            {
                cacheRot = _weaponObject.transform.localRotation;
                cachePos = _weaponObject.transform.localPosition;
            }

            if (ironSightInput && canTween)
            {
                float elapsedTime = 0;

                while (elapsedTime < ironsightTweenDuration)
                {
                    //disable canTween
                    canTween = false;

                    //tween position
                    _weaponObject.transform.localPosition = Vector3.Lerp(cachePos, tweenPosition, elapsedTime / ironsightTweenDuration);
                    _weaponObject.transform.localRotation = Quaternion.Lerp(cacheRot, Quaternion.Euler(tweenRotation), elapsedTime / ironsightTweenDuration);

                    //tween fov
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, tweenFOVTo, elapsedTime / ironsightTweenDuration);
                    //tween vignette scale
                    vignetteImage.localScale = Vector3.Lerp(vignetteImage.localScale, new Vector3(tweenVignetteScaleTo, tweenVignetteScaleTo, tweenVignetteScaleTo), elapsedTime / ironsightTweenDuration);
                    //tween time scale
                    Time.timeScale = Mathf.Lerp(Time.timeScale, tweenTimescaleTo, elapsedTime / ironsightTweenDuration);

                    //increment elapsed time
                    elapsedTime += Time.deltaTime;

                    yield return null;
                }

                _weaponObject.transform.localPosition = tweenPosition;
                _weaponObject.transform.localRotation = Quaternion.Euler(tweenRotation);

            }
            else if (!ironSightInput && !canTween)
            {
                float elapsedTime = 0;

                while (elapsedTime < ironsightTweenDuration)
                {
                    //tween position
                    _weaponObject.transform.localPosition = Vector3.Lerp(tweenPosition, cachePos, elapsedTime / ironsightTweenDuration);
                    _weaponObject.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(tweenRotation), cacheRot, elapsedTime / ironsightTweenDuration);

                    //tween fov
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, cacheFov, elapsedTime / ironsightTweenDuration);
                    //tween vignette scale
                    vignetteImage.localScale = Vector3.Lerp(vignetteImage.localScale, new Vector3(cacheVignetteScale, cacheVignetteScale, cacheVignetteScale), elapsedTime / ironsightTweenDuration);
                    //tween time scale
                    Time.timeScale = Mathf.Lerp(Time.timeScale, cacheTimescale, elapsedTime / ironsightTweenDuration);

                    //increment elapsed time
                    elapsedTime += Time.deltaTime;

                    yield return null;
                }

                //enable canTween
                canTween = true;

                _weaponObject.transform.localPosition = cachePos;
                _weaponObject.transform.localRotation = cacheRot;

            }

        }
    }


    [System.Serializable]
    public class AddForceToRigidbodies
    {
        public bool applyForce = true;
        public float forceAmount = 10f;
    }

    //[System.Serializable]
        //public class WeaponShells
        //{
        //    public Transform spawnPoint;
        //    public GameObject shellPrefab;
        //    [Range(1f,15f)]
        //    public float spawnForceMin = 3f;
        //    [Range(1f,15f)]
        //    public float spawnForceMax = 8f;
        //    public float destroyDelay = 2f;

        //    public void SpawnShells()
        //    {
        //        if (shellPrefab == null)
        //            return;

        //        GameObject shell = GameObject.Instantiate(shellPrefab);
        //        shell.transform.position = spawnPoint.position;
        //        shell.transform.SetParent(spawnPoint);

        //        //apply force
        //        shell.GetComponent<ShellCollision>().ApplyForce(spawnPoint.position, Random.Range(spawnForceMin, spawnForceMax));

        //        //Destroy
        //        GameObject.Destroy(shell, destroyDelay);
        //    }
        //}
}