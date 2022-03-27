using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public struct TransformCurves
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

        if (rx == null)
            rx = new AnimationCurve();

        if (ry == null)
            ry = new AnimationCurve();

        if (rz == null)
            rz = new AnimationCurve();
    }
}
#endif