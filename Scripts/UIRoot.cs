using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI根节点
/// author: 雨Lu尧
/// </summary>
public class UIRoot : MonoBehaviour
{
    private static UIRoot instance;
    private static Camera uiCamera = null;

    private EventSystem eventSystem = null;

    public static UIRoot Instance
    {
        get
        {
            if (!instance)
            {
                GameObject root = GameObject.Find("UIRoot");
                instance = root.GetComponent<UIRoot>() ?? root.AddComponent<UIRoot>();
            }

            return instance;
        }
    }

    public static Camera UICamera
    {
        get
        {
            if (uiCamera == null)
            {
                GameObject uiCameraGo = GameObject.FindGameObjectWithTag("UICamera");
                if (uiCameraGo)
                {
                    uiCamera = uiCameraGo.GetComponent<Camera>();
                }
            }

            return uiCamera;
        }
    }

    public EventSystem EventSystem
    {
        get
        {
            if (eventSystem == null)
            {
                Transform eventSystemTrans = this.transform.Find("EventSystem");
                if (eventSystemTrans)
                {
                    eventSystem = eventSystemTrans.GetComponent<EventSystem>();
                }
            }

            return eventSystem;
        }
    }
    
    void Start()
    {
        DontDestroyOnLoad(transform);
    }
}
