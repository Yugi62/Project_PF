using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private float _cool_Time;          //ÄðÅ¸ÀÓ

    protected float cool_Time { get { return _cool_Time; } }

    protected Player player;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected float current_Time;

    protected abstract void EnableSkill();

    protected void Init()
    {
        player = GetComponentInParent<Player>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        current_Time = 0f;

        spriteRenderer.enabled = false;
    }
}
