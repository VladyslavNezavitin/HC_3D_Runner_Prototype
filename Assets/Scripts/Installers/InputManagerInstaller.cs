using UnityEngine;
using Zenject;

public class InputManagerInstaller : MonoInstaller
{
    [SerializeField] InputManager managerInstance;

    public override void InstallBindings()
    {
        Container.Bind<InputManager>().FromInstance(managerInstance)
            .AsSingle().NonLazy();
    }
}
