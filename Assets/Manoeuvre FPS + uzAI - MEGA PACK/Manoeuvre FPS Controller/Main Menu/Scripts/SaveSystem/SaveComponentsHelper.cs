using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class SaveComponentsHelper : MonoBehaviour
    {
        public List<WeaponShooter> ShooterWeapons = new List<WeaponShooter>();
        public List<WeaponMelee> MeleeWeapons = new List<WeaponMelee>();
        public List<WeaponThrowable> ThrowableItems = new List<WeaponThrowable>();

        public Transform ReturnShooterWeapon(string WpnName)
        {
            for (int i = 0; i < ShooterWeapons.Count; i++)
            {
                if (ShooterWeapons[i].WeaponName == WpnName)
                {
                    return ShooterWeapons[i].transform;
                }
            }

            return null;
        }

        public Transform ReturnMeleeWeapon(string WpnName)
        {
            for (int i = 0; i < MeleeWeapons.Count; i++)
            {
                if (MeleeWeapons[i].WeaponName == WpnName)
                {
                    return MeleeWeapons[i].transform;
                }
            }

            return null;
        }

        public WeaponThrowable ReturnThrowableWeapon(string WpnName)
        {
            for (int i = 0; i < ThrowableItems.Count; i++)
            {
                if (ThrowableItems[i].ItemName == WpnName)
                {
                    return ThrowableItems[i];
                }
            }

            return null;
        }

    }
}