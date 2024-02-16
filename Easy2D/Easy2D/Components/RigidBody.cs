using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Easy2D.Components
{

    public class RigidBody : Component
	{

		public float Mass { get; set; }

		[Description("Vektor der die Richtung und das Tempo angibt, mit dem sich das Objekt bewegt.")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF Velocity { get; set; }

        [Description("Vektor der angibt, wie schnell sich die Geschwindigkeit eines Objekts ändert.")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF Acceleration { get; set; }

        [Description("Vektor der angibt, wie schnell sich die Geschwindigkeit eines Objekts maximal ändern darf.")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF MaxAcceleration { get; set; }  = new PointF(100f, 100f);

		[Description("Die Schwerkraft.")]
        public float Gravity { get; set; }

		[Description("Gibt an ob der RigidBody von äußeren Kräften beeinflusst wird.")]
        public bool IsKinematic { get; set; }

        private bool block = false;

		private float JumpStrength;

		public bool IsGrounded { get; set; }

        private bool IsJumping = false;

		private float InitialJumpSpeed = 20f;

		private float JumpSpeed = 5f;

		private float JumpHeight = 100f;

		private PointF lastPosition;

		

		public RigidBody(GameObject parent, float mass, float gravity, bool isKinematic = false)
		{
			Mass = mass;
			Velocity = PointF.Empty;
			Acceleration = PointF.Empty;
			Gravity = gravity;
			IsKinematic = isKinematic;
			Parent = parent;
			JumpStrength = 10f;
			IsGrounded = false;
		}

		public RigidBody()
		{
            Mass = 1f;
            Velocity = PointF.Empty;
            Acceleration = PointF.Empty;
            Gravity = 8.9f;
            IsKinematic = false;
            JumpStrength = 10f;
            IsGrounded = false;
        }

		public override void Draw(Graphics graphics)
		{
			// draw a line into moving direction
			PointF direction = new PointF(Velocity.X + Acceleration.X, Velocity.Y + Acceleration.Y);
			float length = 200f;
			float magnitude = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
			PointF normalizedDirection = new PointF(direction.X / magnitude, direction.Y / magnitude);
			PointF endPoint = new PointF(Parent.Transform.LocalPosition.X + normalizedDirection.X * length, Parent.Transform.LocalPosition.Y + normalizedDirection.Y * length);
		}

        public override void Update()
        {

            

        }

        public override void FixedUpdate(float deltaTime)
		{

			if (IsJumping)
			{
				Velocity = new PointF(Velocity.X, Velocity.Y + 100f * Gravity * deltaTime);
				Console.WriteLine("Accel: " + Velocity.ToString());
				if (Velocity.Y <= -100f)
				{
					IsJumping = false;
				}
			}
			if (!IsGrounded)
			{
				if (Acceleration.X == 0f)
				{
					Velocity = new PointF(Velocity.X, Velocity.Y + Acceleration.Y * Gravity * deltaTime);
				}
				else if (Acceleration.Y == 0f)
				{
					Velocity = new PointF(Velocity.X + Acceleration.X * deltaTime, Velocity.Y);
				}
				else
				{
					Velocity = new PointF(Velocity.X + Acceleration.X * Gravity * deltaTime, Velocity.Y + Acceleration.Y * Gravity * deltaTime);
				}

				if (Parent != null)
				{
					PointF newPosition = new PointF(Parent.Transform.LocalPosition.X + Velocity.X * deltaTime, Parent.Transform.LocalPosition.Y + Velocity.Y * deltaTime);
					Parent.Transform.LocalPosition = newPosition;
				}

			}

		}

		// Example
        private void HandleInput()
        {
            float deltaX = 0, deltaY = 0;
			float speed = 100;

            if (Easy2D.Input.Down(System.Windows.Forms.Keys.W))
                deltaY = -speed;
            if (Easy2D.Input.Down(System.Windows.Forms.Keys.S))
                deltaY = speed;
            if (Easy2D.Input.Down(System.Windows.Forms.Keys.A))
                deltaX = -speed;
            if (Easy2D.Input.Down(System.Windows.Forms.Keys.D))
                deltaX = speed;


            // Steuerung mit Rotation
            double angleRadians = Math.PI * Parent.Transform.LocalRotation / 180.0;
            float cosTheta = (float)Math.Cos(angleRadians);
            float sinTheta = (float)Math.Sin(angleRadians);
			Velocity = new System.Drawing.PointF(deltaX * cosTheta - deltaY * sinTheta, deltaX * sinTheta + deltaY * cosTheta);

			// Nur Top/Down Steuerung
            //Velocity = new System.Drawing.PointF(deltaX, deltaY);

            if (Easy2D.Input.Down(System.Windows.Forms.Keys.Right))
                Parent.Transform.LocalRotation += 5f;
            if (Easy2D.Input.Down(System.Windows.Forms.Keys.Left))
                Parent.Transform.LocalRotation -= 5f;


        }

        private Dictionary<GameObject, PointF> Collisions;

		public virtual void OnCollision(GameObject other)
		{

			
		}


		public void Jump()
		{
			if (IsGrounded)
			{
				JumpSpeed = InitialJumpSpeed;
				Acceleration = new PointF(Acceleration.X, (0f - Gravity) * JumpSpeed);
				IsJumping = true;
			}
		}
	}
}
