using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Mazo central")]
    public Button drawPileButton;
    public TextMeshProUGUI deckCountText;

    [Header("Jugador local (abajo)")]
    public RectTransform localHandContainer;

    [Header("Jugador rival (arriba)")]
    public RectTransform rivalHandContainer;

    [Header("Info turno")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI messageText;

    [Header("Prefab carta")]
    public GameObject cardPrefab;

    [Header("Botones de rival (para especiales)")]
    public GameObject rivalSelectPanel;
    public Button[] rivalButtons;

    void Awake() { Instance = this; }

    void Start()
    {
        if (drawPileButton != null)
            drawPileButton.onClick.AddListener(OnDrawPileClicked);

        if (rivalSelectPanel != null)
            rivalSelectPanel.SetActive(false);

        // Conectar botones de rival
        for (int i = 0; i < rivalButtons.Length; i++)
        {
            int index = i + 1; // rival 0 es el jugador local, empezamos en 1
            if (rivalButtons[i] != null)
                rivalButtons[i].onClick.AddListener(() => GameManager.Instance.OnRivalSelected(index));
        }

        RefreshUI();
    }

    void OnDrawPileClicked()
    {
        GameManager.Instance.OnDrawPileClicked();
    }

    public void RefreshUI()
    {
        if (deckCountText != null)
            deckCountText.text = "Cartas: " + DeckManager.Instance.CardsLeft();

        Player current = GameManager.Instance.GetCurrentPlayer();
        if (turnText != null && current != null)
            turnText.text = "Turno: " + current.playerName;

        if (GameManager.Instance.players.Count >= 2)
        {
            RenderHand(localHandContainer, GameManager.Instance.players[0], true);
            RenderHand(rivalHandContainer, GameManager.Instance.players[1], false);
        }
    }

    void RenderHand(RectTransform container, Player player, bool isLocal)
    {
        if (container == null || player == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        for (int i = 0; i < player.hand.Count; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, container);
            cardGO.SetActive(true);

            RectTransform rt = cardGO.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(80, 120);

            CardUI cardUI = cardGO.GetComponent<CardUI>();
            if (cardUI != null)
                cardUI.Setup(player.hand[i], i, isLocal);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    // Mostrar/ocultar panel de selección de rival
    public void ShowRivalSelection(bool show)
    {
        if (rivalSelectPanel != null)
            rivalSelectPanel.SetActive(show);
    }

    // Resaltar cartas del rival para que el jugador elija
    public void HighlightRivalCards(int rivalIndex, bool highlight)
    {
        RectTransform container = rivalHandContainer;
        foreach (Transform child in container)
        {
            CardUI cardUI = child.GetComponent<CardUI>();
            if (cardUI != null)
                cardUI.SetHighlight(highlight);
        }
    }

    // Revelar una carta del rival brevemente (Mirón)
    public void RevealCard(Player player, int cardIndex, float duration)
    {
        StartCoroutine(RevealCardCoroutine(player, cardIndex, duration));
    }

    IEnumerator RevealCardCoroutine(Player player, int cardIndex, float duration)
    {
        bool isLocal = player == GameManager.Instance.players[0];
        RectTransform container = isLocal ? localHandContainer : rivalHandContainer;

        if (cardIndex >= container.childCount) yield break;

        CardUI cardUI = container.GetChild(cardIndex).GetComponent<CardUI>();
        if (cardUI == null) yield break;

        cardUI.SetRevealed(true);
        yield return new WaitForSeconds(duration);
        cardUI.SetRevealed(false);
    }

    public void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
        CancelInvoke("ClearMessage");
        Invoke("ClearMessage", 3f);
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }
}