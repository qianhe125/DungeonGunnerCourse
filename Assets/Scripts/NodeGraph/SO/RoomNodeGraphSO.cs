using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [ShowInInspector]
    public RoomNodeTypeListSO roomNodeTypeList;

    [ShowInInspector]
    public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();

    [ShowInInspector]
    public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach (RoomNodeSO node in roomNodeList)
        {
            if (node.roomNodeType = roomNodeType)
            {
                return node;
            }
        }
        return null;
    }

    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }

    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomNode)
    {
        foreach (string childNodeID in parentRoomNode.childRoomNodeIDList)
        {
            yield return GetRoomNode(childNodeID);
        }
    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        foreach (RoomNodeSO roomNode in roomNodeList)
        {
            roomNodeDictionary[roomNode.id] = roomNode;
        }
    }

    [HideInInspector] public RoomNodeSO roomNodeToDrawLineForm;
    [HideInInspector] public Vector2 linePosition;

    public void SetNodeToDrawConnectionLineForm(RoomNodeSO node, Vector2 position)
    {
        roomNodeToDrawLineForm = node;
        linePosition = position;
    }
}