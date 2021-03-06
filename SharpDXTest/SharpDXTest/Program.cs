﻿//#define CUBE
//#define CUBEMAP
//#define MODELCLK
//#define DEBUGLINE
#define ENABLE_SPHERE
#define FONT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDXTest;
using SharpHelper;


namespace Platform
{
    public static class Program
    {

        public static MMDModel Model;
        public static MMDModel RefModel;
        static PMXLoader LoadedFromDialog;
        static DraggableAxis Axis = new DraggableAxis( "axis/axis.csv" );
        static DebugLine Line = new DebugLine( "line/line.csv" );
        static Vector2 Clicked = Vector2.Zero;
        static RayWrap CameraRay;

        public static SharpFPS FpsCounter = new SharpFPS( );
        public static VDBDebugger Debug;
        public static TrackBallCamera Camera = new TrackBallCamera( new Vector3( 0 , 10 , 10 ) , new Vector3( 0 , 10 , 0 ) );
        //static TrackBallCamera camera = new TrackBallCamera(new Vector3(0,5,5) , new Vector3(0,0,0));
        static Matrix Projection;
        static Matrix View;
        static ViewportF Viewport;

        static Mouse Mouse = new Mouse( );
        static RenderForm Form;
        static BlenderModifier.SphereModForm ModForm;
        static LatticeForm LatticeForm;
        static VertMorphViewer MorphViewer;

        static void OnResizeForm( float ratio , SharpDevice device )
        {
            ViewportF[] viewports = device.DeviceContext.Rasterizer.GetViewports<ViewportF>( );
            Viewport = viewports[ 0 ];
            Camera.OnResize( ratio );
        }

        static void PostViewUpdate( SharpDevice device )
        {
            FpsCounter.Update( );
            CameraRay = new RayWrap( Ray.GetPickRay( ( int )Clicked.X , ( int )Clicked.Y , Viewport , View * Projection ) );
            RayWrap currentRay = new RayWrap( Ray.GetPickRay( ( int )Mouse.Position.X , ( int )Mouse.Position.Y , Viewport , View * Projection ) );
#if FONT
            // 最小化したときフォントがなくなることがあった
            device.Font.Begin( );

            //draw string
            device.Font.DrawString( "FPS: " + FpsCounter.FPS , 0 , 0 );

            device.Font.DrawString( "mouse: " + CameraRay.From , 0 , 30 );
            device.Font.DrawString( "mouseto: " + CameraRay.To , 0 , 60 );
            //Debug.vdb_label( "model" );
            //Debug.Send( Axis.ModelStr );
            Debug.vdb_label( "ray" );
            Debug.Send( CameraRay.RayStr );

#endif
            var hits = Axis.HitPos( CameraRay ).ToArray( );
            for ( int j = 0 ; j < hits.Count( ) ; j++ )
            {
                Debug.vdb_label( "hit" );
                Debug.Send( hits[ j ].HitPosition.DebugStr( ) );
#if FONT
                device.Font.DrawString( "hit: " + hits[ j ].Info , 0 , 90 + 30 * j );
#endif
            }
            Axis.OnClicked( Mouse , currentRay );
            //Axis.Scale = new Vector3( 0.5f );
            ModForm.SetOffset( Axis.Position );
            ModForm.EulerRotate = Axis.Rotation.EulerAngle( );
#if DEBUGLINE
            //Line.OnClicked(Mouse, currentRay);
            //Line.SetLine( Vector3.Zero , Axis.RotXFrame);
            //line.SetLine(new Vector3(2,-12,-12), new Vector3(-8,8,10));
#endif

#if ENABLE_SPHERE
            Model.ToSphere( Axis.World );
            Model.ToSphere( Axis.World.InvX( ) , true );
#else
            LatticeForm.FixedUpdate( View , Projection );
            Debug.vdb_label( "lattice" );
            foreach ( var item in  LatticeForm.AllLatticeString)
            {
                //item.DebugWrite( );
                Debug.Send( item );
            }

            LatticeForm.OnClicked( Mouse , currentRay );
#endif

#if FONT
            //flush text to view
            device.Font.End( );
#endif
        }

        static void PreViewUpdate( SharpDevice device )
        {

        }

        public static void NormalStart()
        {
            //render form
            Form = new RenderForm( );
            Form.Text = "MMD Model Viewer";

            ModForm.Show( );
            ModForm.SetFactorBoxChanged( OnFactorTextChanged );
            ModForm.SetRadiusBoxChanged( OnFactorTextChanged );
            ModForm.SetAlphaBarChanged( OnAlphaBarChanged );
            ModForm.SetOffsetBoxChanged( OnOffsetBoxChanged );
            ModForm.SetMorphNameChanged( OnMorphNameChanged );
            ModForm.SetScaleChanged( OnScaleChanged );
            ModForm.SetRotChanged( OnRotTextChanged );

            #region addEvent
            Form.MouseClick += Form_MouseClick;
            Form.MouseMove += Form_MouseMove;
            Form.MouseWheel += Form_MouseWheel;
            Form.KeyUp += Form_KeyUp;
            Form.FormClosed += Form_FormClosed;
            ;
            Debug = new VDBDebugger( );
            #endregion

            using ( SharpDevice device = new SharpDevice( Form ) )
            {
#if Lattice
                LatticeForm = new LatticeForm( Model , device );
                LatticeForm.Show( );
#endif

                Model.LoadTexture( device );
                Axis.LoadTexture( device );
#if Lattice
                LatticeForm.LoadTexture( device );
#endif
#if DEBUGLINE
                Line.LoadTexture(device);
                Line.AfterLoaded();
#endif

                //init frame counter
                FpsCounter.Reset( );
                device.SetBlend( BlendOperation.Add , BlendOption.SourceAlpha , BlendOption.InverseSourceAlpha );
                OnResizeForm( ( float )Form.ClientRectangle.Width / Form.ClientRectangle.Height , device );

                //main loop
                RenderLoop.Run( Form , () => OnUpdate( device ) );

                //release resource

                Model.Dispose( );
                RefModel?.Dispose( );
                Axis.Dispose( );
#if DEBUGLINE
                Line.Dispose();
#endif
            }
        }

        private static void OnRotTextChanged( object sender , EventArgs e )
        {
            Axis.Rotation = ModForm.EulerRotate.QuatFromEuler( );
        }

        private static void OnScaleChanged( object sender , EventArgs e )
        {
            Model.CastScale( ModForm.ToSphereScale );
        }

        private static void Form_FormClosed( object sender , FormClosedEventArgs e )
        {
            ModForm.Save( );
            var morphs = Model.DifferVert( );
            // 枠が不正になる
            LoadedFromDialog.PmxModelData.AddVertMorph( ModForm.MorphName , morphs );
            if ( ModForm.MorphName == "" || ModForm.HasError )
            {
                return;
            }
            LoadedFromDialog.WriteUpdated( );
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main( string[] args )
        {

            if ( !SharpDevice.IsDirectX11Supported( ) )
            {
                MessageBox.Show( "DirectX11 Not Supported" );
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "pmx|*.pmx";
            if ( openFileDialog.ShowDialog( ) == DialogResult.OK )
            {
                string V = openFileDialog.FileName;
                //PMXLoader.WriteTestCSV( V );
                LoadedFromDialog = new PMXLoader( V );
                Model = LoadedFromDialog.MMDModel;
                MorphViewer = new VertMorphViewer( Model.Morphs );
                Model.BindMorphProp( MorphViewer.BarValues );
                MorphViewer.Show( );
                ModForm = new BlenderModifier.SphereModForm( LoadedFromDialog);
                ModForm.OnLoadReferenceModel = OnLoadReferenceModel;
                NormalStart( );
            }

        }
        static void OnLoadReferenceModel( string path )
        {
            var refLoader = new PMXLoader(
               path
            );
            RefModel = refLoader.MMDModel;
        }

        private static void Form_KeyUp( object sender , KeyEventArgs e )
        {
            Util.DebugWrite( e.KeyData.ToString( ) );
        }

        private static void OnUpdate( SharpDevice device )
        {
            // ここでしかdevice手に入らない、まだロードが完全でない
            if ( RefModel != null )
            {
                if ( RefModel.Mesh == null )
                {
                    RefModel.LoadTexture( device );
                }
            }
            //set transformation matrix
            float ratio = ( float )Form.ClientRectangle.Width / Form.ClientRectangle.Height;
            //90° degree with 1 ratio
            Projection = Camera.Projection;
            //Resizing
            if ( device.MustResize )
            {
                device.Resize( );
                OnResizeForm( ratio , device );

            }

            //apply states
            device.UpdateAllStates( );
            PreViewUpdate( device );

            //MATRICES

            //camera

            View = Camera.GetView( );
            //View = Matrix.LookAtLH(new Vector3(0, 30, 70), new Vector3(0, 0, 0), Vector3.UnitY);
            Camera.Update( Mouse , FpsCounter.Delta * 0.001f );
            Mouse.Update( );
            Vector3 from = Camera.Position;
            if ( !float.IsNaN( from.X ) )
            {
                Debug.vdb_label( "campos" );
                Debug.Send( from.DebugStr( ) );
            }

            Matrix world = Matrix.Translation( 0 , 0 , 50 ) * Matrix.RotationY( Environment.TickCount / 1000.0F );

            //light direction
            Vector3 lightDirection = new Vector3( 0.5f , 0 , -1 );
            lightDirection.Normalize( );

            //RENDERING TO DEVICE

            //Set original targets
            device.SetDefaultTargers( );

            //clear color
            device.Clear( Color.Brown );

            //apply shader
            Model.Update( device , View , Projection );
            RefModel?.Update( device , View , Projection );
            Axis.Update( device , View , Projection );

#if DEBUGLINE
            Line.Update(device, View , Projection);
#endif

            PostViewUpdate( device );

            //present
            device.Present( );

        }

        private static void Form_MouseWheel( object sender , MouseEventArgs e )
        {
            RayWrap startRay = new RayWrap( Ray.GetPickRay( e.X , e.Y , Viewport , View * Projection ) );
            Mouse.OnMouseMove( e , startRay );
        }

        private static void Form_MouseMove( object sender , MouseEventArgs e )
        {
            RayWrap startRay = new RayWrap( Ray.GetPickRay( e.X , e.Y , Viewport , View * Projection ) );
            Mouse.OnMouseMove( e , startRay );
        }

        private static void Form_MouseClick( object sender , MouseEventArgs e )
        {
            Clicked = new Vector2( e.X , e.Y );
        }

        private static void OnFactorTextChanged( object sender , EventArgs e )
        {
            Model.OnFactorChanged( ModForm.Factor );
            Model.OnRadiusChanged( ModForm.Radius );
        }

        private static void OnAlphaBarChanged( object sender , EventArgs e )
        {
            Axis.Alpha = ModForm.Alpha;
        }

        private static void OnOffsetBoxChanged( object sender , EventArgs e )
        {
            var offset = ModForm.GetOffset( );
            offset.Match( () => { } , off => Axis.Position = off);
        }

        private static void OnMorphNameChanged( object sender , EventArgs e )
        {
            if ( LoadedFromDialog.HasSameNameMorph( ModForm.MorphName ) )
            {
                ModForm.SetError( "モーフ名が重複" );
            }
            else
            {
                ModForm.SetError( );
            }
        }

    }
}

