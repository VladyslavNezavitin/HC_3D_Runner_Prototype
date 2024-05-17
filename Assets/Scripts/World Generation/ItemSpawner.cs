using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ItemSpawner : MonoBehaviour
{
    private struct ContainerData
    {
        public ItemContainer container;
        public List<Item> boundList;
    }

    [SerializeField] private int poolPerItemCapacity;
    [Header("Space spawn weights:")]
    [SerializeField] private float groundWeight;
    [SerializeField] private float obstacleTopWeight;
    [SerializeField] private float passableWeight;

    [Inject] private Player player;
    [Inject] private PlayerController playerController;
    [Inject] private ItemFactory itemFactory;
    private float containerLength;

    private Queue<ContainerData> containerFillingQueue;
    private Dictionary<Item, ContainerData> itemBoundContainers;
    private Dictionary<int, ObjectPool<Item>> poolMap;

    private void Start() 
    {
        poolMap = new Dictionary<int, ObjectPool<Item>>();
        itemBoundContainers = new Dictionary<Item, ContainerData>();
        containerFillingQueue = new Queue<ContainerData>(); 
        containerLength = playerController.JumpDistance;
        InitializePools();
    }

    private void Update()
    {
        if (containerFillingQueue.Count > 0)
        {
            ContainerData data = containerFillingQueue.Peek();

            if (data.container.IsFull)
            {
                containerFillingQueue.Dequeue();
                return;   
            }

            if (poolMap[data.container.ItemID].TryGet(out var item))
            {
                if (data.container.TryPlaceItem(item))
                {
                    data.boundList.Add(item);
                    itemBoundContainers.Add(item, data);
                }
            }
        }
    }

    private void InitializePools()
    {
        foreach (var itemID in itemFactory.GetSpawnableIDs())
        {
            ObjectPool<Item> pool = new ObjectPool<Item>
                (itemID, poolPerItemCapacity, itemFactory);
                
            pool.AutoExpanding = true;
            poolMap.Add(itemID, pool);
        }
    }

    public List<Item> SpawnItemsForCurrentSpaces(List<SpawnSpace> spawnSpaces)
    {
        var containerProperties = GetContainerProperties(spawnSpaces);
        List<Item> instanceList = new List<Item>();

        foreach (var properties in containerProperties)
        {
            var container = new ItemContainer(properties, playerController);

            containerFillingQueue.Enqueue(new ContainerData 
            {
                container = container,
                boundList = instanceList
            });
        }

        return instanceList;   
    }

    private List<ItemContainer.Properties> GetContainerProperties(List<SpawnSpace> spawnSpaces)
    {
        var containerProperties = new List<ItemContainer.Properties>();
        var possibleProperties = GetPossibleContainerProperties(spawnSpaces);

        float totalWeight = 0f;
        foreach (var properties in possibleProperties)
            totalWeight += properties.Value;

        foreach (var properties in possibleProperties)
        {
            float weightNormalized = properties.Value / totalWeight;
            if (UnityEngine.Random.Range(0f, 1f) < weightNormalized)
                containerProperties.Add(properties.Key);
        }

        return containerProperties;
    }

    private Dictionary<ItemContainer.Properties, float> GetPossibleContainerProperties(List<SpawnSpace> spawnSpaces)
    {
        var possibleContainerProperties = new Dictionary<ItemContainer.Properties, float>();

        foreach (var space in spawnSpaces)
        {
            float spaceLength = space.end.z - space.start.z;

            if (spaceLength < containerLength)
                continue;

            int containerMaxCount = Mathf.FloorToInt(spaceLength / containerLength);
            float spaceCenterZ = space.start.z + spaceLength / 2f;
            float totalContainerLength = containerMaxCount * containerLength;
            float startOffset = (spaceLength - totalContainerLength) / 2f;    // to centrallize container positions
            float containerHalf = containerLength / 2f;
            
            float primaryWeight = space.type switch
            {
                SpawnSpaceType.Ground => groundWeight,
                SpawnSpaceType.ObstacleTop => obstacleTopWeight,
                SpawnSpaceType.PassableWithJump => passableWeight,
                SpawnSpaceType.PassableWithSlide => passableWeight,
                _ => groundWeight
            };

            for (int i = 0, n = 1; i < containerMaxCount; i++, n += 2)
            {
                float containerPositionZ = space.start.z + startOffset + containerHalf * n;
                float distanceFromCenter = Mathf.Abs(spaceCenterZ - containerPositionZ);
                float secondaryWeight = 1f - Mathf.Pow(distanceFromCenter / spaceLength, 2);
                float weight = (primaryWeight + secondaryWeight) / 2f;

                ItemContainerCurveType containerCurveType = space.type switch
                {
                    SpawnSpaceType.Ground => ItemContainerCurveType.Straight,
                    SpawnSpaceType.ObstacleTop => ItemContainerCurveType.Adaptive,
                    SpawnSpaceType.PassableWithJump => ItemContainerCurveType.Arc, 
                    SpawnSpaceType.PassableWithSlide => ItemContainerCurveType.Straight,
                    _ => ItemContainerCurveType.Straight
                };

                int containerItemID = itemFactory.GetRandomItemID();
                Vector3 containerPosition = new Vector3(space.start.x, space.start.y, containerPositionZ);

                possibleContainerProperties.Add(new ItemContainer.Properties
                {
                    itemID = containerItemID,
                    curveType = containerCurveType,
                    position = containerPosition
                },  weight);
            }
        }

        return possibleContainerProperties;
    }

    private void OnEnable()
    {
        player.OnItemPicked += RecycleItem;
        playerController.OnJumpTrajectoryChanged += Player_OnJumpTrajectoryChanged;
    }
        
    private void OnDisable()
    {
        player.OnItemPicked -= RecycleItem;
        playerController.OnJumpTrajectoryChanged -= Player_OnJumpTrajectoryChanged;
    }

    private void Player_OnJumpTrajectoryChanged() => containerLength = playerController.JumpDistance;

    public void RecycleItems(List<Item> itemsToRecycle)
    {
        if (itemsToRecycle == null)
            return;

        foreach (var item in itemsToRecycle)
            RecycleItem(item);
    }
    
    public void RecycleItem(Item item) 
    {
        if (TryRemoveItemFromContainer(item))
        {
            poolMap[item.Id].Return(item);
            itemBoundContainers[item].boundList.Remove(item);
            itemBoundContainers.Remove(item);
        }
        else
            Debug.LogError("None of the containers handles this item!");
    }

    private bool TryRemoveItemFromContainer(Item item)
    {
        if (itemBoundContainers.ContainsKey(item))
        {
            ItemContainer container = itemBoundContainers[item].container;
            if (container.TryRemoveItem(item))
            {
                if (container.IsEmpty)
                    container.Dispose();

                return true;
            }
        }

        return false;
    }
}