using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;

public class RemotingExample : MonoBehaviour
{

    [SerializeField]
    private string IP;

    private bool connected = false;

    public void Connect()
    {
        if (HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Connected)
        {
            HolographicRemoting.Connect(IP);
        }
    }

    void Update()
    {
        if (!connected && HolographicRemoting.ConnectionState == HolographicStreamerConnectionState.Connected)
        {
            connected = true;

            StartCoroutine(LoadDevice("WindowsMR"));
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (connected)
            {
                HolographicRemoting.Disconnect();
                connected = false;
            }
            else
                Connect();
        }

    }

    IEnumerator LoadDevice(string newDevice)
    {
        XRSettings.LoadDeviceByName(newDevice);
        yield return null;
        XRSettings.enabled = true;
    }

    private void OnGUI()
    {
        //IP = GUI.TextField(new Rect(10, 10, 200, 30), IP, 25);
        GUI.Label(new Rect(10, 10, 200, 20), "IP: "+IP);
        GUI.Label(new Rect(10, 30, 200, 20), "C: connect/disconnect to hololens");
        /*
        string button = (connected ? "Disconnect" : "Connect");

        if (GUI.Button(new Rect(220, 10, 100, 30), button))
        {
            if (connected)
            {
                HolographicRemoting.Disconnect();
                connected = false;
            }
            else
                Connect();
            Debug.Log(button);

        }
        */

    }
}