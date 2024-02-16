using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Easy2D
{

	public class Renderer
	{
		public Control Screen;

		public Graphics g;

		public Dictionary<GameObject, int> objects = new Dictionary<GameObject, int>();

		private object lockObject = 1;

		public Color clearColor = Color.White;

		public Renderer(Control screen)
		{
			Screen = screen;
			g = Screen.CreateGraphics();
		}

		public Renderer(Graphics ctx)
		{
			g = ctx;
		}

		public void AddObject(GameObject newObject)
		{
			lock (lockObject)
			{
				try
				{
					objects.Add(newObject, newObject.RenderLayer);
				} catch(System.Exception e) { }
			}
		}

		public void RemoveObject(GameObject gameObject)
		{
			lock (lockObject)
			{
				objects.Remove(gameObject);
			}
		}

		public void Render(bool clear = true)
		{
			if (clear)
			{
				g.Clear(clearColor);
			}
			lock (lockObject)
			{
                //Dictionary<GameObject, int> sortedDictionary = objects.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                Dictionary<GameObject, int> sortedDictionary = objects.OrderBy(x => x.Key.RenderLayer).ToDictionary(x => x.Key, x => x.Value);

                foreach (KeyValuePair<GameObject, int> item in sortedDictionary)
				{
					item.Key.Draw(g);
				}
			}
		}

		public Graphics GetGraphics()
		{
			return g;
		}

		public GameObject Extract(string name)
		{
			lock (lockObject)
			{
				foreach (KeyValuePair<GameObject, int> pair in objects)
				{
					if (pair.Key.Name == name)
					{
                        GameObject result = pair.Key;
						objects.Remove(pair.Key);
						return result;
					}
				}
			}

			return default;
		}

        public GameObject Extract(GameObject g)
        {
            lock (lockObject)
            {
                foreach (KeyValuePair<GameObject, int> pair in objects)
                {
                    if (pair.Key == g)
                    {
                        GameObject result = pair.Key;
                        objects.Remove(pair.Key);
                        return result;
                    }
                }
            }

            return default;
        }

    }
}
