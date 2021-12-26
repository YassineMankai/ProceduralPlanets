using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BiomeColorSettings
{
    public Biome[] biomes;
    public NoiseLayer noise;
    public float noiseOffset;
    public float noiseStrength;
    [Range(0,1f)]
    public float blendAmount;

    [System.Serializable]
    public class Biome
    {
        public Gradient gradient;
        public Color tint;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float tintPercent;
    }

    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        float heightPercent = (pointOnUnitSphere.y + 1) / 2;
        if (noise.enabled)
            heightPercent += (NoiseFilterFactory.createNoiseFilter(noise).Evaluate(pointOnUnitSphere, noise) - noiseOffset) * noiseStrength;
        float biomeIndex = 0;
        int nbBiomes = biomes.Length;

        float blendRange = blendAmount /2 + 0.001f;

        for (int i = 0; i < nbBiomes; i++)
        {
            float dst = heightPercent - biomes[i].startHeight;
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }

        return biomeIndex / Mathf.Max(1, nbBiomes - 1);
    }

}
