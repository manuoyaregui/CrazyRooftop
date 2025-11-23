using UnityEngine;

namespace CrazyRooftop.City
{
    [CreateAssetMenu(fileName = "CityGeneratorConfig", menuName = "CrazyRooftop/City/Generator Config")]
    public class CityGeneratorConfig : ScriptableObject
    {
        [Header("Generation Settings")]
        [Tooltip("Seed for procedural generation. Same seed = same city")]
        public int seed = 12345;
        
        [Tooltip("Size of each chunk (for future infinite generation)")]
        public float chunkSize = 500f;
        
        [Header("Street Layout")]
        [Tooltip("Number of blocks in X direction")]
        [Range(3, 20)]
        public int gridSizeX = 8;
        
        [Tooltip("Number of blocks in Z direction")]
        [Range(3, 20)]
        public int gridSizeZ = 8;
        
        [Tooltip("Width of narrow passages between buildings")]
        [Range(20f, 50f)]
        public float passageWidth = 30f;
        
        [Tooltip("Width of main streets")]
        [Range(40f, 80f)]
        public float streetWidthMin = 50f;
        
        [Range(40f, 80f)]
        public float streetWidthMax = 60f;
        
        [Header("Grid Distortion")]
        [Tooltip("How much to distort the grid (0 = perfect grid, 1 = very organic)")]
        [Range(0f, 1f)]
        public float distortionIntensity = 0.3f;
        
        [Tooltip("Scale of Perlin noise for distortion")]
        [Range(0.01f, 0.5f)]
        public float noiseScale = 0.1f;
        
        [Tooltip("Maximum rotation for blocks (in degrees)")]
        [Range(0f, 45f)]
        public float maxBlockRotation = 15f;
        
        [Header("Building Sizes")]
        [Tooltip("Building width range (X axis)")]
        public Vector2 buildingSizeX = new Vector2(80f, 120f);
        
        [Tooltip("Building height range (Y axis) - Very tall for parkour")]
        public Vector2 buildingSizeY = new Vector2(300f, 500f);
        
        [Tooltip("Building depth range (Z axis)")]
        public Vector2 buildingSizeZ = new Vector2(80f, 120f);
        
        [Header("Building Spacing")]
        [Tooltip("Minimum distance between buildings (for parkour navigation)")]
        [Range(15f, 40f)]
        public float minBuildingSpacing = 20f;
        
        [Tooltip("Maximum distance between buildings")]
        [Range(20f, 50f)]
        public float maxBuildingSpacing = 35f;
        
        [Header("Building Placement")]
        [Tooltip("How many buildings to try placing per block")]
        [Range(1, 10)]
        public int buildingsPerBlock = 3;
        
        [Tooltip("Maximum attempts to place a building before giving up")]
        [Range(10, 100)]
        public int maxPlacementAttempts = 50;
        
        [Header("Visual")]
        [Tooltip("Building prefabs to randomly instantiate. Leave empty to use procedural cubes.")]
        public GameObject[] buildingPrefabs;
        
        [Tooltip("Use procedural cubes if no prefabs are assigned")]
        public bool useProceduralFallback = true;
        
        [Tooltip("Material to apply to procedurally generated buildings (only if using cubes)")]
        public Material buildingMaterial;
        
        [Tooltip("Show debug gizmos in scene view")]
        public bool showDebugGizmos = true;
        
        // Helper methods
        public float GetRandomStreetWidth(System.Random rng)
        {
            return Mathf.Lerp(streetWidthMin, streetWidthMax, (float)rng.NextDouble());
        }
        
        public Vector3 GetRandomBuildingSize(System.Random rng)
        {
            return new Vector3(
                Mathf.Lerp(buildingSizeX.x, buildingSizeX.y, (float)rng.NextDouble()),
                Mathf.Lerp(buildingSizeY.x, buildingSizeY.y, (float)rng.NextDouble()),
                Mathf.Lerp(buildingSizeZ.x, buildingSizeZ.y, (float)rng.NextDouble())
            );
        }
        
        public float GetRandomBuildingSpacing(System.Random rng)
        {
            return Mathf.Lerp(minBuildingSpacing, maxBuildingSpacing, (float)rng.NextDouble());
        }
        
        public GameObject GetRandomBuildingPrefab(System.Random rng)
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            {
                return null;
            }
            
            int index = rng.Next(0, buildingPrefabs.Length);
            return buildingPrefabs[index];
        }
        
        public bool HasBuildingPrefabs()
        {
            return buildingPrefabs != null && buildingPrefabs.Length > 0;
        }
    }
}
