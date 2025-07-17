using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public enum ShootingType { Raycast, Projectile }

    [RequireComponent(typeof(Collider))]
    public class Turret : MonoBehaviour
    {
        public ShootingType ShootingType = ShootingType.Raycast;
        public Transform TurretGun;
        public Transform Target;
        public Transform cachedTarget;

        public float rotationSpeed = 15;
        public float followSpeed = 2;
        public float cooldownTimer = 3f;
        float _timer;

        public TurretAI _turretAI;
        public TurretShooterProperties _shooterProperties;
        public TurretHealth _turretHealth;

        public TurretRaycastShooting _TurretRaycastShooting;
        public TurretProjectileShooting _TurretProjectileShooting;

        //Editor Variables
        [HideInInspector]
        public int propertyTab;

        public UnityEngine.Events.UnityEvent OnTurretDestroy;

        private void Awake()
        {
            InitializeTurret();
        }

        void InitializeTurret()
        {
            //set tag
            transform.tag = "Turret";


            _shooterProperties.InitializeShooterProperties(this, ShootingType);

            _TurretRaycastShooting.InitializeTurretRaycastShooting(TurretGun, _turretAI.targetMask, this);
            _TurretProjectileShooting.InitializeTurretProjectileShooting(TurretGun, _turretAI.targetMask, this);

            _turretAI.InitializeTurretAI(TurretGun, this);
            StartCoroutine(_turretAI.SearchTargetsCoroutine());

            _turretHealth.InitializeTurretHealth(TurretGun.gameObject, this);
        }

        void FixedUpdate()
        {

            if (Target == null) {

                if (cachedTarget == null)
                {
                    Vector3 newRot = new Vector3(TurretGun.localEulerAngles.x,
                        TurretGun.localEulerAngles.y + rotationSpeed,
                        TurretGun.localEulerAngles.z);

                    TurretGun.localEulerAngles = newRot;

                }
                else {

                    if(cachedTarget != null)
                    {
                        //we try shooting the cached target
                        ShootTurret(cachedTarget);

                    }

                    if (_timer >= cooldownTimer)
                    {
                        //if Turret is Idle i.e rotating, make sure our cache is null
                        cachedTarget = null;
                        _timer = 0;
                    }
                    else
                        _timer += Time.deltaTime;

                }
            }
            else
            {
                // NOTE :
                // This is worth mentioning that we are only able to shoot and chase if we have a 
                // target in our range and sight

                //We start chasing and Shooting
                ShootTurret(Target);

                //reset timer
                _timer = 0;
            }

        }

        void ShootTurret(Transform _target)
        {
            switch (ShootingType)
            {
                case ShootingType.Raycast:
                    _TurretRaycastShooting.ShootTarget(_target);
                    break;

                case ShootingType.Projectile:
                    _TurretProjectileShooting.ShootTarget(_target);
                    break;
            }

            //Start chasing Traget
            Quaternion destinationRotation = Quaternion.LookRotation(_target.position - TurretGun.position, Vector3.up);
            TurretGun.rotation = Quaternion.RotateTowards(TurretGun.rotation, destinationRotation, followSpeed );

        }

    }

    [System.Serializable]
    public class TurretShooterProperties
    {

        [Header("Raycast Properties")]
        public LayerMask hitMask;
        public float Range = 100;
        public Transform MuzzleFlashLocation;
        public int maxDamage = 10;
        public int minDamage = 3;

        [Header("Projectile Properties")]
        public string ProjectileName = "Rocket";
        public float ForwardForce = 100;
        public float RotateAmount = 3;
        public ParticleSystem explodeFX;
        public AudioClip explodeSFX;
        public List<Transform> ProjectilePoint = new List<Transform>();
        public int projectileDamage = 100;

        [Header("Common Properties")]
        public float fireRate = 5;
        public ParticleSystem _MuzzleFlash;
        public List<AudioClip> fireSound = new List<AudioClip>();
        public float maxPitch = 1f;
        public float minPitch = 0.75f;
        public List<TurretBulletHit> bulletHit = new List<TurretBulletHit>();

        [HideInInspector]
        public float _fireRateTimer;
        [HideInInspector]
        public ParticleSystem raycastMuzzleToUse;
        [HideInInspector]
        public List<ParticleSystem> projectileMuzzleToUse = new List<ParticleSystem>();
        [HideInInspector]
        public AudioSource source;


        public void InitializeShooterProperties(Turret turret, ShootingType _type)
        {
            source = turret.gameObject.AddComponent<AudioSource>();

            InitMuzzleFlash(_type);

        }

        /// <summary>
        /// Init the Muzzle Flash
        /// </summary>
        void InitMuzzleFlash(ShootingType _type)
        {
            switch (_type)
            {
                case ShootingType.Raycast:
                    //Spawn at our Muzzle Flash Location
                    raycastMuzzleToUse = GameObject.Instantiate(_MuzzleFlash) as ParticleSystem;
                    raycastMuzzleToUse.Stop(true);

                    raycastMuzzleToUse.transform.SetParent(MuzzleFlashLocation);
                    raycastMuzzleToUse.transform.localPosition = Vector3.zero;
                    raycastMuzzleToUse.transform.localEulerAngles = Vector3.zero;
                    break;

                case ShootingType.Projectile:
                    //Spawn Muzzle(s) at our projectile point location
                    for(int i =0; i< ProjectilePoint.Count; i++)
                    {
                        ParticleSystem _m = GameObject.Instantiate(_MuzzleFlash) as ParticleSystem;
                        _m.Stop(true);
                        _m.transform.SetParent(ProjectilePoint[i]);
                        _m.transform.localPosition = Vector3.zero;
                        _m.transform.localEulerAngles = Vector3.zero;
                        projectileMuzzleToUse.Add(_m);

                    }
                    break;
            }


        }


    }

    [System.Serializable]
    public class TurretRaycastShooting
    {
        Transform TurretGun;
        Turret turret;

        /// <summary>
        /// Initialize the Raycast
        /// </summary>
        /// <param name="_TurretGun"></param>
        /// <param name="_Range"></param>
        /// <param name="_targetMask"></param>
        /// <param name="_turret"></param>
        public void InitializeTurretRaycastShooting(Transform _TurretGun, LayerMask _targetMask, Turret _turret)
        {
            TurretGun = _TurretGun;
            turret = _turret;
           
        }

       
        /// <summary>
        /// Shoots the Target
        /// </summary>
        public void ShootTarget(Transform target)
        {
            //increment timer every frame
            turret._shooterProperties._fireRateTimer += Time.deltaTime;

            
            //make sure we are firing at rate specified
            if (turret._shooterProperties._fireRateTimer > (1 / turret._shooterProperties.fireRate))
            {
                //reset timer
                turret._shooterProperties._fireRateTimer = 0;

                //send a ray of specified Range
                RaycastHit hit;
                if (Physics.Raycast(TurretGun.transform.position, TurretGun.transform.forward, out hit, turret._shooterProperties.Range, turret._shooterProperties.hitMask))
                {
                    //emit muzzle flash
                    turret._shooterProperties.raycastMuzzleToUse.Play();

                    //Play Sound
                    PlayFireSound();

                    //Show Hit FX
                    ShowHitFX(hit.transform.tag, hit.transform, hit.point, hit.normal);

                    //Apply Damage  to Manoeuvre fps
                    if (hit.transform.GetComponent<ManoeuvreFPSController>())
                    {
                        PlayerHealth h = hit.transform.GetComponent<ManoeuvreFPSController>().Health;
                        h.OnDamage(Random.Range(turret._shooterProperties.maxDamage, turret._shooterProperties.minDamage));
                    }

                    //Apply Damage  to uzAI
                    if (hit.transform.GetComponent<uzAI.uzAIZombieStateManager>())
                    {
                        uzAI.uzAIZombieHealth h = hit.transform.GetComponent<uzAI.uzAIZombieStateManager>().ZombieHealthStats;
                        h.onDamage(Random.Range(turret._shooterProperties.maxDamage, turret._shooterProperties.minDamage));
                    }

					//Apply Damage  to Shooter AI
					if (hit.transform.GetComponent<ShooterAIStateManager> ()) {

						ShooterAIHealth h = hit.transform.GetComponent<ShooterAIStateManager> ().Health;
						h.onDamage (Random.Range (turret._shooterProperties.maxDamage, turret._shooterProperties.minDamage));
					
					}
                }

            }

        }

        void PlayFireSound()
        {
            AudioClip clip = turret._shooterProperties.fireSound[Random.Range(0, turret._shooterProperties.fireSound.Count)];

            turret._shooterProperties.source.pitch = Random.Range(turret._shooterProperties.minPitch, turret._shooterProperties.maxPitch);
            turret._shooterProperties.source.PlayOneShot(clip);
        }

        void ShowHitFX(string Tag, Transform hitTransform, Vector3 hitPoint, Vector3 hitNormal)
        {
            for(int i =0; i< turret._shooterProperties.bulletHit.Count; i++)
            {
                if(turret._shooterProperties.bulletHit[i].Tag == Tag)
                {
                    //spawn fx
                    ParticleSystem hitObject = GameObject.Instantiate(turret._shooterProperties.bulletHit[i].hitFX, hitPoint, Quaternion.LookRotation(hitNormal)) as ParticleSystem;
                    hitObject.transform.SetParent(hitTransform);
                    hitObject.Play();
                    //play Random sfx at Hit location
                    AudioSource.PlayClipAtPoint(turret._shooterProperties.bulletHit[i].hitSFX, hitTransform.position);
                }
            }
        }
    }

    [System.Serializable]
    public class TurretProjectileShooting
    {
        Transform TurretGun;
        Turret turret;

        /// <summary>
        /// Initialize the Raycast
        /// </summary>
        /// <param name="_TurretGun"></param>
        /// <param name="_Range"></param>
        /// <param name="_targetMask"></param>
        /// <param name="_turret"></param>
        public void InitializeTurretProjectileShooting(Transform _TurretGun, LayerMask _targetMask, Turret _turret)
        {
            TurretGun = _TurretGun;
            turret = _turret;

        }

        /// <summary>
        /// Shoots the Target
        /// </summary>
        public void ShootTarget(Transform target)
        {
            //increment timer every frame
            turret._shooterProperties._fireRateTimer += Time.deltaTime;

            //make sure we are firing at rate specified
            if (turret._shooterProperties._fireRateTimer > (1 / turret._shooterProperties.fireRate))
            {
                //reset timer
                turret._shooterProperties._fireRateTimer = 0;

                //Play Sound
                PlayFireSound();

                //Spawn Projectile 
                SpawnProjectile();

            }

        }

        void PlayFireSound()
        {
            AudioClip clip = turret._shooterProperties.fireSound[Random.Range(0, turret._shooterProperties.fireSound.Count)];

            turret._shooterProperties.source.pitch = Random.Range(turret._shooterProperties.minPitch, turret._shooterProperties.maxPitch);
            turret._shooterProperties.source.PlayOneShot(clip);
        }

        void ShowHitFX(string Tag, Transform hitTransform, Vector3 hitPoint, Vector3 hitNormal)
        {
            for (int i = 0; i < turret._shooterProperties.bulletHit.Count; i++)
            {
                if (turret._shooterProperties.bulletHit[i].Tag == Tag)
                {
                    //spawn fx
                    ParticleSystem hitObject = GameObject.Instantiate(turret._shooterProperties.bulletHit[i].hitFX, hitPoint, Quaternion.LookRotation(hitNormal)) as ParticleSystem;
                    hitObject.transform.SetParent(hitTransform);
                    hitObject.Play();
                    //play Random sfx at Hit location
                    AudioSource.PlayClipAtPoint(turret._shooterProperties.bulletHit[i].hitSFX, hitTransform.position);
                }
            }
        }

        void SpawnProjectile()
        {
            //Spawning Projectile from Pool from each projectile location
            for (int i = 0; i < turret._shooterProperties.ProjectilePoint.Count; i++) {

                GameObject projectile = ProjectilesPooler.Instance.SpawnFromPool(turret._shooterProperties.ProjectileName, turret._shooterProperties.ProjectilePoint[i].position, turret._shooterProperties.ProjectilePoint[i].rotation);
                //Prepare it to EXPLODE!!!
                Transform t = turret.Target == null ? turret.cachedTarget : turret.Target;
                projectile.GetComponent<TurretProjectile>().InitializeProjectile(turret._shooterProperties.ForwardForce, turret._shooterProperties.RotateAmount, turret._shooterProperties.explodeFX, turret._shooterProperties.explodeSFX, turret._shooterProperties.projectileDamage, t);

                //Also Play Muzzle Flash
                turret._shooterProperties.projectileMuzzleToUse[i].Play();
            }

            //Rest all is handled in TurretProjectile.cs Script
        }

    }

    [System.Serializable]
    public class TurretBulletHit
    {
        public string Tag;
        public ParticleSystem hitFX;
        public AudioClip hitSFX ; 

    }

    [System.Serializable]
    public class TurretAI
    {
        Transform TurretGun;
        Turret turret;

        [Header("Sight Properties")]
        [Tooltip("Define how far this agent can see / hear.")]
        [Range(1f, 25f)]
        public float Range = 10f;

        [Tooltip("View Angle of this Agent, It is Recommended that you change it in Editor for better results.")]
        [Range(0f, 360f)]
        public float Angle;

        [Tooltip("How fast you want to check for Threats.")]
        public float SearchIterationTime = 0.25f;

        [Tooltip("What is the Target Layer Mask.")]
        public LayerMask targetMask = 9;
        [Tooltip("What is the Obstacles Layer Mask.")]
        public LayerMask obstacleMask = 10;

        public void InitializeTurretAI(Transform _TurretGun, Turret _turret)
        {
            TurretGun = _TurretGun;
            turret = _turret;
        }

        /// <summary>
        /// Using a Coroutine to optimize the searching process
        /// </summary>
        public IEnumerator SearchTargetsCoroutine()
        {
            while (true)
            {
                //instead of calling every frame
                //we are calling it in every search iteration time
                yield return new WaitForSeconds(SearchIterationTime);
                SearchVisibleTarget();
            }
        }

        /// <summary>
        /// Search the visible Targets as soon as they enter in our Vision Angle
        /// </summary>
        void SearchVisibleTarget()
        {
            turret.Target = null;

            //We get all the colliders in our Range
            Collider[] targetsInViewRadius = Physics.OverlapSphere(TurretGun.transform.position, Range, targetMask);

            //we loop through all the above registered colliders
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                //get the target
                Transform target = targetsInViewRadius[i].transform;

                //else
                //we get the direction
                Vector3 dirToTarget = (target.position - TurretGun.transform.position).normalized;

                //if the angle between Zombie and the direction is less then the view angle
                if (Vector3.Angle(TurretGun.transform.forward, dirToTarget) < Angle / 2)
                {
                    //we see the distance of this target
                    //from our position
                    float distToTarget = Vector3.Distance(TurretGun.transform.position, target.position);

                    //if there's no obstacle in between
                    if (!Physics.Raycast(TurretGun.transform.position, dirToTarget, distToTarget, obstacleMask))
                    {
                        turret.Target = target;
                        turret.cachedTarget = target;

                        
                    }

                }
            }
        }

        /// <summary>
        /// This is only used in the editor to make the arc
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="isAngleGlobal"></param>
        /// <returns></returns>
        public Vector3 DirFromAngle(float angle, bool isAngleGlobal, Transform _TurretGun)
        {
            if (!isAngleGlobal)
            {
                angle += _TurretGun.transform.eulerAngles.y;
            }

            float retAngle = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(retAngle), 0, Mathf.Cos(retAngle));

        }
    }

    [System.Serializable]
    public class TurretHealth
    {
        public int Health = 100;
        public ParticleSystem deathExplosionFX;
        public AudioClip deathSFX;
        public Healthbar healthBar;
        
        GameObject TurretGun;
        ParticleSystem deathFx;
        Turret turret;

        /// <summary>
        /// Initialize Health 
        /// </summary>
        /// <param name="_turretGun"></param>
        /// <param name="_turret"></param>
        public void InitializeTurretHealth(GameObject _turretGun, Turret _turret)
        {
            TurretGun = _turretGun;
            turret = _turret;

            //init death fx
            deathFx = GameObject.Instantiate(deathExplosionFX) as ParticleSystem;
            deathFx.Stop(true);
            deathFx.transform.SetParent(TurretGun.transform);
            deathFx.transform.localPosition = Vector3.zero;
            deathFx.transform.localEulerAngles = Vector3.zero;
            deathFx.transform.SetParent(turret.transform);
        }

        /// <summary>
        /// Lerp the damage slider if health > 0 else Kill Turret
        /// </summary>
        /// <param name="amt"></param>
        public void onDamage(int amt, Transform target)
        {
            if(healthBar)
                healthBar.StartLerp();

            turret.cachedTarget = target;

            if (Health > 0)
                Health -= amt;

            if (Health <= 0)
                KillTurret();
        }

        /// <summary>
        /// Hides the Turret Gun Mesh
        /// and Destroy all what is possible with 
        /// a nice fx and sound fx
        /// </summary>
        void KillTurret()
        {
            //Play dialogue
            gc_PlayerDialoguesManager.Instance.PlayDialogueClip(gc_PlayerDialoguesManager.DialogueType.Kills);

            TurretGun.SetActive(false);
            deathFx.Play();
            turret.GetComponent<AudioSource>().PlayOneShot(deathSFX);

            //invoke event
            turret.OnTurretDestroy.Invoke();

            GameObject.Destroy(turret);
            GameObject.Destroy(turret.GetComponent<Collider>());
        }

    }
}