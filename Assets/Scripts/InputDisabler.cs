using Match3Game;
using UnityEngine;

public class InputDisabler : MonoBehaviour
{
    public void DisableInput()
    {
        Gem[] allGems = FindObjectsOfType<Gem>();
        foreach (Gem gem in allGems)
        {
            Collider2D col = gem.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }

    public void EnableInput()
    {
        Gem[] allGems = FindObjectsOfType<Gem>();
        foreach (Gem gem in allGems)
        {
            Collider2D col = gem.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
        }
    }
}
