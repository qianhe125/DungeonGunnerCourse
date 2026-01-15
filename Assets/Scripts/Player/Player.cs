using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]//对同一个对象上的多个层级渲染
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]//与墙体发生碰撞
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PolygonCollider2D))]//与子弹检测伤害
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[RequireComponent(typeof(MovementByVelocity))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(AimWeaponEvent))]
[RequireComponent(typeof(AnimatePlayer))]
[RequireComponent(typeof(PlayerControl))]
public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerDetailsSO playerDetails;

    [HideInInspector] public Health health;

    [HideInInspector] public SpriteRenderer spriteRenderer;

    [HideInInspector] public Animator animator;

    [HideInInspector] public IdleEvent idleEvent;

    [HideInInspector] public MovementByVelocityEvent movementEvent;

    [HideInInspector] public AimWeaponEvent aimWeaponEvent;

    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;

    private void Awake()
    {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        idleEvent = GetComponent<IdleEvent>();
        movementEvent = GetComponent<MovementByVelocityEvent>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
    }

    public void Initialize(PlayerDetailsSO playerDetails)
    {
        this.playerDetails = playerDetails;
        SetPlayerHealth();
    }

    private void SetPlayerHealth()
    {
        health.SetStartingHealth(playerDetails.playerHealthAmount);
    }
}