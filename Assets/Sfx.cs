using UnityEngine;

// Plays a sound effect in 2D (same volume no matter where the camera is).
// It plays on its own temporary object, so it isn't cut off if the thing
// that played it gets hidden, disabled or teleported.
public static class Sfx
{
    public static void Play(AudioClip clip, float volume = 1f)
    {
    if (clip == null)
    {
    return;
    }

    GameObject soundObject = new GameObject("Sfx " + clip.name);
    AudioSource source = soundObject.AddComponent<AudioSource>();

    source.spatialBlend = 0f;
    source.PlayOneShot(clip, volume);

    Object.Destroy(soundObject, clip.length + 0.1f);
    }
}
