using System;
using CasualGames.Common.Signals;
using PKFramework.SerialPort.Scripts;
using Serilog;
using UnityEngine;
using Zenject;
using ILogger = PKFramework.Logger.ILogger;

namespace CasualGames.Common.Input
{
    public class InputDispatcher : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        [Inject] private InputConfig _config;
        [Inject] private ILogger _logger;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(_config.InputKey))
            {
                _signalBus.Fire<KeyDownSignal>();
            }
            else if (UnityEngine.Input.GetKeyUp(_config.InputKey))
            {
                _signalBus.Fire<KeyUpSignal>();
            }

            else if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _signalBus.Fire<KeyDownSignal>();
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _signalBus.Fire<KeyUpSignal>();
            }

            else if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _signalBus.Fire<KeyDownSignal>();
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    _signalBus.Fire<KeyUpSignal>();
                }
            }
        }
    }
}