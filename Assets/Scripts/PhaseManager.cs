using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PhaseManager is the place to keep a succession of events or "phases" when building 
/// a multi-step AI demo. This is essentially a state variable for the map (aka level)
/// itself, not the state of any particular NPC.
/// 
/// Map state changes could happen for one of two reasons:
///     when the user has pressed a number key 0..9, desiring a new phase
///     when something happening in the game forces a transition to the next phase
/// 
/// One use will be for AI demos that are switched up based on keyboard input. For that, 
/// the number keys 0..9 will be used to dial in whichever phase the user wants to see.
/// </summary>

public class PhaseManager : MonoBehaviour
{
    // Set prefabs
    public GameObject PlayerPrefab;     // You, the player
    public GameObject HunterPrefab;     // Agent doing chasing
    public GameObject WolfPrefab;       // Agent getting chased
    public GameObject RedPrefab;     // reserved for future use
    // public GameObject BluePrefab;    // reserved for future use

    public NPCController house;         // THis goes away

    // Set up to use spawn points. Can add more here, and also add them to the 
    // Unity project. This won't be a good idea later on when you want to spawn
    // a lot of agents dynamically, as with Flocking and Formation movement.

    public GameObject spawner1;
    public Text SpawnText1;
    public GameObject spawner2;
    public Text SpawnText2;
    public GameObject spawner3;
    public Text SpawnText3;

    private List<GameObject> spawnedNPCs;   // When you need to iterate over a number of agents.

    private int currentMapState = 0;           // This stores which state the map or level is in.
    private int previousMapState = 0;          // The map state we were just in

    public int MapState => currentMapState;

    LineRenderer line;                  // GOING AWAY

    public GameObject[] Path;


    public Text narrator;                   // 

    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.

    void Start()
    {
        narrator.text = "This is the place to mention major things going on during the demo, the \"narration.\"";
        spawnedNPCs = new List<GameObject>();
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 3)); // case 3 here <--------
        //spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 4)); // case 4 here <--------

        Invoke("SpawnWolf", 12);
        Invoke("Meeting1", 30);
    }

    /// <summary>
    /// This is where you put the code that places the level in a particular state.
    /// Unhide or spawn NPCs (agents) as needed, and give them things (like movements)
    /// to do. For each case you may well have more than one thing to do.
    /// </summary>
    private void Update()
    {

        string inputstring = Input.inputString;
        int num;

        // Look for a number key click
        if (inputstring.Length > 0)
        {
            Debug.Log(inputstring);

            if (Int32.TryParse(inputstring, out num))
            {
                if (num != currentMapState)
                {
                    previousMapState = currentMapState;
                    currentMapState = num;
                }
            }
        }

        // Check if a game event had caused a change of state in the level.
        if (currentMapState == previousMapState)
            return;

        // If we get here, we've been given a new map state, from either source
        switch (currentMapState)
        {
            case 0:
                EnterMapStateZero();
                break;

            case 1:
                EnterMapStateOne();
                break;

            case 2:
                EnterMapStateTwo();
                break;

            case 3:
                break;

                // ADD MORE CASES AS NEEDED
        }
    }

    private void EnterMapStateZero()
    {
        narrator.text = "In MapState Zero, we're going to ...";

        //currentMapState = 2; // or whatever. Won't necessarily advance the phase every time

        //spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }

    private void EnterMapStateOne()
    {
        narrator.text = "In MapState One, we're going to ...";

        //currentMapState = 2; // or whatever. Won't necessarily advance the phase every time

        //spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }

    private void EnterMapStateTwo()
    {
        narrator.text = "Entering MapState Two";

        currentMapState = 3; // or whatever. Won't necessarily advance the phase every time

        //spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }
    private void EnterMapStateThree()
    {
        narrator.text = "Entering MapState Three";

        currentMapState = 2; // or whatever. Won't necessarily advance the phase every time

        //spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }


    // ... Etc. Etc.

    /// <summary>
    /// SpawnItem placess an NPC of the desired type into the game and sets up the neighboring 
    /// floating text items nearby (diegetic UI elements), which will follow the movement of the NPC.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="spawnPrefab"></param>
    /// <param name="target"></param>
    /// <param name="spawnText"></param>
    /// <param name="mapState"></param>
    /// <returns></returns>
    private GameObject SpawnItem(GameObject spawner, GameObject spawnPrefab, NPCController target, Text spawnText, int mapState)
    {
        Vector3 size = spawner.transform.localScale;
        Vector3 position = spawner.transform.position + new Vector3(UnityEngine.Random.Range(-size.x / 2, size.x / 2), 0, UnityEngine.Random.Range(-size.z / 2, size.z / 2));
        GameObject temp = Instantiate(spawnPrefab, position, Quaternion.identity);
        if (target)
        {
            temp.GetComponent<SteeringBehavior>().target = target;
        }
        temp.GetComponent<NPCController>().label = spawnText;
        temp.GetComponent<NPCController>().mapState = mapState;         // This is separate from the NPC's internal state
        Camera.main.GetComponent<CameraController>().player = temp;
        return temp;
    }


    // These next two methods show spawning an agent might look.
    // You make them happen when you want to by using the Invoke() method.
    // These aren't needed for the first assignment.

    private void SpawnWolf()
    {
        narrator.text = "The Wolf appears. Most wolves are ferocious, but this one is docile.";
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }
    private void Meeting1()
    {
        narrator.text = "The Wolf and Hunter meet...";
        // put more actions in here
    }

    // Here is an example of a method you might want for when an arrival actually happens.
    private void SetArrive(GameObject character)
    {

        character.GetComponent<NPCController>().mapState = 3; // Whatever the new map state is after arrival
        character.GetComponent<NPCController>().DrawConcentricCircle(character.GetComponent<SteeringBehavior>().slowRadiusL);
    }

    // Following the above examples, write whatever methods you need that you can bolt together to 
    // create more complex movement behaviors.

    // YOUR CODE HERE

    // Vestigial. Maybe you'll find it useful.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawner1.transform.position, spawner1.transform.localScale);
        Gizmos.DrawCube(spawner2.transform.position, spawner2.transform.localScale);
        Gizmos.DrawCube(spawner3.transform.position, spawner3.transform.localScale);
    }
}
