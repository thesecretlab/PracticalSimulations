using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;

[AddRandomizerMenu("Perception/Cam Randomiser")]

public class CamRandomiser : Randomizer
{
    public Camera cam;
    public FloatParameter camX;

    protected override void OnIterationStart()
    {
        cam.transform.position = new Vector3(camX.Sample(), 10.0f, -5.5f);
    }
}