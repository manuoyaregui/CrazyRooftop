using System.Collections.Generic;
using UnityEngine;

namespace CrazyRooftop.City
{
    /// <summary>
    /// Places buildings within city blocks with proper spacing
    /// </summary>
    public class BuildingPlacer
    {
        private CityGeneratorConfig config;
        private System.Random rng;
        private int buildingIdCounter = 0;
        
        public BuildingPlacer(CityGeneratorConfig configuration, System.Random random)
        {
            config = configuration;
            rng = random;
        }
        
        public List<GameObject> PlaceBuildings(List<CityBlock> blocks, Transform parent)
        {
            List<GameObject> buildings = new List<GameObject>();
            
            foreach (CityBlock block in blocks)
            {
                List<GameObject> blockBuildings = PlaceBuildingsInBlock(block, parent);
                buildings.AddRange(blockBuildings);
            }
            
            return buildings;
        }
        
        private List<GameObject> PlaceBuildingsInBlock(CityBlock block, Transform parent)
        {
            List<GameObject> buildings = new List<GameObject>();
            List<Bounds> placedBounds = new List<Bounds>();
            
            int buildingsPlaced = 0;
            int attempts = 0;
            
            while (buildingsPlaced < config.buildingsPerBlock && attempts < config.maxPlacementAttempts)
            {
                attempts++;
                
                // Generate random building size
                Vector3 buildingSize = config.GetRandomBuildingSize(rng);
                
                // Try to find a valid position
                Vector3 position;
                if (TryFindValidPosition(block, buildingSize, placedBounds, out position))
                {
                    // Create building
                    GameObject building = CreateBuilding(position, buildingSize, block);
                    building.transform.SetParent(parent);
                    buildings.Add(building);
                    
                    // Add to placed bounds with spacing
                    Bounds buildingBounds = new Bounds(position, buildingSize);
                    float spacing = config.GetRandomBuildingSpacing(rng);
                    buildingBounds.Expand(spacing * 2f); // Expand in all directions
                    placedBounds.Add(buildingBounds);
                    
                    buildingsPlaced++;
                }
            }
            
            return buildings;
        }
        
        private bool TryFindValidPosition(CityBlock block, Vector3 buildingSize, List<Bounds> placedBounds, out Vector3 position)
        {
            const int maxAttempts = 20;
            
            for (int i = 0; i < maxAttempts; i++)
            {
                // Random position within block's usable bounds
                float randomX = ((float)rng.NextDouble() - 0.5f) * (block.usableBounds.size.x - buildingSize.x);
                float randomZ = ((float)rng.NextDouble() - 0.5f) * (block.usableBounds.size.z - buildingSize.z);
                
                // Position at ground level (Y=0) - assumes prefab pivot is at base
                position = block.center + new Vector3(randomX, 0f, randomZ);
                position.y = 0f; // Ensure buildings are at ground level
                
                // Check if position is valid (not overlapping with other buildings)
                Bounds testBounds = new Bounds(position, buildingSize);
                
                bool valid = true;
                foreach (Bounds placed in placedBounds)
                {
                    if (testBounds.Intersects(placed))
                    {
                        valid = false;
                        break;
                    }
                }
                
                if (valid)
                {
                    return true;
                }
            }
            
            position = Vector3.zero;
            return false;
        }
        
        private GameObject CreateBuilding(Vector3 position, Vector3 size, CityBlock block)
        {
            GameObject building;
            
            // Try to use prefab first
            if (config.HasBuildingPrefabs())
            {
                GameObject prefab = config.GetRandomBuildingPrefab(rng);
                building = Object.Instantiate(prefab);
                building.name = $"Building_{buildingIdCounter}";
                
                // Smart scaling: maintain prefab proportions, scale to match target height
                // This allows you to build prefabs at natural proportions (e.g., 1:4:1)
                // and the system will scale them to the generated height while keeping the aspect ratio
                
                // Get prefab's original bounds
                Bounds prefabBounds = GetPrefabBounds(building);
                
                if (prefabBounds.size.magnitude > 0.01f) // Valid bounds
                {
                    // Calculate scale factor based on target height
                    float scaleFactor = size.y / prefabBounds.size.y;
                    
                    // Apply uniform scale to maintain proportions
                    building.transform.localScale = Vector3.one * scaleFactor;
                    
                    // Store the actual size after scaling for BuildingData
                    Vector3 actualSize = prefabBounds.size * scaleFactor;
                    size = actualSize; // Update size to reflect actual scaled size
                }
                else
                {
                    // Fallback: direct scale if bounds can't be calculated
                    building.transform.localScale = size;
                }
            }
            else if (config.useProceduralFallback)
            {
                // Fallback to procedural cube
                building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Building_{buildingIdCounter}_Procedural";
                building.transform.localScale = size;
                
                // Apply material if available
                if (config.buildingMaterial != null)
                {
                    Renderer renderer = building.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = config.buildingMaterial;
                    }
                }
            }
            else
            {
                Debug.LogWarning("CityGenerator: No building prefabs assigned and procedural fallback is disabled!");
                building = new GameObject($"Building_{buildingIdCounter}_Empty");
            }
            
            // Set position and rotation
            building.transform.position = position;
            building.transform.rotation = Quaternion.Euler(0f, block.rotation, 0f);
            
            // Add BuildingData component
            BuildingData data = building.GetComponent<BuildingData>();
            if (data == null)
            {
                data = building.AddComponent<BuildingData>();
            }
            data.Initialize(buildingIdCounter, size, position, block.blockIndex);
            
            buildingIdCounter++;
            
            return building;
        }
        
        /// <summary>
        /// Calculate bounds of a prefab by combining all renderers
        /// </summary>
        private Bounds GetPrefabBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }
            
            Bounds bounds = renderers[0].bounds;
            
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            
            // Convert to local space
            Vector3 localCenter = obj.transform.InverseTransformPoint(bounds.center);
            Vector3 localSize = bounds.size;
            
            return new Bounds(localCenter, localSize);
        }
    }
}
