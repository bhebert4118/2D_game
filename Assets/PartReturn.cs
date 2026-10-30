using System.Collections;
using UnityEngine;

public class PartReturn : MonoBehaviour
{
    [Header("Parts")]
    [Tooltip("The rocket body part. It's the only part that shows up once installed, and it's what launches.")]
    public GameObject rocketBody;

    [Tooltip("Where the rocket stands. Put this empty object on the ground; the bottom of the rocket lines up with it.")]
    public Transform rocketStandPoint;

    [Tooltip("Rotation of the standing rocket. Leave at 0,0,0 for upright; if your image still shows upside down, try 0,0,180.")]
    public Vector3 rocketRotation = Vector3.zero;

    [Header("Puzzle UI")]
    public GameObject puzzlePanel;
    public NumberSlidingPuzzle slidingPuzzle;

    [Header("Game Complete")]
    public int partsNeeded = 2;
    [Tooltip("UI object (e.g. a TMP text saying COMPLETE) shown after the rocket launches. Leave it inactive in the scene.")]
    public GameObject completeScreen;

    [Header("Rocket Launch")]
    public float launchStartSpeed = 1f;
    public float launchAcceleration = 8f;
    [Tooltip("Minimum flight time. The rocket also keeps flying until it is off the top of the screen.")]
    public float launchDuration = 3f;
    [Tooltip("Optional: a particle system (flame/smoke) that plays during the launch. It gets attached to the rocket.")]
    public ParticleSystem launchFlame;
    public AudioClip launchSound;
    public float launchVolume = 1f;
    [Tooltip("The Main Camera's CameraFollow. It switches to following the rocket during the launch.")]
    public CameraFollow cameraFollow;

    private int partsInstalled = 0;

    private PlayerMovement currentPlayer;
    private Rigidbody currentPlayerRigidbody;
    private bool puzzleActive = false;

    private void OnTriggerEnter(Collider other)
    {
    if (puzzleActive)
    {
    return;
    }

    if (!other.CompareTag("Player"))
    {
    return;
    }

    PlayerInventory inventory = other.GetComponent<PlayerInventory>();

    if (inventory == null)
    {
    return;
    }

    GameObject returnedPart = inventory.PutDownItem();

    if (returnedPart == null)
    {
    return;
    }

    RocketPart rocketPart = returnedPart.GetComponent<RocketPart>();

    if (rocketPart != null)
    {
    rocketPart.canBePickedUp = false;
    }

    // Only the rocket body shows up. Any other part (the engine) just stays hidden and counts.
    if (returnedPart == rocketBody)
    {
    StandRocketUp();
    }

    currentPlayer = other.GetComponentInParent<PlayerMovement>();
    currentPlayerRigidbody = other.GetComponentInParent<Rigidbody>();

    puzzleActive = true;

    if (currentPlayerRigidbody != null)
    {
    currentPlayerRigidbody.linearVelocity = Vector3.zero;
    currentPlayerRigidbody.angularVelocity = Vector3.zero;
    }

    if (currentPlayer != null)
    {
    currentPlayer.LockInput(999f);
    }

    if (puzzlePanel != null)
    {
    puzzlePanel.SetActive(true);
    }

    if (slidingPuzzle != null)
    {
    slidingPuzzle.BeginPuzzle(this);
    }
    }

    private void StandRocketUp()
    {
    Transform body = rocketBody.transform;
    Transform standPoint = rocketStandPoint != null ? rocketStandPoint : transform;

    // Stop physics from tipping it over or dropping it.
    Rigidbody bodyRigidbody = rocketBody.GetComponent<Rigidbody>();

    if (bodyRigidbody != null)
    {
    bodyRigidbody.isKinematic = true;
    }

    // Set rotation and scale explicitly instead of copying them from anything else,
    // so a flipped return point or a negative scale can't turn it upside down.
    body.rotation = Quaternion.Euler(rocketRotation);

    Vector3 scale = body.localScale;
    body.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

    body.position = standPoint.position;
    rocketBody.SetActive(true);

    // Line the bottom of the image up with the stand point so it sits on the ground.
    Renderer bodyRenderer = rocketBody.GetComponentInChildren<Renderer>();

    if (bodyRenderer != null)
    {
    float bottom = bodyRenderer.bounds.min.y;
    body.position += Vector3.up * (standPoint.position.y - bottom);
    }
    }

    public void CompletePuzzle()
    {
    if (!puzzleActive)
    {
    return;
    }

    if (puzzlePanel != null)
    {
    puzzlePanel.SetActive(false);
    }

    partsInstalled++;

    if (partsInstalled >= partsNeeded)
    {
    // Game over: the player boards the rocket, it launches, then COMPLETE shows.
    if (currentPlayer != null)
    {
    currentPlayer.gameObject.SetActive(false);
    }

    currentPlayer = null;
    currentPlayerRigidbody = null;
    StartCoroutine(LaunchRocket());
    return;
    }

    if (currentPlayerRigidbody != null)
    {
    currentPlayerRigidbody.linearVelocity = Vector3.zero;
    currentPlayerRigidbody.angularVelocity = Vector3.zero;
    }

    if (currentPlayer != null)
    {
    currentPlayer.LockInput(0f);
    }

    currentPlayer = null;
    currentPlayerRigidbody = null;
    puzzleActive = false;
    }

    private IEnumerator LaunchRocket()
    {
    if (rocketBody != null)
    {
    Transform rocket = rocketBody.transform;
    Renderer rocketRenderer = rocketBody.GetComponentInChildren<Renderer>();
    Camera cam = cameraFollow != null ? cameraFollow.GetComponent<Camera>() : Camera.main;

    if (cameraFollow != null)
    {
    cameraFollow.FollowRocket(rocket);
    }

    Sfx.Play(launchSound, launchVolume);

    if (launchFlame != null)
    {
    launchFlame.transform.SetParent(rocket, true);
    launchFlame.Play();
    }

    float speed = launchStartSpeed;
    float timer = 0f;

    // Fly for at least launchDuration, and keep going until the rocket is above the top of the screen.
    while (timer < launchDuration || IsBelowTopOfScreen(rocketRenderer, cam))
    {
    speed += launchAcceleration * Time.deltaTime;
    rocket.position += Vector3.up * speed * Time.deltaTime;

    timer += Time.deltaTime;
    yield return null;
    }
    }

    if (completeScreen != null)
    {
    completeScreen.SetActive(true);
    }
    }

    private bool IsBelowTopOfScreen(Renderer rocketRenderer, Camera cam)
    {
    if (rocketRenderer == null || cam == null)
    {
    return false;
    }

    float screenTop = cam.transform.position.y + cam.orthographicSize;
    return rocketRenderer.bounds.min.y < screenTop;
    }
}
