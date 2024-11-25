using UnityEngine;

public class HighlightBox : MonoBehaviour
{
    // Variabel yang diperlukan
    public LayerMask boxLayer;
    public Material highlightMaterial;
    private Transform highlightedBox;
    private Material originalTopMaterial;

    private void Start() {
        Application.targetFrameRate = 60;
    }

    // Menghilight Box
    void Update()
    {
        // Mengambil posisi Cursor kamera
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, boxLayer))
        {
            Transform box = hit.transform;

            if (box != highlightedBox)
            {
                ResetBoxMaterial();
                highlightedBox = box;

                MeshRenderer renderer = highlightedBox.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;

                    // Memastikan Object Memiliki minimal 2 material
                    if (materials.Length > 1)
                    {
                        originalTopMaterial = materials[1]; // Simpan Material orginal
                        materials[1] = highlightMaterial;   // Mengganti Material
                        renderer.materials = materials;     // Update Material Array
                    }
                    else
                    {
                        // Debug.LogWarning("The object does not have enough materials to highlight.");
                    }
                }
            }
        }
        else
        {
            ResetBoxMaterial();
            highlightedBox = null;
        }
    }

    // Mengembalikan Material sesuai Original
    void ResetBoxMaterial()
    {
        if (highlightedBox != null)
        {
            MeshRenderer renderer = highlightedBox.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material[] materials = renderer.materials;

                // Cek banyaknya material
                if (materials.Length > 1)
                {
                    materials[1] = originalTopMaterial; // Reset ke material original
                    renderer.materials = materials;     // Update material array
                }
            }
        }
    }
}
