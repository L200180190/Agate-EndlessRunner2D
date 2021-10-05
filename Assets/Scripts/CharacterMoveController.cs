using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveController : MonoBehaviour
{
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;

    private Rigidbody2D rigidBody;


    [Header("Jump")]
    public float jumpAccel;

    private bool isJumping;
    private bool isOnGround;


    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;


    private Animator anim;

    private CharacterSoundController sound;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;

    [Header("Game Over")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;

    // Start is called before the first frame update
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();

        lastPositionX = transform.position.x;
    }

    // Update is called once per frame
    private void Update()
    {
        // Baca input dari player
        if (Input.GetMouseButtonDown(0))
        {
            if (isOnGround)
            {
                isJumping = true;

                sound.PlayJump();
            }
        }

        // Ubah animasi
        anim.SetBool("isOnGround", isOnGround);

        // Kalkulasi Score
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if (scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }

        // Game Over
        if (transform.position.y < fallPositionY)
        {
            GameOver();
        }
    }

    private void FixedUpdate() {
        // RayCast Ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit)
        {
            if (!isOnGround && rigidBody.velocity.y <= 0)
            {
                isOnGround = true;
            }
        }
        else
        {
            isOnGround = false;
        }

        // Kalkulasi Velocity Vector
        Vector2 velocityVector = rigidBody.velocity;

        if (isJumping)
        {
            velocityVector.y += jumpAccel;
            isJumping = false;
        }
        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);

        rigidBody.velocity = velocityVector;
    }

    private void GameOver()
    {
        // Set High Score
        score.FinishScoring();

        // Stop Camera Move
        gameCamera.enabled = false;

        // Tampilkan Game Over
        gameOverScreen.SetActive(true);

        // Disable
        this.enabled = false;
    }

    private void OnDrawGizmos() {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down*groundRaycastDistance), Color.white);
    }
}
