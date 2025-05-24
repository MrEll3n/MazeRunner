using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Singleton-based manager for handling audio playback using OpenAL, including music, sound effects,
    /// positional audio, walking sounds, and 3D listener configuration.
    /// </summary>
    public class SoundManager : IDisposable
    {
        private static SoundManager _instance;
        /// <summary>
        /// Gets the singleton instance of the SoundManager.
        /// </summary>
        public static SoundManager Instance => _instance ??= new SoundManager();

        private ALDevice device;
        private ALContext context;

        private int musicSource;
        private readonly Dictionary<string, int> bufferCache = new();
        private readonly List<int> soundSources = new();

        private const int MaxSimultaneousSounds = 32;

        // --- Kroky ---
        private readonly string[] walkingStepSounds = new[]
        {
            "assets/sfx/walking1.wav",
            "assets/sfx/walking2.wav",
            "assets/sfx/walking3.wav",
            "assets/sfx/walking4.wav"
        };

        private int lastStepIndex = -1;
        private float stepTimer = 0f;
        private float stepInterval = 0.5f;
        private bool wasWalking = false;

        private SoundManager()
        {
            device = ALC.OpenDevice(null);
            context = ALC.CreateContext(device, new ALContextAttributes());
            ALC.MakeContextCurrent(context);

            musicSource = AL.GenSource();
        }

        /// <summary>
        /// Plays background music from the specified path.
        /// </summary>
        /// <param name="path">Path to the WAV file.</param>
        /// <param name="volume">Volume of playback (0–1).</param>
        /// <param name="loop">Whether the music should loop.</param>
        public void PlayMusic(string path, float volume = 0.35f, bool loop = true)
        {
            int buffer = LoadBuffer(path);
            AL.SourceStop(musicSource);
            AL.Source(musicSource, ALSourcei.Buffer, buffer);
            AL.Source(musicSource, ALSourcef.Gain, volume);
            AL.Source(musicSource, ALSourceb.Looping, loop);
            AL.Source(musicSource, ALSource3f.Position, 0, 0, 0);
            AL.Source(musicSource, ALSource3f.Velocity, 0, 0, 0);
            AL.SourcePlay(musicSource);
        }

        /// <summary>
        /// Plays a 2D sound effect from the specified path.
        /// </summary>
        /// <param name="path">Path to the sound file.</param>
        /// <param name="volume">Playback volume.</param>
        public void PlaySound(string path, float volume = 1.0f)
        {
            PlaySound(path, Vector3.Zero, volume);
        }

        /// <summary>
        /// Plays a 3D positional sound effect.
        /// </summary>
        /// <param name="path">Path to the sound file.</param>
        /// <param name="position">World-space position of the sound.</param>
        /// <param name="volume">Playback volume.</param>
        public void PlaySound(string path, Vector3 position, float volume = 1.0f)
        {
            if (soundSources.Count >= MaxSimultaneousSounds)
                return;

            int buffer = LoadBuffer(path);
            int source = AL.GenSource();

            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.Source(source, ALSourcef.Gain, volume);
            AL.Source(source, ALSourceb.Looping, false);
            AL.Source(source, ALSource3f.Position, position.X, position.Y, position.Z);
            AL.Source(source, ALSource3f.Velocity, 0, 0, 0);
            AL.Source(source, ALSourcef.ReferenceDistance, 2.0f);
            AL.Source(source, ALSourcef.MaxDistance, 15.0f);
            AL.Source(source, ALSourcef.RolloffFactor, 1.0f);

            AL.SourcePlay(source);
            soundSources.Add(source);
        }

        /// <summary>
        /// Updates all active sounds and removes sources that have finished playing.
        /// Should be called once per frame.
        /// </summary>
        public void Update()
        {
            soundSources.RemoveAll(src =>
            {
                AL.GetSource(src, ALGetSourcei.SourceState, out int state);
                bool isDone = (ALSourceState)state == ALSourceState.Stopped;

                if (isDone)
                {
                    AL.DeleteSource(src);
                    return true;
                }

                AL.GetSource(src, ALGetSourcei.BuffersQueued, out int queued);
                if (queued == 0 && (ALSourceState)state == ALSourceState.Initial)
                {
                    AL.DeleteSource(src);
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Handles automatic walking sound effects based on player movement and speed.
        /// </summary>
        /// <param name="playerPosition">Current position of the player.</param>
        /// <param name="playerSpeed">Current horizontal speed of the player.</param>
        /// <param name="deltaTime">Elapsed time since last update.</param>
        public void UpdateWalking(Vector3 playerPosition, float playerSpeed, float deltaTime)
        {
            float startThreshold = 0.2f;
            float stopThreshold = 0.1f;

            float fastStep = 1f;
            float slowStep = 0.5f;

            bool isCurrentlyWalking = playerSpeed > startThreshold;

            if (!isCurrentlyWalking && wasWalking && playerSpeed < stopThreshold)
            {
                wasWalking = false;
                stepTimer = 0f;
                return;
            }

            if (isCurrentlyWalking)
            {
                wasWalking = true;
                stepTimer -= deltaTime;

                if (stepTimer <= 0f)
                {
                    float speedNormalized = Math.Clamp(playerSpeed / 3f, 0f, 1f);
                    float maxDuration = 0.5f + (0.25f - 0.5f) * speedNormalized;
                    stepInterval = fastStep + (slowStep - fastStep) * speedNormalized;
                    stepTimer = stepInterval;

                    // výběr náhodného kroku s vyloučením opakování
                    int index;
                    do
                    {
                        index = Random.Shared.Next(walkingStepSounds.Length);
                    } while (walkingStepSounds.Length > 1 && index == lastStepIndex);
                    lastStepIndex = index;

                    string sound = walkingStepSounds[index];
                    PlayStepSound(sound, 1f, maxDuration);
                }
            }
        }

        /// <summary>
        /// Plays a walking step sound with an enforced maximum duration.
        /// </summary>
        /// <param name="path">Path to the step sound file.</param>
        /// <param name="volume">Playback volume.</param>
        /// <param name="maxDurationSeconds">Maximum duration before forcefully stopping the sound.</param>
        public void PlayStepSound(string path, float volume, float maxDurationSeconds)
        {
            if (soundSources.Count >= MaxSimultaneousSounds)
                return;

            int buffer = LoadBuffer(path);
            int source = AL.GenSource();

            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.Source(source, ALSourcef.Gain, volume);
            AL.Source(source, ALSourceb.Looping, false);

            // 2D krokový zvuk – žádná pozice
            AL.SourcePlay(source);
            soundSources.Add(source);

            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(maxDurationSeconds));
                AL.SourceStop(source);
                AL.Source(source, ALSourcei.Buffer, 0);
                lock (soundSources)
                {
                    soundSources.Remove(source);
                }
                AL.DeleteSource(source);
            });
        }

        /// <summary>
        /// Sets the listener position and orientation for 3D sound spatialization.
        /// </summary>
        /// <param name="position">Listener world position.</param>
        /// <param name="forward">Listener forward direction vector.</param>
        /// <param name="up">Listener up direction vector.</param>
        public void SetListener(Vector3 position, Vector3 forward, Vector3 up)
        {
            AL.Listener(ALListener3f.Position, position.X, position.Y, position.Z);
            AL.Listener(ALListener3f.Velocity, 0, 0, 0);

            float[] orientation = new float[]
            {
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z
            };
            AL.Listener(ALListenerfv.Orientation, ref orientation[0]);
        }

        private int LoadBuffer(string path)
        {
            if (bufferCache.TryGetValue(path, out int buffer))
                return buffer;

            byte[] data = LoadWave(File.OpenRead(path), out int channels, out int bits, out int rate);
            ALFormat format = GetSoundFormat(channels, bits);

            buffer = AL.GenBuffer();

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                AL.BufferData(buffer, format, ptr, data.Length, rate);
            }
            finally
            {
                handle.Free();
            }

            bufferCache[path] = buffer;
            return buffer;
        }

        private byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            using var reader = new BinaryReader(stream);
            reader.ReadBytes(22); channels = reader.ReadInt16();
            rate = reader.ReadInt32();
            reader.ReadBytes(6); bits = reader.ReadInt16();
            reader.ReadBytes(8); int dataSize = reader.ReadInt32();
            return reader.ReadBytes(dataSize);
        }

        private ALFormat GetSoundFormat(int channels, int bits) => (channels, bits) switch
        {
            (1, 8) => ALFormat.Mono8,
            (1, 16) => ALFormat.Mono16,
            (2, 8) => ALFormat.Stereo8,
            (2, 16) => ALFormat.Stereo16,
            _ => throw new NotSupportedException("Unsupported WAV format."),
        };

        /// <summary>
        /// Releases OpenAL resources and deletes all audio sources and buffers.
        /// </summary>
        public void Dispose()
        {
            AL.DeleteSource(musicSource);
            foreach (var src in soundSources)
                AL.DeleteSource(src);
            foreach (var buffer in bufferCache.Values)
                AL.DeleteBuffer(buffer);

            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(context);
            ALC.CloseDevice(device);
        }
    }
}