using UnityEngine;

public class GameSFX : MonoBehaviour
{
    public static GameSFX Instance { get; private set; }

    AudioSource source;
    AudioClip dropClip;
    AudioClip scoreClip;
    AudioClip bigScoreClip;
    AudioClip gameOverClip;

    void Awake()
    {
        Instance = this;
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;

        int sr = AudioSettings.outputSampleRate;
        dropClip      = MakeSweep(sr, 700f, 350f, 0.4f, 0.10f);
        scoreClip     = MakeTone(sr,  880f, 0.4f, 0.08f);
        bigScoreClip  = MakeArpeggio(sr, new[] { 523.25f, 659.26f, 783.99f }, 0.4f, 0.10f);
        gameOverClip  = MakeGameOver(sr);
    }

    public void PlayDrop()      => source.PlayOneShot(dropClip,     0.5f);
    public void PlayScore(int pts)
    {
        if (pts >= 500) source.PlayOneShot(bigScoreClip, 0.7f);
        else            source.PlayOneShot(scoreClip,    0.6f);
    }
    public void PlayGameOver()  => source.PlayOneShot(gameOverClip, 0.8f);

    // ── generators ───────────────────────────────────────────────────────

    static AudioClip MakeTone(int sr, float freq, float vol, float dur)
    {
        int n = (int)(dur * sr);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float env = t < 0.1f ? t / 0.1f : 1f - t;
            d[i] = Mathf.Sin(2 * Mathf.PI * freq * i / sr) * vol * env;
        }
        return Make(d, sr);
    }

    static AudioClip MakeSweep(int sr, float f0, float f1, float vol, float dur)
    {
        int n = (int)(dur * sr);
        var d = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            float progress = (float)i / n;
            double freq = f0 + (f1 - f0) * progress;
            float env   = 1f - progress * 0.8f;
            phase += freq / sr;
            d[i]  = (float)System.Math.Sin(2 * System.Math.PI * phase) * vol * env;
        }
        return Make(d, sr);
    }

    static AudioClip MakeArpeggio(int sr, float[] freqs, float vol, float noteDur)
    {
        int noteN = (int)(noteDur * sr);
        int total = noteN * freqs.Length;
        var d = new float[total];
        for (int ni = 0; ni < freqs.Length; ni++)
        {
            int start = ni * noteN;
            for (int i = 0; i < noteN; i++)
            {
                float t   = (float)i / noteN;
                float env = t < 0.1f ? t / 0.1f : 1f - t;
                d[start + i] = Mathf.Sin(2 * Mathf.PI * freqs[ni] * i / sr) * vol * env;
            }
        }
        return Make(d, sr);
    }

    static AudioClip MakeGameOver(int sr)
    {
        float[] freqs = { 523.25f, 440f, 392f, 329.63f };
        float noteDur  = 0.28f;
        int noteN      = (int)(noteDur * sr);
        var d = new float[noteN * freqs.Length];
        for (int ni = 0; ni < freqs.Length; ni++)
        {
            int start = ni * noteN;
            for (int i = 0; i < noteN; i++)
            {
                float t   = (float)i / noteN;
                float env = t < 0.05f ? t / 0.05f : 1f - t;
                d[start + i] = Mathf.Sin(2 * Mathf.PI * freqs[ni] * i / sr) * 0.45f * env;
            }
        }
        return Make(d, sr);
    }

    static AudioClip Make(float[] data, int sr)
    {
        var clip = AudioClip.Create("sfx", data.Length, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}
