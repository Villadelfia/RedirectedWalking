using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class TrickManager : MonoBehaviour {
    public GameObject Hallway;
    public GameObject Door1;
    public GameObject Door2;
    public GameObject Door3;
    public GameObject Door4;
    public GameObject Office1;
    public GameObject Office2;
    public GameObject Office3;
    public GameObject EndRoom;
    public String PortName = "COM1";
    GameObject _officeInstance;
    Transform _button;
    Transform _blinds;
    float _timeout = 1.5f;
    int _ctr;
    private SerialPort _port;
    private bool _triggered = false;

    // Use this for initialization
    void Start() {
        Screen.showCursor = false;
        _port = new SerialPort(PortName, 9600);
        _port.Open();
        Thread t = new Thread(GetControllerStatus);
        t.Start();
        _ctr = 0;
        _officeInstance = (GameObject)Instantiate(Office1, new Vector3(-2.5f, 0f, -1.5f), Quaternion.identity);
        _button = _officeInstance.transform.Find("button/button");
        _blinds = _officeInstance.transform.Find("blinds");
        StartCoroutine(RotateDoor(Door1, 0.3f, false));
    }

    // Update is called once per frame
    void Update() {
        _timeout -= Time.deltaTime;
        if((Input.GetButtonDown("Activate") || _triggered) && _timeout <= 0f) {
            _timeout = 1.5f;
            ++_ctr;
            switch(_ctr) {
                case 1: // Clicking button in room 1
                    StartCoroutine(MoveHallway1(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 2: // Closing door of room 1
                    StartCoroutine(RotateDoor(Door1));
                    StartCoroutine(RespawnRoom(0.6f));
                    StartCoroutine(RotateDoor(Door2, 0.6f, false));
                    break;
                case 3: // Clicking button in room 2
                    StartCoroutine(MoveHallway1(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 4: // Closing door of room 2
                    StartCoroutine(RotateDoor(Door2));
                    StartCoroutine(RespawnRoom(0.6f));
                    StartCoroutine(RotateDoor(Door3, 0.6f, false));
                    break;
                case 5: // Clicking button in room 3
                    StartCoroutine(MoveHallway1(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 6: // Closing door of room 3
                    StartCoroutine(RotateDoor(Door3));
                    StartCoroutine(RespawnRoom(0.6f, 0f, -1f, 0f, -1.6f));
                    StartCoroutine(RotateDoor(Door4, 0.6f, false));
                    break;
            }
        }
        _triggered = false;
    }

    void OnApplicationQuit() {
        _port.Close();
        Debug.Log("Quitting");
    }

    // Move the hallway 1.6m back.
    IEnumerator MoveHallway1(float x, float y, float z, float delay = 0) {
        yield return new WaitForSeconds(delay);
        Hallway.transform.Translate(x, y, z);
        yield return true;
    }

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

    IEnumerator RespawnRoom(float delay = 0f, float rot = 0f, float x = -2.5f, float y = 0f, float z = -1.5f) {
        yield return new WaitForSeconds(delay);
        Destroy(_officeInstance);

        switch(_ctr) {
            case 2:
                _officeInstance =
                    (GameObject)Instantiate(Office2, new Vector3(x, y, z), Quaternion.AngleAxis(rot, Vector3.up));
                _button = _officeInstance.transform.Find("button/button");
                _blinds = _officeInstance.transform.Find("blinds");
                break;
            case 4:
                _officeInstance =
                    (GameObject)Instantiate(Office3, new Vector3(x, y, z), Quaternion.AngleAxis(rot, Vector3.up));
                _button = _officeInstance.transform.Find("button/button");
                _blinds = _officeInstance.transform.Find("blinds");
                break;
            case 6:
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

    void GetControllerStatus() {
        while(true) {
            if(!_triggered) {
                _triggered = _port.ReadChar() == '1';
            }
            _port.DiscardInBuffer();
        }
    }
}
