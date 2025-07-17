using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class ProjectilesPooler : MonoBehaviour
    {
        public Dictionary<string, Queue<GameObject>> PoolDictionary = new Dictionary<string, Queue<GameObject>>();
        public List<Pool> projectilePool = new List<Pool>();

        public static ProjectilesPooler Instance;

        private void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            //creating pool for each object and adding it in our dictionary
            foreach(Pool p in projectilePool)
            {
                //Create a new pool for each object
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for(int i = 0; i< p.PoolSize; i++)
                {
                    GameObject obj = Instantiate(p.ProjectilePrefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localEulerAngles = Vector3.zero;

                    //add the object in the new pool
                    objectPool.Enqueue(obj);
                }
                //Finally Add our new pool in our dictionary
                PoolDictionary.Add(p.ProjectileName, objectPool);
            }
        }

        public GameObject SpawnFromPool(string _tag, Vector3 Position, Quaternion Rotation)
        {

            if (!PoolDictionary.ContainsKey(_tag))
            {
                Debug.Log("Nothing found");
                return null;
            }

            if (PoolDictionary[_tag].Count < 1)
            {
                Debug.Log("Pool Empty");
                return null;
            }

            GameObject objectToSpawn = PoolDictionary[_tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = Position;

            if (Rotation != Quaternion.identity)
                objectToSpawn.transform.rotation = Rotation;

            return objectToSpawn;
        }

        public void AddBackToQueue(GameObject projectile, string _tag)
        {
            //Add back
            PoolDictionary[_tag].Enqueue(projectile);

            //Disable as well
            projectile.SetActive(false);
        }
    }

    [System.Serializable]
    public class Pool
    {
        //Identify the Object with this Tag
        public string ProjectileName;
        //Actual object which will be instantiated 
        public GameObject ProjectilePrefab;
        //how many objects are required in the Pool
        public int PoolSize;

    }
}