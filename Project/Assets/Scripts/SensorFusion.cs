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
    private float yawZeroOpti = 0;
    private float prevYaw = 0;
    private bool firstFrame = true;

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
                yawZeroOpti = optiTrack.GetYaw();
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
            virtualTracker.SetYaw(getModifiedYaw());
            virtualTracker.SetPitch(oculusRift.GetPitch());
            virtualTracker.SetRoll(oculusRift.GetRoll());

            firstFrame = false;
        }
    }

    private float getModifiedYaw() {
        // Get yaw from optitrack tracker.
        float a = optiTrack.GetYaw() - yawZeroOpti;
        a = normAngle(a);

        // If not first frame, take avg of last 2 measurements.
        if(!firstFrame) {
            if(Mathf.Abs(a - prevYaw) > 180.0f) {
                if(a > prevYaw) {
                    prevYaw += 360.0f;
                } else {
                    a += 360.0f;
                }
            }
            a = (a + prevYaw) / 2.0f;
        }
        a = normAngle(a);
        prevYaw = a;

        return a - 180.0f;
    }

    private float normAngle(float a) {
        while(a < 0) {
            a += 360;
        }

        while(a >= 360) {
            a -= 360;
        }

        return a;
    }
}
