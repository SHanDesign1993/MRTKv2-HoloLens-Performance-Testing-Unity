using Microsoft.MixedReality.Toolkit.Core.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.Core.Utilities.Physics;
using Microsoft.MixedReality.Toolkit.Core.Devices.UnityInput;
using MMFrame.Windows.GlobalHook;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Devices.UnityInput
{
    public class MouseSharingManager : MonoBehaviour
    {
        public static MouseSharingManager Instance = null;
        public bool MouseShared = false;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (MouseHook.IsHooking)
            {
                return;
            }
            MouseHook.AddEvent(MouseMonitor);
            MouseHook.Start();
            //MouseHook.Disable();
        }

        void MouseMonitor(ref MMFrame.Windows.GlobalHook.MouseHook.StateMouse s)
        {
            Debug.Log(s.X + ", " + s.Y);
            //Debug.Log(s.Stroke);

            // Bail early if our mouse isn't in our game window.
            if (s.X <= 0 ||
                s.Y <= 0 ||
                s.X >= Screen.width ||
                 s.Y >= Screen.height)
            {
                MouseShared = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                MouseShared = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            /*
             var x = Input.GetAxis（“Mouse X”）;
            var y = Input.GetAxis（“Mouse Y”）;
            var lButton = Input.GetMouseButton（0）;
            var rButton = Input.GetMouseButton（1）;*/
        }

        private void OnApplicationQuit()
        {
            if (MouseHook.IsHooking)
            {
                MouseHook.Stop();
            }
        }
    }
}

