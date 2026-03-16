using UnityEngine;

public class SongCheckpoint : MonoBehaviour
{
    [SerializeField] SpriteRenderer checkpointTile;
    [SerializeField] GameObject wall;
    [SerializeField] ParticleSystem particles;

    private enum State
    {
        NONE,
        REACHED,
        PLAYED,
    }

    [SerializeField] State currentState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = State.NONE;
        ChangeState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == ValueStorer.playerLM)
        {
            if (currentState == State.NONE)
            {
                currentState = State.REACHED;
                GameManager.Instance.UpdateSafePosition(collision.transform.position);
            }

            ChangeState();
        }
    }

    private void ChangeState()
    {
        var particleMain = particles.main;
        var startColor = particleMain.startColor;
        if (currentState == State.NONE)
        {
            checkpointTile.color = new Color32(0, 172, 255, 255);
            startColor.color = new Color32(114, 209, 255, 255);
        }
        else if (currentState == State.REACHED)
        {
            checkpointTile.color = new Color32(0, 255, 169, 255);
            startColor.color = new Color32(112, 255, 207, 255);
        }
        else if (currentState == State.PLAYED)
        {
            checkpointTile.color = new Color32(255, 141, 0, 255);
            startColor.color = new Color32(255, 192, 112, 255);
        }
        particleMain.startColor = startColor;
    }
}
