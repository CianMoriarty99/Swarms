using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {

    public Boids prefab;
    public float spawnRadius = 2;
    public int numberOfBoids = 350;

    void Awake () {
        for (int i = 0; i < numberOfBoids; i++) {

            //https://docs.unity3d.com/ScriptReference/Random-insideUnitSphere.html
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius; 
            Boids boid = Instantiate (prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere; 

        }
    }

}


