using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace MonoGameLibrary.Audio;

public class AudioController : IDisposable
{
	// Tracks sound effect instances created so they can be paused, unpaused, and/or disposed.
	private readonly List<SoundEffectInstance> _activeSoundEffectInstances = [];

	// Tracks the volume for song playback when muting and unmuting.
	private float _previousSongVolume;

	// Tracks the volume for sound effect playback when muting and unmuting.
	private float _previousSoundEffectVolume;

	/// <summary>
	/// Gets a value that indicates if audio is muted.
	/// </summary>
	public bool IsMuted { get; private set; }

	/// <summary>
	/// Gets or Sets the global volume of songs.
	/// </summary>
	/// <remarks>
	/// If IsMuted is true, the getter will always return back 0.0f and the
	/// setter will ignore setting the volume.
	/// </remarks>
	public float SongVolume
	{
		get
		{
			if (IsMuted)
			{
				return 0.0f;
			}

			return MediaPlayer.Volume;
		}
		set
		{
			if (IsMuted)
			{
				return;
			}

			MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
		}
	}

	/// <summary>
	/// Gets or Sets the global volume of sound effects.
	/// </summary>
	/// <remarks>
	/// If IsMuted is true, the getter will always return back 0.0f and the
	/// setter will ignore setting the volume.
	/// </remarks>
	public float SoundEffectVolume
	{
		get
		{
			if (IsMuted)
			{
				return 0.0f;
			}

			return SoundEffect.MasterVolume;
		}
		set
		{
			if (IsMuted)
			{
				return;
			}

			SoundEffect.MasterVolume = Math.Clamp(value, 0.0f, 1.0f);
		}
	}

	/// <summary>
	/// Gets a value that indicates if this audio controller has been disposed.
	/// </summary>
	public bool IsDisposed { get; private set; }

	// Finalizer called when object is collected by the garbage collector.
	~AudioController() => Dispose(false);

	/// <summary>
	/// Disposes of this audio controller and cleans up resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public void Dispose(bool disposing)
	{
		if (IsDisposed)
		{
			return;
		}

		if (disposing)
		{
			foreach (var instance in _activeSoundEffectInstances)
			{
				instance.Dispose();
			}

			_activeSoundEffectInstances.Clear();
		}

		IsDisposed = true;
	}

	/// <summary>
	/// Updates this audio controller.
	/// </summary>
	public void Update()
	{
		for (int i = _activeSoundEffectInstances.Count - 1; i >= 0; i--)
		{
			var instance = _activeSoundEffectInstances[i];
			if (instance.State == SoundState.Stopped)
			{
				if (!instance.IsDisposed)
				{
					instance.Dispose();
				}

				_activeSoundEffectInstances.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Plays the given sound effect.
	/// </summary>
	/// <param name="soundEffect">The sound effect to play.</param>
	/// <returns>The sound effect instance created by this method.</returns>
	public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect) =>
		PlaySoundEffect(soundEffect, 1.0f, 0.0f, 0.0f, false);

	/// <summary>
	/// Plays the given sound effect with the specified properties.
	/// </summary>
	/// <param name="soundEffect">The sound effect to play.</param>
	/// <param name="volume">The volume, ranging from 0.0 (silence) to 1.0 (full volume).</param>
	/// <param name="pitch">The pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).</param>
	/// <param name="pan">The panning, ranging from -1.0 (left speaker) to 0.0 (centered) to 1.0 (right speaker).</param>
	/// <param name="isLooped">Whether the sound effect should loop after playback.</param>
	/// <returns>The sound effect instance created by playing the sound effect.</returns>
	public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan, bool isLooped)
	{
		// Create an instance from the provided sound effect.
		var soundEffectInstance = soundEffect.CreateInstance();

		// Apply the volume, pitch, pan, and loop values specified.
		soundEffectInstance.Volume = volume;
		soundEffectInstance.Pitch = pitch;
		soundEffectInstance.Pan = pan;
		soundEffectInstance.IsLooped = isLooped;

		// Tell the instance to play.
		soundEffectInstance.Play();

		// Add it to the active instances for tracking.
		_activeSoundEffectInstances.Add(soundEffectInstance);

		return soundEffectInstance;
	}

	/// <summary>
	/// Plays the given song.
	/// </summary>
	/// <param name="song">The song to play.</param>
	/// <param name="isRepeating">Optionally, specify if the song should repeat. Defaults to true.</param>
	public static void PlaySong(Song song, bool isRepeating = true)
	{
		// Check if the media player is already playing, if so, stop it.
		// If we do not stop it, this could cause issues on some platforms.
		if (MediaPlayer.State == MediaState.Playing)
		{
			MediaPlayer.Stop();
		}

		MediaPlayer.Play(song);
		MediaPlayer.IsRepeating = isRepeating;
	}

	/// <summary>
	/// Pauses all audio.
	/// </summary>
	public void PauseAudio()
	{
		// Pause any actively playing songs.
		MediaPlayer.Pause();

		// Pause any active sound effects.
		foreach (var instance in _activeSoundEffectInstances)
		{
			instance.Pause();
		}
	}

	/// <summary>
	/// Resumes play of all previously paused audio.
	/// </summary>
	public void ResumeAudio()
	{
		// Resume paused music
		MediaPlayer.Resume();

		// Resume active sound effects.
		foreach (var instance in _activeSoundEffectInstances)
		{
			instance.Resume();
		}
	}

	/// <summary>
	/// Mutes all audio.
	/// </summary>
	public void MuteAudio()
	{
		// Save volumes so they can be restored during UnmuteAudio.
		_previousSongVolume = MediaPlayer.Volume;
		_previousSoundEffectVolume = SoundEffect.MasterVolume;

		// Set volumes to 0 to mute.
		MediaPlayer.Volume = 0.0f;
		SoundEffect.MasterVolume = 0.0f;

		IsMuted = true;
	}

	/// <summary>
	/// Unmutes all audio to the volume level prior to muting.
	/// </summary>
	public void UnmuteAudio()
	{
		// Restore previous volume levels.
		MediaPlayer.Volume = _previousSongVolume;
		SoundEffect.MasterVolume = _previousSoundEffectVolume;

		IsMuted = false;
	}

	/// <summary>
	/// Toggles the current audio mute state.
	/// </summary>
	public void ToggleMute()
	{
		if (IsMuted)
		{
			UnmuteAudio();
		}
		else
		{
			MuteAudio();
		}
	}
}