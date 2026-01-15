using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeNormalStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private static RoomNodeTypeListSO roomNodeTypeList;

    private static RoomNodeSO currentRoomNode;
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    private const float connectLineWidth = 3f;
    private const float connectingLineArrowSize = 8f;

    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    private Vector2 graphOffset;
    private Vector2 graphDrag;//累计的移动值

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
        //if (roomPrefabs == null || roomPrefabs.Count == 0)
        //{
        //    roomPrefabs = new List<GameObject>();
        //    Debug.Log("已查找");
        //    roomPrefabs = HelperUtilities.GetPrefabsInFolder("Assets/Prefabs/Dungeon/Rooms/Catacombs");
        //}
        //if (roomPrefabs.Count > 0)
        //{
        //    foreach (GameObject roomPrefab in roomPrefabs)
        //    {
        //        var tilemaps = roomPrefab.GetComponentsInChildren<TilemapRenderer>();
        //        Debug.Log(tilemaps.Length);
        //        if (tilemaps.Length > 0)
        //        {
        //            Debug.Log("已赋值");
        //            tilemaps[0].sortingLayerName = "Ground";
        //            tilemaps[1].sortingLayerName = "Ground Decoration 1";
        //            tilemaps[2].sortingLayerName = "Ground Decoration 2";
        //            tilemaps[3].sortingLayerName = "Front";
        //            tilemaps[4].sortingLayerName = "Collision";
        //            tilemaps[5].sortingLayerName = "Minimap";
        //        }
        //        EditorUtility.SetDirty(roomPrefab);
        //        AssetDatabase.SaveAssets();
        //    }
        //}
        //if (myPrefabs == null || myPrefabs.Count == 0)
        //{
        //    myPrefabs = new List<GameObject>();
        //    Debug.Log("已查找1");
        //    myPrefabs = HelperUtilities.GetPrefabsInFolder("Assets/Prefabs/Dungeon/Rooms/Sorcery");
        //}
        //if (myPrefabs.Count > 0)
        //{
        //    foreach (GameObject roomPrefab in myPrefabs)
        //    {
        //        var tilemaps = roomPrefab.GetComponentsInChildren<TilemapRenderer>();
        //        Debug.Log(tilemaps.Length);
        //        if (tilemaps.Length > 0)
        //        {
        //            Debug.Log("已赋值1");
        //            tilemaps[0].sortingLayerName = "Ground";
        //            tilemaps[1].sortingLayerName = "Ground Decoration 1";
        //            tilemaps[2].sortingLayerName = "Ground Decoration 2";
        //            tilemaps[3].sortingLayerName = "Front";
        //            tilemaps[4].sortingLayerName = "Collision";
        //            tilemaps[5].sortingLayerName = "Minimap";
        //        }
        //        EditorUtility.SetDirty(roomPrefab);
        //        AssetDatabase.SaveAssets();
        //    }
        //}
    }

    private void OnEnable()
    {
        //选中其他Graph就改变渲染
        Selection.selectionChanged += InSpectorSelectionChanged;

        roomNodeNormalStyle = new GUIStyle();
        roomNodeNormalStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeNormalStyle.normal.textColor = Color.white;
        roomNodeNormalStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeNormalStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InSpectorSelectionChanged;
    }

    private void InSpectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }

    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            OpenWindow();
            currentRoomNodeGraph = roomNodeGraph;
            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            DrawBackgroundGrid(gridSmall, .2f, Color.gray);
            DrawBackgroundGrid(gridLarge, .3f, Color.gray);

            DrawLine();

            DrawRoomConnections();

            ProcessEvent(Event.current);//处理当前的点击事件

            DrawRoomNode();
        }
        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawBackgroundGrid(float gridSize, float gridGravity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridGravity);

        graphOffset = graphDrag;
        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);//如果移动超过网格大小,循环回到起始位置

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height, 0f) + gridOffset);
        }
        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }
        Handles.color = Color.white;
    }

    private void DrawLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineForm.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineForm.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectLineWidth);
        }
    }

    private void DrawRoomNode()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeNormalStyle);
            }
        }
        GUI.changed = true;
    }

    private void ProcessEvent(Event currentEvent)
    {
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineForm != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        //从最后一位往前找最快
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        //右键抬起意味着取消链接
        if (currentEvent.button == 1 || currentRoomNodeGraph.roomNodeToDrawLineForm != null)
        {
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                if (currentRoomNodeGraph.roomNodeToDrawLineForm.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineForm.id);
                }
            }

            ClearDraw();
        }
    }

    private void ClearDraw()
    {
        currentRoomNodeGraph.roomNodeToDrawLineForm = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    private void ProcessLeftMouseDragEvent(Vector2 delta)
    {
        graphDrag += delta;
        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DrawNode(delta);
        }
        GUI.changed = true;
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        //防止在空处右键拖拽
        if (currentRoomNodeGraph.roomNodeToDrawLineForm != null)
        {
            DragConnecting(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConnecting(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        else if (currentEvent.button == 0)
        {
            ClearDraw();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    private void SelectAllRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("创建新节点"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("选中所有节点"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("删除选择节点的链接线"), false, DeletedSelectedRoomLinks);
        menu.AddItem(new GUIContent("删除选中的节点"), false, DeletedSelectedRoomNodes);
        menu.ShowAsContext();//作为下拉框展示
    }

    private void DeletedSelectedRoomNodes()
    {
        Stack<RoomNodeSO> nodesDeletionStack = new Stack<RoomNodeSO>();
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                nodesDeletionStack.Push(roomNode);
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeIDFromNode(roomNode.id);
                    }
                }
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeIDFromNode(roomNode.id);
                    }
                }
            }
        }
        while (nodesDeletionStack.Count > 0)
        {
            RoomNodeSO roomNodeSO = nodesDeletionStack.Pop();

            currentRoomNodeGraph.roomNodeList.Remove(roomNodeSO);
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeSO.id);

            DestroyImmediate(roomNodeSO, true);

            AssetDatabase.SaveAssets();
        }
    }

    private void DeletedSelectedRoomLinks()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromNode(childRoomNode.id);//自己作为父节点移除儿子
                        childRoomNode.RemoveParentRoomNodeIDFromNode(roomNode.id);//儿子作为字节移除父亲
                    }
                }
            }
        }
        ClearAllSelectedRoomNodes();//将所有选中重置
    }

    private void CreateRoomNode(object userData)
    {
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            //默认创建入口节点
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance == true));
        }
        CreateRoomNode(userData, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);//将RoomNode资源节点放入currentRoomNodeGraph对应的文件中,会在下方生成RoomNode

        AssetDatabase.SaveAssets();

        currentRoomNodeGraph.OnValidate();
    }

    private void DrawRoomConnections()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        Vector2 midPosition = (startPosition + endPosition) / 2f;

        Vector2 direction = endPosition - startPosition;
        Vector2 arrowTrailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;//做垂直偏移
        Vector2 arrowTrailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;
        Handles.DrawBezier(arrowHeadPoint, arrowTrailPoint1, arrowHeadPoint, arrowTrailPoint1, Color.white, null, connectLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTrailPoint2, arrowHeadPoint, arrowTrailPoint2, Color.white, null, connectLineWidth);
        Handles.DrawBezier(arrowTrailPoint1, arrowTrailPoint2, arrowTrailPoint1, arrowTrailPoint2, Color.white, null, connectLineWidth);
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectLineWidth);

        GUI.changed = true;
    }
}