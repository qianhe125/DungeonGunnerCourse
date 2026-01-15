using UnityEngine;

public static class Setting
{
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    public const float fideInTime = .5f;

    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;

    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollDown = Animator.StringToHash("rollDown");
    public static int rollRight = Animator.StringToHash("rollRight");

    public static float baseSpeedForPlayerAnimation = 8f;

    /// <summary>
    /// 一个节点的最多链接点
    /// </summary>
    public const int maxChildCorridors = 3;

    public static int open = Animator.StringToHash("open");

    public const string playerTag = "Player";
    public const string playerWeapon = "PlayerWeapon";
}