using UnityEngine;

public class WorldParcel : MonoBehaviour
{
    public ParcelData data;
    public int ownerID;
    public int parcelID = -1;

    private ConveyorBelt sourceBelt;
    private Transform[] waypoints;
    private int currentIndex = 0;
    private float speed;

    private bool isInteractable = false;

    public void Initialize(
        ConveyorBelt belt,
        ParcelData parcelData,
        Transform[] path,
        float moveSpeed)
    {
        sourceBelt = belt;
        data = parcelData;
        waypoints = path;
        speed = moveSpeed;

        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (isInteractable) return;

        if (currentIndex >= waypoints.Length) return;

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