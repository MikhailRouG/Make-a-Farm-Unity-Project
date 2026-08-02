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

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool HasOpenUi => _openUi.Count > 0;

    public void Register(ICloseableUi ui)
    {
        if (!_openUi.Contains(ui))
        {
            _openUi.Add(ui);
            Cursor.visible = true;
        }
    }

    public void Unregister(ICloseableUi ui)
    {
        _openUi.Remove(ui);
    }

    public void CloseTopUi()
    {
        if (_openUi.Count == 0) return;
        _openUi[_openUi.Count - 1].Close();
    }
}
