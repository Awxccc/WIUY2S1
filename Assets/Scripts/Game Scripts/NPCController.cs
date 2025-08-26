using System.Collections;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Idle, Walking }

    [SerializeField] private NPCState currentState = NPCState.Idle;

    public float moveSpeed = 2f;
    public LayerMask groundLayer;

    public Vector2 idleTimeRange = new(2f, 5f);

    public Vector2 walkTimeRange = new(3f, 7f);

    private Rigidbody2D rb;
    private float currentDirection = 1f;
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        initialScale = spriteRenderer.transform.parent.localScale;
    }
    void Start()
    {
        StartCoroutine(FSM());
    }

    void FixedUpdate()
    {
        if (currentState == NPCState.Walking)
        {
            if (!IsGroundAhead() || IsWallAhead())
            {
                Flip();
            }
            rb.linearVelocity = new Vector2(currentDirection * moveSpeed, rb.linearVelocity.y);
        }
    }

    private IEnumerator FSM()
    {
        while (true)
        {
            switch (currentState)
            {
                case NPCState.Idle:
                    yield return StartCoroutine(IdleState());
                    break;
                case NPCState.Walking:
                    yield return StartCoroutine(WalkingState());
                    break;
            }
        }
    }

    private IEnumerator IdleState()
    {
        animator.SetBool("IsWalking", false);
        rb.linearVelocity = Vector2.zero;
        float idleDuration = Random.Range(idleTimeRange.x, idleTimeRange.y);
        yield return new WaitForSeconds(idleDuration);
        currentState = NPCState.Walking;
    }

    private IEnumerator WalkingState()
    {
        currentDirection = Random.value > 0.5f ? 1f : -1f;
        SetDirection(currentDirection);
        animator.SetBool("IsWalking", true);
        float walkDuration = Random.Range(walkTimeRange.x, walkTimeRange.y);
        yield return new WaitForSeconds(walkDuration);

        currentState = NPCState.Idle;
    }

    private void Flip()
    {
        currentDirection *= -1;
        SetDirection(currentDirection);
    }
    private void SetDirection(float direction)
    {
        transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
    }
    public void SetAppearance(HumanSprites appearance)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = appearance.sprite;
        }

        if (animator != null)
        {
            animator.runtimeAnimatorController = appearance.animatorOverride;
        }
    }
    private bool IsGroundAhead()
    {
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, -spriteRenderer.bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, 0.2f, groundLayer);
        Debug.DrawRay(raycastOrigin, Vector2.down * 0.2f, Color.red);
        return hit.collider != null;
    }

    private bool IsWallAhead()
    {
        float checkDistance = 0.3f;
        Vector2 raycastOrigin = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, new Vector2(currentDirection, 0), checkDistance, groundLayer);

        Debug.DrawRay(raycastOrigin, new Vector2(currentDirection, 0) * checkDistance, Color.blue);

        return hit.collider != null;
    }
}