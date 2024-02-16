using System;
using System.Drawing;

namespace Easy2D.Components
{
    public class AudioPlayer : Easy2D.Components.Component
    {

        System.Media.SoundPlayer player;

        public string audio { get; set; }

        public bool loop { get; set; }

        public AudioPlayer()
        {
            player = new System.Media.SoundPlayer();
            loop = false;
        }

        public void Start()
        {
            if (String.IsNullOrEmpty(audio) || null == player)
                return;

            if (!System.IO.File.Exists(audio))
                return;

            player = new System.Media.SoundPlayer(audio);
            player.Load();
            player.Play();
        }

        public void Loop()
        {
            if (String.IsNullOrEmpty(audio) || null == player)
                return;

            if (!System.IO.File.Exists(audio))
                return;

            player = new System.Media.SoundPlayer(audio);
            player.Load();
            player.PlayLooping();
        }

        public void Stop()
        {
            if (null == player)
                return;

            player.Stop();
        }

        public override void Draw(Graphics graphics)
        {

        }
    
    }

}
