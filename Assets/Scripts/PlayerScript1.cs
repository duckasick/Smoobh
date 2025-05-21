
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerScript1 : MonoBehaviour
{


    // Input scheiden van FixedUpdate

    float d;

    [SerializeField] private GameObject playerObject;

    [Header("Grounded")]
    [SerializeField] private float iniAccelForce;
    [SerializeField] private float iniSpeed;
    [SerializeField] private float truAccelForce;
    [SerializeField] private float groundFriction;
    [SerializeField] private float groundSpeed;
    float maxSpeed;

    [Header("Jump")]
    [SerializeField] private GameObject fastObject;
    [SerializeField] private float normalGravity;
    [SerializeField] private float hangGravity;
    [SerializeField] private float fastGravity;
    float currentGravity;
    bool hasJumped;
    bool jumpBuffered;
    bool jumpCut;

    bool grounded;

    float coyoteTimeCounter;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float bufferTimer;
    float bufferTimerCounter;

    [SerializeField] private float jumpForce;
    [SerializeField] private float hangVelocityUp;
    [SerializeField] private float hangVelocityDown;
    [SerializeField] private LayerMask groundLayer;

    bool justLanded;

    Vector3 landingVelocity;
    [SerializeField] private float landingVelocityThreshold;

    [Header("Glider")]
    [SerializeField] private float glidingAccel;
    [SerializeField] private float glidingSpeed;
    [SerializeField] private float glidingDownSpeed;
    [SerializeField] private float glidingGravity;
    [SerializeField] private GameObject glidingWings;
    [SerializeField] private ParticleSystem glidingParticles;
    [SerializeField] private AudioSource glidingAudio;
    bool gliding;

    [Header("Crosshair")]
    [SerializeField] private Transform crosshair;

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    bool dashCan;
    bool dashReset;

    [Header("Wallslide")]
    [SerializeField] private float wallslideMaxHold;
    [SerializeField] private float wallslideGravity;
    float wallslideHoldTimer;
    bool wallsliding;

    [SerializeField] private float wallslideRaycastLength;
    [SerializeField] private Vector3 wallslideRaycastOffset;

    [Header("CornerCorrection")]
    [SerializeField] private LayerMask cornerLayers;
    [SerializeField] private float cornerRaycastLength;
    [SerializeField] private Vector3 innerRaycastOffset;
    [SerializeField] private Vector3 edgeRaycastOffset;
    bool canCorrect;

    [Header("Camera effects")]
    [SerializeField] private Vector2 camMovementFactor;
    [SerializeField] private float yUpperLimit;

    [SerializeField] private VolumeScript volumeScript;
    [SerializeField] private float minVignette;
    [SerializeField] private float maxVignette;
    [SerializeField] private float maxCamshake;
    [SerializeField] private CinemachineBasicMultiChannelPerlin camshake;

    [SerializeField] private GameObject cubeLook;

    [Header("Particles")]
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private ParticleSystem psBase;
    [SerializeField] private GameObject psObject;
    [SerializeField] private GameObject psCarrier;

    [SerializeField] private float maxLineSize;
    [SerializeField] private TrailRenderer trailRendererGround;
    [SerializeField] private TrailRenderer trailRendererTop, trailRendererMiddle, trailRendererBottom;

    [SerializeField] private ParticleSystem landingHeavyParticles;
    [SerializeField] private ParticleSystem landingLightParticles;

    [SerializeField] private ParticleSystem wing1Particles;
    [SerializeField] private ParticleSystem wing2Particles;

    [SerializeField] private ParticleSystem fastFallParticles;

    [Header("Sounds")]
    [SerializeField] private AudioSource jump;

    [SerializeField] private AudioSource windNoise;
    [SerializeField] private float windMinPitch;
    [SerializeField] private float windMaxPitch;
    [SerializeField] private float windVolumeMax;

    [SerializeField] private AudioClip[] flapClips;
    [SerializeField] private AudioSource flapPlayer;
    private AudioClip flapPicked;
    private bool canPlayFlap;

    [SerializeField] private AudioSource landingLightAudio;
    [SerializeField] private AudioSource landingHeavyAudio;

    [SerializeField] private AudioSource Death;
    

    [Header("Objects")]
    [SerializeField] private Vector2 playerVelocityVector;
    [SerializeField] private Transform camTracker;
    [SerializeField] private Vector3 camOffset;

    [Header("Debug")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private float rayLength;
    [SerializeField] private bool camMove;

    bool startKillTimer = false;
    float killTimer = 7f;
    bool killed = false;

    [SerializeField] private GameObject fin;


    Rigidbody rb;

    float ySpeedMax;
    float ySpeedFastFalling;
    float ySpeedDefault;
    float yJumpCutSlow;



    void Start()
    {
        glidingParticles.Play();
        rb = GetComponent<Rigidbody>();
        maxSpeed = groundSpeed;
        currentGravity = normalGravity;
        ps.Play();
        psBase.Play();




        glidingParticles.Stop();
        canPlayFlap = true;
        glidingAudio.volume = 0f;
    }

    void Update()
    {
        d = Time.deltaTime;

        /*
        if (grounded && !gliding) { maxSpeed = groundSpeed; }
        else if (grounded) {
            if (rb.linearVelocity.x > groundSpeed || rb.linearVelocity.x < -groundSpeed) { maxSpeed = landingVelocity.x; }
            else
            { maxSpeed = groundSpeed; }
        }
        else if (gliding) { maxSpeed = glidingSpeed; }
        */

        /// TODO: State machine?
        grounded = Physics.Raycast(transform.position + offset, Vector3.down, rayLength, groundLayer) || Physics.Raycast(transform.position - offset, Vector3.down, rayLength, groundLayer);
        if (gliding) { maxSpeed = glidingSpeed; }
        if (!gliding) { maxSpeed = groundSpeed; }

        if (!killed)
        {
            Gliding();
        }
        xMovement();
        yMovement();
        if (camMove) { CamMovement(); }
        CornerCorrectChecker();
        //Attack();
        //Wallslide();
        //Dash();

        //if (canCorrect) CornerCorrect();

        // Bad Z axis >:(
        //transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        //print(maxSpeed);


        float currentCamShake = map(rb.linearVelocity.magnitude, 0, groundSpeed, 0, maxCamshake);
        if (grounded) { camshake.AmplitudeGain = currentCamShake; }
        if (!grounded) { camshake.AmplitudeGain = 0f; }
        //print(currentCamShake);


        PostProccsing();

        //Reload scene
        if (Input.GetKeyDown(KeyCode.F))
        {
            string currentSceneName = SceneManager.GetActiveScene().name; SceneManager.LoadScene(currentSceneName);
        }

        if (startKillTimer == true) { killTimer -= Time.deltaTime; if (killTimer < 0) { fin.SetActive(true); Death.Play(); Destroy(this.gameObject); } }

    }

    //Debug Gizmos
    private void OnDrawGizmos()
    {
        if (grounded) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(transform.position + offset, transform.position + offset + Vector3.down * rayLength);
        Gizmos.DrawLine(transform.position - offset, transform.position - offset + Vector3.down * rayLength);

        //Corner ups
        Gizmos.DrawLine(transform.position + edgeRaycastOffset, transform.position + edgeRaycastOffset + Vector3.up * cornerRaycastLength);
        Gizmos.DrawLine(transform.position - edgeRaycastOffset, transform.position - edgeRaycastOffset + Vector3.up * cornerRaycastLength);
        Gizmos.DrawLine(transform.position + innerRaycastOffset, transform.position + innerRaycastOffset + Vector3.up * cornerRaycastLength);
        Gizmos.DrawLine(transform.position - innerRaycastOffset, transform.position - innerRaycastOffset + Vector3.up * cornerRaycastLength);

        //wallslide
        Gizmos.DrawLine(this.transform.position - wallslideRaycastOffset, transform.position + wallslideRaycastOffset + Vector3.left * wallslideRaycastLength);
        Gizmos.DrawLine(this.transform.position + wallslideRaycastOffset, transform.position + wallslideRaycastOffset + Vector3.right * wallslideRaycastLength);

    }

    //Camera look ahead
    void CamMovement()
    {
        // Pos + lock Z
        float clampY = Mathf.Clamp(rb.linearVelocity.y, -30, 30);
        camTracker.localPosition = Vector3.Scale(new Vector2(rb.linearVelocity.x, clampY), new Vector3(camMovementFactor.x, camMovementFactor.y, 0f));
        // Limit Y
        if (camTracker.localPosition.y > 0) { camTracker.localPosition = new Vector3(camTracker.localPosition.x, Mathf.Min(camTracker.localPosition.y, yUpperLimit), 0); }
        camTracker.localPosition += camOffset;
    }

    void yMovement()
    { 
        // CoyoteTime
        if (grounded) { coyoteTimeCounter = coyoteTime / 1000; hasJumped = false; jumpCut = false; }
        else { coyoteTimeCounter -= Time.deltaTime; }

        //Gravity
        rb.AddForce(new Vector3(0, currentGravity * d, 0), ForceMode.Acceleration);

        // Jump
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            bufferTimerCounter = bufferTimer / 1000;
            jumpBuffered = true;
        }
        bufferTimerCounter -= Time.deltaTime;

        if (coyoteTimeCounter > 0 && hasJumped == false && bufferTimerCounter > 0 && jumpBuffered && wallsliding == false && !gliding)
        {
            rb.AddRelativeForce(new Vector2(0, jumpForce), ForceMode.VelocityChange);
            jumpBuffered = false;
            hasJumped = true;
            jump.Play();
        }
        
        // Hang time
        if (!grounded && rb.linearVelocity.y > hangVelocityDown && rb.linearVelocity.y < hangVelocityUp)
        {
            currentGravity = hangGravity;
        }
        else { currentGravity = normalGravity; }

        //Jump Cut
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) && jumpCut == false && rb.linearVelocity.y > 0 && !gliding)
        {
            jumpCut = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0);
            currentGravity = normalGravity;
        }


        // Fastfalling
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentGravity = fastGravity;
            ySpeedMax = ySpeedFastFalling;

            // Play particles and set player object to fast falling object. 
            playerObject.SetActive(false);
            fastObject.SetActive(true);
            fastFallParticles.Play();
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            currentGravity = normalGravity;
            ySpeedMax = ySpeedDefault;

            // Play particles and set player object to default
            playerObject.SetActive(true);
            fastObject.SetActive(false);
            fastFallParticles.Stop();
        }


        //Just landed
        if (grounded && justLanded == false)
        {
            justLanded = true;
        }
        if (!grounded) { justLanded = false; }

        //print(currentGravity);
    }

    void xMovement()
    {
        // Reset Velocity
        //if (Input.GetKeyDown(KeyCode.A) && rb.linearVelocity.x > 0) { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); }
        //if (Input.GetKeyDown(KeyCode.D) && rb.linearVelocity.x < 0) { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); }

        //Right
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (rb.linearVelocity.x < iniSpeed)
            {
                rb.AddRelativeForce(new Vector3(iniAccelForce * d, 0, 0));
            }
            else
            {
                rb.AddRelativeForce(new Vector3(truAccelForce * d, 0, 0));
            }
        }
        //Left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (rb.linearVelocity.x > -iniSpeed)
            {
                rb.AddRelativeForce(new Vector3(-iniAccelForce * d, 0, 0));
            }
            else
            {
                rb.AddRelativeForce(new Vector3(-truAccelForce * d, 0, 0));
            }
        }

        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            //Stop player if too slow
            if (rb.linearVelocity.x > -1 && rb.linearVelocity.x < 1)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            else
            {
                if (rb.linearVelocity.x < 0)
                {
                    rb.AddRelativeForce(new Vector3(groundFriction * d,0,0));
                }
                if (rb.linearVelocity.x > 0)
                {
                    rb.AddRelativeForce(new Vector3(-groundFriction * d, 0, 0));
                }
            }
        }

        //Velocity cap;
        if (rb.linearVelocity.x < -maxSpeed) { rb.linearVelocity = new Vector2(-maxSpeed, rb.linearVelocity.y); }
        if (rb.linearVelocity.x > maxSpeed) { rb.linearVelocity = new Vector2(maxSpeed, rb.linearVelocity.y); }

        //Wind noise;
        float windPitch = map(rb.linearVelocity.magnitude, 0, maxSpeed+10, windMinPitch, windMaxPitch);
        float windVolume = map(rb.linearVelocity.magnitude, 0, maxSpeed + 10, 0, windVolumeMax);
        windNoise.pitch = windPitch;
        windNoise.volume = windVolume;
    }

    void Gliding()
    {
        //Check if can glide
        //if (!grounded && rb.linearVelocity.y <= 0) { peaked = true; }
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftAlt)) && !grounded)
        {
            if (rb.linearVelocity.y < glidingDownSpeed) {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, glidingDownSpeed, 0); }
            glidingWings.SetActive(true);
            playerObject.SetActive(false);
            currentGravity = glidingGravity;
            //Play audio and particles
            if (canPlayFlap)
            {
                int index = Random.Range(0, flapClips.Length);
                flapPicked = flapClips[index];
                glidingAudio.volume = 0.576f;
                glidingParticles.Play();
                flapPlayer.clip = flapPicked;
                flapPlayer.Play();
                canPlayFlap = false;
                wing1Particles.Play();
                wing2Particles.Play();
            }
            gliding = true;
        }


        //Stop audio & particles
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftAlt)) { 
            canPlayFlap = true; 
            glidingWings.SetActive(false); 
            gliding = false; playerObject.SetActive(true);
            glidingAudio.volume = 0;
            glidingParticles.Stop();
            wing1Particles.Stop();
            wing2Particles.Stop();
            gliding = false;
        }
    }

    void Attack()
    {
        //print(transform.position);
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //attack
            //Shake cam
            // Check target (bullet, window)
        }
    }

    void Wallslide()
    {
        if (Input.GetKey(KeyCode.A))
        {
            if (Physics.Raycast(this.transform.position - wallslideRaycastOffset, transform.position + wallslideRaycastOffset + Vector3.left * wallslideRaycastLength, cornerLayers))
            {
                wallslideHoldTimer -= Time.deltaTime;
                if (wallslideHoldTimer < 0)
                {
                    currentGravity = wallslideGravity;
                }
                else { rb.linearVelocity = Vector3.zero; }
                wallsliding = true;
            }

        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (Physics.Raycast(this.transform.position + wallslideRaycastOffset, transform.position + wallslideRaycastOffset + Vector3.right * wallslideRaycastLength, cornerLayers))
            {
                wallslideHoldTimer -= Time.deltaTime;
                if (wallslideHoldTimer < 0)
                {
                    currentGravity = wallslideGravity;
                }
                else { rb.linearVelocity = Vector3.zero; }
                wallsliding = true;
            }
        }
        else { currentGravity = normalGravity; wallsliding = false; }
        if (Input.GetKeyDown(KeyCode.Space) && wallsliding == true)
        {
            rb.AddRelativeForce(new Vector2(0, jumpForce), ForceMode.VelocityChange);
            jumpBuffered = false;
            hasJumped = true;
            jump.Play();
        }
        if (grounded) { wallslideHoldTimer = wallslideMaxHold; }

        // Check for wall
        // if input is to wall && !down,
        //      hold for x seconds
        //      else slide down
        //

        // If grounded 
        // Can wallslideHoldTimer = maxSlideHoldTimer
    }

    void Dash()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))// && dashCan && dashReset)
        {
            //Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, -4f));
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z-10));
            Vector3 direction = mouseWorldPos - new Vector3(transform.position.x, transform.position.y, 0);
            print(mouseWorldPos);
            print(direction);

            rb.AddForce(direction.normalized * 60 * -1, ForceMode.Impulse);

            // If !dashed


            // state = dashing

            // Get mouse pos
            // set velocity to 0
            // Set velocity / impulse / wtf to mouse pos
            // slideHoldTimer = maxSlideHoldTimer
            // If doneDashing
            // State = normal
        }

        if (grounded) { dashReset = true; }
        //if (!grounded) { dashCan = true; }
        //else { dashCan = false; }
    }

    public void Kill()
    {
        //State = killed
        startKillTimer = true;
        killed = true;
    }

    void CornerCorrectChecker()
    {
        canCorrect = Physics.Raycast(transform.position + edgeRaycastOffset, Vector3.up, cornerRaycastLength, cornerLayers) &&
                            !Physics.Raycast(transform.position + innerRaycastOffset, Vector3.up, cornerRaycastLength, cornerLayers) ||
                            Physics.Raycast(transform.position - edgeRaycastOffset, Vector3.up, cornerRaycastLength, cornerLayers) &&
                            !Physics.Raycast(transform.position - innerRaycastOffset, Vector3.up, cornerRaycastLength, cornerLayers);
    }

    void CornerCorrect()
    {
        RaycastHit hit;
        //Push player to the right
        if (Physics.Raycast(transform.position - innerRaycastOffset + Vector3.up * cornerRaycastLength, Vector3.left, out hit, cornerRaycastLength, cornerLayers))
        {
            float _newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * cornerRaycastLength,
                transform.position - innerRaycastOffset + Vector3.up * cornerRaycastLength);
            transform.position = new Vector3(transform.position.x + _newPos, transform.position.y, transform.position.z);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
        }
        else
        {
            //Push player to the left
            if (Physics.Raycast(transform.position + innerRaycastOffset + Vector3.up * cornerRaycastLength, Vector3.right, out hit, cornerRaycastLength, cornerLayers))
            {
                float _newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * cornerRaycastLength,
                    transform.position + innerRaycastOffset + Vector3.up * cornerRaycastLength);
                transform.position = new Vector3(transform.position.x - _newPos, transform.position.y, transform.position.z);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
            }
        }
    }

    void PostProccsing()
    {
        // Post processing
        float vignetteAmount = map(rb.linearVelocity.magnitude, 0, groundSpeed, 0, maxVignette);
        if (vignetteAmount < minVignette) { vignetteAmount = minVignette; }
        if (vignetteAmount > maxVignette) { vignetteAmount = maxVignette; }
        volumeScript.vignetteIntensity = vignetteAmount;

        //Speedlines
        //cubeLook.transform.localPosition = rb.linearVelocity;
        //psCarrier.transform.RotateTowards(cubeLook.transform.localPosition);
        float speedlinesSize = map(rb.linearVelocity.magnitude, 0, groundSpeed, 0, maxLineSize);
        if (speedlinesSize < 0) { speedlinesSize = 0; }
        if (speedlinesSize > maxLineSize) { speedlinesSize = maxLineSize; }
        psObject.transform.localScale = new Vector3(psObject.transform.localScale.x, psObject.transform.localScale.y, speedlinesSize);

        if (!grounded)
        {
            trailRendererMiddle.emitting = true; trailRendererGround.emitting = false;
            if (gliding) { trailRendererTop.emitting = false; trailRendererBottom.emitting = false; }
            else { trailRendererTop.emitting = true; trailRendererBottom.emitting = true; }
        }
        else if (!gliding) { trailRendererTop.emitting = false; trailRendererMiddle.emitting = false; trailRendererBottom.emitting = false; trailRendererGround.emitting = true; }
    }
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        landingVelocity = new Vector3(Mathf.Abs(collision.relativeVelocity.x), Mathf.Abs(collision.relativeVelocity.y), Mathf.Abs(collision.relativeVelocity.z));
        print(landingVelocity);
        if (landingVelocity.y > landingVelocityThreshold)
        {
            landingHeavyAudio.Play();
            landingHeavyParticles.Play();
            //Camshake
        }
        else
        {
            landingLightAudio.Play();
            landingLightParticles.Play();
        }
    }

    public void Reset(Transform resetPos)
    {
        transform.position = resetPos.position;
        rb.linearVelocity = Vector3.zero;
        Death.Play();
    }
}

