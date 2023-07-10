using CasualGames.Operius2D.Signals;
using CasualGames.Scenes.Operius2DScene;
using DG.Tweening;
using Kino;
using UnityEngine;
using Zenject;

namespace CasualGames.Operius2D
{
    public class CameraEffect : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private Operius2DSceneLogic _logic;
        private AnalogGlitch _analogGlitch;
        private Vector3 _originalPos;

        private void Awake()
        {
            // _signalBus.Subscribe<PlayerHitSignal>(OnPlayerHit)
            _signalBus.Subscribe<CameraEffectSignal>(OnCameraEffectSignal);
            _signalBus.Subscribe<GameResetSignal>(OnGameResetSignal);
            _signalBus.Subscribe<StartGameSignal>(OnGameStartSignal);
            _signalBus.Subscribe<GameEndSignal>(OnGameEndSignal);
            _analogGlitch = GetComponent<AnalogGlitch>();
            _originalPos = transform.position;
            SimpleGlitch();
        }

        private void OnGameStartSignal(StartGameSignal obj)
        {
            ResetGlitchEffect();
        }

        public void ResetGlitchEffect()
        {
            _analogGlitch.scanLineJitter = 0f;
            _analogGlitch.colorDrift = 0f;
        }

        public void SimpleGlitch()
        {
            _analogGlitch.scanLineJitter = 0.075f;
            _analogGlitch.colorDrift = 0.1f;
        }

        private void OnGameEndSignal(GameEndSignal obj)
        {
            SimpleGlitch();
        }


        private void OnGameResetSignal(GameResetSignal obj)
        {
            ResetGlitchEffect();
        }

        private void OnCameraEffectSignal(CameraEffectSignal obj)
        {
            switch (obj.CameraEffectType)
            {
                case CameraEffectType.SHAKE:
                    ScreenShake();
                    break;
            }
        }

        private void ScreenShake()
        {
            transform
                .DOShakePosition(
                    strength: new Vector3(0.5f, .15f, 0f),
                    duration: 1f,
                    fadeOut: true
                )
                .OnComplete(() => { transform.position = _originalPos; })
                .SetUpdate(true);
        }

        private void OnPlayerHit(PlayerHitSignal obj)
        {
            ScreenShake();
        }

        private void OnDestroy()
        {
            // _signalBus.Unsubscribe<PlayerHitSignal>(OnPlayerHit);
            _signalBus.Unsubscribe<CameraEffectSignal>(OnCameraEffectSignal);
        }
    }
}