using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
