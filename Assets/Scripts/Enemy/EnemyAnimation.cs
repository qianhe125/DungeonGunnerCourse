using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //通过更换动画控制器来更换怪物
    public void SetAnimator(RuntimeAnimatorController animatorController, Color spriteColor)
    {
        animator.runtimeAnimatorController = animatorController;
        spriteRenderer.color = spriteColor;
    }
}