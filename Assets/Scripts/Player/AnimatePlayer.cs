using System;
using UnityEngine;

public class AnimatePlayer : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.idleEvent.OnIdle += IdleEvent_OnIdle;

        player.movementEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;

        player.aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;

        player.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;
    }

    private void OnDisable()
    {
        player.idleEvent.OnIdle -= IdleEvent_OnIdle;

        player.movementEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;

        player.aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;

        player.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;
    }

    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        InitializeAimAnimationParameters();//翻滚时禁止瞄准
        InitializeRollAnimationParameters();
        SetMoveToPositionAnimationParameters(movementToPositionArgs);
    }

    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityArgs)
    {
        InitializeRollAnimationParameters();//移动时停止翻滚
        SetMovementAnimationParameters(movementByVelocityArgs.moveDirection, movementByVelocityArgs.moveSpeed);
    }

    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        InitializeRollAnimationParameters();//解除翻滚状态
        SetIdleAnimationParameters();
    }

    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        InitializeRollAnimationParameters();
        InitializeAimAnimationParameters();
        SetAimWeaponAnimationParameters(aimWeaponEventArgs.aimDirection);
    }

    private void SetAimWeaponAnimationParameters(AimDirection aimDirection)
    {
        switch (aimDirection)
        {
            case AimDirection.Up:
                player.animator.SetBool(Setting.aimUp, true);
                break;

            case AimDirection.UpLeft:
                player.animator.SetBool(Setting.aimUpLeft, true);
                break;

            case AimDirection.UpRight:
                player.animator.SetBool(Setting.aimUpRight, true);
                break;

            case AimDirection.Right:
                player.animator.SetBool(Setting.aimRight, true);
                break;

            case AimDirection.Left:
                player.animator.SetBool(Setting.aimLeft, true);
                break;

            case AimDirection.Down:
                player.animator.SetBool(Setting.aimDown, true);
                break;
        }
    }

    private void InitializeAimAnimationParameters()
    {
        player.animator.SetBool(Setting.aimUp, false);
        player.animator.SetBool(Setting.aimUpLeft, false);
        player.animator.SetBool(Setting.aimUpRight, false);
        player.animator.SetBool(Setting.aimRight, false);
        player.animator.SetBool(Setting.aimLeft, false);
        player.animator.SetBool(Setting.aimDown, false);
    }

    private void InitializeRollAnimationParameters()
    {
        player.animator.SetBool(Setting.rollLeft, false);
        player.animator.SetBool(Setting.rollRight, false);
        player.animator.SetBool(Setting.rollUp, false);
        player.animator.SetBool(Setting.rollDown, false);
    }

    private void SetMoveToPositionAnimationParameters(MovementToPositionArgs movementToPositionArgs)
    {
        if (movementToPositionArgs.isRolling)
        {
            if (movementToPositionArgs.moveDirection.x > 0)
            {
                player.animator.SetBool(Setting.rollRight, true);
            }
            else if (movementToPositionArgs.moveDirection.x < 0)
            {
                player.animator.SetBool(Setting.rollLeft, true);
            }
            else if (movementToPositionArgs.moveDirection.y > 0)
            {
                player.animator.SetBool(Setting.rollUp, true);
            }
            else if (movementToPositionArgs.moveDirection.y < 0)
            {
                player.animator.SetBool(Setting.rollDown, true);
            }
        }
    }

    private void SetIdleAnimationParameters()
    {
        player.animator.SetBool(Setting.isMoving, false);
        player.animator.SetBool(Setting.isIdle, true);
    }

    private void SetMovementAnimationParameters(Vector2 moveDirection, float moveSpeed)
    {
        player.animator.SetBool(Setting.isMoving, true);
        player.animator.SetBool(Setting.isIdle, false);
    }
}