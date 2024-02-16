using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using Easy2D.Components;

namespace Easy2D
{

	public class PortListEditorWindow : Form
	{
		public new GameObject Parent;

		private object selectedPort;

		public List<Component> self;

		public ComboBox comboBox1 = new ComboBox();
		public ListBox listBox1 = new ListBox();
        public Button button1;
        public PropertyGrid propertyGrid1 = new PropertyGrid();

		public object SelectedPort
		{
			get
			{
				return selectedPort;
			}
		}

		public PortListEditorWindow(object value, GameObject parent)
		{

			InitializeComponent();

			Parent = parent;
			self = (List<Component>)value;

			button1.Click += RemoveButton_Click;

			foreach (Component c in self)
			{
				listBox1.Items.Add(c);
			}

            listBox1.SelectedIndexChanged += (sender, e) =>
            {
                propertyGrid1.SelectedObject = listBox1.SelectedItem;
            };

            comboBox1.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

			comboBox1.Items.Add("Add Component");



            // Gib den Namespace an, dessen Klassen du auflisten möchtest
            string namespaceString = "Easy2D.Components";

            /*
            // Rufe den Assembly ab, in dem der Namespace enthalten ist
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Finde alle Typen im Assembly, die im angegebenen Namespace enthalten sind
            var typesInNamespace = assembly.GetTypes()
                .Where(t => t.Namespace == namespaceString)
                .ToList();

            // Gib die gefundenen Klassennamen aus
            Console.WriteLine($"Klassen im Namespace '{namespaceString}':");
            foreach (var type in typesInNamespace)
            {
                Console.WriteLine(type.Name);
                comboBox1.Items.Add(type.Name);
            }
            */

            /*
            // Durchlaufe die gefundenen Typen und zeige abgeleitete Klassen an
            Console.WriteLine($"Abgeleitete Klassen:");
            foreach (var baseType in typesInNamespace)
            {
                Console.WriteLine($"Abgeleitet von {baseType.Name}:");
                foreach (var derivedType in typesInNamespace.Where(t => t.IsSubclassOf(baseType)))
                {
                    Console.WriteLine(derivedType.Name);
                    comboBox1.Items.Add(derivedType.Name);
                }
            }
            */

            
			comboBox1.Items.Add("Sprite");
			comboBox1.Items.Add("SpriteAnimation");
			comboBox1.Items.Add("Collider");
			comboBox1.Items.Add("RigidBody");
            comboBox1.Items.Add("AudioPlayer");
            comboBox1.Items.Add("Clickable");
            comboBox1.Items.Add("Hoverable");

            comboBox1.SelectedIndex = 0;
		}

		private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (comboBox1.SelectedItem.ToString())
			{
				case "Sprite":
					{
						Sprite sprite = new Sprite(null, 0, 0, Parent);
						self.Add(sprite);
						listBox1.Items.Add(sprite);
						break;
					}
				case "SpriteAnimation":
					{
						SpriteAnimation spriteAnimation = new SpriteAnimation(Parent, null, AnimationType.Vertical, 0, 0, 0, 0);
						self.Add(spriteAnimation);
						listBox1.Items.Add(spriteAnimation);
						break;
					}
				case "Collider":
					{
						Collider coll = new Collider();
						coll.Parent = Parent;
						self.Add(coll);
						listBox1.Items.Add(coll);
						break;
					}
				case "RigidBody":
					{
						RigidBody rb = new RigidBody(Parent, 1f, 9.8f);
						self.Add(rb);
						listBox1.Items.Add(rb);
						break;
					}
				case "AudioPlayer":
					{
						AudioPlayer ac = new AudioPlayer();
						ac.Parent = Parent;
						self.Add(ac);
						listBox1.Items.Add(ac);
						break;
					}
                case "Clickable":
                    {
                        Clickable ac = new Clickable();
                        ac.Parent = Parent;
                        self.Add(ac);
                        listBox1.Items.Add(ac);
                        break;
                    }
                case "Hoverable":
                    {
                        Hoverable ac = new Hoverable();
                        ac.Parent = Parent;
                        self.Add(ac);
                        listBox1.Items.Add(ac);
                        break;
                    }
            }
		}

		private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			propertyGrid1.SelectedObject = e;
		}

		private void RemoveButton_Click(object sender, EventArgs e)
		{
			Component component = (Component)propertyGrid1.SelectedObject;
			listBox1.Items.Remove(component);
			self.Remove(component);
			propertyGrid1.Refresh();
		}

        private void InitializeComponent()
        {
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 42);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(203, 420);
            this.listBox1.TabIndex = 0;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 13);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.Location = new System.Drawing.Point(221, 12);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(377, 454);
            this.propertyGrid1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(140, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Remove";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // PortListEditorWindow
            // 
            this.ClientSize = new System.Drawing.Size(610, 478);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.listBox1);
            this.Name = "PortListEditorWindow";
            this.ResumeLayout(false);

        }
    }
}
