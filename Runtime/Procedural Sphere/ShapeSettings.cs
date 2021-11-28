using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject
{
    [Range(0.01f, 10)]
    public float radius = 1;
    [Range(0, 10)]
    public float offsetVelocity = 1;

    public NoiseLayer[] noiseLayers = new NoiseLayer[0];

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;
        [Tooltip("0 = No mask, 1 = Mask, -1 = Reverse Mask.")]
        [Range(-1, 1)]
        public int firstLayerAsMask;
        public NoiseSettings noiseSettings;

        public NoiseLayer(bool enabled, int firstLayerAsMask, NoiseSettings noiseSettings)
        {
            this.enabled = enabled;
            this.firstLayerAsMask = firstLayerAsMask;
            this.noiseSettings = noiseSettings;
        }
    }
}
