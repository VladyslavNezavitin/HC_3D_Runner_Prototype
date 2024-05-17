using UnityEngine;
using TMPro;

[DefaultExecutionOrder(-10)]
public class GameManager : Singleton<GameManager>
{ 
    [SerializeField] Player player;
    [SerializeField] GameObject gameOverScreenPanel;
    [SerializeField] TextMeshProUGUI scoreText;
    public int RowCount => 3;
    public float RowWidth => 3f;
 
    private void Awake() 
    {
        Application.targetFrameRate = 60;
        player.OnLethalCollision += GameOver;
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        gameOverScreenPanel.SetActive(true);
        scoreText.text = "Score: " + Mathf.RoundToInt(player.transform.position.z);
    }
}
