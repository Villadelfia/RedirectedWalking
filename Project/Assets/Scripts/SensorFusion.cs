using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;

public class SensorFusion : MonoBehaviour {
    string optiTrackName = "OptiTrack.Tracker0";
    string oculusRiftName = "OculusRift.Tracker";
    string virtualTrackerName = "SensorFusion";
    private vrTracker optiTrack = null;
    private vrTracker virtualTracker = null;
    private vrTracker oculusRift = null;
    private float scaling = 1.1f;
    public float offsetX = -0.475f;
    public float offsetY = -0.4f;
    private bool initialized = true;

    private bool zeroSet = false;
    private vrVec3 zero;

    // Start
    void Start() {
        // Retrieve trackers by name
        optiTrack = MiddleVR.VRDeviceMgr.GetTracker(optiTrackName);
        oculusRift = MiddleVR.VRDeviceMgr.GetTracker(oculusRiftName);
        virtualTracker = MiddleVR.VRDeviceMgr.GetTracker(virtualTrackerName);

        if(optiTrack == null) {
            MiddleVRTools.Log("[X] SensorFusion: Error : Can't find tracker '" + optiTrackName + "'.");
            initialized = false;
        }

        if(oculusRift == null) {
            MiddleVRTools.Log("[X] SensorFusion: Error : Can't find tracker '" + oculusRiftName + "'.");
            initialized = false;
        }

        if(virtualTracker == null) {
            MiddleVRTools.Log("[X] SensorFusion: Error : Can't find tracker '" + virtualTrackerName + "'.");
            initialized = false;
        }

    }

    // Update is called once per frame
    void Update() {
        if(initialized == true) {
            // Calibrate/Reset calibration
            if(zeroSet == false || Input.GetButtonDown("Reset")) {
                if(zeroSet == true)
                    MiddleVRTools.Log("[>] SensorFusion: Resetting zero point.");
                zero = optiTrack.GetPosition();
                zeroSet = true;
            }

            // Position, offset from zero, with offset and scaling.
            vrVec3 pos = optiTrack.GetPosition();
            float x = (pos.x() - zero.x()) * scaling;
            float y = (pos.y() - zero.y()) * scaling;
            float z = pos.z();
            x += offsetX;
            y += offsetY;
            virtualTracker.SetX(x);
            virtualTracker.SetY(y);
            virtualTracker.SetZ(z);

            // Orientation
            virtualTracker.SetYaw(optiTrack.GetYaw());
            virtualTracker.SetPitch(oculusRift.GetPitch());
            virtualTracker.SetRoll(oculusRift.GetRoll());

        }
    }
}
