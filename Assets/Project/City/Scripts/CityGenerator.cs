using System.Collections.Generic;
using UnityEngine;

namespace CrazyRooftop.City
{
    /// <summary>
    /// Main city generator that orchestrates the entire generation process
    /// </summary>
    public class CityGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CityGeneratorConfig config;
        
        [Header("Runtime Data")]
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private bool regenerateOnPlayMode = true;
        
        [Header("Seed Settings")]
        [Tooltip("Use random seed each time (ignores config seed)")]
        [SerializeField] private bool useRandomSeed = true;
        
        [Tooltip("Current seed being used (for debugging)")]
        [SerializeField] private int currentSeed;
        
        [Header("Player Spawn (Optional)")]
        [Tooltip("Player prefab to spawn automatically after city generation")]
        [SerializeField] private GameObject playerPrefab;
        
        [Tooltip("Spawn player automatically after city generation")]
        [SerializeField] private bool spawnPlayerAutomatically = false;
        
        [Tooltip("Search radius from city center (0 = closest building)")]
        [SerializeField] private float spawnSearchRadius = 100f;
        
        [Tooltip("Offset above building roof")]
        [SerializeField] private float spawnRoofOffset = 5f;
        
        // Reference to spawned player
        private GameObject spawnedPlayer;
        
        // Generated data
        private StreetLayout streetLayout;
        private List<CityBlock> blocks;
        private List<GameObject> buildings;
        
        // Generators
        private StreetLayoutGenerator streetGenerator;
        private BlockGenerator blockGenerator;
        private BuildingPlacer buildingPlacer;
        
        // Container for generated objects
        private Transform cityContainer;
        
        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("CityGenerator: No config assigned!");
                return;
            }
            
            // Create container
            cityContainer = new GameObject("Generated City").transform;
            cityContainer.SetParent(transform);
        }
        
        private void Start()
        {
            if (generateOnStart)
            {
                GenerateCity();
            }
        }
        
        /// <summary>
        /// Main generation method - call this to generate/regenerate the city
        /// </summary>
        public void GenerateCity()
        {
            if (config == null)
            {
                Debug.LogError("CityGenerator: Cannot generate without config!");
                return;
            }
            
            // Clear previous city
            ClearCity();
            
            // Determine seed to use
            if (useRandomSeed)
            {
                currentSeed = Random.Range(int.MinValue, int.MaxValue);
            }
            else
            {
                currentSeed = config.seed;
            }
            
            // Initialize random with seed
            System.Random rng = new System.Random(currentSeed);
            
            // Initialize generators
            streetGenerator = new StreetLayoutGenerator(config, rng);
            blockGenerator = new BlockGenerator(config, rng);
            buildingPlacer = new BuildingPlacer(config, rng);
            
            // Execute generation pipeline
            streetLayout = streetGenerator.Generate();
            
            blocks = blockGenerator.Generate(streetLayout);
            
            buildings = buildingPlacer.PlaceBuildings(blocks, cityContainer);
            
            // Spawn player if enabled
            if (spawnPlayerAutomatically && playerPrefab != null)
            {
                SpawnPlayer();
            }
        }
        
        /// <summary>
        /// Clears all generated city objects
        /// </summary>
        public void ClearCity()
        {
            if (cityContainer != null)
            {
                // Destroy all children
                for (int i = cityContainer.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(cityContainer.GetChild(i).gameObject);
                }
            }
            
            streetLayout = null;
            blocks = null;
            buildings = null;
        }
        
        /// <summary>
        /// Get all generated buildings
        /// </summary>
        public List<GameObject> GetBuildings()
        {
            return buildings;
        }
        
        /// <summary>
        /// Get a random building (useful for player spawning)
        /// </summary>
        public GameObject GetRandomBuilding()
        {
            if (buildings == null || buildings.Count == 0)
            {
                return null;
            }
            
            return buildings[Random.Range(0, buildings.Count)];
        }
        
        /// <summary>
        /// Get the center position of the city
        /// </summary>
        public Vector3 GetCityCenter()
        {
            if (streetLayout == null || streetLayout.intersections.Count == 0)
            {
                return Vector3.zero;
            }
            
            // Calculate average position of all intersections
            Vector3 center = Vector3.zero;
            foreach (Vector3 intersection in streetLayout.intersections)
            {
                center += intersection;
            }
            center /= streetLayout.intersections.Count;
            
            return center;
        }
        
        /// <summary>
        /// Get a building near the center of the city
        /// </summary>
        /// <param name="searchRadius">How far from center to search (0 = closest building)</param>
        public GameObject GetBuildingNearCenter(float searchRadius = 0f)
        {
            if (buildings == null || buildings.Count == 0)
            {
                return null;
            }
            
            Vector3 center = GetCityCenter();
            
            if (searchRadius <= 0f)
            {
                // Find the absolute closest building to center
                GameObject closestBuilding = null;
                float closestDistance = float.MaxValue;
                
                foreach (GameObject building in buildings)
                {
                    float distance = Vector3.Distance(building.transform.position, center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBuilding = building;
                    }
                }
                
                return closestBuilding;
            }
            else
            {
                // Find all buildings within radius, then pick random
                List<GameObject> candidateBuildings = new List<GameObject>();
                
                foreach (GameObject building in buildings)
                {
                    float distance = Vector3.Distance(building.transform.position, center);
                    if (distance <= searchRadius)
                    {
                        candidateBuildings.Add(building);
                    }
                }
                
                if (candidateBuildings.Count > 0)
                {
                    return candidateBuildings[Random.Range(0, candidateBuildings.Count)];
                }
                
                // Fallback: if no buildings in radius, return closest
                Debug.LogWarning($"CityGenerator: No buildings within radius {searchRadius}, using closest instead");
                return GetBuildingNearCenter(0f);
            }
        }
        
        /// <summary>
        /// Spawn player on a building near the city center
        /// </summary>
        private void SpawnPlayer()
        {
            // Destroy previous player if exists
            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
            }
            
            GameObject building = GetBuildingNearCenter(spawnSearchRadius);
            
            if (building == null)
            {
                Debug.LogWarning("CityGenerator: Cannot spawn player - no buildings available!");
                return;
            }
            
            BuildingData buildingData = building.GetComponent<BuildingData>();
            
            if (buildingData == null)
            {
                Debug.LogError("CityGenerator: Building missing BuildingData component!");
                return;
            }
            
            // Calculate roof position using the actual bounds of the building
            Renderer renderer = building.GetComponentInChildren<Renderer>();
            
            if (renderer != null)
            {
                // Use the renderer bounds to get the actual top of the building
                Bounds bounds = renderer.bounds;
                Vector3 spawnPosition = new Vector3(
                    bounds.center.x,
                    bounds.max.y + spawnRoofOffset,
                    bounds.center.z
                );
                
                // Spawn player
                spawnedPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                spawnedPlayer.name = "Player";
            }
            else
            {
                Debug.LogError("CityGenerator: Building has no Renderer! Cannot calculate roof position.");
            }
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!config || !config.showDebugGizmos)
                return;
            
            // Show preview of city bounds BEFORE generation (in edit mode)
            if (!Application.isPlaying)
            {
                DrawCityPreview();
                return;
            }
            
            // Show actual city data AFTER generation (in play mode)
            if (streetLayout != null)
            {
                DrawStreetGizmos();
            }
            
            if (blocks != null)
            {
                DrawBlockGizmos();
            }
        }
        
        private void DrawCityPreview()
        {
            // Calculate approximate city bounds based on config
            float avgStreetWidth = (config.streetWidthMin + config.streetWidthMax) / 2f;
            float avgBuildingX = (config.buildingSizeX.x + config.buildingSizeX.y) / 2f;
            float avgBuildingZ = (config.buildingSizeZ.x + config.buildingSizeZ.y) / 2f;
            
            float cellSizeX = avgBuildingX + avgStreetWidth;
            float cellSizeZ = avgBuildingZ + avgStreetWidth;
            
            float totalSizeX = cellSizeX * config.gridSizeX;
            float totalSizeZ = cellSizeZ * config.gridSizeZ;
            
            Vector3 center = transform.position;
            center.x += totalSizeX / 2f;
            center.z += totalSizeZ / 2f;
            
            // Draw city bounds
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireCube(center, new Vector3(totalSizeX, 10f, totalSizeZ));
            
            // Draw center marker
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, 20f);
            Gizmos.DrawLine(center + Vector3.up * 50f, center - Vector3.up * 50f);
            
            // Draw grid preview
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            for (int x = 0; x <= config.gridSizeX; x++)
            {
                Vector3 start = transform.position + new Vector3(x * cellSizeX, 0f, 0f);
                Vector3 end = start + new Vector3(0f, 0f, totalSizeZ);
                Gizmos.DrawLine(start, end);
            }
            
            for (int z = 0; z <= config.gridSizeZ; z++)
            {
                Vector3 start = transform.position + new Vector3(0f, 0f, z * cellSizeZ);
                Vector3 end = start + new Vector3(totalSizeX, 0f, 0f);
                Gizmos.DrawLine(start, end);
            }
            
            // Draw label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(center + Vector3.up * 30f, 
                $"City Preview\n{config.gridSizeX}x{config.gridSizeZ} blocks\n~{totalSizeX:F0}x{totalSizeZ:F0} units");
            #endif
        }
        
        private void DrawStreetGizmos()
        {
            // Draw intersections
            Gizmos.color = Color.yellow;
            foreach (Vector3 intersection in streetLayout.intersections)
            {
                Gizmos.DrawSphere(intersection, 2f);
            }
            
            // Draw streets
            foreach (StreetSegment street in streetLayout.streets)
            {
                // Different colors for main streets vs passages
                Gizmos.color = street.isMainStreet ? Color.cyan : Color.green;
                Gizmos.DrawLine(street.start, street.end);
                
                // Draw street width
                Vector3 direction = (street.end - street.start).normalized;
                Vector3 perpendicular = new Vector3(-direction.z, 0f, direction.x) * (street.width / 2f);
                
                Gizmos.color = street.isMainStreet ? new Color(0f, 1f, 1f, 0.3f) : new Color(0f, 1f, 0f, 0.3f);
                Gizmos.DrawLine(street.start + perpendicular, street.end + perpendicular);
                Gizmos.DrawLine(street.start - perpendicular, street.end - perpendicular);
            }
        }
        
        private void DrawBlockGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            
            foreach (CityBlock block in blocks)
            {
                // Draw block bounds
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                    block.center,
                    Quaternion.Euler(0f, block.rotation, 0f),
                    Vector3.one
                );
                
                Gizmos.matrix = rotationMatrix;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(block.size.x, 10f, block.size.z));
                
                // Draw usable bounds
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(block.usableBounds.size.x, 5f, block.usableBounds.size.z));
                
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
        
#if UNITY_EDITOR
        // Editor button to regenerate
        [ContextMenu("Generate City")]
        private void EditorGenerateCity()
        {
            GenerateCity();
        }
        
        [ContextMenu("Clear City")]
        private void EditorClearCity()
        {
            ClearCity();
        }
#endif
    }
}
