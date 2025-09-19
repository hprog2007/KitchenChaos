using UnityEngine;
using UnityEngine.UI;

public class CircleProgressBarUI : MonoBehaviour
{
    public static CircleProgressBarUI Instance { get; private set; }

    [SerializeField] private Image progressImage;
    [SerializeField] private float timerMax = 5f;

    private float currentTimer;
    private bool timerStarted;

    public void Setup(Vector3 position, float timerMaxParam)
    {
        transform.position = new Vector3(position.x, transform.position.y, position.z);
        timerMax = timerMaxParam;
        currentTimer = Time.deltaTime;
        timerStarted = true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        gameObject.SetActive(false);
        currentTimer = Time.deltaTime;
        timerStarted = false;
    }
    private void Update()
    {
        if (timerStarted)
        {
            currentTimer += Time.deltaTime;
            progressImage.fillAmount = currentTimer / timerMax;
            if (currentTimer >= timerMax)
            {
                gameObject.SetActive(false);
                currentTimer = Time.deltaTime;
                StopTimer();
            }
        }
    }

    public void ResetTimer()
    {
        currentTimer = timerMax;
    }

    public void SetTimer(float timerMaxParam)
    {
        timerMax = timerMaxParam;
    }

    private void StopTimer() => timerStarted = false;
}
