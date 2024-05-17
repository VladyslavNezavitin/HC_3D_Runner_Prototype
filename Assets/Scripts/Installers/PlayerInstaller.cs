using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField] private Player playerInstance;
    [SerializeField] private PlayerController controllerInstance;

    public override void InstallBindings()
    {
        Container.Bind<Player>().FromInstance(playerInstance)
            .AsSingle().NonLazy();

        Container.Bind<PlayerController>().FromInstance(controllerInstance)
            .AsSingle().NonLazy();
    }
}