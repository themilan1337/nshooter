using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uzAI
{

    public class AudioTargetEnabler : MonoBehaviour
    {

        public List<AudioTarget> _targets = new List<AudioTarget>();
        public List<SimpleRotation> Indicators = new List<SimpleRotation>();

        int i = 0;

        private void Awake()
        {
            DisableAllIndicators(); 
        }

        void DisableAllIndicators()
        {
            foreach (SimpleRotation s in Indicators)
            {
                s.enabled = false;
               
            }
        }

        public void EnableTarget()
        {
            if (i < _targets.Count-1)
                i++;
            else
                i = 0;

            if (_targets[i] != null)
            {
                _targets[i].EnableAudioTarget();
                DisableAllIndicators();
                Indicators[i].enabled = true;
                
            }
            
        }

        
    }
}