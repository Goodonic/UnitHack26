using System.Collections;
using UnityEngine;

public class GameBoyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gameBoyObject;
    [SerializeField] private Animator animator;

    private bool inBattle;

    public static GameBoyController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        gameBoyObject.SetActive(false);
    }

    public void StartBattle()
    {
        if (inBattle) return;

        inBattle = true;

        gameBoyObject.SetActive(true);

        animator.ResetTrigger("CloseTrigger");
        animator.SetTrigger("OpenTrigger");
    }

    public void EndBattle()
    {
        if (!inBattle) return;

        inBattle = false;

        animator.ResetTrigger("OpenTrigger");
        animator.SetTrigger("CloseTrigger");

        StartCoroutine(HideAfterClose());
    }

    private IEnumerator HideAfterClose()
    {
        yield return new WaitForSeconds(0.6f); // под анимацию закрытия
        gameBoyObject.SetActive(false);
    }
}