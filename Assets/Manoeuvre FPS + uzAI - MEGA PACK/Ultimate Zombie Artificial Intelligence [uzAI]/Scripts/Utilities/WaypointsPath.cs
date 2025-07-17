using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    public class WaypointsPath : MonoBehaviour
    {
        public List<Transform> waypoints;

        private void Awake()
        {
            //set the layer as Waypoint layer
            this.gameObject.layer = 11;

            Transform[] child = GetComponentsInChildren<Transform>();
            foreach(Transform t in child)
            {
                t.tag = "Destination";
                t.gameObject.AddComponent<SphereCollider>().isTrigger = true;
                t.gameObject.layer = 11;
            }

            Collider c = GetComponent<Collider>();
            if (c)
                Destroy(c);
        }
    }
}