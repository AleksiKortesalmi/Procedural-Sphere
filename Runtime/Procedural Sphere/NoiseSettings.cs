using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType {Simple, Rigid};
    public FilterType filterType;
    public SimpleNoiseSettings simpleNoiseSettings;
    [DrawIf("filterType", 1)]
    public RigidNoiseSettings rigidNoiseSettings;

    [System.Serializable]
    public class SimpleNoiseSettings
    {
        public float strength = 1;
        [Range(1, 8)]
        public int numLayers = 1;
        public float firstLayerRoughness = 1;
        public float roughness = 1;
        public float persistence = 1;
        public float minValue = 0;
        public Vector3 offset;

        public SimpleNoiseSettings(float strength, int numLayers, float baseRoughness, float roughness, float persistence, float minValue, Vector3 offset)
        {
            this.strength = strength;
            this.numLayers = numLayers;
            this.firstLayerRoughness = baseRoughness;
            this.roughness = roughness;
            this.persistence = persistence;
            this.minValue = minValue;
            this.offset = offset;
        }
    }

    [System.Serializable]
    public class RigidNoiseSettings
    {
        public float weightMultiplier = .8f;

        public RigidNoiseSettings(float weightMultiplier)
        {
            this.weightMultiplier = weightMultiplier;
        }
    }

    public NoiseSettings(FilterType filterType, SimpleNoiseSettings simpleNoiseSettings, RigidNoiseSettings rigidNoiseSettings)
    {
        this.filterType = filterType;
        this.simpleNoiseSettings = simpleNoiseSettings;
        this.rigidNoiseSettings = rigidNoiseSettings;
    }
}
