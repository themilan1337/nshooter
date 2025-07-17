using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    public class WaveSpawner : MonoBehaviour
    {
        //What all the Zombies will run after
        public Transform Target;

        public float maxSpawnDistance = 15f;

        public float UIDuration = 2f;

        public float WaveStartDelay = 5f;
        
        //All the Waves
        public List<Wave> _Waves = new List<Wave>();

        //All the Spawn Points
        public List<WaveSpawnPoint> _SpawnPoints = new List<WaveSpawnPoint>();

        public WaveSoundProperties _soundProperties;

        //Current wave we are having
        public int _currentWave = 0;

        //Current Wave's total Zombies
        public int _currentWaveTotalZombies = 0;

        //Just a check how many zombies have been spawned
        public int _totalZombiesSpawned = 0;

        //Poor man's Singelton
        public static WaveSpawner Instance;
        float _delayTimer = 0;
        CanvasGroup WavesHUD;

        //Editor var
        [HideInInspector]
        public int tabCount;

        private void Awake()
        {
            Instance = this;

            //init Sound properties
            _soundProperties.Initialize(gameObject);

        }

        // Use this for initialization
        void Start()
        {
            //find UI
            WavesHUD = GameObject.Find("WavesHUD").GetComponent<CanvasGroup>();

            //intialize all waves
            InitializeWaves();

            //after init spawn All waves
            SpawnAllWaves();

            StartWaveCoroutine(true);
        }

        public void StartWaveCoroutine(bool shouldPlayStartClip)
        {
            //start the Wave!
            StartCoroutine(StartWave(shouldPlayStartClip));
        }

        /// <summary>
        /// We first Initialize All the waves by First Spawning all the "Types" of Zombies
        /// </summary>
        public void InitializeWaves()
        {
            //make sure everything is setup properly
            if (Target == null)
            {
                Debug.LogWarning("Please assign a Target");
                return;
            }

            if(_SpawnPoints.Count ==0)
            {
                Debug.LogWarning("Please assign Spawn Points");
                return;
            }

            if(_Waves.Count == 0)
            {
                Debug.LogWarning("Please Create at least one Wave to start!");
                return;
            }

            //look inside each wave
            for (int i = 0; i < _Waves.Count; i++)
            {
                //Instantiate every Zombie type
                for (int z = 0; z < _Waves[i]._ZombieTypes.Count; z++)
                {
                    //instantiate each zombie of each wave at Initialization
                    GameObject Zombie = Instantiate(_Waves[i]._ZombieTypes[z].gameObject) as GameObject;

                    //rename for identification
                    Zombie.name = Zombie.name + " Wave : " + i;

                    //set it as our child
                    Zombie.transform.SetParent(this.transform);
                    Zombie.transform.localPosition = Vector3.zero;
                    Zombie.transform.localEulerAngles = Vector3.zero;

                    //disable it
                    Zombie.SetActive(false);

                    //add it in list
                    _Waves[i]._myZombies.Add(Zombie);
                    _totalZombiesSpawned++;
                }
            }
        }

        /// <summary>
        /// Now We Spawn All Waves All Zombies based on No Of Zombies.
        /// </summary>
        public void SpawnAllWaves()
        {
            //spawn all waves all zombies at once
            for (int w = 0; w < _Waves.Count; w++)
            {
                //set current wave
                _currentWave = w;

                //set this to already instantiated amount
                _currentWaveTotalZombies = _Waves[_currentWave]._myZombies.Count;

                //we will keep spawning unless we have reached the Total number of Zombies
                while (_currentWaveTotalZombies < _Waves[_currentWave].NoOfZombies)
                {
                    for (int i = 0; i < _Waves[_currentWave]._ZombieTypes.Count; i++)
                    {
                        //double check the _currentWaveTotalZombies
                        if (_currentWaveTotalZombies >= _Waves[_currentWave].NoOfZombies)
                            break; //exit immediately

                        //give each zombie chance of 50% to get spawned
                        if (Random.value <= 0.5f)
                        {
                            //Instantitate
                            GameObject Zombie = Instantiate(_Waves[_currentWave]._ZombieTypes[i].gameObject) as GameObject;

                            //rename for identification
                            Zombie.name = Zombie.name + " Wave : " + _currentWave;

                            //set it as our child
                            Zombie.transform.SetParent(this.transform);
                            Zombie.transform.localPosition = Vector3.zero;
                            Zombie.transform.localEulerAngles = Vector3.zero;

                            //disable it
                            Zombie.SetActive(false);

                            //add it in list of current wave
                            _Waves[_currentWave]._myZombies.Add(Zombie);

                            //we only increment if this happens
                            _currentWaveTotalZombies++;
                            _totalZombiesSpawned++;

                        }
                    }
                }

            }

            //reset current wave
            _currentWave = 0;
            //reset current wave total Zombies
            _currentWaveTotalZombies = 0;
        }

        /// <summary>
        /// This method will be called whenever we are starting the Wave
        /// </summary>
        public IEnumerator StartWave(bool shouldPlayStartClip)
        {
            //As soon as Last Zombie has been killed of current wave
            if (_Waves[_currentWave]._myZombies.Count == 0 || _Waves[_currentWave]._myZombies[0] == null)
            {
                //start next wave
                _currentWave++;

                //reset total Zombies
                _currentWaveTotalZombies = 0;

                //reset all spawn points allowance
                for(int i = 0; i < _SpawnPoints.Count; i++)
                    _SpawnPoints[i].currentlySpawned = 0;

                //play end clip
                _soundProperties.PlayClip(WaveSoundProperties.WaveClipType.End);

                //reset timer
                _delayTimer = 0;

                //make sure check flag to play start clip
                shouldPlayStartClip = true;

                if (WavesHUD)
                    StartCoroutine(WaveUIText("Wave " + _currentWave + " Survived !"));

                Debug.Log("All Zombies Dead, Starting next wave : " + _currentWave);
            }

            //check if all waves have been completed
            if (_currentWave >= _Waves.Count)
            {
                //play Finish clip
                _soundProperties.PlayClip(WaveSoundProperties.WaveClipType.Finish);

                Debug.Log("Waves Finished!!!");

                if (WavesHUD)
                    StartCoroutine(WaveUIText("All waves Survived !"));

                StopCoroutine(StartWave(false)); //exit

                ///make sure sound is disabled
                shouldPlayStartClip = false;
            }

            //if all the zombies have been spawned
            if(_currentWave < _Waves.Count)
            {
                if (_currentWaveTotalZombies >= _Waves[_currentWave].NoOfZombies)
                    yield return null; // exit

                while (_delayTimer <= WaveStartDelay)
                {
                    _delayTimer += Time.deltaTime;
                    yield return null;
                }

                //spawn all zombies
                SpawnThisWaveZombies(shouldPlayStartClip);

            }
                
        }

        void SpawnThisWaveZombies(bool shouldPlayStartClip)
        {
            //Play Start Clip
            if (shouldPlayStartClip)
            {
                _soundProperties.PlayClip(WaveSoundProperties.WaveClipType.Start);

                if (WavesHUD)
                    StartCoroutine(WaveUIText("Wave " + (_currentWave+1)));
            }
            foreach (GameObject g in _Waves[_currentWave]._myZombies)
            {
                if (g.GetComponent<SpawnHelper>() == null)
                {
                    //get the spawn point
                    Transform _sPoint = DetermineSpawnPoint();

                    //if we don't have a spawn point
                    if (_sPoint == null)
                    {
                        Debug.Log("No spawn Point Found");
                        break; // we stop this wave
                    }

                    //add and cache Spawn Helper
                    SpawnHelper _sh = g.AddComponent<SpawnHelper>();
                    _sh.Initialize(Target, _currentWave, _sPoint, this);

                    //send the Zombie to that Spawn Point
                    g.transform.SetParent(_sPoint);
                    g.transform.localPosition = Vector3.zero;
                    g.transform.localEulerAngles = Vector3.zero;

                    //and enable it
                    g.SetActive(true);

                    //and increment counter
                    _currentWaveTotalZombies++;
                }

            }
        }

        /// <summary>
        /// loops through all the Spawn Points, finds the closest one and returns it
        /// </summary>
        Transform DetermineSpawnPoint()
        {
            //we went through all the spawn points
            for(int i = 0; i < _SpawnPoints.Count; i++) {

                if(_SpawnPoints[i].currentlySpawned < _SpawnPoints[i].maxAllowed )
                {
                    //we see it's distance from our target
                    if(Vector3.Distance(_SpawnPoints[i].SpawnPoint.position, Target.position) <= maxSpawnDistance)
                    {
                        //increment currently Spawned value
                        _SpawnPoints[i].currentlySpawned++;

                        //return this point
                        return _SpawnPoints[i].SpawnPoint;
                    }

                }
                
            }

            //else return null
            return null;
        }

        IEnumerator WaveUIText(string text)
        {
            float et = 0;

            WavesHUD.GetComponentInChildren<UnityEngine.UI.Text>().text = text;

            while (et < 0.2f)
            {
                WavesHUD.alpha = Mathf.Lerp(WavesHUD.alpha, 1, et / 0.2f);

                et += Time.deltaTime;
                yield return null;
            }
            WavesHUD.alpha = 1f;

            yield return new WaitForSeconds(UIDuration);

            et = 0;

            while (et < 0.2f)
            {
                WavesHUD.alpha = Mathf.Lerp(WavesHUD.alpha, 0, et / 0.2f);

                et += Time.deltaTime;
                yield return null;
            }
            WavesHUD.alpha = 0f;
        }

    }

    [System.Serializable]
    public class Wave
    {
        public int NoOfZombies = 5;
        public List<uzAIZombieStateManager> _ZombieTypes = new List<uzAIZombieStateManager>();

        public List<GameObject> _myZombies = new List<GameObject>();

    }

    [System.Serializable]
    public class WaveSpawnPoint
    {
        public Transform SpawnPoint;
        public int maxAllowed = 2;
        public int currentlySpawned = 0;
    }

    [System.Serializable]
    public class WaveSoundProperties
    {
        public enum WaveClipType
        {
            Start,
            End,
            Finish
        }


        public AudioClip WaveStartClip;
        public AudioClip WaveEndClip;
        public AudioClip WaveFinishClip;

        AudioSource source;

        public void Initialize(GameObject myGameObject)
        {
            source = myGameObject.AddComponent<AudioSource>();
        }

        public void PlayClip(WaveClipType _type)
        {
            switch (_type)
            {
                case WaveClipType.Start:
                    source.PlayOneShot(WaveStartClip, 1);
                    break;

                case WaveClipType.End:
                    source.PlayOneShot(WaveEndClip, 1);
                    break;

                case WaveClipType.Finish:
                    source.PlayOneShot(WaveFinishClip, 1);
                    break;
            }
        }

    }

}