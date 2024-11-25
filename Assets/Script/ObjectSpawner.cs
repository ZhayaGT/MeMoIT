using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;


public class ObjectSpawner : MonoBehaviour
{
    // Informasi Difficulty
    public enum Difficulty { Easy, Mid, Hard }
    public Difficulty currentDifficulty = Difficulty.Easy;

    [Header("Spawn Object & Position")]
    public GameObject[] objectTypes; // Object yang akan di Spawn
    public Transform[] spawnPositions; // Posisi Spawn
    public Transform[] comparePositions; // Posisi Compare Layout

    [Header("UI & Animation")]
    public TextMeshProUGUI countdownText;
    public bool useAnimation;
    public enum SpawnAnimationType { Fall, Scale }

    [Header("Reference")]
    public MenuBarController menuBarController;

    // Difficulty
    private int minObjects;
    private int maxObjects;
    private int maxTypes;
    private string difficulty;

    // Tempat Data Disimpan
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Dictionary<Transform, GameObject> objectAtSpawnPoint = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, string> spawnPositionStatus = new Dictionary<Transform, string>();

    // Mengatur Difficulty
    void Start()
    {
        difficulty = PlayerPrefs.GetString("SelectedDifficulty", "Easy");

        // currentDifficulty diatur sesuai nilai `difficulty`
        switch (difficulty)
        {
            case "Easy":
                currentDifficulty = Difficulty.Easy;
                break;
            case "Medium":
                currentDifficulty = Difficulty.Mid;
                break;
            case "Hard":
                currentDifficulty = Difficulty.Hard;
                break;
            default:
                currentDifficulty = Difficulty.Easy;
                break;
        }

        AnalyticsManager.Instance.Difficulty(difficulty);

        // Atur parameter difficulty
        SetDifficultyParameters();

        // Mulai coroutine untuk spawn objek
        StartCoroutine(SpawnNewObjects());
    }

    // Pola Difficulty
    void SetDifficultyParameters()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                minObjects = 4;
                maxObjects = 6;
                maxTypes = 3;
                break;
            case Difficulty.Mid:
                minObjects = 8;
                maxObjects = 10;
                maxTypes = 4;
                break;
            case Difficulty.Hard:
                minObjects = 15;
                maxObjects = 20;
                maxTypes = 6;
                break;
        }
        // Debug.Log($"Difficulty set to {currentDifficulty}. Parameters - Min: {minObjects}, Max: {maxObjects}, Max Types: {maxTypes}");
    }

    // Spawn Object dan simpan data
    IEnumerator SpawnNewObjects()
    {
        ClearSpawnedObjects();
        spawnPositionStatus.Clear();

        HashSet<int> usedPositions = new HashSet<int>();
        int objectsToSpawn = Random.Range(minObjects, maxObjects + 1);
        Debug.Log($"Spawning {objectsToSpawn} new objects.");

        // Memilih Type Object spawn
        List<GameObject> availableTypes = new List<GameObject>();
        for (int i = 0; i < maxTypes; i++)
        {
            if (i < objectTypes.Length)
            {
                availableTypes.Add(objectTypes[i]);
            }
        }

        //Spawn Objects
        for (int objectsSpawned = 0; objectsSpawned < objectsToSpawn; objectsSpawned++)
        {
            int randomPositionIndex = Random.Range(0, spawnPositions.Length);
            while (usedPositions.Contains(randomPositionIndex))
            {
                randomPositionIndex = Random.Range(0, spawnPositions.Length);
            }
            usedPositions.Add(randomPositionIndex);

            int randomIndex = Random.Range(0, availableTypes.Count);
            GameObject objectToSpawn = availableTypes[randomIndex];

            // Spawn dan simpan data
            SpawnObjectAtPosition(objectToSpawn, spawnPositions[randomPositionIndex], 2);
            spawnPositionStatus[spawnPositions[randomPositionIndex]] = objectToSpawn.name;

            yield return new WaitForSeconds(0.2f);
        }

        // simpan bagian kosong sebagai ("kosong")
        foreach (var spawnPosition in spawnPositions)
        {
            if (!spawnPositionStatus.ContainsKey(spawnPosition))
            {
                spawnPositionStatus[spawnPosition] = "kosong";
            }
        }

        StartCoroutine(CountdownAndClearObjects());
    }

    //Spawn Object sesuai posisi
    void SpawnObjectAtPosition(GameObject objectPrefab, Transform spawnPosition, int Anim)
    {
        GameObject spawnedObject = Instantiate(objectPrefab, spawnPosition.position, Quaternion.identity);
        spawnedObject.transform.SetParent(spawnPosition);
        spawnedObject.transform.localPosition = Vector3.zero;

        spawnedObjects.Add(spawnedObject);
        objectAtSpawnPoint[spawnPosition] = spawnedObject;

        Destroy(spawnedObject.GetComponent<Collider>());
        Destroy(spawnedObject.GetComponent<Rigidbody>());

        AnimateSpawn(spawnedObject, spawnPosition.position,Anim);

        AudioManager.Instance.PlaySFX(0);

        // Debug.Log($"Spawned object: {spawnedObject.name} at position: {spawnPosition.name}");
    }

    // Animasikan Spawn Object
    void AnimateSpawn(GameObject obj, Vector3 spawnPosition, int Anim)
    {
        switch (Anim)
        {
            case 1: // Fall
                Vector3 initialPosition = spawnPosition + Vector3.up * 3;
                obj.transform.position = initialPosition;
                obj.transform.DOMove(spawnPosition, 0.5f).SetEase(Ease.OutBounce);
                break;

            case 2: // Scale
                obj.transform.localScale = Vector3.zero;
                obj.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutExpo);
                break;
        }
    }

    // Memulai Countdown dan hapus objek
    IEnumerator CountdownAndClearObjects()
    {
        int countdownTime = 10; // Waktu countdown
        while (countdownTime > 0)
        {
            // Set teks dan reset skala ke default
            countdownText.text = countdownTime.ToString();
            countdownText.transform.localScale = Vector3.one;

            // Animasi scale menggunakan DoTween
            countdownText.transform.DOScale(Vector3.one * 1.1f, 0.5f) 
                .SetEase(Ease.OutBounce)                              
                .OnComplete(() =>                                     
                {
                    countdownText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
                });

            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        // Tampilkan pesan
        countdownText.text = "Good Luck!";
        countdownText.transform.DOScale(Vector3.one * 1f, 0.5f).SetEase(Ease.OutBounce);

        //Debug.Log("Countdown finished. Triggering Rewind animation...");

        // Tunggu animasi selesai sebelum menghancurkan objek
        yield return new WaitForSeconds(1f);

        ClearSpawnedObjects();
        countdownText.text = "";

        // Panggil Item Bar
        menuBarController.MoveObject();
    }

    // Hapus Objek
    void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }

        spawnedObjects.Clear();
        objectAtSpawnPoint.Clear();
    }

    // Respawn objek
    public void RespawnObjects()
    {
        ClearSpawnedObjects();
        StartCoroutine(RespawnObjectsOneByOne());
    }

    // Spawn Objek 1 1
    IEnumerator RespawnObjectsOneByOne()
    {
        foreach (var kvp in spawnPositionStatus)
        {
            // Dapatkan nama objek untuk direspawn
            GameObject objectToSpawn = GetObjectByName(kvp.Value);
            if (objectToSpawn != null)
            {
                // Cari indeks posisi spawn awal (kvp.Key) di spawnPositions
                int spawnIndex = System.Array.IndexOf(spawnPositions, kvp.Key);

                if (spawnIndex >= 0 && spawnIndex < comparePositions.Length)
                {
                    // Gunakan posisi dari comparePositions berdasarkan indeks
                    Transform newSpawnPosition = comparePositions[spawnIndex];

                    // Respawn objek di posisi baru
                    SpawnObjectAtPosition(objectToSpawn, newSpawnPosition, 2);

                    // Debug.Log($"Respawned {objectToSpawn.name} at {newSpawnPosition.name}");
                }
                else
                {
                    // Debug.LogWarning($"Invalid spawn index for position: {kvp.Key.name}. Ensure comparePositions matches spawnPositions.");
                }

                yield return new WaitForSeconds(0.2f); // Sesuaikan delay jika diperlukan
            }
            else
            {
                // Debug.LogWarning($"Object with name {kvp.Value} not found in objectTypes!");
            }
        }
    }

    // Dapatkan nama Objek
    GameObject GetObjectByName(string name)
    {
        foreach (var obj in objectTypes)
        {
            if (obj.name == name)
                return obj;
        }
        return null;
    }

    // Fungsi Restart Game
    public void RestartGame()
    {
        SetDifficultyParameters(); // Reset difficulty if needed
        StartCoroutine(SpawnNewObjects()); // Spawn new objects with potentially different data
    }

    // Database untuk simpan data
    public Dictionary<Transform, string> GetSpawnPositionStatus()
    {
        return new Dictionary<Transform, string>(spawnPositionStatus);
    }

    // Melakukan perbandingan
    void ComparePlayerData()
    {
        Dictionary<Transform, string> playerPlacedObjects = new Dictionary<Transform, string>(spawnPositionStatus);

        List<Transform> keys = new List<Transform>(playerPlacedObjects.Keys);
        if (keys.Count >= 4)
        {
            playerPlacedObjects[keys[0]] = "WrongObject1";
            playerPlacedObjects[keys[1]] = "WrongObject2";
        }

        int matchingCount = 0;
        foreach (var kvp in spawnPositionStatus)
        {
            if (playerPlacedObjects.ContainsKey(kvp.Key) && playerPlacedObjects[kvp.Key] == kvp.Value)
            {
                matchingCount++;
            }
        }

        float matchPercentage = ((float)matchingCount / spawnPositionStatus.Count) * 100;
        countdownText.text = $"Match: {matchPercentage}%";
        // Debug.Log($"Match percentage: {matchPercentage}%");
    }
}