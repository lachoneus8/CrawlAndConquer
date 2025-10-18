using UnityEngine;

public class VictoryBarDisplay : MonoBehaviour
{
    [Tooltip("The UI container where the Victory bar will be displayed.")]
    public RectTransform barContainer;
    [Tooltip("Prefab for displaying victory progress in the bar. will be scaled according to how close to victory")]
    public RectTransform ProgressBarSegment;

    public IGameplayController controller;
    //todo move score to gameplay controller

    void Start()
    {
        UpdateBar();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBar();

    }
    void UpdateBar()
    {
        if (barContainer is null)
        {
            Debug.LogWarning("barContainer not found.");
            return;
        }
        if (ProgressBarSegment is null)
        {
            Debug.LogWarning("ProgressBarSegment not found.");
            return;
        }
        if (controller is null)
        {
            Debug.LogWarning("GameplayController not found.");
            return;
        }
        Points points = controller.GetPoints();
        if (points.currentPoints >= points.victoryPoints)
        {//display 100% if more current points than req for victory
            SetBarSegmentSize(ProgressBarSegment, 1f);
            return;
        }

        SetBarSegmentSize(ProgressBarSegment, ((float)points.currentPoints / (float)points.victoryPoints));
    }

    void SetBarSegmentSize(RectTransform segmentRect, float percentage)
    {
        segmentRect.anchorMin = new Vector2(0, 0);
        segmentRect.anchorMax = new Vector2(percentage, 1);
        segmentRect.offsetMin = Vector2.zero;
        segmentRect.offsetMax = Vector2.zero;
    }
}
