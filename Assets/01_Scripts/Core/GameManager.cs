using UnityEngine;
using UnityEngine.Serialization;

    public class GameManager : MonoBehaviour
    {
        private static GameManager Instance { get; set; }
        
        [Header("Game State")]
        [SerializeField] private bool isPaused = false;

        [Header("References")] 
        [SerializeField] private PlayerController player;
        [SerializeField] private CameraController cameraController;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            Debug.Log("Game Initialized!");
        }

        
        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }
        
        public bool IsPaused() => isPaused;
    }