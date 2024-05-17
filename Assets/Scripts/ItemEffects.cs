using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class ItemEffects : MonoBehaviour
{
    [Header("Visual model:")]
    [SerializeField] private GameObject visual;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 orientation;
    [Header("Floating:")]
    [SerializeField] private bool isFloating;
    [SerializeField] private float floatHeight;
    [SerializeField] private float floatSpeed;
    [Header("Spinning:")]
    [SerializeField] private bool isSpinning;
    [SerializeField] private Vector3 degreesPerSecond;

    private Item item;
    private Vector3 initialPosition;
    private Vector3 initialRotation;

    private void Awake() => item = GetComponent<Item>();

    private void OnEnable() 
    {
        item.OnItemPicked += Item_OnItemPicked;
        item.OnVisualRefresh += Item_OnVisualRefresh;
    }

    private void OnDisable()
    {
        item.OnItemPicked -= Item_OnItemPicked;
        item.OnVisualRefresh -= Item_OnVisualRefresh;
    }

    private void Update() 
    {
        if (isFloating)
        {
            float delta = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

            visual.transform.position = new Vector3(initialPosition.x,
                initialPosition.y + delta, initialPosition.z);
        }

        if (isSpinning)
        {
            Vector3 delta = new Vector3 
            {
                x = degreesPerSecond.x * Time.deltaTime,
                y = degreesPerSecond.y * Time.deltaTime,
                z = degreesPerSecond.z * Time.deltaTime
            };

            visual.transform.Rotate(delta, Space.World);
        }
    }

    private void Item_OnItemPicked()
    {

    }

    private void Item_OnVisualRefresh()
    {
        visual.transform.position = transform.position + offset;
        visual.transform.rotation = Quaternion.Euler(orientation);
        initialPosition = visual.transform.position;
        initialRotation = visual.transform.rotation.eulerAngles;
    }

}
