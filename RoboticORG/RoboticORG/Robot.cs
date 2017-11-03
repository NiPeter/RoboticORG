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
using MathNet.Spatial;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace RoboticORG
{
    class Robot : ColoredObject3D
    {
        public DH DHTable;

        public Robot()
        {
            DHTable = new DH();
            Color = ColorToVector(System.Drawing.Color.Black);
        }

        public Robot(DH dh)
        {
            DHTable = dh;
            Color = ColorToVector(System.Drawing.Color.Black);
        }

        #region Dziedziczone z klasy abstrakcyjnej
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

        public override Vector3[] GetColorData()
        {
            List<Vector3> colors = new List<Vector3>();

            for (int i = 0; i < DHTable.Count * 2; i++)
            {
                colors.Add(Color);
            }

            return colors.ToArray();
        }

        public override Vector3[] GetVerts()
        {
            List<Vector3> verts = new List<Vector3>();

            for (int i = 0; i < DHTable.Count - 1; i++) // Dla każdego członu dwa punkty, nie wliczamy członu "ground"
            {
                double[] P = DHTable.GetTformMatrix(0, i).Column(3, 0, 3).ToArray();
                verts.Add(new Vector3((float)P[0], (float)P[1], (float)P[2]));

                P = DHTable.GetTformMatrix(0, i + 1).Column(3, 0, 3).ToArray();
                verts.Add(new Vector3((float)P[0], (float)P[1], (float)P[2]));
            }

            return verts.ToArray();
        }
        #endregion

        // Funkcja ustawia kąty na przegubach
        public void SetJointPos(double[] jointPos)
        {
            Matrix<double> M = DenseMatrix.CreateDiagonal(4, 4, 1);

            int index = 0;
            for (int i = 0; i < DHTable.Count; i++)
            {
                if (DHTable[i] is IControlable) // Jeżeli człon jest IControlable to ustaw jego pozycję
                {
                    IControlable ic = (IControlable)DHTable[i];
                    if (index < jointPos.Length) ic.Position(jointPos[index++]);
                }
            }
        }


        public override string ToString()
        {
            return DHTable.ToString();
        }

    }

    class DH : List<Joint>
    {

        public DH() : base()
        {
            Add(0, 0, 0, 0);
        }

        public DH(double alpha, double transX, double transZ, double theta) : base()
        {
            Add(0, 0, 0, 0);
            Add(alpha, transX, transZ, theta);
        }

        // Dodaje człon na podstawie parametrów DH członu
        public void Add(double alpha, double transX, double transZ, double theta)
        {
            Add(new Joint(alpha, transX, transZ, theta));
        }

        public override string ToString()
        {
            string str = "";
            int i = 0;
            foreach (Joint j in this)
            {
                str = str + i++ + " " + j.ToString() + "\r\n";
            }

            return str;
        }

        public Matrix<double> GetTformMatrix(int from, int to)
        {
            Matrix<double> Tform = DenseMatrix.CreateDiagonal(4, 4, 1);
            if (from <= to)
            {
                for (int i = from + 1; i <= to; i++)
                {
                    Tform = Tform * base[i].GetTformMatrix();
                }
            }
            else
            {
                for (int i = to + 1; i <= from; i++)
                {
                    Tform = Tform * base[i].GetTformMatrix();
                }
                Tform = Tform.Inverse();
            }

            return Tform;
        }

    }

    class Translational : Joint, IControlable
    {
        public Translational(double alpha, double transX, double transZ, double theta) : base(alpha, transX, transZ, theta)
        {
        }

        public void Position(double pos)
        {
            base.transZ = pos;
        }
        public override string ToString()
        {
            return "Translational " + base.ToString();
        }
    }
    class Revolute : Joint, IControlable
    {
        public Revolute(double alpha, double transX, double transZ, double theta) : base(alpha, transX, transZ, theta)
        {
        }

        public void Position(double pos)
        {
            base.theta = pos;
        }

        public override string ToString()
        {
            return "Revolute " + base.ToString();
        }

    }

    interface IControlable
    {
        // Ustawia pozycję członu
        void Position(double pos);
    }

    class Joint
    {
        // Klasę należy traktować jak człon robota, bądź linijkę w tabelce DH

        protected double alpha;
        protected double transX;
        protected double transZ;
        protected double theta;

        public Joint(double alpha, double transX, double transZ, double theta)
        {
            this.alpha = alpha;
            this.transX = transX;
            this.transZ = transZ;
            this.theta = theta;
        }

        public override string ToString()
        {
            return "Joint: alpha = " + alpha + ", X = " + transX + ", Z = " + transZ + ", theta = " + theta;
        }

        public DenseMatrix GetTformMatrix()
        {
            DenseMatrix A = DenseMatrix.CreateDiagonal(4, 4, 1);
            A = (CoordinateSystem.Roll(alpha, AngleUnit.Degrees) * CoordinateSystem.Translation(new Vector3D(transX, 0, transZ)) * CoordinateSystem.Yaw(theta, AngleUnit.Degrees));
            return A;
        }

    }
}
