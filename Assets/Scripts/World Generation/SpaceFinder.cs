using System.Collections.Generic;
using UnityEngine;

public enum SpawnSpaceType
{
    Ground,
    ObstacleTop,
    PassableWithJump,
    PassableWithSlide
}
public struct SpawnSpace
{
    public SpawnSpaceType type;
    public Vector3 start;
    public Vector3 end;
}

public class SpaceFinder
{
    private struct DoubleRaycastResults
    {
        public RaycastHitObject nextObject;
        public bool nextObjectExists;
    }
    private struct RaycastHitObject
    {
        public Bounds bounds;
        public float objectRowX;
        public float distanceToPrevious;
        public bool isPassableByJump;
        public bool isPassableBySlide;
    }

    private List<RaycastHitObject> cachedRaycastHitObjects = new List<RaycastHitObject>();

    private PlayerController playerController;
    private float passableSpaceLength;  // Space formed around passable obstacles
    private float primaryRayOffset;     // Height offset of the first ray from the ground
    private float secondaryRayOffset;   // Height offset of the second ray from the first ray
    private float maxRayDistance;
    private LayerMask obstacleMask;

    public SpaceFinder(PlayerController playerController) 
    {        
        this.playerController = playerController;
        
        passableSpaceLength = playerController.JumpDistance;
        primaryRayOffset = playerController.CurrentRadius;
        secondaryRayOffset = primaryRayOffset * 2f;

        playerController.OnJumpTrajectoryChanged += Player_OnJumpTrajectoryChanged;
    }

    private void Player_OnJumpTrajectoryChanged() => passableSpaceLength = playerController.JumpDistance;

    public List<SpawnSpace> FindSpawnSpaces(WorldTile tileInstance)
    {
        if (cachedRaycastHitObjects.Count == 0) 
            CacheTileBoundObjects(tileInstance);

        List<RaycastHitObject> newCachedRaycastHitObjects = new List<RaycastHitObject>();
        List<SpawnSpace> spawnSpaces = new List<SpawnSpace>();

        foreach (var cachedObject in cachedRaycastHitObjects)
        {
            RaycastHitObject currentObject = cachedObject;
            DoubleRaycastResults hitResults = PerformDoubleRaycastFromObject(currentObject);

            while (hitResults.nextObjectExists)
            {
                float currentToNextDistance;

                if (currentObject.isPassableByJump | currentObject.isPassableBySlide)
                {
                    float objectHalf = currentObject.bounds.extents.z;
                    float passableHalf = passableSpaceLength / 2f;

                    float centerToPreviousDistance = currentObject.distanceToPrevious + objectHalf;
                    float centerToNextDistance = hitResults.nextObject.bounds.min.z - currentObject.bounds.center.z;

                    if (centerToPreviousDistance >= passableHalf && centerToNextDistance >= passableHalf)
                    {
                        spawnSpaces.Add(GetPassableSpace(currentObject));
                        spawnSpaces.Add(GetPreviousToPassableSpace(currentObject));

                        // distance to the passable space end of the current object
                        currentToNextDistance = hitResults.nextObject.bounds.min.z - 
                            (currentObject.bounds.center.z + passableHalf);
                    }
                    else
                    {
                        spawnSpaces.Add(GetPreviousToCurrentSpace(currentObject));
                        spawnSpaces.Add(GetObjectTopSpace(currentObject));

                        // distance to the end of the current object
                        currentToNextDistance = hitResults.nextObject.bounds.min.z -
                            (currentObject.bounds.center.z + objectHalf);
                    }
                }
                else
                {
                    spawnSpaces.Add(GetPreviousToCurrentSpace(currentObject));
                    spawnSpaces.Add(GetObjectTopSpace(currentObject));

                    // distance to the end of the current object
                    currentToNextDistance = hitResults.nextObject.bounds.min.z -
                        (currentObject.bounds.center.z + currentObject.bounds.extents.z);
                }

                currentObject = hitResults.nextObject;
                currentObject.distanceToPrevious = currentToNextDistance;

                hitResults = PerformDoubleRaycastFromObject(currentObject);
            }

            // if current row doesn't have any more obstacles
            newCachedRaycastHitObjects.Add(currentObject);
        }

        cachedRaycastHitObjects = newCachedRaycastHitObjects;

        return spawnSpaces;
    }

    private SpawnSpace GetObjectTopSpace(RaycastHitObject currentObject)
    {
        return new SpawnSpace
        {
            start = new Vector3(currentObject.objectRowX,
                currentObject.bounds.size.y, currentObject.bounds.min.z),

            end = new Vector3(currentObject.objectRowX,
                currentObject.bounds.size.y, currentObject.bounds.max.z),

            type = SpawnSpaceType.ObstacleTop
        };
    }

    private SpawnSpace GetPreviousToCurrentSpace(RaycastHitObject currentObject)
    {
        return new SpawnSpace
        {
            start = new Vector3(currentObject.objectRowX, 0f, 
                currentObject.bounds.min.z - currentObject.distanceToPrevious),

            end = new Vector3(currentObject.objectRowX, 0f, currentObject.bounds.min.z),
            type = SpawnSpaceType.Ground
        };
    }

    private SpawnSpace GetPreviousToPassableSpace(RaycastHitObject currentObject)
    {
        return new SpawnSpace 
        {
            start = new Vector3(currentObject.objectRowX, 0f, 
                currentObject.bounds.min.z - currentObject.distanceToPrevious),

            end = new Vector3(currentObject.objectRowX, 0f, 
                currentObject.bounds.center.z - passableSpaceLength / 2f),

            type = SpawnSpaceType.Ground
        };
    }

    private SpawnSpace GetPassableSpace(RaycastHitObject currentObject)
    {
        return new SpawnSpace
        {
            start = new Vector3(currentObject.objectRowX, 0f, 
                currentObject.bounds.center.z - passableSpaceLength / 2f),

            end = new Vector3(currentObject.objectRowX, 0f, 
                currentObject.bounds.center.z + passableSpaceLength / 2f),

            type = currentObject.isPassableBySlide ? SpawnSpaceType.PassableWithSlide : SpawnSpaceType.PassableWithJump
        };
    }

    private DoubleRaycastResults PerformDoubleRaycastFromObject(RaycastHitObject currentObject)
    {
        Vector3 primaryRayOrigin = new Vector3(currentObject.objectRowX,
            primaryRayOffset, currentObject.bounds.max.z);

        Ray primaryRay = new Ray(primaryRayOrigin, Vector3.forward);
        Vector3 secondaryRayOrigin = primaryRayOrigin + Vector3.up * secondaryRayOffset;
        Ray secondaryRay = new Ray(secondaryRayOrigin, Vector3.forward);

        bool primaryHasHit = Physics.Raycast(primaryRay, out var primaryHit);
        bool secondaryHasHit = Physics.Raycast(secondaryRay,  out var secondaryHit);

        // if hit different objects
        if (primaryHasHit && secondaryHasHit &&
            primaryHit.transform.root.Equals(secondaryHit.transform.root) == false)
        {
            if (primaryHit.point.z < secondaryHit.point.z)
                secondaryHasHit = false;
            else
                primaryHasHit = false; 
        }

        RaycastHit hitInfo = primaryHasHit ? primaryHit : secondaryHit;

        RaycastHitObject hitObject = primaryHasHit | secondaryHasHit ? 
            new RaycastHitObject
            {
                bounds = hitInfo.collider.bounds,
                objectRowX = currentObject.objectRowX,
                distanceToPrevious = hitInfo.collider.bounds.min.z - primaryRayOrigin.z,
                isPassableByJump = secondaryHasHit == false,
                isPassableBySlide = primaryHasHit == false
            } : default(RaycastHitObject);

        return new DoubleRaycastResults
        {
            nextObject = hitObject,
            nextObjectExists = primaryHasHit | secondaryHasHit
        };
    }

    private void CacheTileBoundObjects(WorldTile tileInstance)
    {
        for (int rowIndex = 0; rowIndex < GameManager.Instance.RowCount; rowIndex++)
        {
            Vector3 tileLowerBound = new Vector3
            {
                x = Utils.RowToXPosition(rowIndex),
                y = primaryRayOffset,
                z = tileInstance.transform.position.z - WorldTile.TileLength / 2f
            };

            cachedRaycastHitObjects.Add(new RaycastHitObject
            {
                bounds = new Bounds(tileLowerBound, Vector3.zero),
                objectRowX = Utils.RowToXPosition(rowIndex),
                distanceToPrevious = 0f,
                isPassableByJump = false,
                isPassableBySlide = false,
            });
        }
    }
}