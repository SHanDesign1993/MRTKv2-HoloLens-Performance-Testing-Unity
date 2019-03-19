using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.Core.Providers.UnityInput;
using Microsoft.MixedReality.Toolkit.Core.Utilities.Async;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class mouseEvent : UnityEvent<Vector3> { }
public class MouseAssistant : MonoBehaviour
{
    [SerializeField]
    Text mouseInfo;
    [SerializeField]
    Toggle screenInfo;

    [SerializeField]
    MouseIndicator indicator;
    float lerpSpeed = 2.0f;
    //the device manager of mouse 
    MouseDeviceManager mouseManager;
    GameObject mousePointer;
    Camera cam;
    bool mouseInited = false;
    [SerializeField]
    bool mouseInView = false;
    
    public UnityEvent<Vector3> OnMouseInView = new mouseEvent();
    public UnityEvent<Vector3> OnMouseOutOfView = new mouseEvent();

    async void Start()
    {
        //wait MRTK register extended service
        await new WaitUntil(() => MixedRealityToolkit.RegisteredMixedRealityServices != null);
        foreach (var service in MixedRealityToolkit.RegisteredMixedRealityServices)
        {
            //get mouse service by name
            if (service.Item2.Name == "Mouse")
            {
                mouseManager = (MouseDeviceManager)service.Item2;
                mousePointer = mouseManager.Controller.InputSource.Pointers[0].BaseCursor.GameObjectReference;
                cam = Camera.main;
                mouseInited = true;
            }
                
        }
    }

    void Update()
    {
        if (!mouseInfo || !screenInfo) return;
        if (!Input.mousePresent) { mouseInfo.text = "Not Found"; return; }
        if (!mouseInited){ mouseInfo.text = "Not Inited"; return; }

        mouseInfo.text = "(" + Input.mousePosition.x + "," + Input.mousePosition.y + ")";

        MonitorMousPos();
    }

    void MonitorMousPos()
    {
        Vector3 screenPoint = cam.WorldToViewportPoint(mousePointer.transform.position);
        mouseInView = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (screenInfo.isOn != mouseInView)
        {
            screenInfo.isOn = mouseInView;
            indicator.gameObject.SetActive(!mouseInView);
            if (mouseInView)
            {
                OnMouseInView.Invoke(mousePointer.transform.position);
                indicator.ResetDefault();
                //arrow.transform.rotation = Quaternion.identity;
            }
            else
            {
                OnMouseOutOfView.Invoke(mousePointer.transform.position);
                indicator.PointToMouse(mousePointer.transform);
                //if (rotateRoutine != null) StopCoroutine(rotateRoutine);
                //rotateRoutine = StartCoroutine(rotatetowardZAxis(arrow.transform, mousePointer.transform, 0.4f));
            }
        }
    }

    bool rotating = false;
    Coroutine rotateRoutine;
    IEnumerator rotatetowardZAxis(Transform root, Transform target, float duration)
    {
        if (rotating)
        {
            yield break;
        }
        rotating = true;

        Quaternion currentRot = root.rotation;
        Vector3 direction = target.transform.position - root.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            root.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), counter / duration);
            yield return null;
        }

        rotating = false;
    }
}
