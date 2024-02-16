using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Easy2D.Components
{

	public class Sprite : Component
	{

        public string imagePath { get; set; }
        private System.Drawing.Image image;

        [Browsable(true)]
        [Editor(typeof(SpriteImagePathEditor), typeof(UITypeEditor))]
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

        [Description("The width of the displayed Image.")]
        public int width { get; set; }

        [Description("The hight of the displayed Image.")]
        public int height { get; set; }

        [Description("Translate the image on the X-Axis.")]
        public float offsetX { get; set; } = 0f;

        [Description("Translate the image on the Y-Axis.")]
        public float offsetY { get; set; } = 0f;

        [Description("Rotate the image relative to its parent.")]
        public float rotation { get; set; } = 0f;

		public Sprite(System.Drawing.Image image, int width, int height, GameObject parent)
		{
			Parent = parent;
			this.height = height;
			this.width = width;
			this.image = image;
		}

        public Sprite()
        {
            this.height = 0;
            this.width = 0;
        }

		public override void Draw(Graphics g)
        {
			if (image != null)
			{

                Matrix originalMatrix = g.Transform.Clone();

                PointF rotationCenter = new PointF(Parent.Transform.GlobalPosition.X, Parent.Transform.GlobalPosition.Y);

                g.TranslateTransform(rotationCenter.X, rotationCenter.Y);
                g.RotateTransform(Parent.Transform.GlobalRotation + rotation);
                g.TranslateTransform(-rotationCenter.X, -rotationCenter.Y);

                g.DrawImage(image, Parent.Transform.GlobalPosition.X + offsetX, Parent.Transform.GlobalPosition.Y + offsetY, width, height);

                g.Transform = originalMatrix;

            }
        }

        public override void Update()
        {
            base.Update();
            
        }

        public override void FixedUpdate(float deltaTime)
        {
            base.FixedUpdate(deltaTime);

        }

    }
}
