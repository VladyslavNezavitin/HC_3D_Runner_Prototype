using System;
using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Player), typeof(CharacterController), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    public event Action OnSlide;
    public event Action OnJump;
    public event Action OnMoveLeft;
    public event Action OnMoveRight;
    public event Action OnJumpTrajectoryChanged;

    public const float Gravity = -9.81f;

    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float slidingTime = 1f;    
    [SerializeField] private float sideMovementTime = 0.2f;
    [SerializeField] private float gravityMultiplier = 1f; 
    [SerializeField] private LayerMask groundLayer;
 
    [Inject] private InputManager inputManager;
    private Player player;
    private CharacterController controller;
    private CapsuleCollider capsuleCollider;

    private Vector3 playerVelocity;
    private int currentRowIndex;

    private Vector3 colliderCenterInitial;
    private float colliderHeightInitial;
    private bool isMovingSideways;
    private bool isSliding;

    public float JumpDistance { get; private set; }
    public float RunSpeed => runSpeed;
    public float JumpHeight => jumpHeight;
    public float CurrentGravity => Gravity * gravityMultiplier;
    public float CurrentRadius => controller.radius;
    public float SlidingTime => slidingTime;

    private void Awake() 
    {   
        player = GetComponent<Player>();
        controller = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        currentRowIndex = 1;
        playerVelocity.z = runSpeed;
        colliderCenterInitial = controller.center;
        colliderHeightInitial = controller.height;
        RecalculateJumpDistance();
    }

    private void OnEnable() 
    {
        inputManager.OnSwipeLeft += MoveLeft;
        inputManager.OnSwipeRight += MoveRight;
        inputManager.OnSwipeUp += Jump;
        inputManager.OnSwipeDown += Slide;
        player.OnNonLethalCollision += Player_OnNonLethalCollision;
    }

    private void OnDisable() 
    {
        inputManager.OnSwipeLeft -= MoveLeft;
        inputManager.OnSwipeRight -= MoveRight;
        inputManager.OnSwipeUp -= Jump;
        inputManager.OnSwipeDown -= Slide;
        player.OnNonLethalCollision -= Player_OnNonLethalCollision;
    }

    private void Update() 
    {
        ApplyGravity();
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void Player_OnNonLethalCollision(int direction)
    {
        isMovingSideways = false;
        StopCoroutine("SidewaysMovementRoutine");

        switch (direction)
        {
            case -1: MoveRight(); break;
            case 1: MoveLeft(); break;
            default: break;
        }
    }

    private void Jump()
    {
        if (IsOnGround())
        {
            CancelSlide();
            OnJump?.Invoke();
            playerVelocity.y = Mathf.Sqrt(2 * -Gravity * gravityMultiplier * jumpHeight);
        }
    }

    private void MoveLeft()
    {
        if(currentRowIndex - 1 >= 0 && isMovingSideways == false)
        {
            float toPositionX = Utils.RowToXPosition(currentRowIndex - 1);
            OnMoveLeft?.Invoke();
            CancelSlide();
            StartCoroutine("SidewaysMovementRoutine", toPositionX);
            currentRowIndex--;
        }
    }

    private void MoveRight()
    {
        if(currentRowIndex + 1 <= 2 && isMovingSideways == false)
        {
            float toPositionX = Utils.RowToXPosition(currentRowIndex + 1);
            OnMoveRight?.Invoke();
            CancelSlide();
            StartCoroutine("SidewaysMovementRoutine", toPositionX);
            currentRowIndex++;
        }
    }

    private void Slide() 
    {
        if (IsOnGround())
        {
            StartCoroutine("SlideMainPhaseRoutine");
        }
        else
            StartCoroutine("SlideLandingPhaseCoroutine");
    }

    private IEnumerator SidewaysMovementRoutine(float toPositionX)
    {
        float fromPositionX = transform.position.x;
        float timerNormalized = 0f;
        float distanceMoved = 0f;
        float distanceToMove = toPositionX - fromPositionX;

        isMovingSideways = true;

        while (timerNormalized < 1f)
        {
            timerNormalized += Time.deltaTime * (1f / sideMovementTime);

            float movementDelta = Mathf.SmoothStep(0f, distanceToMove, timerNormalized) - distanceMoved;
            distanceMoved += movementDelta;

            controller.Move(Vector3.right * movementDelta);
            yield return new WaitForEndOfFrame();
        }

        float deviation = Mathf.Abs(toPositionX - transform.position.x);
        float maxDeviation = 0.1f;

        if (deviation > maxDeviation)
            yield return StartCoroutine("SidewaysMovementRoutine", toPositionX);

        isMovingSideways = false;
    }

    private IEnumerator SlideLandingPhaseCoroutine()
    {
        float superGravityMultiplier = 25f;

        while (IsOnGround() == false)
        {
            playerVelocity.y += Gravity * superGravityMultiplier * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return StartCoroutine("SlideMainPhaseRoutine");
    }

    private IEnumerator SlideMainPhaseRoutine()
    {
        isSliding = true;
        OnSlide?.Invoke();

        float colliderHeightShrinked = colliderHeightInitial / 2f;
        Vector3 colliderCenterShrinked = colliderCenterInitial;
        colliderCenterShrinked.y /= 2f;

        controller.center = colliderCenterShrinked;
        controller.height = colliderHeightShrinked;
        capsuleCollider.center = colliderCenterShrinked;
        capsuleCollider.height = colliderHeightShrinked;
        
        float timerNormalized = 0f;
        while (timerNormalized < 1f)
        {
            timerNormalized += Time.deltaTime * 1f / slidingTime;
            yield return new WaitForEndOfFrame();
        }

        CancelSlide();
    }

    private void CancelSlide()
    {
        if (isSliding == false) return;

        StopCoroutine("SlideMainPhaseRoutine");

        controller.center = colliderCenterInitial;
        controller.height = colliderHeightInitial;
        capsuleCollider.center = colliderCenterInitial;
        capsuleCollider.height = colliderHeightInitial;

        isSliding = false;
    }

    private void ApplyGravity()
    {
        if (IsOnGround() && playerVelocity.y < 0f)
            playerVelocity.y = 0f;
        else
            playerVelocity.y += Gravity * gravityMultiplier * Time.deltaTime;
    }

    private bool IsOnGround()
    {
        float checkSphereRadius = .1f;
        return Physics.CheckSphere(transform.position, checkSphereRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void RecalculateJumpDistance()
    {
        float jumpTime = 2 * JumpHeight / Mathf.Sqrt(2 * -CurrentGravity);
        JumpDistance = jumpTime * RunSpeed;
    }
}