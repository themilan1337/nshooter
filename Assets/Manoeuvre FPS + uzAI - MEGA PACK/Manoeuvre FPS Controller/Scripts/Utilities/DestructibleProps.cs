using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class DestructibleProps : MonoBehaviour
    {

        [Range(0, 200)]
        public int Health = 100;
        [Range(0, 500)]
        public float _destructionForce = 10f;
        [Range(0, 50)]
        public float range = 5f;

        public bool fadeMesh = true;

        [Range(0, 20)]
        public float fadeMeshDelay = 5f;
        [Range(0, 3f)]
        public float fadeMeshDuration = 0.5f;

        public ParticleSystem destructionFX;
        public AudioClip destructionSFX;
        public Material faderMaterial;

        List<Renderer> childRenderers = new List<Renderer>();

        public Healthbar healthBar;

        public bool hasExploded = false;
        // Use this for initialization
        void Awake()
        {
            //Get All colliders in our list
            foreach (Renderer R in GetComponentsInChildren<Renderer>())
            {

                childRenderers.Add(R);
            }
        }

        private void Start()
        {

        }

        public void OnDamage(int amount)
        {
            if (healthBar)
                healthBar.StartLerp();

            Health -= amount;

            if (Health <= 0)
                DestroyObject();
        }

        void DestroyObject()
        {
            //Play Dialogue
            gc_PlayerDialoguesManager.Instance.PlayDialogueClip(gc_PlayerDialoguesManager.DialogueType.Kills);

            //disable collider
            GetComponent<Collider>().enabled = false;

            for (int i = 0; i < childRenderers.Count; i++)
            {
                //enable colliders
                if (childRenderers[i].GetComponent<Collider>())
                    childRenderers[i].GetComponent<Collider>().enabled = true;

                //add rigidbody
                if (childRenderers[i].gameObject.GetComponent<Rigidbody>() == null)
                    childRenderers[i].gameObject.AddComponent<Rigidbody>();

                //add force
                childRenderers[i].gameObject.GetComponent<Rigidbody>().AddExplosionForce(_destructionForce, transform.position, range);
                //disable collisions
                childRenderers[i].gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;

            }

            //init particles
            ParticleSystem pfx = Instantiate(destructionFX) as ParticleSystem;
            pfx.transform.SetParent(this.transform);
            pfx.transform.localPosition = Vector3.zero;
            pfx.transform.localEulerAngles = Vector3.zero;

            //emit particles
            pfx.Play();

            //play clip
            AudioSource.PlayClipAtPoint(destructionSFX, transform.position);

            //if we have drop items script then drop its contents
            if (GetComponent<DropItems>())
                GetComponent<DropItems>().Drop(fadeMeshDelay);

            //start fading
            if (fadeMesh)
                StartCoroutine(FadeMesh());
        }

        IEnumerator FadeMesh()
        {
            yield return new WaitForSeconds(fadeMeshDelay);

            for (int i = 0; i < childRenderers.Count; i++)
            {
                //swap shaders
                childRenderers[i].GetComponent<Renderer>().material.shader = faderMaterial.shader;
            }

            //now start fade
            float et = 0;
            Color c = Color.white;

            while (et < fadeMeshDuration)
            {
                c.a = Mathf.Lerp(c.a, 0, et / fadeMeshDuration);
                foreach (Renderer R in childRenderers)
                {
                    if (R)
                        R.GetComponent<Renderer>().material.color = c;
                }

                et += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < childRenderers.Count; i++)
            {
                Destroy(childRenderers[i].gameObject);
            }

            yield return new WaitForEndOfFrame();

            Destroy(gameObject);
        }

        /// <summary>
        /// Prepare for destruction!!
        /// </summary>
        public void OnExplosion(float forceAmt, Vector3 pos, float radius)
        {
            if (hasExploded)
                return;
            else
                hasExploded = true;

            GetComponent<Collider>().enabled = false;

            for (int i = 0; i < childRenderers.Count; i++)
            {
                //detach them all
                childRenderers[i].transform.SetParent(null);

                //enable colliders
                if (childRenderers[i].GetComponent<Collider>())
                    childRenderers[i].GetComponent<Collider>().enabled = true;

                //add rigidbody
                if (childRenderers[i].gameObject.GetComponent<Rigidbody>() == null)
                {
                    childRenderers[i].gameObject.AddComponent<Rigidbody>();
                    //also add force
                    childRenderers[i].gameObject.GetComponent<Rigidbody>().AddExplosionForce(forceAmt, pos, radius);
                }
            }

            //start fading
            if (fadeMesh)
                StartCoroutine(FadeMesh());
        }
    }
}