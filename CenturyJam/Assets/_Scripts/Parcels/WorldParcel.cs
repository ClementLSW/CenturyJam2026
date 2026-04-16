using UnityEngine;

public class WorldParcel : MonoBehaviour
{
    public ParcelData data;
    public int ownerID;
    public int parcelID = -1;
    public Color parcelColor = Color.white;

    private ConveyorBelt sourceBelt;
    private Transform[] waypoints;
    private int currentIndex = 0;
    private float speed;

    private bool isInteractable = false;

    public void Initialize(
        ConveyorBelt belt,
        ParcelData parcelData,
        Transform[] path,
        float moveSpeed,
        int startIndex = 0)
    {
        sourceBelt = belt;
        data = parcelData;
        waypoints = path;
        speed = moveSpeed;
        currentIndex = startIndex;
        isInteractable = startIndex >= path.Length;

        transform.position = waypoints[Mathf.Clamp(startIndex, 0, path.Length - 1)].position;
    }

    void Update()
    {
        if (isInteractable) return;

        if (waypoints == null || currentIndex >= waypoints.Length) return;

        Transform target = waypoints[currentIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Length)
            {
                ReachEnd();
            }
        }
    }

    private void ReachEnd()
    {
        isInteractable = true;
    }

    public bool IsInteractable()
    {
        return isInteractable;
    }
}