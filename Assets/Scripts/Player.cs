using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerID;
    public string playerName;
    public List<Card> hand = new List<Card>();
    public const int MAX_HAND = 6;
    public const int FAMILY_WIN = 4;

    public bool AddCard(Card card)
    {
        if (hand.Count >= MAX_HAND)
        {
            Debug.LogWarning("Mano llena, no se puede añadir carta.");
            return false;
        }
        hand.Add(card);
        return true;
    }

    public Card RemoveCard(int index)
    {
        if (index < 0 || index >= hand.Count) return null;
        Card removed = hand[index];
        hand.RemoveAt(index);
        return removed;
    }

    public bool CheckWin()
    {
        // Contar cartas por familia (solo no bloqueadas)
        Dictionary<CardFamily, int> count = new Dictionary<CardFamily, int>();

        foreach (Card c in hand)
        {
            if (c.isSpecial) continue;
            if (c.isBlocked) continue;

            if (!count.ContainsKey(c.family))
                count[c.family] = 0;

            count[c.family]++;

            if (count[c.family] >= FAMILY_WIN)
                return true;
        }
        return false;
    }

    public bool HandFull()
    {
        return hand.Count >= MAX_HAND;
    }
}