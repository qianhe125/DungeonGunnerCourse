using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    public string levelName;

    public List<RoomTemplateSO> roomTemplateList;

    public List<RoomNodeGraphSO> roomNodeGraphList;

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomNodeGraphList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        foreach (var roomTemplate in roomTemplateList)
        {
            if (roomTemplate == null)
            {
                return;
            }
            if (roomTemplate.roomNodeType.isCorridorEW)
            {
                isEWCorridor = true;
            }
            if (roomTemplate.roomNodeType.isCorridorNS)
            {
                isNSCorridor = true;
            }
            if (roomTemplate.roomNodeType.isEntrance)
            {
                isEntrance = true;
            }
        }

        if (isEWCorridor == false)
        {
            Debug.Log(name.ToString() + "没有东西走廊发现");
        }
        if (isNSCorridor == false)
        {
            Debug.Log(name.ToString() + "没有南北走廊发现");
        }
        if (isEntrance == false)
        {
            Debug.Log(name.ToString() + "没有入口节点发现");
        }
        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;
            foreach (var roomNode in roomNodeGraph.roomNodeList)
            {
                if (roomNode == null)
                    continue;
                if (roomNode.roomNodeType.isEntrance || roomNode.roomNodeType.isCorridorEW || roomNode.roomNodeType.isCorridorNS
                    || roomNode.roomNodeType.isCorridor || roomNode.roomNodeType.isNone)
                {
                    continue;
                }
                bool isRoomNodeTypeFound = false;

                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null) continue;
                    if (roomTemplateSO.roomNodeType == roomNode.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }
                if (isRoomNodeTypeFound == false)
                {
                    Debug.Log(name.ToString() + "没有模板与之匹配" + roomNode.roomNodeType.name.ToString() + "--出自" + roomNodeGraph.name.ToString());
                }
            }
        }
    }
}