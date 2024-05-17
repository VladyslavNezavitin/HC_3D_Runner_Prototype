using UnityEngine;

[CreateAssetMenu(fileName = "RoadSegment", menuName = "ScriptableObjects/RoadSegment")]
public class WorldSegment : ScriptableObject 
{
    public WorldTile[] roadTiles;

    private void OnValidate() 
    {
        if (roadTiles.Length == 0) 
            throw new System.InvalidOperationException(Messages.ARRAY_NULL_OR_EMPTY);
    }
}