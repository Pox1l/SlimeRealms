using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    
    public string objectID;

    private void Awake()
    {
        DontDestroy[] others = FindObjectsOfType<DontDestroy>();

        foreach (var obj in others)
        {
            if (obj != this && obj.objectID == this.objectID)
            {
                Destroy(gameObject);
                return;
            }
        }

        
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);
    }
}