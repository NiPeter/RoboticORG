using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;

namespace RoboticORG
{
    class Ground : ColoredObject3D
    {
        List<Rectangle> PlanesList = new List<Rectangle>();
        private float size;

        public Ground(Color color, float size = 10)
        {
            this.size = size;
            // Top
            PlanesList.Add(new Rectangle(color));
            PlanesList[0].Position = new Vector3(-0.5f * size, -0.5f * size, 0);

            // Front
            PlanesList.Add(new Rectangle(color));
            PlanesList[1].Position = new Vector3(0.5f * size, -0.5f * size, 0);
            PlanesList[1].Rotation = new Vector3(0, (float)(Math.PI / 2.0), 0);

            // Back
            PlanesList.Add(new Rectangle(color));
            PlanesList[2].Position = new Vector3(-0.5f * size, -0.5f * size, 0);
            PlanesList[2].Rotation = new Vector3(0, (float)(Math.PI / 2.0), 0);

            //Left
            PlanesList.Add(new Rectangle(color));
            PlanesList[3].Position = new Vector3(-0.5f * size, -0.5f * size, 0);
            PlanesList[3].Rotation = new Vector3(-(float)(Math.PI / 2.0), 0, 0);

            //Right
            PlanesList.Add(new Rectangle(color));
            PlanesList[4].Position = new Vector3(-0.5f * size, 0.5f * size, 0);
            PlanesList[4].Rotation = new Vector3(-(float)(Math.PI / 2.0), 0, 0);

            for (int i = 0; i < PlanesList.Count; i++)
            {
                PlanesList[i].Scale = Vector3.One * size;
            }
        }

        public override void Draw()
        {
            foreach (Rectangle rec in PlanesList)
            {
                rec.Draw();
            }
        }

        public override Vector3[] GetColorData()
        {
            List<Vector3> colorDataList = new List<Vector3>();

            foreach (Rectangle rec in PlanesList)
            {
                Vector3[] planeColorData = rec.GetColorData();
                for (int i = 0; i < planeColorData.Length; i++)
                {
                    colorDataList.Add(planeColorData[i]);
                }
            }

            return colorDataList.ToArray();
        }

        public override Vector3[] GetVerts()
        {
            List<Vector3> vertList = new List<Vector3>();

            foreach (Rectangle rec in PlanesList)
            {
                Vector3[] planeVerts = rec.GetVerts();
                for (int i = 0; i < planeVerts.Length; i++)
                {
                    vertList.Add(planeVerts[i]);
                }
            }


            return vertList.ToArray();
        }
    }

    class Rectangle : ColoredObject3D
    {

        public Rectangle()
        {
        }

        public Rectangle(Color color)
        {
            Color = ColorToVector(color);
        }

        public Rectangle(Vector3 color)
        {
            Color = color;
        }

        public override void Draw()
        {
            Vector3[] vertexes = GetVerts();
            Vector3[] colors = GetColorData();

            // Przelicz macierz modelowania
            CalculateModelMatrix();

            // Załaduj macierz na GPU
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref ModelMatrix);

            // Rysujemy układ współrzędnych 
            GL.Begin(BeginMode.Quads);

            for (int i = 0; i < vertexes.Length; i++)
            {
                GL.Color3(colors[i]);
                GL.Vertex3(vertexes[i]);

            }
            GL.End(); // Koniec rysowania
        }

        public override Vector3[] GetColorData()
        {
            return new Vector3[]
            {
                    Color, // róg nr. 0
                    Color,
                    Color,
                    Color,
            };
        }

        public override Vector3[] GetVerts()
        {
            return new Vector3[]
            {
                new Vector3(0f, 0f,  0f), // róg nr. 0
                new Vector3(1f, 0f,  0f),
                new Vector3(1f, 1f,  0f),
                new Vector3(0f, 1f,  0f),

            };
        }
    }

    class CS : Object3D
    {

        public override Vector3[] GetColorData()
        {
            return new Vector3[]
            {
                // X
                new Vector3( 1f, 0f, 0f),
                new Vector3( 1f, 0f, 0f),
                //Y
                new Vector3( 0f, 1f, 0f),
                new Vector3( 0f, 1f, 0f),
                //Z
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f, 0f, 1f)
            };
        }


        public override Vector3[] GetVerts()
        {

            return new Vector3[]
            {   
                //X
                new Vector3(0f, 0f,  0f), // róg nr. 0
                new Vector3(1f, 0f,  0f),
                //Y
                new Vector3(0f, 0f,  0f), // róg nr. 0
                new Vector3(0f, 1f,  0f),
                //Z
                new Vector3(0f, 0f,  0f), // róg nr. 0
                new Vector3(0f, 0f,  1f),

            };
        }


        public override void Draw()
        {
            Vector3[] vertexes = GetVerts();
            Vector3[] colors = GetColorData();

            // Przelicz macierz modelowania
            CalculateModelMatrix();

            // Załaduj macierz na GPU
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref ModelMatrix);

            // Rysujemy układ współrzędnych 
            GL.Begin(BeginMode.Lines);

            for (int i = 0; i < vertexes.Length; i++)
            {
                GL.Color3(colors[i]);
                GL.Vertex3(vertexes[i]);

            }
            GL.End(); // Koniec rysowania
        }

    }

    public abstract class ColoredObject3D : Object3D
    {
        public Vector3 Color = Vector3.Zero;

        public Vector3 ColorToVector(Color color)
        {
            float R, G, B;
            R = color.R / 255.0f;
            G = color.G / 255.0f;
            B = color.B / 255.0f;

            return new Vector3(R, G, B);
        }

    }

    public abstract class Object3D
    {
        // Pozycja obiektu w ukłądzie współrzędnych 
        public Vector3 Position = Vector3.Zero;

        // Rotacja obiektu względem układu współrzędnych
        public Vector3 Rotation = Vector3.Zero;

        // Skala obiektu (jego wielkość), domyślnie jednostkowa
        public Vector3 Scale = Vector3.One;

        // Macierz modelowania obiektu
        public Matrix4 ModelMatrix = Matrix4.Identity;

        // Funkcja rysująca obiekt
        public abstract void Draw();
        // Funkcja zwracająca vertexy tworzące obiekt 
        public abstract Vector3[] GetVerts();

        // Funkcja zwracająca kolory vertexów
        public abstract Vector3[] GetColorData();

        // Funkcja przelicza macierz modelowania
        public void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale)
                * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z)
                * Matrix4.CreateTranslation(Position);
        }



    }
}
