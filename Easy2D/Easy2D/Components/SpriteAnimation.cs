using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;

namespace Easy2D.Components
{

    public enum AnimationType
    {
        Horizontal,
        Vertical,
        Matrix
    }

    public class SpriteAnimation : Component
	{

        public string imagePath { get; set; }
        private System.Drawing.Image image;

        [Browsable(true)]
        [Editor(typeof(SpriteAnimationImagePathEditor), typeof(UITypeEditor))]
        public System.Drawing.Image Image
        {
            get { return image; }
            set { image = value; }
        }

        [Browsable(false)]
        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }

        [Description("Ausrichtung der Animationen. Wenn die Animationen in den Spalten und die Frames in den Zeilen sind ist es eine horizontale Animation.")] 
        public AnimationType animationType { get; set; }

		[Description("Die Breite eines Frames (in Pixel).")]
        public int frameWidth { get; set; }

        [Description("Die Höhe eines Frames (in Pixel).")]
        public int frameHeight { get; set; }

        [Description("Manuelles setzen des Frame.")]
        public int currentFrame { get; set; }

        [Description("Anzahl der Frames der Animation.")]
        public int totalFrames { get; set; }

        [Description("Der Index der aktuell abgespielten Animation.")]
        public int currentAnimation { get; set; }

        public float timestamp = 0f;

		public bool running { get; set; }

        public float scaleX { get; set; }
        public float scaleY { get; set; }

        public SpriteAnimation(GameObject parent, Image image, AnimationType animationType, int frameWidth, int frameHeight, int totalFrames, int _currentAnimation)
		{
			Parent = parent;
			
			this.image = image;
			this.animationType = animationType;
			this.frameWidth = frameWidth;
			this.frameHeight = frameHeight;
			this.totalFrames = totalFrames;
			currentAnimation = _currentAnimation;
		}

		public SpriteAnimation()
		{
			running = false;
			scaleX = 1f;
			scaleY = 1f;
        }

		public void Start()
		{
			running = true;
		}

		public void Stop()
		{
			currentFrame = 0;
			running = false;
		}

        public override void Update()
        {
            base.Update();

        }

        public override void FixedUpdate(float deltaTime)
		{
			if (running)
			{
				timestamp += deltaTime;
				if (!(timestamp < 0.125f))
				{
					if(totalFrames != 0)
						currentFrame = (currentFrame + 1) % totalFrames;
					timestamp = 0f;
				}
			}
		}

		public override void Draw(Graphics g)
		{
            if (image != null)
            {
                System.Drawing.RectangleF sourceRect = System.Drawing.RectangleF.Empty;
                System.Drawing.RectangleF destRect = System.Drawing.RectangleF.Empty;

				if (animationType == AnimationType.Horizontal)
				{
					sourceRect = new System.Drawing.RectangleF(currentFrame * frameWidth, currentAnimation * frameHeight, frameWidth, frameHeight);
					destRect = new System.Drawing.RectangleF( Parent.Transform.GlobalPosition.X - frameWidth / 2 * scaleX, Parent.Transform.GlobalPosition.Y - frameHeight / 2 * scaleY, frameWidth * scaleX, frameHeight * scaleY);
				}
				else if (animationType == AnimationType.Vertical)
				{
					sourceRect = new System.Drawing.RectangleF(currentAnimation * frameWidth, currentFrame * frameHeight, frameWidth, frameHeight);
					destRect = new System.Drawing.RectangleF(Parent.Transform.GlobalPosition.X - frameWidth / 2 * scaleX, Parent.Transform.GlobalPosition.Y - frameHeight / 2 * scaleY, frameWidth * scaleX, frameHeight * scaleY);
				}
				else if (animationType == AnimationType.Matrix)
                {
                    int framesPerRow = image.Width / frameWidth;
                    int row = currentFrame / framesPerRow;
                    int column = currentFrame % framesPerRow;
                    sourceRect = new RectangleF(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
					destRect = new RectangleF(Parent.Transform.GlobalPosition.X - frameWidth / 2 * scaleX, Parent.Transform.GlobalPosition.Y - frameHeight / 2 * scaleY, frameWidth * scaleX, frameHeight * scaleY);
                }

                Matrix originalMatrix = g.Transform.Clone();

                PointF rotationCenter = new PointF(destRect.X + destRect.Width / 2, destRect.Y + destRect.Height / 2);

                g.TranslateTransform(rotationCenter.X, rotationCenter.Y);
                g.RotateTransform(Parent.Transform.GlobalRotation); // \todo configure global/local rotation
                g.TranslateTransform(-rotationCenter.X, -rotationCenter.Y);

                g.DrawImage(image, destRect, sourceRect, System.Drawing.GraphicsUnit.Pixel);

                g.Transform = originalMatrix;
            }
        }

		public void ChangeAnimation(int newAnimation)
		{
			if (currentAnimation != newAnimation)
			{
				currentAnimation = newAnimation;
				currentFrame = 0;
			}
		}

	}

}
