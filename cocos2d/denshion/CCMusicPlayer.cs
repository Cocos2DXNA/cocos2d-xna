using System;
using Microsoft.Xna.Framework.Media;
using MediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
#if WINDOWS_PHONE8
using Microsoft.Phone.Shell;
#endif
using Cocos2D;

namespace CocosDenshion
{
    /// <summary>
    /// This interface controls the media player on the device. For Microsoft mobile devices
    /// that play music, e.g. Zune and phone, you must not intefere with the background music
    /// unless the user has allowed it.
    /// </summary>
    public class CCMusicPlayer
    {
        public static ulong s_mciError;

        private bool m_IsRepeatingAfterClose;
        private bool m_IsShuffleAfterClose;
        private TimeSpan m_PlayPositionAfterClose = TimeSpan.Zero;
        //private MediaQueue m_QueueAfterClose;
        private Song m_SongToPlayAfterClose;
        private float m_VolumeAfterClose = 1f;

        /// <summary>
        /// Track if we did play our own game song, otherwise the media player is owned
        /// by the user of the device and that user is listening to background music.
        /// </summary>
        private bool m_didPlayGameSong;

        private Song m_music;
        private int m_nSoundId;

        public CCMusicPlayer()
        {
            m_nSoundId = 0;
            if (Microsoft.Xna.Framework.Media.MediaPlayer.State == MediaState.Playing)
            {
                SaveMediaState();
            }
        }

        public float Volume
        {
            get { 
                return Microsoft.Xna.Framework.Media.MediaPlayer.Volume; 
            }

            set
            {
                if (value >= 0.0f && value <= 1.0f)
                {
                    Microsoft.Xna.Framework.Media.MediaPlayer.Volume = value;
                }
            }
        }

        public int SoundID
        {
            get { return m_nSoundId; }
        }

        public void SaveMediaState()
        {
            try
            {
                // User is playing a song, so remember the song state.
                m_SongToPlayAfterClose = Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong;
                m_VolumeAfterClose = Microsoft.Xna.Framework.Media.MediaPlayer.Volume;
#if !NETFX_CORE
                m_PlayPositionAfterClose = Microsoft.Xna.Framework.Media.MediaPlayer.PlayPosition;
#endif
                m_IsRepeatingAfterClose = Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating;
                m_IsShuffleAfterClose = Microsoft.Xna.Framework.Media.MediaPlayer.IsShuffled;
            }
            catch (Exception ex)
            {
                CCLog.Log("Failed to save the media state of the game.");
                CCLog.Log(ex.ToString());
            }
        }

        public void RestoreMediaState()
        {
            if (m_SongToPlayAfterClose != null && m_didPlayGameSong)
            {
                try
                {
                    Microsoft.Xna.Framework.Media.MediaPlayer.IsShuffled = m_IsShuffleAfterClose;
                    Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating = m_IsRepeatingAfterClose;
                    Microsoft.Xna.Framework.Media.MediaPlayer.Volume = m_VolumeAfterClose;
                    Microsoft.Xna.Framework.Media.MediaPlayer.Play(m_SongToPlayAfterClose);
            }
                catch (Exception ex)
                {
                    CCLog.Log("Failed to restore the media state of the game.");
                    CCLog.Log(ex.ToString());
                }
        }
        }

        ~CCMusicPlayer()
        {
            Close();
            try
            {
            RestoreMediaState();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public void Open(string pFileName, int uId)
        {
            if (string.IsNullOrEmpty(pFileName))
            {
                return;
            }

            Close();

            m_music = CCContentManager.SharedContentManager.Load<Song>(pFileName);

            m_nSoundId = uId;
        }

        public void Play(bool bLoop)
        {
            if (null != m_music)
            {
                Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating = bLoop;
                Microsoft.Xna.Framework.Media.MediaPlayer.Play(m_music);
                m_didPlayGameSong = true;
            }
        }

        public void Play()
        {
            Play(false);
        }

        public void Close()
        {
            if (IsPlaying() && m_didPlayGameSong)
            {
                Stop();
            }
            m_music = null;
        }

        /// <summary>
        /// Pauses the current song being played. 
        /// </summary>
        public void Pause()
        {
            Microsoft.Xna.Framework.Media.MediaPlayer.Pause();
        }

        /// <summary>
        /// Resumes playback of the current song.
        /// </summary>
        public void Resume()
        {
            Microsoft.Xna.Framework.Media.MediaPlayer.Resume();
        }

        /// <summary>
        /// Stops playback of the current song and resets the playback position to zero.
        /// </summary>
        public void Stop()
        {
            Microsoft.Xna.Framework.Media.MediaPlayer.Stop();
        }

        /// <summary>
        /// resets the playback of the current song to its beginning.
        /// </summary>
        public void Rewind()
        {
            Song s = Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong;

            Stop();

            if (null != m_music)
            {
                Microsoft.Xna.Framework.Media.MediaPlayer.Play(m_music);
            }
            else if (s != null)
            {
                Microsoft.Xna.Framework.Media.MediaPlayer.Play(s);
            }
        }

        /// <summary>
        /// Returns true if any song is playing in the media player, even if it is not one
        /// of the songs in the game.
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            if (MediaState.Playing == Microsoft.Xna.Framework.Media.MediaPlayer.State)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if one of the game songs is playing.
        /// </summary>
        /// <returns></returns>
        public bool IsPlayingMySong()
        {
            if (!m_didPlayGameSong)
            {
                return (false);
            }
            if (MediaState.Playing == Microsoft.Xna.Framework.Media.MediaPlayer.State)
            {
                return true;
            }

            return false;
        }
    }
}