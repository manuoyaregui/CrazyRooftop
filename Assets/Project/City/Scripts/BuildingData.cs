using UnityEngine;

namespace CrazyRooftop.City
{
    /// <summary>
    /// Component attached to each generated building to store its data
    /// </summary>
    public class BuildingData : MonoBehaviour
    {
        public int buildingId;
        public Vector3 size;
        public Vector3 position;
        public int blockIndex;
        
        public void Initialize(int id, Vector3 buildingSize, Vector3 buildingPosition, int block)
        {
            buildingId = id;
            size = buildingSize;
            position = buildingPosition;
            blockIndex = block;
        }
    }
}
