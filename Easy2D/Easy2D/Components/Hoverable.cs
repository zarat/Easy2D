using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy2D.Components
{
    public class Hoverable : Component
    {

        public bool hovered = false;

        public Hoverable()
        {

        }

        public override void Draw(Graphics graphics)
        {
            // No need for drawing
        }

        public override void Update()
        {
            base.Update();

            // ...

        }

        public override void FixedUpdate(float deltaTime)
        {
            base.FixedUpdate(deltaTime);

            // ...

        }

        public virtual void Hover(Point position)
        {
            Console.WriteLine("Hoverable Hover");
            hovered = true;
        }

        public virtual void Left(Point position)
        {
            Console.WriteLine("Hoverable Left");
            hovered = false;
        }

    }
}
