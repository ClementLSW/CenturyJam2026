using System.Collections;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float destroyTime = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SelfDestructSoon());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SelfDestructSoon()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}
