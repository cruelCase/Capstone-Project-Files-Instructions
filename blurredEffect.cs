using UnityEngine;

public class blurredEffect : MonoBehaviour
{
    [Tooltip("Assign the GameObjects or Transforms that should be enabled while this object is active.")]
    public Transform[] targets;

    void OnEnable()
    {
        SetTargetsActive(true);
    }

    void OnDisable()
    {
        SetTargetsActive(false);
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
