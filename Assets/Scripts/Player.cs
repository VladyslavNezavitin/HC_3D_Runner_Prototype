using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    public event Action<Item> OnItemPicked;
    public event Action OnLethalCollision;
    public event Action<int> OnNonLethalCollision;

    private bool detectCollisions = true;

    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            if (detectCollisions == false) return;

            float limAngle = 45;
            float cosYN = Vector3.Dot(transform.up, hit.normal);
            float cosZN = Vector3.Dot(transform.forward, hit.normal);
            float limAngleCos = Mathf.Cos(limAngle * Mathf.Deg2Rad);

            // if no collision (walkable surface / ceiling)
            if (cosYN > limAngleCos || cosYN < -limAngleCos) return;

            // if lethal collision (front)
            if (cosZN > limAngleCos || cosZN < -limAngleCos)
            {
                Obstacle obstacle = hit.gameObject.GetComponent<Obstacle>();
                obstacle.Hit();
                OnLethalCollision?.Invoke();
            }
            else
            {
                int direction = hit.normal.x > 0 ? -1 : 1;
                OnNonLethalCollision?.Invoke(direction);
                
                StartCoroutine(CollisionImmunityCoroutine(.25f));
            }
        } 
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.TryGetComponent<Item>(out var pickedItem))
        {
            pickedItem.Pick();
            OnItemPicked(pickedItem);
        }

        if (other.gameObject.CompareTag("Obstacle"))
            OnLethalCollision?.Invoke();
    }

    private IEnumerator CollisionImmunityCoroutine(float seconds)
    {
        detectCollisions = false;

        yield return new WaitForSeconds(seconds);

        detectCollisions = true;
    }
}
