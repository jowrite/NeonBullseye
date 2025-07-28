using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TargetFSM : MonoBehaviour
{
    public enum  TargetState { Idle, Active, Hit, Pattern }

    [Header("VFX")]
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private Color patternColor = Color.blue;
    //[SerializeField] private ParticleSystem hitParticles;
    //[SerializeField] private ParticleSystem patternParticles;
    [SerializeField] private float hitFlashDuration = 0.2f;
    [SerializeField] private float hitBrightness = 2.5f;
    [SerializeField] private float patternOutlineWidth = 3f;
    [SerializeField] private Color patternOutlineColor = Color.cyan;

    [Header("SFX")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip patternSound;
    [SerializeField] private AudioClip resetSound;

    private TargetState currentState;
    public bool IsHit => currentState == TargetState.Hit;
    private SpriteRenderer spriteRenderer;
    private Material targetMaterial; //For shader effects
    private Color defaultColor;

    private int baseScore = 100; // Base score for hitting a target
    private int patternBonus = 500; // Bonus for hitting a pattern target

    #region Movement Parameters

    public enum MovementType { Static, SineWave, Swing }

    [Header("Target Movement Settings")]
    [SerializeField] private MovementType movementType;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveHeight = 1.5f;

    //New swing movement settings
    [SerializeField] private float swingAmplitude = 45f; //Max degrees left/right
    [SerializeField] private float swingSpeed = 2f; //Adjusts how fast the target swings (tweak in inspector prn)
    [SerializeField] private float pendulumLength = 2f;
    [SerializeField] private float thetaAngle = 30f; //Initial angle in degrees
    private float g = 9.81f; //Gravity
    private float omega;

    private Vector3 startPosition;
    private float randomOffset;

    #endregion

    #region Setup and Initialization
    private void Start()
    {
        startPosition = transform.position;
        //randomOffset = Random.Range(0f, 6.28f); unsure if even need this, but keeping for now
        omega = Mathf.Sqrt(g / pendulumLength); //Calculate angular frequency for swing

    }

    private void Update()
    {
        switch (movementType)
        {
            case MovementType.SineWave:
                // Sine wave movement
                float newY = startPosition.y + Mathf.Sin(Time.time * moveSpeed + randomOffset) * moveHeight;
                transform.position = new Vector3(startPosition.x, newY, startPosition.z);
                break;

            case MovementType.Static:
                // Do nothing, target remains static
                break;

            case MovementType.Swing: //Simulates a pendulum swing
                float thetaRad = thetaAngle * Mathf.Deg2Rad; //Convert angle to radians
                float angle = thetaRad * Mathf.Cos(omega * Time.deltaTime); //Calculate angle based on pendulum motion
                transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg); //Apply rotation
                break;
        }
        
    }


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetMaterial = spriteRenderer.material;
        defaultColor = spriteRenderer.color;
        SetState(TargetState.Idle);

        //Default shader settings
        targetMaterial.SetFloat("_OutlineEnabled", 0f);
        targetMaterial.SetFloat("_Brightness", 1f);
        targetMaterial.SetFloat("_HueShift", 0f);
    }

    private void SetState(TargetState newState)
    {
        currentState = newState;

        switch (currentState)
        { 
            case TargetState.Idle:
                spriteRenderer.color = defaultColor;
                targetMaterial.SetFloat("_HitFlash", 0f);
                break;
            case TargetState.Active:
                spriteRenderer.color = activeColor;
                targetMaterial.SetFloat("_HitFlash", 0f);
                break;
            case TargetState.Hit:
                spriteRenderer.color = hitColor;
                targetMaterial.SetFloat("_HitFlash", 1f);
                //hitParticles.Play();
                //AudioSource.PlayClipAtPoint(hitSound, transform.position);
                Invoke(nameof(ResetTarget), hitFlashDuration);
                break;
            case TargetState.Pattern:
                spriteRenderer.color = patternColor;
                //patternParticles.Play();
                //AudioSource.PlayClipAtPoint(patternSound, transform.position);
                break;
        }
    }

    #endregion

    #region Target Interaction Methods
    public void HandleHit()
    {
        if (currentState == TargetState.Pattern)
        {
            PlayPatternEffects();
            GameManager.gm.HandlePatternHit(this); //Notify GameManager of pattern hit
            GameManager.gm.ShowScorePopup(patternBonus, transform.position + Vector3.up * 0.5f); //visual score popup
            SetState(TargetState.Hit);
        }
        else if (currentState == TargetState.Active)
        {
            PlayNormalHitEffect();
            GameManager.gm.AddScore(baseScore); //Add base score
            GameManager.gm.ShowScorePopup(baseScore, transform.position + Vector3.up * 0.5f); //visual score popup
            SetState(TargetState.Hit);
        }

        //Schedule reset after delay
        Invoke("ResetTarget", 2f);
    }

    private void ResetTarget()
    {
        SetState(TargetState.Idle);
    }

    public void ResetToIdle()
    {
        CancelInvoke(nameof(ResetTarget));
        SetState(TargetState.Idle);
    }

    //PatternManager will call this method to set the target to Pattern state
    public void SetAsPatternTarget()
    {
        // Thick cyan outline
        targetMaterial.SetFloat("_OutlineEnabled", 1f);
        targetMaterial.SetFloat("_OutlineWidth", patternOutlineWidth);
        targetMaterial.SetColor("_OutlineColor", patternOutlineColor);
        // Subtle pulsing effect
        targetMaterial.SetFloat("_PulseSpeed", 1.5f);

        SetState(TargetState.Pattern);
    }

    private void PlayNormalHitEffect()
    {
        //Brightness pulse
        targetMaterial.SetFloat("_Brightness", hitBrightness);
        //Random hue shift
        targetMaterial.SetFloat("_HueShift", Random.Range(0.1f, 0.3f));

        //Reset via coroutine
        StartCoroutine(ResetShaderProperties(0.2f));
    }

    private void PlayPatternEffects()
    {
        //SFX
        AudioSource.PlayClipAtPoint(patternSound, transform.position, 0.7f);
        //VFX
        //patternParticles.Play();
        //Shader effects
        targetMaterial.SetFloat("_OutlineEnabled", 1f);
        targetMaterial.SetColor("_OutlineColor", patternOutlineColor);
    }

    private IEnumerator ResetShaderProperties(float delay)
    {
        yield return new WaitForSeconds(delay);

        //Return to default
        float elapsed = 0;
        float duration = 0.5f;
        float startBrightness = targetMaterial.GetFloat("_Brightness");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            targetMaterial.SetFloat("_Brightness", Mathf.Lerp(startBrightness, 1f, t));
            yield return null;
        }

        targetMaterial.SetFloat("_HueShift", 0f);
    }

    public void SetMovementParams(MovementType type, float swingSpeed, float moveHeight)
    {
        this.movementType = type;
        this.moveHeight = moveHeight;
    }

    #endregion

}
