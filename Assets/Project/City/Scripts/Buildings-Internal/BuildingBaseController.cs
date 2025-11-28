using UnityEngine;

public class BuildingBaseController : MonoBehaviour
{
    [Tooltip("Elements to enable when the player enters the building's trigger")]
    [SerializeField] private GameObject[] _elementsToEnableOnPlayerEnter;

    private bool _isPlayerInside = false;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isPlayerInside && other.CompareTag("Player"))
        {
            _isPlayerInside = true;
            enableElements();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isPlayerInside && other.CompareTag("Player"))
        {
            _isPlayerInside = true;
            enableElements();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isPlayerInside && other.CompareTag("Player"))
        {
            _isPlayerInside = false;
            disableElements();
        }
    }

    private void enableElements()
    {
        foreach (GameObject element in _elementsToEnableOnPlayerEnter)
        {
            element.SetActive(true);
        }
    }

    private void disableElements()
    {
        foreach (GameObject element in _elementsToEnableOnPlayerEnter)
        {
            element.SetActive(false);
        }
    }
}
