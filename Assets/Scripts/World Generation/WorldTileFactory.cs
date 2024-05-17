using System.Collections.Generic;
using UnityEngine;

public class WorldTileFactory
{
    private WorldSegment startSegment;
    private WorldSegment[] worldSegments;
    private bool isFirstRequest;

    private Queue<WorldTile> tileQueue = new Queue<WorldTile>();

    public WorldTileFactory(WorldSegment startSegment, WorldSegment[] worldSegments)
    {
        if (worldSegments == null || worldSegments.Length == 0)
            throw new System.ArgumentException(Messages.ARRAY_NULL_OR_EMPTY);

        this.startSegment = startSegment;
        this.worldSegments = worldSegments;
        isFirstRequest = true;
    }

    public WorldTile GetTile(Vector3 tilePosition)
    {
        if (tileQueue.TryDequeue(out var tile) == false)
        {
            CacheNewSegment();
            tile = tileQueue.Dequeue();
        }

        return Object.Instantiate(tile, tilePosition, Quaternion.Euler(-90, 0, 0));
    }

    private void CacheNewSegment()
    {
        WorldTile[] tilesToCache;
        
        if (isFirstRequest && startSegment != null)
        {
            tilesToCache = startSegment.roadTiles;
            isFirstRequest = false;
        }
        else
            tilesToCache = GetTilesFromRandomSegment();

        foreach (var tile in tilesToCache)
            tileQueue.Enqueue(tile);
    }

    private WorldTile[] GetTilesFromRandomSegment()
    {
        int index = Random.Range(0, worldSegments.Length);
        WorldSegment segment = worldSegments[index];

        return segment.roadTiles;
    }
}