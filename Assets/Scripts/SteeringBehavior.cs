using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* Alberto Mejia
 * Milena Xochito Gonzalez 
 */

/// <summary>
/// This is where all of the various steering behavior methods are.
/// </summary>

public struct SteeringOutput {
    public Vector3 linear;
    public float angular;
}

public class SteeringBehavior : MonoBehaviour {
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

    protected void Start() {
        agent = GetComponent<NPCController>();
        //wanderOrientation = agent.orientation;
    }

    /* These functions follow the pseudo-code in the book
     * AI for Games by Ian Millington
     */

    public SteeringOutput Seek(){
        // Create the structure to hold our output
        SteeringOutput steering = new SteeringOutput
        {
            linear = target.position - agent.position
        };
        steering.linear.Normalize();
        steering.linear *= maxAcceleration;
        steering.angular = Face().angular;
        return steering;
    }

    public SteeringOutput Flee() {
        SteeringOutput tmp = Seek();
        return new SteeringOutput { linear = -1 * tmp.linear }; //, angular = FaceAway().angular }; <- When wall collision is implemented add
    }

    public SteeringOutput DynamicArrive(Vector3 direction) {
        //// Create the structure to hold our output
        //SteeringOutput steering = new SteeringOutput();

        //// Get the direction to the target
        ////Vector3 direction = target.position - agent.position;
        //float distance = direction.magnitude;
        //float targetSpeed;

        //// Check if we are there, return no steering
        //if (distance < targetRadiusL) {
        //    targetSpeed = 0;
        //    steering.linear = Vector3.zero;
        //    return steering;
        //}
        //// If we are outside the slowRadius, then go max speed
        //else if (distance > slowRadiusL) {
        //    targetSpeed = maxSpeed;
        //}
        //// Otherwise calculate a scaled speed
        //else {
        //    targetSpeed = maxSpeed * distance / slowRadiusL;
        //}

        //// The target velocity combines speed and direction
        //Vector3 targetVelocity = direction;
        //targetVelocity.Normalize();
        //targetVelocity *= targetSpeed;

        //// Acceleration tries to get to the target velocity
        //steering.linear = targetVelocity - agent.velocity;
        //steering.linear = steering.linear / timeToTarget;

        //// Check if the acceleration is too fast
        //if (steering.linear.magnitude > maxAcceleration) {
        //    steering.linear.Normalize();
        //    steering.linear *= maxAcceleration;
        //}

        //// Output the steering 
        //return steering;

        target.DrawConcentricCircle(slowRadiusL);
        Vector3 targetVelocity = maxSpeed * (direction / slowRadiusL);
        Vector3 difference = targetVelocity - agent.velocity;
        targetVelocity = Vector3.ClampMagnitude((difference / timeToTarget), maxAcceleration);
        return new SteeringOutput() { linear = targetVelocity };
    }

    public SteeringOutput Pursue(Vector3 distance) {
        float prediction;
        Vector3 linearTarget = new Vector3();

        target.DrawConcentricCircle(targetRadiusL);

        float speed = agent.velocity.magnitude;
        if (speed <= (distance.magnitude / maxPrediction)) {
            prediction = maxPrediction;
        }
        else {
            prediction = (distance.magnitude / speed);
        }
        linearTarget += (target.position + target.velocity * prediction) - agent.position;
        agent.DrawCircle(linearTarget+agent.position, 1);
        
        linearTarget.Normalize();
        linearTarget *= maxAcceleration;
        return new SteeringOutput() { linear = linearTarget , angular = Face().angular};
    }

    public SteeringOutput PursueWithDynamicArrive() {
        Vector3 direction = target.position - agent.position; // The distance to target
        if (direction.magnitude < targetRadiusL) {
            return DynamicArrive(direction);  // If the distance between target and agent is smaller than the large radius, then arrive
        }
        else {
            return Pursue(direction);
        }
    }

    public SteeringOutput Evade() {
        SteeringOutput tmp = Pursue(target.position - agent.position);
        return new SteeringOutput { linear = -1*(tmp.linear+agent.position), angular = FaceAway().angular};

        //float prediction;
        //Vector3 direction = target.position - agent.position;
        //Vector3 linearTarget = new Vector3();

        //target.DrawConcentricCircle(targetRadiusL);

        //float speed = agent.velocity.magnitude;
        //if (speed <= (direction.magnitude / maxPrediction)) {
        //    prediction = maxPrediction;
        //}
        //else {
        //    prediction = (direction.magnitude / speed);
        //}
        //linearTarget += (target.position + target.velocity * prediction) ;
        //agent.DrawCircle(linearTarget, 1);
        //linearTarget.Normalize();
        //linearTarget *= maxAcceleration;
        //return new SteeringOutput() { linear = -1*linearTarget , angular = FaceAway().angular};
    }

    public float mapToRange(float rotation) {
        rotation = rotation % (2 * Mathf.PI);
        float turningDist = rotation - agent.orientation;
        if (Mathf.Abs(turningDist) < targetRadiusA) {
            float targetSpeed = maxRotation * (turningDist / slowRadiusA);
            float difference = targetSpeed - agent.rotation;
            turningDist = (difference / timeToTarget);
        }
        return turningDist;
    }

    public SteeringOutput Face() {
        Vector3 dir = target.position - agent.position;
        // Get the naive direction to the target
        float angle = Mathf.Atan2(dir.x, dir.z);
        return new SteeringOutput { angular = mapToRange(angle) };
    }

    public SteeringOutput FaceAway() {
        Vector3 dir = agent.position - target.position;
        float angle = Mathf.Atan2(dir.x, dir.z);
        return new SteeringOutput { angular = mapToRange(angle) };
    }

    public SteeringOutput Align() {
        float angle = target.orientation;
        return new SteeringOutput { angular = mapToRange(angle) };
    }

    public float randomBinomial() {
        // returns a random number between −1 and 1, where values around zero are more likely.
        return UnityEngine.Random.value - UnityEngine.Random.value;
    }

    private Vector3 AngleToVector(float angle) {
        return new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
    }

    public SteeringOutput Wander() {
        SteeringOutput steering = new SteeringOutput();
        agent.DrawCircle(agent.position, wanderRadius * wanderOffset);
        float randVar = randomBinomial();

        // Calculate the target to delegate to face
        wanderOrientation += randVar * wanderRate; // Update the wander orientation

        // Calculate the combined target orientation
        float targetOrientation = wanderOrientation + agent.orientation;
        Vector3 wanderTarget = agent.position + wanderOffset * AngleToVector(agent.orientation);
        wanderTarget += wanderRadius * AngleToVector(targetOrientation);
        Vector3 direction = wanderTarget - agent.position;

        agent.DrawCircle(wanderTarget, 2);
        steering.angular = mapToRange(Mathf.Atan2(direction.x, direction.z));

        // Now set the linear acceleration to be at full acceleration in the direction of the orientation
        steering.linear = maxAcceleration * AngleToVector(agent.orientation);
        return steering;
    }
}
