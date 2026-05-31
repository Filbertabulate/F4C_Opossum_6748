using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Create a variable that is able to access all possible enemy units that we can spawn,
    // from a folder declared in the hierarchy called "EnemyUnits", where we can add all the enemy 
    // unit prefabs that we want to spawn in the game.
    private GameObject[] enemyUnits;
    // Currently the set up needs me to do maunal assignment of the size of the array and the elements in the 
    // Unity editor, which is not ideal. thus I plan to switch to using a Resource folder to store the enemy 
    // unit prefabs, and then we can load all the prefabs in that folder into the array at startup.

    // We also need a variable to store the spawn point of the enemy units, which is a transform 
    // that we can set in the Unity editor.
    // Using Transform since it is a postion based variable, where we care mainly about the x, y positions
    // of said spawn point.
    public Transform spawnPoint;

    // Get the Enemy Base HealthBar that can be used later onwards since we want to stop spawnning units
    // if the based is destroyed
    public HealthSystem EnemyBaseHealth;

    // For now, I will set the spawnning interval to be a fixed value of 25 seconds, 
    // which means that every 25 seconds, a random unit will spawn. (Trial and error for now)
    // Eventually, we can make this more complex by having different spawn intervals 
    // for different types of units, where it could be based on the monearty values the enemy has
    // or based on the number of defeated units to unlock stronger units to spawn more frequently.
    public float spawnInterval = 25f;

    // Since we have the spawn interval, similar to the idea of waiting for the unit to cooldown their attack
    // we need to have a timer to track that 5f "seconds" has passed before we can spawn the next unit.
    // We set it to 0 for now since we want the first unit to spawn at the start of the game, 
    // but we can also set it to spawnInterval.
    private float spawnTimer = 0f;

    // Since I need to differentiate which unit comes first, I need a tracker to tag each spawned unit
    private long nextSpawnOrder = 0;


    private void Awake()
    {
        // Load all the enemy unit prefabs from the Resources/EnemyUnits folder into the enemyUnits array.
        // This way we can easily add or remove enemy unit prefabs from the folder without having to manually 
        // assign them in the Unity editor, which is more convenient and less error-prone.
        // The usage of Resources.LoadAll is based on the Unity documentation, where we can load all assets of a 
        // specific type from a folder in the Resources directory.
        // https://docs.unity3d.com/ScriptReference/Resources.LoadAll.html
        // As such, I had to create a folder called Resources in the Assets directory, and then create a subfolder
        // called EnemyUnits to store all the enemy unit prefabs that I want to spawn in the game.
        enemyUnits = Resources.LoadAll<GameObject>("EnemyUnits");
    }

    // Update is called once per frame
    private void Update()
    {
        // If the based of the enemy (us in this case has been destroyed), stop spawning any more units
        // This is becuase the referece to the base has been destroyed, thus the refernce no longer points
        // to a base, which means the value becomes null.
        if (EnemyBaseHealth == null)
        {
            return;
        }

        // First we reduce the spawn timer by the time that has passed since the last frame, 
        // which is given by Time.deltaTime. this way if the spawnTimer is <= 0, it
        // means that the spawn interval has passed and we can spawn a new unit.
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            // We call the method spawnEnemyUnit to spawn a random unit from the enemyUnits array.
            spawnEnemyUnit();

            // After spawning the unit, we reset the spawn timer to the spawn interval, 
            // so that we can start counting down for the next spawn.
            spawnTimer = spawnInterval;
        }
    }

    // Create a method call spawnEnemyUnit, where we can call this method to spawn a specific unit, 
    // which will be useful for the enemy base to spawn units. (current based on time only)
    private void spawnEnemyUnit()
    {
        // Since the enemyUnits array can be empty, i.e. in the hierarchy we have not assigned any enemy units
        // to the spawner folder, we should add a check to prevent errors from trying to access an element from an 
        // empty array.
        if (enemyUnits.Length == 0)
        {
            Debug.LogWarning("No enemy units assigned to the spawner!");
            return;
        }

        // We want to spawn a random unit from the enemyUnits array, so we can use Random.Range to get a random index.
        int randomEnemyUnit = Random.Range(0, enemyUnits.Length);
        GameObject unitToSpawn = enemyUnits[randomEnemyUnit];

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
