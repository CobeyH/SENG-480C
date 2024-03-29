using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class EditorInputListener
{
    static bool updating = false;
    public static SerialController serialController;
    public static ArduinoInput arduinoInput;

    static EditorInputListener()
    {
        System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);

        value += EditorGlobalKeyPress;

        info.SetValue(null, value);
    }

    static void EditorGlobalKeyPress()
    {
        if(Event.current.keyCode == KeyCode.I && Event.current.type == EventType.KeyDown)
        {
            if(updating)
            {
                Debug.Log("Stop update");
                serialController.OnDisable();
                EditorApplication.update -= Update;
                updating = false;
            }
            else
            {
                Debug.Log("Start update");
                if (serialController == null)
                {
                    serialController = Editor.FindObjectOfType<SerialController>();

                    if (serialController == null)
                    {
                        Debug.LogError("SerialController is not assigned"); 
                        return;
                    }
                }

                if (arduinoInput == null)
                {
                    arduinoInput = Editor.FindObjectOfType<ArduinoInput>();

                    if(arduinoInput == null)
                    {
                        Debug.LogError("MotionListener is not assigned");
                        return;
                    }
                }

                serialController.OnEnable();
                EditorApplication.update += Update;
                updating = true;
            }
        }
    }

    static void Update()
    {
        string message = serialController.ReadSerialMessage();

        if (message == null)
            return;

        Debug.Log(message);

        string[] input = message.Split(',');

        if (input.Length < 3)
            return;

        arduinoInput.HandleEditorInput(input);
    }
}