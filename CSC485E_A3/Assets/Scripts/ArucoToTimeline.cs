using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArucoUnity.Objects.Trackers;
using UnityEngine.Timeline;
using UnityEngine.Playables;

using UnityEditor.Timeline;
using UnityEditor;

[RequireComponent(typeof(PlayableDirector))]
public class ArucoToTimeline : MonoBehaviour
{
    [SerializeField] private ArucoObjectsTracker tracker;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private TimelineAsset timelineAsset;

    [SerializeField] private ArucoTrackDict arucoTracks;

    private Dictionary<AnimationClip, TransformCurves> clipCurves;

    private struct TransformCurves
    {
        public AnimationCurve px, py, pz, rx, ry, rz;
        public TransformCurves(AnimationClip clip)
        {
            px = null; py = null; pz = null; rx = null; ry = null; rz = null;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                switch (binding.propertyName)
                {
                    case "position.x":
                        px = AnimationUtility.GetEditorCurve(clip, binding);
                        break;

                    case "position.y":
                        py = AnimationUtility.GetEditorCurve(clip, binding);
                        break;

                    case "position.z":
                        pz = AnimationUtility.GetEditorCurve(clip, binding);
                        break;

                    case "rotation.x":
                        rx = AnimationUtility.GetEditorCurve(clip, binding);
                        break;

                    case "rotation.y":
                        ry = AnimationUtility.GetEditorCurve(clip, binding);
                        break;

                    case "rotation.z":
                        rz = AnimationUtility.GetEditorCurve(clip, binding);
                        break;
                }
            }

            if (px == null)
                px = new AnimationCurve();

            if (py == null)
                py = new AnimationCurve();
            
            if (pz == null)
                pz = new AnimationCurve();
            
            if(rx == null)
                rx = new AnimationCurve();

            if (ry == null)
                ry = new AnimationCurve();

            if (rz == null)
                rz = new AnimationCurve();
        }
    }

    private void Reset()
    {
        playableDirector = GetComponent<PlayableDirector>();
        tracker = FindObjectOfType<ArucoObjectsTracker>();
    }

    private void OnValidate()
    {
        if(playableDirector.playableAsset != null)
            timelineAsset = playableDirector.playableAsset as TimelineAsset;
    }

    private void Start()
    {
        clipCurves = new Dictionary<AnimationClip, TransformCurves>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            SetBindings();

        if (Input.GetKeyDown(KeyCode.I))
            CreateTracks();

        if (Input.GetKeyDown(KeyCode.Space))
            InsertKeyframes();
    }

    public void CreateTracks()
    {
        if (arucoTracks == null)
            arucoTracks = new ArucoTrackDict();

        Debug.Log($"Make tracks for  {tracker.ArucoObjects.Count} aruco objects");

        foreach (var category in tracker.ArucoObjects)
        {
            Debug.Log($"Create track for category: {category.Key.Name}");

            foreach (var obj in category.Value.Values)
            {
                Debug.Log($"Create track for obj: {obj.gameObject.name}");

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
                    track = timelineAsset.CreateTrack(typeof(AnimationTrack), null, obj.gameObject.name);
                    arucoTracks.Add(obj.gameObject.GetInstanceID(), track as AnimationTrack);
                }
            }
        }
    }

    private void SetBindings()
    {
        foreach (var category in tracker.ArucoObjects)
        {
            foreach (var obj in category.Value.Values)
            {
                if(arucoTracks.TryGetValue(obj.gameObject.GetInstanceID(), out var track))
                {
                    foreach (var output in timelineAsset.outputs)
                    {
                        if (output.streamName == track.name)
                        {
                            playableDirector.SetGenericBinding(output.sourceObject, obj.gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void InsertKeyframes()
    {
        foreach(var category in tracker.ArucoObjects)
        {
            foreach(var obj in category.Value.Values)
            {
                if(obj.gameObject.activeSelf)
                {
                    //insert keyframe
                    if(arucoTracks.TryGetValue(obj.gameObject.GetInstanceID(), out var track))
                    {
                        TimelineClip clip;
                        if (track.hasClips == false)
                        {
                            var anim_clip = new AnimationClip();
                            
                            if(AssetDatabase.IsValidFolder($"Assets/Clips/{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}") == false)
                                AssetDatabase.CreateFolder("Assets/Clips", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

                            AssetDatabase.CreateAsset(anim_clip, $"Assets/Clips/{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}/{obj.gameObject.name}.asset");

                            clip = track.CreateClip(new AnimationClip());
                            clip.animationClip.legacy = true;
                        }
                        else
                        {
                            var clips = track.GetClips().GetEnumerator();
                            clips.MoveNext();
                            clip = clips.Current;
                        }

                        InsertKeyframe(obj.gameObject, clip.animationClip);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
    }

    private void InsertKeyframe(GameObject obj, AnimationClip clip)
    {
        float time = (float)playableDirector.time;

        if(clipCurves.TryGetValue(clip, out var curves) == false)
        {
            curves = new TransformCurves(clip);
            clipCurves.Add(clip, curves);
        }

        //position
        curves.px.AddKey(new Keyframe(time, obj.gameObject.transform.position.y));
        clip.SetCurve(string.Empty, typeof(Transform), "position.x", curves.px);
         
        curves.py.AddKey(new Keyframe(time, obj.gameObject.transform.position.y));
        clip.SetCurve(string.Empty, typeof(Transform), "position.y", curves.py);

        curves.pz.AddKey(new Keyframe(time, obj.gameObject.transform.position.z));
        clip.SetCurve(string.Empty, typeof(Transform), "position.z", curves.pz);

        //rotation
        curves.rx.AddKey(new Keyframe(time, obj.gameObject.transform.position.y));
        clip.SetCurve(string.Empty, typeof(Transform), "rotation.x", curves.rx);

        curves.ry.AddKey(new Keyframe(time, obj.gameObject.transform.position.y));
        clip.SetCurve(string.Empty, typeof(Transform), "rotation.y", curves.ry);

        curves.rz.AddKey(new Keyframe(time, obj.gameObject.transform.position.z));
        clip.SetCurve(string.Empty, typeof(Transform), "rotation.z", curves.rz);
    }

}
