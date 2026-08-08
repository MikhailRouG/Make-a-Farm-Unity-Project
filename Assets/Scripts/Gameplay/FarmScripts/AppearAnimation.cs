using System.Collections;
using UnityEngine;

namespace Gameplay.Farm
{
    public class AppearAnimation : MonoBehaviour
    {
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private float animationDuration = 1f;

        private float _size;
        private Vector3 _initialScale;
        private Coroutine _animationCoroutine;

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
            _initialScale = transform.localScale;
        }

        private void OnDisable()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }
        public void Initialize(float size)
        {
            if (!isActiveAndEnabled)
                return;

            _size = Mathf.Max(0f, size);

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateScale());
        }

        private IEnumerator AnimateScale()
        {
            // A zero duration set in the inspector would divide by zero and put
            // NaN into localScale, making the object vanish from the scene.
            if (animationDuration <= 0f)
            {
                transform.localScale = _initialScale * (scaleCurve.Evaluate(1f) * _size);
                _animationCoroutine = null;
                yield break;
            }

            float timer = 0f;

            while (timer < animationDuration)
            {
                timer += Time.deltaTime;

                float normalizedTime = timer / animationDuration;

                float curveValue = scaleCurve.Evaluate(normalizedTime) * _size;

                transform.localScale = _initialScale * curveValue;

                yield return null;
            }

            float finalCurveValue = scaleCurve.Evaluate(1f) * _size;
            transform.localScale = _initialScale * finalCurveValue;

            _animationCoroutine = null;
        }
    }
}