using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionsProbe : MonoBehaviour
{
    [SerializeField] string debugId = "IAProbe";
    [SerializeField] PlayerInput playerInput;

    readonly List<InputAction> bound = new List<InputAction>();

    void OnEnable()
    {
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null || playerInput.actions == null) { Debug.LogError("[" + debugId + "] No PlayerInput"); return; }

        foreach (var map in playerInput.actions.actionMaps)
        {
            if (!map.enabled) map.Enable();
            foreach (var act in map.actions)
            {
                act.started += OnAny;
                act.performed += OnAny;
                act.canceled += OnAny;
                if (!act.enabled) act.Enable();
                bound.Add(act);
            }
        }

        var mapsState = "";
        foreach (var m in playerInput.actions.actionMaps) mapsState += m.name + "(" + (m.enabled ? "on" : "off") + ") ";
        Debug.Log("[" + debugId + "] Maps " + mapsState);
    }

    void OnDisable()
    {
        for (int i = 0; i < bound.Count; i++)
        {
            if (bound[i] == null) continue;
            bound[i].started -= OnAny;
            bound[i].performed -= OnAny;
            bound[i].canceled -= OnAny;
        }
        bound.Clear();
    }

    void OnAny(InputAction.CallbackContext ctx)
    {
        string map = ctx.action.actionMap != null ? ctx.action.actionMap.name : "<null>";
        Debug.Log("[" + debugId + "] " + map + "/" + ctx.action.name + " phase=" + ctx.phase);
    }
}
