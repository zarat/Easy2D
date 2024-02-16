using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Drawing.Design;


using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Policy;


using Easy2D;
using Easy2D.Components;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

namespace E2D
{

    public partial class Form1 : Form
    {


        #region Member


        public List<GameObject> gameObjects = new List<GameObject>();

        public Timer timer;

        private GameObject objectToDrag;
        private PointF lastMousePosition; 

        public Renderer renderer;

        private Bitmap imageBitmap;

        private Easy2D.Manager manager = Easy2D.Manager.GetInstance();

        public bool running = false;

        public string projectFolder = String.Empty;

        #endregion


        #region Initializer

        public Form1()
        {
            InitializeComponent();

            InitializeMenu();

            //InitializeGame();

        }

        public void InitializeMenu()
        {
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");

            ToolStripMenuItem fileMenu_New = new ToolStripMenuItem("New");
            fileMenu_New.Click += fileMenu_New_Click;
            fileMenu.DropDownItems.Add(fileMenu_New);

            ToolStripMenuItem fileMenu_Open = new ToolStripMenuItem("Open");
            fileMenu_Open.Click += fileMenu_Open_Click;
            fileMenu.DropDownItems.Add(fileMenu_Open);

            ToolStripMenuItem fileMenu_Save = new ToolStripMenuItem("Save");
            fileMenu_Save.Click += fileMenu_Save_Click;
            fileMenu.DropDownItems.Add(fileMenu_Save);

            menuStrip1.Items.Add(fileMenu);
        }

        public void InitializeTreeview()
        {
            treeView1.AllowDrop = true;
            treeView1.DragEnter += TreeView1_DragEnter;
            treeView1.ItemDrag += TreeView1_ItemDrag;
            treeView1.DragDrop += TreeView1_DragDrop;
        }
        
        public void InitializeGame() { 

            imageBitmap = new Bitmap(640, 480);
            pictureBox1.Size = imageBitmap.Size;
            pictureBox1.Image = imageBitmap;

            manager.renderer = new Renderer(Graphics.FromImage(imageBitmap));
            renderer = manager.renderer;
            renderer.clearColor = Color.White;

            treeView1.AfterSelect += (sender, e) =>
            {
                GameObject selectedGameObject = (GameObject)e.Node.Tag;
                if (selectedGameObject != null)
                {
                    propertyGrid1.SelectedObject = selectedGameObject;
                }
            };

            propertyGrid1.PropertyValueChanged += (sender, e) =>
            {
                if (e.ChangedItem != null)
                {
                    string propertyName = e.ChangedItem.Label;
                    if(propertyName == "RenderLayer")
                    {
                        GameObject g = manager.renderer.Extract(((GameObject)propertyGrid1.SelectedObject).Name);
                        g.RenderLayer = (int)e.ChangedItem.Value;manager.renderer.AddObject(g);
                    }
                }
                pictureBox1.Focus();
            };

            propertyGrid1.SelectedGridItemChanged += (sender, e) =>
            {
                
                pictureBox1.Focus();
            };

            // Subscribe to mouse events
            pictureBox1.MouseDown += GameForm_MouseDown;
            pictureBox1.MouseMove += GameForm_MouseMove;
            pictureBox1.MouseUp += GameForm_MouseUp;

            running = true;

            // Start game loop
            timer = new Timer();
            timer.Interval = 1000 / 60;
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        #endregion


        #region Menu Events

        private void fileMenu_New_Click(object sender, EventArgs e)
        {

            AskForProjectFolder();

            if(String.IsNullOrEmpty(projectFolder))
            {
                return;
            }

            manager.projectFolder = projectFolder;

            imageBitmap = new Bitmap(640, 480);
            pictureBox1.Size = imageBitmap.Size;
            pictureBox1.Image = imageBitmap;

            Graphics gr = Graphics.FromImage(imageBitmap);
            gr.SmoothingMode = SmoothingMode.AntiAlias;

            manager.renderer = new Renderer(gr);
            renderer = manager.renderer;
            renderer.clearColor = Color.White;

            if (null != gameObjects)
                gameObjects.Clear();
            else
                gameObjects = new List<GameObject>();

            InitializeTreeview();

            treeView1.AfterSelect += (s, el) =>
            {
                GameObject selectedGameObject = (GameObject)el.Node.Tag;
                if (selectedGameObject != null)
                {
                    propertyGrid1.SelectedObject = selectedGameObject;
                }
            };

            propertyGrid1.PropertyValueChanged += (s, el) =>
            {
                if (el.ChangedItem != null)
                {
                    string propertyName = el.ChangedItem.Label;
                    if (propertyName == "RenderLayer")
                    {
                        GameObject g = manager.renderer.Extract(((GameObject)propertyGrid1.SelectedObject).Name);
                        g.RenderLayer = (int)el.ChangedItem.Value; manager.renderer.AddObject(g);
                    }
                }
            };

            UpdateTreeview();

            // Subscribe to mouse events
            pictureBox1.MouseDown += GameForm_MouseDown;
            pictureBox1.MouseMove += GameForm_MouseMove;
            pictureBox1.MouseUp += GameForm_MouseUp;

            running = true;

            if(null != timer)
            {
                timer.Stop();
            }

            // Start game loop
            timer = new Timer();
            timer.Interval = 1000 / 30;
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        private void fileMenu_Open_Click(object sender, EventArgs e)
        {
            
            string selectedFilePath = "";

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML Dateien (*.xml)|*.xml|Alle Dateien (*.*)|*.*"; // Filter für bestimmte Dateitypen

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    selectedFilePath = openFileDialog.FileName;
                }
            }

            if (String.IsNullOrEmpty(selectedFilePath))
                return;

            projectFolder = Path.GetDirectoryName(selectedFilePath);

            Manager.Instance.projectFolder = projectFolder;

            gameObjects = Manager.Instance.LoadScene(selectedFilePath, true);

            if (null == gameObjects)
                return;

            imageBitmap = new Bitmap(640, 480);
            pictureBox1.Size = imageBitmap.Size;
            pictureBox1.Image = imageBitmap;

            manager.renderer = new Renderer(Graphics.FromImage(imageBitmap));
            renderer = manager.renderer;
            renderer.clearColor = Color.White;

            foreach (GameObject gameObject in gameObjects)
            {
                try
                {
                    renderer.AddObject(gameObject);
                } catch(Exception ex) { }
            }

            InitializeTreeview();

            treeView1.AfterSelect += (s, el) =>
            {
                GameObject selectedGameObject = (GameObject)el.Node.Tag;
                if (selectedGameObject != null)
                {
                    propertyGrid1.SelectedObject = selectedGameObject;
                }
            };

            propertyGrid1.PropertyValueChanged += (s, el) =>
            {
                if (el.ChangedItem != null)
                {
                    string propertyName = el.ChangedItem.Label;
                    if (propertyName == "RenderLayer")
                    {
                        GameObject g = manager.renderer.Extract(((GameObject)propertyGrid1.SelectedObject).Name);
                        if (null != g && null != el.ChangedItem.Value)
                        {
                            int newRenderLayer = (int)el.ChangedItem.Value;
                            g.RenderLayer = newRenderLayer;
                            manager.renderer.AddObject(g);
                        }
                    }
                }
            };

            UpdateTreeview();

            // Subscribe to mouse events
            pictureBox1.MouseDown += GameForm_MouseDown;
            pictureBox1.MouseMove += GameForm_MouseMove;
            pictureBox1.MouseUp += GameForm_MouseUp;

            running = true;

            if(null != timer)
            {
                timer.Stop();

            }
            // Start game loop
            timer = new Timer();
            timer.Interval = 1000 / 30;
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        private void fileMenu_Save_Click(object sender, EventArgs e)
        {

            string selectedFilePath = "";

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "XML Dateien (*.xml)|*.xml|Alle Dateien (*.*)|*.*"; // Filter für bestimmte Dateitypen

                DialogResult result = saveFileDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                {
                    selectedFilePath = saveFileDialog.FileName;
                }
            }

            if (String.IsNullOrEmpty(selectedFilePath))
                return;

            Manager.Instance.SaveScene(selectedFilePath);

        }

        #endregion


        #region Event Handler

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {

            UpdateTreeview();


            // rdit, remove, drag
            GameObject toEdit = null;
            GameObject toRemove = null;
            bool hit = false;
            foreach (GameObject g in gameObjects)
            {
                // \todo handle renderlayer!
                if (IsPointInPolygon(e.Location, g.GetGlobalPoints().ToArray()))
                {
                    if (null != g.GetComponent<Clickable>())
                    {
                        try
                        {
                            g.GetComponent<Clickable>().Click(e.Location);
                        } catch(Exception ex) { Console.WriteLine("Ex: " + ex.Message); }

                    }
                    if (Input.Pressed(Keys.E))
                        toEdit = g;

                    else if (Input.Pressed(Keys.D))
                    {
                        GameObject tmp = renderer.Extract(g.Name);
                        toRemove = g;
                        tmp = null;
                    }
                    else 
                        objectToDrag = g;

                    propertyGrid1.SelectedObject = g;

                    hit = true;

                    TreeNodeCollection nodes = treeView1.Nodes;
                    foreach (TreeNode node in nodes)
                    {
                        if (node.Text == g.Name)
                        {
                            treeView1.SelectedNode = node;
                            treeView1.Select();
                        }
                    }

                    lastMousePosition = e.Location;
                }
            }

            if (null != toRemove)
            {
                gameObjects.Remove(toRemove);
                toRemove = null;
            }

            if (!hit)
            {
                propertyGrid1.SelectedObject = null;
            }

            if (objectToDrag == null && !hit && Input.Pressed(Keys.N))
            {
                GameObject gameObject = new GameObject();
                gameObject.Name = "Neues GameObject " + gameObjects.Count;
                gameObject.Transform.LocalPosition = e.Location;
                gameObject.Update();
                Mesh gameObjectMesh = new Mesh(gameObject);
                gameObject.AddComponent(gameObjectMesh);
                TreeNode it = new TreeNode(gameObject.Name);
                it.Tag = gameObject;
                treeView1.Nodes.Add(it);
                gameObjects.Add(gameObject);
                renderer.AddObject(gameObject);
            }

            if (toEdit != null)
            {
                NodeExample n = new NodeExample(toEdit);
                MainForm m = new MainForm(n);
                m.ShowDialog();
            }

        }

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {

            // Wenn ein GameObject gezogen wird
            if (objectToDrag != null)
            {
                float dx = e.X - lastMousePosition.X;
                float dy = e.Y - lastMousePosition.Y;

                if (objectToDrag.Parent == null)
                {
                    objectToDrag.Transform.LocalPosition = new PointF(
                        objectToDrag.Transform.LocalPosition.X + dx,
                        objectToDrag.Transform.LocalPosition.Y + dy
                    );
                    objectToDrag.Update();
                }

                lastMousePosition = e.Location;

                Invalidate();
            }

            // hoverable items
            foreach (GameObject g in gameObjects)
            {

                if (IsPointInPolygon(e.Location, g.GetGlobalPoints().ToArray()))
                {
                    if (null != g.GetComponent<Hoverable>())
                    {
                        if(!g.GetComponent<Hoverable>().hovered)
                            g.GetComponent<Hoverable>().Hover(e.Location);
                    }
                }
                else
                {

                    if (null != g.GetComponent<Hoverable>())
                    {
                        if (g.GetComponent<Hoverable>().hovered)
                            g.GetComponent<Hoverable>().Left(e.Location);
                    }

                }
            }

        }

        private void GameForm_MouseUp(object sender, MouseEventArgs e)
        {

            foreach (GameObject g in gameObjects)
            {
                // \todo handle renderlayer!
                if (IsPointInPolygon(e.Location, g.GetGlobalPoints().ToArray()))
                {
                    if (null != g.GetComponent<Clickable>())
                    {
                        g.GetComponent<Clickable>().Release(e.Location);
                    }
                }
            }

            if (null != objectToDrag)
            {
                //objectToDrag.isBeingDragged = false;
                objectToDrag = null;
            }

        }


        private void Timer_Tick(object sender, EventArgs e)
        {

            float deltaTime = timer.Interval / 1000f;

            gameObjects.Clear();
            foreach(var o in Manager.Instance.renderer.objects)
            {
                gameObjects.Add(o.Key);
            }

            if (running)
            {

                // Update
                foreach (GameObject g in gameObjects)
                {
                    g.Update();
                }

                // Collisions
                foreach (GameObject g in gameObjects)
                {
                    // Rigidbody
                    if (null != g.GetComponent<RigidBody>())
                    {
                        foreach (GameObject other in gameObjects)
                        {
                            if (null == other.GetComponent<RigidBody>() || other == g)
                                continue;

                            if (PolygonIntersectsPolygon(g.GetGlobalPoints().ToList<PointF>(), other.GetGlobalPoints().ToList<PointF>()))
                            {
                                if (!g.GetComponent<RigidBody>().IsKinematic)
                                {
                                    g.GetComponent<RigidBody>().OnCollision(other);
                                    g.GetComponent<RigidBody>().IsGrounded = true;
                                }
                            }
                            else
                            {
                                if (!g.GetComponent<RigidBody>().IsKinematic)
                                {
                                    g.GetComponent<RigidBody>().IsGrounded = false;
                                }
                            }
                        }
                    }

                    // Collider
                    if (null != g.GetComponent<Collider>())
                    {
                        foreach (GameObject other in gameObjects)
                        {
                            if (null == other.GetComponent<Collider>() || other == g)
                                continue;

                            if (PolygonIntersectsPolygon(g.GetGlobalPoints().ToList<PointF>(), other.GetGlobalPoints().ToList<PointF>()))
                                g.GetComponent<Collider>().OnCollision(other);
                        }
                    }

                }

                // FixedUpdate
                foreach (GameObject g in gameObjects)
                {
                    g.FixedUpdate(deltaTime);
                }

            }

            // Render stuff
            renderer.Render();

            // Paint on the resulting image after all gameobjects are drawn.
            foreach (GameObject g in gameObjects)
            {
                try
                {
                    PointF[] p = g.GetGlobalPoints().ToArray<PointF>();
                    Pen pen = new Pen(Color.Orange, 5);
                    renderer.GetGraphics().DrawPolygon(pen, p);
                }
                catch (Exception) { }
            }

            pictureBox1.Image = imageBitmap;

        }

        #endregion



        #region TreeView

        public void UpdateTreeview()
        {
            treeView1.Nodes.Clear();

            TreeNode rootNode = new TreeNode("Root");

            treeView1.Nodes.Add(rootNode);

            foreach (KeyValuePair<GameObject, int> objects in renderer.objects)
            {
                TreeNode n = new TreeNode(objects.Key.Name);
                n.Tag = objects.Key;

                if (objects.Key.Parent == null)
                {
                    rootNode.Nodes.Add(n);
                }
                else
                {
                    TreeNode parentNode = FindNodeByGameObjectName(treeView1.Nodes, objects.Key.Parent.Name);
                    if (parentNode != null)
                    {
                        parentNode.Nodes.Add(n);
                    }
                    else
                    {
                        rootNode.Nodes.Add(n);
                    }
                }

            }

            treeView1.ExpandAll();

        }

        private TreeNode FindNodeByGameObjectName(TreeNodeCollection nodes, string gameObjectName)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is GameObject gameObject && gameObject.Name == gameObjectName)
                {
                    return node;
                }
                TreeNode foundNode = FindNodeByGameObjectName(node.Nodes, gameObjectName);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }

        private void TreeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void TreeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void TreeView1_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode newParentNode = null;

            // Bestimme die Zielknoten
            Point pt = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode destinationNode = treeView1.GetNodeAt(pt);

            if (destinationNode == null)
            {
                return;
            }

            newParentNode = destinationNode;

            /*
            // Bestimme den neuen Elternknoten
            if (destinationNode.Level == 0)
            {
                newParentNode = destinationNode;
            }
            else
            {
                newParentNode = destinationNode.Parent;
            }
            */

            // Verschiebe den verschobenen Knoten unter den neuen Elternknoten
            TreeNode movedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (newParentNode != null && movedNode != null)
            {

                Console.WriteLine(newParentNode.Text + "-" + newParentNode.Level);

                if (newParentNode.Level == 0)
                {
                    if (null == ((GameObject)movedNode.Tag).Parent)
                        return;

                    GameObject elem = renderer.Extract(((GameObject)movedNode.Tag).Name);
                    GameObject elemParent = renderer.Extract(elem.Parent.Name);
                    elemParent.RemoveChild(elem);
                    elem.Parent = null;
                    renderer.AddObject(elemParent);
                    renderer.AddObject(elem);
                }
                else if (newParentNode.Level > 0)
                {
                    GameObject newParent = renderer.Extract(((GameObject)newParentNode.Tag).Name);
                    GameObject newChild = renderer.Extract(((GameObject)movedNode.Tag).Name);

                    gameObjects.Remove(newParent);
                    gameObjects.Remove(newChild);

                    newParent.AddChild(newChild);

                    renderer.AddObject(newParent);
                    renderer.AddObject(newChild);

                    gameObjects.Add(newParent);
                    gameObjects.Add(newChild);

                    newParent.Update();
                    newChild.Update();


                    destinationNode.Nodes.Add((TreeNode)movedNode.Clone());
                    movedNode.Remove();
                }

                UpdateTreeview();
            }
        }

        #endregion



        #region Collisions

        public bool CheckCollision(PointF[] first, PointF[] second)
        {
            List<PointF> self = first.ToList();
            List<PointF> otherList = second.ToList();

            return PolygonIntersectsPolygon(self, otherList) || PolygonIntersectsPolygon(otherList, self);
        }

        /// <summary>
        /// Diese Methode überprüft, ob ein Punkt des ersten Polygons mit einem beliebigen Punkt des zweiten Polygons durch eine Linie verbunden werden kann, 
        /// die das zweite Polygon schneidet. Dazu wird die Methode LineIntersectsPolygon für jede Kante des ersten Polygons aufgerufen.
        /// </summary>
        /// <param name="polygonA"></param>
        /// <param name="polygonB"></param>
        /// <returns></returns>
        public bool PolygonIntersectsPolygon(List<PointF> polygonA, List<PointF> polygonB)
        {
            int countA = polygonA.Count;
            int countB = polygonB.Count;

            for (int i = 0; i < countA; i++)
            {
                PointF p1 = polygonA[i];
                PointF p2 = polygonA[(i + 1) % countA];

                if (LineIntersectsPolygon(p1, p2, polygonB))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Diese Methode überprüft, ob eine Linie, definiert durch zwei Punkte p1 und p2, ein beliebiges Segment des gegebenen Polygons polygon schneidet. 
        /// Dazu wird die Methode LineIntersectsLine für jede Kante des Polygons aufgerufen.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public bool LineIntersectsPolygon(PointF p1, PointF p2, List<PointF> polygon)
        {
            int count = polygon.Count;

            for (int i = 0; i < count; i++)
            {
                PointF q1 = polygon[i];
                PointF q2 = polygon[(i + 1) % count];

                if (LineIntersectsLine(p1, p2, q1, q2))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Diese Methode überprüft, ob zwei Linien, definiert durch je zwei Punkte, sich schneiden. 
        /// Dazu wird der Algorithmus zur Bestimmung der Orientierung verwendet, um zu prüfen, ob die Linien sich überschneiden.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public bool LineIntersectsLine(PointF p1, PointF p2, PointF q1, PointF q2)
        {
            int o1 = Orientation(p1, p2, q1);
            int o2 = Orientation(p1, p2, q2);
            int o3 = Orientation(q1, q2, p1);
            int o4 = Orientation(q1, q2, p2);

            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            if (o1 == 0 && OnSegment(p1, q1, p2)) return true;
            if (o2 == 0 && OnSegment(p1, q2, p2)) return true;
            if (o3 == 0 && OnSegment(q1, p1, q2)) return true;
            if (o4 == 0 && OnSegment(q1, p2, q2)) return true;

            return false;
        }

        /// <summary>
        /// Diese Methode bestimmt die Orientierung von drei Punkten: p, q und r. 
        /// Sie wird verwendet, um festzustellen, ob zwei Linien sich schneiden oder parallel sind.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public int Orientation(PointF p, PointF q, PointF r)
        {
            float val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0) return 0;
            return (val > 0) ? 1 : 2;
        }

        /// <summary>
        /// Diese Methode überprüft, ob ein Punkt q sich auf der Linie, die durch die Punkte p und r definiert wird, befindet. 
        /// Sie wird verwendet, um zu prüfen, ob ein Schnittpunkt innerhalb eines Segments liegt.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool OnSegment(PointF p, PointF q, PointF r)
        {
            return q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                   q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y);
        }

        private bool IsPointInPolygon(PointF location, PointF[] points)
        {
            PointF[] polygon = points;
            int polygonLength = polygon.Length;
            bool inside = false;

            for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
            {
                if ((polygon[i].Y > location.Y) != (polygon[j].Y > location.Y) &&
                    (location.X < (polygon[j].X - polygon[i].X) * (location.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public void CheckCollisionDirectionWithPreviousPosition(GameObject g1, GameObject g2)
        {
            // Vorherige Positionen der GameObjects abrufen
            PointF previousPosition1 = g1.Transform.lastLocalPosition;
            PointF previousPosition2 = g2.Transform.lastLocalPosition;

            // Richtungsvektoren zwischen vorheriger und aktueller Position berechnen
            PointF movementDirection1 = new PointF(g1.Transform.LocalPosition.X - previousPosition1.X, g1.Transform.LocalPosition.Y - previousPosition1.Y);
            PointF movementDirection2 = new PointF(g2.Transform.LocalPosition.X - previousPosition2.X, g2.Transform.LocalPosition.Y - previousPosition2.Y);

            // Normalisiere die Richtungsvektoren
            float length1 = (float)Math.Sqrt(movementDirection1.X * movementDirection1.X + movementDirection1.Y * movementDirection1.Y);
            movementDirection1 = new PointF(movementDirection1.X / length1, movementDirection1.Y / length1);

            float length2 = (float)Math.Sqrt(movementDirection2.X * movementDirection2.X + movementDirection2.Y * movementDirection2.Y);
            movementDirection2 = new PointF(movementDirection2.X / length2, movementDirection2.Y / length2);

            // Berechne die Differenz der Bewegungsrichtungen
            float diffX = movementDirection1.X - movementDirection2.X;
            float diffY = movementDirection1.Y - movementDirection2.Y;

            Console.WriteLine("Collision direction: " + diffX + ":" + diffY);
        }

        public void CheckCollisionDirection(GameObject g1, GameObject g2)
        {
            // Mittelpunkte der GameObjects berechnen
            PointF center1 = GetCenter(g1.GetGlobalPoints().ToList<PointF>());
            PointF center2 = GetCenter(g2.GetGlobalPoints().ToList<PointF>());

            // Richtungsvektor zwischen den Mittelpunkten berechnen
            PointF collisionDirection = new PointF(center2.X - center1.X, center2.Y - center1.Y);

            // Normalisiere den Richtungsvektor
            float length = (float)Math.Sqrt(collisionDirection.X * collisionDirection.X + collisionDirection.Y * collisionDirection.Y);
            collisionDirection = new PointF(collisionDirection.X / length, collisionDirection.Y / length);

            // Bestimme die Richtung der Collider-Kanten von g2
            List<PointF> colliderPoints = g2.GetGlobalPoints().ToList<PointF>();
            List<PointF> edgeDirections = new List<PointF>();
            for (int i = 0; i < colliderPoints.Count; i++)
            {
                PointF p1 = colliderPoints[i];
                PointF p2 = colliderPoints[(i + 1) % colliderPoints.Count];
                PointF edgeDirection = new PointF(p2.X - p1.X, p2.Y - p1.Y);
                float edgeLength = (float)Math.Sqrt(edgeDirection.X * edgeDirection.X + edgeDirection.Y * edgeDirection.Y);
                edgeDirections.Add(new PointF(edgeDirection.X / edgeLength, edgeDirection.Y / edgeLength));
            }

            // Bestimme aus welcher Richtung die Kollision erfolgt
            PointF collisionEdgeDirection = new PointF();
            foreach (PointF edgeDirection in edgeDirections)
            {
                float dotProduct = collisionDirection.X * edgeDirection.X + collisionDirection.Y * edgeDirection.Y;
                if (dotProduct > 0)
                {
                    collisionEdgeDirection = edgeDirection;
                    break;
                }
            }

            // Ausgabe der Kollisionsrichtung
            Console.WriteLine("Collision direction: " + collisionEdgeDirection.ToString());
        }

        public PointF GetCenter(List<PointF> points)
        {
            float sumX = 0;
            float sumY = 0;
            foreach (PointF point in points)
            {
                sumX += point.X;
                sumY += point.Y;
            }
            return new PointF(sumX / points.Count, sumY / points.Count);
        }


        #endregion



        #region Other

        public void AskForProjectFolder()
        {

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // Set the initial directory (optional)
            folderBrowserDialog.SelectedPath = @"C:\";

            // Show the dialog and check if the user selected a folder
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Retrieve the selected folder
                string selectedFolder = folderBrowserDialog.SelectedPath;
                Console.WriteLine("Selected folder: " + selectedFolder);
                projectFolder = selectedFolder;
            }
            else
            {
                Console.WriteLine("Folder selection cancelled.");
                projectFolder = String.Empty;
            }

        }

        public void ShootForwardRay(GameObject sender, float distance)
        {

            // Berechnen Sie den Forward-Vektor des GameObjects
            PointF forwardVector = new PointF((float)Math.Cos(sender.Transform.GlobalRotation * Math.PI / 180), (float)Math.Sin(sender.Transform.GlobalRotation * Math.PI / 180));

            // Berechnen Sie den Endpunkt der Linie basierend auf dem Forward-Vektor und der Entfernung
            float endX = sender.Transform.GlobalPosition.X + forwardVector.X * distance;
            float endY = sender.Transform.GlobalPosition.Y + forwardVector.Y * distance;

            // Zeichnen Sie die Linie
            using (Graphics g = this.CreateGraphics())
            {
                g.DrawLine(Pens.Red, sender.Transform.GlobalPosition.X, sender.Transform.GlobalPosition.Y, endX, endY);
            }

        }

        // \obsolete
        private RectangleF GetBoundingBox(GameObject obj)
        {
            // Initialize min and max points with extreme values
            PointF minPoint = new PointF(float.MaxValue, float.MaxValue);
            PointF maxPoint = new PointF(float.MinValue, float.MinValue);

            // Iterate through all mesh components of the object
            foreach (var component in obj.Components)
            {
                if (component is Mesh meshComponent)
                {
                    // Iterate through all local points of the mesh
                    foreach (var localPoint in meshComponent.localPoints)
                    {
                        // Transform local point to global point
                        //PointF globalPoint = TransformPoint(localPoint, obj.Transform);
                        PointF globalPoint = obj.TransformPoint(localPoint, obj.Transform);

                        // Update min and max points based on the global point
                        minPoint.X = Math.Min(minPoint.X, globalPoint.X);
                        minPoint.Y = Math.Min(minPoint.Y, globalPoint.Y);
                        maxPoint.X = Math.Max(maxPoint.X, globalPoint.X);
                        maxPoint.Y = Math.Max(maxPoint.Y, globalPoint.Y);
                    }
                }
            }

            // Calculate the size of the bounding box
            SizeF size = new SizeF(maxPoint.X - minPoint.X, maxPoint.Y - minPoint.Y);

            // Calculate the position of the bounding box (bottom-left corner)
            PointF position = new PointF(minPoint.X, minPoint.Y);

            return new RectangleF(position, size);
        }

        #endregion


    }


    #region Node Editor

    public class MainForm : Form
    {

        public NodeExample node;

        public MainForm(NodeExample _node)
        {

            InitializeUI();
            this.Text = "Node Editor";
            node = _node;

        }

        private void InitializeUI()
        {
            Size = new Size(400, 400);
            Text = "Node Example";
            Paint += MainForm_Paint;
            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            node.Paint(sender, e);
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            node.OnMouseDown(sender, e);
            Invalidate();
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            node.OnMouseMove(sender, e);
            Invalidate();
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            node.OnMouseUp(sender, e);
            Invalidate();
        }
    }

    public class NodeExample : GameObject
    {
        private const int pointSize = 10;

        private PointF[] dragPoints;
        private int selectedPointIndex = -1;

        public GameObject customNode;

        public PointF[] Points;

        public NodeExample(GameObject _node)
        {

            //PointF oldOrigin = _node.Origin;

            //_node.Origin = new PointF(100,100); // new PointF(_node.Bounds.X + _node.Bounds.Width, _node.Bounds.Y + _node.Bounds.Height);

            customNode = _node;

            Points = _node.GetComponent<Mesh>().localPoints.ToArray(); //_node.GetGlobalPoints().ToArray();

            dragPoints = new PointF[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                dragPoints[i] = new PointF((float)Points[i].X + customNode.Transform.LocalPosition.X, (float)Points[i].Y + customNode.Transform.LocalPosition.Y);
            }
        }

        public void Paint(object sender, PaintEventArgs e)
        {

            foreach (PointF point in dragPoints)
            {
                e.Graphics.FillRectangle(Brushes.Red, point.X - pointSize / 2, point.Y - pointSize / 2, pointSize, pointSize);
            }
        }

        public void OnMouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < dragPoints.Length; i++)
            {
                RectangleF rect = new RectangleF(dragPoints[i].X - pointSize / 2, dragPoints[i].Y - pointSize / 2, pointSize, pointSize);
                if (rect.Contains(e.Location))
                {
                    selectedPointIndex = i;
                    break;
                }
            }
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPointIndex != -1)
            {
                dragPoints[selectedPointIndex] = new Point(e.X, e.Y);
                UpdatePoints();
            }
        }

        public void OnMouseUp(object sender, MouseEventArgs e)
        {
            selectedPointIndex = -1;
        }

        private void UpdatePoints()
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new PointF(dragPoints[i].X - customNode.Transform.LocalPosition.X, dragPoints[i].Y - customNode.Transform.LocalPosition.Y);
                //Points[i] = new PointF(dragPoints[i].X, dragPoints[i].Y);
            }
            customNode.GetComponent<Mesh>().localPoints = Points.ToList();
        }
    }

    #endregion





}
