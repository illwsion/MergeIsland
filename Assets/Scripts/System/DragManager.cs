// DragManager.cs
using UnityEngine;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance { get; private set; }
    public bool IsDragging { get; private set; }
    public GameObject inputBlocker;

    void Update()
    {
        inputBlocker.SetActive(DragManager.Instance.IsDragging);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartDrag(){
        IsDragging = true;
    }
    public void EndDrag(){
        IsDragging = false;
    }
}
