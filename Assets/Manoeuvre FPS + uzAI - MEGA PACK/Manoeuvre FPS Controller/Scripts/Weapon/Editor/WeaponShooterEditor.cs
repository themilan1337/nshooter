using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(WeaponShooter))]
    public class WeaponShooterEditor : Editor
    {
        WeaponShooter _weaponShooter;
        LayerMask HitMask;
        List<hitInfo> _hitInfo = new List<hitInfo>();

        private void OnEnable()
        {
            _weaponShooter = (WeaponShooter)target;
        }

        public override void OnInspectorGUI()
        {
            //weapon texture
            Texture t = (Texture)Resources.Load("EditorContent/Shooter-icon");
            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //draw property drawer
            DrawWeaponProperties();
        }

        void DrawWeaponProperties()
        {
            //property Tab 1
            _weaponShooter.propertyTab1 = GUILayout.Toolbar(_weaponShooter.propertyTab1, new string[] { "Shooter", "Force", "Muzzle", "Bullet Hit" });

            switch (_weaponShooter.propertyTab1)
            {
                case 0:
                    //shooter
                    _weaponShooter.propertyName = "shooter";
                    _weaponShooter.propertyTab2 = 999;
                    break;
                case 1:
                    //force
                    _weaponShooter.propertyName = "force";
                    _weaponShooter.propertyTab2 = 999;
                    break;
                case 2:
                    //muzzle
                    _weaponShooter.propertyName = "muzzle";
                    _weaponShooter.propertyTab2 = 999;
                    break;
                case 3:
                    //Bullet
                    _weaponShooter.propertyName = "Bullet";
                    _weaponShooter.propertyTab2 = 999;
                    break;
            }

            //property Tab 2
            _weaponShooter.propertyTab2 = GUILayout.Toolbar(_weaponShooter.propertyTab2, new string[] { "Recoil", "Sounds", "Iron Sight", });

            switch (_weaponShooter.propertyTab2)
            {
                case 0:
                    //recoil
                    _weaponShooter.propertyName = "recoil";
                    _weaponShooter.propertyTab1 = 999;
                    break;
                case 1:
                    //Sounds
                    _weaponShooter.propertyName = "Sounds";
                    _weaponShooter.propertyTab1 = 999;
                    break;
                case 2:
                    //Iron sight
                    _weaponShooter.propertyName = "Iron sight";
                    _weaponShooter.propertyTab1 = 999;
                    break;
            }


            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            switch (_weaponShooter.propertyName)
            {
                case "shooter":
                    DrawShooterProperties();
                    break;
                case "force":
                    DrawForceProperties();
                    break;
                case "muzzle":
                    DrawMuzzleProperties();
                    break;
                case "Bullet":
                    DrawBulletProperties();
                    break;
                case "recoil":
                    DrawRecoilProperties();
                    break;
                case "Sounds":
                    DrawSoundsProperties();
                    break;
                case "Iron sight":
                    DrawIronsightProperties();
                    break;

            }


        }

        void DrawShooterProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            LayerMask hitMask = _weaponShooter.shooterProperties.hitMask;
            float ShootRange = _weaponShooter.shooterProperties.ShootRange;
            string ProjectileName = _weaponShooter.shooterProperties.ProjectileName;
            //int projectilesToShoot = _weaponShooter.shooterProperties.projectilesToShoot;
            float ProjectileForce = _weaponShooter.shooterProperties.ProjectileForce;
            float Spread = _weaponShooter.shooterProperties.Spread;

            if (_weaponShooter.ShootingType == ShootingType.Raycast) {

                EditorGUILayout.HelpBox("Assign Layers which this weapon can shoot.", MessageType.None);
                 hitMask = LayerMaskUtility.LayerMaskField("Hit Mask", _weaponShooter.shooterProperties.hitMask);

                EditorGUILayout.HelpBox("How far this weapon can shoot?", MessageType.None);
                 ShootRange = EditorGUILayout.Slider("Shooting Range", _weaponShooter.shooterProperties.ShootRange, 5, 200);

            }
            else
            {
                ProjectileName = EditorGUILayout.TextField("Projectile Name", _weaponShooter.shooterProperties.ProjectileName);
                //projectilesToShoot = EditorGUILayout.IntField("Projectiles To Shoot", _weaponShooter.shooterProperties.projectilesToShoot);
                ProjectileForce = EditorGUILayout.FloatField("Projectile Force", _weaponShooter.shooterProperties.ProjectileForce);
                Spread = EditorGUILayout.FloatField("Spread", _weaponShooter.shooterProperties.Spread);
            }

            EditorGUILayout.HelpBox("Number of bullets to fire per second.", MessageType.None);
            float FireRate = EditorGUILayout.FloatField("Fire Rate", _weaponShooter.shooterProperties.fireRate);

            EditorGUILayout.HelpBox("Shake Camera while shooting?.", MessageType.None);
            bool shakeCamera = EditorGUILayout.Toggle("Shake Camera", _weaponShooter.shooterProperties.shakeCamera);

            float ShakeAmount = EditorGUILayout.Slider("Shake Amount", _weaponShooter.shooterProperties.shakeAmount, 0.1f, 2f);

            EditorGUILayout.HelpBox("From how far zombies can hear while shooting.", MessageType.None);
            float HearRange = EditorGUILayout.Slider("Hear Range", _weaponShooter.shooterProperties.HearRange,1,100);

            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "shooter");

                _weaponShooter.shooterProperties.hitMask = hitMask;
                _weaponShooter.shooterProperties.ShootRange = ShootRange;
                _weaponShooter.shooterProperties.fireRate = FireRate;
                _weaponShooter.shooterProperties.shakeCamera = shakeCamera;
                _weaponShooter.shooterProperties.shakeAmount = ShakeAmount;
                _weaponShooter.shooterProperties.HearRange = HearRange;
                _weaponShooter.shooterProperties.ProjectileName = ProjectileName;
                //_weaponShooter.shooterProperties.projectilesToShoot = projectilesToShoot;
                _weaponShooter.shooterProperties.ProjectileForce = ProjectileForce;
                _weaponShooter.shooterProperties.Spread = Spread;

            }
        }

        void DrawForceProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Add force to rigidbodies on collission?", MessageType.None);
            bool ApplyForce = EditorGUILayout.Toggle("Apply Force", _weaponShooter.addForceProperties.applyForce);

            float forceAmount = EditorGUILayout.Slider("Force Amount", _weaponShooter.addForceProperties.forceAmount, 0.1f,250f);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Force");

                _weaponShooter.addForceProperties.applyForce = ApplyForce;
                _weaponShooter.addForceProperties.forceAmount = forceAmount;
            }
        }

        void DrawMuzzleProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Add Custom Muzzle Flash for this weapon and assign it's location.", MessageType.None);

            ParticleSystem MuzzleFlash = (ParticleSystem)EditorGUILayout.ObjectField("Muzzle Flash", _weaponShooter.muzzleProperties.muzzleFlash, typeof(ParticleSystem));
            Transform MuzzleFlashLocation = (Transform)EditorGUILayout.ObjectField("Muzzle Flash Location", _weaponShooter.muzzleProperties.muzzleFlashLocation, typeof(Transform));

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Muzzle");
                _weaponShooter.muzzleProperties.muzzleFlash = MuzzleFlash;
                _weaponShooter.muzzleProperties.muzzleFlashLocation = MuzzleFlashLocation;

            }
        }

        void DrawBulletProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Maximum and Minimum Damage to give to the uzAI Zombie.", MessageType.None);

            int maxDamage = EditorGUILayout.IntField("Maximum Damage", _weaponShooter.bulletHitProperties.maxDamage);
            int minDamage = EditorGUILayout.IntField("Minimum Damage", _weaponShooter.bulletHitProperties.minDamage);

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //draw hit info list
            DrawHitInfoList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "hit info");

                _weaponShooter.bulletHitProperties.maxDamage = maxDamage;
                _weaponShooter.bulletHitProperties.minDamage = minDamage;
                _weaponShooter.bulletHitProperties.hits = _hitInfo;

            }
        }

        void DrawHitInfoList()
        {
            _hitInfo = _weaponShooter.bulletHitProperties.hits;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Assign Different tags for different Impact Particles and Sounds.", MessageType.Info);

            EditorGUILayout.LabelField("Total Hit Tags : " + _hitInfo.Count, EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button("Add Tag"))
            {
                _hitInfo.Add(new hitInfo());
            }

            EditorGUILayout.Space();

            for (int i = 0; i < _hitInfo.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal("box");
                _hitInfo[i].hitTag = EditorGUILayout.TextField("Hit Tag", _weaponShooter.bulletHitProperties.hits[i].hitTag);

                if (GUILayout.Button("X"))
                {
                    _hitInfo.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();

                _hitInfo[i].hitParticle = (GameObject)EditorGUILayout.ObjectField("Hit Particle", _weaponShooter.bulletHitProperties.hits[i].hitParticle, typeof(GameObject));

                //Draw hit audio clip
                DrawHitAudioClip(i);


                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

            EditorGUILayout.EndVertical();

        }

        void DrawHitAudioClip(int i)
        {
            EditorGUILayout.BeginVertical("window");

            EditorGUILayout.HelpBox("Add Audio Clips for hit effect", MessageType.Info);

            if (GUILayout.Button("Add Audio Clip"))
            {
                AudioClip newAC = null;
                _hitInfo[i].hitAudioClip.Add(newAC);
            }

            if (_hitInfo[i].hitAudioClip.Count == 0)
                EditorGUILayout.HelpBox("Add at least 1 Audio Clip for this tag's hit effect!", MessageType.Error);

            for (int j = 0; j < _hitInfo[i].hitAudioClip.Count; j++)
            {

                EditorGUILayout.BeginHorizontal("box");

                _hitInfo[i].hitAudioClip[j] = (AudioClip)EditorGUILayout.ObjectField("Hit Audio " + j, _weaponShooter.bulletHitProperties.hits[i].hitAudioClip[j], typeof(AudioClip));

                if (GUILayout.Button("X"))
                {
                    _hitInfo[i].hitAudioClip.RemoveAt(j);
                }

                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();

        }

        void DrawRecoilProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Select the type of Recoil you want. Procedural or Animated.", MessageType.None);
            AnimationType RecoilType = (AnimationType) EditorGUILayout.EnumPopup("Recoil Type", _weaponShooter.recoilProperties.RecoilType);

            float recoilPositionFactor = 0;
            float recoilRotationFactor = 0;
            float Speed = 0;
            string FireRecoilAnimationName = "";
            Animation RecoilAnimation = null;
			float AnimationSpeed = 0;

            if (RecoilType == AnimationType.Procedural)
            {
                EditorGUILayout.HelpBox("Define how much position recoil will happen while shooting.", MessageType.None);
                 recoilPositionFactor = EditorGUILayout.FloatField("Recoil Position Factor", _weaponShooter.recoilProperties.recoilPositionFactor);

                EditorGUILayout.HelpBox("Define how much rotation recoil will happen while shooting.", MessageType.None);
                 recoilRotationFactor = EditorGUILayout.FloatField("Recoil Rotation Factor", _weaponShooter.recoilProperties.recoilRotationFactor);

                EditorGUILayout.HelpBox("Speed at which gun will come back to it's original position.", MessageType.None);
                 Speed = EditorGUILayout.FloatField("Recoil Speed", _weaponShooter.recoilProperties.speed);
            }
            else
            {
                RecoilAnimation = (Animation) EditorGUILayout.ObjectField("Recoil Animation", _weaponShooter.recoilProperties.RecoilAnimation, typeof(Animation));

                EditorGUILayout.HelpBox("Animation Name to be Played.", MessageType.None);
                FireRecoilAnimationName = EditorGUILayout.TextField("Fire Recoil Animation Name", _weaponShooter.recoilProperties.FireRecoilAnimationName);
           
				EditorGUILayout.HelpBox("Animation Speed which will also determines the Fire Rate.", MessageType.None);
				AnimationSpeed = EditorGUILayout.FloatField ("Animation Speed", _weaponShooter.recoilProperties.AnimationSpeed);
			
				AnimationSpeed = Mathf.Clamp (AnimationSpeed, 0.01f, AnimationSpeed);
			}


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Recoil");
                _weaponShooter.recoilProperties.RecoilType = RecoilType;
                _weaponShooter.recoilProperties.RecoilAnimation = RecoilAnimation;
                _weaponShooter.recoilProperties.FireRecoilAnimationName = FireRecoilAnimationName;
				_weaponShooter.recoilProperties.AnimationSpeed = AnimationSpeed;

                _weaponShooter.recoilProperties.recoilPositionFactor = recoilPositionFactor;
                _weaponShooter.recoilProperties.recoilRotationFactor = recoilRotationFactor;
                _weaponShooter.recoilProperties.speed = Speed;
            }
        }

        void DrawSoundsProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Assign Sounds for Equip, Reload and Fire SFX", MessageType.Info);

            AudioClip EquipSound = (AudioClip)EditorGUILayout.ObjectField("Equip Sound",_weaponShooter.weaponSoundProperties.equipSound , typeof(AudioClip));
            AudioClip reloadSound = (AudioClip)EditorGUILayout.ObjectField("Reload Sound",_weaponShooter.weaponSoundProperties.reloadSound , typeof(AudioClip));
            AudioClip fireSound = (AudioClip)EditorGUILayout.ObjectField("Fire Sound", _weaponShooter.weaponSoundProperties.fireSound , typeof(AudioClip));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("The above assigned SFX will be played with random pitch everytime set below." +
                "In this way, you can have same SFX for different weapons with different pitch!", MessageType.Info);

            float maxPitchVariation = EditorGUILayout.Slider("Max Pitch Variation", _weaponShooter.weaponSoundProperties.maxPitchVariation, -3,3);
            float minPitchVariation = EditorGUILayout.Slider("Min Pitch Variation", _weaponShooter.weaponSoundProperties.minPitchVariation, -3,3);

            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Sounds");

                _weaponShooter.weaponSoundProperties.equipSound = EquipSound;
                _weaponShooter.weaponSoundProperties.reloadSound = reloadSound;
                _weaponShooter.weaponSoundProperties.fireSound = fireSound;

                _weaponShooter.weaponSoundProperties.maxPitchVariation = maxPitchVariation;
                _weaponShooter.weaponSoundProperties.minPitchVariation = minPitchVariation;
            }
        }

       
        void DrawIronsightProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Do you want to use Iron Sight ?", MessageType.None);
            bool useIronsight = EditorGUILayout.Toggle("Use Iron Sight", _weaponShooter.ironSightProperties.useIronsight);

            float tweenFOVTo = _weaponShooter.ironSightProperties.tweenFOVTo;
            float tweenTimescaleTo = _weaponShooter.ironSightProperties.tweenTimescaleTo;
            Vector3 tweenPosition = _weaponShooter.ironSightProperties.tweenPosition;
            Vector3 tweenRotation = _weaponShooter.ironSightProperties.tweenRotation;

            if (useIronsight)
            {
                EditorGUILayout.HelpBox("Field Of View will be Tweened to this value while using IronSight", MessageType.None);
                tweenFOVTo = EditorGUILayout.Slider("Tweened F.O.V", _weaponShooter.ironSightProperties.tweenFOVTo, 1, 75);

                EditorGUILayout.HelpBox("Position of the Weapon while in ironsight mode.", MessageType.None);
                tweenPosition = EditorGUILayout.Vector3Field("Tween Position", _weaponShooter.ironSightProperties.tweenPosition);

                EditorGUILayout.HelpBox("Rotation of the Weapon while in ironsight mode.", MessageType.None);
                tweenRotation = EditorGUILayout.Vector3Field("Tween Rotation", _weaponShooter.ironSightProperties.tweenRotation);

                EditorGUILayout.HelpBox("TimeScale will be slowed down to this value while using IronSight \n" +
                    "Note : 0.5 is best value for slo mo effect, still you can experiment with different values.", MessageType.None);
                tweenTimescaleTo = EditorGUILayout.Slider("Tweened Time Scale", _weaponShooter.ironSightProperties.tweenTimescaleTo, 0.1f, 1f);

            }


            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Ironsight");

                _weaponShooter.ironSightProperties.useIronsight = useIronsight;
                _weaponShooter.ironSightProperties.tweenFOVTo = tweenFOVTo;
                _weaponShooter.ironSightProperties.tweenTimescaleTo = tweenTimescaleTo;

                _weaponShooter.ironSightProperties.tweenPosition = tweenPosition;
                _weaponShooter.ironSightProperties.tweenRotation = tweenRotation;

            } 
        }

       
    }
}