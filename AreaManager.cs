using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaManager : MonoBehaviour
{
    public GameObject[] areas; // Assign 5 areas here in the Inspector
    public Transform leftBoundary;
    public Transform rightBoundary;
    public Transform player;

    private int currentArea = 0;
    private Transform[] spawnLeft;
    private Transform[] spawnRight;
    private Transform[] leftEdges;
    private Transform[] rightEdges;

    public void LoadScene2()
    {
        SceneManager.LoadScene("Scene2"); // Make sure Scene2 is in Build Settings
    }
    public void LoadScene1()
    {
        SceneManager.LoadScene("Scene1"); // Make sure Scene2 is in Build Settings
    }
    public void LoadScene3()
    {
        SceneManager.LoadScene("Scene3"); // Make sure Scene3 is in Build Settings
    }
    public void LoadScene4()
    {
        SceneManager.LoadScene("Scene4"); // Make sure Scene2 is in Build Settings
    }
    public void LoadSceneH()
    {
        SceneManager.LoadScene("House"); // Make sure Scene2 is in Build Settings
    }
    public void LoadSceneI()
    {
        SceneManager.LoadScene("Intro"); // Make sure Scene2 is in Build Settings
    }
    void Start()
    {
        int count = areas.Length;
        spawnLeft = new Transform[count];
        spawnRight = new Transform[count];
        leftEdges = new Transform[count];
        rightEdges = new Transform[count];
        

        for (int i = 0; i < count; i++)
        {
            spawnLeft[i] = areas[i].transform.Find("SpawnPointLeft");
            spawnRight[i] = areas[i].transform.Find("SpawnPointRight");
            leftEdges[i] = areas[i].transform.Find("LeftEdge");
            rightEdges[i] = areas[i].transform.Find("RightEdge");
        }

        UpdateArea();

        // FIX: Player should always start on the LEFT when scene loads
        MovePlayer(spawnLeft[currentArea]);
    }


    public void MoveToNextArea()
    {
        if (currentArea < areas.Length - 1)
        {
            currentArea++;
            UpdateArea();
            MovePlayer(spawnLeft[currentArea]); // coming from left → spawn near left
        }
    }

    public void MoveToPreviousArea()
    {
        if (currentArea > 0)
        {
            currentArea--;
            UpdateArea();
            MovePlayer(spawnRight[currentArea]); // coming from right → spawn near right
        }
    }

    private void UpdateArea()
    {
        // Enable only the current area
        for (int i = 0; i < areas.Length; i++)
            areas[i].SetActive(i == currentArea);

        // Enable/disable boundaries depending on area
        leftBoundary.gameObject.SetActive(currentArea != 0);
        rightBoundary.gameObject.SetActive(currentArea != areas.Length - 1);

        // Move camera to background position
        Transform background = areas[currentArea].transform.Find("Background" + (currentArea + 1));
        if (background != null)
        {
            Camera.main.transform.position = new Vector3(
                background.position.x,
                background.position.y,
                Camera.main.transform.position.z
            );
        }

        // Move boundaries to match new area
        if (leftEdges[currentArea] != null)
            leftBoundary.position = leftEdges[currentArea].position;
        if (rightEdges[currentArea] != null)
            rightBoundary.position = rightEdges[currentArea].position;
    }

    private void MovePlayer(Transform spawnPoint)
    {
        if (spawnPoint == null) return;

        Vector3 pos = player.position;
        pos.x = spawnPoint.position.x;
        pos.y = spawnPoint.position.y;
        player.position = pos;
    }
}
