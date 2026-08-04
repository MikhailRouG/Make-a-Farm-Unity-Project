using UnityEngine;

public class AmbientSoundManager : MonoBehaviour
{
    public static AmbientSoundManager Instance { get; private set; }

    [SerializeField] private float _defaultFadeDuration = 2f;
    [SerializeField] private float _ambientVolume = 1f;

    private AudioCrossfader _crossfader;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _crossfader = new AudioCrossfader(
            this,
            gameObject.AddComponent<AudioSource>(),
            gameObject.AddComponent<AudioSource>());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Play(AudioClip clip, float fadeDuration = -1f)
    {
        _crossfader.Play(clip, _ambientVolume, fadeDuration < 0f ? _defaultFadeDuration : fadeDuration);
    }

    public void Stop(float fadeDuration = -1f)
    {
        _crossfader.Stop(fadeDuration < 0f ? _defaultFadeDuration : fadeDuration);
    }
}
