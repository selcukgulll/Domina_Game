using UnityEngine;

// --- EKSÝK OLAN KISIM BURASIYDI ---
public enum AIState
{
    Idle, Approach, Attack, Retreat, Block, Stunned
}
// ----------------------------------

public class GladiatorAI : MonoBehaviour
{
    public GladiatorData Data;
    public AIState State = AIState.Approach;
    public Transform Target;

    void Update()
    {
        // ÖLÜM KONTROLÜ
        if (Data.HP <= 0)
        {
            // Öldüyse rengini Kýrmýzý yap ve yan yatýr
            // GetComponent<SpriteRenderer>().color = Color.red;
            transform.rotation = Quaternion.Euler(0, 0, 90);
            return; // Hareketi durdur
        }

        if (Target == null) return;

        // EÐER RAKÝP ÖLDÜYSE SALDIRMAYI BIRAK
        var targetAI = Target.GetComponent<GladiatorAI>();
        if (targetAI != null && targetAI.Data.HP <= 0)
        {
            State = AIState.Idle; // Sakin dur
            return;
        }

        switch (State)
        {
            case AIState.Approach:
                MoveToTarget();
                break;
            case AIState.Attack:
                Attack();
                break;
            case AIState.Idle:
                // Zafer aný
                break;
        }

        // Basit mesafe kontrolü
        float dist = Vector2.Distance(transform.position, Target.position);
        if (dist < 1.5f) State = AIState.Attack;
        else State = AIState.Approach;
    }

    void MoveToTarget()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            Target.position,
            Data.Agility * Time.deltaTime
        );
    }
    void Attack()
    {
        // Saldýrý hýzý/þansý kontrolü
        if (Random.value < 0.05f)
        {
            // Hedefin üzerindeki AI scriptini al
            var targetAI = Target.GetComponent<GladiatorAI>();

            if (targetAI != null)
            {
                // Deðiþtirdiðimiz yeni Resolve fonksiyonunu çaðýr
                // (Kendi verimiz, Rakibin AI Scripti)
                CombatResolver.Resolve(Data, targetAI);
            }
        }
    }

    void LateUpdate()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.sortingOrder = -(int)(transform.position.y * 100);
    }
}