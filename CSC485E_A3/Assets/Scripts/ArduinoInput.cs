using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArduinoInput : MonoBehaviour
{
    [SerializeField] private SerialController serialController;

    [Header("Potentiometer")]
    [SerializeField] private float potentiometer_scale = 1;
    [SerializeField] private Vector2 potentiometer_extents = new Vector2(0, 1023);
    public float potentiometer_reading { get; private set; }

    [Header("Ultrasonic Sensor")]
    [SerializeField] private Vector2 ultrasonic_deadzone = new Vector2(40, 60);
    [SerializeField] private Vector2 ultrasonic_extents = new Vector2(1, 99);
    public float ultrasonic_reading { get; private set; }

    [Header("Capacitive Sensor")]
    [SerializeField] private float capacitive_threshold = 0.5f;
    [SerializeField] private UnityEvent<float> onCapacitivePress;
    [SerializeField] private UnityEvent<float> onCapacitiveRelease;
    public float capactive_reading { get; private set; }

    private void Reset()
    {
        serialController = FindObjectOfType<SerialController>();
    }

    private void OnValidate()
    {
        if (ultrasonic_deadzone.x <= 1)
            ultrasonic_deadzone.x = 2;

        if (ultrasonic_deadzone.y < ultrasonic_deadzone.x)
            ultrasonic_deadzone.y = ultrasonic_deadzone.x;

        if (ultrasonic_extents.x >= ultrasonic_deadzone.x)
            ultrasonic_extents.x = ultrasonic_deadzone.x - 1;

        if (ultrasonic_extents.y <= ultrasonic_deadzone.y)
            ultrasonic_extents.y = ultrasonic_deadzone.y + 1;
    }

    private void Update()
    {
        string message = serialController.ReadSerialMessage();

        if (message == null)
            return;

        string[] input = message.Split(',');

        if (input.Length < 2)
            return;

        int index = 0;

        HandlePotentiometer(input, ref index);
        HandleUltrasonic(input, ref index);
        HandleCapacitive(input, ref index);
    }

    public void HandleEditorInput(string[] input)
    {
        int index = 0;
        HandlePotentiometer(input, ref index);
        HandleUltrasonic(input, ref index);
        HandleCapacitive(input, ref index);
    }

    public void HandlePotentiometer(string[] input, ref int start_index)
    {
        if (float.TryParse(input[start_index], out float result))
            result = potentiometer_scale * Mathf.InverseLerp(potentiometer_extents.x, potentiometer_extents.y, result);
        else
            potentiometer_reading = 0;

        start_index++;
    }

    public void HandleUltrasonic(string[] input, ref int start_index)
    {
        if (float.TryParse(input[start_index], out float result))
        //x axis from joystick -> z axis in world
        {
            result = Mathf.InverseLerp(ultrasonic_extents.x, ultrasonic_extents.y, result);
            ultrasonic_reading = potentiometer_reading * result;
        }
        else
            ultrasonic_reading = 0;

        start_index++;
    }

    private void HandleCapacitive(string[] input, ref int start_index)
    {

        if (float.TryParse(input[start_index], out float result) == false)
            result = 0;


        if(capactive_reading < capacitive_threshold)
        {
            if (result > capacitive_threshold)
                onCapacitivePress.Invoke(ultrasonic_reading);
        }
        else
        {
            if (result < capacitive_threshold)
                onCapacitiveRelease.Invoke(ultrasonic_reading);
        }

        capactive_reading = result;
    }
}
