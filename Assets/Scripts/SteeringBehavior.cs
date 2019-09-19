using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public struct SteeringOutput
{
    public Vector3 linear;
    public float angular;
}

public class SteeringBehavior : MonoBehaviour
{

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;

    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    protected void Start()
    {
        agent = GetComponent<NPCController>();
        //wanderOrientation = agent.orientation;
    }

    public SteeringOutput Seek()
    {
        SteeringOutput steering = new SteeringOutput
        {
            linear = new Vector3(0.0F, 0.0F, 0.0F)
        };
        steering.linear = target.position - agent.position;
        float distance = (target.position - agent.position).magnitude;
        if (distance < slowRadiusL)
        {
            DynamicArrive();
        }
        else
        {
            steering.linear.Normalize();
            steering.linear *= maxAcceleration;
        }
        return steering;
    }

    public SteeringOutput Flee()
    {
        SteeringOutput steering = new SteeringOutput
        {
            linear = agent.position - target.position
        };
        steering.linear.Normalize();
        steering.linear *= maxAcceleration;
        return steering;
    }

    public SteeringOutput DynamicArrive()
    {
        /* This functions follows the pseudo-code for arrive in the book
         * AI for Games by Ian Millington
         */

        // Create the structure to hold our output
        SteeringOutput steering = new SteeringOutput();

        // Get the direction to the target
        Vector3 direction = target.position - agent.position;
        float distance = direction.magnitude;

        // Check if we are there, return no steering
        if (distance < targetRadiusL)
        {
            return steering; // Return None
        }

        // If we are outside of the slowRadius, then go max speed
        else if (distance > slowRadiusL)
        {
            //float targetSpeed = maxSpeed;
            target.maxSpeed = maxSpeed;
        }

        // Otherwise calculate a scaled speed
        else
        {
            agent.DrawCircle(agent.position, 1.0f);
            float targetSpeed = maxSpeed * (distance / slowRadiusL);
            // The target velocity combines speed and direction 
            target.velocity = direction;
            target.velocity.Normalize();
            target.velocity *= target.maxSpeed;

            // Acceleration tries to get to the target velocty 
            steering.linear = target.velocity - agent.velocity;
            steering.linear /= timeToTarget;

            // Check if acceleration is too fast 
            if (steering.linear.magnitude > maxAcceleration)
            { //maxAcceleration
                steering.linear.Normalize();
                steering.linear *= maxAcceleration;
            }
        }
        // Output the steering
        steering.angular = 0;
        return steering;
    }

    public SteeringOutput Pursue()
    {
        /* OVERRIDES the target data in seek (in other words 
         * this class has two bits of data called target: 
         * Seek.target is the superclass target which
         * will be automatically calculated and shouldn’t
         * be set, and Pursue.target is the target we’re pursuing). 
         */

        // Other data is derived from the Seek() function 
        float prediction;

        // 1.  Calculate the target to delegate to seek
        // Work out the distance to target 
        Vector3 direction = target.position - agent.position;
        float distance = direction.magnitude;

        // Work out our current speed 
        float speed = agent.velocity.magnitude;

        // Check if speed is too small to give a reasonable prediction time
        if (speed <= distance / maxPrediction)
        {
            prediction = maxPrediction;
        }

        //  Otherwise calculate the prediction time 
        else
        {
            prediction = distance / speed;
        }
        // Put the target together 
        // Create the structure to hold our output
        SteeringOutput steering;
        steering  = this.Seek();
        steering.linear += (target.position + target.velocity * prediction);
        return steering;
    }

    public SteeringOutput Evade() {
        SteeringOutput steering;
        steering = this.Pursue();
        steering.linear = -steering.linear;
        return steering;
    }


    public double mapToRange(float rot){
        //return Math.PI * angle / 180.0;

        while (Mathf.PI < rot) {
            rot -= 2 * Mathf.PI;
        }


        while (-Mathf.PI > rot) {
            rot += 2 * Mathf.PI;
        }


        return rot;
    }


    public SteeringOutput Face() {
        // Create the structure to hold our output
        SteeringOutput steering = new SteeringOutput();
        
        Vector3 dir = target.position - agent.position;
        if (dir.magnitude == 0) {
            steering.angular = 0;
            return steering;
        }

        // Get the naive direction to the target
        float angle = Mathf.Atan2(dir.x, dir.z);
        //float rotation = target.orientation - agent.orientation;
        float rotation = angle - agent.orientation;

        // Map the result to the (-pi, pi) interval
        double rot = mapToRange(rotation);
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA) {
            // return None
            agent.rotation = 0;
        }

        //float targetRotation = 0;
        // If we are outside the slowRadius, then use maximum rotation
        if(slowRadiusA < rotationSize){
            target.rotation = maxRotation;
            //targetRotation = maxRotation;
        }
        else{ // Otherwise calculate a scaled rotation
            target.rotation = maxRotation * rotationSize / slowRadiusA;
        }

        // The final target rotation combines speed (already in the variable) and direction
        target.rotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        steering.angular = target.rotation - agent.rotation;
        steering.angular /= timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(steering.angular);
        if (angularAcceleration > maxAngularAcceleration) {
            steering.angular /= angularAcceleration;
            steering.angular *= maxAngularAcceleration;
        }

        //Vector3 a = new Vector3(0.0F, 0.0F, 0.0F);
        //steering.linear = agent.position - agent.position;
        steering.linear = agent.position;//Vector3.zero;
        return steering;

        
    }

/* 
    public SteeringOutput Wander(){

        float randVar = Random.value - Random.value;

        // 1. Calculate the target to delegate to face
        // Update the wander orientation
        wanderOrientation += randVar * wanderRate;

        // Calculate the combined target orientation
        target.orientation = wanderOrientation + agent.orientation;

        // Calculate the center of the wander circle
        float x = Mathf.Sin(agent.orientation);
        float z = Mathf.Cos(agent.orientation);

        Vector3 tmpV = new Vector3(x, 0, z);
        Vector3 loc = agent.position + wanderOffset * tmpV;
        agent.DrawCircle(loc, wanderRadius);

        // Calculate the target location
        x = Mathf.Sin(orientation);
        z = Mathf.Cos(orientation);
        Vector3 tmpV2 = new Vector3(x, 0, z);
        loc += wanderRadius * tmpV2;

        Vector3 dir = position - agent.position;

        // 2. Delegate to face
        steering = Face.getSteering();


        // 3. Now set the linear acceleration to be at full acceleration in the direction of the orientation
        float x = Mathf.Sin(agent.orientation);
        float z = Mathf.Cos(agent.orientation);

        Vector3 tmpV = new Vector3(x, 0, z);

        steering.linear = maxAcceleration * tmpV;


        return steering;

    }
*/
}
