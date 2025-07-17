using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class DropItems : MonoBehaviour
    {

        public List<GameObject> _DropItems = new List<GameObject>();
        [Range(1f, 10f)]
        public float dropRange = 3f;

        // Use this for initialization
        void Start()
        {

        }

        public void Drop(float delay)
        {
            //Drop Items
            if (_DropItems.Count > 0)
                StartCoroutine(SpawnItems(delay));
        }

        IEnumerator SpawnItems(float delay)
        {
            List<GameObject> items = new List<GameObject>();

            for (int i = 0; i < _DropItems.Count; i++)
            {
                GameObject g = Instantiate(_DropItems[i]) as GameObject;

                Vector3 randomPos = Random.insideUnitSphere * dropRange;
                randomPos = new Vector3(randomPos.x, transform.position.y, randomPos.z);
                Vector3 newPos = new Vector3(transform.position.x + randomPos.x, transform.position.y + 1, randomPos.z + transform.position.z);
                g.transform.position = newPos;
                g.transform.localScale = Vector3.zero;

                items.Add(g);
            }

            yield return new WaitForSeconds(delay);

            float et = 0;
            Vector3 scale = Vector3.zero;

            while (et < 0.25f)
            {
                scale = Vector3.Lerp(scale, Vector3.one, et / 0.25f);

                foreach (GameObject g in items)
                {
                    g.transform.localScale = scale;
                }

                et += Time.deltaTime;

                yield return null;
            }
        }


    }
}