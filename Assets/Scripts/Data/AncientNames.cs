using UnityEngine;

public static class AncientNames
{
    // "Header file" gibi: tek kaynak burasý.
    public static readonly string[] Names = new string[]
    {
        "Spartacus","Crixus","Gannicus","Oenomaus","Agron","Varro","Mira","Naevia","Ashur","Barca",
        "Michael","Batiatus","Lucretia","Glaber","Ilithyia","Crassus","Caesar","Tiberius","Kore","Saxa",
        "Lugo","Pietros","Duro","Nemetes","Sura","Sedullus","Castus","Heracleo","Nasir","Lucius",
        "Gaius","Marcus","Titus","Decimus","Aulus","Publius","Quintus","Sextus","Appius","Servius",
        "Spurius","Gnaeus","Lucullus","Aemilius","Cornelius","Julius","Cassius","Brutus","Antonius","Octavianus",
        "Drusus","Germanicus","Vitellius","Vespasianus","Traianus","Hadrianus","Aurelius","Severus","Maximus","Valerius",
        "Flavius","Claudius","Calvus","Longinus","Marcellus","Scaevola","Sulpicius","Fabius","Cato","Cicero",
        "Seneca","Plinius","Lentulus","Sergius","Catullus","Horatius","Ovidius","Virgilius","Nero","Galba",
        "Otho","Pertinax","Commodus","Domitianus","Nerva","Caracalla","Geta","Diocletianus","Constantinus","Licinius",
        "Rufus","Felix","Justus","Sejanus","Sabinus","Paullus","Priscus","Silvanus","Aquilinus","Aelianus",
        "Thracius","Rhesus","Kotys","Seuthes","Sitalces","Teres","Bessus","Dromichaetes","Bendis","Zalmoxis",
        "Gaelorix","Vercingetorix","Brennus","Atepomaros","Diviciacus","Dumnorix","Ambiorix","Viridomaros","Segovax","Cavarinos",
        "Lucterius","Acco","Epasnactos","Bituitus","Dubnovellaunus","Caratacus","Cunobelinus","Boudicca","Prasutagus","Mandubracius",
        "Arminius","Segimerus","Maroboduus","Chariovalda","Thusnelda","Hermann","Wulfgar","Sigimar","Hadugast","Raginfrid",
        "Leonidas","Themistokles","Perikles","Alkibiades","Sokrates","Platon","Aristoteles","Diogenes","Epiktetos","Zeno",
        "Solon","Pythagoras","Herakles","Achilleus","Odysseus","Ajax","Hektor","Agamemnon","Menelaos","Patroklos",
        "Theseus","Orpheus","Jason","Perseus","Atalanta","Kassandra","Andromakhe","Penelope","Arete","Nausikaa",
        "Xanthos","Kyros","Dareios","Xerxes","Artabanos","Mithridates","Tigranes","Pharnakes","Arsakes","Orodes",
        "Anubis","Imhotep","Nefru","Khepri","Seti","Ramses","Horemheb","Ahmose","Nekhbet","Sobek",
        "Massinissa","Jugurtha","Syphax","Hamilcar","Hasdrubal","Hannibal","Mago","Bomilcar","Abdmelqart","Azelion"
    };

    /// Rastgele isim (tekrar olabilir)
    public static string RandomName()
    {
        if (Names == null || Names.Length == 0) return "Unnamed";
        return Names[Random.Range(0, Names.Length)];
    }

    /// Seed ile deterministik isim: ayný seed => ayný sonuç
    public static string NameFromSeed(int seed)
    {
        if (Names == null || Names.Length == 0) return "Unnamed";
        int idx = Mathf.Abs(seed) % Names.Length;
        return Names[idx];
    }
}
