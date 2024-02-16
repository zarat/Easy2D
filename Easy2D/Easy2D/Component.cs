using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Easy2D.Components
{

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class Component
	{
		public GameObject Parent;

		public string Name
		{
			get
			{
				return GetType().ToString();
			}
		}

		public Component(GameObject parent)
		{
			Parent = parent;
		}

		public Component()
		{
			Parent = null;
		}

		public virtual void Init() { 
		
		}

		public abstract void Draw(Graphics graphics);

		public virtual void Update()
		{
		}

		public virtual void Update(float deltaTime)
		{
		}

        public virtual void FixedUpdate(float deltaTime)
        {
        }
    }
}
