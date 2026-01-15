using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    public string id;
    public List<string> parentRoomNodeIDList = new();
    public List<string> childRoomNodeIDList = new();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    public RoomNodeTypeSO roomNodeType;

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    private List<String> types = new List<String>();

    public void Draw(GUIStyle roomNodeStyle)
    {
        GUILayout.BeginArea(rect, roomNodeStyle);

        EditorGUI.BeginChangeCheck();
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            PopupSelections();
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);
        GUILayout.EndArea();
    }

    private void PopupSelections()
    {
        var names = GetRoomNodeTypesToDisplay();

        int selected = types.FindIndex(x => x == roomNodeType.roomNodeTypeName);

        int selection = EditorGUILayout.Popup("", selected, names);

        string name = types[selection];

        roomNodeType = roomNodeTypeList.list.Find(x => x.roomNodeTypeName == name);

        if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
            !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
             !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
        {
            RemoveAllChildNodesLinks();
        }
    }

    private void RemoveAllChildNodesLinks()
    {
        if (childRoomNodeIDList.Count > 0)
        {
            for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
            {
                RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                if (childRoomNode != null)
                {
                    RemoveChildRoomNodeIDFromNode(childRoomNode.id);
                    childRoomNode.RemoveParentRoomNodeIDFromNode(id);
                }
            }
        }
    }

    private string[] GetRoomNodeTypesToDisplay()
    {
        types.Clear();
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                types.Add(roomNodeTypeList.list[i].roomNodeTypeName);
            }
        }
        return types.ToArray();
    }

    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();//创建一个独特的ID序号
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;//选中之后Inspector转到选定指定脚本
        isSelected = !isSelected;
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineForm(this, currentEvent.mousePosition);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessMouseMoveDragEvent(currentEvent);
        }
    }

    private void ProcessMouseMoveDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DrawNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DrawNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }

    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossRoom = false;
        foreach (var roomNode in roomNodeGraph.roomNodeList)
        {
            //是否已经连接过Boss节点
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossRoom = true;
        }
        //已经连接过Boss节点不允许再次连接
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossRoom)
        {
            Debug.Log("已经连接过Boss节点不允许再次连接");
            return false;
        }
        //不能自己连接自己
        if (childID == id)
        {
            Debug.Log("不能自己连接自己");
            return false;
        }
        //不能父子对连
        if (parentRoomNodeIDList.Contains(childID))
        {
            Debug.Log("不能父子对连");
            return false;
        }
        //不能子对象二次连接
        if (childRoomNodeIDList.Contains(childID))
        {
            Debug.Log("不能子对象二次连接");
            return false;
        }
        //如果childID=="None"直接返回
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
        {
            Debug.Log("空节点请设置");
            return false;
        }
        //走廊的下方不能放走廊
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            Debug.Log("走廊的下方不能放走廊");
            return false;
        }
        //非走廊房间与非走廊房间不能相连
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            Debug.Log("非走廊房间与非走廊房间不能相连");
            return false;
        }
        //一个房间最多链接三个走廊
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Setting.maxChildCorridors)
        {
            Debug.Log("一个房间最多链接三个走廊");
            return false;
        }
        //入口房间不能成为子节点
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
        {
            Debug.Log("入口房间不能成为子节点");
            return false;
        }
        return true;
    }

    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        if (parentID == id)
        {
            return false;
        }
        if (parentRoomNodeIDList.Contains(parentID))
        {
            return false;
        }
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    //删除指定ID的节点
    public bool RemoveChildRoomNodeIDFromNode(string childID)
    {
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    //删除指定ID的节点
    public bool RemoveParentRoomNodeIDFromNode(string parentID)
    {
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }

#endif
}