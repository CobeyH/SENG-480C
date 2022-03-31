using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArucoUnity.Objects.Trackers;
using UnityEngine.Timeline;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor.Timeline;
using UnityEditor;

[RequireComponent(typeof(PlayableDirector))]
public class ArucoToTimeline : MonoBehaviour
{
    [SerializeField] private ArucoObjectsTracker tracker;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private TimelineAsset timelineAsset;

    [SerializeField] private ArucoTrackDict arucoTracks;
    [SerializeField] private double timestep = 1.0 / 60.0;

    [SerializeField] private ArduinoInput arduinoInput;

    private Dictionary<AnimationClip, TransformCurves> clipCurves;
    [SerializeField, HideInInspector] private double timeline_zero = 0.0000;

    private void Reset()
    {
        tracker = FindObjectOfType<ArucoObjectsTracker>();
        playableDirector = GetComponent<PlayableDirector>();

        if (playableDirector != null && playableDirector.playableAsset != null)
            timelineAsset = playableDirector.playableAsset as TimelineAsset;
    }

    private void Start()
    {
        clipCurves = new Dictionary<AnimationClip, TransformCurves>();
    }

    // Update is called once per frame
    void Update()
    {
        //todo: replace with hardware button
        if (Input.GetKeyDown(KeyCode.K))
        {
            InsertKeyframes();
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            timeline_zero = playableDirector.time;
        }

        //todo: replace with ultrasonic
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            playableDirector.time -= timestep;
            
            if (playableDirector.time < 0)
                playableDirector.time = 0;

            TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            playableDirector.time += timestep;
            TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
        }

        if(arduinoInput.held)
        {
            UpdateTimeline(arduinoInput.ultrasonic_reading);
        }
    }

    public void UpdateTimeline(float normalized_position)
    {
        //double new_time = (double)Mathf.InverseLerp(0, (float)playableDirector.duration, normalized_position);
        //Debug.Log($"Update timeline: {new_time} << {playableDirector.duration}, {normalized_position}");
        
        playableDirector.time = playableDirector.duration * normalized_position + timeline_zero;
        TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
    }

    public void CreateTracks()
    {
        if (arucoTracks == null)
            arucoTracks = new ArucoTrackDict();

        Debug.Log($"Make tracks for  {tracker.arucoObjectsToDetect.Length} aruco objects");

        for (int i = 0; i < tracker.arucoObjectsToDetect.Length; i++)
        {
            var obj = tracker.arucoObjectsToDetect[i];

            if (arucoTracks.ContainsKey(obj.gameObject.GetInstanceID()))
            {
                if (arucoTracks[obj.gameObject.GetInstanceID()] != null)
                    continue;
                else
                    arucoTracks.Remove(obj.gameObject.GetInstanceID());
            }

            TrackAsset track = null;
            foreach (var t in timelineAsset.GetRootTracks())
            {
                if (t.name == obj.gameObject.name)
                {
                    Debug.Log($"Track exists for {t.name}");
                    arucoTracks.Add(obj.gameObject.GetInstanceID(), t as AnimationTrack);
                    track = t;
                    break;
                }
            }

            if (track == null)
            {
                string[] split = obj.gameObject.name.Split('_');
            
                track = timelineAsset.CreateTrack(typeof(AnimationTrack), null, $"{split[2]}_{split[3]}");
                arucoTracks.Add(obj.gameObject.GetInstanceID(), track as AnimationTrack);
            }
        }

        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
    }

    public void SetBindings()
    {
        for (int i = 0; i < tracker.arucoObjectsToDetect.Length; i++)
        {
            if (arucoTracks.TryGetValue(tracker.arucoObjectsToDetect[i].gameObject.GetInstanceID(), out var track))
            {
                foreach (var output in timelineAsset.outputs)
                {
                    if (output.streamName == track.name)
                    {
                        playableDirector.SetGenericBinding(output.sourceObject, tracker.arucoObjectsToDetect[i].gameObject);
                        break;
                    }
                }
            }
        }

        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }

    public void InsertKeyframes()
    {
        for (int i = 0; i < tracker.arucoObjectsToDetect.Length; i++)
        {
            var obj = tracker.arucoObjectsToDetect[i];
            if (obj.gameObject.activeSelf)
            {
                //insert keyframe
                if (arucoTracks.TryGetValue(obj.gameObject.GetInstanceID(), out var track))
                {
                    TimelineClip clip;
                    if (track.hasClips == false)
                    {
                        clip = track.CreateClip(new AnimationClip());
                        clip.displayName = track.name;
                        
                        if (AssetDatabase.IsValidFolder($"Assets/Clips/{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}") == false)
                            AssetDatabase.CreateFolder("Assets/Clips", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

                        AssetDatabase.CreateAsset(clip.animationClip, $"Assets/Clips/{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}/{obj.gameObject.name}.asset");
                    }
                    else
                    {
                        var clips = track.GetClips().GetEnumerator();
                        clips.MoveNext();
                        clip = clips.Current;
                    }

                    InsertKeyframe(obj.gameObject, clip.animationClip);

                    //EditorUtility.SetDirty(clip.animationClip);
                    EditorUtility.SetDirty(track);
                }
            }
        }

        EditorUtility.SetDirty(timelineAsset);
        AssetDatabase.SaveAssets();
    }

    private void InsertKeyframe(GameObject obj, AnimationClip clip)
    {
        float time = (float)playableDirector.time;

        if (clipCurves.TryGetValue(clip, out var curves) == false)
        {
            curves = new TransformCurves(clip);
            clipCurves.Add(clip, curves);
        }

        UpdateClipCurve(clip, curves.px, "localPosition.x", time, obj.transform.localPosition.x);
        UpdateClipCurve(clip, curves.pz, "localPosition.z", time, obj.transform.localPosition.z);
        UpdateClipCurve(clip, curves.ry, "localRotation.y", time, obj.transform.localRotation.y);
    }

    private void UpdateClipCurve(AnimationClip clip, AnimationCurve curve, string property, float time, float value)
    {
        int index = curve.AddKey(new Keyframe(time, value));
        if (index == 0 && time > 0)
            curve.AddKey(new Keyframe(0, value));
        else if (curve.keys[index - 1].value == curve.keys[index].value)
        {
            curve.RemoveKey(index);
            return;
        }

        clip.SetCurve(string.Empty, typeof(Transform), property, curve);
    }

}

[CustomEditor(typeof(ArucoToTimeline))]
public class ArucoToTimeline_Editor : Editor
{
    ArucoToTimeline Target { get { return (ArucoToTimeline)target; } }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create Tracks"))
            Target.CreateTracks();

        if (GUILayout.Button("Set Bindings"))
            Target.SetBindings();
    }
}
#endif