//#define CUBE
//#define CUBEMAP
//#define MODELCLK
#define FONT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDXTest;
using SharpHelper;

using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace Platform
{
  public static class Program
  {
    //Data
    struct Data
    {
      public Matrix world;
      public Matrix worldViewProjection;
      public Vector4 lightDirection;
      public Vector4 cameraPosition;

      public Matrix mat1;
      public Matrix mat2;
      public Matrix mat3;
      public Matrix mat4;
      public Matrix mat5;
      public Matrix mat6;
    }

    static MMDModel model = new MMDModel(@"../../mikuCSV.csv");
    static DraggableAxis axis ;
    static Vector2 clicked = Vector2.Zero;
    static RayWrap CameraRay;

    public static SharpFPS FpsCounter = new SharpFPS();
    public static VDBDebugger debug;
    public static TrackBallCamera Camera = new TrackBallCamera(new Vector3(0,10,10) , new Vector3(0,10,0));
    //static TrackBallCamera camera = new TrackBallCamera(new Vector3(0,5,5) , new Vector3(0,0,0));
    static Matrix Projection;
    static Matrix View;
    static Mouse mouse = new Mouse();
    static RenderForm Form;

    static void PostViewUpdate(SharpDevice device)
    {
#if FONT
          //begin drawing text
          device.Font.Begin();

          //draw string
          FpsCounter.Update();
          device.Font.DrawString("FPS: " + FpsCounter.FPS, 0, 0);
          var startRay = CordUtil.ScreenToWorld(clicked, 0, new Vector2(Form.Size.Width, Form.Size.Height), View, Projection);
          var endRay = CordUtil.ScreenToWorld(clicked, 1, new Vector2(Form.Size.Width, Form.Size.Height), View, Projection);

          
          var viewport = new ViewportF(0, 0, Form.Size.Width, Form.Size.Height);
          CameraRay = //new RayWrap(startRay, endRay);
            new RayWrap(Ray.GetPickRay((int)clicked.X, (int)clicked.Y, viewport, View * Projection));
      var currentRay = new RayWrap(Ray.GetPickRay((int)mouse.Position.X, (int)mouse.Position.Y, viewport, View * Projection));
          device.Font.DrawString("mouse: " + CameraRay.From, 0, 30);
          device.Font.DrawString("mouseto: " + CameraRay.To, 0, 60);
          debug.vdb_label("model");
          debug.Send(axis.ModelStr);
          debug.vdb_label("ray");
          debug.Send(CameraRay.RayStr);
#if MODELCLK
          var hits = model.HitPos(CameraRay).ToArray();
          for (int j = 0; j < hits.Count(); j++)
          {
            debug.vdb_label("hit");
            debug.Send(hits[j].HitPosition.DebugStr());
            device.Font.DrawString("hit: " + hits[j].Info, 0, 90 + 30 * j);
            if (j == 0)
            {
              model.ToSphere(hits[0].HitPosition);
            }
          }

#endif
          var hits = axis.HitPos(CameraRay).ToArray();
          for (int j = 0; j < hits.Count(); j++)
          {
            debug.vdb_label("hit");
            debug.Send(hits[j].HitPosition.DebugStr());
            device.Font.DrawString("hit: " + hits[j].Info, 0, 90 + 30 * j);
          }
        axis.OnClicked(mouse,currentRay);

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
#if false
      var r = (90.0f).Rad();
      var te = new Quaternion(new Vector3(1, 0, 0), r);
      var ro = te.EulerAngle();
      var rr = new Vector3(r , 0, 0).ToQuaternion();
      var rrrr= ro = rr.EulerAngle();
#endif
      if (!SharpDevice.IsDirectX11Supported())
      {
        System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
        return;
      }

      //render form
      Form = new RenderForm();
      Form.Text = "Tutorial 16: Environment Mapping";
      //frame rate counter
#region 
      Form.MouseClick += Form_MouseClick;
      Form.MouseMove += Form_MouseMove;
      Form.MouseWheel += Form_MouseWheel;
      axis = new DraggableAxis("axis/axisHead.csv");
      debug = new VDBDebugger();
#endregion
      using (SharpDevice device = new SharpDevice(Form))
      {
        //load model from wavefront obj file
#if CUBEMAP
        SharpMesh teapot = SharpMesh.CreateFromObj(device, "../../teapot.obj");
        //init shader
        SharpShader cubeMapPass = new SharpShader(device, "../../HLSLEnv.txt",
            new SharpShaderDescription() { VertexShaderFunction = "VS", GeometryShaderFunction = "GS", PixelShaderFunction = "PS" },
            new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
            });
        //second pass
        SharpShader standard = new SharpShader(device, "../../HLSLEnv.txt",
            new SharpShaderDescription() { VertexShaderFunction = "VS_SECOND", PixelShaderFunction = "PS_SECOND" },
            new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
            });
#else
#endif
        model.LoadTexture(device);
        axis.LoadTexture(device);
#if CUBE

        //second pass
        SharpShader reflection = new SharpShader(device, "../../HLSLEnv.txt",
            new SharpShaderDescription() { VertexShaderFunction = "VS_SECOND", PixelShaderFunction = "PS_REFLECTION" },
            new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
            });

#endif
        // おそらく render target 作るか　tut4シェーダを入れ替えれば描画できるようになる
#if CUBEMAP
        //render target
        SharpCubeTarget cubeTarget = new SharpCubeTarget(device, 512, Format.R8G8B8A8_UNorm);

        //init constant buffer
        Buffer11 dataConstantBuffer = cubeMapPass.CreateBuffer<Data>();
#else

#endif
        //init frame counter
        FpsCounter.Reset();
        device.SetBlend(BlendOperation.Add, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha);


        //main loop
        RenderLoop.Run(Form, () =>
        {
          //Resizing
          if (device.MustResize)
          {
            device.Resize();
          }

          //apply states
          device.UpdateAllStates();
          PreViewUpdate(device);

          //MATRICES

          //set transformation matrix
          float ratio = (float)Form.ClientRectangle.Width / (float)Form.ClientRectangle.Height;
          //90° degree with 1 ratio
          Projection = Matrix.PerspectiveFovLH(3.14F / 2.0F, 1, 1F, 10000.0F);

          //camera
#if false
          Vector3 from = new Vector3(0, 30, 70);
          Vector3 to = new Vector3(0, 0, 0);

          Matrix view = Matrix.LookAtLH(from, to, Vector3.UnitY);
#else
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

#endif
          Matrix world = Matrix.Translation(0, 0, 50) * Matrix.RotationY(Environment.TickCount / 1000.0F);

          //light direction
          Vector3 lightDirection = new Vector3(0.5f, 0, -1);
          lightDirection.Normalize();
#if CUBEMAP
          //six axis 
          Matrix view1 = Matrix.LookAtLH(new Vector3(), new Vector3(1, 0, 0), Vector3.UnitY);
          Matrix view2 = Matrix.LookAtLH(new Vector3(), new Vector3(-1, 0, 0), Vector3.UnitY);
          Matrix view3 = Matrix.LookAtLH(new Vector3(), new Vector3(0, 1, 0), -Vector3.UnitZ);
          Matrix view4 = Matrix.LookAtLH(new Vector3(), new Vector3(0, -1, 0), Vector3.UnitZ);
          Matrix view5 = Matrix.LookAtLH(new Vector3(), new Vector3(0, 0, 1), Vector3.UnitY);
          Matrix view6 = Matrix.LookAtLH(new Vector3(), new Vector3(0, 0, -1), Vector3.UnitY);



          //BEGIN RENDERING TO CUBE TEXTURE 

          cubeTarget.Apply();
          cubeTarget.Clear(Color.CornflowerBlue);


          Data sceneInformation = new Data()
          {
            world = world,
            worldViewProjection = world * View * Projection,
            lightDirection = new Vector4(lightDirection, 1),
            cameraPosition = new Vector4(from, 1),
            mat1 = world * view1 * Projection,
            mat2 = world * view2 * Projection,
            mat3 = world * view3 * Projection,
            mat4 = world * view4 * Projection,
            mat5 = world * view5 * Projection,
            mat6 = world * view6 * Projection
          };
          //write data inside constant buffer
          device.UpdateData<Data>(dataConstantBuffer, sceneInformation);


          //apply shader
          cubeMapPass.Apply();

          //apply constant buffer to shader
          device.DeviceContext.VertexShader.SetConstantBuffer(0, dataConstantBuffer);
          device.DeviceContext.GeometryShader.SetConstantBuffer(0, dataConstantBuffer);
          device.DeviceContext.PixelShader.SetConstantBuffer(0, dataConstantBuffer);
          //draw mesh
          teapot.Begin();
          for (int i = 0; i < teapot.SubSets.Count; i++)
          {
            device.DeviceContext.PixelShader.SetShaderResource(0, teapot.SubSets[i].DiffuseMap);
            // 映り込み
            teapot.Draw(i);
            //teapot.VertexBuffer.
          }
          Projection.Invert();
#region mmdModel

          teapot.Set(model.Vertice, model.Index);
#endregion
#endif


          //RENDERING TO DEVICE

          //Set original targets
          device.SetDefaultTargers();

          //clear color
          device.Clear(Color.Brown);
#if CUBEMAP
          standard.Apply();
#else
          //apply shader
          model.Update(device, View * Projection);
          axis.Update(device, View * Projection);
#endif
#if CUBE


          //set target
          device.DeviceContext.PixelShader.SetShaderResource(1, cubeTarget.Resource);


          Projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 10000.0F);
          sceneInformation = new Data()
          {
            world = world,
            worldViewProjection = world * View * Projection,
            lightDirection = new Vector4(lightDirection, 1),
            cameraPosition = new Vector4(from, 1),
            mat1 = world * view1 * Projection,
            mat2 = world * view2 * Projection,
            mat3 = world * view3 * Projection,
            mat4 = world * view4 * Projection,
            mat5 = world * view5 * Projection,
            mat6 = world * view6 * Projection
          };
          //write data inside constant buffer
          device.UpdateData<Data>(dataConstantBuffer, sceneInformation);

          //room
          teapot.Begin();
          for (int i = 0; i < teapot.SubSets.Count; i++)
          {
            device.DeviceContext.PixelShader.SetShaderResource(0, teapot.SubSets[i].DiffuseMap);
            // 周りの黒いモデル
            teapot.Draw(i);
          }



          //apply shader
          reflection.Apply();


          sceneInformation = new Data()
          {
            world = Matrix.Identity,
            worldViewProjection = View * Projection,
            lightDirection = new Vector4(lightDirection, 1),
            cameraPosition = new Vector4(from, 1),
            mat1 = Matrix.Identity,
            mat2 = Matrix.Identity,
            mat3 = Matrix.Identity,
            mat4 = Matrix.Identity,
            mat5 = Matrix.Identity,
            mat6 = Matrix.Identity
          };
          //write data inside constant buffer
          device.UpdateData<Data>(dataConstantBuffer, sceneInformation);
          //draw mesh
          teapot.Begin();
          for (int i = 0; i < teapot.SubSets.Count; i++)
          {
            device.DeviceContext.PixelShader.SetShaderResource(0, teapot.SubSets[i].DiffuseMap);
            // 中央のモデル
            teapot.Draw(i);
          }
#else
#endif
          PostViewUpdate(device);

          //present
          device.Present();

        });


        //release resource
#if CUBEMAP
        teapot.Dispose();
        dataConstantBuffer.Dispose();

        cubeMapPass.Dispose();
        standard.Dispose();
#else
#endif
        model.Dispose();
        axis.Dispose();
#if CUBE
        reflection.Dispose();

        cubeTarget.Dispose();
#endif


      }
    }

    private static void Form_MouseWheel(object sender, MouseEventArgs e)
    {
      mouse.OnMouseMove(e);
    }

    private static void Form_MouseMove(object sender, MouseEventArgs e)
    {
      var form = Form;
      Vector2 screenPos = new Vector2(e.X, e.Y);
      var startRay = CordUtil.ScreenToWorld(screenPos, 0, new Vector2(form.Size.Width, form.Size.Height), View, Projection);

      var viewport = new ViewportF(0, 0, form.Size.Width, form.Size.Height);
      mouse.IsMoved = true;
      mouse.Clicked = e.Button == MouseButtons.Left;
      mouse.RightClicked = e.Button == MouseButtons.Right;
      if (mouse.Clicked)
      {
        mouse.LastClickedPos = mouse.Position;
        mouse.LastClickedWorldPos = mouse.WorldPosition;
      }
      else
      {

      }
      mouse.Position = screenPos;
      mouse.WorldPosition = startRay;
      //System.Diagnostics.Debug.WriteLine(startRay + " start");
      //System.Diagnostics.Debug.WriteLine(mouse.LastClickedWorldPos + " last");
      mouse.OnMouseMove(e);
    }

    private static void Form_MouseClick(object sender, MouseEventArgs e)
    {
      clicked = new Vector2(e.X, e.Y);
      //System.Diagnostics.Debug.WriteLine(clicked);
    }
  }
}

