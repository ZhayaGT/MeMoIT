using System.Collections;
using UnityEngine;

// Script Untuk membagi kamera
public class ViewportAnimator : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public GameObject animationIndicator; 
    public ObjectSpawner objectSpawner;

    public void AnimateViewportsAndRespawn()
    {
        // Aktifkan indikator
        if (animationIndicator != null)
        {
            animationIndicator.SetActive(true);
        }

        // Ubah viewport kamera
        if (camera1 != null)
        {
            camera1.rect = new Rect(0.5f, camera1.rect.y, camera1.rect.width, camera1.rect.height);
        }

        if (camera2 != null)
        {
            camera2.rect = new Rect(-0.5f, camera2.rect.y, camera2.rect.width, camera2.rect.height);
        }

        StartCoroutine(RespawnObjectsWithDelay());
    }

    // Respawn Objek perbandingan
    private IEnumerator RespawnObjectsWithDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (objectSpawner != null)
        {
            objectSpawner.RespawnObjects();
        }
    }
}
