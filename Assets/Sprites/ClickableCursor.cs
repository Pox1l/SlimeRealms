using UnityEngine;

public class ClickableCursor : MonoBehaviour
{
    [Header("Nastavení kurzorù")]
    public Texture2D normalCursor;   // Obrázek pro normální stav
    public Texture2D clickCursor;    // Obrázek pøi kliknutí

    [Header("Bod kliknutí")]
    // Bod, kterým se reálnì kliká (napø. špièka prstu).
    // X a Y jsou v pixelech od levého horního rohu obrázku.
    public Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        // Na zaèátku nastavíme základní kurzor
        SetNormalCursor();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            SetClickCursor();
        }
       
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
         
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                SetNormalCursor();
            }
        }

    }

    // Pomocná funkce pro nastavení normálního kurzoru
    void SetNormalCursor()
    {
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
    }

    // Pomocná funkce pro nastavení klikacího kurzoru
    void SetClickCursor()
    {
        // Pøi kliknutí mùžeme mírnì posunout hotspot, pokud se mìní tvar ruky
        // Pro jednoduchost používáme stejný, ale mùžeš si vytvoøit 'clickHotSpot'.
        Cursor.SetCursor(clickCursor, hotSpot, CursorMode.Auto);
    }
}