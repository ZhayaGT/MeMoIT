using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

// Script untuk mengatur item bar pada gameplay
public class MenuBarController : MonoBehaviour
{
    [Header("Menu Bars Component")]
    public List<GameObject> menuBars;
    public float moveDistance = 50f;
    public float animationDuration = 0.5f;

    [Header("Activate Object")]
    private GameObject activeMenu;
    public GameObject menuParent;
    public GameObject validateButton;
    public GameObject endMenu;
    public GameObject teksMenu;

    [Header("Position Information")]
    public float endPosition;
    public float originalPosition;
    public float duration = 1;

    // Mengset default posisi dan menambahkan listener
    void Start()
    {
        validateButton.GetComponent<Transform>().localScale = Vector3.zero;
        // Pastikan semua menu dalam posisi awal dan child[1] nonaktif
        foreach (var menu in menuBars)
        {
            menu.gameObject.GetComponent<Button>().onClick.AddListener(() => OnMenuClicked(menu));
            menu.transform.localPosition = Vector3.zero; // Posisi awal
            if (menu.transform.childCount > 1)
            {
                menu.transform.GetChild(1).gameObject.SetActive(false); // Nonaktifkan child[1]
            }
        }
    }

    // Fungsi yang mengatur item ketika di click
    public void OnMenuClicked(GameObject selectedMenu)
    {
        // Jika menu yang sama diklik, abaikan
        if (activeMenu == selectedMenu)
            return;

        // Turunkan menu yang sebelumnya aktif
        if (activeMenu != null)
        {
            ResetMenu(activeMenu);
        }

        // Naikkan menu yang baru diklik
        ActivateMenu(selectedMenu);

        AudioManager.Instance.PlaySFX(1);

        // Tetapkan menu yang baru sebagai aktif
        activeMenu = selectedMenu;
    }

    // Animasi Menu terpilih
    private void ActivateMenu(GameObject menu)
    {
        // Naikkan menu ke atas menggunakan DoTween
        menu.transform.DOLocalMoveY(moveDistance, animationDuration).SetEase(Ease.OutQuad);

        // Aktifkan child[1] jika ada
        if (menu.transform.childCount > 1)
        {
            menu.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    // Menurunkan Menu
    private void ResetMenu(GameObject menu)
    {
        // Turunkan menu ke posisi awal menggunakan DoTween
        menu.transform.DOLocalMoveY(126, animationDuration).SetEase(Ease.OutQuad);

        // Nonaktifkan child[1] jika ada
        if (menu.transform.childCount > 1)
        {
            menu.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    // Menampilkan teks validation
    public void MoveObject()
    {
        Sequence sequence = DOTween.Sequence();
        GameObject obj = menuParent;
        GameObject val = validateButton;
        Vector3 scale = new Vector3(1, 1, 1);
        // Set posisi awal

        // Pindahkan ke posisi akhir dengan DoTween
        sequence.Append(obj.transform.DOMoveY(endPosition, duration).SetEase(Ease.InOutQuad))
                .Append(val.transform.DOScale(scale, 1).SetEase(Ease.InOutQuad));
        
    }

    // Menunjukan UI hasil
    public void RewindObject()
    {
        Sequence sequence = DOTween.Sequence();
        GameObject obj = menuParent;
        GameObject val = validateButton;
        GameObject end = endMenu;
        Vector3 scale = new Vector3(0, 0, 0);

        // Pindahkan ke posisi awal dengan DoTween
        obj.transform.DOMoveY(originalPosition, duration).SetEase(Ease.InOutQuad);
        validateButton.transform.DOScale(scale, 0.5f).SetEase(Ease.InOutQuad);

        // Munculkan teks keterangan
        end.transform.DOMoveY(endPosition,duration).SetEase(Ease.InOutQuad);
        teksMenu.transform.DOMoveY(endPosition,duration).SetEase(Ease.InOutQuad);
    }

    // Reload Scene saat ini
    public void ReloadScene()
    {
        AudioManager.Instance.PlaySFX(0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Kembali ke Main Menu
    public void LoadMainMenu()
    {
        // Pastikan AudioManager instance tersedia
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(0);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null! No sound will play.");
        }

        Sequence sequence = DOTween.Sequence();

        // Animasi semua menu bar turun
        foreach (var menu in menuBars)
        {
            sequence.Join(menu.transform.DOLocalMoveY(0, animationDuration).SetEase(Ease.InOutQuad));
        }

        // Setelah animasi selesai, muat scene Main Menu
        sequence.OnComplete(() =>
        {
            SceneManager.LoadSceneAsync(0);
        });

        // Pastikan animasi dimulai
        sequence.Play();
    }
}
