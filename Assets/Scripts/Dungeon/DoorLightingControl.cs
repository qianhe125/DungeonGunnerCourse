using System;
using System.Collections;
using UnityEngine;
using VHierarchy.Libs;

[DisallowMultipleComponent]
public class DoorLightingControl : MonoBehaviour
{
    private bool isLit = false;
    private Door door;

    private void Awake()
    {
        door = GetComponent<Door>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FadeInDoor(door);
    }

    public void FadeInDoor(Door door)
    {
        Material material = new Material(GameResources.Instance.variableLitShader);

        if (!isLit)
        {
            SpriteRenderer[] spriteRendererArray = GetComponentsInParent<SpriteRenderer>();

            foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
            {
                StartCoroutine(FadeInDoorRoutine(spriteRenderer, material));
            }
            isLit = true;
        }
    }

    private IEnumerator FadeInDoorRoutine(SpriteRenderer spriteRenderer, Material material)
    {
        spriteRenderer.material = material;

        for (float i = .05f; i <= 1f; i += Time.deltaTime / Setting.fideInTime)
        {
            material.SetFloat("_Alpha", i);//i只在0~1之间
            yield return null;
        }
        spriteRenderer.material = GameResources.Instance.litMaterial;
    }
}