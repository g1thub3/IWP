using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LayeredUI : MonoBehaviour
{
    protected static List<LayeredUI> _instances = new List<LayeredUI>();
    public static LayeredUI CurrentInstance { 
        get {
            if (_instances.Count < 1) 
                return null;
            else
                return _instances.Last();
        }
    }
    protected PlayerInput _inputManager;
    protected List<MenuLayer> _layers;

    private bool _active;

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
        _active = false;
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
            if (!GlobalCanvasManager.Instance.IsInteractionActive && CurrentInstance == this)
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
            if (!_active)
            {
                _active = true;
                _instances.Add(this);
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
        else {
            if (_active)
            {
                _active = false;
                _instances.Remove(this);
            }
        }
        return false;
    }
}
