using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    public TextMeshProUGUI cardLabel;
    public Image cardBackground;
    public Image blockedOverlay;

    private Card cardData;
    private int cardIndex;
    private bool belongsToLocalPlayer;

    // Colores por familia
    static readonly Color colorObrero   = new Color(1f, 0.85f, 0.2f);   // amarillo
    static readonly Color colorCocinero = new Color(0.3f, 0.8f, 0.3f);  // verde
    static readonly Color colorMedico   = new Color(0.9f, 0.3f, 0.3f);  // rojo
    static readonly Color colorPolicia  = new Color(0.3f, 0.5f, 0.9f);  // azul
    static readonly Color colorSpecial  = new Color(0.6f, 0.3f, 0.8f);  // morado
    static readonly Color colorHidden   = new Color(0.3f, 0.3f, 0.3f);  // gris oscuro

    public void Setup(Card card, int index, bool isLocalPlayer)
    {
        cardData = card;
        cardIndex = index;
        belongsToLocalPlayer = isLocalPlayer;

        if (isLocalPlayer)
        {
            // Mostrar carta real
            if (card.isSpecial)
            {
                cardBackground.color = colorSpecial;
                cardLabel.text = card.special.ToString();
            }
            else
            {
                cardBackground.color = GetFamilyColor(card.family);
                cardLabel.text = card.family.ToString();
            }

            // Overlay de bloqueada
            if (blockedOverlay != null)
                blockedOverlay.gameObject.SetActive(card.isBlocked);
        }
        else
        {
            // Rival: mostrar dorso
            cardBackground.color = colorHidden;
            cardLabel.text = "?";
            if (blockedOverlay != null)
                blockedOverlay.gameObject.SetActive(false);
        }
    }

    Color GetFamilyColor(CardFamily family)
    {
        switch (family)
        {
            case CardFamily.Obrero:   return colorObrero;
            case CardFamily.Cocinero: return colorCocinero;
            case CardFamily.Medico:   return colorMedico;
            case CardFamily.Policia:  return colorPolicia;
            default: return Color.white;
        }
    }

    public void OnClick()
    {
        if (!belongsToLocalPlayer) return;
        if (GameManager.Instance.phase != TurnPhase.CardDrawn) return;
        GameManager.Instance.OnCardInHandClicked(cardIndex);
        UIManager.Instance.RefreshUI();
    }

    public void SetHighlight(bool on)
{
    if (cardBackground != null)
        cardBackground.color = on
            ? new Color(1f, 0.9f, 0.2f)  // amarillo highlight
            : (belongsToLocalPlayer ? GetFamilyColor(cardData.family) : colorHidden);
}

public void SetRevealed(bool revealed)
{
    if (!revealed)
    {
        Setup(cardData, cardIndex, belongsToLocalPlayer);
        return;
    }
    // Mostrar la carta aunque sea del rival
    cardBackground.color = cardData.isSpecial
        ? colorSpecial
        : GetFamilyColor(cardData.family);
    cardLabel.text = cardData.isSpecial
        ? cardData.special.ToString()
        : cardData.family.ToString();
}
}

