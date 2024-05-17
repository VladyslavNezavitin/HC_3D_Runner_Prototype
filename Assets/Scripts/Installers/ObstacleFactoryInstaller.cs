using UnityEngine;
using Zenject;

public class ObstacleFactoryInstaller : MonoInstaller
{
    [SerializeField] private Obstacle[] spawnableObstacles;
    [SerializeField] private ObstacleBlock[] spawnableObstacleBlocks;

    public override void InstallBindings()
    {
        Container.Bind<ObstacleFactory>().FromNew()
            .AsSingle().WithArguments(spawnableObstacles, spawnableObstacleBlocks).NonLazy();  
    }
}