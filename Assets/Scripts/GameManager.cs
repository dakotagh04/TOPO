using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnPhase
{
    Waiting,
    CardDrawn,
    SelectingCardToDiscard,
    SelectingRivalForSpecial,
    SelectingCardForSpecial,
    EndTurn
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<Player> players = new List<Player>();
    public int currentTurn = 0;
    public TurnPhase phase = TurnPhase.Waiting;
    public Card drawnCard = null;
    public int localPlayerID = 0;

    // Para cartas especiales interactivas
    private CardSpecial pendingSpecial;
    private Player specialUser;
    private int selectedRivalIndex = -1;

    void Awake() { Instance = this; }

    void Start()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("No hay jugadores asignados.");
            return;
        }
        DeckManager.Instance.BuildDeck();
        DealStartingCards();
        UIManager.Instance.RefreshUI();
        UIManager.Instance.ShowMessage("Turno de " + GetCurrentPlayer().playerName);
    }

    void DealStartingCards()
    {
        for (int round = 0; round < 4; round++)
            foreach (Player p in players)
            {
                Card c = DeckManager.Instance.DrawCard();
                if (c != null) p.AddCard(c);
            }
    }

    public Player GetCurrentPlayer()
    {
        if (players == null || players.Count == 0) return null;
        if (currentTurn < 0 || currentTurn >= players.Count) return null;
        return players[currentTurn];
    }

    public bool IsMyTurn()
    {
        return currentTurn == localPlayerID;
    }

    // ── Robar carta ──────────────────────────────────────────
    public void OnDrawPileClicked()
    {
        if (phase != TurnPhase.Waiting) return;
        if (!IsMyTurn()) return;

        drawnCard = DeckManager.Instance.DrawCard();
        if (drawnCard == null)
        {
            UIManager.Instance.ShowMessage("El mazo está vacío.");
            return;
        }

        phase = TurnPhase.CardDrawn;

        if (drawnCard.isSpecial)
        {
            UIManager.Instance.ShowMessage("Carta especial: " + drawnCard.special.ToString() + " — elige un rival");
            pendingSpecial = drawnCard.special;
            specialUser = GetCurrentPlayer();
            phase = TurnPhase.SelectingRivalForSpecial;
            UIManager.Instance.ShowRivalSelection(true);
            return;
        }

        UIManager.Instance.ShowMessage("Robaste: " + drawnCard.family.ToString());

        if (!GetCurrentPlayer().HandFull())
        {
            GetCurrentPlayer().AddCard(drawnCard);
            drawnCard = null;
            EndTurn();
        }
        else
        {
            phase = TurnPhase.SelectingCardToDiscard;
            UIManager.Instance.ShowMessage("Mano llena — toca una carta para descartarla");
            UIManager.Instance.RefreshUI();
        }
    }

    // ── El jugador toca una carta de su mano ─────────────────
    public void OnCardInHandClicked(int cardIndex)
    {
        if (!IsMyTurn()) return;

        // Descartar para quedarse con la robada
        if (phase == TurnPhase.SelectingCardToDiscard)
        {
            Player current = GetCurrentPlayer();
            Card discarded = current.RemoveCard(cardIndex);
            if (discarded != null) DeckManager.Instance.ReturnCard(discarded);
            current.AddCard(drawnCard);
            drawnCard = null;
            EndTurn();
            return;
        }

        // Seleccionar carta propia para especial (Ladrón/Mago)
        if (phase == TurnPhase.SelectingCardForSpecial)
        {
            ExecuteSpecialWithCard(cardIndex);
            return;
        }
    }

    // ── Seleccionar rival para carta especial ─────────────────
    public void OnRivalSelected(int rivalIndex)
    {
        if (phase != TurnPhase.SelectingRivalForSpecial) return;

        selectedRivalIndex = rivalIndex;

        // Especiales que no necesitan elegir carta específica
        if (pendingSpecial == CardSpecial.Terremoto)
        {
            ActivateTerremoto(specialUser);
            EndSpecial();
            return;
        }

        if (pendingSpecial == CardSpecial.Miron)
        {
            ActivateMiron(specialUser, players[rivalIndex]);
            EndSpecial();
            return;
        }

        if (pendingSpecial == CardSpecial.Pillin)
        {
            ActivatePillin(players[rivalIndex]);
            EndSpecial();
            return;
        }

        if (pendingSpecial == CardSpecial.Pala)
        {
            ActivatePala(specialUser);
            EndSpecial();
            return;
        }

        // Especiales que necesitan elegir una carta del rival
        phase = TurnPhase.SelectingCardForSpecial;
        UIManager.Instance.ShowRivalSelection(false);
        UIManager.Instance.ShowMessage("Elige una carta del rival");
        UIManager.Instance.HighlightRivalCards(rivalIndex, true);
    }

    // ── Ejecutar especial con carta elegida ───────────────────
    void ExecuteSpecialWithCard(int cardIndex)
    {
        Player rival = players[selectedRivalIndex];

        switch (pendingSpecial)
        {
            case CardSpecial.Ladron:
                if (specialUser.hand.Count == 0 || rival.hand.Count == 0) break;
                int myIndex = Random.Range(0, specialUser.hand.Count);
                Card temp = specialUser.hand[myIndex];
                specialUser.hand[myIndex] = rival.hand[cardIndex];
                rival.hand[cardIndex] = temp;
                UIManager.Instance.ShowMessage("Ladrón: cartas intercambiadas");
                break;

            case CardSpecial.Mago:
                if (specialUser.hand.Count == 0 || rival.hand.Count == 0) break;
                int myIdx = Random.Range(0, specialUser.hand.Count);
                // Mago: el jugador ya vio ambas cartas antes de intercambiar
                UIManager.Instance.ShowMessage(
                    "Mago: tú tenías " + specialUser.hand[myIdx].id +
                    " | rival tenía " + rival.hand[cardIndex].id);
                Card t = specialUser.hand[myIdx];
                specialUser.hand[myIdx] = rival.hand[cardIndex];
                rival.hand[cardIndex] = t;
                break;

            case CardSpecial.Tapon:
                if (rival.hand[cardIndex].isSpecial)
                {
                    UIManager.Instance.ShowMessage("No puedes bloquear una carta especial");
                    return;
                }
                rival.hand[cardIndex].isBlocked = true;
                UIManager.Instance.ShowMessage("Tapón: carta bloqueada");
                break;
        }

        UIManager.Instance.HighlightRivalCards(selectedRivalIndex, false);
        EndSpecial();
    }

    void EndSpecial()
    {
        drawnCard = null;
        pendingSpecial = CardSpecial.None;
        specialUser = null;
        selectedRivalIndex = -1;
        EndTurn();
    }

    // ── Activar especiales sin selección de carta ─────────────
    void ActivateMiron(Player user, Player target)
    {
        if (target.hand.Count == 0) return;
        int index = Random.Range(0, target.hand.Count);
        string cardName = target.hand[index].isSpecial
            ? target.hand[index].special.ToString()
            : target.hand[index].family.ToString();
        UIManager.Instance.ShowMessage("Mirón: ves la carta " + cardName + " de " + target.playerName);
        UIManager.Instance.RevealCard(target, index, 3f);
    }

    void ActivateTerremoto(Player user)
    {
        foreach (Player p in players)
        {
            if (p == user) continue;
            foreach (Card c in p.hand)
                DeckManager.Instance.ReturnCard(c);
            p.hand.Clear();
        }
        UIManager.Instance.ShowMessage("Terremoto: todos los rivales descartan su mano");
    }

    void ActivatePala(Player user)
    {
        bool found = false;
        foreach (Card c in user.hand)
        {
            if (c.isBlocked) { c.isBlocked = false; found = true; break; }
        }
        UIManager.Instance.ShowMessage(found ? "Pala: carta desbloqueada" : "Pala: no hay cartas bloqueadas");
    }

    void ActivatePillin(Player rival)
    {
        for (int i = rival.hand.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            Card tmp = rival.hand[i];
            rival.hand[i] = rival.hand[r];
            rival.hand[r] = tmp;
        }
        UIManager.Instance.ShowMessage("Pillín: cartas de " + rival.playerName + " reordenadas");
    }

    // ── Fin de turno ──────────────────────────────────────────
    void EndTurn()
    {
        foreach (Player p in players)
        {
            if (p.CheckWin())
            {
                UIManager.Instance.ShowMessage("¡¡ GANA " + p.playerName.ToUpper() + " !!");
                phase = TurnPhase.Waiting;
                UIManager.Instance.RefreshUI();
                return;
            }
        }

        currentTurn = (currentTurn + 1) % players.Count;
        phase = TurnPhase.Waiting;
        UIManager.Instance.RefreshUI();
        UIManager.Instance.ShowMessage("Turno de " + GetCurrentPlayer().playerName);
    }
}