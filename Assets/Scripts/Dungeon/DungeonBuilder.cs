using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonBuilder : SingletonMonoBehavior<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    public Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList = null;
    private bool dungeonBuildSuccessful = false;

    private void OnEnable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("_Alpha", 0f);
    }

    private void OnDisable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("_Alpha", 1f);
    }

    protected override void Awake()
    {
        base.Awake();

        LoadRoomNodeTypeList();
    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts <= Setting.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;
            //随机选择一个层级模版来建造
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;

            dungeonBuildSuccessful = false;
            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Setting.maxDungeonRebuildAttemptsForRoomGraph)
            {
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);

                if (dungeonBuildSuccessful)
                {
                    InstantiateRoomGameObjects();
                }
            }
        }

        return dungeonBuildSuccessful;
    }

    private void InstantiateRoomGameObjects()
    {
        foreach (KeyValuePair<string, Room> keyValue in dungeonBuilderRoomDictionary)
        {
            Room room = keyValue.Value;
            //我们只是知道了lowerBounds的世界坐标,但创建房间时的中心点,并非是lowerBounds,相对中心点的位置是templateLowerBounds
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0);

            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            InstantiatedRoom instantiatesRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatesRoom.room = room;

            instantiatesRoom.Initialize(roomGameObject);

            room.instantiatedRoom = instantiatesRoom;
        }
        Debug.Log("构建成功");
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));
        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log(roomNodeGraph + "未设置入口节点");
            return false;
        }
        bool noRoomOverlaps = true;
        noRoomOverlaps = ProcessRoomsInOpenNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        //将所有的字节点完全建立才能出去
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        //有节点剩余说明建立存在失败
        else
        {
            return false;
        }
    }

    private bool ProcessRoomsInOpenNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();
            //将子节点压入栈内,可以实现先序遍历
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }
            if (roomNode.roomNodeType.isEntrance)//入口节点不用做重叠判断
            {
                //得到随机的,与之匹配的模板信息
                RoomTemplateSO roomTemplate = GetRandomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else//其他节点判断是否重叠
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }
        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;
        while (roomOverlaps)
        {
            //父对象所有未连接的门道
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false;
            }
            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }
        return true;//没有重叠就会跳出来
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        //当前模板应该选择的门道
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
        //lowerBounds会随时改变,doorwayParent.position - parentRoom.templateLowerBounds是作为相对坐标不会改变
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = doorway.orientation switch
        {
            Orientation.north => new Vector2Int(0, -1),
            Orientation.east => new Vector2Int(-1, 0),
            Orientation.south => new Vector2Int(0, 1),
            Orientation.west => new Vector2Int(1, 0),
            Orientation.none => Vector2Int.zero,
            _ => Vector2Int.zero,
        };
        //room.lowerBounds + doorway.position - room.templateLowerBounds = parentDoorwayPosition + adjustment;
        //lowerBounds随时改变
        room.lowerBounds = parentDoorwayPosition + adjustment - doorway.position + room.templateLowerBounds;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);
        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;

            return false;
        }
    }

    //对每一个已经存在的房间节点进行判断重叠
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (KeyValuePair<string, Room> keyValue in dungeonBuilderRoomDictionary)
        {
            Room room = keyValue.Value;
            //对所有已经放置的节点进行判断
            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }
            if (IsOverlappingRoom(roomToTest, room))
            {
                return room;
            }
        }
        return null;
    }

    private bool IsOverlappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = isOverlappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingY = isOverlappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if (isOverlappingX && isOverlappingY)
        {
            return true;
        }
        return false;
    }

    private bool isOverlappingInterval(int pos1min, int pos1max, int pos2min, int pos2max)
    {
        if (Mathf.Max(pos1min, pos2min) > Mathf.Min(pos1max, pos2max))
        {
            return false;
        }
        return true;
    }

    //得到与父门道相对的现在模板的门道
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (var doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;
        if (roomNode.roomNodeType.isCorridor)
        {
            //与走廊相接的房间节点
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));//获取南北走廊
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));//获取东西走廊
                    break;

                default:
                    break;
            }
        }
        else
        {
            //与走廊相连的普通节点
            roomTemplate = GetRandomTemplate(roomNode.roomNodeType);
        }
        return roomTemplate;
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isUnavailable && !doorway.isConnected)
                yield return doorway;
        }
    }

    //根据模板和信心创建一个新房间

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();
        room.id = roomNode.id;
        room.templateID = roomTemplate.guid;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);
        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
            //设置初始房间
            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }
        return room;
    }

    //对字符串进行深度拷贝
    private List<String> CopyStringList(List<string> originalStringList)
    {
        List<String> newStringList = new();
        foreach (string str in originalStringList)
        {
            newStringList.Add(str);
        }
        return newStringList;
    }

    //对Doorway的数据进行深度拷贝
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();
        foreach (Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;

            newDoorwayList.Add(newDoorway);
        }
        return newDoorwayList;
    }

    private RoomTemplateSO GetRandomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new();
        foreach (var roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }
        if (matchingRoomTemplateList.Count == 0)
            return null;

        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    public Room GetRoomByRoomID(string ID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(ID, out Room room))
        {
            return room;
        }
        return null;
    }

    private void ClearDungeon()
    {
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (var keyValue in dungeonBuilderRoomDictionary)
            {
                Room room = keyValue.Value;
                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log(roomNodeGraphList + "为空");
            return null;
        }
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        roomTemplateDictionary.Clear();

        foreach (var roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log(roomNodeTypeList + "存在重复的模版");
            }
        }
    }
}