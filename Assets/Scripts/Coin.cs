using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Coin : MonoBehaviour
{
    const float KillY = -7f;

    bool   landed;
    Color  coinColor;

    void Awake()
    {
        // ランダムカラー
        coinColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.9f, 1f);
        var rend = GetComponent<Renderer>();
        var mat  = rend.material;
        mat.color = coinColor;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", coinColor * 1.8f);

        // ランダム反発係数
        var pm = new PhysicsMaterial("CoinBounce");
        pm.bounciness      = Random.Range(0.2f, 0.7f);
        pm.bounceCombine   = PhysicsMaterialCombine.Maximum;
        pm.frictionCombine = PhysicsMaterialCombine.Minimum;
        GetComponent<SphereCollider>().material = pm;

        // トレイルエフェクト
        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time       = 0.25f;
        trail.startWidth = 0.18f;
        trail.endWidth   = 0f;
        var trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        trailMat.SetColor("_BaseColor", coinColor);
        trail.material   = trailMat;
        trail.startColor = coinColor;
        trail.endColor   = new Color(coinColor.r, coinColor.g, coinColor.b, 0f);
    }

    void Update()
    {
        if (!landed && transform.position.y < KillY)
            Land(0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (landed || !other.TryGetComponent<ScoreZone>(out var zone)) return;
        Land(zone.Points);
    }

    void Land(int points)
    {
        landed = true;
        if (points > 0)
        {
            GameManager.Instance?.AddScore(points, transform.position, coinColor);
            GameSFX.Instance?.PlayScore(points);
        }
        GameManager.Instance?.RegisterCoinLanded();
        SpawnBurst(transform.position, coinColor);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!landed)
            GameManager.Instance?.RegisterCoinLanded();
    }

    static void SpawnBurst(Vector3 pos, Color color)
    {
        var go = new GameObject("Burst");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor    = color;
        main.startSpeed    = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.maxParticles  = 25;
        main.loop          = false;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.15f;

        var pRend = ps.GetComponent<ParticleSystemRenderer>();
        var pMat  = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        pMat.SetColor("_BaseColor", color);
        pRend.material = pMat;

        ps.Play();
        Destroy(go, 2f);
    }
}
