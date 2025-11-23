using UnityEngine;
using CrazyRooftop.City;

namespace CrazyRooftop.City
{
    /// <summary>
    /// Example script showing how to integrate CityGenerator with player spawning
    /// </summary>
    public class CityPlayerSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CityGenerator cityGenerator;
        [SerializeField] private GameObject playerPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private float roofOffset = 5f; // Offset above building roof
        
        [Tooltip("Search radius from city center (0 = spawn on closest building to center)")]
        [SerializeField] private float centerSearchRadius = 100f;
        
        private GameObject spawnedPlayer;
        
        private void Start()
        {
            // Wait a frame for city to generate
            Invoke(nameof(SpawnPlayer), 0.1f);
        }
        
        private void SpawnPlayer()
        {
            if (cityGenerator == null)
            {
                Debug.LogError("CityPlayerSpawner: No CityGenerator assigned!");
                return;
            }
            
            // Get a building near the center of the city
            GameObject building = cityGenerator.GetBuildingNearCenter(centerSearchRadius);
            
            if (building == null)
            {
                Debug.LogError("CityPlayerSpawner: No buildings generated!");
                return;
            }
            
            // Get building data
            BuildingData buildingData = building.GetComponent<BuildingData>();
            
            // Calculate roof position
            Vector3 roofPosition = building.transform.position;
            roofPosition.y += (buildingData.size.y / 2f) + roofOffset;
            
            // Spawn player
            if (playerPrefab != null)
            {
                spawnedPlayer = Instantiate(playerPrefab, roofPosition, Quaternion.identity);
                
                Vector3 cityCenter = cityGenerator.GetCityCenter();
                float distanceFromCenter = Vector3.Distance(building.transform.position, cityCenter);
                
                Debug.Log($"Player spawned on building {buildingData.buildingId} at {roofPosition}");
                Debug.Log($"Distance from city center: {distanceFromCenter:F1} units");
            }
            else
            {
                Debug.LogWarning("CityPlayerSpawner: No player prefab assigned, just showing spawn position");
                Debug.DrawRay(roofPosition, Vector3.up * 10f, Color.green, 10f);
            }
        }
        
        // Optional: Respawn player on a different building
        [ContextMenu("Respawn Player")]
        public void RespawnPlayer()
        {
            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
            }
            
            SpawnPlayer();
        }
    }
}
