using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public float RotateSpeed = 40f;
    public LayerMask TargetMask;
    public Color DotHightlightColor;
    public Color DotColor;
    public SpriteRenderer Dot;
    public bool HideCursor = true;

    Color _originalDotColor;

    void Start()
    {
        _originalDotColor = DotColor;
        Cursor.visible = !HideCursor;
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * RotateSpeed * Time.deltaTime);
    }

    public void DetectTargets(Ray ray)
    {

        var hitsSomethingInTargetMask = Physics.Raycast(ray, 100, TargetMask);
        if (hitsSomethingInTargetMask)
        {
          //  print("Something has been hit");
            Dot.color = DotHightlightColor;
        }
        else
        {
            Dot.color = _originalDotColor;
        }
    }
}

