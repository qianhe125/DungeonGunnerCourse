using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HelperUtilities : MonoBehaviour
{
    public static Camera mainCamera;

    /// <summary>
    /// 得到鼠标的世界坐标
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        Vector3 mouseScreenPosition = Input.mousePosition;

        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0, Screen.height);

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    public static bool ValidateCheckPositiveRange(Object thisObject, string fieldNameMinimum, float valueToCheckMinimum, string fieldNameMaximum,
        float valueToCheckMaximum, bool isZeroAllowed)
    {
        bool error = false;
        if (valueToCheckMinimum > valueToCheckMaximum)
        {
            Debug.Log("对于" + thisObject.name.ToString() + "之中, " + fieldNameMinimum + "必须小于" + fieldNameMaximum);
        }
        if (ValidateCheckPositiveValue(thisObject, fieldNameMaximum, valueToCheckMinimum, isZeroAllowed))
        {
            error = true;
        }
        if (ValidateCheckPositiveValue(thisObject, fieldNameMaximum, valueToCheckMaximum, isZeroAllowed))
        {
            error = true;
        }
        return error;
    }

    /// <summary>
    /// 得到某个方向的度数
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static float GetAngleFromVector(Vector3 direction)
    {
        float radians = Mathf.Atan2(direction.y, direction.x);
        float degrees = radians * Mathf.Rad2Deg;

        return degrees;
    }

    public static AimDirection GetAimDirection(float angleDegrees)
    {
        AimDirection aimDirection;

        if (angleDegrees >= 22f && angleDegrees <= 67f)
        {
            aimDirection = AimDirection.UpRight;
        }
        else if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            aimDirection = AimDirection.Up;
        }
        else if (angleDegrees > 112f && angleDegrees <= 158f)
        {
            aimDirection = AimDirection.UpLeft;
        }
        else if ((angleDegrees > 158f && angleDegrees <= 180f) || (angleDegrees > -180f && angleDegrees <= -135f))
        {
            aimDirection = AimDirection.Left;
        }
        else if (angleDegrees > -135f && angleDegrees <= -45f)
        {
            aimDirection = AimDirection.Down;
        }
        else if ((angleDegrees > -45f && angleDegrees <= 0f) || (angleDegrees > 0 && angleDegrees < 22f))
        {
            aimDirection = AimDirection.Right;
        }
        else
        {
            aimDirection = AimDirection.Right;
        }
        return aimDirection;
    }

    public static bool ValidateCheckEmptyString(Object thisObject, string fileName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fileName + " 为空,必须包含一个值" + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    //检查对象是否为空
    public static bool ValidateCheckNullValue(Object thisObject, string fileName, Object objectCheck)
    {
        if (objectCheck == null)
        {
            Debug.Log(fileName + "为空,必须包含一个值" + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    //对容器进行检查
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumableObjectToCheck == null)
        {
            Debug.Log(fieldName + "is null in object" + thisObject.name.ToString());
            return true;
        }
        //检查每一个成员变量是否为空项
        foreach (var item in enumableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " 有空值" + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }
        if (count == 0)
        {
            Debug.Log(fieldName + " 没有一个值" + thisObject.name.ToString());
        }
        return error;
    }

    //检查数值
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed)
    {
        bool error = false;
        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + "必须为一个非负数");
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + "必须为一个正数" + thisObject.name.ToString());
                error = true;
            }
        }
        return error;
    }

    public static Vector3 GetSpawnPositionNearestToPlayer(Vector3 playerPosition)
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instantiatedRoom.grid;

        Vector3 nearestSpawnPosition = new Vector3(10000f, 10000f, 0);
        //将每一个出生点遍历在最近的出生点出生
        foreach (Vector2Int spawnPositionGrid in currentRoom.spawnPositionArray)
        {
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPositionGrid);
            if (Vector3.Distance(spawnPositionWorld, playerPosition) < Vector3.Distance(nearestSpawnPosition, playerPosition))
            {
                nearestSpawnPosition = spawnPositionWorld;
            }
        }
        return nearestSpawnPosition;
    }

    /// <summary>
    /// 获取指定文件夹下的所有预制体
    /// </summary>
    private static List<GameObject> GetPrefabsInFolder(string folderPath)
    {
        List<GameObject> prefabs = new List<GameObject>();

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"无效的文件夹路径: {folderPath}");
            return prefabs;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        return prefabs;
    }
}