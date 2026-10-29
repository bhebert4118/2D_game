using UnityEngine;

public class floor_wall : MonoBehaviour
{
    public float probability;
    public bool visible;

    void Start()
    {
    visible = IsVisible(probability);

    foreach (Renderer objectRenderer in GetComponentsInChildren<Renderer>())
    {
    objectRenderer.enabled = visible;
    }

    foreach (Collider objectCollider in GetComponentsInChildren<Collider>())
    {
    objectCollider.enabled = visible;
    }
    }

    bool IsVisible(float probability)
    {
    return Random.value < probability / 100;
    }
}