using UnityEngine;
using System.Collections;

namespace Gameplay.Farm
{
    public class AppearAnimation : MonoBehaviour
    {
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private float animationDuration = 0.5f;

        private Vector3 initialScale;
        private Coroutine animationCoroutine;

        private void OnValidate()
        {
            if (scaleCurve == null || scaleCurve.length == 0)
            {
                scaleCurve = new AnimationCurve(
                    new Keyframe(0f, 0f, 0f, 2f),
                    new Keyframe(0.7f, 1.1f, 0f, 0f),
                    new Keyframe(1f, 1.0f, -0.5f, 0f)
                );
            }
        }

        private void Awake()
        {
            initialScale = transform.localScale;

        }

        private void OnEnable()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateScale());
        }

        private void OnDisable()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = null;
        }
        private IEnumerator AnimateScale()
        {
            float timer = 0f;

            while (timer < animationDuration)
            {
                timer += Time.deltaTime;

                float normalizedTime = timer / animationDuration;

                float curveValue = scaleCurve.Evaluate(normalizedTime);

                transform.localScale = initialScale * curveValue;

                yield return null;
            }

            float finalCurveValue = scaleCurve.Evaluate(1f);
            transform.localScale = initialScale * finalCurveValue;

            animationCoroutine = null;
        }
    }
}