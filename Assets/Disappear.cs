using UnityEngine;

public class Disappear : MonoBehaviour
{
    public float VisTime;

    private float startTime;
    private float invisibleTime;
    private bool isVisible = true;

    private Renderer[] objectRenderers;
    private Collider[] objectColliders;

    void Start()
    {
    startTime = Time.time;
    invisibleTime = VisTime / 2f;

    objectRenderers = GetComponentsInChildren<Renderer>(true);
    objectColliders = GetComponentsInChildren<Collider>(true);
    }

    void Update()
    {
    if (isVisible && Time.time - startTime >= VisTime)
    {
    SetVisibility(false);
    }
    else if (!isVisible && Time.time - startTime >= invisibleTime)
    {
    SetVisibility(true);
    }
    }

    void SetVisibility(bool visible)
    {
    isVisible = visible;
    startTime = Time.time;

    foreach (Renderer objectRenderer in objectRenderers)
    {
    objectRenderer.enabled = visible;
    }

    foreach (Collider objectCollider in objectColliders)
    {
    objectCollider.enabled = visible;
    }
    }
}