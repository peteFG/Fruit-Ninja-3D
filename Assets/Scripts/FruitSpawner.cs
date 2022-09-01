using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    private int[] _possibleSpawnPoints;
    private GameObject _introText;
    public GameObject timeText;
    public GameObject scoreText;
    public GameObject creditText;
    public GameObject timeHighScoreText;
    public GameObject survivalHighScoreText;
    private GameObject _allHighScores;
    public GameObject gameOverScreen;
    public GameObject gameOverScoreText;
    public GameObject selectionBanana;
    public GameObject bomb;
    public GameObject livesDisplay;
    public GameObject[] fruitPrefab;
    public GameObject[] spawnPoints;
    public List<GameObject> spawnedFruits;
    public List<GameObject> spawnedBombs;
    public SliceListener sliceListener;
    private float _timeLeft = 60f;
    public bool selectable;
    private bool _timeRunning = true;
    private bool _timerStarted;
    private int _spawnerCounter;
    public string mode;
    private HighScores _highScores = HighScores.Instance;


    private void Start()
    {
        _allHighScores = GameObject.Find("HighScores");
        timeHighScoreText.GetComponent<TextMeshProUGUI>().text =
            "<size=140%>" + string.Join("\n", _highScores.GetTimeScores()) + "</size>";
        survivalHighScoreText.GetComponent<TextMeshProUGUI>().text =
            "<size=140%>" + string.Join("\n", _highScores.GetSurvivalScores()) + "</size>";
        _introText = GameObject.Find("IntroText");
        selectable = true;
    }

    private void Update()
    {
        if (_timerStarted && _timeRunning)
        {
            DisplayRemainingTime();
        }

        switch (mode)
        {
            case "score":
                _allHighScores.SetActive(false);
                timeText.SetActive(true);
                scoreText.SetActive(true);
                livesDisplay.SetActive(false);
                break;
            case "survival":
                _allHighScores.SetActive(false);
                timeText.SetActive(false);
                scoreText.SetActive(true);
                livesDisplay.SetActive(true);
                break;
        }

        if (sliceListener.lives <= 0 || _timeLeft <= 0.0f)
        {
            sliceListener.lives = 5;
            _timeLeft = 60;
            ReturnToSelectionScreen();
        }
    }


    private void DisplayRemainingTime()
    {
        if (_timeLeft >= 0.0f)
        {
            _timeLeft -= Time.deltaTime;
        }
        else
        {
            timeText.GetComponent<TextMeshProUGUI>().text = "Time over";
            _timeLeft = 0;
            _timeRunning = false;
            _timerStarted = false;
        }

        float minutes = Mathf.FloorToInt(_timeLeft / 60);
        float seconds = Mathf.FloorToInt(_timeLeft % 60);

        timeText.GetComponent<TextMeshProUGUI>().text =
            "Time:\n <size=120%>" + string.Format("{0:00}:{1:00}", minutes, seconds) + "</size>";
    }

    public IEnumerator SpawnFruitScore()
    {
        mode = "score";
        _timeLeft = 60f;
        _timerStarted = true;
        _timeRunning = true;
        _introText.SetActive(false);
        creditText.SetActive(false);

        while (!selectable && _timeLeft >= 0.0f)
        {
            SpawnFruits();
            yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
        }
    }

    public IEnumerator SpawnFruitSurvival()
    {
        mode = "survival";
        scoreText.SetActive(false);
        _introText.SetActive(false);
        creditText.SetActive(false);
        while (!selectable && sliceListener.lives > 0)
        {
            SpawnFruits();
            yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
        }
    }

    private void SpawnFruits()
    {
        GameObject fruitGo = Instantiate(fruitPrefab[Random.Range(0, fruitPrefab.Length)]);
        spawnedFruits.Add(fruitGo);
        fruitGo.GetComponent<AudioSource>().Play();
        Rigidbody fruitRigidBody = fruitGo.GetComponent<Rigidbody>();
        fruitRigidBody.velocity = new Vector3(0f, Random.Range(6f, 7.5f), -.5f);
        fruitRigidBody.angularVelocity =
            new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        fruitRigidBody.useGravity = true;

        fruitGo = determineSpawnpoint(fruitGo);
    }

    public IEnumerator SpawnBomb()
    {
        yield return new WaitForSeconds(4f);
        while (!selectable && (sliceListener.lives > 0 || sliceListener.points >= 500))
        {
            GameObject go = Instantiate(bomb);
            spawnedBombs.Add(go);
            Rigidbody bombRigidBody = go.GetComponent<Rigidbody>();
            bombRigidBody.velocity = new Vector3(0f, 5f, -.5f);
            bombRigidBody.angularVelocity =
                new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            bombRigidBody.useGravity = true;

            go = determineSpawnpoint(go);
            yield return new WaitForSeconds(Random.Range(10f, 14f));
        }
    }

    private void ReturnToSelectionScreen()
    {
        StopAllCoroutines();
        switch (mode)
        {
            case "score":
                _highScores.AddTimeScore(sliceListener.points);
                break;
            case "survival":
                _highScores.AddSurvivalScore(sliceListener.points);
                break;
        }

        mode = "";
        scoreText.SetActive(false);
        timeText.SetActive(false);
        livesDisplay.SetActive(false);
        spawnedFruits = new List<GameObject>();
        _timeRunning = false;
        gameOverScreen.SetActive(true);
        selectable = true;
        _timeRunning = false;
        _timerStarted = false;
        foreach (var fruit in GameObject.FindGameObjectsWithTag("Fruit"))
        {
            Destroy(fruit);
        }

        foreach (var bomba in GameObject.FindGameObjectsWithTag("Bomb"))
        {
            Destroy(bomba);
        }

        gameOverScoreText.GetComponent<TextMeshProUGUI>().text += "<size=160%>" + sliceListener.points + "</size>";
        StartCoroutine(WaitAndActivateBanana());
    }

    private IEnumerator WaitAndActivateBanana()
    {
        yield return new WaitForSeconds(2f);
        selectionBanana.SetActive(true);
    }

    private GameObject determineSpawnpoint(GameObject go)
    {
        // Für mehrere Spawnpoints folgenden Code einkommentieren.
        //switch (spawnerCounter)
        //{
        //    case 0:
        //        // 0, 1, 2
        //        possibleSpawnPoints = new[] {0, 1, 2};
        //        break;
        //    //case 1:
        //    //    // 0, 1
        //    //    possibleSpawnPoints = new[] {0, 1};
        //    //    break;
        //    //case 2:
        //    //    // 0, 2
        //    //    possibleSpawnPoints = new[] {0, 2};
        //    //    break;
        //    //case 3:
        //    //    // 1, 2, 3
        //    //    possibleSpawnPoints = new[] { 1, 2, 3 };
        //    //    break;
        //}
        //spawnerCounter = possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Length)];
        var point = spawnPoints[_spawnerCounter];
        Vector3 pos = point.transform.position;
        if (_spawnerCounter == 0)
        {
            pos.x += Random.Range(-1.2f, 1.2f);
        }

        //else
        //{
        //    pos.z += Random.Range(-0.5f, 0.5f);
        //}
        go.transform.position = pos;
        return go;
    }
}