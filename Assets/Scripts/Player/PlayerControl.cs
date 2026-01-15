using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInputSystem))]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private MovementDetailsSO movementDetails;

    [SerializeField] private Transform weaponShootPosition;

    private PlayerInputSystem playerInput;

    private Player player;
    private float moveSpeed;

    //翻滚相关
    private Coroutine playerRollCoroutine;

    private WaitForFixedUpdate waitForFixedUpdate;
    private bool isPlayerRolling;
    private float playerRollCooldownTimer = 0;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerInput = new PlayerInputSystem();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();

        SetPlayerAnimationSpeed();
    }

    private void SetPlayerAnimationSpeed()
    {
        player.animator.speed = moveSpeed / Setting.baseSpeedForPlayerAnimation;
    }

    #region Input

    private Vector2 movementDirection;
    private bool rollKeyButtonDown = false;

    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.Player.Movement.performed += ctx =>
        {
            movementDirection = ctx.ReadValue<Vector2>();
        };
        playerInput.Player.Movement.canceled += ctx =>
        {
            movementDirection = Vector2.zero;
        };
        playerInput.Player.Roll.started += ctx =>
        {
            rollKeyButtonDown = true;
        };
        playerInput.Player.Roll.canceled += ctx =>
        {
            rollKeyButtonDown = false;
        };
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    #endregion Input

    private void Update()
    {
        if (isPlayerRolling) { return; }
        MovementInput();

        WeaponInput();

        ResetPlayerRollCooldown();
    }

    private void ResetPlayerRollCooldown()
    {
        if (playerRollCooldownTimer > 0)
        {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }

    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        Vector3 mousePosition = HelperUtilities.GetMouseWorldPosition();
        //得到相对于手持武器点的方向,用于武器转动
        weaponDirection = (mousePosition - weaponShootPosition.position);
        //得到相对于角色位置的方向,用于角色转动
        Vector3 playerDirection = (mousePosition - transform.position);

        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    private void MovementInput()
    {
        //已设置DeadArea>0.1
        if (movementDirection != Vector2.zero)
        {
            Debug.Log(rollKeyButtonDown);
            if (!rollKeyButtonDown)
            {
                player.movementEvent.CallMovementByVelocityEvent(movementDirection, moveSpeed);
            }
            else if (playerRollCooldownTimer <= 0)
            {
                PlayRoll((Vector3)movementDirection);
            }
        }
        else
        {
            player.movementEvent.CallMovementByVelocityEvent(movementDirection, 0);
            player.idleEvent.CallIdleEvent();
        }
    }

    private void PlayRoll(Vector3 movementDirection)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(movementDirection));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            StopPlayerRollCoroutine();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision != null)
        {
            StopPlayerRollCoroutine();
        }
    }

    private void StopPlayerRollCoroutine()
    {
        if (playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);
            isPlayerRolling = false;
        }
    }

    private IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        float minDistance = .2f;
        isPlayerRolling = true;
        Vector3 targetPosition = player.transform.position + direction * movementDetails.rollDistance;

        while (Vector3.Distance(targetPosition, player.transform.position) > minDistance)
        {
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition, player.transform.position, movementDetails.rollSpeed, direction, isPlayerRolling);

            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;

        playerRollCooldownTimer = movementDetails.rollColdDownTime;

        player.transform.position = targetPosition;
    }

    #region OnValidate

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }

    #endregion OnValidate
}