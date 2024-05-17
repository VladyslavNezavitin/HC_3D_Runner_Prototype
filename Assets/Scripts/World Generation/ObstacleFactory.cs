using System.Collections.Generic;
using UnityEngine;

public class ObstacleFactory : IFactory<Obstacle>
{
    private Obstacle[] spawnableObstacles;
    private ObstacleBlock[] spawnableObstacleBlocks;

    public ObstacleFactory(Obstacle[] spawnableObstacles, ObstacleBlock[] spawnableObstacleBlocks)
    {
        if (spawnableObstacles == null || spawnableObstacles.Length == 0 ||
            spawnableObstacleBlocks == null || spawnableObstacleBlocks.Length == 0)
            throw new System.ArgumentException(Messages.ARRAY_NULL_OR_EMPTY);

        this.spawnableObstacles = spawnableObstacles;
        this.spawnableObstacleBlocks = spawnableObstacleBlocks;
    }

    public int[] GetSpawnableIDs()
    {
        int[] idList = new int[spawnableObstacles.Length];

        for (int i = 0; i < idList.Length; i++)
            idList[i] = spawnableObstacles[i].Id;

        return idList;
    }

    public ObstacleBlock GetRandomObstacleBlock()
    {
        int index = Random.Range(0, spawnableObstacleBlocks.Length);
        return spawnableObstacleBlocks[index];
    }

    public Obstacle Get(int id)
    {
        for (int i = 0; i < spawnableObstacles.Length; i++)
        {
            if (spawnableObstacles[i].Id == id)
                return Object.Instantiate(spawnableObstacles[i]);
        }

        throw new System.InvalidOperationException(Messages.NOT_SPAWNABLE);
    } 
}