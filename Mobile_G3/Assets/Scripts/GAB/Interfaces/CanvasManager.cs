using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanvasManager : MonoSingleton<CanvasManager>
{
    // Warning - This is for in game canvas only!

    [SerializeField] private CanvasData[] canvases;

    [Serializable]
    public struct CanvasData
    {
        public GameObject canvas;
        public CanvasType type;
    }

    public void DisplayCanvas(params CanvasType[] canvasTypes)
    {
        var datas = new List<CanvasData?>();

        CanvasData? data;
        foreach (var ct in canvasTypes)
        {
            if (ct == CanvasType.None)
            {
                datas.Clear();
                break;
            }
            
            data = GetCanvasDataFromType(ct);

            if (!data.HasValue)
            {
                Debug.LogError("There's no valid canvas!");
                continue;
            }

            datas.Add(data);
        }

        foreach (var cd in canvases)
        {
            if (canvasTypes.Contains(cd.type)) continue;
            DisableCanvas(cd);
        }

        foreach (var cd in datas)
        {
            cd?.canvas.SetActive(true);
        }
    }

    private void DisableCanvas(CanvasData data)
    {
        // todo - fix bug
        //if(data.type == CanvasType.TimerCanvas) data.canvas.transform.parent.GetComponent<MainCanvasManager>().ResetCanvasIfNeeded();
        data.canvas.SetActive(false);
    }

    private CanvasData? GetCanvasDataFromType(CanvasType type)
    {
        foreach (var data in canvases)
        {
            if (data.type == type) return data;
        }

        return null;
    }
}