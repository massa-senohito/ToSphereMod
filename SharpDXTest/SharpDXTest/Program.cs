//#define CUBE
//#define CUBEMAP
//#define MODELCLK
//#define DEBUGLINE
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

    static MMDModel model = new MMDModel(@"miku/mikuCSV.csv");
    static DraggableAxis axis = new DraggableAxis("axis/axis.csv");
    static DebugLine line = new DebugLine("line/line.csv");
    static Vector2 clicked = Vector2.Zero;
    static RayWrap CameraRay;

    public static SharpFPS FpsCounter = new SharpFPS();
    public static VDBDebugger debug;
    public static TrackBallCamera Camera = new TrackBallCamera(new Vector3(0,10,10) , new Vector3(0,10,0));
    //static TrackBallCamera camera = new TrackBallCamera(new Vector3(0,5,5) , new Vector3(0,0,0));
    static Matrix Projection;
    static Matrix View;
    static ViewportF Viewport;

    static Mouse mouse = new Mouse();
    static RenderForm Form;
    static BlenderModifier.SphereModForm ModForm = new BlenderModifier.SphereModForm();

    static void OnResizeForm(float ratio, SharpDevice device)
    {
      var viewports = device.DeviceContext.Rasterizer.GetViewports<ViewportF>();
      Viewport = viewports[0];
      Camera.OnResize(ratio);
    }

    static void PostViewUpdate(SharpDevice device)
    {
#if FONT
          //begin drawing text
          device.Font.Begin();

          //draw string
          FpsCounter.Update();
          device.Font.DrawString("FPS: " + FpsCounter.FPS, 0, 0);

          CameraRay = new RayWrap(Ray.GetPickRay((int)clicked.X, (int)clicked.Y, Viewport, View * Projection));
      var currentRay = new RayWrap(Ray.GetPickRay((int)mouse.Position.X, (int)mouse.Position.Y, Viewport, View * Projection));
          device.Font.DrawString("mouse: " + CameraRay.From, 0, 30);
          device.Font.DrawString("mouseto: " + CameraRay.To, 0, 60);
          debug.vdb_label("model");
          debug.Send(axis.ModelStr);
          debug.vdb_label("ray");
          debug.Send(CameraRay.RayStr);

          var hits = axis.HitPos(CameraRay).ToArray();
          for (int j = 0; j < hits.Count(); j++)
          {
            debug.vdb_label("hit");
            debug.Send(hits[j].HitPosition.DebugStr());
            device.Font.DrawString("hit: " + hits[j].Info, 0, 90 + 30 * j);
          }
          axis.OnClicked(mouse,currentRay);
#if DEBUGLINE
      line.OnClicked(mouse, currentRay);
#endif
      //flush text to view
      device.Font.End();
#endif
    }

    static void PreViewUpdate(SharpDevice device)
    {

    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

      if (!SharpDevice.IsDirectX11Supported())
      {
        System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
        return;
      }

      //render form
      Form = new RenderForm();
      Form.Text = "Tutorial 16: Environment Mapping";

      ModForm.Show();
      ModForm.SetFactorBoxChanged( OnFactorTextChanged );
      ModForm.SetRadiusBoxChanged( OnFactorTextChanged );
      //frame rate counter
#region 
      Form.MouseClick += Form_MouseClick;
      Form.MouseMove += Form_MouseMove;
      Form.MouseWheel += Form_MouseWheel;
      debug = new VDBDebugger();
#endregion
      using (SharpDevice device = new SharpDevice(Form))
      {

        model.LoadTexture(device);
        axis.LoadTexture(device);
#if DEBUGLINE
        line.LoadTexture(device);
        line.AfterLoaded();
#endif

        //init frame counter
        FpsCounter.Reset();
        device.SetBlend(BlendOperation.Add, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha);
        OnResizeForm((float)Form.ClientRectangle.Width / Form.ClientRectangle.Height, device);

        //main loop
        RenderLoop.Run(Form, () =>
        {
          //set transformation matrix
          float ratio = (float)Form.ClientRectangle.Width / Form.ClientRectangle.Height;
          //90° degree with 1 ratio
          Projection = Camera.Projection;
          //Resizing
          if (device.MustResize)
          {
            device.Resize();
            OnResizeForm(ratio, device);

          }

          //apply states
          device.UpdateAllStates();
          PreViewUpdate(device);

          //MATRICES

          //camera

          View = Camera.GetView();
          //View = Matrix.LookAtLH(new Vector3(0, 30, 70), new Vector3(0, 0, 0), Vector3.UnitY);
          Camera.Update(mouse,FpsCounter.Delta*0.001f);
          mouse.Update();
          Vector3 from = Camera.Position;
          if (!float.IsNaN(from.X))
          {
            debug.vdb_label("campos");
            debug.Send(from.DebugStr());
          }

          Matrix world = Matrix.Translation(0, 0, 50) * Matrix.RotationY(Environment.TickCount / 1000.0F);

          //light direction
          Vector3 lightDirection = new Vector3(0.5f, 0, -1);
          lightDirection.Normalize();

          //RENDERING TO DEVICE

          //Set original targets
          device.SetDefaultTargers();

          //clear color
          device.Clear(Color.Brown);

          //apply shader
          model.Update(device, View * Projection);
          axis.Update(device, View * Projection);
#if DEBUGLINE
          line.Update(device, View * Projection);
#endif
          model.ToSphere(axis.Position);

          PostViewUpdate(device);

          //present
          device.Present();

        });


        //release resource

        model.Dispose();
        axis.Dispose();
#if DEBUGLINE
        line.Dispose();
#endif



      }
    }

    private static void Form_MouseWheel(object sender, MouseEventArgs e)
    {
      var startRay = new RayWrap(Ray.GetPickRay( e.X, e.Y, Viewport, View * Projection));
      mouse.OnMouseMove(e,startRay);
    }

    private static void Form_MouseMove(object sender, MouseEventArgs e)
    {
      var startRay = new RayWrap(Ray.GetPickRay( e.X, e.Y, Viewport, View * Projection));
      mouse.OnMouseMove(e,startRay);
    }

    private static void Form_MouseClick(object sender, MouseEventArgs e)
    {
      clicked = new Vector2(e.X, e.Y);
    }

    private static void OnFactorTextChanged(object sender,EventArgs e)
    {
      model.OnFactorChanged(ModForm.Factor);
      model.OnRadiusChanged(ModForm.Radius);
    }
  }
}

