using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseIndicator : MonoBehaviour
{
    [SerializeField]
    bool Isfacing = false;
    [SerializeField]
    float rotatingSpeed = 6f;
    [SerializeField]
    float distanceToCam = 2.0f;
    Transform target;
    Camera cam;

    public void PointToMouse(Transform pointer)
    {
        target = pointer;
        Isfacing = true;
        if (cam == null)
            cam = Camera.main;
    }

    public void ResetDefault()
    {
        Isfacing = false;
        transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Isfacing || target==null) return;
        Vector3 newPos = cam.transform.position + cam.transform.forward * distanceToCam;
        transform.position = newPos;

        var lookPos = target.position - transform.position;
        lookPos.z = 0;
        var rotation = Quaternion.LookRotation(lookPos);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotatingSpeed);

        
    }
}
