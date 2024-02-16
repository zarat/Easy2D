using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy2D.Components
{
    public class Clickable : Component
    {

        public Clickable() { 
        
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

        public virtual void Click(Point position)
        {
            Console.WriteLine("Clickable Click");
        }

        public virtual void Release(Point position)
        {
            Console.WriteLine("Clickable Released");
        }

    }

}
