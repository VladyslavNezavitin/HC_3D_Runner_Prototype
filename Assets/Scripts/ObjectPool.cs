using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> pool;
    private IFactory<T> factory;
    private int objectID;
    public bool AutoExpanding;

    public ObjectPool(int objectID, int capacity, IFactory<T> factory)
    { 
        if (factory == null)
            throw new System.ArgumentException("Factory is Null!");

        this.objectID = objectID;
        this.factory = factory;
        this.pool = new Queue<T>();

        for (int i = 0; i < capacity; i++)
            CreateInstance();
    }

    private void CreateInstance(bool isActiveByDefault = false)
    {
        T instance = factory.Get(objectID);
        instance.gameObject.SetActive(isActiveByDefault);
        pool.Enqueue(instance);
    }

    public bool TryGet(out T instance)
    {
        if (pool.TryDequeue(out instance) == false)
        {
            if (AutoExpanding)
            {
                CreateInstance(true);
                instance = pool.Dequeue();
                return true;
            }
            else
                return false;
        }

        instance.gameObject.SetActive(true);
        return true;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}