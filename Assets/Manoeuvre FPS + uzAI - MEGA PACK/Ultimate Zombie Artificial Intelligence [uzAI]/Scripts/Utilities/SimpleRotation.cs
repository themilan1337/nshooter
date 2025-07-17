using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    public class SimpleRotation : MonoBehaviour
    {

        public float rotationSpeed = 3;
        

        void Update()
        {
            
            Vector3 newRot = new Vector3(transform.rotation.eulerAngles.x ,
                transform.rotation.eulerAngles.y + rotationSpeed,
                transform.rotation.eulerAngles.z );

            transform.eulerAngles = newRot;
        }
    }
}