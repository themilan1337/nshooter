using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class gc_PlayerDialoguesManager : MonoBehaviour
    {

        public enum DialogueType
        {
            Pickup,
            Kills
        }

        public bool enableDialogues = true;

        [Range(0, 1)]
        public float pickupDialogueFrequency = 0.85f;
        [Range(0, 1)]
        public float killsDialogueFrequency = 0.85f;
        [Range(0, 1)]
        public float DialoguePitch = 0.90f;
        [Range(0, 1)]
        public float DialogueVolume = 0.45f;

        public List<AudioClip> PickupDialoguesList = new List<AudioClip>();
        public List<AudioClip> KillsDialoguesList = new List<AudioClip>();

        public static gc_PlayerDialoguesManager Instance;
        AudioSource DialogueSource;

        // Use this for initialization
        void Awake()
        {
            Initialize();

            Instance = this;

        }
        void Initialize()
        {
            //add source to player object
            DialogueSource = FindObjectOfType<ManoeuvreFPSController>().gameObject.AddComponent<AudioSource>();

            //modify pitch to add depth in my voice :P
            DialogueSource.pitch = DialoguePitch;

            //init volume as well
            DialogueSource.volume = DialogueVolume;

        }


        public void PlayDialogueClip(DialogueType _dialogueType)
        {
            //exit if enable dialogues is disabled
            if (!enableDialogues)
                return;

            switch (_dialogueType)
            {
                case DialogueType.Kills:
                    PlayKillsDialogue();
                    break;

                case DialogueType.Pickup:
                    PlayPickupsDialogue();
                    break;
            }

        }


        void PlayKillsDialogue()
        {
            if (Random.value > killsDialogueFrequency)
                return;

            //taking random clip index
            int randomClipCount = Random.Range(0, KillsDialoguesList.Count);

            //caching that clip
            AudioClip clipToPlay = KillsDialoguesList[randomClipCount];

            //playing it and making sure its volume is 1
            if(!DialogueSource.isPlaying)
                DialogueSource.PlayOneShot(clipToPlay, 1);
        }

        void PlayPickupsDialogue()
        {
            if (Random.value > pickupDialogueFrequency)
                return;
            
            //taking random clip index
            int randomClipCount = Random.Range(0, PickupDialoguesList.Count);

            //caching that clip
            AudioClip clipToPlay = PickupDialoguesList[randomClipCount];

            //playing it and making sure its volume is 1
            if(!DialogueSource.isPlaying)
                DialogueSource.PlayOneShot(clipToPlay, 1);
        }

    }
}