using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Easy2D.Components
{

	public class Mesh : Component
	{

		public List<PointF> localPoints { get; set; }

		private GameObject gameObject;

		public Color color { get; set; }

		public string Name { get; set; }

		public Brush brush = Brushes.Transparent;
		public Pen pen = Pens.Black;

		public Mesh(List<PointF> Vertices, GameObject gameObject)
		{
			localPoints = Vertices;
			this.gameObject = gameObject;
		}

		public Mesh(GameObject gameObject)
		{
			List<PointF> list = new List<PointF>();
			list.Add(new PointF(-50f, -50f));
			list.Add(new PointF(-50f, 50f));
			list.Add(new PointF(50f, 50f));
			list.Add(new PointF(50f, -50f));
			localPoints = list;
			this.gameObject = gameObject;
		}

		public override void Draw(Graphics graphics)
		{

			if (localPoints.Count >= 3)
			{
				PointF[] globalPoints = new PointF[localPoints.Count];
				for (int i = 0; i < localPoints.Count; i++)
				{
					PointF scaledPoint = new PointF(localPoints[i].X * gameObject.Transform.Scale, localPoints[i].Y * gameObject.Transform.Scale);
					PointF rotatedPoint = RotatePoint(scaledPoint, PointF.Empty, gameObject.Transform.GlobalRotation);
					PointF globalPoint = new PointF(rotatedPoint.X + gameObject.Transform.GlobalPosition.X, rotatedPoint.Y + gameObject.Transform.GlobalPosition.Y);
					globalPoints[i] = globalPoint;
				}
				Brush _brush = new SolidBrush(color);
				graphics.FillPolygon(_brush, globalPoints.ToArray<System.Drawing.PointF>());
			}
		}

		private PointF RotatePoint(PointF point, PointF origin, float degree)
		{
			double radians = (double)degree * Math.PI / 180.0;
			double cosTheta = Math.Cos(radians);
			double sinTheta = Math.Sin(radians);
			float x = (float)((double)(point.X - origin.X) * cosTheta - (double)(point.Y - origin.Y) * sinTheta + (double)origin.X);
			float y = (float)((double)(point.X - origin.X) * sinTheta + (double)(point.Y - origin.Y) * cosTheta + (double)origin.Y);
			return new PointF(x, y);
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

    }
}
