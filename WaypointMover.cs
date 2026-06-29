using UnityEngine;
using System.Collections;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent; // Assign in inspector
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;
    public Transform imageObject; // Assign the image object in inspector

    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waypoints = new Transform[waypointParent.childCount];
    
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseController.isGamePaused || isWaiting || (imageObject != null && imageObject.gameObject.activeSelf))
        {
            return;
        }
        moveToWaypoint();
    }

        void moveToWaypoint()
        {
            Transform target = waypoints[currentWaypointIndex];

            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                StartCoroutine(WaitAtWaypoint());
            }
        }

        IEnumerator WaitAtWaypoint()
        {
            isWaiting = true;
            yield return new WaitForSeconds(waitTime);
            //isWaiting = false;

            currentWaypointIndex = loopWaypoints ? (currentWaypointIndex + 1) % waypoints.Length : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);
            isWaiting = false;  
        }

    }

