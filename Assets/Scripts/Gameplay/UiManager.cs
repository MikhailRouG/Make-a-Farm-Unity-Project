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
        Cursor.visible = true;
    }

    public void Unregister(ICloseableUi ui)
    {
        _openUi.Remove(ui);
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
