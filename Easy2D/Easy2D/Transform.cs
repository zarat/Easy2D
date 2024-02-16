using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Easy2D
{

	public class Transform
	{
		private PointF localPosition;
        public PointF lastLocalPosition;
        public PointF LocalPosition { get { return localPosition; } set { lastLocalPosition = localPosition; localPosition = value; } }

		private PointF globalPosition;
		public PointF lastGlobalPosition;
		public PointF GlobalPosition { get { return globalPosition; } set { lastGlobalPosition = globalPosition; globalPosition = value; } }

        public float GlobalRotation;

		public float LocalRotation;

		public float Scale;

		public List<Transform> Children;

		public Transform Parent;

		public GameObject GameObject;

		public void Transform1()
		{
			LocalPosition = PointF.Empty;
			GlobalPosition = PointF.Empty;
			GlobalRotation = 0f;
			LocalRotation = 0f;
			Scale = 1f;
			Children = new List<Transform>();
		}

        public Transform(GameObject parent)
        {
			GameObject = parent;
            LocalPosition = PointF.Empty;
            GlobalPosition = PointF.Empty;
            GlobalRotation = 0f;
            LocalRotation = 0f;
            Scale = 1f;
            Children = new List<Transform>();
        }

        public void AddChild(Transform child)
		{
			child.Parent = this;
			Children.Add(child);
		}

		public void RemoveChild(Transform child)
		{
			child.Parent = null;
			Children.Remove(child);
		}

		public void UpdateGlobalTransform()
		{

			// If we have a parent transform, apply its position and rotation
			if (Parent != null)
			{
				GlobalRotation = LocalRotation + Parent.GlobalRotation;
				PointF rotatedPosition = RotatePoint(LocalPosition, PointF.Empty, Parent.GlobalRotation);
				GlobalPosition = new PointF(rotatedPosition.X + Parent.GlobalPosition.X, rotatedPosition.Y + Parent.GlobalPosition.Y);
			}
			else
			{
				GlobalPosition = LocalPosition;
				GlobalRotation = LocalRotation;
			}

			foreach (Transform child in Children)
			{
				child.UpdateGlobalTransform();
			}
		}

		public void Rotate(float angleInDegrees)
		{
			LocalRotation += angleInDegrees;
		}

		public void RotateChildren(float angleInDegrees)
		{
			foreach (Transform child in Children)
			{
				child.LocalRotation += angleInDegrees;
			}
		}

		public void MoveInDirection(float distance)
		{
			float angleInRadians = GlobalRotation * (float)Math.PI / 180f;
			float dx = distance * (float)Math.Cos(angleInRadians);
			float dy = distance * (float)Math.Sin(angleInRadians);
			Move(dx, dy);
		}

		public void ScaleBy(float factor)
		{
			Scale *= factor;
		}

		private void Move(float dx, float dy)
		{
			LocalPosition = new PointF(LocalPosition.X + dx, LocalPosition.Y + dy);
		}

		public void MoveForward(float distance)
		{
			float angleInRadians = GlobalRotation * (float)Math.PI / 180f;
			float dx = distance * (float)Math.Cos(angleInRadians);
			float dy = distance * (float)Math.Sin(angleInRadians);
			Move(dx, dy);
		}

		public void MoveBackward(float distance)
		{
			MoveForward(0f - distance);
		}

		public void MoveLeft(float distance)
		{
			float angleInRadians = GlobalRotation * (float)Math.PI / 180f;
			float dx = (0f - distance) * (float)Math.Sin(angleInRadians);
			float dy = distance * (float)Math.Cos(angleInRadians);
			Move(dx, dy);
		}

		public void MoveRight(float distance)
		{
			float angleInRadians = GlobalRotation * (float)Math.PI / 180f;
			float dx = distance * (float)Math.Sin(angleInRadians);
			float dy = (0f - distance) * (float)Math.Cos(angleInRadians);
			Move(dx, dy);
		}

		public void MoveTowardsMouse(PointF mousePosition, float speed)
		{
			float dx = mousePosition.X - GlobalPosition.X;
			float dy = mousePosition.Y - GlobalPosition.Y;
			float distance = (float)Math.Sqrt(dx * dx + dy * dy);
			if (distance > 0f)
			{
				float factor = speed / distance;
				dx *= factor;
				dy *= factor;
				Move(dx, dy);
			}
		}

		public void MoveForwardTowardsMouse(PointF mousePosition, float speed, Control form)
		{
			Point screenMousePosition = Cursor.Position;
			Point formMousePosition = form.PointToClient(screenMousePosition);
			int x = formMousePosition.X;
			int y = formMousePosition.Y;
			float dx = (float)x - GlobalPosition.X;
			float dy = (float)y - GlobalPosition.Y;
			if (Math.Abs(dx) < 2f || Math.Abs(dy) < 2f)
			{
				return;
			}
			LookAt(new PointF(x, y), form);
			float distance = (float)Math.Sqrt(dx * dx + dy * dy);
			if (distance > 0f)
			{
				PointF forwardVector = new PointF((float)Math.Cos((double)GlobalRotation * Math.PI / 180.0), (float)Math.Sin((double)GlobalRotation * Math.PI / 180.0));
				float dotProduct = forwardVector.X * dx + forwardVector.Y * dy;
				if (dotProduct > 0f)
				{
					float factor = speed / distance;
					dx *= factor;
					dy *= factor;
					Move(dx, dy);
				}
			}
		}

		public void LookAt(PointF mousePosition, Control form)
		{
			Point screenMousePosition = Cursor.Position;
			Point formMousePosition = form.PointToClient(screenMousePosition);
			int x = formMousePosition.X;
			int y = formMousePosition.Y;
			float dx = (float)x - GlobalPosition.X;
			float dy = (float)y - GlobalPosition.Y;
			float angleToMouse = (float)Math.Atan2(dy, dx) * (180f / (float)Math.PI);
			float angleDifference = angleToMouse - GlobalRotation;
			if (angleDifference > 180f)
			{
				angleDifference -= 360f;
			}
			else if (angleDifference < -180f)
			{
				angleDifference += 360f;
			}
			float maxRotationSpeed = 5f;
			float clampedAngleDifference = Math.Min(Math.Abs(angleDifference), maxRotationSpeed) * (float)Math.Sign(angleDifference);
			Rotate(clampedAngleDifference);
		}

		public void LookAtPoint(PointF target)
		{
			float dx = target.X - LocalPosition.X;
			float dy = target.Y - LocalPosition.Y;
			float angleToMouse = (float)Math.Atan2(dy, dx) * (180f / (float)Math.PI);
			float angleDifference = angleToMouse - GlobalRotation;
			if (angleDifference > 180f)
			{
				angleDifference -= 360f;
			}
			else if (angleDifference < -180f)
			{
				angleDifference += 360f;
			}
			float maxRotationSpeed = 5f;
			float clampedAngleDifference = Math.Min(Math.Abs(angleDifference), maxRotationSpeed) * (float)Math.Sign(angleDifference);
			Rotate(clampedAngleDifference);
		}

		private PointF RotatePoint(PointF point, PointF origin, float angleInDegrees)
		{
			double angleInRadians = (double)angleInDegrees * Math.PI / 180.0;
			double cosTheta = Math.Cos(angleInRadians);
			double sinTheta = Math.Sin(angleInRadians);
			float x = (float)((double)(point.X - origin.X) * cosTheta - (double)(point.Y - origin.Y) * sinTheta + (double)origin.X);
			float y = (float)((double)(point.X - origin.X) * sinTheta + (double)(point.Y - origin.Y) * cosTheta + (double)origin.Y);
			return new PointF(x, y);
		}

		public void MoveGlobal(float dx, float dy)
		{
			GlobalPosition = new PointF(GlobalPosition.X + dx, GlobalPosition.Y + dy);
		}

		public void RotateGlobal(float angleInDegrees)
		{
			GlobalRotation += angleInDegrees;
		}

		public void ScaleGlobal(float factor)
		{
			Scale *= factor;
		}

		public void RotateAroundPoint(PointF point, float angleInDegrees)
		{
			float dx = GlobalPosition.X - point.X;
			float dy = GlobalPosition.Y - point.Y;
			PointF rotatedPosition = RotatePoint(new PointF(dx, dy), PointF.Empty, angleInDegrees);
			GlobalPosition = new PointF(rotatedPosition.X + point.X, rotatedPosition.Y + point.Y);
			LocalPosition = RotatePoint(LocalPosition, PointF.Empty, angleInDegrees);
		}
	}
}
