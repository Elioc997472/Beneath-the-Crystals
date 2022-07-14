using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player _instance;
    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float accel;
    [SerializeField] private float deccel;
    [SerializeField] private float coyoteTime = .5f;
    [SerializeField] private float wallJumpLerpAmount = 1;

    [Header("Game Variables")]
    [SerializeField] private int score;
    [SerializeField] private float reach;
    [SerializeField] private int miningStrength;
    [SerializeField] private float miningSpeed; //dmg per .1f second
    private BreakableBlock currTarget;

    private Rigidbody2D rb;
    private CapsuleCollider2D playerCollider;
    private LayerMask ground;

    private bool isJumping;
    private bool isWallJumping;
    private bool isMining;
    private bool isFacingRight;
    private bool isFalling;

    private float LastOnGroundTime;

    #region CHECK PARAMETERS
    [Header("Checks")]

    [SerializeField] private Vector2 groundCastSize;
    [Space(5)]
    [SerializeField] private Transform frontWallCheckPoint;
    [SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize;

    public int MiningStrength { get => miningStrength; set => miningStrength = value; }
    public float MiningSpeed { get => miningSpeed; set => miningSpeed = value; }
    public int Score { get => score; set => score = value; }
    #endregion
    void Awake()
    {
        ground = LayerMask.GetMask("Ground");
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        _instance = this;
    }

    void Update()
    {
        LastOnGroundTime -= Time.deltaTime;

        currTarget = GetBreakableBlock();
        Debug.Log(IsGrounded());
        if (IsGrounded() || (LastOnGroundTime > 0 && !isJumping)) if (Input.GetKey(KeyCode.W)) Jump();

        //Mining
        BreakableBlock target = GetBreakableBlock();
        if (target != currTarget)
        {
            if (currTarget != null) currTarget.ResetDamage();
            currTarget = target;
        }
        if (Input.GetMouseButton(0)) Mine(currTarget);
        else if (currTarget != null) currTarget.ResetDamage();

        //Animator
        if (rb.velocity.x != 0) isFacingRight = rb.velocity.x > 0 ? true : false;
    }

    private void FixedUpdate()
    {
        if (isWallJumping) Run(wallJumpLerpAmount);
        else Run(1);
    }

    private void Run(float lerpAmount)
    {
        float targetSpeed = Input.GetAxis("Horizontal") * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > .01f) ? accel : deccel;
        if ((rb.velocity.x > targetSpeed && targetSpeed > 0.01f)) accelRate = 0;
        float movement = speedDif * accelRate;
        movement = Mathf.Lerp(rb.velocity.x, movement, lerpAmount);
        rb.AddForce(movement * Vector2.right);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = true;
    }

    private void Mine(BreakableBlock target)
    {
        if (target != null)
        {
            target.Mining();
        }
    }

    private bool IsGrounded()
    {
        Vector3 startCast = playerCollider.bounds.center;
        startCast.y -= .6f;
        RaycastHit2D raycast = Physics2D.BoxCast(startCast, groundCastSize, 0, Vector2.down, .05f, ground);
        if (raycast.collider != null)
        {
            LastOnGroundTime = coyoteTime;
            isJumping = false;
            return true;
        }
        return false;
    }

    private BreakableBlock GetBreakableBlock()
    {
        Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 angle = mouseWorldPos - playerPos;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, angle, reach, ground);
        Debug.DrawRay(transform.position, angle, Color.green);
        if (hit.transform != null) return hit.transform.GetComponent<BreakableBlock>();
        else return null;
    }

}
