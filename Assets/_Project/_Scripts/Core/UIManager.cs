namespace Game.Core
{
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private CanvasGroup StartCanvas;
    [SerializeField] private CanvasGroup GameplayCanvas;
    [SerializeField] private CanvasGroup SettingsCanvas;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthSlider;
    [SerializeField] private Button exitButton;
    
    [Header("NPC References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private ScoreSystem scoreSystem;
    
    private void OnEnable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealthDisplay;
            healthSystem.OnDeath += OnNPCDeath;
        }
        
        if (scoreSystem != null)
        {
            scoreSystem.OnScoreChanged += UpdateScoreDisplay;
        }
    }
    
    private void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthDisplay;
            healthSystem.OnDeath -= OnNPCDeath;
        }
        
        if (scoreSystem != null)
        {
            scoreSystem.OnScoreChanged -= UpdateScoreDisplay;
        }
    }
    
    private void Start()
    {
        if (healthSystem != null)
        {
            UpdateHealthDisplay(healthSystem.CurrentHealth);
            healthSlider.fillAmount = healthSystem.MaxHealth;
        }
        
        if (scoreSystem != null)
        {
            UpdateScoreDisplay(scoreSystem.CurrentScore);
        }

        if (exitButton!=null)
        {
            exitButton.onClick.AddListener(Exit);
        }
    }
    
    private void UpdateHealthDisplay(float currentHealth)
    {
        if (healthText != null)
            healthText.text = $"Health: {currentHealth:F0}";
        
        if (healthSlider != null)
            healthSlider.fillAmount = currentHealth/healthSystem.MaxHealth;
    }
    
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
    
    private void OnNPCDeath()
    {
        if (healthText != null)
            healthText.text = "Stopped";
    }

    private void Exit()
    {
        Application.Quit();
    }
}
}