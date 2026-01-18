[System.Serializable]
public class ResourcesData
{
    public int Gold = 5000;
    public int Food = 50;
    public int Water = 50;
    public int Wine = 10;

    // --- STAFF (PERSONEL) ---
    public bool HasDoctore = false; // XP Gain
    public bool HasMedicus = false; // Healing
    public bool HasFaber = false;   // Better Gear / Strength Gain

    public int MedicusLevel = 1;
    public int DoctoreLevel = 1;
    public int FaberLevel = 1;

    public void ConsumeDaily(int gladiatorCount)
    {
        Food -= gladiatorCount;
        Water -= gladiatorCount;
    }

    public bool HasBasicNeeds()
    {
        return Food > 0 && Water > 0;
    }
}