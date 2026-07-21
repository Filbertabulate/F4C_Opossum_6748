using System.Collections;
using UnityEngine;

public class MeteorStrikeAbility : MonoBehaviour
{
    [Header("Ability Settings")]
    public GameObject meteorPrefab;
    public int goldCost = 50;
    
    [Header("Meteor Shower Settings")]
    public int numberOfMeteors = 15;        // Total meteors to drop
    public float spawnAreaWidth = 160f;      // How wide the drop zone is (X-axis)
    public float spawnHeight = 12f;         // How high up they spawn (Y-axis)
    public float delayBetweenSpawns = 0.1f; // Time delay between each meteor
    
    [Header("Dependencies")]
    public EconomySystem economySystem; 

    public void CastMeteorStrike()
    {
        if (economySystem.TrySpendGold(goldCost)) 
        {
            // Start the Coroutine to trigger the shower sequence
            StartCoroutine(SpawnMeteorShower());
            Debug.Log("Meteor Shower Cast! " + goldCost + " Gold spent.");
        }
        else
        {
            Debug.Log("Meteor Strike failed: Not enough gold.");
        }
    }

    // A Coroutine allows us to pause code execution (yield) to create a delay
    private IEnumerator SpawnMeteorShower()
    {
        for (int i = 0; i < numberOfMeteors; i++)
        {
            // Pick a random X position between the left and right bounds of our width
            float randomX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
            
            // Set the spawn position high in the air
            Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0f);
            
            // Spawn one meteor
            Instantiate(meteorPrefab, spawnPosition, meteorPrefab.transform.rotation);            
            // Wait for a fraction of a second before looping to spawn the next one
            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }
}