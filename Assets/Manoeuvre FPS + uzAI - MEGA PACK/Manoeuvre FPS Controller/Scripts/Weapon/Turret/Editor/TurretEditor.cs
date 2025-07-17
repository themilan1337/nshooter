using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(Turret))]
    public class TurretEditor : Editor
    {
        Turret _turret;
        Transform turretGun;
        TurretAI _ai;

        bool fireSoundsToggle;
        bool bulletsHitToggle;

        SerializedObject _SO_Turret;
        SerializedProperty OnTurretDestroy;

        private void OnEnable()
        {
            _turret = (Turret)target;
            turretGun = _turret.TurretGun;
            _ai = _turret._turretAI;

            _SO_Turret = new SerializedObject(_turret);
            OnTurretDestroy = _SO_Turret.FindProperty("OnTurretDestroy");
        }

        public override void OnInspectorGUI()
        {
            DrawPropertyTabs();

            //DrawDefaultInspector();

        }

        void DrawPropertyTabs()
        {
            Texture t = Resources.Load("EditorContent/Turret-icon") as Texture;
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            _turret.propertyTab = GUILayout.Toolbar(_turret.propertyTab, new string[] {"Turret", "AI", "Shooter", "Health" });

            switch (_turret.propertyTab)
            {
                case 00:
                    DrawTurretProperties();
                    break;
                case 01:
                    DrawTurretAIProperties();
                    break;
                case 02:
                    DrawTurretShooterProperties();
                    break;
                case 03:
                    DrawTurretHealthProperties();
                    break;

            }

            //draw on Death event
            EditorGUILayout.PropertyField(OnTurretDestroy);

            _SO_Turret.ApplyModifiedProperties();

        }

        void DrawTurretProperties()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("-- Turret Properties --", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Select the Shoot Behaviour - Raycast OR Projectile", EditorStyles.helpBox);
            ShootingType ShootingType = (ShootingType)EditorGUILayout.EnumPopup("Shooting Type", _turret.ShootingType);
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("This is the Transform which will be rotating, searching for targets.", EditorStyles.helpBox);
            Transform TurretGun = (Transform)EditorGUILayout.ObjectField("Turret Gun", _turret.TurretGun, typeof(Transform));
            EditorGUILayout.LabelField("How fast Turret Gun will Rotate.", EditorStyles.helpBox);
            float rotationSpeed = EditorGUILayout.Slider("Rotation Speed", _turret.rotationSpeed, 0, 10f);
            EditorGUILayout.LabelField("How fast Turret Gun will chase a Target.", EditorStyles.helpBox);
            float followSpeed = EditorGUILayout.Slider("Follow Speed", _turret.followSpeed, 0, 10f);
            EditorGUILayout.LabelField("How long Turret gun will wait after losing target.", EditorStyles.helpBox);
            float cooldownTimer = EditorGUILayout.Slider("Cooldown Timer", _turret.cooldownTimer, 0, 10f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Turret Properties");

                _turret.ShootingType = ShootingType;
                _turret.TurretGun = TurretGun;
                _turret.rotationSpeed = rotationSpeed;
                _turret.followSpeed = followSpeed;
                _turret.cooldownTimer = cooldownTimer;
            }
        }

        void DrawTurretAIProperties()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("-- Turret AI Properties --", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("How far this Turret can see.", EditorStyles.helpBox);
            float Range = EditorGUILayout.Slider("Range", _turret._turretAI.Range, 1, 25);
            EditorGUILayout.LabelField("Turret's FOV.", EditorStyles.helpBox);
            float Angle = EditorGUILayout.Slider("Angle", _turret._turretAI.Angle, 0, 360);
            EditorGUILayout.LabelField("Searching for targets in every : " + _turret._turretAI.SearchIterationTime +" seconds." , EditorStyles.helpBox);
            float SearchIterationTime = EditorGUILayout.FloatField("Search Iteration Time", _turret._turretAI.SearchIterationTime);
            EditorGUILayout.LabelField("All the Target Layer Masks which This Turret will shoot." , EditorStyles.helpBox);
            LayerMask targetMask = LayerMaskUtility.LayerMaskField("Target Mask", _turret._turretAI.targetMask);
            EditorGUILayout.LabelField("All the Obstacle Layer Masks which will block This Turret's FOV." , EditorStyles.helpBox);
            LayerMask obstacleMask = LayerMaskUtility.LayerMaskField("Obstacle Mask", _turret._turretAI.obstacleMask);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Turret AI");
                _turret._turretAI.Range = Range;
                _turret._turretAI.Angle = Angle;
                _turret._turretAI.SearchIterationTime = SearchIterationTime;
                _turret._turretAI.targetMask = targetMask;
                _turret._turretAI.obstacleMask = obstacleMask;
            }
        }

        void DrawTurretShooterProperties()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginVertical("Box");

            switch (_turret.ShootingType)
            {
                case ShootingType.Raycast:
                    DrawRaycastProperties();
                    break;
                case ShootingType.Projectile:
                    DrawProjectileProperties();
                    break;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawCommonShooterProperties();
            
        }

        void DrawRaycastProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("-- Raycast Properties --", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("All the layers which can be hit with raycast.", EditorStyles.helpBox);
            LayerMask hitMask = LayerMaskUtility.LayerMaskField("Hit Mask", _turret._shooterProperties.hitMask);
            EditorGUILayout.LabelField("Raycast Range. Different from AI Range.", EditorStyles.helpBox);
            float Range = EditorGUILayout.Slider("Range", _turret._shooterProperties.Range, 1, 250);
            EditorGUILayout.LabelField("Muzzle Flash Location.", EditorStyles.helpBox);
            Transform MuzzleFlashLocation = (Transform)EditorGUILayout.ObjectField("MuzzleFlashLocation", _turret._shooterProperties.MuzzleFlashLocation, typeof(Transform));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Random between max and min damage will be given to the Player / Zombie.", EditorStyles.helpBox);
            int maxDamage = EditorGUILayout.IntSlider("Max Damage", _turret._shooterProperties.maxDamage, 0, 100);
            int minDamage = EditorGUILayout.IntSlider("Min Damage", _turret._shooterProperties.minDamage, 0, 100);

            EditorGUILayout.EndVertical();

            

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Turret Shooter Properties");

                _turret._shooterProperties.hitMask = hitMask;
                _turret._shooterProperties.Range = Range;
                _turret._shooterProperties.MuzzleFlashLocation = MuzzleFlashLocation;
                _turret._shooterProperties.maxDamage = maxDamage;
                _turret._shooterProperties.minDamage = minDamage;
            }

        }

        void DrawProjectileProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("-- Projectile Properties --", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Name of the Prefab which is to be used from the Pool.", EditorStyles.helpBox);
            string ProjectileName = EditorGUILayout.TextField("Projectile Name", _turret._shooterProperties.ProjectileName);

            EditorGUILayout.LabelField("Forward Force is the Projectile Speed in forward Direction", EditorStyles.helpBox);
            float ForwardForce = EditorGUILayout.FloatField("Forward Force", _turret._shooterProperties.ForwardForce);

            EditorGUILayout.LabelField("How much Projectile will rotate towards the Target", EditorStyles.helpBox);
            float RotateAmount = EditorGUILayout.FloatField("Rotate Amount", _turret._shooterProperties.RotateAmount);

            EditorGUILayout.LabelField("How much Damage it will cause to uzAI Zombie and Player", EditorStyles.helpBox);
            int projectileDamage = EditorGUILayout.IntField("Damage", _turret._shooterProperties.projectileDamage);

            EditorGUILayout.LabelField("This Particle FX will be spawned when it collides with any object", EditorStyles.helpBox);
            ParticleSystem explodeFX = (ParticleSystem)EditorGUILayout.ObjectField("Explode FX", _turret._shooterProperties.explodeFX, typeof(ParticleSystem));

            EditorGUILayout.LabelField("This Audio SFX will be played when it collides with any object", EditorStyles.helpBox);
            AudioClip explodeSFX = (AudioClip)EditorGUILayout.ObjectField("Explode SFX", _turret._shooterProperties.explodeSFX, typeof(AudioClip));

            DrawProjectilePointsList();

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Turret Projectile Properties");

                _turret._shooterProperties.ProjectileName = ProjectileName;
                _turret._shooterProperties.ForwardForce = ForwardForce;
                _turret._shooterProperties.RotateAmount = RotateAmount;
                _turret._shooterProperties.explodeFX = explodeFX;
                _turret._shooterProperties.explodeSFX = explodeSFX;
                _turret._shooterProperties.projectileDamage = projectileDamage;
            }
        }

        void DrawProjectilePointsList()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Total Points : " + _turret._shooterProperties.ProjectilePoint.Count, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("In case your Turret has more then 1 Projectile Points, you can always add more below.", EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                GameObject point = new GameObject();

                point.transform.SetParent(_turret.transform);
                point.transform.localPosition = Vector3.zero;
                point.transform.localEulerAngles = Vector3.zero;
                point.name = "Projectile Point";

                _turret._shooterProperties.ProjectilePoint.Add(point.transform);
                
            }

            if (GUILayout.Button("Clears"))
            {
                _turret._shooterProperties.ProjectilePoint.Clear();
                
            }

            EditorGUILayout.EndHorizontal();

            for (int i =0; i< _turret._shooterProperties.ProjectilePoint.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("helpbox");

                Transform _t = (Transform)EditorGUILayout.ObjectField("Point : " + i, _turret._shooterProperties.ProjectilePoint[i], typeof(Transform));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    if(_turret._shooterProperties.ProjectilePoint[i] != null)
                        DestroyImmediate(_turret._shooterProperties.ProjectilePoint[i].gameObject);

                    _turret._shooterProperties.ProjectilePoint.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();


                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "ProjectilePointsList");

                    _turret._shooterProperties.ProjectilePoint[i] = _t;
                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawCommonShooterProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("-- Common Shooter Properties --", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Number of Raycasts / Projectiles to Fire per seconds.", EditorStyles.helpBox);
            float fireRate = EditorGUILayout.FloatField("Fire Rate", _turret._shooterProperties.fireRate);
            fireRate = Mathf.Clamp(fireRate, 0.1f, fireRate);
            ParticleSystem _MuzzleFlash = (ParticleSystem)EditorGUILayout.ObjectField("Muzzle Flash", _turret._shooterProperties._MuzzleFlash, typeof(ParticleSystem));
            EditorGUILayout.Space();

           
            //EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("-- Fire Sounds Properties --", EditorStyles.centeredGreyMiniLabel);

            string s1 = fireSoundsToggle ? "Hide" : "Show";
            fireSoundsToggle = GUILayout.Toggle(fireSoundsToggle, s1, "Button");

            if (fireSoundsToggle)
                DrawFireSounds();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("-- Bullet Hit Properties --", EditorStyles.centeredGreyMiniLabel);

            string s2 = bulletsHitToggle ? "Hide" : "Show";
            bulletsHitToggle = GUILayout.Toggle(bulletsHitToggle, s2, "Button");

            if (bulletsHitToggle)
                DrawBulletHits();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "CommonShooterProperties");
                _turret._shooterProperties.fireRate = fireRate;
                _turret._shooterProperties._MuzzleFlash = _MuzzleFlash;

            }
        }

        void DrawFireSounds()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Maximum and Minimum Pitch Variation", EditorStyles.helpBox);
            float maxPitch = EditorGUILayout.Slider("Max Pitch", _turret._shooterProperties.maxPitch, -3, 3);
            float minPitch = EditorGUILayout.Slider("Min Pitch", _turret._shooterProperties.minPitch, -3, 3);

            EditorGUILayout.EndVertical();

            DrawFireSoundsList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Turret Fire Sounds");

                _turret._shooterProperties.maxPitch = maxPitch;
                _turret._shooterProperties.minPitch = minPitch;
            }
        }

        void DrawFireSoundsList()
        {

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Total Fire Sounds : " + _turret._shooterProperties.fireSound.Count, EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                AudioClip newAC = null;
                _turret._shooterProperties.fireSound.Add(newAC);
            }

            if (GUILayout.Button("Clear"))
            {
                _turret._shooterProperties.fireSound.Clear();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("A random sound from below will be played everytime Turret Shoots.", EditorStyles.helpBox);

            for (int i = 0; i < _turret._shooterProperties.fireSound.Count; i++)
            {

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("Box");

                AudioClip c = (AudioClip)EditorGUILayout.ObjectField(_turret._shooterProperties.fireSound[i], typeof(AudioClip));

                if (GUILayout.Button("X",  EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _turret._shooterProperties.fireSound.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Turret Fire Sounds List");

                    _turret._shooterProperties.fireSound[i] = c;
                }

            }

            EditorGUILayout.EndVertical();

        }

        void DrawBulletHits()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Total Bullet Hits : " + _turret._shooterProperties.bulletHit.Count, EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                _turret._shooterProperties.bulletHit.Add(new TurretBulletHit());
            }

            if (GUILayout.Button("Clear"))
            {
                _turret._shooterProperties.bulletHit.Clear();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Corresponding Bullet Hit Particle FX with random Sound FX will be spawned .", EditorStyles.helpBox);

            for (int i = 0; i < _turret._shooterProperties.bulletHit.Count; i++)
            {

                EditorGUI.BeginChangeCheck();

                if (i > 0)
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();

                string Tag = EditorGUILayout.TextField("Tag", _turret._shooterProperties.bulletHit[i].Tag);

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _turret._shooterProperties.bulletHit.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                ParticleSystem hitFX = (ParticleSystem)EditorGUILayout.ObjectField("Hit FX", _turret._shooterProperties.bulletHit[i].hitFX, typeof(ParticleSystem));

                AudioClip hitSFX = (AudioClip)EditorGUILayout.ObjectField("Hit SFX", _turret._shooterProperties.bulletHit[i].hitSFX, typeof(AudioClip));
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Turret Bullet Hit List");

                    _turret._shooterProperties.bulletHit[i].Tag = Tag;
                    _turret._shooterProperties.bulletHit[i].hitFX = hitFX;
                    _turret._shooterProperties.bulletHit[i].hitSFX = hitSFX;
                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawTurretHealthProperties()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("-- Turret Health Properties --", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Total health of the Turret.", EditorStyles.helpBox);
            int Health = EditorGUILayout.IntSlider("Health", _turret._turretHealth.Health, 1, 500);

            EditorGUILayout.LabelField("Death Explosion Particle FX.", EditorStyles.helpBox);
            ParticleSystem deathExplosionFX = (ParticleSystem)EditorGUILayout.ObjectField("Death Explosion FX", _turret._turretHealth.deathExplosionFX, typeof(ParticleSystem));

            EditorGUILayout.LabelField("Death Sound FX.", EditorStyles.helpBox);
            AudioClip deathSFX = (AudioClip)EditorGUILayout.ObjectField("Death Sound FX", _turret._turretHealth.deathSFX, typeof(AudioClip));


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Turret Health");

                _turret._turretHealth.Health = Health;
                _turret._turretHealth.deathExplosionFX = deathExplosionFX;
                _turret._turretHealth.deathSFX = deathSFX;

            }
        }

        #region Scene GUI
        private void OnSceneGUI()
        {

            DrawSightGizmos();

        }
        void DrawSightGizmos()
        {

            if (turretGun == null) {

                Handles.color = Color.red;
                Handles.Label(_turret.transform.position, "Please Assign the Turret Gun.");
                return;
            }
            Handles.color = Color.yellow;
            Handles.DrawWireArc(turretGun.transform.position, Vector3.up, Vector3.forward, 360, _ai.Range);

            Color c = Color.cyan;
            c.a = 0.35f;
            Handles.color = c;
            Handles.DrawSolidArc(turretGun.transform.position, turretGun.transform.up, turretGun.transform.forward, _ai.Angle / 2, _ai.Range);
            Handles.DrawSolidArc(turretGun.transform.position, turretGun.transform.up, turretGun.transform.forward, -_ai.Angle / 2, _ai.Range);

            Vector3 AngleA = _ai.DirFromAngle(-_ai.Angle / 2, false, turretGun);
            Vector3 AngleB = _ai.DirFromAngle(_ai.Angle / 2, false, turretGun);

            Handles.color = Color.red;
            Handles.DrawLine(turretGun.transform.position, turretGun.transform.position + AngleA * _ai.Range);
            Handles.DrawLine(turretGun.transform.position, turretGun.transform.position + AngleB * _ai.Range);

            if(_turret.Target)
            {
                Handles.DrawLine(turretGun.transform.position, _turret.Target.position);
            }
        }

#endregion

    }
}