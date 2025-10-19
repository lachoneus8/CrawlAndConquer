using System;
using UnityEngine;

public class TitleScale : MonoBehaviour
{
    [SerializeField] private RectTransform title;
    [Tooltip("number of seconds between each full size cycle.")]
    public float cycleTime;

    [Tooltip("Minimum scale value.")]
    public float minScale = 0.9f;

    [Tooltip("Maximum scale value.")]
    public float maxScale = 1.1f;

    private float timeInCycle;
    private void Start()
    {
        if (title == null)
            title = GetComponent<RectTransform>();

        timeInCycle = 0f;
    }
    private void Update()
    {
        timeInCycle += Time.deltaTime;

        if (timeInCycle > cycleTime)
            timeInCycle -= cycleTime;

        ResizeTitle(timeInCycle);
    }

    private void ResizeTitle(float time)
    {
        float normalized = time / cycleTime;

        float scaleFactor = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(normalized * Mathf.PI * 2f) + 1f) / 2f);

        title.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }
}