using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace say_sShop.QvPenSummoner
{
    public class PenTeleporter : UdonSharpBehaviour
    {
        [Header("【ここは触らないでね】")]
        public PenArrayManager PenArrayManager;
        public SummonAccessController SummonAccessController;

        float _requiredDownTime = 0.5f;
        float _downInputStartTime;
        bool _isVR;
        bool _hasTeleported;
        Vector3 _penTeleportPos;
        Vector3[] _initialPenPos;
        VRCPlayerApi _localPlayer;

        void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            _isVR = _localPlayer.IsUserInVR();
            _initialPenPos = new Vector3[PenArrayManager.PenArray.Length];
            for (int i = 0; i < PenArrayManager.PenArray.Length; i++)
            {
                _initialPenPos[i] = PenArrayManager.PenArray[i].transform.position;
            }
        }
        public bool CanTeleport()
        {
            return SummonAccessController.IsUserAllowed();
        }
        
        void Update()
        {
            if (!CanTeleport())
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (_isVR)
            {
                HandleViveVRInput();
            }
            else
            {
                HandleDesktopInput();
            }
        }
        
        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (_isVR)
            {
                HandleVRInput(value);
            }
        }
        
        void HandleViveVRInput()
        {
            float trackpadVertical = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");

            if (Mathf.Abs(trackpadVertical) >= 0.8f)
            {
                HandleVRInput(trackpadVertical);
            }
            else
            {
                ResetTeleportFlags();
            }
        }
        
        void HandleDesktopInput()
        {
            if (Input.GetKey(KeyCode.P))
            {
                ProcessTeleportInput();
            }
            else
            {
                ResetTeleportFlags();
            }
        }

        void HandleVRInput(float value)
        {
            _penTeleportPos = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            if (value <= -0.8f)
            {
                ProcessTeleportInput();
            }
            else
            {
                ResetTeleportFlags();
            }
        }

        void ProcessTeleportInput()
        {
            if (_downInputStartTime == 0f)
            {
                _downInputStartTime = Time.time;
            }
            else if (Time.time - _downInputStartTime >= _requiredDownTime && !_hasTeleported)
            {
                TeleportPen();
                _hasTeleported = true;
            }
        }

        void ResetTeleportFlags()
        {
            _hasTeleported = false;
            _downInputStartTime = 0f;
        }

        void TeleportPen()
        {
            Quaternion penRotation;

            if (_isVR)
            {
                _penTeleportPos = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            }
            else
            {
                _penTeleportPos = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                _penTeleportPos += _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward * 0.5f;
            }
    
            float yRotation = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation.eulerAngles.y;
            penRotation = Quaternion.Euler(0, yRotation, 0);
    
            if (SummonAccessController.AccessLimited && SummonAccessController.IsUserAllowed())
            {
                GameObject exclusiveObject = SummonAccessController.GetExclusiveObjectForUser(_localPlayer.displayName);
                if (exclusiveObject != null)
                {
                    exclusiveObject.transform.position = _penTeleportPos;
                    exclusiveObject.transform.rotation = penRotation;
                }
            }
            else
            {
                TeleportAllPens(penRotation);
            }
        }

        void TeleportAllPens(Quaternion penRotation)
        {
            VRCPlayerApi penOwner;
            foreach (GameObject pen in PenArrayManager.PenArray)
            {
                penOwner = GetPenOwner(pen);
                if (penOwner == _localPlayer)
                {
                    pen.transform.position = _penTeleportPos;
                    pen.transform.rotation = penRotation;
                    return;
                }
            }

            for (int i = 0; i < PenArrayManager.PenArray.Length; i++)
            {
                if (!CheckPenMoved(i))
                {
                    if (!Networking.LocalPlayer.IsOwner(PenArrayManager.PenArray[i].gameObject))
                    {
                        Networking.SetOwner(Networking.LocalPlayer, PenArrayManager.PenArray[i].gameObject);
                    }
    
                    PenArrayManager.PenArray[i].transform.position = _penTeleportPos;
                    PenArrayManager.PenArray[i].transform.rotation = penRotation;
                    break;
                }
            }
        }


        VRCPlayerApi GetPenOwner(GameObject pen)
        {
            return Networking.GetOwner(pen);
        }

        bool CheckPenMoved(int penIndex)
        {
            return Vector3.Distance(_initialPenPos[penIndex], PenArrayManager.PenArray[penIndex].transform.position) > 0.01f;
        }
    }
}