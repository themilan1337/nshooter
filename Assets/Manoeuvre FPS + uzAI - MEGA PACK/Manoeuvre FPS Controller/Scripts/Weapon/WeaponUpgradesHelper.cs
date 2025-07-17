using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class WeaponUpgradesHelper : MonoBehaviour
    {
        WeaponHandler _handler;

        // Use this for initialization
        public void UpgradeWeaponHandler()
        {
            //get WeaponHandler  class reference
            _handler = GetComponent<WeaponHandler>();

            //get weapon name and set attributes
            if (_handler)
            {
                for(int i = 0; i< _handler.Weapons.Count; i++)
                {
                    if(_handler.Weapons[i].WeaponObject.GetComponent<WeaponShooter>())
                        UpgradeShooterWeapon(_handler.Weapons[i].WeaponName, _handler.Weapons[i].WeaponObject.GetComponent<WeaponShooter>(), _handler.Weapons[i]);
                    else
                        UpgradeMeleeWeapon(_handler.Weapons[i].WeaponName, _handler.Weapons[i].WeaponObject.GetComponent<WeaponMelee>());
                    
                }

            }

        }

        void UpgradeShooterWeapon(string WeaponName, WeaponShooter _shooter, WeaponClassification _wc)
        {
          
            //set shooting range
            _shooter.shooterProperties.ShootRange = PlayerPrefs.GetFloat(WeaponName + "_ShootRange",
                _shooter.shooterProperties.ShootRange);
        
            //set fire rate
            _shooter.shooterProperties.fireRate = PlayerPrefs.GetFloat(WeaponName + "_fireRate",
                _shooter.shooterProperties.fireRate);

            //set hear range
            _shooter.shooterProperties.HearRange = PlayerPrefs.GetFloat(WeaponName + "_HearRange", 
                _shooter.shooterProperties.HearRange);

            //set damage
            _shooter.bulletHitProperties.minDamage = (int) PlayerPrefs.GetFloat(WeaponName + "_Damage", _shooter.bulletHitProperties.minDamage);
            _shooter.bulletHitProperties.maxDamage = (int) PlayerPrefs.GetFloat(WeaponName + "_Damage", _shooter.bulletHitProperties.maxDamage);

            //set clip count
            _wc.Capacity = (int)PlayerPrefs.GetFloat(WeaponName + "_AmmoCapacity", _wc.Capacity);

             

        }

        void UpgradeMeleeWeapon(string WeaponName, WeaponMelee _melee)
        {
            //set melee attack distance
            _melee._MeleeAttackRange.AttackDistance = PlayerPrefs.GetFloat(WeaponName + "_AttackDistance",
                _melee._MeleeAttackRange.AttackDistance);

            //set damage
            _melee._MeleeAttack.maxDamage = (int)PlayerPrefs.GetFloat(WeaponName + "_Damage", _melee._MeleeAttack.maxDamage);
            _melee._MeleeAttack.minDamage = (int)PlayerPrefs.GetFloat(WeaponName + "_Damage", _melee._MeleeAttack.minDamage);

        }
    }



}