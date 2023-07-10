using CasualGames.Common.Input;
using CasualGames.Common.Signals;
using UnityEngine;
using Zenject;

namespace CasualGames.Common.Installers
{
    public class CommonInstaller : MonoInstaller
    {
        [SerializeField]
        private InputDispatcher _inputDispatcher;

        public override void InstallBindings()
        {
            Container.Bind<InputDispatcher>().FromComponentInNewPrefab(_inputDispatcher).AsSingle().NonLazy();

            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<CoinInsertedSignal>();
            Container.DeclareSignal<CoinReturnedSignal>();

            Container.DeclareSignal<KeyDownSignal>().OptionalSubscriber();
            Container.DeclareSignal<KeyUpSignal>().OptionalSubscriber();
        }
    }
}