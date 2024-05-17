using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WorldSpawner : MonoBehaviour
{
    [SerializeField] private float viewDistance;
    [SerializeField] private float chunkDeletionThreshold;

    [Inject] private ObstacleSpawner obstacleSpawner;
    [Inject] private ItemSpawner itemSpawner;
    [Inject] private WorldTileFactory worldTileFactory;
    [Inject] private PlayerController playerController;
    
    private SpaceFinder spaceFinder;
    private Queue<ChunkData> activeChunks;
    private float nextChunkPositionZ;

    private void Start() 
    {
        activeChunks = new Queue<ChunkData>();
        spaceFinder = new SpaceFinder(playerController);
        nextChunkPositionZ = 0f;
    }

    private void Update() 
    {

        if (playerController.transform.position.z + viewDistance > nextChunkPositionZ)
            SpawnNewChunk();

        if (playerController.transform.position.z - activeChunks.Peek().position.z > chunkDeletionThreshold)
            RemoveOldestChunk();
    }

    private void SpawnNewChunk()
    {
        Vector3 chunkPosition = new Vector3(0f, 0f, nextChunkPositionZ);
        WorldTile tile = worldTileFactory.GetTile(chunkPosition);

        List<Item> chunkItems = null;
        List<Obstacle> chunkObstacles = null;

        chunkObstacles = obstacleSpawner.SpawnObstaclesForCurrentTile(tile);
        List<SpawnSpace> spawnSpaces = spaceFinder.FindSpawnSpaces(tile);

        //DEBUG_VISUALIZE_SPACES(spawnSpaces);

        if (tile.DisableItemSpawn == false)
            chunkItems = itemSpawner.SpawnItemsForCurrentSpaces(spawnSpaces);

        ChunkData chunk = new ChunkData
        {
            position = chunkPosition,
            tile = tile,
            obstacles = chunkObstacles,
            items = chunkItems
        };

        activeChunks.Enqueue(chunk);
        nextChunkPositionZ += WorldTile.TileLength;
    }

    private void RemoveOldestChunk()
    {
        ChunkData oldestChunk = activeChunks.Dequeue();

        if (oldestChunk.items != null && oldestChunk.items.Count > 0)
        {
            List<Item> itemsToRecycle = new List<Item>(oldestChunk.items);
            itemSpawner.RecycleItems(itemsToRecycle);
        }

        if (oldestChunk.obstacles != null && oldestChunk.obstacles.Count > 0)
        {
            List<Obstacle> obstaclesToRecycle = new List<Obstacle>(oldestChunk.obstacles);
            obstacleSpawner.RecycleObstacles(obstaclesToRecycle);
        }

        Destroy(oldestChunk.tile.gameObject);
    }

    private void DEBUG_VISUALIZE_SPACES(List<SpawnSpace> spawnSpaces)
    {
        foreach (var space in spawnSpaces)
        {
            Color color = space.type switch
            {
                SpawnSpaceType.Ground => Color.green,
                SpawnSpaceType.ObstacleTop => Color.yellow,
                SpawnSpaceType.PassableWithJump => Color.magenta,
                SpawnSpaceType.PassableWithSlide => Color.magenta,
                _ => Color.cyan
            };

            Debug.DrawLine(space.start, space.end, color, 100f);
        }
    }
}

public class ChunkData
{
    public Vector3 position;
    public WorldTile tile;
    public List<Item> items;
    public List<Obstacle> obstacles;
}