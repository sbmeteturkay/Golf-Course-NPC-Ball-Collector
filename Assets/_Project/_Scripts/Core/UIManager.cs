using Game.Feature.Behaviors;
using UnityEngine.SceneManagement;

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
    [SerializeField] private TextMeshProUGUI strategyText;
    [SerializeField] private Image healthSlider;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button restartButton;
    
    [Header("NPC References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private ScoreSystem scoreSystem;
    [SerializeField] private NPCBrain npcBrain; //
    
    private Color greedyColor = new Color(1f, 0.84f, 0f); // Altın sarısı
    private Color balancedColor = new Color(0.2f, 0.8f, 1f); // Mavi
    private Color safetyColor = new Color(1f, 0.3f, 0.3f); // Kırmızı
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
        if (npcBrain != null)
        {
            npcBrain.OnStrategyChanged += UpdateStrategyDisplay;
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
        if (npcBrain != null)
        {
            npcBrain.OnStrategyChanged -= UpdateStrategyDisplay;
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
        if (restartButton!=null)
        {
            restartButton.onClick.AddListener(Restart);
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
    private void UpdateStrategyDisplay(string strategyName)
    {
        if (strategyText != null)
        {
            strategyText.text = $"Strategy: {strategyName}";
            if (strategyName.Contains("Greedy"))
                strategyText.color = greedyColor;
            else if (strategyName.Contains("Survival"))
                strategyText.color = safetyColor;
            else
                strategyText.color = balancedColor;
        }

    }
    private void Exit()
    {
        Application.Quit();
    }
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
}