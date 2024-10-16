using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSorting : MonoBehaviour
{
    [SerializeField] private int _sortingOrder;
    
    // Start is called before the first frame update
    private void Start()
    {
        GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerID = 0;
        GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder = _sortingOrder;
    }
}
