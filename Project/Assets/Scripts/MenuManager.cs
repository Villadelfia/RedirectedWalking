using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class MenuManager : MonoBehaviour {
    // External Settings
    public String PortName = "COM1";

    // Internals
    private SerialPort _port;
    private volatile bool _triggered = false;
    private float _timer = 0f;

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

        Camera.main.backgroundColor = Color.grey;
    }

    // Update is called once per frame
    void Update() {
        _timer -= Time.deltaTime;
        if(_timer < 0) {
            Camera.main.backgroundColor = Color.grey;
        }

        if(Input.GetButtonDown("Activate") || _triggered) {
            // Go from blue BG -> green BG -> load next scene.
            if(Camera.main.backgroundColor == Color.grey) {
                Camera.main.backgroundColor = Color.green * 0.5f;
                _timer = 1f;
            } else {
                EndScene();
            }
        }

        // Emergency stop
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Quit();
        }

        _triggered = false;
        Screen.showCursor = false;
    }

    // Cleanup
    void OnApplicationQuit() {
        Debug.Log("Cleaning up...");
        _port.Close();
    }

    // Ends the scene after 5 seconds.
    void EndScene() {
        Debug.Log("Ending scene in 20 seconds...");
        OnApplicationQuit();
        Application.LoadLevel("offices");
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

    void Quit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
