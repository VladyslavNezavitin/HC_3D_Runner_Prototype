using UnityEngine;
using Zenject;

public class ItemSpawnerInstaller : MonoInstaller
{
    [SerializeField] private ItemSpawner spawnerInstance;

    public override void InstallBindings()
    {
        Container.Bind<ItemSpawner>().FromInstance(spawnerInstance)
            .AsSingle().NonLazy();
    }
}