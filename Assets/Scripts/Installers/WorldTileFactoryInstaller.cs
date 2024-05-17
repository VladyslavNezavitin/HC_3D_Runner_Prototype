using UnityEngine;
using Zenject;

public class WorldTileFactoryInstaller : MonoInstaller
{
    [SerializeField] private WorldSegment startSegment;
    [SerializeField] private WorldSegment[] segments;

    public override void InstallBindings()
    {
        Container.Bind<WorldTileFactory>().FromNew()
            .AsSingle().WithArguments(startSegment, segments).NonLazy();
    }
}