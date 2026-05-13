using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Prompt")]
    public string prompt = "Interact";

    [Header("Settings")]
    public bool disableAfterInteract = false;

    [Header("Events")]
    public UnityEvent onInteract;

    [Header("Glow / Highlight")]
    [Tooltip("Renderers that should glow when the player is looking at this object. If empty, all child Renderers are used automatically.")]
    public Renderer[] glowRenderers;

    [Tooltip("Optional Light (point/spot) that pulses while highlighted — gives a soft halo around the object.")]
    public Light optionalGlowLight;

    [Tooltip("Glow tint. Cyan reads well against most environments.")]
    public Color glowColor = new Color(0.2f, 0.95f, 1f, 1f);

    [Range(0f, 8f)]
    public float glowIntensity = 2.5f;

    [Tooltip("Pulses per second. Set to 0 for a steady glow.")]
    public float pulseSpeed = 2.5f;

    private bool hasInteracted;
    private bool isHighlighted;
    private MaterialPropertyBlock _mpb;
    private Color[] _originalEmissions;
    private float _originalLightIntensity;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();

        if (glowRenderers == null || glowRenderers.Length == 0)
        {
            glowRenderers = GetComponentsInChildren<Renderer>(true);
        }

        if (glowRenderers != null)
        {
            _originalEmissions = new Color[glowRenderers.Length];
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                var r = glowRenderers[i];
                if (r == null) continue;
                if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(EmissionColorId))
                {
                    _originalEmissions[i] = r.sharedMaterial.GetColor(EmissionColorId);
                }
                else
                {
                    _originalEmissions[i] = Color.black;
                }
                // Make sure emission stays enabled on the shader keyword so
                // _EmissionColor actually shows up via MaterialPropertyBlock.
                if (r.sharedMaterial != null)
                {
                    r.sharedMaterial.EnableKeyword("_EMISSION");
                }
            }
        }

        if (optionalGlowLight != null)
        {
            _originalLightIntensity = optionalGlowLight.intensity;
            optionalGlowLight.color = glowColor;
            optionalGlowLight.enabled = false;
        }
    }

    private void Update()
    {
        if (!isHighlighted) return;

        float t = pulseSpeed > 0f
            ? (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) * 0.5f
            : 1f;
        float intensity = Mathf.Lerp(glowIntensity * 0.35f, glowIntensity, t);

        ApplyEmission(glowColor * intensity);

        if (optionalGlowLight != null)
        {
            optionalGlowLight.intensity = Mathf.Lerp(_originalLightIntensity, _originalLightIntensity + intensity * 2f, t);
            optionalGlowLight.color = glowColor;
        }
    }

    private void ApplyEmission(Color emission)
    {
        if (glowRenderers == null) return;
        for (int i = 0; i < glowRenderers.Length; i++)
        {
            var r = glowRenderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionColorId, emission);
            r.SetPropertyBlock(_mpb);
        }
    }

    public void SetHighlight(bool on)
    {
        if (isHighlighted == on) return;
        isHighlighted = on;

        if (optionalGlowLight != null)
        {
            optionalGlowLight.enabled = on;
        }

        if (!on)
        {
            // Restore original emission values.
            if (glowRenderers == null) return;
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                var r = glowRenderers[i];
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                Color reset = (_originalEmissions != null && i < _originalEmissions.Length)
                    ? _originalEmissions[i]
                    : Color.black;
                _mpb.SetColor(EmissionColorId, reset);
                r.SetPropertyBlock(_mpb);
            }
        }
    }

    public void Interact()
    {
        if (hasInteracted)
        {
            return;
        }

        onInteract.Invoke();

        if (disableAfterInteract)
        {
            hasInteracted = true;
            SetHighlight(false);
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Always leave the object un-highlighted so emission doesn't get stuck.
        if (isHighlighted)
        {
            SetHighlight(false);
        }
    }
}
