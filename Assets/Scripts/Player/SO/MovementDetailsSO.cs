using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/MovementDetails")]
public class MovementDetailsSO : ScriptableObject
{
    [SerializeField] private float miniMoveSpeed = 8f;

    [SerializeField] private float maxMoveSpeed = 8f;

    [SerializeField] private bool allowRoll = false;

    [ShowIf("@allowRoll")] public float rollDistance;

    [ShowIf("@allowRoll")] public float rollSpeed;

    [ShowIf("@allowRoll")] public float rollColdDownTime;

    public float GetMoveSpeed()
    {
        if (miniMoveSpeed == maxMoveSpeed)
        {
            return miniMoveSpeed;
        }
        else
        {
            return Random.Range(miniMoveSpeed, maxMoveSpeed);
        }
    }

    #region OnValidation

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(miniMoveSpeed), miniMoveSpeed, nameof(maxMoveSpeed), maxMoveSpeed, false);

        if (allowRoll)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollDistance), rollDistance, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollSpeed), rollSpeed, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollColdDownTime), rollColdDownTime, false);
        }
    }

    #endregion OnValidation
}