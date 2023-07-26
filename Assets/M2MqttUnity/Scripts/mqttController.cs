"""
This script defines an mqttController responsible for managing the interactions between an MQTTReceiver and a 3D sphere object in the scene.
The primary functions include receiving, validating, processing and publishing MQTT messages, and moving the sphere object based on the received data.

Primary methods:
    Start(): Initializes the script, finding and assigning references to the MQTT receiver and sphere object.
    OnMessageArrivedHandler(string newMsg): Validates and processes new MQTT messages. If the message is in the correct format and passes the debounce check, it will move the sphere and publish the move.
    MoveSphere(string message): Parses the MQTT message into a Vector3 and moves the sphere based on the received coordinates.
    Publish(): Constructs and publishes a message detailing the sphere's movement to "information" and "movement" topics.
    ParseMessageToVector3(string message): Converts an MQTT message string into a Vector3 object, representing a coordinate in 3D space.

The script has a debounce mechanism to ensure that messages received within a short time frame (0.1s) from each other are not processed more than once, potentially preventing redundant operations.
If the message format is invalid or if certain components are not found in the scene, the script logs an error message in the Unity Editor console.
"""

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class mqttController : MonoBehaviour
{
    public string nameController = "Controller 1";
    // public string tagOfTheMQTTReceiver = "Receiver";
    public string sphereTag = "Sphere"; 
    public mqttReceiver _eventSender;
    public GameObject sphereObject;

    private Vector3 oldPos = Vector3.zero, newPos = Vector3.zero, movement = Vector3.zero;

    // Sanity check to avoid unwanted echo
    private float previousTimestamp = 0.0f;
    // 0.1 second threshold
    private float debounceTime = 0.1f;

    private Transform sphereTransform;

    private void Start()
    {
        // _eventSender = GameObject.FindWithTag(tagOfTheMQTTReceiver)?.GetComponent<mqttReceiver>();
        _eventSender = FindObjectOfType<mqttReceiver>();
        if (_eventSender == null) 
        {
            Debug.LogError("mqttReceiver component not found in the scene.");
            return;
        }
        _eventSender.OnMessageArrived += OnMessageArrivedHandler;

        if (sphereObject != null)
        {
            sphereTransform = sphereObject.transform;
        }
    }

    private void OnMessageArrivedHandler(string newMsg)
    {
        // Check if message is in the expected format
        if (!Regex.IsMatch(newMsg, @"\(\s*\+?-?\d*\.?\d*\s*,\s*\+?-?\d*\.?\d*\s*,\s*\+?-?\d*\.?\d*\s*\)"))
        {
            Debug.LogError("Invalid message format: " + newMsg);
        }
        else {
            float currentTimestamp = Time.time;
            // If two messages are received within 0.1 second, only the first one will be processed
            // This is to make sure if other relays in the scene are echoing the message, only the first one will be processed
            if ((currentTimestamp - previousTimestamp >= debounceTime))
            {
                MoveSphere(newMsg);
                Publish();
            }
            previousTimestamp = currentTimestamp;
        }
    }

    private void MoveSphere(string message)
    {
        movement = ParseMessageToVector3(message);

        if (sphereTransform != null)
        {
            oldPos = sphereTransform.position;
            sphereTransform.position += movement;
        }
    }

    private void Publish()
    {
        if (sphereTransform != null)
        {
            // Publish happens after the movement
            newPos = sphereTransform.position;
            string payload = string.Format("I have moved from (X: {0}, Y: {1}, Z: {2}) to (X: {3}, Y: {4}, Z: {5})",
                oldPos.x, oldPos.y, oldPos.z,
                newPos.x, newPos.y, newPos.z);
            // Debug.Log("Publish debug, payload: " + payload);

            // Access the mqttReceiver component and publish the information message
            if (_eventSender != null)
            {
                _eventSender.messagePublish = payload;

                _eventSender.topicPublish = movement.ToString();
                _eventSender.Publish();

                _eventSender.topicPublish = "information";
                _eventSender.Publish();
            }
        }
    }

    private Vector3 ParseMessageToVector3(string message)
    {
        string[] coordinates = message.Trim('(', ')').Split(',');
        
        // Check the length of the array to avoid index out of range exception
        if (coordinates.Length != 3)
        {
            Debug.LogError("Invalid message format: " + message);
            return Vector3.zero;
        }

        // If a coordinate value is missing, replace it with "0"
        for (int i = 0; i < coordinates.Length; i++)
        {
            if (string.IsNullOrEmpty(coordinates[i]))
            {
                coordinates[i] = "0";
            }
        }

        float x = float.Parse(coordinates[0]);
        float y = float.Parse(coordinates[1]);
        float z = float.Parse(coordinates[2]);

        return new Vector3(x, y, z);
    }
}
