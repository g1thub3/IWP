using UnityEngine;

public class FRAreaManager : MonoBehaviour
{
    [SerializeField] private Transform _playerCharacter;

    [System.Serializable]
    public class AreaSpawnPoint
    {
        public string Origin;
        public Transform Spawn;
    }

    public AreaSpawnPoint[] spawnPoints;

    private void Start()
    {
        GlobalCanvasManager.LoadInstance();
        foreach (var spawnPoint in spawnPoints) { 
            if (spawnPoint.Origin.Equals(GameSceneManager.Instance.previousArea))
            {
                _playerCharacter.position = spawnPoint.Spawn.position;
                break;
            }
        }
    }
}
