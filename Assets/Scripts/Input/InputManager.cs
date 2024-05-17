using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public event Action OnSwipeUp;
    public event Action OnSwipeDown;
    public event Action OnSwipeLeft;
    public event Action OnSwipeRight;

    private PlayerControls playerControls;

    private float swipeDeltaThreshold = Screen.width / 10;
    private Vector2 swipeStartPosition;
    private bool isSwiping;    

    private void OnEnable() => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();
    private void Update() => HandleInput();
    
    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void Start() 
    {   
        playerControls.Touch.PrimaryContact.started += context => StartPrimaryTouch(context);
        playerControls.Touch.PrimaryContact.canceled += context => StopPrimaryTouch(context);
              
        isSwiping = false;
    }

    // For some reason PrimaryPosition of the very first touch is returning zero so it must wait
    // until the end of the current frame to be updated proreply (input system bug?)
    private bool isFirstTouch = true;
    private void StartPrimaryTouch(InputAction.CallbackContext context)
    {
        if (isFirstTouch)
        {
            StartCoroutine(FirstTouchWait());
            isFirstTouch = false;
        }
        else
        {
            swipeStartPosition = playerControls.Touch.PrimaryPosition.ReadValue<Vector2>();
            isSwiping = true;
        }

        IEnumerator FirstTouchWait() 
        {
            yield return new WaitForEndOfFrame(); 
            StartPrimaryTouch(context);
        }
    }

    private void StopPrimaryTouch(InputAction.CallbackContext context)
    {
        isSwiping = false;
    }

    private void HandleInput()
    {
        if (isSwiping)
        {
            Vector2 swipeCurrentPosition = playerControls.Touch.PrimaryPosition.ReadValue<Vector2>();
            Vector2 swipeDelta = swipeCurrentPosition - swipeStartPosition;

            // If a horizontal swipe was performed (and x delta is bigger than y delta)
            if (Mathf.Abs(swipeDelta.x) >= swipeDeltaThreshold && 
                Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                // If swiped right
                if (swipeDelta.x > 0)
                    OnSwipeRight?.Invoke();
                else
                    OnSwipeLeft?.Invoke();

                isSwiping = false;
            }
            // If a vertical swipe was performed
            else if (Mathf.Abs(swipeDelta.y) >= swipeDeltaThreshold)
            {
                // If swiped up
                if (swipeDelta.y > 0)
                    OnSwipeUp?.Invoke();
                else
                    OnSwipeDown?.Invoke();

                isSwiping = false;
            }
        }
    }   
}