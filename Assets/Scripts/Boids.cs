using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour {

/*
Each Boid (member of the swarm) will have this script attached to it
The script will be independent for each boid however, i.e every Boid 
needs their own position, direction, velocity, etc.
*/

/*
BoidSettings is a "ScriptableObject" which allows each boid to obtain its 
starting settings without creating 50 new instances of the class.
For more info: https://docs.unity3d.com/Manual/class-ScriptableObject.html
*/
    Settings settings;

//These are the values of the boid that update every frame

    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 position; //Current position (x,y,z)
    public Vector3 forward; //Current Direction (x,y,z)
    public int howManyPerceived; //How many boids can this boid see 
    public Vector3 avgSwarmDirection; //Of all the boids in the perception radius, what is the average direction  
    public Vector3 avgAvoidanceDirection; //For each perceived boid, there will be an avoidance direction to turn away from that boid, this is the average of avoidance for all perceived boids.
    public Vector3 otherBoidCoordinates; //The centre Coordinate of all perceived boids.
    public Material material; //this boids Material
    public Transform boidTransform; //We "Cache" the transform for optimisation

    //Awake happens before anything
    void Awake () {
        material = transform.GetComponentInChildren<MeshRenderer>().material; //Get the material component of this boid
        boidTransform = transform; //Set the Cache
    }

    //We can Initialize the boid with Settings from our BoidSettings Script
    public void Initialize (Settings settings) {
        this.settings = settings; //Get a new reference of the settings just for this boid
        position = boidTransform.position; //Initialize the start position
        forward = boidTransform.forward; //Initialize the start direction
        float startSpeed = (settings.minimumSpeed + settings.maximumSpeed) / 2; //Initialize the start speed
        velocity = transform.forward * startSpeed; //Initialize the velocity
    }


    //Update is called every frame
    void Update()
    {
        //If this boid is the target then change its colour to red
        if (ThirdPersonCamera.Target == this.gameObject)
        {
            this.material.color = Color.red;
        }
        else
        {
            this.material.color = Color.blue;
        }
    }

    public void MoveBoid () {

       acceleration = Vector3.zero; //Start with an acceleration of 0

        if (howManyPerceived != 0) {

            //Divide the sum of the other boids coordinates with how many there are to get the average,
            //This will give back the centre of all perceived boids
            otherBoidCoordinates /= howManyPerceived; 

            //We need to get the difference between this boids location and the swarms centre location
            Vector3 offsetToSwarmCentre = (otherBoidCoordinates - position);

            //Cohesion - Move towards the Centre of the swarm, based on the weight
            var cohesion = MoveTowards (offsetToSwarmCentre) * settings.cohesionWeight; 

            //Alignment - Move towards the average swarms direction, based on the weight
            var alignment = MoveTowards (avgSwarmDirection) * settings.alignmentWeight;

            //Seperation - Move towards the avoidance direction, based on the weight
            var seperation = MoveTowards (avgAvoidanceDirection) * settings.seperateWeight;

            //Add these new values to the acceleration of the boid
            acceleration += cohesion;
            acceleration += alignment;
            acceleration += seperation;
        }

        //Check whether the boid is heading for an obstacle
        if (HeadingForObstacle ()) {

            //if it is then get the closest direction that doesnt intersect with an obstacle
            Vector3 avoidCollisionRay = ObstacleDirections ();
            //And move towards that direction
            Vector3 avoidCollisionForce = MoveTowards (avoidCollisionRay) * settings.avoidCollisionWeight;
            acceleration += avoidCollisionForce;
        }

        //Some physics equations
        // V - m/s,  a - m/s^2, T - s
        velocity += acceleration * Time.deltaTime;

        //Speed is the scalar of the velocity vector
        float speed = velocity.magnitude;
        // 
        Vector3 direction = velocity / speed;

        //Make sure the speed doesn't exceed the min or max speeds
        speed = Mathf.Clamp (speed, settings.minimumSpeed, settings.maximumSpeed);
        //Update the velocity to effectively clamp the velocity as well
        velocity = direction * speed;

        // X - m, V - m/s, T = s
        boidTransform.position += velocity * Time.deltaTime;

        //Set the new direction of the boid
        boidTransform.forward = direction;
        //Set the new position of the boid
        position = boidTransform.position;
        forward = direction;
    }

    Vector3 MoveTowards (Vector3 vector) {
        //Normalizing the vector just gives the direction towards it, not the length, 
        //so we can make sure it can only travels the max speed towards it
        Vector3 v = vector.normalized * settings.maximumSpeed - velocity;
        //https://docs.unity3d.com/ScriptReference/Vector3.ClampMagnitude.html
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

    bool HeadingForObstacle() {
        //What did the raycast hit?
        RaycastHit hit;
        //If it does hit something, return true
        if (Physics.SphereCast (position, settings.raycastDisplacementRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask)) {
            return true;
        } else 
            return false;
    }

    Vector3 ObstacleDirections() {
        //An array of all the directions the boids can move (300)
        Vector3[] rayDirections = BoidHelper.directions;
        //Iterate through all the directions
        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = boidTransform.TransformDirection (rayDirections[i]);
            //Shoot a ray from this direction
            Ray ray = new Ray (position, dir);
            //If it Doesn't hit anything (an obstacle), then return that directions
            if (!Physics.SphereCast (ray, settings.raycastDisplacementRadius, settings.collisionAvoidDst, settings.obstacleMask)) {
                return dir;
            }
        }
        //If somehow everything around it is an obstacle then just move forward
        //We just need this so that this function always returns something
        return forward;
    }



 
}