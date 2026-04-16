using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float recoverySpeed = 1.5f;
    [SerializeField] private float maximumScaleShake;
    [SerializeField] private float frequency;
    private float _initialCameraScale;

    private float _strength = 1;
    private float seed;

    private void Awake()
    {
        seed = Random.value;
    }

    private void Start()
    {
        _initialCameraScale = Camera.main.orthographicSize;
    }

    public void ImpulseShake()
    {
        _strength = 1;
    }

    private void Update()
    {
        Camera.main.orthographicSize = _initialCameraScale - 0.1f -
                                       maximumScaleShake * (Mathf.PerlinNoise(seed, Time.time * frequency) * 2 - 1) *
                                       _strength;

        _strength = Mathf.Clamp01(_strength - recoverySpeed * Time.deltaTime);
    }
}