using System;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Player
{
    public class PlayerInteraction : NetworkBehaviour
    {
        private const float DistanceTolerance = 1.5f;

        [Header("References")]
        [SerializeField] private Transform _rayOrigin;

        [Header("Settings")]
        [SerializeField] private float _interactDistance = 3f;
        [SerializeField] private LayerMask _interactableMask;

        private Camera _playerCamera;
        private IInteractable _currentTarget;
        private Highlightable _currentHighlight;

        public bool HasTarget { get; private set; }
        public Vector3 LookPoint { get; private set; }

        public event Action<string> OnHasInteraction;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (_playerCamera == null)
                _playerCamera = FindAnyObjectByType<Camera>();

            if (_rayOrigin == null)
                _rayOrigin = transform;
        }

        public void InteractionByForward()
        {
            if (!isLocalPlayer || _playerCamera == null) return;

            // Deliberately no pointer-over-UI test: here the crosshair aims, not the
            // mouse. The OS cursor stays wherever it was last left, and testing it
            // would kill the target every frame it happens to rest over the HUD.
            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            Interaction(ray);
        }

        public void InteractionByCursor()
        {
            if (!isLocalPlayer || _playerCamera == null) return;

            if (IsPointerOverUi())
            {
                ClearTarget();
                return;
            }

            Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
            Interaction(ray);
        }

        public void Interaction(Ray ray)
        {
            Debug.DrawRay(ray.origin, ray.direction * _interactDistance, Color.green);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactableMask))
            {
                LookPoint = hit.point;
                SetTarget(hit.collider);
                return;
            }

            ClearTarget();
        }

        // Both lookups walk up from the collider, which often sits on a child of the
        // prefab that owns the script and the renderers. Resolving them side by side
        // is what keeps the outline and the prompt describing the same object.
        private void SetTarget(Collider hitCollider)
        {
            IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();

            if (interactable == null)
            {
                ClearTarget();
                return;
            }

            SetHighlight(hitCollider.GetComponentInParent<Highlightable>());

            if (_currentTarget == interactable) return;

            _currentTarget = interactable;
            HasTarget = true;
            OnHasInteraction?.Invoke(interactable.InteractionPrompt);
        }

        public void TryInteract()
        {
            if (!isLocalPlayer || _currentTarget == null) return;

            // A destroyed target still reads as non-null through the interface; only
            // the Unity object comparison catches it.
            if (_currentTarget is UnityEngine.Object unityObj && unityObj == null)
            {
                ClearTarget();
                return;
            }

            if (_currentTarget is Component component)
            {
                if (component.TryGetComponent(out NetworkIdentity identity))
                    CmdExecuteInteraction(identity);
                else
                    _currentTarget.Interact(gameObject);
            }

            // The target is deliberately kept: the player is still looking at it. The
            // input action is edge triggered, so holding the key cannot repeat, and
            // dropping it here used to leave HasTarget true with no target behind it,
            // re-firing the prompt on the next frame.
        }

        // The target comes from the client, so the distance is re-checked here.
        [Command]
        private void CmdExecuteInteraction(NetworkIdentity targetIdentity)
        {
            if (targetIdentity == null)
                return;

            float distance = Vector3.Distance(transform.position, targetIdentity.transform.position);

            if (distance > _interactDistance * DistanceTolerance)
                return;

            if (targetIdentity.TryGetComponent(out IInteractable serverTarget))
                serverTarget.Interact(gameObject);
        }

        // Public so an opening UI can drop the target: Player stops driving the ray
        // while a panel is up, which otherwise freezes the outline on whatever the
        // player happened to be looking at when the shop opened.
        public void ClearTarget()
        {
            if (HasTarget)
                OnHasInteraction?.Invoke(string.Empty);

            SetHighlight(null);

            _currentTarget = null;
            HasTarget = false;
        }

        private void SetHighlight(Highlightable next)
        {
            if (_currentHighlight != next)
            {
                if (_currentHighlight != null)
                    _currentHighlight.SetHighlighted(false);

                _currentHighlight = next;

                if (_currentHighlight != null)
                    _currentHighlight.SetHighlighted(true);

                return;
            }

            // Still the same object as last frame. It gets one poke per frame so it can
            // notice its geometry was swapped under the outline - a growing plant
            // replaces the very renderers the hulls hang under. Only the hovered object
            // pays for this, which is why Highlightable has no Update of its own.
            if (_currentHighlight != null)
                _currentHighlight.RefreshSources();
        }

        private void OnDisable()
        {
            SetHighlight(null);
        }

        private static bool IsPointerOverUi() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
