using UnityEngine;

public class BattlefieldIntel
{
    public Vector3 lastKnownEnemyPosition;
    public float timeStamp;
    public int enemyCount;
    public bool hasBossBeenSeen;
    public Vector3 lastKnownBossPosition;
    public float threatLevel; // 0-1 scale
    
    public BattlefieldIntel(Vector3 enemyPos, int enemyCount, bool bossSpotted, Vector3 bossPos)
    {
        lastKnownEnemyPosition = enemyPos;
        timeStamp = Time.time;
        this.enemyCount = enemyCount;
        hasBossBeenSeen = bossSpotted;
        lastKnownBossPosition = bossPos;
        threatLevel = CalculateThreatLevel();
    }
    
    private float CalculateThreatLevel()
    {
        float threat = enemyCount * 0.1f;
        if (hasBossBeenSeen) threat += 0.5f;
        return Mathf.Clamp01(threat);
    }
}