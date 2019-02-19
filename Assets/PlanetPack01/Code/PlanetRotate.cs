using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class PlanetRotate : MonoBehaviour
{
    public static float GlobalSpeedup = 1.0f;

    public Vector3 InitialOrientation = Vector3.zero;
    public Vector3 RotationOrientation = Vector3.zero;

    private Vector3 CurrentOrientation = Vector3.zero;

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        EditorUtility.SetSelectedWireframeHidden(GetComponent<Renderer>(), true);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        CurrentOrientation += RotationOrientation * Time.deltaTime * GlobalSpeedup;
        transform.localRotation = Quaternion.Euler(InitialOrientation) * Quaternion.Euler(CurrentOrientation);
    }
}
