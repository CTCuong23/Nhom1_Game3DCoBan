using UnityEngine;


public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door }
    public ObjectType type;

    [Header("Cài đặt")]
    public float holdTime = 2f;
    //private float currentHoldTime = 0f;
    //private bool isPlayerNearby = false;
    //private bool isCollected = false;

    //private bool isHintShowing = false;

    //void Update()
    //{
    //    if (isPlayerNearby && !isCollected)
    //    {
    //        if (!GameManager.instance.isAiming)
    //        {
    //            // Nếu đang loading dở thì hủy
    //            if (currentHoldTime > 0)
    //            {
    //                currentHoldTime = 0f;
    //                GameManager.instance.StopLoading();
    //            }

    //            // Ẩn gợi ý nếu đang hiện
    //            if (isHintShowing)
    //            {
    //                GameManager.instance.HideHint();
    //                isHintShowing = false;
    //            }

    //            return; // Dừng code tại đây, không cho làm gì thêm
    //        }

    //        if (!isHintShowing)
    //        {
    //            ShowInteractionHint();
    //            isHintShowing = true;
    //        }

    //        // Logic cửa: Nếu là cửa và chưa đủ đồ thì không cho giữ E
    //        if (type == ObjectType.Door && GameManager.instance.currentItems < 3) return;

    //        // Logic giữ E (như cũ)
    //        if (Input.GetKey(KeyCode.E))
    //        {
    //            currentHoldTime += Time.deltaTime;
    //            GameManager.instance.UpdateLoading(currentHoldTime, holdTime);

    //            if (currentHoldTime >= holdTime)
    //            {
    //                PerformAction();
    //            }
    //        }
    //        else
    //        {
    //            if (currentHoldTime > 0)
    //            {
    //                currentHoldTime = 0f;
    //                GameManager.instance.StopLoading();
    //            }
    //        }

    //        if (type == ObjectType.Door && GameManager.instance.currentItems < 3) return;

           
    //        if (Input.GetKey(KeyCode.E))
    //        {
    //            currentHoldTime += Time.deltaTime;

                
    //            GameManager.instance.UpdateLoading(currentHoldTime, holdTime);
               

    //            if (currentHoldTime >= holdTime)
    //            {
    //                PerformAction();
    //            }
    //        }
    //        else
    //        {
                
    //            if (currentHoldTime > 0)
    //            {
    //                currentHoldTime = 0f;
    //                GameManager.instance.StopLoading(); 
    //            }
    //        }
    //    }
    //}

    public void PerformAction()
    {
        //isCollected = true;
        //isHintShowing = false;
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

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        isPlayerNearby = true;
    //        currentHoldTime = 0f;


    //        if (type == ObjectType.Item) GameManager.instance.ShowHint("Giữ E để nhặt");
    //        else if (type == ObjectType.Door)
    //        {
    //            if (GameManager.instance.currentItems >= 3) GameManager.instance.ShowHint("Vật phẩm: 3/3\nGiữ E để thoát");
    //            else GameManager.instance.ShowHint($"Chưa đủ đồ ({GameManager.instance.currentItems}/3)");
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        isPlayerNearby = false;
    //        currentHoldTime = 0f;
    //        GameManager.instance.HideHint();
    //        GameManager.instance.StopLoading();
    //        isHintShowing = false;
    //    }
    //}
    //void ShowInteractionHint()
    //{
    //    if (type == ObjectType.Item)
    //        GameManager.instance.ShowHint("Giữ E để nhặt");
    //    else if (type == ObjectType.Door)
    //    {
    //        if (GameManager.instance.currentItems >= 3)
    //            GameManager.instance.ShowHint("Vật phẩm: 3/3\nGiữ E để thoát");
    //        else
    //            GameManager.instance.ShowHint($"Chưa đủ đồ ({GameManager.instance.currentItems}/3)");
    //    }
    //}
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