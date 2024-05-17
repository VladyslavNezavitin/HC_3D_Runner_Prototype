using UnityEngine;

public class ItemFactory : IFactory<Item>
{  
    [System.Serializable] public struct SpawnableItem
    {
        public Item itemPrefab;
        [Range(0, 1f)] public float spawnChance;
    }
    
    private SpawnableItem[] spawnableItems;

    public ItemFactory(SpawnableItem[] spawnableItems)
    {
        if (spawnableItems == null || spawnableItems.Length == 0)
            throw new System.ArgumentException(Messages.ARRAY_NULL_OR_EMPTY);

        this.spawnableItems = spawnableItems;
    }

    public int GetRandomItemID()
    {
        int[] idList = GetSpawnableIDs();
        return idList[Random.Range(0, idList.Length)];
    }

    public int[] GetSpawnableIDs()
    {
        int[] idList = new int[spawnableItems.Length];

        for (int i = 0; i < idList.Length; i++)
            idList[i] = spawnableItems[i].itemPrefab.Id;

        return idList;
    }

    public Item Get(int id)
    {
        foreach (var item in spawnableItems)
        {
            if (id == item.itemPrefab.Id)
                return Object.Instantiate(item.itemPrefab);
        }

        Debug.LogError(Messages.NOT_SPAWNABLE);
        return null;
    }
}