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
    
    // adding text updating
    public TextMeshProUGUI menuText;

    // adding level storage
    private List<Level> levels;
    private Level currentLevel;
    private int currentWave = 1;    

    private Dictionary<string, Enemy> enemiesByType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemiesByType = EnemyJsonLoader.LoadEnemies();
        levels = LevelsJsonLoader.LoadLevels();

        float yOffset = 130;

        // dynamic buttons for each level/difficulty
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
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

        level_selector.gameObject.SetActive(false);

        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();

        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
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
        int wave = currentWave;

        // spawn all enemy types defined in JSON
        foreach (Spawn spawn in currentLevel.spawns)
        {
            StartCoroutine(HandleSpawn(spawn, wave));
        }

        // wait for enemies to die
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;

        // move to next wave
        currentWave++;
        menuText.text = $"Wave {currentWave-1} Passed!";

        // if exceeded number of waves, end the level
        if (currentLevel.waves > 0 && currentWave > currentLevel.waves)
        {
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
    }

    void SpawnEnemy(Spawn spawn, Enemy baseEnemy, int wave)
    {
        // select random spawn point
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
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
