using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _musicClip;
    [SerializeField] private AudioClip _ambientClip;
    [SerializeField] private float _fadeDuration = -1f;

    private void Start()
    {
        if (_musicClip != null)
            MusicManager.Instance?.PlayTrack(_musicClip, _fadeDuration);

        if (_ambientClip != null)
            AmbientSoundManager.Instance?.Play(_ambientClip, _fadeDuration);
    }
}
