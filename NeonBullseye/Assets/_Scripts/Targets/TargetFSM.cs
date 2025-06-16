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
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem patternParticles;
    [SerializeField] private float hitFlashDuration = 0.2f;
    [SerializeField] private float hitBrightness = 2.5f;
    [SerializeField] private float patternOutlineWidth = 3f;
    [SerializeField] private Color patternOutlineColor = Color.cyan;

    [Header("SFX")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip patternSound;
    [SerializeField] private AudioClip resetSound;

    private TargetState currentState;
    private SpriteRenderer spriteRenderer;
    private Material targetMaterial; //For shader effects
    private Color defaultColor;

    private int baseScore = 100; // Base score for hitting a target


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
                hitParticles.Play();
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
                Invoke(nameof(ResetTarget), hitFlashDuration);
                break;
            case TargetState.Pattern:
                spriteRenderer.color = patternColor;
                patternParticles.Play();
                AudioSource.PlayClipAtPoint(patternSound, transform.position);
                break;
        }
    }

    public void HandleHit()
    {
        if (currentState == TargetState.Pattern)
        {
            PlayPatternEffects();
            GameManager.gm.HandlePatternHit(this);
            SetState(TargetState.Hit);
        }
        else if (currentState == TargetState.Active)
        {
            GameManager.gm.AddScore(baseScore);
            SetState(TargetState.Hit);
        }

        //Schedule reset after delay
        Invoke("ResetTarget", 2f);
    }

    private void ResetTarget()
    {
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
        //VFX
        //Shader effects

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
}
