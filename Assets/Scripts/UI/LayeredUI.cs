using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LayeredUI : MonoBehaviour
{
    protected PlayerInput _inputManager;
    protected List<MenuLayer> _layers;

    protected MenuLayer CurrentLayer
    {
        get
        {
            if (_layers.Count < 1)
                return null;
            else
                return _layers.Last();
        }
    }

    protected void PerformFunction()
    {
        if (CurrentLayer != null)
        {
            CurrentLayer.functions[CurrentLayer.CurrentSelection]();
        }
    }

    protected void Start()
    {
        _layers = new List<MenuLayer>();
    }

    protected bool Process()
    {
        if (CurrentLayer != null)
        {
            if (CurrentLayer.nextFrameTrigger)
            {
                CurrentLayer.OnCreateFrameComplete();
                CurrentLayer.nextFrameTrigger = false;
            }
            if (!GlobalCanvasManager.Instance.IsInteractionActive)
            {
                CurrentLayer.Control(_inputManager);
                if (_inputManager.actions["Accept"].WasPressedThisFrame())
                {
                    AudioManager.Instance.PlaySFXInScreen("Confirm");
                    PerformFunction();
                }
                if (_inputManager.actions["Decline"].WasPressedThisFrame())
                {
                    AudioManager.Instance.PlaySFXInScreen("Close");
                    CurrentLayer.Close();
                }
            }
            if (!CurrentLayer.IsOpen)
            {
                CurrentLayer.Close();
                _layers.Remove(CurrentLayer);
                if (CurrentLayer != null)
                {
                    if (CurrentLayer.refresh != null)
                    {
                        CurrentLayer.refresh();
                        CurrentLayer.OnRefresh();
                    }
                }
            }
            return true;
        }
        return false;
    }
}
