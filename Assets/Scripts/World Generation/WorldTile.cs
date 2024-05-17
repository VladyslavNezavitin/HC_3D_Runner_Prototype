using UnityEngine;

public class WorldTile : MonoBehaviour
{
    public const float TileLength = 20f;
    
    [SerializeField] private bool disableObstacleSpawn; 
    [SerializeField] private bool disableItemSpawn;
    [SerializeField] private bool disableBoosterSpawn;

    public bool DisableObstacleSpawn => disableObstacleSpawn;
    public bool DisableItemSpawn => disableItemSpawn;
    public bool DisableBoosterSpawn => disableBoosterSpawn;
}