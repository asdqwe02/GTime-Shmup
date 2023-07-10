using System.Collections;
using CasualGames.Common.Input;
using CasualGames.Operius2D;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Models;
using CasualGames.Operius2D.Signals;
using PKFramework.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// ReSharper disable once CheckNamespace
namespace CasualGames.Scenes.Operius2DScene
{
    public class Operius2DSceneController : SceneController
    {
        [Inject] private Operius2DSceneLogic _logic;
        [Inject] private SignalBus _signalBus;
        [Inject] private GameConfig _gameConfig;
        [Inject] private PowerupController.Pool _powerUpPool;
        [Inject] private PoolCache _poolCache;
        [SerializeField] private BoxCollider2D _boundary;
        [SerializeField] private GameObject _playerLifeCotainer;
        [SerializeField] private GameObject _gameOverScreen;
        [SerializeField] private GameObject _startGamePanel;
        [SerializeField] private GameObject _winGameScreen;
        [SerializeField] private TextMeshProUGUI _score;
        [SerializeField] private TextMeshProUGUI _coinCounter;
        [SerializeField] private TextMeshProUGUI _waveCount;
        [SerializeField] private TextMeshProUGUI _nextWaveCountDownText;
        [SerializeField] private Button _resetGameButton;
        [SerializeField] private Button _startGameButton;
        Coroutine ShowStartPanelCoroutine;

        //This function call when our scene is being loaded
        public override void OnSceneLoaded(object data)
        {
            base.OnSceneLoaded(data);
        }

        //This function call when our scene is being removed/unloaded
        public override void OnSceneUnloaded()
        {
            base.OnSceneUnloaded();
        }

        private void Start()
        {
            _signalBus.Subscribe<UpdatePlayerLife>(OnUpdatePlayerLife);
            _signalBus.Subscribe<GameResetSignal>(OnGameResetSignal);
            _signalBus.Subscribe<GameEndSignal>(OnGameEnd);
            _signalBus.Subscribe<UpdateScoreSignal>(OnUpdateScore);
            _signalBus.Subscribe<PowerUpSignal>(OnPowerUpSignal);
            _signalBus.Subscribe<NextWaveCountDownSignal>(OnNextWaveCountDownSignal);
            _signalBus.Subscribe<SpawnEnemySignal>(OnEnemySpawnSignal);
            _signalBus.Subscribe<StartGameSignal>(OnStartGame);
            _signalBus.Subscribe<UpdateCoinCountSignal>(OnUpdateCoinCount);
            _logic.Init();
            SetUpBoundaryCollider();
            _resetGameButton.onClick.AddListener(ResetGame);
            _startGameButton.onClick.AddListener(StartGame);
#if ARCADE
            _coinCounter.gameObject.SetActive(true);
            _score.gameObject.SetActive(false);
#else
             _coinCounter.gameObject.SetActive(false);
            _score.gameObject.SetActive(true);
#endif
        }

        private void OnUpdateCoinCount(UpdateCoinCountSignal obj)
        {
            _coinCounter.text = $"Coins: {obj.Coin}";
        }

        private void OnStartGame(StartGameSignal obj)
        {
            _startGamePanel.gameObject.SetActive(false);
            _gameOverScreen.gameObject.SetActive(false);
        }

        private void StartGame()
        {
            _logic.StartGame();
        }

        private void OnEnemySpawnSignal(SpawnEnemySignal obj)
        {
            _waveCount.text = $"Wave: {obj.WaveCount}";
        }

        private void OnNextWaveCountDownSignal(NextWaveCountDownSignal obj)
        {
            _nextWaveCountDownText.text = obj.CountDownTime.ToString("0.0");
        }

        private void OnPowerUpSignal(PowerUpSignal obj)
        {
            if (obj.Spawn)
            {
                var powerUp = _powerUpPool.Spawn(obj.PowerUpType);
                powerUp.transform.position = transform.position;
                _poolCache.ActivePowerUpRefs.Add(powerUp);
            }
        }

        private void OnUpdateScore(UpdateScoreSignal obj)
        {
            _score.text = $"Score: \n{obj.Score}";
        }


        private void OnGameEnd(GameEndSignal obj)
        {
            if (obj.Win)
            {
                _winGameScreen.SetActive(true);
            }
            else
            {
                _gameOverScreen.SetActive(true);
            }
#if ARCADE
            if (obj.ShowStartPanel)
            {
                // ShowStartPanelCoroutine = StartCoroutine(ShowStartPanel());
                ShowStartPanel();
            }
#endif
        }

        private void ResetGame()
        {
            _signalBus.Fire(new GameResetSignal());
        }

        private void OnGameResetSignal(GameResetSignal obj)
        {
            foreach (Transform life in _playerLifeCotainer.transform)
            {
                life.gameObject.SetActive(true);
            }

            _gameOverScreen.SetActive(false);
            _winGameScreen.SetActive(false);
        }

        void ShowStartPanel()
        {
            // yield return new WaitForSecondsRealtime(_gameConfig.ResetWaitTime);
            _startGamePanel.gameObject.SetActive(true);
        }

        private void OnUpdatePlayerLife(UpdatePlayerLife obj)
        {
            if (obj.Life <= 0)
            {
                // reset game
                _gameOverScreen.SetActive(true);
                _playerLifeCotainer.transform.GetChild(2).gameObject.SetActive(false);
                return;
            }

            for (int i = 0; i < _playerLifeCotainer.transform.childCount - obj.Life; i++)
            {
                _playerLifeCotainer.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void SetUpBoundaryCollider()
        {
            float height = Camera.main.orthographicSize * 2 - _gameConfig.BoundaryPadding.y;
            float width = height * Camera.main.aspect - _gameConfig.BoundaryPadding.x;
            _boundary.size = new Vector2(width, height);
            _logic.SetUpBoundary(_boundary);
        }


#if UNITY_EDITOR
        [MenuItem("PKFramework/Open Scene/Operius2DScene")]
        public static void OpenSceneOperius2DScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(@"Assets/CasualGames/Scenes/Operius2DScene/Scenes/Operius2DScene.unity");
            }
        }
#endif
    }
}