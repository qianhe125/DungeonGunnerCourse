using UnityEngine;

public abstract class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        { return instance; }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Debug.Log(gameObject.name + "重复存在,已删除");
            Destroy(gameObject);
        }
    }
}