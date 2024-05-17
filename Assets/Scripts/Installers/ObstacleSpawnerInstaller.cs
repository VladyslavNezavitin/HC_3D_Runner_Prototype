using UnityEngine;
using Zenject;

public class ObstacleSpawnerInstaller : MonoInstaller<ObstacleSpawnerInstaller>
{
    [SerializeField] ObstacleSpawner spawnerInstance;

    public override void InstallBindings()
    {
        Container.Bind<ObstacleSpawner>().FromInstance(spawnerInstance)
            .AsSingle().NonLazy();
    }
}