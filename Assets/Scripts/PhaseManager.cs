using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

public class PhaseManager : MonoBehaviour {
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

    private List<GameObject> spawnedNPCs;      // When you need to iterate over a number of agents.

    private int seekState = 1;
    private int fleeState = 2;
    private int pursueState = 3;
    private int evadeState = 4;
    private int faceState = 5;
    private int alignState = 6;
    private int wanderState = 7;
    private int staticState = 8;

    private int currentMapState = 0;           // This stores which state the map or level is in.
    private int previousMapState = 0;          // The map state we were just in
    public int MapState => currentMapState;

    LineRenderer line;                  // GOING AWAY
    public GameObject[] Path;
    public Text narrator;

    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.

    void Start() {
        narrator.text = @"Press 1-7: {1: Seek/Flee, 
        3: Pursue, 4: Evade, 5: Face, 6: Align, 7: Wander, 0: Restart}";
        spawnedNPCs = new List<GameObject>();
    }

    /// <summary>
    /// This is where you put the code that places the level in a particular state.
    /// Unhide or spawn NPCs (agents) as needed, and give them things (like movements)
    /// to do. For each case you may well have more than one thing to do.
    /// </summary>
    private void Update() {
        previousMapState = currentMapState;
        string inputstring = Input.inputString;
        // Look for a number key click
        if (inputstring.Length > 0) {
            if (Int32.TryParse(inputstring, out int num)) {
                if (num == 0) SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reset
                if (num == 1) EnterMapStateOne();   // Seek
                if (num == 2) EnterMapStateTwo();   // Flee
                if (num == 3) EnterMapStateThree(); // Pursue
                if (num == 4) EnterMapStateFour();  // Evade
                if (num == 5) EnterMapStateFive();  // Face
                if (num == 6) EnterMapStateSix();   // Align
                if (num == 7) EnterMapStateSeven(); // Wander
                if (num == 8) EnterMapStateEight(); // Static
            }
        }
        // Check if a game event had caused a change of state in the level.
        //if (currentMapState == previousMapState) return;
        //if (currentMapState != previousMapState) DestroyNPCs();
    }

    private void EnterMapStateOne() { // Seek
        DestroyNPCs();
        currentMapState = 1;
        narrator.text = "In MapState Zero, the hunter will seek the fleeing wolf o.o\nSEEKing you out";

        SpawnNPC(spawner1, HunterPrefab, SpawnText1, seekState);
        SpawnNPC(spawner2, WolfPrefab, SpawnText2, fleeState); // Spawn a fleeing wolf (mapState 2)

        // Set targets to each other
        SetTarget(spawnedNPCs[0], spawnedNPCs[1]);
        SetTarget(spawnedNPCs[1], spawnedNPCs[0]);

        //setTarget(0,1); 
        //setTarget(1,0);
    }

    private void EnterMapStateTwo() { // Flee
        DestroyNPCs();
        currentMapState = 2;
        narrator.text = "In MapState Zero, the wolf will flee the hunter 0.0";

        SpawnNPC(spawner1, HunterPrefab, SpawnText1, fleeState); // Spawn a fleeing hunter (mapState 2)
        SpawnNPC(spawner2, WolfPrefab, SpawnText2, staticState);

        SetTarget(spawnedNPCs[0], spawnedNPCs[1]);
        SetTarget(spawnedNPCs[1], spawnedNPCs[0]);
    }

    private void EnterMapStateThree() { //Pursue
        DestroyNPCs();
        currentMapState = 3;
        narrator.text = "In MapState Two, we're going to Pursue >:) !";
        //HunterPrefab.GetComponent<NPCController>()
        SpawnNPC(spawner1, WolfPrefab, SpawnText1, pursueState);
        SpawnNPC(spawner2, HunterPrefab, SpawnText2, evadeState); // Spawn an Evading wolf (mapState 4)
        //spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText2, 3));
        SetTarget(spawnedNPCs[0], spawnedNPCs[1]);
        SetTarget(spawnedNPCs[1], spawnedNPCs[0]);
    }

    private void EnterMapStateFour() { // Evade
        DestroyNPCs();
        currentMapState = 4;
        narrator.text = "In MapSate Three, we're going to Evade Dx";

        SpawnNPC(spawner1, HunterPrefab, SpawnText1, evadeState);
        SpawnNPC(spawner2, WolfPrefab, SpawnText2, staticState); // Spawn a Static wolf (mapState 8)

        SetTarget(spawnedNPCs[0], spawnedNPCs[1]);
        SetTarget(spawnedNPCs[1], spawnedNPCs[0]);
       
        //setTarget(0,1);
    }

    private void EnterMapStateFive() { // Face
        DestroyNPCs();
        currentMapState = 5;
        narrator.text = "In MapSate Four, we're going to Face with the Player";

        SpawnNPC(spawner1, HunterPrefab, SpawnText1, faceState);
        SpawnNPC(spawner2, WolfPrefab, SpawnText2, staticState); // Spawn a Static wolf (mapState 8)

        SetTarget(spawnedNPCs[0], spawnedNPCs[1]); // Set the target of Face to the static NPC
        //SetTarget(spawnedNPCs[1], spawnedNPCs[0]);
        //setTarget(0,1); 
    }

    private void EnterMapStateSix() {  // Align
        DestroyNPCs();
        currentMapState = 6;
        narrator.text = "Align with Wander";

        SpawnNPC(spawner1, HunterPrefab, SpawnText1, alignState);
        SpawnNPC(spawner2, WolfPrefab, SpawnText2, staticState); // Spawn a Static wolf (mapState 8)

        SetTarget(spawnedNPCs[0], spawnedNPCs[1]);  // Set the target of align to the wandering NPC
        SetTarget(spawnedNPCs[1], spawnedNPCs[0]);
    }

    private void EnterMapStateSeven() { // Wander
       DestroyNPCs();
       this.currentMapState = 7;
       narrator.text = "Let's  Wander!";
       //SpawnText1.text = "WE OUT HERE CRIPPING";
       SpawnText2.text = "WE OUT HERE CRIPPING";
       SpawnNPC(spawner2, WolfPrefab, SpawnText2, wanderState); // Spawn wandering NPC

    }

    private void EnterMapStateEight() {  // Static
        DestroyNPCs();
        currentMapState = 8;
        narrator.text = "Statics";
        SpawnNPC(spawner1, WolfPrefab, SpawnText1, staticState); // Spawn Static NPC
    }

    /// <summary>
    /// SpawnItem places an NPC of the desired type into the game and sets up the neighboring 
    /// floating text items nearby (diegetic UI elements), which will follow the movement of the NPC.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="spawnPrefab"></param>
    /// <param name="target"></param>
    /// <param name="spawnText"></param>
    /// <param name="mapState"></param>
    /// <returns></returns>
    private GameObject SpawnItem(GameObject spawner, GameObject spawnPrefab, NPCController target, Text spawnText, int mapState) {
        Vector3 size = spawner.transform.localScale;
        Vector3 position = spawner.transform.position + new Vector3(UnityEngine.Random.Range(-size.x / 2, size.x / 2), 0, UnityEngine.Random.Range(-size.z / 2, size.z / 2));
        GameObject temp = Instantiate(spawnPrefab, position, Quaternion.identity); 
        if (target) {
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

    private void SpawnNPC(GameObject spawnLocation, GameObject NPCprefab, Text spawnText, int mapState, NPCController targetPrefab=null) {
        // Spawns NPC
        //GameObject target = SpawnItem(spawner1, targetPrefab, ag, SpawnText2, 2);
        GameObject agent = SpawnItem(spawnLocation, NPCprefab, targetPrefab, spawnText, mapState);
        //agent.GetComponent<SteeringBehavior>().target = target;
        spawnedNPCs.Add(agent);
        //spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText2, 3));
    }

    private void DestroyNPCs() {
        foreach (GameObject NPCs in spawnedNPCs) {
            Destroy(NPCs);
        }
        spawnedNPCs.Clear();
    }

    public void SetTarget(GameObject agent, GameObject target) {
        agent.GetComponent<SteeringBehavior>().target = target.GetComponent<NPCController>();
    }

    private void setTarget(int agent, int target) {
        spawnedNPCs[agent].GetComponent<SteeringBehavior>().target = spawnedNPCs[target].GetComponent<NPCController>();
    }

    // Here is an example of a method you might want for when an arrival actually happens.
    private void SetArrive(GameObject character) {
        character.GetComponent<NPCController>().mapState = 0; // Whatever the new map state is after arrival
        character.GetComponent<NPCController>().DrawConcentricCircle(character.GetComponent<SteeringBehavior>().slowRadiusL);
    }

    // Following the above examples, write whatever methods you need that you can bolt together to 
    // create more complex movement behaviors.

    // YOUR CODE HERE

    // Vestigial. Maybe you'll find it useful.
    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawner1.transform.position, spawner1.transform.localScale);
        Gizmos.DrawCube(spawner2.transform.position, spawner2.transform.localScale);
        Gizmos.DrawCube(spawner3.transform.position, spawner3.transform.localScale);
    }
}
