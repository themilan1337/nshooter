using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class MainMenuAudioManager : MonoBehaviour
    {
        public string MainMenuAudioClip = "Main Menu";

        public List<AudioData> audioDatas = new List<AudioData>();

        public static MainMenuAudioManager Instance;

        // Use this for initialization
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(this);

            //add audio sources
            foreach (AudioData _ad in audioDatas)
                _ad.AddAudioSource(gameObject);

            //play main menu audio clip
            if (!string.IsNullOrEmpty(MainMenuAudioClip))
            {
                //loop the main menu music
                foreach (AudioData _ad in audioDatas)
                {
                    if (_ad.ClipName == MainMenuAudioClip)
                        _ad._mySource.loop = true;
                }

                //finally play the clip
                PlayAudioClip(MainMenuAudioClip);
            }

        }

        /// <summary>
        /// Pass the clip name to play.
        /// </summary>
        /// <param name="ClipName"></param>
        public void PlayAudioClip(string ClipName)
        {
            foreach(AudioData _ad in audioDatas)
            {
                if(_ad.ClipName == ClipName)
                {
                    _ad.PlayAudioClip();

                }
            }
        }

        public void DestroyAudioManager()
        {
            foreach (AudioData _ad in audioDatas)
            {
                _ad._mySource.volume = 0;
            }

            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class AudioData
    {
        public string ClipName;
        public AudioClip AudioClip;
        public float Volume = 1;
        public float maxPitch;
        public float minPitch;

        public AudioSource _mySource;

        public void AddAudioSource(GameObject obj)
        {
            _mySource = obj.AddComponent<AudioSource>();

            _mySource.volume = Volume;
            _mySource.clip = AudioClip;
        }

        public void PlayAudioClip()
        {
            _mySource.pitch = Random.Range(minPitch, maxPitch);
            _mySource.Play();
        }
    }
}