using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    private List<Card> deck = new List<Card>();

    void Awake()
    {
        Instance = this;
    }

    public void BuildDeck()
    {
        deck.Clear();

        // 6 cartas por familia
        CardFamily[] families = { CardFamily.Obrero, CardFamily.Cocinero, CardFamily.Medico, CardFamily.Policia };
        foreach (CardFamily f in families)
        {
            for (int i = 1; i <= 6; i++)
            {
                deck.Add(new Card(f.ToString() + "_" + i, f));
            }
        }

        // Cartas especiales x4
        CardSpecial[] specials4 = { CardSpecial.Ladron, CardSpecial.Mago, CardSpecial.Miron, CardSpecial.Tapon, CardSpecial.Pala };
        foreach (CardSpecial s in specials4)
        {
            for (int i = 1; i <= 4; i++)
            {
                deck.Add(new Card(s.ToString() + "_" + i, s));
            }
        }

        // Cartas especiales x2
        for (int i = 1; i <= 2; i++)
        {
            deck.Add(new Card("Terremoto_" + i, CardSpecial.Terremoto));
            deck.Add(new Card("Pillin_" + i, CardSpecial.Pillin));
        }

        ShuffleDeck();
        Debug.Log("Mazo creado con " + deck.Count + " cartas.");
    }

    void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    public Card DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning("El mazo está vacío.");
            return null;
        }
        Card drawn = deck[0];
        deck.RemoveAt(0);
        return drawn;
    }

    public void ReturnCard(Card card)
    {
        int insertAt = Random.Range(0, deck.Count + 1);
        deck.Insert(insertAt, card);
    }

    public int CardsLeft()
    {
        return deck.Count;
    }
}