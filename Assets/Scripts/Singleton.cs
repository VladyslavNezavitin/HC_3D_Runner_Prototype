using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object Lock = new object();

    public static T Instance
    {
        get
        {
            lock(Lock)
            {
                if (instance != null)
                    return instance;

                var instances = FindObjectsOfType<T>();
                
                if (instances.Length > 0)
                {
                    if (instances.Length == 1)
                        return instance = instances[0];

                    Debug.LogWarning($"[{nameof(Singleton<T>)}<{nameof(T)}>] There should never be more than one instance.");

                    for (int i = 1; i < instances.Length; i++)
                        Destroy(instances[i]);

                    return instance = instances[0];
                }

                Debug.LogWarning($"[{nameof(Singleton<T>)}<{nameof(T)}>] An instance is not exist and will be created.");
                
                GameObject newGameObject = new GameObject();
                newGameObject.hideFlags = HideFlags.HideInHierarchy;
                return instance = newGameObject.AddComponent<T>();
            }
        }
    }

    private void OnDestroy() 
    {
        if (instance == this)
            Destroy(instance);    
    }
        
}