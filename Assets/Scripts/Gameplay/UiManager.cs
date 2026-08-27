using System.Collections.Generic;
using UnityEngine;

public interface ICloseableUi
{
    void Close();
}

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    private readonly List<ICloseableUi> _openUi = new();

    public bool HasOpenUi => _openUi.Count > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Register(ICloseableUi ui)
    {
        if (ui == null || _openUi.Contains(ui))
            return;

        _openUi.Add(ui);
        ApplyCursor();
    }

    public void Unregister(ICloseableUi ui)
    {
        if (!_openUi.Remove(ui))
            return;

        ApplyCursor();
    }

    // The single owner of the cursor while any UI is open. Player stops touching it
    // in that case, so the two no longer overwrite each other every frame.
    private void ApplyCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = HasOpenUi;
    }

    public void CloseTopUi()
    {
        if (_openUi.Count == 0) return;

        ICloseableUi top = _openUi[_openUi.Count - 1];

        // Close() usually calls Unregister itself, but drop the entry here for the
        // implementations that do not, or the list would stay stuck forever.
        top.Close();

        if (_openUi.Count > 0 && _openUi[_openUi.Count - 1] == top)
            _openUi.RemoveAt(_openUi.Count - 1);
    }
}
