using UnityEngine;


public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door }
    public ObjectType type;

    [Header("Cài đặt")]
    public float holdTime = 2f;

    public void PerformAction()
    {
      
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

    
    public string GetHintText()
    {
        if (type == ObjectType.Item) return "Giữ E để nhặt";
        if (type == ObjectType.Door)
        {
            if (GameManager.instance.currentItems >= 3) return "Vật phẩm: 3/3\nGiữ E để thoát";
            else return $"Chưa đủ đồ ({GameManager.instance.currentItems}/3)";
        }
        return "";
    }
}