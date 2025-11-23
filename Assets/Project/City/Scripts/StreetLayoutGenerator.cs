using System.Collections.Generic;
using UnityEngine;

namespace CrazyRooftop.City
{
    /// <summary>
    /// Data structure for street layout
    /// </summary>
    [System.Serializable]
    public class StreetLayout
    {
        public List<Vector3> intersections = new List<Vector3>();
        public List<StreetSegment> streets = new List<StreetSegment>();
        
        // Grid dimensions
        public int gridSizeX;
        public int gridSizeZ;
        public float cellSizeX;
        public float cellSizeZ;
    }
    
    [System.Serializable]
    public class StreetSegment
    {
        public Vector3 start;
        public Vector3 end;
        public float width;
        public bool isMainStreet; // true = main street, false = passage
        
        public StreetSegment(Vector3 s, Vector3 e, float w, bool main = true)
        {
            start = s;
            end = e;
            width = w;
            isMainStreet = main;
        }
    }
    
    /// <summary>
    /// Generates street layout with distorted grid
    /// </summary>
    public class StreetLayoutGenerator
    {
        private CityGeneratorConfig config;
        private System.Random rng;
        
        public StreetLayoutGenerator(CityGeneratorConfig configuration, System.Random random)
        {
            config = configuration;
            rng = random;
        }
        
        public StreetLayout Generate()
        {
            StreetLayout layout = new StreetLayout();
            layout.gridSizeX = config.gridSizeX;
            layout.gridSizeZ = config.gridSizeZ;
            
            // Calculate base cell sizes (average street width + average building size)
            float avgStreetWidth = (config.streetWidthMin + config.streetWidthMax) / 2f;
            float avgBuildingX = (config.buildingSizeX.x + config.buildingSizeX.y) / 2f;
            float avgBuildingZ = (config.buildingSizeZ.x + config.buildingSizeZ.y) / 2f;
            
            layout.cellSizeX = avgBuildingX + avgStreetWidth;
            layout.cellSizeZ = avgBuildingZ + avgStreetWidth;
            
            // Generate base grid intersections
            GenerateIntersections(layout);
            
            // Apply distortion to intersections
            ApplyDistortion(layout);
            
            // Generate street segments from intersections
            GenerateStreets(layout);
            
            return layout;
        }
        
        private void GenerateIntersections(StreetLayout layout)
        {
            for (int z = 0; z <= layout.gridSizeZ; z++)
            {
                for (int x = 0; x <= layout.gridSizeX; x++)
                {
                    Vector3 position = new Vector3(
                        x * layout.cellSizeX,
                        0f,
                        z * layout.cellSizeZ
                    );
                    layout.intersections.Add(position);
                }
            }
        }
        
        private void ApplyDistortion(StreetLayout layout)
        {
            int gridWidth = layout.gridSizeX + 1;
            
            for (int i = 0; i < layout.intersections.Count; i++)
            {
                Vector3 pos = layout.intersections[i];
                
                // Don't distort edges to keep city bounded
                int x = i % gridWidth;
                int z = i / gridWidth;
                
                if (x == 0 || x == layout.gridSizeX || z == 0 || z == layout.gridSizeZ)
                {
                    continue; // Skip edge intersections
                }
                
                // Apply Perlin noise-based distortion
                float noiseX = Mathf.PerlinNoise(
                    pos.x * config.noiseScale + config.seed,
                    pos.z * config.noiseScale + config.seed
                );
                
                float noiseZ = Mathf.PerlinNoise(
                    pos.x * config.noiseScale + config.seed + 100f,
                    pos.z * config.noiseScale + config.seed + 100f
                );
                
                // Convert noise (0-1) to offset (-1 to 1)
                float offsetX = (noiseX - 0.5f) * 2f * layout.cellSizeX * config.distortionIntensity;
                float offsetZ = (noiseZ - 0.5f) * 2f * layout.cellSizeZ * config.distortionIntensity;
                
                pos.x += offsetX;
                pos.z += offsetZ;
                
                layout.intersections[i] = pos;
            }
        }
        
        private void GenerateStreets(StreetLayout layout)
        {
            int gridWidth = layout.gridSizeX + 1;
            
            // Generate horizontal streets
            for (int z = 0; z <= layout.gridSizeZ; z++)
            {
                for (int x = 0; x < layout.gridSizeX; x++)
                {
                    int index1 = z * gridWidth + x;
                    int index2 = z * gridWidth + (x + 1);
                    
                    Vector3 start = layout.intersections[index1];
                    Vector3 end = layout.intersections[index2];
                    
                    // Alternate between main streets and passages
                    bool isMainStreet = (z % 2 == 0);
                    float width = isMainStreet ? config.GetRandomStreetWidth(rng) : config.passageWidth;
                    
                    layout.streets.Add(new StreetSegment(start, end, width, isMainStreet));
                }
            }
            
            // Generate vertical streets
            for (int x = 0; x <= layout.gridSizeX; x++)
            {
                for (int z = 0; z < layout.gridSizeZ; z++)
                {
                    int index1 = z * gridWidth + x;
                    int index2 = (z + 1) * gridWidth + x;
                    
                    Vector3 start = layout.intersections[index1];
                    Vector3 end = layout.intersections[index2];
                    
                    // Alternate between main streets and passages
                    bool isMainStreet = (x % 2 == 0);
                    float width = isMainStreet ? config.GetRandomStreetWidth(rng) : config.passageWidth;
                    
                    layout.streets.Add(new StreetSegment(start, end, width, isMainStreet));
                }
            }
        }
        
        // Helper to get intersection at grid coordinates
        public static int GetIntersectionIndex(int x, int z, int gridWidth)
        {
            return z * gridWidth + x;
        }
    }
}
