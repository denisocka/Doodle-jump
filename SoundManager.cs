using NAudio.Wave;

static class SoundManager
{
    public static void PlaySound(Stream soundStream)
    {
        soundStream.Position = 0; 

        var reader = new WaveFileReader(soundStream);
        var output = new WaveOutEvent();

        output.Init(reader);
        output.Play();

        output.PlaybackStopped += (s, e) =>
        {
            output.Dispose();
            reader.Dispose();
        };
    }
}