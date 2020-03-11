using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorSpawner : MonoBehaviour {

    public Predators prefab;
    public float spawnRadius = 2;
    public int numberOfPredators = 350;

    void Awake () {
        for (int i = 0; i < numberOfPredators; i++) {

            //https://docs.unity3d.com/ScriptReference/Random-insideUnitSphere.html
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius; 
            Predators predator = Instantiate (prefab);
            predator.transform.position = pos;
            predator.transform.forward = Random.insideUnitSphere; 

        }
    }

}


