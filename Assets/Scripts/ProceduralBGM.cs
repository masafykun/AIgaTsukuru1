using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralBGM : MonoBehaviour
{
    // C major pentatonic (C D E G A), 2 octaves
    static readonly float[] Melody =
    {
        261.63f, 329.63f, 392.00f, 523.25f,   // C E G C5
        523.25f, 440.00f, 392.00f, 329.63f,   // C5 A G E
        293.66f, 392.00f, 440.00f, 523.25f,   // D G A C5
        659.26f, 523.25f, 392.00f, 261.63f,   // E5 C5 G C
    };
    // Bass (one octave lower, half tempo)
    static readonly float[] Bass =
    {
        130.81f, 130.81f, 196.00f, 196.00f,
        261.63f, 261.63f, 196.00f, 196.00f,
        146.83f, 146.83f, 220.00f, 220.00f,
        261.63f, 261.63f, 196.00f, 130.81f,
    };

    const float NoteLen = 0.18f;
    const float MelodyVol = 0.18f;
    const float BassVol   = 0.10f;

    int sampleRate;
    double melodyPhase, bassPhase;
    long sampleCount;

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        var src = GetComponent<AudioSource>();
        var clip = AudioClip.Create("BGM", sampleRate * 4, 1, sampleRate, true, OnRead, OnSeek);
        src.clip = clip;
        src.loop   = true;
        src.volume = 1f;
        src.Play();
    }

    void OnRead(float[] data)
    {
        long sampPerNote     = (long)(NoteLen * sampleRate);
        long sampPerBassNote = sampPerNote * 2;

        for (int i = 0; i < data.Length; i++)
        {
            int mIdx = (int)((sampleCount / sampPerNote)     % Melody.Length);
            int bIdx = (int)((sampleCount / sampPerBassNote) % Bass.Length);

            float mFreq = Melody[mIdx];
            float bFreq = Bass[bIdx];

            float mT = (float)(sampleCount % sampPerNote)     / sampPerNote;
            float bT = (float)(sampleCount % sampPerBassNote) / sampPerBassNote;

            float mEnv = Envelope(mT);
            float bEnv = Envelope(bT);

            // Square wave for melody, triangle for bass
            melodyPhase += mFreq / sampleRate;
            if (melodyPhase >= 1.0) melodyPhase -= 1.0;
            float mSample = (melodyPhase < 0.5 ? 1f : -1f) * MelodyVol * mEnv;

            bassPhase += bFreq / sampleRate;
            if (bassPhase >= 1.0) bassPhase -= 1.0;
            float bSample = (float)(bassPhase < 0.5 ? bassPhase * 4 - 1 : 3 - bassPhase * 4) * BassVol * bEnv;

            data[i] = mSample + bSample;
            sampleCount++;
        }
    }

    void OnSeek(int pos)
    {
        sampleCount  = pos;
        melodyPhase  = 0;
        bassPhase    = 0;
    }

    static float Envelope(float t) =>
        t < 0.05f ? t / 0.05f :
        t > 0.80f ? (1f - t) / 0.20f : 1f;
}
