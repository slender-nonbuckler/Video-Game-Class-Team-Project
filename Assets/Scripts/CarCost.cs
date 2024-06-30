using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCost : MonoBehaviour
{

    [SerializeField] private int cost = 0;

    public int Cost
    {
        get { return cost; }
        private set { cost = value; }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
