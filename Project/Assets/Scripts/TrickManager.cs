using UnityEngine;
using System.Collections;

public class TrickManager : MonoBehaviour {
    public GameObject hallway;
    public GameObject door1;
    public GameObject door2;
    public GameObject door3;
    public GameObject door4;
    public GameObject door5;
    public GameObject door6;
    public GameObject office;
    GameObject officeInstance;
    Transform button;
    Transform blinds;
    float timeout = 1.5f;

    int ctr;

    // Use this for initialization
    void Start() {
        Screen.showCursor = false;
        ctr = 0;
        officeInstance = (GameObject)Instantiate(office, new Vector3(-2.5f, 0f, -1.5f), Quaternion.identity);
        button = officeInstance.transform.Find("button/button");
        blinds = officeInstance.transform.Find("blinds");
        StartCoroutine(RotateDoor(door1, 0.3f, false));
    }

    // Update is called once per frame
    void Update() {
        timeout -= Time.deltaTime;
        if(Input.GetButtonDown("Activate") && timeout <= 0f) {
            timeout = 1.5f;
            ++ctr;
            switch(ctr) {
                case 1: // Clicking button in room 1
                    StartCoroutine(MoveHallway1(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 2: // Closing door of room 1
                    StartCoroutine(RotateDoor(door1));
                    StartCoroutine(RespawnRoom(0.6f));
                    StartCoroutine(RotateDoor(door2, 0.6f, false));
                    break;
                case 3: // Clicking button in room 2
                    StartCoroutine(MoveHallway1(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 4: // Closing door of room 2
                    StartCoroutine(RotateDoor(door2));
                    StartCoroutine(RespawnRoom(0.6f));
                    StartCoroutine(RotateDoor(door3, 0.6f, false));
                    break;
                case 5: // Clicking button in room 3
                    StartCoroutine(MoveHallway1(0f, 0f, 1.6f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton());
                    break;
                case 6: // Closing door of room 3
                    StartCoroutine(RotateDoor(door3));
                    StartCoroutine(RespawnRoom(0.6f, -90f, 0.55f, 0f, -2.425f));
                    StartCoroutine(RotateDoor(door4, 0.6f, false));
                    break;
                case 7: // Clicking button in room 4
                    StartCoroutine(MoveHallway1(-1.6f, 0f, 0f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton(0f, true));
                    break;
                case 8: // Closing door of room 4
                    StartCoroutine(RotateDoor(door4));
                    StartCoroutine(RespawnRoom(0.6f, -90f, 0.55f, 0f, -2.425f));
                    StartCoroutine(RotateDoor(door5, 0.6f, false));
                    break;
                case 9: // Clicking button in room 5
                    StartCoroutine(MoveHallway1(-1.6f, 0f, 0f));
                    StartCoroutine(DescendBlinds());
                    StartCoroutine(DepressButton(0f, true));
                    break;
                case 10: // Closing door of room 5
                    StartCoroutine(RotateDoor(door5));
                    StartCoroutine(RotateDoor(door6, 0.6f, false));
                    break;
                default:
                    break;
            }
        }
    }

    // Move the hallway 1.6m back.
    IEnumerator MoveHallway1(float x, float y, float z, float delay = 0) {
        yield return new WaitForSeconds(delay);
        hallway.transform.Translate(x, y, z);
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
        if(office != null)
            Destroy(officeInstance);
        officeInstance = (GameObject)Instantiate(office, new Vector3(x, y, z), Quaternion.AngleAxis(rot, Vector3.up));
        button = officeInstance.transform.Find("button/button");
        blinds = officeInstance.transform.Find("blinds");
        yield return true;
    }

    IEnumerator DepressButton(float delay = 0f, bool isRotated = false) {
        yield return new WaitForSeconds(delay);
        Vector3 begin = button.transform.position;
        Vector3 end = begin + (isRotated ? new Vector3(0.03f, 0f, 0f) : new Vector3(0f, 0f, -0.03f));

        for(float i = 0f; i < 0.25f; i += Time.deltaTime) {
            button.transform.position = Vector3.Slerp(begin, end, i / 0.25f);
            yield return true;
        }

        button.transform.position = end;
        yield return true;
    }

    IEnumerator DescendBlinds(float delay = 0f) {
        yield return new WaitForSeconds(delay);
        Vector3 begin = blinds.transform.position;
        Vector3 end = begin - new Vector3(0f, 0.995f, 0f);

        for(float i = 0f; i < 1.5f; i += Time.deltaTime) {
            blinds.transform.position = Vector3.Slerp(begin, end, i / 1.5f);
            yield return true;
        }

        blinds.transform.position = end;
        yield return true;
    }

}
