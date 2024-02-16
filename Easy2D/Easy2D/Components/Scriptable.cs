﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy2D.Components
{
    public class Scriptable : Component
    {

        public Scriptable()
        {

        }

        public override void Draw(Graphics graphics)
        {
            // No need for drawing
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

        public virtual void Invoke(string functionName, List<object> args)
        {

        }

    }

}
