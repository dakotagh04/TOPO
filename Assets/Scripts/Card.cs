using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardFamily { Obrero, Cocinero, Medico, Policia }
public enum CardSpecial { None, Ladron, Mago, Miron, Tapon, Pala, Terremoto, Pillin }

[System.Serializable]
public class Card
{
    public string id;
    public CardFamily family;
    public CardSpecial special;
    public bool isSpecial;
    public bool isBlocked;

    public Card(string id, CardFamily family)
    {
        this.id = id;
        this.family = family;
        this.isSpecial = false;
        this.isBlocked = false;
        this.special = CardSpecial.None;
    }

    public Card(string id, CardSpecial special)
    {
        this.id = id;
        this.special = special;
        this.isSpecial = true;
        this.isBlocked = false;
        this.family = CardFamily.Obrero; // valor por defecto, no se usa
    }
}