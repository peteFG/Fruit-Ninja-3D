using System.Collections;
using UnityEngine;

public class DestroyFruit : MonoBehaviour
{
    public SliceListener sliceListener;
    public FruitSpawner fruitSpawner;
    private bool _pointsDeducted;

    private void Start()
    {
        StartCoroutine(SelfDestruct());
        fruitSpawner = GameObject.Find("Spawnpoints").GetComponent<FruitSpawner>();
        sliceListener = GameObject.Find("Katana_LODA Variant 1").GetComponent<SliceListener>();
    }

    private void Update()
    {
        if ("survival".Equals(fruitSpawner.mode)
            && fruitSpawner.spawnedFruits.Contains(gameObject)
            && gameObject.transform.position.y <= -0.05f && !_pointsDeducted)
        {
            _pointsDeducted = true;
            sliceListener.DeductOneLife();
        }
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(7f);
        Destroy(gameObject);
    }
}