using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RawImage))]
public class CameraInput : MonoBehaviour
{
    [SerializeField] private RawImage image;
    public WebCamTexture webCamTexture { get; private set; }
    public WebCamDevice[] devices { get; private set; }

    private void Reset()
    {
        image = GetComponent<RawImage>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetDevices();
        image.texture = webCamTexture;
    }

    public void GetDevices()
    {
        devices = WebCamTexture.devices;
    }

    public void SetDevice(int index)
    {
        webCamTexture = new WebCamTexture(devices[index].name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraInput))]
public class CameraInput_Editor : Editor
{
    private CameraInput Target { get { return (CameraInput)target; } }
    private int device_index;
    private string[] device_names;

    public override VisualElement CreateInspectorGUI()
    {
        var vis = base.CreateInspectorGUI();
        
        Target.GetDevices();
        device_names = new string[Target.devices.Length];
        for (int i = 0; i < Target.devices.Length; i++)
        {
            device_names[i] = Target.devices[i].name;
            if (Target.webCamTexture != null && Target.webCamTexture.deviceName == Target.devices[i].name)
                device_index = i;
        }

        if (Target.webCamTexture == null && Target.devices.Length > 0)
        {
            Target.SetDevice(0);
            device_index = 0;
        }

        return vis;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(Target.devices.Length > 0)
        device_index = EditorGUILayout.Popup("Current Device" , device_index, device_names);
    }
}
#endif