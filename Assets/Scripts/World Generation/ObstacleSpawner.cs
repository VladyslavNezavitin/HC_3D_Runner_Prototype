using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ObstacleSpawner : MonoBehaviour 
{    
    private struct ObstacleToSpawn
    {
        public Obstacle prefab;
        public Vector3 worldPosition;
        public Quaternion worldRotation;
    }
    
    [SerializeField] private float distanceBetweenBlocks;
    [SerializeField] private int poolPerObstacleCapacity;
    [Inject] private ObstacleFactory obstacleFactory;

    private Dictionary<int, ObjectPool<Obstacle>> poolMap;
    private List<ObstacleToSpawn> cachedObstacles;
    private float nextBlockStartZ;

    private void Start() 
    {
        poolMap = new Dictionary<int, ObjectPool<Obstacle>>();
        cachedObstacles = new List<ObstacleToSpawn>();
        nextBlockStartZ = 0f;
        
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var obstacleID in obstacleFactory.GetSpawnableIDs())
        {
            ObjectPool<Obstacle> pool = new ObjectPool<Obstacle>
                (obstacleID, poolPerObstacleCapacity, obstacleFactory);

            pool.AutoExpanding = true;
            poolMap.Add(obstacleID, pool);
        }
    }

    public List<Obstacle> SpawnObstaclesForCurrentTile(WorldTile tile)
    {
        if (tile.DisableObstacleSpawn)
        {
            nextBlockStartZ += WorldTile.TileLength;
            return null;
        }

        List<Obstacle> obstacleInstances = new List<Obstacle>();
        List<ObstacleToSpawn> instantiatedObstacles = new List<ObstacleToSpawn>();
        
        float chunkLowerBound = tile.transform.position.z - WorldTile.TileLength / 2f;
        float chunkUpperBound = tile.transform.position.z + WorldTile.TileLength / 2f;

        if (cachedObstacles.Count == 0)
            CacheNewObstacleBlock();

        foreach (var cached in cachedObstacles)
        {
            if (cached.worldPosition.z <= chunkUpperBound)
            {
                if (poolMap.ContainsKey(cached.prefab.Id) == false)
                {
                    Debug.LogError("Pool for this obstacle is not initialized!");
                    break;
                }

                if (poolMap[cached.prefab.Id].TryGet(out var instance))
                {
                    instance.transform.position = cached.worldPosition;
                    instance.transform.rotation = cached.worldRotation;

                    obstacleInstances.Add(instance);
                    instantiatedObstacles.Add(cached);
                }   
            }
        }

        foreach (var obstacle in instantiatedObstacles)
            cachedObstacles.Remove(obstacle);

        return obstacleInstances;
    }

    private void CacheNewObstacleBlock()
    {
        ObstacleBlock block = obstacleFactory.GetRandomObstacleBlock();
        float blockPositionZ = nextBlockStartZ + block.Length / 2f;

        foreach (var obstacle in block.Obstacles)
        {
            Vector3 obstacleWorldPosition = new Vector3 
            {
                x = obstacle.transform.localPosition.x,
                y = obstacle.transform.localPosition.y,
                z = blockPositionZ + obstacle.transform.localPosition.z
            };

            cachedObstacles.Add(new ObstacleToSpawn
            {
                prefab = obstacle,
                worldPosition = obstacleWorldPosition,
                worldRotation = obstacle.transform.localRotation
            });
        }

        nextBlockStartZ += block.Length + distanceBetweenBlocks;
    }

    public void RecycleObstacles(List<Obstacle> obstacles)
    {
        foreach (var obstacle in obstacles)
            RecycleObstacle(obstacle);
    }

    public void RecycleObstacle(Obstacle obstacle)
    {
        if (poolMap.ContainsKey(obstacle.Id))
            poolMap[obstacle.Id].Return(obstacle);
        // WARNING! NO REMOVAL FROM BOUND LIST (LIKE IN ITEM SPAWNER)
    }
}