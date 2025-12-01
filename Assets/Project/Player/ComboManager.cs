using UnityEngine;
using System.Collections.Generic;

namespace CrazyRooftop.Player
{
    /// <summary>
    /// Singleton manager that coordinates all combo detection
    /// </summary>
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }

        [Header("References")]
        [Tooltip("Reference to the player controller")]
        public PlayerController PlayerController;

        [Header("Combo Detectors")]
        [Tooltip("List of combo detectors to monitor")]
        public List<ComboDetector> ComboDetectors = new List<ComboDetector>();

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Initialize all detectors
            foreach (var detector in ComboDetectors)
            {
                if (detector != null)
                {
                    detector.Reset();
                }
            }

            Debug.Log("[ComboManager] Initialized!");
        }

        private void Start()
        {
            // Fallback: If player wasn't registered during Awake, try to find it
            if (PlayerController == null)
            {
                PlayerController = FindObjectOfType<PlayerController>();
                if (PlayerController != null)
                {
                    Debug.Log("[ComboManager] Player found via fallback search!");
                }
                else
                {
                    Debug.LogWarning("[ComboManager] No PlayerController found in scene!");
                }
            }
        }

        /// <summary>
        /// Called by PlayerController to register itself with the manager
        /// </summary>
        public void RegisterPlayer(PlayerController player)
        {
            PlayerController = player;
            Debug.Log("[ComboManager] Player registered!");
        }

        private void Update()
        {
            if (PlayerController == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            // Update all detectors
            foreach (var detector in ComboDetectors)
            {
                if (detector != null)
                {
                    detector.OnUpdate(PlayerController, deltaTime);
                }
            }
        }

        /// <summary>
        /// Called by PlayerController when player lands
        /// </summary>
        public void OnPlayerLanded()
        {
            if (PlayerController == null)
            {
                return;
            }

            foreach (var detector in ComboDetectors)
            {
                if (detector != null)
                {
                    detector.OnLanded(PlayerController);
                }
            }
        }

        /// <summary>
        /// Called by PlayerController when state changes
        /// </summary>
        public void OnPlayerStateChanged(CharacterStateEnum newState)
        {
            if (PlayerController == null)
            {
                return;
            }

            foreach (var detector in ComboDetectors)
            {
                if (detector != null)
                {
                    detector.OnStateChanged(PlayerController, newState);
                }
            }
        }

        /// <summary>
        /// Reset all combo detectors
        /// </summary>
        public void ResetAllDetectors()
        {
            foreach (var detector in ComboDetectors)
            {
                if (detector != null)
                {
                    detector.Reset();
                }
            }
        }
    }
}
