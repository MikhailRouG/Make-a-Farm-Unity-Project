using Mirror;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
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
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (rayOrigin == null)
            rayOrigin = transform;
    }

    public void InteractionByForward()
    {
        Ray ray = new Ray(rayOrigin.position, playerCamera.transform.forward);
        Interaction(ray);
    }
    public void InteractionByCursor()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
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
            else if (HasTarget)
            {
                ClearTarget();
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
        if (currentTarget is UnityEngine.Object unityObj && unityObj == null)
        {
            currentTarget = null;
            return;
        }

        if (currentTarget is Component component)
        {
            currentTarget.Interact(gameObject);

            CmdVerifyInteraction(component.transform.position);
        }

        currentTarget = null;
    }

    [Command]
    private void CmdVerifyInteraction(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > interactDistance + 0.5f)
        {
            Debug.LogWarning($"[Anti-Cheat] Player tried to interact too far away! Distance: {distance}");
            return;
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
