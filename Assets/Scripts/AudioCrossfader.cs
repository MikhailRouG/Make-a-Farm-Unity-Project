using System.Collections;
using UnityEngine;

public class AudioCrossfader
{
    private readonly MonoBehaviour _runner;
    private readonly AudioSource _sourceA;
    private readonly AudioSource _sourceB;
    private AudioSource _active;
    private AudioClip _currentClip;
    private Coroutine _fadeCoroutine;

    public AudioCrossfader(MonoBehaviour runner, AudioSource sourceA, AudioSource sourceB)
    {
        _runner = runner;
        _sourceA = sourceA;
        _sourceB = sourceB;
        _active = sourceA;

        SetupSource(sourceA);
        SetupSource(sourceB);
    }

    private static void SetupSource(AudioSource source)
    {
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
    }

    public void Play(AudioClip clip, float targetVolume, float fadeDuration)
    {
        if (clip == null || clip == _currentClip)
            return;

        _currentClip = clip;

        AudioSource from = _active;
        AudioSource to = _active == _sourceA ? _sourceB : _sourceA;
        _active = to;

        to.clip = clip;
        to.volume = 0f;
        to.Play();

        if (_fadeCoroutine != null)
            _runner.StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = _runner.StartCoroutine(CrossFadeRoutine(from, to, targetVolume, fadeDuration));
    }

    public void Stop(float fadeDuration)
    {
        _currentClip = null;

        if (_fadeCoroutine != null)
            _runner.StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = _runner.StartCoroutine(FadeOutRoutine(_active, fadeDuration));
    }

    private IEnumerator CrossFadeRoutine(AudioSource from, AudioSource to, float targetVolume, float duration)
    {
        float fromStart = from.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            from.volume = Mathf.Lerp(fromStart, 0f, t);
            to.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        from.volume = 0f;
        from.Stop();
        to.volume = targetVolume;

        _fadeCoroutine = null;
    }

    private IEnumerator FadeOutRoutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();

        _fadeCoroutine = null;
    }
}
