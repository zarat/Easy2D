using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Design;

using Easy2D.Components;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.Data;

namespace Easy2D
{

    public class FileDialogEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        return openFileDialog.FileName;
                    }
                }
            }
            return value;
        }
    }

    public class GameObject : ICloneable
    {

		[Category("GameObject")]
		[Browsable(false)]
		public Transform Transform { get; set; }

        [Category("GameObject")]
        public string Name { get; set; } = "New GameObject";

        [Category("GameObject")]
        public GameObject Parent { get; set; }

        public bool isBeingDragged = false;


		public string[] importedScripts { get; set; } = new string[] { };

		public string[] scriptsToImport { get; set; } = new string[] { };

		// \toto scripts should only load at start
        public string[] _script { 
			get { 
				return scriptsToImport; 
			}
			set { 
				scriptsToImport = value;

				
				foreach(string script in scriptsToImport)
				{
					if(!importedScripts.Contains(script))
					{
						Console.WriteLine("Found a new Script");
						// execute
						object o = ScriptLoader.LoadAndInstantiate(Path.Combine(Manager.Instance.projectFolder, script), "", "");
						if(null != o)
						{
							global::Easy2D.Components.Component c = o as global::Easy2D.Components.Component;
							c.Parent = this;
							
							AddComponent(c);
							try
							{
								c.Init();
							}catch(Exception ex) { Console.WriteLine("[ERR] " + ex.Message); }
                            importedScripts.Append(script);
							Console.WriteLine("Imported the new script");
						}

					}
				}
				

			} 
		}

		/*
		private string scripts;

        [Category("GameObject")]
        [Editor(typeof(FileDialogEditor), typeof(UITypeEditor))]
        private string Scripts
        {
			get { return scripts; }
			set {
                object o = ScriptLoader.LoadAndInstantiate(value, "", "");
                if (null != o)
                {
                    global::Easy2D.Components.Component c = o as global::Easy2D.Components.Component;
                    c.Parent = this;
                        
                    AddComponent(c);
                    c.Init();
                }
            }
        }
		*/

        private int renderLayer = 0;
		private int previousRenderLayer = 0;
        [Category("GameObject")]
        public int RenderLayer { 
			get { return renderLayer; } 
			set { 
				previousRenderLayer = renderLayer; 
				renderLayer = value; 
			} 
		}

        [Category("GameObject")]
        [Browsable(true)]
		[Description("Select a value from the dropdown.")]
		[Editor(typeof(PortListEditor), typeof(UITypeEditor))]
		public List<global::Easy2D.Components.Component> Components { get; set; }

        [Category("Transform")]
        public float X
		{
			get
			{
				return Transform.LocalPosition.X;
			}
			set
			{
				Transform.LocalPosition = new PointF(value, Transform.LocalPosition.Y);
			}
		}

        [Category("Transform")]
        public float Y
		{
			get
			{
				return Transform.LocalPosition.Y;
			}
			set
			{
				Transform.LocalPosition = new PointF(Transform.LocalPosition.X, value);
			}
		}

        [Category("Transform")]
        public float Rotation
		{
			get
			{
				return Transform.LocalRotation;
			}
			set
			{
				Transform.LocalRotation = value;
			}
		}

		[Category("Transform")]
        public float GlobalX
        {
            get
            {
                return Transform.GlobalPosition.X;
            }
            set
            {
                Transform.GlobalPosition = new PointF(value, Transform.GlobalPosition.Y);
            }
        }
        [Category("Transform")]
        public float GlobalY
        {
            get
            {
				return Transform.GlobalPosition.Y;
            }
            set
            {
                Transform.GlobalPosition = new PointF(Transform.GlobalPosition.X, value);
            }
        }

        public bool disposable = false;
		public float lifeTime { get; set; } = 0f;

        public object Clone()
        {
            //First we create an instance of this specific type.
            object newObject = Activator.CreateInstance(this.GetType());
            return newObject;
        }

        public GameObject()
		{
			Transform = new Transform(this);
            Components = new List<Components.Component>();
		}

		public void AddChild(GameObject child)
		{
			if(null == child || child == this)
				return;

			child.Parent = this;
			Transform.AddChild(child.Transform);
		}

		public void RemoveChild(GameObject child)
		{
			if (null == child || child == this)
				return;

			child.Parent = null;
			Transform.RemoveChild(child.Transform);
		}

		public virtual void Update()
		{
			// update all the components
			// \todo children!
			foreach( global::Easy2D.Components.Component c in Components)
			{
				try
				{
					c.Update();
				}catch(Exception e) { }
			}
			Transform.UpdateGlobalTransform();
		}

		public virtual void FixedUpdate(float deltaTime)
		{
			if (disposable)
			{
				lifeTime -= deltaTime;

				if (lifeTime <= 0f)
				{
					GameObject t = Manager.Instance.renderer.Extract(this);
					t = null;
				}
			}

			// only in editor
			if (isBeingDragged)
				return;
			
			foreach (global::Easy2D.Components.Component c in Components)
				c.FixedUpdate(deltaTime);
		}

        public virtual void Draw(Graphics graphics)
		{
			foreach (global::Easy2D.Components.Component component in Components)
			{
				component.Draw(graphics);
			}
		}

		public void AddComponent(global::Easy2D.Components.Component component)
		{
			Components.Add(component);
		}

		public void RemoveComponent(global::Easy2D.Components.Component component)
		{
			Components.Remove(component);
		}

        public T GetComponent<T>() where T : global::Easy2D.Components.Component
        {
			foreach (global::Easy2D.Components.Component component in Components)
			{
				T typedComponent = component as T;
				if (typedComponent != null)
				{
					return typedComponent;
				}
			}
			return null;
		}

        public List<T> GetComponents<T>() where T : global::Easy2D.Components.Component
        {
			List<T> result = new List<T>();

            foreach (global::Easy2D.Components.Component component in Components)
            {
                T typedComponent = component as T;
                if (typedComponent != null)
                {
                    result.Add(typedComponent);
                }
            }
            return result;
        }

        public List<PointF> GetGlobalPoints()
		{
			List<PointF> globalPoints = new List<PointF>();
			foreach (global::Easy2D.Components.Component component in Components)
			{
                Mesh meshComponent = component as Mesh;
				if (meshComponent == null)
				{
					continue;
				}
				foreach (PointF localPoint in meshComponent.localPoints)
				{
					PointF scaledPoint = new PointF(localPoint.X * Transform.Scale, localPoint.Y * Transform.Scale);
					PointF globalPoint = TransformPoint(scaledPoint, Transform);
					globalPoints.Add(globalPoint);
				}
			}
			return globalPoints;
		}

		public PointF TransformPoint(PointF point, Transform transform)
		{
			PointF rotatedPoint = RotatePoint(point, PointF.Empty, transform.GlobalRotation);
			PointF globalPoint = new PointF(rotatedPoint.X + transform.GlobalPosition.X, rotatedPoint.Y + transform.GlobalPosition.Y);
			return globalPoint;
		}

		public PointF RotatePoint(PointF point, PointF origin, float angleInDegrees)
		{
			double angleInRadians = (double)angleInDegrees * Math.PI / 180.0;
			double cosTheta = Math.Cos(angleInRadians);
			double sinTheta = Math.Sin(angleInRadians);
			float x = (float)((double)(point.X - origin.X) * cosTheta - (double)(point.Y - origin.Y) * sinTheta + (double)origin.X);
			float y = (float)((double)(point.X - origin.X) * sinTheta + (double)(point.Y - origin.Y) * cosTheta + (double)origin.Y);
			return new PointF(x, y);
		}
	}
}
