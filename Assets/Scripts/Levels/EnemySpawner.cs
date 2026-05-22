using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
// for buttons
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    // adding level storage
    private List<Level> levels;
    private readonly List<string> characterClasses = new List<string>();
    private Level currentLevel;
    private int currentWave = 1;    
    private string selectedCharacterClass;

    // tracks spawn count
    private int activeSpawnCoroutines = 0;

    private Dictionary<string, Enemy> enemiesByType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemiesByType = EnemyJsonLoader.LoadEnemies();
        levels = LevelsJsonLoader.LoadLevels();
        LoadCharacterClasses();

        BuildClassButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        if (string.IsNullOrWhiteSpace(selectedCharacterClass))
        {
            Debug.LogWarning("Select a character class before starting a level.");
            return;
        }

        // initiate selected level
        currentLevel = levels.FirstOrDefault(l => l.name == levelname);

        // if no level is founded, log error
        if (currentLevel == null)
        {
            Debug.LogError($"Level not found: {levelname}");
            return;
        }

        // reset wave counter
        currentWave = 1;

        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        playerController.SetCharacterClass(selectedCharacterClass);

        level_selector.gameObject.SetActive(false);

        playerController.StartLevel();

        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }

    public void ReturnToMenu()
    {
        StopAllCoroutines();

        // clear remaining enemies
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(e);

        // reset game state
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        GameManager.Instance.playerWon = false;
        GameManager.Instance.currentWave = 0;
        GameManager.Instance.activeWave = 0;

        currentWave = 1;
        currentLevel = null;
        selectedCharacterClass = null;

        // show the level selector
        level_selector.gameObject.SetActive(true);
        BuildClassButtons();
    }

    public void SelectCharacterClass(string className)
    {
        selectedCharacterClass = className;
        BuildLevelButtons();
    }

    private void LoadCharacterClasses()
    {
        characterClasses.Clear();

        TextAsset classFile = Resources.Load<TextAsset>("classes");
        if (classFile == null)
        {
            Debug.LogError("Could not find Assets/Resources/classes.json");
            return;
        }

        try
        {
            JObject root = JObject.Parse(classFile.text);
            foreach (JProperty property in root.Properties())
            {
                characterClasses.Add(property.Name);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse classes.json: {e.Message}");
        }
    }

    private void BuildClassButtons()
    {
        ClearSelectorButtons();

        float yOffset = 130;
        foreach (string className in characterClasses)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, yOffset);
            yOffset -= 60;

            MenuSelectorController ctrl = selector.GetComponent<MenuSelectorController>();
            ctrl.spawner = this;
            ctrl.SetCharacterClass(className);
        }
    }

    private void BuildLevelButtons()
    {
        ClearSelectorButtons();

        float yOffset = 130;
        foreach (Level level in levels)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, yOffset);
            yOffset -= 60;

            MenuSelectorController ctrl = selector.GetComponent<MenuSelectorController>();
            ctrl.spawner = this;
            ctrl.SetLevel(level.name);
        }
    }

    private void ClearSelectorButtons()
    {
        for (int i = level_selector.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(level_selector.transform.GetChild(i).gameObject);
        }
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        
        // for player progression
        GameManager.Instance.player
            .GetComponent<PlayerController>()
            .ApplyWaveScaling(currentWave);

        GameManager.Instance.activeWave = currentWave;
        GameManager.Instance.waveEnemiesKilled = 0;
        int wave = currentWave;

        foreach (Spawn spawn in currentLevel.spawns)
        {
            activeSpawnCoroutines++;
            StartCoroutine(HandleSpawn(spawn, wave));
        }

        // wait until:
        // 1. all spawning finished
        // 2. all enemies dead
        yield return new WaitUntil(() =>
            activeSpawnCoroutines == 0 &&
            GameManager.Instance.enemy_count == 0
        );
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        // RewardScreenManager detects the state change and calls ShowReward() itself

        // move to next wave
        currentWave++;
        GameManager.Instance.currentWave = currentWave - 1;

        // if exceeded number of waves, end the level
        if (currentLevel.waves > 0 && currentWave > currentLevel.waves)
        {
            GameManager.Instance.playerWon = true;
            GameManager.Instance.state = GameManager.GameState.GAMEOVER;
            yield break;
        }

        // stops coroutine - button will trigger next wave
        yield break;
    }

    IEnumerator HandleSpawn(Spawn spawn, int wave)
    {
        // look up base enemy definition from enemy.json
        if (!enemiesByType.TryGetValue(spawn.enemy, out Enemy baseEnemy))
        {
            Debug.LogError($"Enemy not found: {spawn.enemy}");
            yield break;
        }

        // use provided sequence or default to spawn one at a time
        List<int> sequence = spawn.sequence ?? new List<int> { 1 };

        // delay between spawn groups
        float delay = RPNEvaluator.RPNEvaluator.Evaluate(
            spawn.delay ?? "2",
            new Dictionary<string, int> { { "wave", wave } }
        );

        // total number of enemies to spawn
        int count = Mathf.FloorToInt(
            RPNEvaluator.RPNEvaluator.Evaluate(
                spawn.count,
                new Dictionary<string, int> { { "wave", wave } }
            )
        );

        int spawned = 0;    // number of enemies spawned so far
        int seqIndex = 0;   // index into sequence pattern

        // spawns until total count is reached
        while (spawned < count)
        {
            // determine how many enemies to spawn in group
            int groupSize = sequence[seqIndex % sequence.Count];

            // ensures enemies aren't spawned more than remaining count
            int actualSpawn = Mathf.Min(groupSize, count - spawned);

            // spawn enemies in this group
            for (int i = 0; i < actualSpawn; i++)
            {
                SpawnEnemy(spawn, baseEnemy, wave);
            }

            spawned += actualSpawn;
            seqIndex++;

            yield return new WaitForSeconds(delay);
        }

        // spawning for this enemy type finished
        activeSpawnCoroutines--;
    }

    void SpawnEnemy(Spawn spawn, Enemy baseEnemy, int wave)
    {
        // filter spawn points by location type
        SpawnPoint[] eligible = SpawnPoints;
        string loc = (spawn.location ?? "random").Trim().ToLower();
        if (loc == "random red")
            eligible = System.Array.FindAll(SpawnPoints, p => p.kind == SpawnPoint.SpawnName.RED);
        else if (loc == "random green")
            eligible = System.Array.FindAll(SpawnPoints, p => p.kind == SpawnPoint.SpawnName.GREEN);
        else if (loc == "random bone")
            eligible = System.Array.FindAll(SpawnPoints, p => p.kind == SpawnPoint.SpawnName.BONE);

        if (eligible.Length == 0) eligible = SpawnPoints; // fallback to all

        SpawnPoint spawn_point = eligible[Random.Range(0, eligible.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 pos = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject new_enemy = Instantiate(enemy, pos, Quaternion.identity);

        // evaluate stats using RPN
        int hp = RPNEvaluator.RPNEvaluator.Evaluate(
            spawn.hp ?? "base",
            new Dictionary<string, int> { { "base", baseEnemy.hp }, { "wave", wave } }
        );

        int speed = RPNEvaluator.RPNEvaluator.Evaluate(
            spawn.speed ?? "base",
            new Dictionary<string, int> { { "base", baseEnemy.speed }, { "wave", wave } }
        );

        int damage = RPNEvaluator.RPNEvaluator.Evaluate(
            spawn.damage ?? "base",
            new Dictionary<string, int> { { "base", baseEnemy.damage }, { "wave", wave } }
        );

        // assign sprite based on enemy type
        new_enemy.GetComponent<SpriteRenderer>().sprite =
            GameManager.Instance.enemySpriteManager.Get(baseEnemy.sprite);

        // configure enemy stats
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = speed;
        en.attackDamage = damage;

        GameManager.Instance.AddEnemy(new_enemy);
    }
}
