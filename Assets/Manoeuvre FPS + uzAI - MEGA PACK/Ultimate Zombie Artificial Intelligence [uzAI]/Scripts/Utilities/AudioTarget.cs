using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    public class AudioTarget : MonoBehaviour
    {
        public bool enableAtStart;

        public AudioClip clipToPlay;
        public float AudioRange = 5f;

        // Use this for initialization
        IEnumerator Start()
        {
            //wait half a second for everything else to load
            yield return new WaitForSeconds(0.5f);

            //now if enable at start is checked, start the audio target
            if (enableAtStart)
                EnableAudioTarget();

        }

        /// <summary>
        /// Call this method to Invoke This Audio Target
        /// </summary>
        public void EnableAudioTarget()
        {
            //instantiate new object for trigger
            GameObject myAwarenessTrigger = new GameObject();
            myAwarenessTrigger.transform.SetParent(this.transform);
            myAwarenessTrigger.transform.localPosition = Vector3.zero;
            //add trigger and set radius
            myAwarenessTrigger.AddComponent<SphereCollider>().radius = AudioRange;
            myAwarenessTrigger.GetComponent<SphereCollider>().isTrigger = true;

            //set layer, tag and name
            myAwarenessTrigger.layer = LayerMask.NameToLayer("AwarenessTrigger");
            myAwarenessTrigger.tag = "AwarenessTrigger";
            myAwarenessTrigger.name = "AwarenessTrigger";

            //Play Audio Clip
            if(clipToPlay)
                AudioSource.PlayClipAtPoint(clipToPlay, transform.position);

            //Start Lerping Radius
            StartCoroutine(LerpRadius(myAwarenessTrigger.GetComponent<SphereCollider>()));
        }

        IEnumerator LerpRadius(SphereCollider col)
        {
            float radius = col.radius;
            float et = 0;

            while (et < 0.3f)
            {

                col.radius = Mathf.Lerp(col.radius, 0, et/0.3f);
                et += Time.deltaTime;

                yield return null;
            }
            col.gameObject.name = "Destination";
            col.gameObject.tag = "Destination";
            col.gameObject.layer = LayerMask.NameToLayer("Destination");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(transform.position, AudioRange);
        }
    }
}