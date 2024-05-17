using UnityEngine;

public interface IFactory<T> where T : MonoBehaviour
{
    public T Get(int id);
}