using UnityEngine;
using Zenject;

public class ItemFactoryInstaller : MonoInstaller
{
    [SerializeField] ItemFactory.SpawnableItem[] spawnableItems;

    public override void InstallBindings()
    {
        Container.Bind<ItemFactory>().FromNew()
            .AsSingle().WithArguments(spawnableItems).NonLazy();  
    }
}