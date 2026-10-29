using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float cameraZ = -10f;

    // Used for the rocket launch ending (see FollowRocket).
    private float maxY = Mathf.Infinity;
    private bool smoothFollow = false;
    public float rocketFollowSmoothing = 5f;

    void LateUpdate()
    {
    if (player == null) return;

    Vector3 targetPosition = new Vector3(
        player.position.x,
        Mathf.Min(player.position.y, maxY),
        cameraZ
    );

    if (smoothFollow)
    {
    transform.position = Vector3.Lerp(transform.position, targetPosition, rocketFollowSmoothing * Time.deltaTime);
    }
    else
    {
    transform.position = targetPosition;
    }

    transform.rotation = Quaternion.identity;
    }

    // Switches the camera to follow the rocket, but only up to one screen height above where it is now.
    public void FollowRocket(Transform rocket)
    {
    player = rocket;
    smoothFollow = true;

    Camera cam = GetComponent<Camera>();
    float screenHeight = cam != null ? cam.orthographicSize * 2f : 10f;

    maxY = transform.position.y + screenHeight;
    }
}
