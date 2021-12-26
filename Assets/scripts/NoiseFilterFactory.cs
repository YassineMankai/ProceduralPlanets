using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseFilterFactory
{
    public static SimpleNoiseFilter simpleNoiseFilter = new SimpleNoiseFilter();
    public static RigidNoiseFilter rigidNoiseFilter = new RigidNoiseFilter();

    public static INoiseFilter createNoiseFilter(NoiseLayer noiseLayer)
    {
        switch (noiseLayer.filterType)
        {
            case NoiseLayer.FilterType.Simple:
                return simpleNoiseFilter;
            case NoiseLayer.FilterType.Rigid:
                return rigidNoiseFilter;
        }
        return null;
    }
}
