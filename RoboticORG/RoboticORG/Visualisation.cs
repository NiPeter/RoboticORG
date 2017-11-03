using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RoboticORG
{
    class Visualisation : GameWindow
    {

        #region variables

        Camera cam = new Camera();
        List<Object3D> DrawList = new List<Object3D>();
        //List<Robot>Robots = new List<Robot>();
        Robot Puma5230_Arm;

        double[] q = { 40, -60, 55, 2, -30 }; // zmienne pomocnicze do sterowania robota
        double ads = 1;
        #endregion variables


        public Visualisation() : base(700, 700, new GraphicsMode(32, 24, 0, 4))
        {
            base.WindowBorder = WindowBorder.Fixed; // Brak możliwości zmiany rozmiaru okna
            Title = "RoboticORG!";    // Ustaw tytuł okienka
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.Enable(EnableCap.DepthTest); // Sprawdzaj które obiekty powinny być wyświetlane przed innymi - depthtest
            GL.ClearColor(Color.WhiteSmoke); // ustaw kolor tła

            // Dodawanie elementów sceny
            DrawList.Add(new CS());
            DrawList[DrawList.Count - 1].Scale = new Vector3((float)(150 / 10.0));

            DrawList.Add(new Ground(Color.GhostWhite, 150));
            
            // Dodawanie robotów

            //DH dh = new DH();
            //dh.Add(new Revolute(90, 0, 0, 0));
            //dh.Add(new Revolute(0, 0, 0, 0));
            //dh.Add(new Revolute(0, 5, 0, 0));
            //dh.Add(new Revolute(0, 5, 0, 0));

            //Robot SimpleArm = new Robot(dh);
            //SimpleArm.Position.X = 10.0f;
            //SimpleArm.Position.Y = 23.0f;
            //SimpleArm.Rotation.Z = 1f;
            //DrawList.Add(SimpleArm);

            DH dhPuma = new DH();
            dhPuma.Add(new Revolute(0, 0, 0, q[0]));
            dhPuma.Add(new Revolute(-90, 0, 0, q[1]));
            dhPuma.Add(new Revolute(0, 2, 0, q[2]));
            dhPuma.Add(new Joint(-90, 0, 0, 90));
            dhPuma.Add(new Joint(90, 0, 0, 0));
            dhPuma.Add(new Translational(0, 0, q[3], 0));
            dhPuma.Add(new Joint(-90, 0, 0, -90));
            dhPuma.Add(new Revolute(-90, 0, 0, q[4]));
            dhPuma.Add(new Joint(0, 2, 0, 0));


            Puma5230_Arm = new Robot(dhPuma);
            DrawList.Add(Puma5230_Arm);

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Wyczyść aktualny bufor

            // Rysuj wszystkie obiekty z DrawList
            foreach (Object3D vol in DrawList) vol.Draw();

            SwapBuffers();  // Zamień bufory
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Pobierz aktualną macierz projekcji z kamery i załaduj na GPU
            Matrix4 ViewProjectionMatrix = cam.GetViewMatrix();
            LoadProjectionMatrix(ref ViewProjectionMatrix);

            // W tym miejscu powinna znaleźć się logika i sterowanie
            Puma5230_Arm.SetJointPos(q); 
        }

        #region Mouse Events
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            cam.MouseDown(e);

        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            cam.MouseWheel(e);

        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            cam.KeyDown(e);

            switch (e.Key)
            {
                case Key.Keypad1:
                    q[0] += ads;
                    break;
                case Key.Keypad2:
                    q[0] -= ads;
                    break;
                case Key.Keypad3:
                    q[1] += ads;
                    break;
                case Key.Keypad4:
                    q[1] -= ads;
                    break;
                case Key.Keypad5:
                    q[2] += ads;
                    break;
                case Key.Keypad6:
                    q[2] -= ads;
                    break;
                case Key.Keypad7:
                    q[3] += 0.1 * ads;
                    break;
                case Key.Keypad8:
                    q[3] -= 0.1 * ads;
                    break;
                case Key.Keypad9:
                    q[4] += ads;
                    break;
                case Key.Keypad0:
                    q[4] -= ads;
                    break;
                default:
                    break;
            }
            if (q[3] < 0.1) q[3] = 0.1;
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            cam.MouseUpdate();
        }
        #endregion Mouse Events

        #region Functions
        void LoadProjectionMatrix(ref Matrix4 ViewProjectionMatrix)
        {
            // Załaduj macierz projekcji na GPU
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref ViewProjectionMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
        }
        #endregion Functions

    }


}
