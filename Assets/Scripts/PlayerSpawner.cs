using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    // Create a variable that is able to access all possible player units that we can spawn,
    // from a folder declared in the hierarchy called "PlayerUnits", where we can add all the player 
    // unit prefabs that we want to spawn in the game.
    private GameObject[] playerUnits;
    // Currently the set up needs me to do maunal assignment of the size of the array and the elements in the 
    // Unity editor, which is not ideal. thus I plan to switch to using a Resource folder to store the player 
    // unit prefabs, and then we can load all the prefabs in that folder into the array at startup.

    // We also need a variable to store the spawn point of the player units, which is a transform 
    // that we can set in the Unity editor.
    // Using Transform since it is a postion based variable, where we care mainly about the x, y positions
    // of said spawn point.
    public Transform spawnPoint;

    // Initially did not wanted to get this spawn cooldown, but since there is currently collision if I
    // do spawning of units too fast, a temporary fix is to have a cool down for spawn to prevent units jittering
    // and not being able to move.
    public float spawnCooldown = 0.5f;
    private float spawnCooldownTimer = 0f;

    // Get the player Base HealthBar that can be used later onwards since we want to stop spawnning units
    // if the based is destroyed
    public HealthSystem playerBaseHealth;

    // Since I need to differentiate which unit comes first, I need a tracker to tag each spawned unit
    private long nextSpawnOrder = 0;

    // For our player spawnning, for now I want it to be spawn by the user pressing a keyboard key, but
    // eventually I will create a toolbar or smthing similar for the user to press to spawn the character
    // they want to spawn.
    // For now, for the user to spawn a player, I will set the key to be press to be "z".
    // public KeyCode spawnKey = KeyCode.Z;
    // Forgo this first as it seme i need to change some setting, so I just did the old method of 
    // keyboard.current.zKey.wasPressedThisFrame to do the spawning instead for now.

    private void Awake()
    {
        // Load all the player unit prefabs from the Resources/PlayerUnits folder into the playerUnits array.
        // This way we can easily add or remove player unit prefabs from the folder without having to manually 
        // assign them in the Unity editor, which is more convenient and less error-prone.
        // The usage of Resources.LoadAll is based on the Unity documentation, where we can load all assets of a 
        // specific type from a folder in the Resources directory.
        // https://docs.unity3d.com/ScriptReference/Resources.LoadAll.html
        // As such, I had to create a folder called Resources in the Assets directory, and then create a subfolder
        // called PlayerUnits to store all the player unit prefabs that I want to spawn in the game.
        playerUnits = Resources.LoadAll<GameObject>("PlayerUnits");
    }

    // Update is called once per frame
    private void Update()
    {
        // For tracking if can spawn unit again
        spawnCooldownTimer -= Time.deltaTime;

        // If the user has selected to spawn the unit by pressing the key "z", then the methond SpawnPlayerUnit()
        // will spawn a random unit from the resource folder of the units avaiable.
        if (Keyboard.current.zKey.wasPressedThisFrame  && spawnCooldownTimer <= 0f)
        {
            SpawnPlayerUnit();
            spawnCooldownTimer = spawnCooldown;
        }
    }

    // Create a method call spawnPlayerUnit, where we can call this method to spawn a specific unit, 
    // which will be useful for the player base to spawn units. (current based on time only)
    private void SpawnPlayerUnit()
    {
        // Since the playerUnits array can be empty, i.e. in the hierarchy we have not assigned any player units
        // to the spawner folder, we should add a check to prevent errors from trying to access an element from an 
        // empty array.
        if (playerUnits.Length == 0)
        {
            Debug.LogWarning("No player units assigned to the spawner (Resources Folder in Assets)!");
            return;
        }

        // We want to spawn a random unit from the playerUnits array, so we can use Random.Range to get a random index.
        int randomPlayerUnit = Random.Range(0, playerUnits.Length);
        GameObject unitToSpawn = playerUnits[randomPlayerUnit];

        // Then we can instantiate the selected unit at the spawn point's position and rotation.
        // https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
        // Instantiate is a method that creates a copy of the given object, in this case, the unitToSpawn, 
        // at the specified position and rotation.
        // This way I only need to create one of that unit in the hierarchy, and I can spawn as many as I want 
        // by instantiating it.
        // Why I am storing the value of the spawned Unit is becuase I need to updates its spawn Order value
        // in its UnitMove Script so that the is Ally tracking method works as indented.
        GameObject spawnedUnit = Instantiate(unitToSpawn, spawnPoint.position, spawnPoint.rotation);

        // Obtain the UnitMove script form the spawned unit, if there is.
        UnitMove unitMove = spawnedUnit.GetComponent<UnitMove>();

        // If such a script is available in this ally unit, then I will define its spawn Order value as such
        // And increment the next spawnOrder value up by one to keep it unique, where lower spawnOrder number
        // means the unit was spawned first
        if (unitMove != null)
        {
            unitMove.InitialiseSpawnOrder(nextSpawnOrder);
            nextSpawnOrder++;
        }
    }
}