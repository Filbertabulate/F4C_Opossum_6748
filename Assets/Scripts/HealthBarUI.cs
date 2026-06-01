using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HealthBarUI : MonoBehaviour
{
    // Following the tutorial from this video: https://www.youtube.com/watch?v=lYZayXViTN8

    // Create an enum to easily switch orientation from the Unity Inspector
    // Adding this in since the bases HP bare are vertical compared to the units
    public enum BarOrientation { Horizontal, Vertical }

    // Needing to create a default BarOrientation where it is usally Horizontal since
    // there is only two bases usually
    public BarOrientation orientation = BarOrientation.Horizontal;

    public float health;
    public float maxHealth;
    public float healthBarWidth;
    public float healthBarHeight;

    [SerializeField] 
    private RectTransform healthBar;

    // Function to set max health value, taking in the maxHealth value from the HealthBar script and 
    // assigning it to the maxHealth variable in this script.
    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
    }

    // Create a set health function that takes in the current health value from the HealthBar script and 
    // assigns it to the health variable in this script.
    // We will also use this function to update the health bar's width based on the current health value 
    // in relation to the max health value.
    public void SetHealth(float health)
    {
        this.health = health;
        if (orientation == BarOrientation.Horizontal)
        {
            // Calculate width for horizontal right-to-left drop
            float healthPercentageWidth = (health / maxHealth) * healthBarWidth;
            
            // Update the health bar's width based on the current health percentage.
            // We dont need to change the height of the health bar.
            healthBar.sizeDelta = new Vector2(healthPercentageWidth, healthBarHeight);
        }
        else if (orientation == BarOrientation.Vertical)
        {
            // Calculate height for vertical top-to-bottom drop
            float healthPercentageHeight = (health / maxHealth) * healthBarHeight;

            // Update the health bar's height based on the current health percentage.
            // we dont need to change the width of the health bar.
            healthBar.sizeDelta = new Vector2(healthBarWidth, healthPercentageHeight);
        }
    }
}
