using System;
using UnityEngine;

[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    [SerializeField] private Transform weaponRotationPointTransform;

    private AimWeaponEvent aimWeaponEvent;

    private void Awake()
    {
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
    }

    private void OnEnable()
    {
        aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeapon;
    }

    private void OnDisable()
    {
        aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeapon;
    }

    private void AimWeaponEvent_OnWeapon(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        Aim(aimWeaponEventArgs.aimDirection, aimWeaponEventArgs.aimAngle);
    }

    private void Aim(AimDirection aimDirection, float aimAngle)
    {
        weaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);
        float scaleValueY = aimDirection switch
        {
            AimDirection.Up or
            AimDirection.UpRight or
            AimDirection.Right or
            AimDirection.Down => 1,

            AimDirection.Left or
            AimDirection.UpLeft or
            _ => -1,
        };

        weaponRotationPointTransform.localScale = new Vector3(1f, scaleValueY, 0);
    }

    #region OnValidate

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRotationPointTransform), weaponRotationPointTransform);
    }

    #endregion OnValidate
}