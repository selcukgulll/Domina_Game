using UnityEngine;

public enum GladiatorClass { Thraex, Retiarius, Murmillo }
public enum GladiatorState { Idle, Training, Resting } // State eklendi

[System.Serializable]
public class GladiatorData
{
    public int ID;
    public string Name;
    public GladiatorClass Class;
    public GladiatorState CurrentState = GladiatorState.Idle; // O an ne yapýyor?
    public int GridIndex = -1;

    [Header("Stats")]
    public float HP;
    public float MaxHP;
    public float Stamina;
    public float Strength;   // Hasar ve HP belirleyici
    public float Agility;    // Hýz
    public float Aggression; // Saldýrý sýklýðý
    public float Mentality;  // Stat çarpaný (1.0 = Normal, 1.2 = Boosted)
    public int XP;

    public int BodyTypeIndex = 0;


    public bool IsAlive => HP > 0;
    public int Wins = 0;

    // Constructor (Rastgele statlar için)
    public GladiatorData(int gladiatorid)
    {
        this.ID = gladiatorid;
        Name = AncientNames.RandomName();
        Class = (GladiatorClass)Random.Range(0, 3);

        Strength = Random.Range(5, 15);
        Agility = Random.Range(5, 15);
        Aggression = Random.Range(0.1f, 1.0f); // %10 ile %100 arasý saldýrganlýk
        Mentality = 1.0f;

        MaxHP = Strength * 10; // Strength caný belirler
        HP = MaxHP;
        Stamina = 100;
        XP = 0;

        Wins = 0;

        BodyTypeIndex = Random.Range(0, 2);
    }

    public void GainXP(int amount)
    {
        XP += amount;
        // Ýleride seviye atlama mantýðý buraya eklenecek
    }

    // Gladyatörün gücüne göre dinamik fiyat hesaplama
    public int CalculateValue()
    {
        float totalStats = Strength + Agility + (MaxHP / 10f);
        // Baz fiyat 50 + Stat baþýna 10 Gold
        int price = 50 + Mathf.RoundToInt(totalStats * 10);
        return price;
    }


}