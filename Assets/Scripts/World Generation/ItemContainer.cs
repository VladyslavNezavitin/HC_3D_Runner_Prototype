using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Zenject;

public enum ItemContainerCurveType
{
    Straight,
    Arc,
    Adaptive
}

public class ItemContainer
{   
    public struct Properties
    {
        public Vector3 position;
        public int itemID;
        public ItemContainerCurveType curveType;
    }
    private struct Slot
    {
        public Vector3 worldPosition;
        public Item containedItem;
    }
    
    private const float ItemYOffset = 0.2f;
    private const float itemBoundingBoxSize = 0.65f;
    public float Length { get; private set; }
    public float ArcHeight { get; private set; }
    
    private PlayerController playerController;
    private Properties properties;
    private int occupiedSlotCount = 0;

    public int ItemID => properties.itemID;
    public bool IsFull => occupiedSlotCount == itemSlots.Length;
    public bool IsEmpty => occupiedSlotCount == 0;
    public bool isDisposed;

    private int itemCount;
    private Slot[] itemSlots;

    public ItemContainer(Properties config, PlayerController playerController)
    {
        this.playerController = playerController;
        this.properties = config;

        Length = playerController.JumpDistance;
        itemCount = Mathf.FloorToInt(Length / itemBoundingBoxSize);
        itemSlots = new Slot[itemCount];

        playerController.OnJumpTrajectoryChanged += Recalculate;
        Recalculate();    
    }

    public bool TryPlaceItem(Item itemInstance)
    {
        if (IsFull) return false;
        if (isDisposed) return false;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].containedItem == null)
            {
                itemSlots[i].containedItem = itemInstance;
                itemInstance.transform.position = itemSlots[i].worldPosition;
                itemInstance.RefreshVisual();

                occupiedSlotCount++;
                return true;
            }
        }

        return false;
    }

    public bool TryRemoveItem(Item itemInstance)
    {
        if (IsEmpty) Dispose();
        if (isDisposed) return false;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].containedItem == itemInstance)
            {
                itemSlots[i].containedItem = null;
                occupiedSlotCount--;
                return true;
            }
        }

        return false;
    }

    private void Recalculate()
    {   
        Length = playerController.JumpDistance;
        ArcHeight = playerController.JumpHeight - ItemYOffset;

        float containerStartZ = properties.position.z - Length / 2f;
        float distanceBetweenItemBoxes = (Length - itemCount *
            itemBoundingBoxSize) / (itemCount - 1);

        Vector3[] newSlotPositions = new Vector3[itemCount];

        for (int i = 0, n = 1; i < itemCount; i++, n += 2)
        {
            float itemSlotZ = containerStartZ + (itemBoundingBoxSize / 2f) * n
                + distanceBetweenItemBoxes * i;

            float itemSlotY = properties.curveType switch
            {
                ItemContainerCurveType.Arc => CalculateArcY(itemSlotZ),
                ItemContainerCurveType.Adaptive => CalculateAdaptiveY(itemSlotZ),
                ItemContainerCurveType.Straight => properties.position.y,
                _ => properties.position.y
            };
            
            newSlotPositions[i] = new Vector3(properties.position.x, itemSlotY + ItemYOffset, itemSlotZ);            
        }

        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].worldPosition = newSlotPositions[i];

            if (itemSlots[i].containedItem != null)
            {
                itemSlots[i].containedItem.transform.position = newSlotPositions[i];
                itemSlots[i].containedItem.RefreshVisual();
            }
        }
    }
 
    private float CalculateAdaptiveY(float itemPositionZ)
    {
        Vector3 rayOrigin = new Vector3(properties.position.x, 25f, itemPositionZ);
            if (Physics.Raycast(rayOrigin, Vector3.down, out var hitInfo) == false)
            {
                Debug.LogError(Messages.RAYCAST_FAILED);
                Debug.DrawRay(rayOrigin, Vector3.down * 100, Color.red, 100f);
            }

        return hitInfo.point.y;
    }

    private float CalculateArcY(float itemPositionZ)
    {
        float distanceFromCenter = Mathf.Abs(properties.position.z - itemPositionZ);
        float relativeHeight = 1f - Mathf.Pow(distanceFromCenter / (Length / 2f), 2);
        return Mathf.Lerp(properties.position.y, properties.position.y + ArcHeight, relativeHeight);
    }

    public void Dispose()
    {
        if (isDisposed) 
            return;

        playerController.OnJumpTrajectoryChanged -= Recalculate;
        isDisposed = true;
    }
}