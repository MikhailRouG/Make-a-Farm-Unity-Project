using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private float _defaultFadeDuration = 1.5f;
    [SerializeField] private float _musicVolume = 1f;

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

    public void PlayTrack(AudioClip clip, float fadeDuration = -1f)
    {
        _crossfader.Play(clip, _musicVolume, fadeDuration < 0f ? _defaultFadeDuration : fadeDuration);
    }

    public void Stop(float fadeDuration = -1f)
    {
        _crossfader.Stop(fadeDuration < 0f ? _defaultFadeDuration : fadeDuration);
    }
}
