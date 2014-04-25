using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class TrickManager : MonoBehaviour {
    // External Settings
    public GameObject Hallway;
    public GameObject Door1;
    public GameObject Door2;
    public GameObject Door3;
    public GameObject Door4;
    public GameObject Office1;
    public GameObject Office2;
    public GameObject Office3;
    public Light Light1;
    public Light Light2;
    public Light Light3;
    public GameObject EndRoom;
    public Material EndRoomMaterial;
    public String PortName = "COM1";

    // Internals
    private GameObject _officeInstance;
    private Transform _button;
    private Transform _blinds;
    private float _timeout = 1.5f;
    private int _ctr = 1;
    private SerialPort _port;
    private volatile bool _triggered = false;
    private bool _ended = false;

    // Initialization
    void Start() {
        // Open the SerialPort to the controller
        _port = new SerialPort(PortName, 9600);
        try {
            _port.Open();
        } catch(IOException) {
            // Ignore, we can run without a controller.
            Debug.Log("Controller not connected.");
        }

        // Start the Serial polling thread.
        Thread t = new Thread(GetControllerStatus);
        t.Start();

        // Init the first room and find the needed opbjects within
        _officeInstance = (GameObject)Instantiate(Office1, new Vector3(-2.5f, 0f, -1.5f), Quaternion.identity);
        _button = _officeInstance.transform.Find("button/button");
        _blinds = _officeInstance.transform.Find("blinds");

        // Open the first door
        StartCoroutine(RotateDoor(Door1, 0.3f, false));
    }

    // Update is called once per frame
    void Update() {
        if(_ended)
            return;

        // Decrement the delay between actions
        _timeout -= Time.deltaTime;

        // Check if a transition is requested, and that it is allowed by position, view and timeout
        if((Input.GetButtonDown("Activate") || _triggered) && _timeout <= 0f && CanTriggerNextStep()) {
            // Reset the timeout
            _timeout = 1.5f;

            // Make the animation happen based on the active step
            switch(_ctr) {
                case 1: // Clicking button in room 1
                    StartCoroutine(MoveHallway(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 2: // Closing door of room 1
                    StartCoroutine(RotateDoor(Door1));
                    StartCoroutine(RespawnRoom(0.6f));
                    StartCoroutine(RotateDoor(Door2, 0.6f, false));
                    break;
                case 3: // Clicking button in room 2
                    StartCoroutine(MoveHallway(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 4: // Closing door of room 2
                    StartCoroutine(RotateDoor(Door2));
                    StartCoroutine(RespawnRoom(0.6f));
                    StartCoroutine(RotateDoor(Door3, 0.6f, false));
                    break;
                case 5: // Clicking button in room 3
                    StartCoroutine(MoveHallway(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 6: // Closing door of room 3
                    StartCoroutine(RotateDoor(Door3));
                    StartCoroutine(RespawnRoom(0.6f, 0f, -1f, 0f, -1.6f));
                    StartCoroutine(RotateDoor(Door4, 0.6f, false));
                    break;
            }

            // Increment the "step" of the process
            ++_ctr;
        }

        // After the last door is opened, and the endroom has been entered.
        if(_ctr == 7 && InView(new Vector3(-2f, 0f, -2f)) && Distance(new Vector3(-2f, 0f, -2f)) <= 1f) {
            StartCoroutine(EndScene());
            _ended = true;
        }

        // Emergency stop
        if(Input.GetKeyDown(KeyCode.Escape)) {
            OnApplicationQuit();
            Application.LoadLevel("empty");
        }

        // Reset the controller input
        _triggered = false;
    }

    // Cleanup
    void OnApplicationQuit() {
        Debug.Log("Cleaning up...");
        _port.Close();
        EndRoomMaterial.color = Color.white;
    }

    // Move the hallway 1.6m back.
    IEnumerator MoveHallway(float x, float y, float z, float delay = 0) {
        yield return new WaitForSeconds(delay);
        Hallway.transform.Translate(x, y, z);
        yield return true;
    }

    // Open or close a door, meaning a 90 degree rotation in a certain direction
    IEnumerator RotateDoor(GameObject door, float delay = 0f, bool close = true) {
        yield return new WaitForSeconds(delay);

        float angle = door.transform.rotation.eulerAngles.y;
        float goal = close ? angle - 90 : angle + 90;
        Quaternion end = Quaternion.AngleAxis(goal, Vector3.up);
        Quaternion begin = door.transform.rotation;

        for(float i = 0f; i < 0.5f; i += Time.deltaTime) {
            door.transform.rotation = Quaternion.Slerp(begin, end, i / 0.5f);
            yield return true;
        }

        door.transform.rotation = end;
        yield return true;
    }

    // Destroy and replace the existing office
    IEnumerator RespawnRoom(float delay = 0f, float rot = 0f, float x = -2.5f, float y = 0f, float z = -1.5f) {
        yield return new WaitForSeconds(delay);
        Destroy(_officeInstance);

        switch(_ctr) {
            case 3:
                _officeInstance =
                    (GameObject)Instantiate(Office2, new Vector3(x, y, z), Quaternion.AngleAxis(rot, Vector3.up));
                _button = _officeInstance.transform.Find("button/button");
                _blinds = _officeInstance.transform.Find("blinds");
                break;
            case 5:
                _officeInstance =
                    (GameObject)Instantiate(Office3, new Vector3(x, y, z), Quaternion.AngleAxis(rot, Vector3.up));
                _button = _officeInstance.transform.Find("button/button");
                _blinds = _officeInstance.transform.Find("blinds");
                break;
            case 7:
                _officeInstance =
                    (GameObject)Instantiate(EndRoom, new Vector3(x, y, z), Quaternion.AngleAxis(rot, Vector3.up));
                _button = null;
                _blinds = null;
                break;
            default:
                _officeInstance =
                    (GameObject)Instantiate(Office1, new Vector3(x, y, z), Quaternion.AngleAxis(rot, Vector3.up));
                _button = _officeInstance.transform.Find("button/button");
                _blinds = _officeInstance.transform.Find("blinds");
                break;
        }
        yield return true;
    }

    // Play the animation for pressing the button
    IEnumerator DepressButton(float delay = 0f, bool isRotated = false) {
        yield return new WaitForSeconds(delay);
        Vector3 begin = _button.transform.position;
        Vector3 end = begin + (isRotated ? new Vector3(0.03f, 0f, 0f) : new Vector3(0f, 0f, -0.03f));

        for(float i = 0f; i < 0.25f; i += Time.deltaTime) {
            _button.transform.position = Vector3.Slerp(begin, end, i / 0.25f);
            yield return true;
        }

        _button.transform.position = end;
        yield return true;
    }

    // Play the animation for closing the blinds
    IEnumerator DescendBlinds(float delay = 0f) {
        yield return new WaitForSeconds(delay);
        Vector3 begin = _blinds.transform.position;
        Vector3 end = begin - new Vector3(0f, 0.995f, 0f);

        for(float i = 0f; i < 1.5f; i += Time.deltaTime) {
            _blinds.transform.position = Vector3.Slerp(begin, end, i / 1.5f);
            yield return true;
        }

        _blinds.transform.position = end;
        yield return true;
    }

    // Ends the scene after 5 seconds
    IEnumerator EndScene() {
        Debug.Log("Ending scene in 20 seconds...");
        StartCoroutine(DimLights());
        yield return new WaitForSeconds(20f);
        OnApplicationQuit();
        Application.LoadLevel("empty");
        yield return true;
    }

    // Turns off the lights
    IEnumerator DimLights() {
        float begin1 = Light1.intensity;
        float begin2 = Light2.intensity;
        float begin3 = Light3.intensity;
        float colVal = 1f;

        for(float i = 0f; i < 5f; i += Time.deltaTime) {
            Light1.intensity = Mathf.Lerp(begin1, 0, i / 5f);
            Light2.intensity = Mathf.Lerp(begin2, 0, i / 5f);
            Light3.intensity = Mathf.Lerp(begin3, 0, i / 5f);
            colVal = Mathf.Lerp(1f, 0f, i / 5f);
            EndRoomMaterial.color = new Color(colVal, colVal, colVal);
            yield return true;
        }
        yield return true;
    }

    // Set the triggered state if the button is pressed on the controller
    void GetControllerStatus() {
        if(!_port.IsOpen)
            return;
        while(true) {
            _triggered = _port.ReadChar() == '1';
            _port.DiscardInBuffer();
        }
    }

    // Returns the distance on the XZ plane of our position to a point
    float Distance(Vector3 pos) {
        Vector2 v1 = new Vector2(transform.position.x, transform.position.z);
        Vector2 v2 = new Vector2(pos.x, pos.z);
        return Vector2.Distance(v1, v2);
    }

    // Is Vector3 in view
    bool InView(Vector3 v) {
        v.y = Camera.main.transform.position.y;
        Vector3 vis = Camera.main.WorldToViewportPoint(v);
        if((vis.x < 0 || vis.x > 1) || vis.z <= 0) {
            return false;
        }
        return true;
    }

    // Checks the distance to the next object to activate, and if it is visible
    bool CanTriggerNextStep() {
        Vector3 doorPosition = new Vector3(-1f, 0f, -0.4f);
        Vector3 buttonPosition = new Vector3(-4f, 0f, -3f);
        switch (_ctr) {
            case 1:
            case 3:
            case 5:
                // Is the "button" visible and within 1 meter
                if(InView(buttonPosition) && Distance(buttonPosition) <= 1f) {
                    return true;
                }
                break;
            case 2:
            case 4:
            case 6:
                // Is door1 visible and within 1 meter
                if(InView(doorPosition) && Distance(doorPosition) <= 1f) {
                    return true;
                }
                break;
        }
        return false;
    }
}
