using UnityEngine;

public class StatusCanvas : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particles;
    
    public void RunParticles()
    {
        foreach (ParticleSystem particle in particles)
        {
            particle.Play();
        }
    }
}
