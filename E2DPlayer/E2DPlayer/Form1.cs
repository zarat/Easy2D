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


using Easy2D;
using Easy2D.Components;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace E2DPlayer
{

    public partial class Form1 : Form
    {

        #region DLLImport

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        #endregion


        #region Member


        public List<GameObject> gameObjects = new List<GameObject>();

        public Timer timer;


        private GameObject objectToDrag; // Das GameObject, das gezogen wird
        private PointF lastMousePosition; // Die letzte Mausposition

        public Dictionary<Keys, int> pressedKeys = new Dictionary<Keys, int>();

        public Renderer renderer;

        private Bitmap imageBitmap;
        private PictureBox pictureBox1;

        private Easy2D.Manager manager = Easy2D.Manager.GetInstance();

        public bool running = false;

        #endregion


        public Form1()
        {
            InitializeComponent();

            InitializeGame();

        }



        public void InitializeGame()
        {

            this.ClientSize = new Size(640, 480);

            imageBitmap = new Bitmap(640, 480);

            pictureBox1 = new PictureBox();

            pictureBox1.Size = imageBitmap.Size;

            pictureBox1.Margin = new Padding(0);
            pictureBox1.Padding = new Padding(0);

            pictureBox1.Image = imageBitmap;

            Controls.Add(pictureBox1);

            manager.renderer = new Renderer(Graphics.FromImage(imageBitmap));
            renderer = manager.renderer;
            renderer.clearColor = Color.White;

            manager.projectFolder = ".\\";

            gameObjects = Manager.Instance.LoadScene("Scene.xml");
            foreach(GameObject g in gameObjects)
            {
                Console.WriteLine("Added " + g.Name);
                renderer.AddObject(g);
            }

            


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




        #region Menu Events

        public List<GameObject> LoadScene(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            List<GameObject> addedGameObjects = new List<GameObject>();

            // foreach gameobject
            foreach (XmlElement nodeElement in xmlDoc.SelectNodes("//GameObjects/GameObject"))
            {

                GameObject gameObject = new GameObject();

                string gameObjectName = nodeElement.GetAttribute("Name");
                gameObject.Name = gameObjectName;

                if (null != nodeElement.GetAttribute("Parent"))
                {
                    foreach (GameObject alreadyAddedGameObject in addedGameObjects)
                    {
                        if (alreadyAddedGameObject.Name == nodeElement.GetAttribute("Parent"))
                        {
                            gameObject.Parent = alreadyAddedGameObject;
                            //gameObjects.Remove(alreadyAddedGameObject);
                            alreadyAddedGameObject.AddChild(gameObject);
                            //gameObjects.Add(alreadyAddedGameObject);
                            break;
                        }
                    }
                }
                int renderLayer = int.Parse(nodeElement.GetAttribute("RenderLayer"));
                gameObject.RenderLayer = renderLayer;
                float globalPositionX = float.Parse(nodeElement.GetAttribute("Transform.GlobalPosition.X"));
                float globalPositionY = float.Parse(nodeElement.GetAttribute("Transform.GlobalPosition.Y"));
                gameObject.Transform.GlobalPosition = new PointF(globalPositionX, globalPositionY);
                float localPositionX = float.Parse(nodeElement.GetAttribute("Transform.LocalPosition.X"));
                float localPositionY = float.Parse(nodeElement.GetAttribute("Transform.LocalPosition.Y"));
                gameObject.Transform.LocalPosition = new PointF(localPositionX, localPositionY);
                float globalRotation = float.Parse(nodeElement.GetAttribute("Transform.GlobalRotation"));
                gameObject.Transform.GlobalRotation = globalRotation;
                float localRotation = float.Parse(nodeElement.GetAttribute("Transform.LocalRotation"));
                gameObject.Transform.LocalRotation = localRotation;

                // foreach Component
                foreach (XmlElement componentElement in nodeElement.SelectNodes("Components/Component"))
                {
                    string componentType = componentElement.GetAttribute("type");
                    switch (componentType)
                    {
                        case "Easy2D.Components.AudioPlayer":
                            {
                                AudioPlayer audioComponent = new AudioPlayer();
                                audioComponent.audio = componentElement.GetAttribute("audio");
                                audioComponent.loop = componentElement.GetAttribute("loop") == "True" ? true : false;
                                audioComponent.Parent = gameObject;
                                gameObject.AddComponent(audioComponent);
                            }
                            break;
                        case "Easy2D.Components.Clickable": // Should be a scriptable component
                            {
                                Clickable clickable = new Clickable();
                                clickable.Parent = gameObject;
                                gameObject.AddComponent(clickable);
                            }
                            break;
                        case "Easy2D.Components.Collider":
                            {
                                Collider collider = new Collider();
                                collider.Parent = gameObject;
                                gameObject.AddComponent(collider);
                            }
                            break;
                        case "Easy2D.Components.Hoverable": // Should be a scriptable Component
                            {
                                Hoverable hoverable = new Hoverable();
                                hoverable.Parent = gameObject;
                                gameObject.AddComponent(hoverable);
                            }
                            break;
                        case "Easy2D.Components.Mesh":
                            {
                                List<PointF> points = new List<PointF>();
                                foreach (XmlElement pointElement in componentElement.SelectNodes("Points/Point"))
                                {
                                    //float x = float.Parse(pointElement.GetAttribute("x"));
                                    //float y = float.Parse(pointElement.GetAttribute("y"));

                                    float x = 0;
                                    float y = 0;
                                    if (float.TryParse(pointElement.GetAttribute("x"), out float resultX))
                                    {
                                        x = resultX;
                                    }
                                    if (float.TryParse(pointElement.GetAttribute("y"), out float resultY))
                                    {
                                        y = resultY;
                                    }

                                    points.Add(new PointF(x, y));
                                }
                                Mesh mesh = new Mesh(points, gameObject);
                                gameObject.AddComponent(mesh);
                            }
                            break;
                        case "Easy2D.Components.RigidBody":
                            {
                                RigidBody rb = new RigidBody();

                                rb.Mass = float.Parse(componentElement.GetAttribute("mass"));
                                rb.Mass = float.Parse(componentElement.GetAttribute("mass"));

                                float velocityX = float.Parse(componentElement.GetAttribute("velocityX"));
                                float velocityY = float.Parse(componentElement.GetAttribute("velocityY"));
                                rb.Velocity = new PointF(velocityX, velocityY);

                                float accelerationX = float.Parse(componentElement.GetAttribute("accelerationX"));
                                float accelerationY = float.Parse(componentElement.GetAttribute("accelerationY"));
                                rb.Acceleration = new PointF(accelerationX, accelerationY);

                                float maxaccelerationX = float.Parse(componentElement.GetAttribute("maxaccelerationX"));
                                float maxaccelerationY = float.Parse(componentElement.GetAttribute("maxaccelerationY"));
                                rb.MaxAcceleration = new PointF(maxaccelerationX, maxaccelerationY);

                                float gravity = float.Parse(componentElement.GetAttribute("gravity"));
                                rb.Gravity = gravity;
                                bool isKinematic = componentElement.GetAttribute("isKinematic") == "True" ? true : false;
                                rb.IsKinematic = isKinematic;

                                rb.Parent = gameObject;
                                gameObject.AddComponent(rb);
                            }
                            break;
                        case "Easy2D.Components.Sprite":
                            {
                                string imagePath = componentElement.GetAttribute("imagePath");
                                int width = int.Parse(componentElement.GetAttribute("width"));
                                int height = int.Parse(componentElement.GetAttribute("height"));
                                System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
                                Sprite sprite = new Sprite(image, width, height, gameObject);
                                float offsetX = float.Parse(componentElement.GetAttribute("offsetX"));
                                float offsetY = float.Parse(componentElement.GetAttribute("offsetY"));
                                sprite.offsetX = offsetX;
                                sprite.offsetY = offsetY;
                                sprite.Parent = gameObject;
                                sprite.imagePath = imagePath;
                                sprite.rotation = float.Parse(componentElement.GetAttribute("rotation"));
                                gameObject.AddComponent(sprite);


                            }
                            break;
                        case "Easy2D.Components.SpriteAnimation":
                            {

                                string imagePath = componentElement.GetAttribute("imagePath");
                                SpriteAnimation spriteAnimation = new SpriteAnimation();
                                System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
                                spriteAnimation.Image = image;
                                spriteAnimation.ImagePath = imagePath;
                                string animationType = componentElement.GetAttribute("animationType");
                                switch (animationType)
                                {
                                    case "Horizontal":
                                        spriteAnimation.animationType = AnimationType.Horizontal;
                                        break;
                                    case "Vertical":
                                        spriteAnimation.animationType = AnimationType.Vertical;
                                        break;
                                    case "Matrix":
                                        spriteAnimation.animationType = AnimationType.Matrix;
                                        break;
                                }

                                spriteAnimation.frameWidth = int.Parse(componentElement.GetAttribute("frameWidth"));
                                spriteAnimation.frameHeight = int.Parse(componentElement.GetAttribute("frameHeight"));
                                spriteAnimation.currentFrame = int.Parse(componentElement.GetAttribute("currentFrame"));
                                spriteAnimation.totalFrames = int.Parse(componentElement.GetAttribute("totalFrames"));
                                spriteAnimation.currentAnimation = int.Parse(componentElement.GetAttribute("currentAnimation"));
                                spriteAnimation.scaleX = float.Parse(componentElement.GetAttribute("scaleX"));
                                spriteAnimation.scaleY = float.Parse(componentElement.GetAttribute("scaleY"));
                                spriteAnimation.running = componentElement.GetAttribute("running") == "True" ? true : false;
                                spriteAnimation.Parent = gameObject;
                                gameObject.AddComponent(spriteAnimation);
                            }
                            break;

                    }




                }

                // Todo Scripts 
                List<string> addedScriptsList = new List<string>();

                foreach (XmlElement scriptElement in nodeElement.SelectNodes("Scripts/Script"))
                {

                    string scriptPath = scriptElement.GetAttribute("path");

                    if (addedScriptsList.Contains(scriptPath))
                    {
                        continue;
                    }
                    //addedScriptsList.Add(scriptPath);


                    object o = ScriptLoader.LoadAndInstantiate(scriptPath, "", "");
                    if (null != o)
                    {
                        addedScriptsList.Add(scriptPath);
                        global::Easy2D.Components.Component c = o as global::Easy2D.Components.Component;
                        c.Parent = gameObject;
                        gameObject.AddComponent(c);
                        c.Init();
                    }

                }

                gameObject.importedScripts = addedScriptsList.ToArray<string>();
                gameObject.scriptsToImport = addedScriptsList.ToArray<string>();
                gameObject._script = addedScriptsList.ToArray<string>();

                //gameObject.Update(); ??
                addedGameObjects.Add(gameObject);

            }

            return addedGameObjects;

        }

        #endregion


        #region Event Handler

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {

            foreach (GameObject g in gameObjects)
            {
                // \todo handle renderlayer!
                if (IsPointInPolygon(e.Location, g.GetGlobalPoints().ToArray()))
                {
                    if (null != g.GetComponent<Clickable>())
                        g.GetComponent<Clickable>().Click(e.Location);
                }
            }

        }

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {

         

            // hoverable items
            foreach (GameObject g in gameObjects)
            {

                if (IsPointInPolygon(e.Location, g.GetGlobalPoints().ToArray()))
                {
                    if (null != g.GetComponent<Hoverable>())
                    {
                        if (!g.GetComponent<Hoverable>().hovered)
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


        }


        public int ticks = 0;

        private void Timer_Tick(object sender, EventArgs e)
        {

            float deltaTime = timer.Interval / 1000f;

            gameObjects.Clear();

            foreach (var o in Manager.Instance.renderer.objects)
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
            

            pictureBox1.Image = imageBitmap;

        }

        #endregion


        public void UpdateGameObjects()
        {
            foreach (GameObject g in gameObjects)
            {
                g.Update();
            }
        }

        public void UpdatePhysics(float deltaTime)
        {
            foreach (GameObject g in gameObjects)
            {
                g.FixedUpdate(deltaTime);
            }
        }






        #region TreeView

        
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

        #endregion



        #region Input

        public void HandleInput(float deltaTime)
        {



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


    #region Test

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

    public class ScriptLoader
    {
        public static object LoadAndInstantiate(string filePath, string className, string namespaceName)
        {

            if (!File.Exists(filePath))
                return null;

            string code = File.ReadAllText(filePath);

            CompilerParameters compilerParams = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            // Füge die Assembly deines Projekts hinzu
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            compilerParams.ReferencedAssemblies.Add(assemblyPath);
            compilerParams.ReferencedAssemblies.Add("D:\\Projekte\\Easy2D\\Easy2D\\bin\\Debug\\Easy2D.dll");

            // Füge weitere Assemblys hinzu, falls erforderlich
            compilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
            compilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");

            CompilerResults results = CodeDomProvider.CreateProvider("CSharp")
                .CompileAssemblyFromSource(compilerParams, code);

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
                return null;
            }
            else
            {

                Assembly assembly = results.CompiledAssembly;
                Type[] types = assembly.GetExportedTypes();
                foreach (Type type in types)
                {

                    object instance = Activator.CreateInstance(type);
                    return instance;

                }

                Console.WriteLine($"Keine Klasse im Namespace {namespaceName} gefunden.");
                return null;

                /*
                Assembly assembly = results.CompiledAssembly;
                Type type = assembly.GetType($"{namespaceName}.{className}");
                if (type != null)
                {
                    object instance = Activator.CreateInstance(type);
                    return instance;
                }
                else
                {
                    Console.WriteLine($"Klasse {className} im Namespace {namespaceName} nicht gefunden.");
                    return null;
                }
                */

            }
        }
    }

    #endregion

}
