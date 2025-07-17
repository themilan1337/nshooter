using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    public class SpawnHelper : MonoBehaviour
    {
        public int _myWave;
        public Transform _mySpawnPoint;
        public WaveSpawner _waveSpawner;
        public Transform Target;

        // Use this for initialization
        void Start()
        {

        }

        public void Initialize(Transform t, int w, Transform p, WaveSpawner ws)
        {
            Target = t;
            _myWave = w;
            _mySpawnPoint = p;
            _waveSpawner = ws;
        }

        public void DetachFromWave()
        {
            //reduce 1 allowance from the Spawn Points List
            for (int i = 0; i < _waveSpawner._SpawnPoints.Count; i++)
            {
                if (_waveSpawner._SpawnPoints[i].SpawnPoint == _mySpawnPoint)
                {

                    _waveSpawner._SpawnPoints[i].currentlySpawned--;

                    //remove this Zombie
                    if (_waveSpawner._Waves[_myWave]._myZombies.Contains(gameObject))
                        _waveSpawner._Waves[_myWave]._myZombies.Remove(gameObject);

                    //make sure to start the next wave
                    _waveSpawner.StartWaveCoroutine(false);

                }
            }

        }

    }
}