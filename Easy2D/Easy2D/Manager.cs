
using System;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using Easy2D.Components;
using System.Linq;
using System.Security.Policy;

namespace Easy2D
{

    public class Manager
    {

        private static Manager instance;

        public Renderer renderer;

        public string projectFolder;

        private Manager()
        {

        }

        public static Manager GetInstance()
        {
            if (instance == null)
            {
                instance = new Manager();
            }
            return instance;
        }

        public static Manager Instance { get { return instance; } }

        /// <summary>
        /// Save the current scene to file
        /// </summary>
        /// \todo Assets folder
        /// <param name="filePath"></param>
        public void SaveScene(string filePath, bool inEditor = false)
        {

            if(String.IsNullOrEmpty(projectFolder))
            {
                return;
            }

            string assetsFolder = projectFolder;

            // XML-Dokument erstellen
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            XmlElement root = xmlDoc.CreateElement("Scene");
            root.SetAttribute("version", "0.1");

            xmlDoc.AppendChild(root);

            XmlElement gameObjectListElement = xmlDoc.CreateElement("GameObjects");

            foreach (KeyValuePair<GameObject, int> objects in renderer.objects)
            {

                XmlElement gameObjectElement = xmlDoc.CreateElement("GameObject");

                gameObjectElement.SetAttribute("Name", objects.Key.Name);
                if (null != objects.Key.Parent)
                    gameObjectElement.SetAttribute("Parent", objects.Key.Parent.Name);

                gameObjectElement.SetAttribute("RenderLayer", objects.Key.RenderLayer.ToString());

                // Transform
                gameObjectElement.SetAttribute("Transform.LocalPosition.X", objects.Key.Transform.LocalPosition.X.ToString());
                gameObjectElement.SetAttribute("Transform.LocalPosition.Y", objects.Key.Transform.LocalPosition.Y.ToString());
                gameObjectElement.SetAttribute("Transform.GlobalPosition.X", objects.Key.Transform.GlobalPosition.X.ToString());
                gameObjectElement.SetAttribute("Transform.GlobalPosition.Y", objects.Key.Transform.GlobalPosition.Y.ToString());
                gameObjectElement.SetAttribute("Transform.LocalRotation", objects.Key.Transform.LocalRotation.ToString());
                gameObjectElement.SetAttribute("Transform.GlobalRotation", objects.Key.Transform.GlobalRotation.ToString());
                gameObjectElement.SetAttribute("Transform.Scale", objects.Key.Transform.Scale.ToString());
                gameObjectElement.SetAttribute("disposabe", objects.Key.disposable.ToString());
                gameObjectElement.SetAttribute("lifeTime", objects.Key.lifeTime.ToString());

                XmlElement gameObjectComponentListElement = xmlDoc.CreateElement("Components");

                // Components
                foreach (Easy2D.Components.Component component in objects.Key.Components)
                {
                    string componentType = component.Name; // component.GetType().ToString();

                    switch (componentType)
                    {
                        case "Easy2D.Components.AudioPlayer":
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                AudioPlayer audioComponent = (AudioPlayer)component;
                                componentElement.SetAttribute("type", componentType);

                                string audioPath = "";

                                // Save sprite 
                                // If the image is NOT in project folder, copy it
                                if (!audioComponent.audio.Contains(projectFolder))
                                {
                                    File.Copy(audioComponent.audio, Path.Combine(projectFolder, Path.GetFileName(audioComponent.audio)));
                                }

                                componentElement.SetAttribute("audio", Path.GetFileName(audioComponent.audio));
                                
                                
                                componentElement.SetAttribute("loop", audioComponent.loop.ToString());
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;
                        case "Easy2D.Components.Clickable": // Should be a scriptable component
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                componentElement.SetAttribute("type", componentType);
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;
                        case "Easy2D.Components.Collider":
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                componentElement.SetAttribute("type", componentType);
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;
                        case "Easy2D.Components.Hoverable": // Should be a scriptable Component
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                componentElement.SetAttribute("type", componentType);
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;
                        case "Easy2D.Components.Mesh":
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                Mesh mesh = (Mesh)component;
                                componentElement.SetAttribute("type", componentType);

                                // TODO
                                XmlElement pointListElement = xmlDoc.CreateElement("Points");
                                foreach (PointF point in mesh.localPoints)
                                {
                                    XmlElement pointElement = xmlDoc.CreateElement("Point");
                                    pointElement.SetAttribute("x", point.X.ToString());
                                    pointElement.SetAttribute("y", point.Y.ToString());
                                    pointListElement.AppendChild(pointElement);
                                }
                                componentElement.AppendChild(pointListElement);

                                // Save color as Hexstring
                                componentElement.SetAttribute("color", mesh.color.ToArgb().ToString("X"));

                                // name
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;
                        case "Easy2D.Components.RigidBody":
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                RigidBody rb = (RigidBody)component;
                                componentElement.SetAttribute("type", componentType);
                                componentElement.SetAttribute("mass", rb.Mass.ToString());
                                componentElement.SetAttribute("velocityX", rb.Velocity.X.ToString());
                                componentElement.SetAttribute("velocityY", rb.Velocity.Y.ToString());
                                componentElement.SetAttribute("accelerationX", rb.Acceleration.X.ToString());
                                componentElement.SetAttribute("accelerationY", rb.Acceleration.Y.ToString());
                                componentElement.SetAttribute("maxaccelerationX", rb.MaxAcceleration.X.ToString());
                                componentElement.SetAttribute("maxaccelerationY", rb.MaxAcceleration.Y.ToString());
                                componentElement.SetAttribute("gravity", rb.Gravity.ToString());
                                componentElement.SetAttribute("isKinematic", rb.IsKinematic.ToString());
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;
                        case "Easy2D.Components.Sprite":
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                Sprite sprite = (Sprite)component;
                                componentElement.SetAttribute("type", componentType);

                                // Save sprite 
                                // If the image is NOT in project folder, copy it
                                if( !sprite.ImagePath.Contains(projectFolder))
                                {
                                    File.Copy(sprite.ImagePath, Path.Combine(projectFolder, Path.GetFileName(sprite.ImagePath)));
                                }
                                
                                componentElement.SetAttribute("imagePath", Path.GetFileName(sprite.ImagePath));
                                
                                componentElement.SetAttribute("width", sprite.width.ToString());
                                componentElement.SetAttribute("height", sprite.height.ToString());
                                componentElement.SetAttribute("offsetX", sprite.offsetX.ToString());
                                componentElement.SetAttribute("offsetY", sprite.offsetY.ToString());
                                componentElement.SetAttribute("rotation", sprite.rotation.ToString());
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;
                        case "Easy2D.Components.SpriteAnimation":
                            {
                                XmlElement componentElement = xmlDoc.CreateElement("Component");
                                SpriteAnimation sprite = (SpriteAnimation)component;
                                componentElement.SetAttribute("type", componentType);

                                if (!sprite.ImagePath.Contains(projectFolder))
                                {
                                    File.Copy(sprite.ImagePath, Path.Combine(projectFolder, Path.GetFileName(sprite.ImagePath)));
                                }

                                componentElement.SetAttribute("imagePath", Path.GetFileName(sprite.ImagePath));

                                componentElement.SetAttribute("animationType", sprite.animationType.ToString());
                                componentElement.SetAttribute("frameWidth", sprite.frameWidth.ToString());
                                componentElement.SetAttribute("frameHeight", sprite.frameHeight.ToString());
                                componentElement.SetAttribute("currentFrame", sprite.currentFrame.ToString());
                                componentElement.SetAttribute("totalFrames", sprite.totalFrames.ToString());
                                componentElement.SetAttribute("currentAnimation", sprite.currentAnimation.ToString());
                                componentElement.SetAttribute("scaleX", sprite.scaleX.ToString());
                                componentElement.SetAttribute("scaleY", sprite.scaleY.ToString());
                                componentElement.SetAttribute("running", sprite.running.ToString());
                                gameObjectComponentListElement.AppendChild(componentElement);
                            }
                            break;

                    }
                }

                XmlElement gameObjectScriptListElement = xmlDoc.CreateElement("Scripts");

                // Scripts (in _script)
                foreach (string scriptableComponent in objects.Key._script)
                {
                    XmlElement gameObjectScriptElement = xmlDoc.CreateElement("Script");
                    gameObjectScriptElement.SetAttribute("path", scriptableComponent);

                    gameObjectScriptListElement.AppendChild(gameObjectScriptElement);
                }

                gameObjectElement.AppendChild(gameObjectComponentListElement);
                gameObjectElement.AppendChild(gameObjectScriptListElement);

                gameObjectListElement.AppendChild(gameObjectElement);

                root.AppendChild(gameObjectListElement);

            }

            xmlDoc.Save(filePath);
        }


        /// <summary>
        /// Load a saved scene from file
        /// </summary>
        /// \todo Assets folder
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<GameObject> LoadScene(string filePath, bool inEditor = false)
        {

            if (!System.IO.File.Exists(filePath))
                return null;

            string assetsFolder = Path.GetDirectoryName(filePath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

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
                gameObject.disposable = nodeElement.GetAttribute("disposable") == "True" ? true : false;
                gameObject.lifeTime = float.Parse(nodeElement.GetAttribute("lifeTime"));

                // foreach Component
                foreach (XmlElement componentElement in nodeElement.SelectNodes("Components/Component"))
                {
                    string componentType = componentElement.GetAttribute("type");
                    switch (componentType)
                    {
                        case "Easy2D.Components.AudioPlayer":
                            {
                                AudioPlayer audioComponent = new AudioPlayer();

                                string audioPath = "";

                                if (inEditor)
                                {
                                    audioPath = projectFolder + "\\" + componentElement.GetAttribute("audio");
                                }
                                else
                                {
                                    audioPath = componentElement.GetAttribute("audio");
                                }

                                // \todo project folder
                                audioComponent.audio = audioPath;

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

                                // Color is saved as HexString
                                string colorString = componentElement.GetAttribute("color");
                                // Parse it to an ARGB int
                                int argb = int.Parse(colorString, System.Globalization.NumberStyles.HexNumber);
                                // Color-Objekt erstellen
                                Color color = Color.FromArgb(argb);

                                mesh.color = color;
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
                                
                                int width = int.Parse(componentElement.GetAttribute("width"));
                                int height = int.Parse(componentElement.GetAttribute("height"));

                                Sprite sprite;

                                // Load Sprite
                                // When we load it, it should already be in the project folder
                                // only the filename should stay in imagePath
                                string imagePath = componentElement.GetAttribute("imagePath");

                                if (inEditor)
                                {
                                    imagePath = projectFolder + "\\" + imagePath;
                                }
                                else
                                {
                                    imagePath = componentElement.GetAttribute("imagePath");
                                }

                                if (!String.IsNullOrEmpty(imagePath) && File.Exists(imagePath)) {
                                    
                                    System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
                                    sprite = new Sprite(image, width, height, gameObject);

                                }
                                else
                                {
                                    sprite = new Sprite();
                                }

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

                                //string imagePath = componentElement.GetAttribute("imagePath");
                                SpriteAnimation spriteAnimation = new SpriteAnimation();

                                /*
                                // \todo project folder
                                System.Drawing.Image image = System.Drawing.Image.FromFile(Path.Combine(projectFolder, imagePath));
                                */

                                System.Drawing.Image image = null;

                                string imagePath = componentElement.GetAttribute("imagePath");

                                if (inEditor)
                                {
                                    imagePath = projectFolder + "\\" + imagePath;
                                }
                                else
                                {
                                    imagePath = componentElement.GetAttribute("imagePath");
                                }

                                if (!String.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                                {

                                    image = System.Drawing.Image.FromFile(imagePath);

                                }

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
                        continue;

                    Console.WriteLine("Script found " + Path.Combine(projectFolder, scriptPath));

                    object o = ScriptLoader.LoadAndInstantiate(Path.Combine(projectFolder, scriptPath), "", "");
                    if (null != o)
                    {
                        Console.WriteLine("Initiated " + Path.Combine(projectFolder, scriptPath));
                        addedScriptsList.Add(scriptPath);
                        global::Easy2D.Components.Component c = o as global::Easy2D.Components.Component;
                        c.Parent = gameObject;
                        gameObject.AddComponent(c);

                        try
                        {
                            c.Init();
                        }
                        catch (Exception ex) { Console.WriteLine("[DEBUG] " + ex.Message); }
                    }
                    else
                        Console.WriteLine("Initiation FAILED " + Path.Combine(projectFolder, scriptPath));
                }

                gameObject.importedScripts = addedScriptsList.ToArray<string>();
                gameObject.scriptsToImport = addedScriptsList.ToArray<string>();
                gameObject._script = addedScriptsList.ToArray<string>();

                //gameObject.Update(); ??
                addedGameObjects.Add(gameObject);

            }

            return addedGameObjects;

        }


    }


    #region To add images in propertygrid and get the filepath as well 

    public class SpriteImagePathEditor : UITypeEditor
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
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Bild auswählen";
                openFileDialog.Filter = "Bilddateien|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Alle Dateien|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Sprite sprite = (Sprite)context.Instance;
                    sprite.ImagePath = openFileDialog.FileName; // Pfad speichern
                    sprite.Image = Image.FromFile(openFileDialog.FileName); // Bild laden
                    return sprite.Image; // Bild zurückgeben
                }
            }
            return value;
        }
    }

    public class SpriteAnimationImagePathEditor : UITypeEditor
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
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Bild auswählen";
                openFileDialog.Filter = "Bilddateien|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Alle Dateien|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SpriteAnimation spriteAnimation = (SpriteAnimation)context.Instance;
                    spriteAnimation.ImagePath = openFileDialog.FileName; // Pfad speichern
                    spriteAnimation.Image = Image.FromFile(openFileDialog.FileName); // Bild laden
                    return spriteAnimation.Image; // Bild zurückgeben
                }
            }
            return value;
        }
    }


    #endregion


}
