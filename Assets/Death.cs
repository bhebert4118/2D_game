using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    public AudioClip deathSound;
    public float deathVolume = 1f;

    void OnTriggerEnter(Collider other)
    {
    if (other.CompareTag("Player"))
    {
    PlayerInventory inventory = other.GetComponent<PlayerInventory>();
    PlayerMovement player = other.GetComponent<PlayerMovement>();
    PlayerMovement movement = other.GetComponent<PlayerMovement>();
    Rigidbody rb = other.GetComponent<Rigidbody>();


    Sfx.Play(deathSound, deathVolume);

    player.transform.position = new Vector3(0, 0, 0);
    rb.linearVelocity = Vector3.zero;


    if (movement != null)
    {
    movement.LockInput(0.5f);
    }

    }
    }
}