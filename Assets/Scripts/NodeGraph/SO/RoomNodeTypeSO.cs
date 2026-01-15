using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    [Header("是否在地图编辑器中显示")]
    public bool displayInNodeGraphEditor = true;

    [Header("是否是一个走廊")]
    public bool isCorridor;

    [Header("南北朝向")]
    public bool isCorridorNS;

    [Header("东西朝向")]
    public bool isCorridorEW;

    [Header("是否作为一个入口")]
    public bool isEntrance;

    [Header("是否是BOSS房")]
    public bool isBossRoom;

    [Header("是否未被指派")]
    public bool isNone;

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
}