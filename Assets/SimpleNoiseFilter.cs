using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNoiseFilter 
{
    Noise noise = new Noise();

    public float Evaluate(Vector3 point, NoiseLayer noiseLayer)
    {
        float noiseValue = 0;
        float frequency = noiseLayer.baseRoghness;
        float amplitude = 1;

        for (int i=0; i < noiseLayer.nbLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + noiseLayer.center);
            noiseValue += (v + 1) * .5f * amplitude;
            frequency *= noiseLayer.roghness;
            amplitude *= noiseLayer.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - noiseLayer.minValue);

        return noiseValue * noiseLayer.strength;
    }
}
