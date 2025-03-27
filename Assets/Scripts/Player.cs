using System;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    #region  Variaveis
    private static PlayerInput PlayerInput;
    private Rigidbody2D rb;

    //Variaveis de Movimento
    private Vector2 moveVelocity;
    private Vector2 movement;
    private bool IsFacingRight;
    private bool RunIsHeld;

    //Variaveis de input jump
    private bool jumpWasPressed;
    private bool jumpIsHeld;
    private bool jumpWasReleased;


    [Header("Walk")]
    public float MaxWalkSpeed = 10f;
    public float groundAcceleration = 5f;
    public float groundDecceleration = 20f;
    public float airAcceleration = 5f;
    public float airDecceleration = 5f;

    [Header("Run")]
    public float maxRunSpeed = 20f;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("HeadCheck")]
    public Transform headCheckPos;
    public Vector2 headCheckSize = new Vector2(0.5f, 0.05f);

    //Variaveis de controle
    bool isGrounded;
    bool headBumped;

    [Header("Jump")]
    public float jumpHeight = 8f;
    public int maxJumps = 2;
    public float jumpHeightCompensationFactor = 0.15f;
    public float timeTillJumpApex = 0.35f;
    public float gravityOnReleaseMultiplier = 2f;
    public float maxFallSpeed = 20f;
    //Variaveis de controle
    public float VerticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberUsedJumps;

    [Header("Jump Cut")]
    public float timeToCancelFullJump = 0.5f;

    [Header("Jump Apex")]
    public float ApexThreshold = 0.9f;
    public float ApexeHangTime = 0.05f;
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.15f;
    private float jumpBufferTimer;
    private bool jumpReleaseDuringBuffer;


    [Header("Jump Coyote Time")]
    public float jumpCoyoteTime = 0.1f;
    private float coyoteTimer;

    //Variaveis de Input
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpAction;

    //Ajustes de pulo e gravidade
    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    #endregion

    private void Awake()
    {
        //Inicializando o PlayerInput e suas actions
        PlayerInput = GetComponent<PlayerInput>();
        moveAction = PlayerInput.actions["Move"];
        runAction = PlayerInput.actions["Sprint"];
        jumpAction = PlayerInput.actions["Jump"];
    }
    private void Start()
    {
        IsFacingRight = true;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //Actions
        movement = moveAction.ReadValue<Vector2>();
        RunIsHeld = runAction.IsPressed();
        jumpWasPressed = jumpAction.WasPressedThisFrame();
        jumpIsHeld = jumpAction.IsPressed();
        jumpWasReleased = jumpAction.WasReleasedThisFrame();

        JumpChecks();
        Timers();

    }

    private void FixedUpdate()
    {
        //Checks
        CheckCollisions();
        Jump();

        if (isGrounded)
        {
            Move(groundAcceleration, groundDecceleration, movement);
        }
        else
        {
            Move(airAcceleration, airDecceleration, movement);
        }
    }

    #region Movement

    private void Move(float acceleration, float decceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            FlipSprite(moveInput);
            Vector2 targetVelocity = Vector2.zero;
            if (RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * maxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MaxWalkSpeed;
            }

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, decceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }
    }


    //Função para inverter o sprite do jogador de acordo com sua direção
    private void FlipSprite(Vector2 moveInput)
    {
        //Verifica se esta olhando para a direita e tem um movimento para a esquerda
        //Ou se não está olhando e tem um movimento para a direita
        if (IsFacingRight && moveInput.x < 0 || !IsFacingRight && moveInput.x > 0)
        {
            IsFacingRight = !IsFacingRight;
            Vector3 ls = transform.localScale; //Pega o localscale do player
            ls.x *= -1; //Multiplica por -1 para inverter o sprite no eixo x
            transform.localScale = ls; //O localscale do player recebe o novo localScale ja 
        }
    }
    #endregion

    #region Jump

    private void Jump()
    {
        //Aplicar gravidade enquanto pula
        if (isJumping)
        {
            if (headBumped)
            {
                isFastFalling = true;
            }

            
        }

        //check bater a cabeça

        //controle do apex

        //gravidade enquanto subindo

        //gravidade descendo

        //jump cut

        //gravidade normal enquanto falling

        //limitar fall speed
    }

    private void JumpChecks()
    {
        //Quando o botao é pressionado
        if (jumpWasPressed)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpReleaseDuringBuffer = false;
        }
        //Quando o botao é solto
        if (jumpWasReleased)
        {
            if (jumpBufferTimer > 0f)
            {
                jumpReleaseDuringBuffer = true;
            }

            if (isJumping && VerticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = timeToCancelFullJump;
                    VerticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        //Iniciar pulo com coyote time e jump buffer
        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpReleaseDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        //double jump
        else if (jumpBufferTimer > 0f && isJumping && numberUsedJumps < maxJumps)
        {
            isFastFalling = false;
            InitiateJump(1);
        }
        //air jump depois do coyote timer
        else if (jumpBufferTimer > 0f && isFalling && numberUsedJumps < maxJumps)
        {
            InitiateJump(2);
            isFastFalling = false;
        }

        //encostou no chao
        if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            numberUsedJumps = 0;
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberUsedJumps)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        jumpBufferTimer = 0f;
        numberUsedJumps += numberUsedJumps;
        VerticalVelocity = InitialJumpVelocity;
    }

    #endregion

    #region Collisions

    private void IsGrounded()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void BumpedHead()
    {
        if (Physics2D.OverlapBox(headCheckPos.position, headCheckSize, 0, groundLayer))
        {
            headBumped = true;
        }
        else
        {
            headBumped = false;
        }
    }

    private void CheckCollisions()
    {
        IsGrounded();
        BumpedHead();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(headCheckPos.position, headCheckSize);
    }

    #endregion


    #region Gravity Adjusts

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * timeTillJumpApex;
    }

    #endregion

    #region Timers

    private void Timers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = jumpCoyoteTime;
        }
    }

    #endregion
}
