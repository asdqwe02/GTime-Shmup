using System;
using UnityEngine;
using UnityEngine.Events;

namespace CasualGames.Operius2D
{
    public class ParticleSystemCallBack : MonoBehaviour
    {
        public UnityEvent OnParticleStopEvent;
        public UnityEvent OnParticleTriggerEvent;
        public UnityEvent OnParticleCollisionEvent;

        private void OnParticleSystemStopped()
        {
            OnParticleStopEvent?.Invoke();
        }

        private void OnParticleTrigger()
        {
            OnParticleTriggerEvent?.Invoke();
        }

        private void OnParticleCollision(GameObject other)
        {
            OnParticleCollisionEvent?.Invoke();
        }
    }
}