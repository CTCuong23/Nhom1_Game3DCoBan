using UnityEngine;


public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door }
    public ObjectType type;

    [Header("Cài đặt")]
    public float holdTime = 2f;
    private float currentHoldTime = 0f;
    private bool isPlayerNearby = false;
    private bool isCollected = false;

    void Update()
    {
        if (isPlayerNearby && !isCollected)
        {
            
            if (type == ObjectType.Door && GameManager.instance.currentItems < 3) return;

           
            if (Input.GetKey(KeyCode.E))
            {
                currentHoldTime += Time.deltaTime;

                
                GameManager.instance.UpdateLoading(currentHoldTime, holdTime);
               

                if (currentHoldTime >= holdTime)
                {
                    PerformAction();
                }
            }
            else
            {
                
                if (currentHoldTime > 0)
                {
                    currentHoldTime = 0f;
                    GameManager.instance.StopLoading(); 
                }
            }
        }
    }

    void PerformAction()
    {
        isCollected = true;
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        if (type == ObjectType.Item)
        {
            GameManager.instance.CollectItem();
            Destroy(gameObject);
        }
        else if (type == ObjectType.Door)
        {
            GameManager.instance.WinGame();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            currentHoldTime = 0f;

            
            if (type == ObjectType.Item) GameManager.instance.ShowHint("Giữ E để nhặt");
            else if (type == ObjectType.Door)
            {
                if (GameManager.instance.currentItems >= 3) GameManager.instance.ShowHint("Vật phẩm: 3/3\nGiữ E để thoát");
                else GameManager.instance.ShowHint($"Chưa đủ đồ ({GameManager.instance.currentItems}/3)");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            currentHoldTime = 0f;
            GameManager.instance.HideHint();
            GameManager.instance.StopLoading(); 
        }
    }
}