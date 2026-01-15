using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDetails_", menuName = "Scriptable Objects/Weapon Details")]
public class WeaponDetailsSO : ScriptableObject
{
    [Tooltip("武器名称")]
    public string weaponName;

    [Tooltip("武器图片")]
    public Sprite weaponSprite;

    [Tooltip("武器的发射点")]
    public Vector3 weaponShootPosition;

    [Tooltip("是否可以无限发射子弹")]
    public bool hasInfiniteAmmo = false;

    [Tooltip("无限的弹夹")]
    public bool hasInfiniteCapacity = false;

    [Tooltip("武器的弹夹容量")]
    public int weaponClipAmmoCapacity = 6;

    [Tooltip("每发弹夹的容量")]
    public int weaponAmmoCapacity = 100;

    [Tooltip("开火的延迟")]
    public float weaponFireRate = .2f;

    [Tooltip("激光武器的充能速率")]
    public float weaponPrechargeTime = 0f;

    [Tooltip("重新装弹的时间")]
    public float weaponReloadTime = 0f;
}