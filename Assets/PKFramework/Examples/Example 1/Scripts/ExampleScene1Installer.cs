using Zenject;

namespace PKFramework.Examples
{
    public class ExampleScene1Installer: MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.Bind<ExampleScene1Logic>().AsSingle();
        }
    }
   
}