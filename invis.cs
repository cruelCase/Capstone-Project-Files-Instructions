using UnityEngine;

public class invis : MonoBehaviour
{
    [Tooltip("Assign the GameObjects or Transforms that should be disabled while this object is active.")]
    public Transform[] targets;

    void OnEnable()
    {
        SetTargetsActive(false);
    }

    void OnDisable()
    {
        SetTargetsActive(true);
    }

    void SetTargetsActive(bool value)
    {
        if (targets == null)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
                targets[i].gameObject.SetActive(value);
        }
    }
}
