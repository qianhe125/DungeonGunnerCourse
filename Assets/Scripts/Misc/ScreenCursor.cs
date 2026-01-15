using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
    private void Awake()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        //不会受到相机的影响
        transform.position = Input.mousePosition;
    }
}