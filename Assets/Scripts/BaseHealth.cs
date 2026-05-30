using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    // Set up the base's health points, tempoary value
    public int hp = 100;

    // Creating a method called TakeDamage, where if the base takes damage, 
    // it will reduce the hp by the damage amount and print the current hp 
    // to the console. If the hp is less than or equal to 0, 
    // it will print a message saying that the enemy base is destroyed.
    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. HP left: " + hp);

        if (hp <= 0)
        {
            Debug.Log("Enemy base destroyed!");
            Destroy(gameObject);
        }
    }
}
