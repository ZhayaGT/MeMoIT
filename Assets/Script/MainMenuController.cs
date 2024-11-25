using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

// Script yang mengatur Main Menu
public class MainMenuController : MonoBehaviour
{
    [Header("UI Components")]
    public RectTransform[] menuComponents;
    public Button difficultyButton;
    public Button playButton;          
    public Button exitButton;
    public TextMeshProUGUI difficultyText;

    [Header("Animation Settings")]
    public float popDuration = 0.3f;
    public float idleSwingDuration = 2f;
    public float idleSwingAmount = 10f;

    [Header("Difficulty Settings")]
    private string[] difficulties = { "Easy", "Medium", "Hard" };
    private int currentDifficultyIndex = 0;

    // Memainkan Animasi dan menambahkan fungsi pada button
    private void Start()
    {
        PlayPopAnimations();
        UpdateDifficultyText();

        // Tambahkan listener ke button
        difficultyButton.onClick.AddListener(ToggleDifficulty);
        playButton.onClick.AddListener(PlayGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    // Memainkan Animasi Pop
    private void PlayPopAnimations()
    {
        for (int i = 0; i < menuComponents.Length; i++)
        {
            RectTransform component = menuComponents[i];
            component.localScale = Vector3.zero; // Scale 0
            
            // Tambahkan delay
            DOVirtual.DelayedCall(i * popDuration, () =>
            {
                AudioManager.Instance.PlaySFX(0); // Mainkan SFX
            });

            component.DOScale(Vector3.one, popDuration)
                    .SetDelay(i * popDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        // Mulai idle animation
                        StartIdleAnimation(component);
                    });
        }
    }

    // Idle Animation (Bergoyang)
    private void StartIdleAnimation(RectTransform component)
    {
        // Tambahkan rotasi idle
        component.DORotate(new Vector3(0, 0, idleSwingAmount), idleSwingDuration)
                 .SetEase(Ease.InOutSine)
                 .SetLoops(-1, LoopType.Yoyo);
    }

    // Untuk Mengatur Difficulty
    private void ToggleDifficulty()
    {
        // Pindahkan index ke difficulty berikutnya
        currentDifficultyIndex = (currentDifficultyIndex + 1) % difficulties.Length;

        // Update teks pada tombol
        UpdateDifficultyText();

        // Mainkan SFX
        AudioManager.Instance.PlaySFX(0);

        // Animasi pop untuk tombol
        difficultyButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1);
    }

    // Mengganti Teks Difficulty
    private void UpdateDifficultyText()
    {
        difficultyText.text = difficulties[currentDifficultyIndex];
    }

    // Berpindah ke scene Gameplay
    private void PlayGame()
    {
        string selectedDifficulty = difficulties[currentDifficultyIndex];

        // Simpan Difficulty
        PlayerPrefs.SetString("SelectedDifficulty", selectedDifficulty);

        AudioManager.Instance.PlaySFX(0);

        // Animasi button play
        playButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1)
                  .OnComplete(() =>
                  {
                      // Pindah ke scene berikutnya
                      SceneManager.LoadScene(1);
                  });
    }

    // Fungsi Exit Game
    private void ExitGame()
    {

        AudioManager.Instance.PlaySFX(0);
        // Animasi tombol exit
        exitButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1)
                .OnComplete(() =>
                {
                    #if UNITY_EDITOR
                                    // Jika dijalankan di Editor, tampilkan log dan hentikan Play Mode
                                    Debug.Log("Exiting game...");
                                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                                    // Jika dijalankan di build, keluar dari aplikasi
                                    Application.Quit();
                    #endif
                });
    }

}
