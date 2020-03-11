using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : MonoBehaviour {

    const int threadGroupSize = 1024;

    public PredatorSettings psettings;
    public ComputeShader compute;
    Predators[] predators;

    void Start() {
        predators = FindObjectsOfType<Predators>();
        foreach (Predators b in predators) {
            b.Initialize(psettings);
        }

    }

    void FixedUpdate () {
        if (predators != null) {

            int numPredators = predators.Length;
            var predatorData = new PredatorData[numPredators];

            for (int i = 0; i < predators.Length; i++) {
                predatorData[i].position = predators[i].position;
                predatorData[i].direction = predators[i].forward;
            }

            var predatorBuffer = new ComputeBuffer(numPredators, PredatorData.Size);
            predatorBuffer.SetData(predatorData);

            compute.SetBuffer(0, "predators", predatorBuffer);
            compute.SetInt("numPredators", predators.Length);
            compute.SetFloat("viewRadius", psettings.perceptionRadius);
            compute.SetFloat("avoidRadius", psettings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numPredators / (float) threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            predatorBuffer.GetData(predatorData);

            for (int i = 0; i < predators.Length; i++) {
                predators[i].avgSwarmDirection = predatorData[i].SwarmDirection;
                predators[i].otherPredatorCoordinates = predatorData[i].SwarmCentre;
                predators[i].avgAvoidanceDirection = predatorData[i].avoidanceDirection;
                predators[i].howManyPerceived = predatorData[i].numSwarm;

                predators[i].MovePredator();
            }

            predatorBuffer.Release();
        }
    }

    public struct PredatorData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 SwarmDirection;
        public Vector3 SwarmCentre;
        public Vector3 avoidanceDirection;
        public int numSwarm;

        

        public static int Size {
            get {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }
}