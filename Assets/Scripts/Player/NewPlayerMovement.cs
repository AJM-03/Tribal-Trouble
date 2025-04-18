using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float jumpHeight;
    public float jumpMoveSpeed;
    public float doubleJumpHeight;
    public float glideSpeed;
    public float rotationSpeed;
    public float slopeSlideSpeed;
    public float flipHeight;
    public float flipSpeed;
    public float instantFlipJumpBonus;
    public float slamSpeed;


    [Header("Physics")]
    public float gravityMultiplier;
    private float ySpeed;
    private float gravity;
    private Vector3 movementDirection;
    private Vector3 playerDirectionInput;
    private int jumpInput = 0;
    private int crouchInput = 0;
    private int sprintInput = 0;
    private float inputMagnitude;
    private float originalStepOffset;
    private int stepCount;
    public float jumpButtonGracePeriod;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    private float spinSpeed;
    private RaycastHit slopeHit;


    [Header("State")]
    private bool isJumping;
    private bool slam;
    private bool isGrounded;
    private bool isCrouching;
    private int superSprintTime;
    private bool doubleJump;


    [Header("Particles + Animation")]
    private float randomAnimTimer = 30;
    public ParticleSystem runParticles;
    public ParticleSystem walkParticles;
    public ParticleSystem jumpParticles;
    public ParticleSystem landingParticles;
    public ParticleSystem doubleJumpParticles;
    public ParticleSystem spinningParticles;
    public ParticleSystem stopSpinningParticles;
    public ParticleSystem fallingParticles;
    public ParticleSystem slamFallParticles;
    public ParticleSystem slamLandParticles;
    public ParticleSystem flipJumpParticles;
    public ParticleSystem instantFlipJumpParticles;
    public ParticleSystem sprintParticles;
    public ParticleSystem sprintDustParticles;
    public ParticleSystem sprintStartParticles;
    public ParticleSystem superSprintParticles;
    public ParticleSystem superSprintDustParticles;
    public ParticleSystem superSprintStartParticles;
    private Transform spinObject;


    [Header("Sounds")]
    public AudioClip jumpSound;
    public AudioClip spearSpinSound;
    public AudioClip tornadoSound;
    public AudioClip softLandingSound;
    public AudioClip hardLandingSound;
    public AudioClip[] softFootsteps;
    public AudioClip[] hardFootsteps;
    public AudioClip slamLandSound;
    public AudioClip sprintStartSound;
    public AudioClip superSprintStartSound;


    [Header("Components")]
    public Transform cameraTransform;
    private Animator animator;
    private CharacterController characterController;
    private TerrainDetector terrainDetector;
    public Transform groundChecker;
    public LayerMask groundLayer;
    private RaycastHit hit;




    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
        spinObject = gameObject.transform.GetChild(0).GetComponent<Transform>();
        terrainDetector = new TerrainDetector();
        stepCount = 0;
    }



    void Update()
    {
        movementDirection = playerDirectionInput;

        inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        if (crouchInput == 2 && isGrounded)
        {
            Crouch();
        }
        else if (crouchInput == 3 && isGrounded)
        {
            crouchInput = 0;
            animator.SetBool("IsCrouching", false);
            isCrouching = false;
        }
        else if (crouchInput == 0 && isGrounded && isCrouching)
            isCrouching = false;

        if (sprintInput == 1 && isGrounded && !isCrouching && movementDirection != Vector3.zero)
            Sprint();
        else if (sprintInput == 2 && isGrounded && !isCrouching && movementDirection != Vector3.zero)
            Sprint();
        else
        {
            superSprintTime = 0;
            sprintParticles.Stop();
            superSprintParticles.Stop();
        }

        animator.SetFloat("InputMagnitude", inputMagnitude, 0.05f, Time.deltaTime);

        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;
        movementDirection.Normalize();

        gravity = Physics.gravity.y * gravityMultiplier;

        ySpeed += gravity * Time.deltaTime;

        if (characterController.isGrounded && !OnSteepSlope())
        {
            lastGroundedTime = Time.time;
        }

        JumpInput();

        GroundedCheck();

        MovementCheck();

        if (isGrounded == false)
        {
            Vector3 velocity = new Vector3(0,0,0);
            if (isCrouching || slam)
                velocity = movementDirection * inputMagnitude * flipSpeed;
            else
                velocity = movementDirection * inputMagnitude * jumpMoveSpeed;
            velocity.y = ySpeed;

            if (slam)
            {
                velocity.x /= 1.5f;
                velocity.z /= 1.5f;
                velocity.y = slamSpeed;
            }

            characterController.Move(velocity * Time.deltaTime);

            if (ySpeed < -9)
                fallingParticles.Play();

            if (doubleJump && !slam)
            {
                spinObject.Rotate(0, spinSpeed * Time.deltaTime, 0);
                fallingParticles.Stop();

                if (spinSpeed % 25 == 0)  // If it is a multiple of 25
                    SoundManager.Instance.PlaySound(spearSpinSound, (((spinSpeed / 10000) + 0.65f) * 2) - 1);  // Play the sound

                if (spinSpeed > 850)
                {
                    spinSpeed = spinSpeed - 0.5f;

                    if (!spinningParticles.isPlaying)
                        spinningParticles.Play();
                }
                else
                {
                    doubleJump = false;
                    SetAnimatorBool("IsDoubleJumping", false);
                    SetAnimatorBool("IsFalling", true);
                    spinningParticles.Stop();
                    fallingParticles.Stop();
                    stopSpinningParticles.Play();
                }
            }
        }

        if (OnSteepSlope())
        {
            SteepSlopeMovement();
        }

        if (crouchInput == 1)
            crouchInput = 2;
        if (sprintInput == 1)
            sprintInput = 2;
    }
    public void JumpInput()
    {
        if (jumpInput == 1)
        {
            jumpInput = 2;
            jumpButtonPressedTime = Time.time;

            GroundedCheck();

            if (!isGrounded && animator.GetBool("IsFalling") && !doubleJump && !OnSteepSlope())
            {
                DoubleJump();
            }
        }

        else if (jumpInput == 2)
        {
            if (!isGrounded && animator.GetBool("IsFalling") && doubleJump)
            {
                ySpeed = glideSpeed;
                SetAnimatorBool("IsGliding", true);
            }
        }

        else if (jumpInput == 3)
        {
            jumpInput = 0;

            if (isJumping && ySpeed > 0)  // High and low jumping
            {
                gravity *= 2;  
            }

            if (doubleJump && animator.GetBool("IsFalling"))
                stopSpinningParticles.Play();
        }
    }



    private void Jump()
    {
        ySpeed = 0;
        ySpeed = Mathf.Sqrt(jumpHeight * -3 * gravity);
        SetAnimatorBool("IsJumping", true);
        jumpParticles.Play();
        isJumping = true;
        jumpButtonPressedTime = null;
        lastGroundedTime = null;
        if (randomAnimTimer < 10)
            randomAnimTimer = Random.Range(20, 30);
        SoundManager.Instance.PlaySound(jumpSound, 0.35f);
    }



    private void DoubleJump()
    {
        animator.SetBool("IsFalling", false);
        ySpeed = Mathf.Sqrt(doubleJumpHeight * -3 * gravity);
        SetAnimatorBool("IsDoubleJumping", true);
        doubleJumpParticles.Play();
        doubleJump = true;
        spinSpeed = 1500;
        SoundManager.Instance.PlaySound(tornadoSound, 0.45f);
    }



    private void FlipJump(float jumpBonus)
    {
        ySpeed = Mathf.Sqrt((flipHeight + jumpBonus) * -3 * gravity);
        SetAnimatorBool("IsJumping", true);
        SetAnimatorBool("IsGrounded", true);
        flipJumpParticles.Play();
        isJumping = true;
        isCrouching = true;
        jumpButtonPressedTime = null;
        lastGroundedTime = null;
        if (randomAnimTimer < 10)
            randomAnimTimer = Random.Range(20, 30);
    }



    private void Slam()
    {
        slam = true;
        animator.SetBool("Slam", true);
        slamFallParticles.Play();
        stopSpinningParticles.Play();
    }



    private void Crouch()
    {
        inputMagnitude /= 2;
        animator.SetBool("IsCrouching", true);
        isCrouching = true;
    }



    private void Sprint()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Slam") && !superSprintStartParticles.isPlaying)
        {
            inputMagnitude *= 3;
            superSprintTime = 250;
            superSprintStartParticles.Play();
            stopSpinningParticles.Play();
            SoundManager.Instance.PlaySound(superSprintStartSound, 0.8f);
        }
        else if (superSprintTime > 0)
        {
            inputMagnitude *= 3;
            superSprintTime--;
            superSprintParticles.Play();
        }
        else
        {
            inputMagnitude *= 2;
            superSprintTime = 0;
            superSprintParticles.Stop();
            sprintParticles.Play();
        }

        if (sprintInput == 1 && superSprintTime == 0)
        {
            sprintStartParticles.Play();
            stopSpinningParticles.Play();
            SoundManager.Instance.PlaySound(sprintStartSound, 0.8f);
        }
    }



    private void GroundedCheck()
    {
        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)  // If grounded
        {
            if (!isGrounded && animator.GetBool("IsFalling"))  // Just landed
            {
                landingParticles.Play();
                spinningParticles.Stop();
                fallingParticles.Stop();
                slamFallParticles.Stop();

                if (slam)
                {
                    slamLandParticles.Play();
                    SoundManager.Instance.PlaySound(slamLandSound, 0.85f);
                }

                else
                {
                    if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f) && hit.transform.tag == "Terrain")
                    {
                        SoundManager.Instance.PlaySound(GetRandomClip(false), 1);  // Landed on grass or rock
                    }
                    else
                        SoundManager.Instance.PlaySound(hardLandingSound, 1);  // Land on an object
                }
            }

            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;
            SetAnimatorBool("IsGrounded", true);
            isGrounded = true;
            SetAnimatorBool("IsJumping", false);
            isJumping = false;
            SetAnimatorBool("IsFalling", false);
            doubleJump = false;
            SetAnimatorBool("IsDoubleJumping", false);
            spinSpeed = 1500;
            SetAnimatorBool("Slam", false);
            slam = false;

            spinObject.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod && !OnSteepSlope())
            {
                if (isCrouching && movementDirection == Vector3.zero)
                    FlipJump(0);
                else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Slam"))
                    Jump();
                else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Slam"))
                {
                    FlipJump(instantFlipJumpBonus);
                    instantFlipJumpParticles.Play();
                }
            }
        }
        else  // If not grounded
        {
            characterController.stepOffset = 0;
            SetAnimatorBool("IsGrounded", false);
            isGrounded = false;

            if ((isJumping && ySpeed < 0) || ySpeed < -2)
            {
                SetAnimatorBool("IsJumping", false);

                if (jumpInput == 0 && doubleJump)
                {
                    SetAnimatorBool("IsGliding", false);
                    SetAnimatorBool("IsDoubleJumping", false);
                    spinObject.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    spinningParticles.Stop();
                }
                else if (jumpInput == 2 && doubleJump)
                {
                    SetAnimatorBool("IsGliding", true);
                    SetAnimatorBool("IsDoubleJumping", true);
                }
                else
                {
                    SetAnimatorBool("IsDoubleJumping", false);
                    spinningParticles.Stop();
                }

                if (!Physics.Raycast(groundChecker.position, Vector3.down, 1f, groundLayer, QueryTriggerInteraction.Ignore))
                    SetAnimatorBool("IsFalling", true);

                if (crouchInput == 1 && !slam && animator.GetBool("IsFalling"))
                    Slam();
            }
        }
    }



    private void MovementCheck()
    {
        if (movementDirection != Vector3.zero)  // If moving
        {
            SetAnimatorBool("IsRunning", true);
            Step();

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Slam"))
            {
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }

            if (randomAnimTimer < 10)
                randomAnimTimer = Random.Range(20, 30);

            if (OnSlope())  // --- Smooth down slopes
                ySpeed = -1000 * Time.deltaTime;
        }
        else  // If still
        {
            SetAnimatorBool("IsRunning", false);
            randomAnim();
        }
    }



    private bool OnSlope()
    {
        if (isJumping)
            return false;

        RaycastHit hit;

        if (Physics.Raycast(groundChecker.position, Vector3.down, out hit, 0.75f))
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        return false;
    }



    private bool OnSteepSlope()
    {
        if (isJumping)
            return false;

        if (Physics.Raycast(groundChecker.position, Vector3.down, out slopeHit, 2f))
        {
            float slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
            if (slopeAngle > characterController.slopeLimit) 
            {
                return true;
            }
        }
        return false;
    }



    private void SteepSlopeMovement()
    {
        Vector3 slopeDirection = Vector3.up - slopeHit.normal * Vector3.Dot(Vector3.up, slopeHit.normal);
        float slideSpeed = slopeSlideSpeed + Time.deltaTime;

        movementDirection = slopeDirection * -slideSpeed;
        movementDirection.y = movementDirection.y - slopeHit.point.y;

        characterController.Move(movementDirection * Time.deltaTime);

        spinningParticles.Stop();
        fallingParticles.Stop();
    }



    private void randomAnim()
    {
        int randomAnim = 0;


        if (randomAnimTimer <= 0)
        {
            randomAnim = Random.Range(-1, 6);
            randomAnimTimer = Random.Range(15, 25);
        }
        else
            randomAnimTimer -= Time.deltaTime;


        if (randomAnim != 0)
        {
            if (randomAnim == 1)
                animator.SetTrigger("HappyIdle");
            else if (randomAnim == 2)
                animator.SetTrigger("WaveIdle");
            else if (randomAnim == 3)
                animator.SetTrigger("AngryIdle");
            else if (randomAnim == 4)
                animator.SetTrigger("PointIdle");
            else if (randomAnim == 5)
                animator.SetTrigger("DanceIdle");

            randomAnim = 0;
        }
    }



    public void OnMovement(InputAction.CallbackContext value)  // Movement input
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        playerDirectionInput = new Vector3(inputMovement.x, 0, inputMovement.y);
    }



    public void CrouchButtonPressed(InputAction.CallbackContext value)  // Crouch input
    {
        if (value.performed && crouchInput != 2)
        {
            crouchInput = 1;
        }

        else if (value.canceled)
            crouchInput = 3;
    }



    public void SprintButtonPressed(InputAction.CallbackContext value)  // Sprint input
    {

        if (value.performed && sprintInput != 2)
        {
            sprintInput = 1;
        }

        else if (value.canceled)
            sprintInput = 0;
    }



    public void JumpButtonPressed(InputAction.CallbackContext value)  // Jump input
    {
        if (value.performed && jumpInput != 2)
            jumpInput = 1;

        else if (value.canceled)
            jumpInput = 3;
    }



    private void Step()
    {
        stepCount++;

        if (isGrounded && inputMagnitude > 2.5f)
        {
            superSprintDustParticles.Play();
            if (stepCount % 30 == 0)
                SoundManager.Instance.PlaySound(GetRandomClip(true), 0.8f);
        }


        else if (isGrounded && inputMagnitude > 1.5f)
        {
            sprintDustParticles.Play();
            if (stepCount % 50 == 0)
                SoundManager.Instance.PlaySound(GetRandomClip(true), 0.7f);
        }


        else if (isGrounded && inputMagnitude > 0.6f)
        {
            runParticles.Play();
            if (stepCount % 80 == 0)
                SoundManager.Instance.PlaySound(GetRandomClip(true), 0.6f);
        }


        else if (isGrounded && inputMagnitude > 0.4f)
        {
            walkParticles.Play();
            if (stepCount % 130 == 0)
                SoundManager.Instance.PlaySound(GetRandomClip(true), 0.5f);
        }
    }



    private AudioClip GetRandomClip(bool step)
    {
        int terrainTextureIndex = terrainDetector.GetActiveTerrainTextureIdx(transform.position);

        if (step)
        {
            switch (terrainTextureIndex)
            {
                case 0:  // Rock
                    return hardFootsteps[Random.Range(0, hardFootsteps.Length)];
                case 1:  // Grass
                    return softFootsteps[Random.Range(0, softFootsteps.Length)];
                case 2:  // Dirt
                    return hardFootsteps[Random.Range(0, hardFootsteps.Length)];
                case 3:  // Dirt 2
                    return hardFootsteps[Random.Range(0, hardFootsteps.Length)];
                default:  // Else
                    return hardFootsteps[Random.Range(0, hardFootsteps.Length)];
            }
        }
        else
        {
            switch (terrainTextureIndex)
            {
                case 0:  // Rock
                    return hardLandingSound;
                case 1:  // Grass
                    return softLandingSound;
                case 2:  // Dirt
                    return hardLandingSound;
                case 3:  // Dirt 2
                    return hardLandingSound;
                default:  // Else
                    return hardLandingSound;
            }
        }
    }



    private void OnAnimatorMove()  // Root motion movement
    {
        if (isGrounded)
        {
            Vector3 velocity = animator.deltaPosition;
            velocity.y = ySpeed * Time.deltaTime;

            characterController.Move(velocity);
        }
    }



    private void SetAnimatorBool(string name, bool on)  // Set animation boolean
    {
        animator.SetBool(name, on);
    }
}
