using UnityEngine;
using System.Collections;

public enum AIState
{
    Idle, Approach, Attack, Stunned, Roaming // Roaming: Gezinme modu eklendi
}

public class GladiatorAI : MonoBehaviour
{
    public GladiatorData Data;
    public AIState State = AIState.Idle;
    public Transform Target;

    // --- YENÝ DEÐÝÞKEN: BU GLADYATÖR NEREDE? ---
    public bool IsCombatMode = false;
    // ------------------------------------------

    [Header("Savaþ Ayarlarý")]
    public float AttackRange = 1.4f;
    public float AttackCooldown = 1.5f;
    public float DamageDelay = 0.4f;
    public float StunDuration = 0.5f;

    [Header("Hareket Ayarlarý")]
    public float BaseMoveSpeed = 1.8f;      // world speed (animle uyum)
    public float AgilityMoveBonus = 0.06f;  // agility çarpaný

    [Header("Ludus Ayarlarý")]
    public float WanderRadius = 3.0f; // Ne kadar uzaða koþabilir?
    private Vector2 wanderTarget;     // Gezmek için seçtiði hedef nokta
    private float wanderTimer = 0f;

    private float lastAttackTime = -999f;
    private bool isDead = false;

    private bool attackInProgress = false;
    private GladiatorAI cachedTargetAI = null;

    public float KnockbackDistance = 0.25f;
    public float KnockbackTime = 0.08f;


    float GetAgilitySpeedMul()
    {
        // Agility float olduðu için aþýrý uçmasýn diye clamp
        return Mathf.Clamp(1f + (Data.Agility * 0.08f), 0.85f, 2.0f);
    }

    float GetAttackCooldown()
    {
        // Agility arttýkça cooldown düþsün
        float mul = GetAgilitySpeedMul();
        return AttackCooldown / mul;
    }

    // Baþlangýç noktasý (Ludus'ta çok uzaklaþmasýn diye)
    private Vector2 startPosition;

    void Start()
    {
        startPosition = transform.position;
        // Baþlangýçta rastgele bir hedef belirle (Ludus modu için)
        PickRandomWanderPoint();
    }

    public void GetHit()
    {
        if (isDead || !IsCombatMode) return; // Ludus'ta hasar almasýnlar
        StopAllCoroutines();
        State = AIState.Stunned;
        StartCoroutine(Knockback());
        StartCoroutine(RecoverFromStun());
    }

    IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(StunDuration);
        if (!isDead && Data.HP > 0) State = AIState.Approach;
    }

    void Update()
    {
        if (isDead || Data == null || Data.HP <= 0) return;
        if (Data.HP <= 0 && IsCombatMode) { HandleDeath(); return; }

        // --- ANA AYRIM NOKTASI ---
        if (IsCombatMode)
        {
            UpdateCombatLogic();
        }
        else
        {
            UpdateLudusLogic();
        }
    }

    // --- SAVAÞ MANTIÐI (ESKÝ KODLAR BURAYA TAÞINDI) ---
    void UpdateCombatLogic()
    {
        if (Target == null) return;
        var targetAI = Target.GetComponent<GladiatorAI>();

        // Rakip hit/dodge/stun içindeyken saldýrma: bekle
        if (targetAI != null && targetAI.State == AIState.Stunned)
        {
            State = AIState.Idle;
            var v = GetComponent<GladiatorView>();
            if (v != null) v.SetRunning(false);
            return;
        }

        if (targetAI != null && targetAI.Data.HP <= 0)
        {
            State = AIState.Idle;
            GetComponent<GladiatorView>().SetRunning(false);
            return;
        }

        if (State == AIState.Stunned) { GetComponent<GladiatorView>().SetRunning(false); return; }

        float dist = Vector2.Distance(transform.position, Target.position);

        // Mesafe Kararlarý
        if (State == AIState.Approach)
        {
            if (dist <= AttackRange) State = AIState.Attack;
        }
        else if (State == AIState.Attack)
        {
            if (dist > AttackRange + 0.2f) State = AIState.Approach;
        }
        else
        {
            if (dist > AttackRange + 0.2f) State = AIState.Approach;
        }

        // Görsel
        var view = GetComponent<GladiatorView>();
        if (view != null) view.SetRunning(State == AIState.Approach);
        FlipTowards(Target.position);

        // Hareket
        if (State == AIState.Approach)
        {
            float moveSpeed = BaseMoveSpeed * (1f + (Data.Agility * AgilityMoveBonus));
            transform.position = Vector2.MoveTowards(transform.position, Target.position, moveSpeed * Time.deltaTime);
        }
        else if (State == AIState.Attack)
        {
            if (!attackInProgress && Time.time >= lastAttackTime + GetAttackCooldown())
                StartCoroutine(PerformAttack());

        }
    }

    // --- YENÝ LUDUS MANTIÐI (ANTRENMAN VE DÝNLENME) ---
    void UpdateLudusLogic()
    {
        var view = GetComponent<GladiatorView>();
        if (view == null) return;

        // 1. DÝNLENME (RESTING)
        if (Data.CurrentState == GladiatorState.Resting)
        {
            // Olduðu yerde dursun veya otursun
            view.SetRunning(false);
            // Ýleride buraya "view.PlaySit()" gibi bir animasyon ekleyebilirsin.
        }

        // 2. ANTRENMAN (TRAINING)
        else if (Data.CurrentState == GladiatorState.Training)
        {
            // Antrenman mantýðý: Rastgele bir yere koþ, bekle, tekrar koþ.
            float dist = Vector2.Distance(transform.position, wanderTarget);

            if (dist < 0.1f)
            {
                // Hedefe vardý, biraz bekle
                view.SetRunning(false);
                wanderTimer += Time.deltaTime;

                if (wanderTimer > 2.0f) // 2 saniye bekle
                {
                    PickRandomWanderPoint();
                    wanderTimer = 0f;
                }
            }
            else
            {
                // Hedefe koþ
                view.SetRunning(true);
                FlipTowards(wanderTarget);
                transform.position = Vector2.MoveTowards(transform.position, wanderTarget, (Data.Agility * 0.5f) * Time.deltaTime); // Yarý hýzda koþsun
            }
        }

        // 3. BOÞTA (IDLE)
        else
        {
            view.SetRunning(false);
        }
    }

    void PickRandomWanderPoint()
    {
        // Baþlangýç noktasýnýn etrafýnda rastgele bir nokta seç
        Vector2 randomPoint = Random.insideUnitCircle * WanderRadius;
        wanderTarget = startPosition + randomPoint;
    }

    void FlipTowards(Vector3 targetPos)
    {
        var view = GetComponent<GladiatorView>();
        if (view == null) return;

        // hedef saðdaysa saða bak, soldaysa sola bak
        bool faceRight = targetPos.x > transform.position.x;
        view.SetFacing(faceRight);
    }



    IEnumerator PerformAttack()
    {
        if (attackInProgress) yield break;

        attackInProgress = true;
        lastAttackTime = Time.time;

        // Koþmayý kapat
        var myView = GetComponent<GladiatorView>();
        if (myView != null)
        {
            myView.SetRunning(false);

            // Agility anim hýzýný da etkiletsin
            if (myView.UnitAnimator != null)
                myView.UnitAnimator.speed = GetAgilitySpeedMul();

            int attackIndex = (Random.value > 0.5f) ? 0 : 1;
            myView.PlayAttack(attackIndex);
        }

        // Hedefi cache'le (Event geldiðinde target null olmasýn diye)
        cachedTargetAI = null;
        if (Target != null) cachedTargetAI = Target.GetComponent<GladiatorAI>();

        // Artýk hasarý burada vermiyoruz!
        // Hasar: Animation Event -> AnimEvent_AttackHit()
        // Attack bitiþi: Animation Event -> AnimEvent_AttackEnd()
        yield return null;
    }


    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        // 1) AI'ý tamamen durdur (Update'te erken return için de kullanýlýyor olmalý)
        IsCombatMode = false;
        State = AIState.Idle; // veya Dead state'in varsa AIState.Dead yap

        // 2) Saldýrý / hareket coroutine'lerini kes
        StopAllCoroutines();

        // 3) Collider'larý kapat (temas / hedefleme dursun)
        var col2D = GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = false;

        var box2D = GetComponent<BoxCollider2D>();
        if (box2D != null) box2D.enabled = false;

        // 4) Animasyonu anýnda death'e çek
        var view = GetComponent<GladiatorView>();
        if (view != null)
        {
            view.SetRunning(false);  // Run1 bool kapanýr
            view.PlayDeath();        // Death trigger
        }

        // 5) Görseli arkaya al (opsiyonel)
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = -10;

        // 6) Eðer child'larda sprite renderer varsa onlarý da arkaya al (variant prefablar için)
        foreach (var childSr in GetComponentsInChildren<SpriteRenderer>())
            childSr.sortingOrder = -10;
    }


    // Attack animasyonunun "vur" frame'ine Animation Event olarak koyacaðýz
    public void AnimEvent_AttackHit()
    {
        if (isDead || State == AIState.Stunned || !IsCombatMode) return;
        if (cachedTargetAI == null) return;
        if (cachedTargetAI.Data == null || cachedTargetAI.Data.HP <= 0) return;

        // Hâlâ menzilde mi?
        if (Target != null && Vector2.Distance(transform.position, Target.position) < AttackRange + 0.5f)
        {
            CombatResolver.Resolve(Data, cachedTargetAI);
        }
    }

    // Attack animasyonunun sonuna Animation Event olarak koyacaðýz
    public void AnimEvent_AttackEnd()
    {
        attackInProgress = false;

        // Anim hýzýný normale döndür (istersen burada býrakabilirsin)
        var myView = GetComponent<GladiatorView>();
        if (myView != null && myView.UnitAnimator != null)
            myView.UnitAnimator.speed = 1f;
    }

    IEnumerator Knockback()
    {
        if (Target == null) yield break;

        Vector2 dir = (transform.position - Target.position).normalized;
        Vector2 start = transform.position;
        Vector2 end = start + dir * KnockbackDistance;

        float t = 0f;
        while (t < KnockbackTime)
        {
            t += Time.deltaTime;
            transform.position = Vector2.Lerp(start, end, t / KnockbackTime);
            yield return null;
        }
    }


}