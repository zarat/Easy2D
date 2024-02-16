using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Easy2D.Components
{

	public class Collider : Component
	{

		public bool Colliding { get; set; }

		public PointF lastPosition;

		public Collider(GameObject parent)
		{
			Parent = parent;
		}

		public Collider()
		{

			Colliding = false;
		}

		public override void Update()
		{
			base.Update();

		}

        public override void FixedUpdate(float deltaTime)
        {

            HandleCollision();

            base.FixedUpdate(deltaTime);

        }


        private void HandleCollision()
        {
            if (Colliding)
            {
				Parent.Transform.LocalPosition = lastPosition;
     
				Colliding = false;
            }
            else
            {
                lastPosition = Parent.Transform.LocalPosition;
            }
        }

        public override void Draw(Graphics graphics)
		{
		}

		public virtual void OnCollision(GameObject other)
        {
            Colliding = true;
        }

	}
}
