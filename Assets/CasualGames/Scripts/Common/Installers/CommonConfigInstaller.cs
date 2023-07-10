using System;
using CasualGames.Common.Input;
using UnityEngine;
using Zenject;

namespace CasualGames.Common.Installers
{
    [Serializable]
    [CreateAssetMenu(menuName = "Casual Game/Common Settings",fileName = "CommonSettings.asset" )]
    public class CommonConfigInstaller: ScriptableObjectInstaller
    {
        [SerializeField] private InputConfig _inputConfig;
        public override void InstallBindings()
        {
            Container.BindInstance(_inputConfig);
        }
    }
}