using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;
using Unity.Services.Analytics;
using Unity.Services.Core;

// Script untuk player menyusun Object
public class PlayerObjectBuilder : MonoBehaviour
{
    [Header("Spawn Object")]
    public Image[] uiImages;
    public GameObject[] spawnObjects;

    [Header("Shovel")]
    public Image shovelImage;
    private bool isShovelSelected = false;
    private bool canPlaceObjects = true; // Default-nya true

    [Header("Teks UI")]
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI Similiar;

    [Header("Reference")]
    public MenuBarController menuBarController;
    public ViewportAnimator viewportAnimator;

    public AnalyticsManager analyticsManager;

    // Database
    private Dictionary<Transform, GameObject> playerPlacedObjects = new Dictionary<Transform, GameObject>();
    private GameObject selectedObjectPrefab = null;

    // Tambah Listener
    private void Start()
    {
        // Tambahkan listener untuk setiap UI Image agar bisa memilih objek
        foreach (var uiImage in uiImages)
        {
            uiImage.gameObject.GetComponent<Button>().onClick.AddListener(() => SelectObject(uiImage));
        }
        // Tambahkan listener untuk memilih shovel
        shovelImage.gameObject.GetComponent<Button>().onClick.AddListener(SelectShovel);

        Debug.Log("PlayerObjectBuilder: Listener untuk UI Images dan shovel berhasil ditambahkan.");
    }

    // Pilih dan letakan objek
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Cube")))
            {
                Transform parentTransform = hit.collider.transform;
                
                if (isShovelSelected)
                {
                    RemoveObject(parentTransform.GetChild(0));
                }
                else if (selectedObjectPrefab != null)
                {
                    PlaceObject(parentTransform.GetChild(0));
                }
            }
            else
            {
                // Debug.Log("PlayerObjectBuilder: Tidak ada target yang valid untuk penempatan.");
            }
        }
    }

    // Pilih shovel
    private void SelectShovel()
    {
        selectedObjectPrefab = null;
        isShovelSelected = true;
        // Debug.Log("PlayerObjectBuilder: Shovel dipilih untuk menghapus objek.");
    }

    //Memilih objek pada item bar
    private void SelectObject(Image uiImage)
    {
        int index = System.Array.IndexOf(uiImages, uiImage);
        if (index >= 0 && index < spawnObjects.Length)
        {
            selectedObjectPrefab = spawnObjects[index];
            isShovelSelected = false;  // Matikan mode shovel jika ada objek yang dipilih
            // Debug.Log($"PlayerObjectBuilder: Objek {selectedObjectPrefab.name} dipilih untuk penempatan.");
        }
        else
        {
            // Debug.LogWarning("PlayerObjectBuilder: Indeks UI Image di luar jangkauan.");
        }
    }

    // Tempatkan Objek pada ground
    private void PlaceObject(Transform parent)
    {
        if (!canPlaceObjects)
        {
            return;
        }
        if (playerPlacedObjects.ContainsKey(parent))
        {
            // Hapus objek yang sudah ada jika ada
            Destroy(playerPlacedObjects[parent]);
            playerPlacedObjects.Remove(parent);
        }

        // Tempatkan objek baru
        GameObject placedObject = Instantiate(selectedObjectPrefab, parent);
        placedObject.transform.localPosition = Vector3.zero;

        // Animasi DoTween untuk efek muncul
        placedObject.transform.localScale = Vector3.zero; // Mulai dengan skala 0
        placedObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutExpo); // Animasi skala menjadi 1

        playerPlacedObjects[parent] = placedObject;

        AudioManager.Instance.PlaySFX(0);

       //  Debug.Log($"Object placed: {placedObject.name} at {parent.name}");
    }

    // Hapus Objek
    public void RemoveObject(Transform parent)
    {
        if (playerPlacedObjects.ContainsKey(parent))
        {
            Destroy(playerPlacedObjects[parent]);
            playerPlacedObjects.Remove(parent);
            // Debug.Log($"PlayerObjectBuilder: Objek pada {parent.name} telah dihapus.");
        }
        else
        {
            // Debug.LogWarning("PlayerObjectBuilder: Tidak ada objek untuk dihapus di posisi ini.");
        }
    }

    // Simpan layout player pada database
    public void SavePlayerLayout()
    {

        foreach (var kvp in playerPlacedObjects)
        {
            Transform position = kvp.Key;
            GameObject placedObject = kvp.Value;
            Debug.Log($"PlayerObjectBuilder: Objek {placedObject.name} ditempatkan di posisi {position.name}.");
        }

        CompareLayouts();
        menuBarController.RewindObject();
        canPlaceObjects = false;

        // Belah Kamera
        if (viewportAnimator != null)
        {
            viewportAnimator.AnimateViewportsAndRespawn();
        }

    }

    //Menghitung kesamaan
    private void CompareLayouts()
    {
        AudioManager.Instance.PlaySFX(0);

        ObjectSpawner objectSpawner = FindObjectOfType<ObjectSpawner>();
        if (objectSpawner == null)
        {
            // Debug.LogError("PlayerObjectBuilder: ObjectSpawner tidak ditemukan di scene!");
            return;
        }

        // Mendapatkan spawnData
        Dictionary<Transform, string> spawnData = objectSpawner.GetSpawnPositionStatus();

        // filter bidang kosong
        int totalDataObjects = spawnData.Values.Where(value => value != "kosong").Count();
        int playerPlacedCount = playerPlacedObjects.Count;
        int correctPlacements = 0;

        // Debug.Log($"Total positions expected (excluding 'kosong'): {totalDataObjects}, Total positions placed by player: {playerPlacedCount}");

        // Cek semua objek yang telah ditempatkan oleh pemain
        foreach (var playerPlacement in playerPlacedObjects)
        {
            Transform spawnPosition = playerPlacement.Key;
            GameObject playerObject = playerPlacement.Value;
            string playerObjectName = playerObject.name.Replace("(Clone)", "").Trim();

            // Cek apakah posisi yang ditempati oleh player ada di data spawn
            if (spawnData.ContainsKey(spawnPosition))
            {
                string expectedObjectName = spawnData[spawnPosition];
                // Debug.Log($"Checking player object at {spawnPosition.name}: {playerObjectName} (Expected: {expectedObjectName})");

                // Jika objek yang ditempatkan sesuai dengan data yang diharapkan, tambahkan 1 ke correctPlacements
                if (playerObjectName == expectedObjectName && expectedObjectName != "kosong")
                {
                    correctPlacements++;
                    // Debug.Log($"Correct placement at {spawnPosition.name}. Correct placements: {correctPlacements}.");
                }
                else
                {
                    // Debug.Log($"Mismatch at {spawnPosition.name}: Expected {expectedObjectName}, got {playerObjectName}. No points added.");
                }
            }
            else
            {
                // Jika posisi yang ditempati tidak diharapkan
                // Debug.Log($"Unexpected placement at {spawnPosition.name}: Object {playerObjectName} was placed but not expected here. No points added.");
            }

            
        }

        // Debug.Log($"Total correct placements: {correctPlacements}");

        // Perhitungan persentase berdasarkan kondisi
        float matchPercentage = 0f;

        if (playerPlacedCount > totalDataObjects)
        {
            // Jika player meletakkan lebih banyak objek daripada yang diharapkan
            matchPercentage = ((float)correctPlacements / playerPlacedCount) * 100;
            // Debug.Log($"Player placed more objects than expected. Calculating similarity as correct placements / total player placements.");
        }
        else
        {
            // Jika player meletakkan objek sesuai atau kurang dari yang diharapkan
            matchPercentage = ((float)correctPlacements / totalDataObjects) * 100;
            // Debug.Log($"Player placed equal or fewer objects than expected. Calculating similarity as correct placements / total expected placements.");
        }

        // Debug.Log($"Raw match percentage before clamping: {matchPercentage}");

        // Pastikan persentase tidak melebihi 100% dan dibulatkan ke integer
        int finalMatchPercentage = Mathf.Clamp(Mathf.RoundToInt(matchPercentage), 0, 100);

        //Debug.Log($"Final match percentage (after clamping): {finalMatchPercentage}%");

        // Hasil Similiarity
        Similiar.text = "Similarity:";
        feedbackText.text = $"{finalMatchPercentage}%";
        feedbackText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutBack);

        AnalyticsManager.Instance.Match();
        AnalyticsManager.Instance.Similarity(finalMatchPercentage);
        
    }

    // Tempat menyimpan data
    private Dictionary<Transform, string> ConvertPlayerData()
    {
        Dictionary<Transform, string> playerData = new Dictionary<Transform, string>();
        foreach (var kvp in playerPlacedObjects)
        {
            Transform position = kvp.Key;
            string objectName = kvp.Value.name.Replace("(Clone)", "").Trim();
            playerData[position] = objectName;
        }
        return playerData;
    }

    // Menambahkan fungsi untuk respawn objek yang sudah ditempatkan
    public void RespawnPlacedObjects()
    {
        // Membuat salinan dari kunci (keys) untuk iterasi
        var placedObjectsKeys = new List<Transform>(playerPlacedObjects.Keys);

        // Loop melalui semua objek yang sudah ditempatkan oleh pemain menggunakan salinan kunci
        foreach (var parentTransform in placedObjectsKeys)
        {
            // Ambil objek yang telah ditempatkan
            GameObject placedObject = playerPlacedObjects[parentTransform];

            // Hapus objek lama
            Destroy(placedObject);

            // Respawn objek baru di posisi yang sama dan animasi muncul
            GameObject respawnedObject = Instantiate(selectedObjectPrefab, parentTransform);
            respawnedObject.transform.localPosition = Vector3.zero;
            respawnedObject.transform.localScale = Vector3.zero; // Mulai dengan skala 0
            respawnedObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutExpo); // Animasi skala menjadi 1

            // Perbarui referensi objek yang baru di dalam dictionary
            playerPlacedObjects[parentTransform] = respawnedObject;
            Debug.Log($"Object respawned: {respawnedObject.name} at {parentTransform.name}");
        }
    }
}
