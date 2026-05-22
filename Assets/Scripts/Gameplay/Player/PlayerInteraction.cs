using Mirror;
using Mirror.Examples.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("References")]
     private Camera _playerCamera;
    [SerializeField] private Transform rayOrigin;

    [Header("Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private bool interactByMouse;

    private bool cursorStatus;
    private IInteractable currentTarget;
    [HideInInspector] public bool HasTarget;
    public Action<String> OnHasInteraction;
    public Vector3 LookPoint { get; private set; }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (_playerCamera == null)
        {
            _playerCamera =FindAnyObjectByType<Camera>();   
        }
        if (rayOrigin == null)
            rayOrigin = transform;
    }

    public void InteractionByForward()
    {
        if (!isLocalPlayer || _playerCamera == null) return;
        Ray ray = new Ray(rayOrigin.position, _playerCamera.transform.forward);
        Interaction(ray);
    }
    public void InteractionByCursor()
    {
        if (!isLocalPlayer || _playerCamera == null) return;
        Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
        Interaction(ray);
    }
    public void Interaction(Ray ray)
    {
        if(CheckMouseOnUi()) return;
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            LookPoint = hit.point;

            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable component))
            {
                if (currentTarget != component)
                {
                    currentTarget = component;
                    HasTarget = true;
                    OnHasInteraction?.Invoke(currentTarget.InteractionPrompt);
                }
                return;
            }
        }

        ClearTarget();
    }

    private void ClearTarget()
    {
        if (HasTarget)
            OnHasInteraction?.Invoke(string.Empty);

        currentTarget = null;
        HasTarget = false;
    }

    public void TryInteract()
    {
        if (!isLocalPlayer || currentTarget == null) return;
        if (currentTarget is UnityEngine.Object unityObj && unityObj == null)
        {
            currentTarget = null;
            return;
        }

        if (currentTarget is Component component)
        {
            if (component.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
            {
                CmdExecuteInteraction(identity);
            }
            else
            {
                currentTarget.Interact(gameObject);
            }
        }

        currentTarget = null;
    }

    [Command]
    private void CmdExecuteInteraction(NetworkIdentity targetIdentity)
    {
        float distance = Vector3.Distance(transform.position, targetIdentity.transform.position);

        if (targetIdentity.TryGetComponent<IInteractable>(out IInteractable serverTarget))
        {
            serverTarget.Interact(gameObject);
        }
    }

    private bool CheckMouseOnUi()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearTarget();
            return true;
        }
        return false;
    }
}
