using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System;

public class Game : MonoBehaviour
{
    const float pausedTimeScale = 0.2f;
    [SerializeField, Range(1f, 10f)]
    float playSpeed = 1f;
    [SerializeField, Range(0, 100)]
    int startingPlayerHealth = 10;
    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField]
    GameBoard board = default;
    [SerializeField]
    GameTileContentFactory tileContentFactory = default;
    [SerializeField]
    WarFactory warFactory = default;
    GameBehaviorCollection enemies = new GameBehaviorCollection();
    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
    public static Game Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("Null instance");
                return instance;
            }
            else
            {
                return instance;
            }
        }
    }
    TowerType selectedTowerType;
    static Game instance;
    [SerializeField]
    GameScenario[] scenarios = default;
    GameScenario.State activeScenario;
    int currentScenarioIndex = 0;
    int playerHealth;
    // UI--
    [SerializeField]
    private bool gameIsPlaying = true;
    public TextMeshProUGUI playerHealthVar, scenarioVar;
    public Button interactableButton;
    bool defeatCoroutineIsExecuted = false;
    // --UI
    public void StartNextRound()
    {
        gameIsPlaying = true;
        interactableButton.interactable = false;
    }
    public static Shell SpawnShell()
    {
        Shell shell = Instance.warFactory.Shell;
        Instance.nonEnemies.Add(shell);
        return shell;
    }
    public static Explosion SpawnExplosion()
    {
        Explosion explosion = Instance.warFactory.Explosion;
        Instance.nonEnemies.Add(explosion);
        return explosion;
    }
    void OnEnable()
    {
        instance = this;
    }
    void Awake()
    {
        playerHealth = startingPlayerHealth;
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
    }
    void Start()
    {
        gameIsPlaying = false;
        activeScenario = scenarios[currentScenarioIndex].Begin();
        scenarioVar.text = (1 + currentScenarioIndex).ToString();
        playerHealthVar.text = startingPlayerHealth.ToString();
    }
    private void OnValidate()
    {
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleAlternativeTouch();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTowerType = TowerType.Laser;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Mortar;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale =
                Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
        }
        else if (Time.timeScale != pausedTimeScale)
        {
            Time.timeScale = playSpeed;
        }
        if (playerHealth <= 0 && startingPlayerHealth > 0)
        {
            Debug.Log("Defeat!");
            if (!defeatCoroutineIsExecuted)
            {
                StartCoroutine(Defeat());
            }
        }
        if (gameIsPlaying)
        {
            if (!activeScenario.Progress() && enemies.IsEmpty)
            {
                Debug.Log("Round won!");
                StartCoroutine(Victory());
            }
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }
    private IEnumerator Victory()
    {
        NextScenario();
        gameIsPlaying = false;
        ChangeButtonText("Round won!");
        yield return new WaitForSeconds(2f);
        ChangeButtonText("Start Next Round");
        interactableButton.interactable = true;
        interactableButton.onClick.AddListener(delegate { StartNextRound(); });
    }
    private IEnumerator Defeat()
    {
        defeatCoroutineIsExecuted = true;
        ResetBoard();
        interactableButton.interactable = false;
        gameIsPlaying = false;
        ChangeButtonText("Defeat!");
        yield return new WaitForSeconds(2f);
        ChangeButtonText("Start New Game");
        interactableButton.interactable = true;
        interactableButton.onClick.AddListener(delegate { BeginNewGame(); });
    }
    private void ChangeButtonText(string text)
    {
        interactableButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
    public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
    {
        GameTile spawnPoint =
            Instance.board.GetSpawnPoint(Random.Range(0, Instance.board.SpawnPointCount));
        Enemy enemy = factory.Get(type);
        enemy.SpawnOn(spawnPoint);
        Instance.enemies.Add(enemy);
    }
    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile, selectedTowerType);
            }
            else
            {
                board.ToggleWall(tile);
            }
        }
    }
    void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else
            {
                board.ToggleSpawnPoint(tile);
            }
        }
    }
    public void BeginNewGame()
    {
        defeatCoroutineIsExecuted = false;
        currentScenarioIndex = 0;
        playerHealth = startingPlayerHealth;
        ChangeButtonText("Start Next Round");
        interactableButton.interactable = false;
        gameIsPlaying = true;
        ResetBoard();
        activeScenario = scenarios[currentScenarioIndex].Begin();
    }
    public void NextScenario()
    {
        currentScenarioIndex++;
        if (currentScenarioIndex >= scenarios.Length)
        {
            currentScenarioIndex = 0;
        }
        activeScenario = scenarios[currentScenarioIndex].Begin();
        scenarioVar.text = (1 + currentScenarioIndex).ToString();
    }
    public static void EnemyReachedDestination()
    {
        Instance.playerHealth -= 1;
        if (Instance.playerHealth >= 0)
        {
            Instance.playerHealthVar.text = Instance.playerHealth.ToString();
        }
    }
    private void ResetBoard()
    {
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
    }
}
