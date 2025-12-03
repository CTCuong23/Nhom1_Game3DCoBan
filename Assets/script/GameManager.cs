using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Cài đặt UI")]
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI winText;
    [Header("UI Loading Mới")]
    public GameObject interactionPanel; 
    public Image loadingBar;            
    public TextMeshProUGUI timerText;   

    [HideInInspector] public int currentItems = 0;
    private int totalItems = 3;

    public bool isAiming = false;

    void Awake()
    {
        // Setup Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
        hintText.text = ""; 
        winText.gameObject.SetActive(false);
        interactionPanel.SetActive(false);
    }

    public void CollectItem()
    {
        currentItems++;
        UpdateUI();
    }

    void UpdateUI()
    {
       
        itemCountText.text = $"Số vật phẩm đã nhặt: {currentItems}/{totalItems}";

        
        if (currentItems >= totalItems)
        {
            itemCountText.color = Color.green;
        }
        else
        {
            itemCountText.color = Color.red;
        }
    }

    
    public void ShowHint(string message)
    {
        hintText.text = message;
        hintText.gameObject.SetActive(true);
    }

    public void HideHint()
    {
        hintText.text = "";
        hintText.gameObject.SetActive(false);
    }
    public void UpdateLoading(float currentHoldTime, float maxHoldTime)
    {
        // 1. Hiện Panel lên
        interactionPanel.SetActive(true);
        hintText.gameObject.SetActive(false); 

       
        float progress = currentHoldTime / maxHoldTime;
        loadingBar.fillAmount = progress;

       
        float timeRemaining = maxHoldTime - currentHoldTime;
        if (timeRemaining < 0) timeRemaining = 0;
        timerText.text = timeRemaining.ToString("F2") + "s"; 
    }

    
    public void StopLoading()
    {
        interactionPanel.SetActive(false);
        loadingBar.fillAmount = 0;

      
        if (hintText.text != "") hintText.gameObject.SetActive(true);
    }

    public void WinGame()
    {
        StopLoading(); 
        winText.gameObject.SetActive(true);
        Time.timeScale = 0;
    }
}
