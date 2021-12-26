using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidNoiseFilter : INoiseFilter
{
    Noise noise = new Noise();

    public float Evaluate(Vector3 point, NoiseLayer noiseLayer)
    {
        float noiseValue = 0;
        float frequency = noiseLayer.baseRoghness;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < noiseLayer.nbLayers; i++)
        {
            float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + noiseLayer.center));
            v *= v;
            v *= weight;
            weight = v;
            noiseValue += v * amplitude;
            frequency *= noiseLayer.roghness;
            amplitude *= noiseLayer.persistence;
        }

        noiseValue = noiseValue - noiseLayer.minValue;

        return noiseValue * noiseLayer.strength;
    }
}
