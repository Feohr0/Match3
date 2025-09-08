using UnityEngine;

public class OcclusionController : MonoBehaviour
{
    [Header("Occlusion Settings")]
    public GameObject targetObject; // Saklanacak obje
    public Material occlusionMaterial; // Occlusion shader'lý material

    [Header("Dynamic Control")]
    public bool hideTarget = true;
    public KeyCode toggleKey = KeyCode.Space;

    private Renderer occlusionRenderer;
    private Renderer targetRenderer;

    void Start()
    {
        // Occlusion renderer'ý al
        occlusionRenderer = GetComponent<Renderer>();

        if (occlusionRenderer != null && occlusionMaterial != null)
        {
            occlusionRenderer.material = occlusionMaterial;
        }

        // Target renderer'ý al
        if (targetObject != null)
        {
            targetRenderer = targetObject.GetComponent<Renderer>();

            // Target objenin render queue'sunu ayarla
            if (targetRenderer != null)
            {
                targetRenderer.material.renderQueue = 2001; // Geometry+1
            }
        }

        UpdateOcclusion();
    }

    void Update()
    {
        // Toggle kontrolü
        if (Input.GetKeyDown(toggleKey))
        {
            hideTarget = !hideTarget;
            UpdateOcclusion();
        }
    }

    void UpdateOcclusion()
    {
        if (occlusionRenderer != null)
        {
            occlusionRenderer.enabled = hideTarget;
        }
    }

    // Public metodlar
    public void ShowTarget()
    {
        hideTarget = false;
        UpdateOcclusion();
    }

    public void HideTarget()
    {
        hideTarget = true;
        UpdateOcclusion();
    }

    public void ToggleTarget()
    {
        hideTarget = !hideTarget;
        UpdateOcclusion();
    }
}