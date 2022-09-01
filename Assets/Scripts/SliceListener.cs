using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SliceListener : MonoBehaviour
{
    public Slicer slicer;
    public GameObject scoreText;
    public FruitSpawner fruitSpawner;
    public AudioSource sliceSound;
    public AudioSource explosionSound;
    public int points;
    public int lives = 5;
    [SerializeField] private Image[] hearts;
    public ParticleSystem explosion;
    private DestroyFruit _destroyFruit;

    private void HandleScore()
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "Score:\n<size=120%>" + points + "</size>";
    }
    
    private void Update()
    {
        HandleScore();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (fruitSpawner.spawnedFruits.Contains(other.gameObject) && other.gameObject.layer == 8 &&
            other.gameObject.tag.Equals("Fruit"))
        {
            fruitSpawner.spawnedFruits.Remove(other.gameObject);
            sliceSound.Play();
            slicer.isTouched = true;
        }

        if (fruitSpawner.spawnedBombs.Contains(other.gameObject) && other.gameObject.layer == 9)
        {
            fruitSpawner.spawnedBombs.Remove(other.gameObject);
            switch (fruitSpawner.mode)
            {
                case "score" when points >= 150:
                    points -= 150;
                    break;
                case "score":
                    points = 0;
                    break;
                case "survival":
                    DeductOneLife();
                    break;
            }

            var bombpos = other.gameObject.transform.position;
            explosion.transform.position = bombpos;
            StartCoroutine(BombExplosion());
            explosionSound.Play();
            Destroy(other.gameObject);
        }

        if (fruitSpawner.selectable && other.gameObject.layer == 10)
        {
            sliceSound.Play();
            fruitSpawner.selectable = false;
            RemoveSelectables();
            StartCoroutine(fruitSpawner.SpawnFruitSurvival());
            StartCoroutine(fruitSpawner.SpawnBomb());
        }

        if (fruitSpawner.selectable && other.gameObject.layer == 11)
        {
            sliceSound.Play();
            fruitSpawner.selectable = false;
            RemoveSelectables();
            StartCoroutine(fruitSpawner.SpawnFruitScore());
            StartCoroutine(fruitSpawner.SpawnBomb());
        }

        if (fruitSpawner.selectable && other.gameObject.layer == 12)
        {
            sliceSound.Play();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    public void DeductOneLife()
    {
        lives -= 1;
        hearts[lives].gameObject.SetActive(false);
    }

    private IEnumerator BombExplosion()
    {
        explosion.gameObject.SetActive(true);
        if (explosion.isPlaying) explosion.Stop();
        if (!explosion.isPlaying) explosion.Play();
        yield return new WaitForSeconds(1.5f);
        explosion.gameObject.SetActive(false);
    }

    private static void RemoveSelectables()
    {
        var selectables = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var selectable in selectables)
        {
            selectable.SetActive(false);
        }
    }
}