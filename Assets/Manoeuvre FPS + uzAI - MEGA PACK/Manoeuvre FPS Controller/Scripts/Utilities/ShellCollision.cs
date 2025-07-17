using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class ShellCollision : MonoBehaviour
    {

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(.2f);

            GetComponent<Rigidbody>().isKinematic = false;
        }

        public void ApplyForce(Vector3 point, float f)
        {
            GetComponent<Rigidbody>().AddForce(transform.right, ForceMode.Force);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.gameObject.GetComponent<ShellCollision>())
                transform.parent = null;
        }   

    }
}