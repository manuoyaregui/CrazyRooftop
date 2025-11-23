using System.Collections.Generic;
using UnityEngine;

namespace CrazyRooftop.City
{
    /// <summary>
    /// Represents a city block (manzana) that can contain buildings
    /// </summary>
    [System.Serializable]
    public class CityBlock
    {
        public Vector3 center;
        public Vector3 size;
        public float rotation; // Y-axis rotation in degrees
        public Bounds usableBounds;
        public int blockIndex;
        
        // Grid coordinates
        public int gridX;
        public int gridZ;
        
        public CityBlock(Vector3 c, Vector3 s, float rot, int index, int x, int z)
        {
            center = c;
            size = s;
            rotation = rot;
            blockIndex = index;
            gridX = x;
            gridZ = z;
            
            // Calculate usable bounds (slightly smaller than actual block to avoid streets)
            usableBounds = new Bounds(center, size * 0.85f);
        }
    }
    
    /// <summary>
    /// Generates city blocks from street layout
    /// </summary>
    public class BlockGenerator
    {
        private CityGeneratorConfig config;
        private System.Random rng;
        
        public BlockGenerator(CityGeneratorConfig configuration, System.Random random)
        {
            config = configuration;
            rng = random;
        }
        
        public List<CityBlock> Generate(StreetLayout layout)
        {
            List<CityBlock> blocks = new List<CityBlock>();
            
            int gridWidth = layout.gridSizeX + 1;
            int blockIndex = 0;
            
            // Generate blocks from grid cells
            for (int z = 0; z < layout.gridSizeZ; z++)
            {
                for (int x = 0; x < layout.gridSizeX; x++)
                {
                    // Get the four corners of this block
                    int bottomLeft = StreetLayoutGenerator.GetIntersectionIndex(x, z, gridWidth);
                    int bottomRight = StreetLayoutGenerator.GetIntersectionIndex(x + 1, z, gridWidth);
                    int topLeft = StreetLayoutGenerator.GetIntersectionIndex(x, z + 1, gridWidth);
                    int topRight = StreetLayoutGenerator.GetIntersectionIndex(x + 1, z + 1, gridWidth);
                    
                    Vector3 bl = layout.intersections[bottomLeft];
                    Vector3 br = layout.intersections[bottomRight];
                    Vector3 tl = layout.intersections[topLeft];
                    Vector3 tr = layout.intersections[topRight];
                    
                    // Calculate block center
                    Vector3 center = (bl + br + tl + tr) / 4f;
                    
                    // Calculate block size (average of opposite sides)
                    float sizeX = ((br - bl).magnitude + (tr - tl).magnitude) / 2f;
                    float sizeZ = ((tl - bl).magnitude + (tr - br).magnitude) / 2f;
                    
                    // Account for street width - make blocks smaller
                    float avgStreetWidth = (config.streetWidthMin + config.streetWidthMax) / 2f;
                    sizeX -= avgStreetWidth;
                    sizeZ -= avgStreetWidth;
                    
                    // Ensure minimum size
                    sizeX = Mathf.Max(sizeX, 50f);
                    sizeZ = Mathf.Max(sizeZ, 50f);
                    
                    Vector3 size = new Vector3(sizeX, 0f, sizeZ);
                    
                    // Random rotation for variety
                    float rotation = ((float)rng.NextDouble() - 0.5f) * 2f * config.maxBlockRotation;
                    
                    CityBlock block = new CityBlock(center, size, rotation, blockIndex, x, z);
                    blocks.Add(block);
                    
                    blockIndex++;
                }
            }
            
            return blocks;
        }
    }
}
