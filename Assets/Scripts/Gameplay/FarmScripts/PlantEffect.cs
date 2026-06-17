using System.Collections;
using UnityEngine;

namespace Gameplay.Farm
{
    public class PlantEffect : MonoBehaviour
    {
        private EffectConfig _config;
        private Plant _plant;
        private Coroutine _startPlayCoroutine;

        public void Awake()
        {
            _plant = GetComponent<Plant>();
        }
        private void OnEnable()
        {
            if (_plant == null) return;

            _plant.OnInitialized += Init;
            _plant.OnUpdateStage += PlayLocal;
        }

        private void OnDisable()
        {
            if (_plant == null) return;

            _plant.OnInitialized -= Init;
            _plant.OnUpdateStage -= PlayLocal;
        }
        private void Init(EffectConfig effect)
        {
            _config = effect;
        }
        private void PlayLocal(EffectState state, string text)
        {
            if (_config == null) return;

            if (!_config.TryGetEffect(state, out EffectEntry effect)) return;
            if (effect.Prefab == null) return;
            if (state == EffectState.Start)
            {
                StartPlay(effect);
                return;
            }
            GameObject instance = Instantiate(
                effect.Prefab,
                transform.position,
                transform.rotation
            );

            Destroy(instance, effect.Lifetime);
        }
        private void StartPlay(EffectEntry effect)
        {
            if (_startPlayCoroutine != null)
                StopCoroutine(_startPlayCoroutine);

            _startPlayCoroutine = StartCoroutine(StartPlayRoutine(effect));
        }
        private IEnumerator StartPlayRoutine(EffectEntry effect)
        {
            if (effect == null || effect.Prefab == null)
                yield break;

            SetChildrenActive(false);

            GameObject instance = Instantiate(
                effect.Prefab,
                transform.position,
                transform.rotation
            );

            yield return new WaitForSeconds(effect.Lifetime);

            if (instance != null)
                Destroy(instance);

            SetChildrenActive(true);

            _startPlayCoroutine = null;
        }
        private void SetChildrenActive(bool value)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(value);
            }
        }
        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded)
            {
                PlayLocal(EffectState.Destroy, string.Empty);
            }
        }
    }
}