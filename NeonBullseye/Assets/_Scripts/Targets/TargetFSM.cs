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

    public enum MovementType { Static, SineWave, Swing }

    [Header("Target Movement Settings")]
    [SerializeField] private MovementType movementType;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveHeight = 1.5f;
    [SerializeField] private Rigidbody2D swingAnchor;
    private HingeJoint2D hinge; //For swing movement
    private Vector3 startPosition;
    private float randomOffset;


    private void Start()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, 6.28f);

        if (movementType == MovementType.Swing)
        {
            SetupSwing();
        }    
    }

    private void Update()
    {
        switch (movementType)
        {
            case MovementType.SineWave:
                float newY = startPosition.y + Mathf.Sin(Time.time * moveSpeed + randomOffset) * moveHeight;
                transform.position = new Vector3(startPosition.x, newY, startPosition.z);
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

        if (hinge != null)
        {
            JointMotor2D motor = hinge.motor;
            motor.motorSpeed = swingSpeed;
            hinge.motor = motor;
        }
    }

    private void SetupSwing()
    {
        hinge = gameObject.AddComponent<HingeJoint2D>();
        
        if (swingAnchor != null)
        {
            hinge.connectedBody = swingAnchor;
            hinge.anchor = Vector2.zero; // Set anchor to the center of the target
            hinge.connectedAnchor = Vector2.zero; 
        }
        else
        {
            Debug.LogWarning("Swing anchor not assigned to " + gameObject.name);
        }

        hinge.useMotor = true;
        JointMotor2D motor = new JointMotor2D
        {
            motorSpeed = Random.Range(30f, 60f),
            maxMotorTorque = 1000f
        };
        hinge.motor = motor;
    }
}
