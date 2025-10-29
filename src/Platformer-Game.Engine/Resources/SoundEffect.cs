using Raylib_cs;

namespace PlatformerGame.Engine.Resources
{
    public class SoundEffect : Resource
    {
        private Sound[][] _soundBuffer;
        private int[] _currentAlias;
        private Random _random;

        public SoundEffect(string[] sourcePath, int maxConcurrent = 1)
            : base(ResourceType.SoundEffect)
        {
            if (maxConcurrent <= 0)
                throw new ArgumentException($"Invalid maxConcurrent value passed for sound effect ({maxConcurrent}), there must be at least 1 slot");

            _soundBuffer = new Sound[sourcePath.Length][];
            _currentAlias = new int[sourcePath.Length];
            _random = new Random();

            for (int i = 0; i < sourcePath.Length; i++)
            {
                _soundBuffer[i] = new Sound[maxConcurrent];
                Sound[] sounds = _soundBuffer[i];

                // Load sound from source and initialize aliases
                sounds[0] = Raylib.LoadSound(sourcePath[i]);
                for (int j = 1; j < maxConcurrent; j++)
                    sounds[j] = Raylib.LoadSoundAlias(sounds[0]);
            }
        }

        public override void Dispose()
        {
            foreach (Sound[] buffer in _soundBuffer)
            {
                for (int i = 1; i < SoundAliasCount; i++)
                    Raylib.UnloadSoundAlias(buffer[i]);
                Raylib.UnloadSound(buffer[0]);
            }
        }

        public int SoundSourcesCount => _soundBuffer.Length;
        public int SoundAliasCount => _soundBuffer[0].Length;

        public void Play()
        {
            int sourceIndex = 0;
            if (SoundSourcesCount > 1)
                sourceIndex = _random.Next(0, SoundSourcesCount);

            ref int currentIndex = ref _currentAlias[sourceIndex];
            Raylib.PlaySound(_soundBuffer[sourceIndex][currentIndex]);

            currentIndex++;
            // Wrap back around to the first alias sound and play from that
            if (currentIndex >= SoundAliasCount)
                currentIndex = 0;
        }
    }
}