using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboticORG
{
    class Camera
    {
        // Orientacja kamery
        OrientationCtrl orientation = new OrientationCtrl();

        // Pozycja kamery
        PositionCtrl position = new PositionCtrl();

        // Aktualny Zoom
        float zoom = 3.4f;
        float zoomSensitivity = 0.8f;

        // Pozycja referencyjna myszki, wykorzystywana wszędzie tam,
        // gdzie potrzebna jest informacja o zmianie pozycji myszki od czasu naciśnięcia przycisku.
        Vector2 refMousePos = new Vector2();

        #region Kontrola pozycji i orientacji
        // TODO Obie kalsy są takie same, zamknąć w jedną klasę i dodać abstrakt
        private class PositionCtrl
        {
            // Wektor aktualnej orientacji ekranu, tzn. kąty obrotu po osiach X Y Z
            public Vector3 Current = new Vector3(0.0f);

            // Wektor orientacji odniesienia 
            public Vector3 Reference = new Vector3(0.0f);

            // Czułość wskaźnika myszy.
            public float sensitivity = 0.0015f;

            public Matrix4 Matrix
            {
                get
                {
                    return Matrix4.CreateTranslation(Current.X, Current.Y, 0);
                }
            }

            public void SetReference()
            {
                Reference = new Vector3(Current);
            }

            public void AddDelta(float x, float y)
            {
                x = -x * sensitivity;
                y = y * sensitivity;

                Current.X = (Reference.X + x);
                Current.Y = (Reference.Y + y);
            }
        }

        private class OrientationCtrl
        {
            // Korekcja układu współrzędnych tak, aby oś Z skierowana była ku górze, a oś X wychodziła z ekranu.
            private Matrix4 Correction = Matrix4.CreateRotationZ((float)(-Math.PI / 2.0)) * Matrix4.CreateRotationX((float)(-Math.PI / 2.0));

            // Wektor aktualnej orientacji ekranu, tzn. kąty obrotu po osiach X Y Z
            public Vector3 Current = new Vector3(0.58f, 0f, -1.32f);

            // Wektor orientacji odniesienia 
            public Vector3 Reference = new Vector3(0.0f);

            // Czułość wskaźnika myszy.
            public float sensitivity = 0.01f;

            public Matrix4 Matrix
            {
                get
                {
                    return Matrix4.CreateRotationZ(Current.Z) * Correction * Matrix4.CreateRotationX(Current.X);
                }
            }

            public void SetReference()
            {
                Reference = new Vector3(Current);
            }

            public void AddDelta(float x, float y)
            {
                x = -x * sensitivity;
                y = -y * sensitivity;

                Current.Z = (Reference.Z + x);
                Current.X = (Reference.X + y);

                if (Current.X < 0) Current.X = 0; else if (Current.X > Math.PI) Current.X = (float)Math.PI;
            }

        }
        #endregion

        #region Mousevents
        public void MouseWheel(MouseWheelEventArgs e)
        {

            zoom += e.DeltaPrecise * zoomSensitivity;

            if (zoom < 0.1) zoom = 0.1f;
        }

        public void MouseDown(MouseButtonEventArgs e)
        {
            KeyboardState keybState = Keyboard.GetState();

            refMousePos.X = OpenTK.Input.Mouse.GetState().X;
            refMousePos.Y = OpenTK.Input.Mouse.GetState().Y;

            if (keybState.IsKeyDown(Key.LShift) && e.Mouse.IsButtonDown(MouseButton.Middle))
            {
                orientation.SetReference();
            }
            else if (e.Mouse.IsButtonDown(MouseButton.Left))
            {
                position.SetReference();
            }
        }

        public void KeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {

                case Key.LShift:

                    break;

                case Key.R:

                    Environment.Exit(Environment.ExitCode);
                    break;

            }
        }

        public void MouseUpdate()
        {
            KeyboardState keybState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 delta = (refMousePos - mousePos);

            if (keybState.IsKeyDown(Key.LShift) && mouseState.IsButtonDown(MouseButton.Middle))
            {

                orientation.AddDelta(delta.X, delta.Y);

            }
            else if (keybState.IsKeyDown(Key.LShift) && mouseState.IsButtonDown(MouseButton.Left) && mouseState.IsButtonDown(MouseButton.Right))
            {
                position.Current = new Vector3(0f);
                position.Reference = new Vector3(0f);

            }
            else if (mouseState.IsButtonDown(MouseButton.Left))
            {
                delta *= zoom;
                position.AddDelta(delta.X, delta.Y);
            }
        }
        #endregion Mousevents


        // Funnkcja  GetViewMatrix zwraca aktualną macierz widoku kamery.
        public Matrix4 GetViewMatrix()
        {
            return orientation.Matrix * position.Matrix * Matrix4.CreateOrthographicOffCenter(-zoom, zoom, -zoom, zoom, -160, 160);
        }


    }
}
