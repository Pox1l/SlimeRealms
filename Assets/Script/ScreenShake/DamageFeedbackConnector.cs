using UnityEngine;
using MoreMountains.Feedbacks;
using System; // Pro jistotu

public class DamageFeedbackConnector : MonoBehaviour
{
    [Header("Feedbacks")]
    public MMF_Player smallShake;
    public MMF_Player bigShake;
    public MMF_Player bossLandShake; // 🔥 Nový slot pro bosse

    void Awake()
    {
        // Najde všechny MMF Playery v potomcích (i kdyby byly vypnuté)
        MMF_Player[] allFeedbacks = GetComponentsInChildren<MMF_Player>(true);

        foreach (var feedback in allFeedbacks)
        {
            string name = feedback.gameObject.name;

            // Hledá klíčová slova v názvu GameObjectu
            if (name.Contains("Small"))
            {
                smallShake = feedback;
            }
            else if (name.Contains("Big"))
            {
                bigShake = feedback;
            }
            // 🔥 Automaticky najde objekt, který má v názvu "Boss" nebo "Land"
            else if (name.Contains("Boss") || name.Contains("Land"))
            {
                bossLandShake = feedback;
            }
        }
    }

    void OnEnable()
    {
        // Přihlášení k Hráčovi
        PlayerStats.OnPlayerHit += DecideShake;

        // 🔥 Přihlášení ke starému bossovi (Slime)
        BossController.OnBossLand += PlayBossShake;

        // 🔥 NOVÉ: Přihlášení k novému bossovi (DKBoss)
        DKBossController.OnBossLand += PlayBossShake;

        Boss3Controller.OnBossLand += PlayBossShake;
    }

    void OnDisable()
    {
        PlayerStats.OnPlayerHit -= DecideShake;

        // Odhlášení starého bosse
        BossController.OnBossLand -= PlayBossShake;

        // 🔥 NOVÉ: Odhlášení nového bosse
        DKBossController.OnBossLand -= PlayBossShake;

        Boss3Controller.OnBossLand -= PlayBossShake;
    }

    // Logika pro hráče (podle damage)
    void DecideShake(int damageAmount)
    {
        if (damageAmount >= 10)
        {
            if (bigShake != null) bigShake.PlayFeedbacks();
        }
        else
        {
            if (smallShake != null) smallShake.PlayFeedbacks();
        }
    }

    // 🔥 Logika pro bosse (prostě přehraje shake)
    void PlayBossShake()
    {
        if (bossLandShake != null)
        {
            bossLandShake.PlayFeedbacks();
        }
    }
}