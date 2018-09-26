//#define CUBE
//#define CUBEMAP
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

namespace Tutorial16
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

    static MMDModel model;
    static Vector2 clicked = Vector2.Zero;
    static RayWrap CameraRay;
    public static VDBDebugger debug;
    static TrackBallCamera camera = new TrackBallCamera(new Vector3(0,10,30) , new Vector3(0,10,0));
    static Matrix Projection;
    static Matrix View;
    static Mouse mouse =new Mouse();
    static RenderForm Form;
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
      SharpFPS fpsCounter = new SharpFPS();
#region 
      Form.MouseClick += Form_MouseClick;
      Form.MouseMove += Form_MouseMove;
      model = new MMDModel(@"../../mikuCSV.csv");
      debug = new VDBDebugger();
      ViewportF viewport;
#endregion
      using (SharpDevice device = new SharpDevice(Form))
      {
        //load model from wavefront obj file
        SharpMesh teapot = SharpMesh.CreateFromObj(device, "../../teapot.obj");
#if CUBEMAP
        //init shader
        SharpShader cubeMapPass = new SharpShader(device, "../../HLSL.txt",
            new SharpShaderDescription() { VertexShaderFunction = "VS", GeometryShaderFunction = "GS", PixelShaderFunction = "PS" },
            new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
            });
#endif
        //second pass
        SharpShader standard = new SharpShader(device, "../../HLSL.txt",
            new SharpShaderDescription() { VertexShaderFunction = "VS_SECOND", PixelShaderFunction = "PS_SECOND" },
            new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
            });
#if CUBE

        //second pass
        SharpShader reflection = new SharpShader(device, "../../HLSL.txt",
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
        fpsCounter.Reset();


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
          View = camera.GetView();
          //View = Matrix.LookAtLH(new Vector3(0, 30, 70), new Vector3(0, 0, 0), Vector3.UnitY);
          camera.Update(mouse,fpsCounter.Delta*0.001f);
          mouse.Update();
          //camera.Move(new Vector3(0.1f, 0, 0));
          Vector3 from = View.TranslationVector;
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

#endif
#region mmdModel

          teapot.Set(model.Vertice, model.Index);
#endregion

          //RENDERING TO DEVICE

          //Set original targets
          device.SetDefaultTargers();

          //clear color
          device.Clear(Color.Brown);
          //apply shader
          standard.Apply();
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

#else
#endif
          //draw mesh
          teapot.Begin();
          for (int i = 0; i < teapot.SubSets.Count; i++)
          {
            device.DeviceContext.PixelShader.SetShaderResource(0, teapot.SubSets[i].DiffuseMap);
            // 中央のモデル
            teapot.Draw(i);
          }

#if FONT
          //begin drawing text
          device.Font.Begin();

          //draw string
          fpsCounter.Update();
          device.Font.DrawString("FPS: " + fpsCounter.FPS, 0, 0);
          var startRay = CordUtil.ScreenToWorld(clicked, 0, new Vector2(Form.Size.Width, Form.Size.Height), View, Projection);
          var endRay = CordUtil.ScreenToWorld(clicked, 1, new Vector2(Form.Size.Width, Form.Size.Height), View, Projection);

          

          device.Font.DrawString("mouse: " + startRay, 0, 30);
          device.Font.DrawString("mouseto: " + endRay, 0, 60);
          viewport = new ViewportF(0, 0, Form.Size.Width, Form.Size.Height);
          CameraRay = //new RayWrap(startRay, endRay);
            new RayWrap(Ray.GetPickRay((int)clicked.X, (int)clicked.Y, viewport, View * Projection));
          debug.vdb_label("model");
          debug.Send(model.ModelStr);
          debug.vdb_label("ray");
          debug.Send(CameraRay.RayStr);

          var hits = model.HitPos(CameraRay).ToArray();
          for (int j = 0; j < hits.Count(); j++)
          {
            debug.vdb_label("hit");
            debug.Send(hits[j].DebugStr());
            device.Font.DrawString("hit: " + hits[j], 0, 90 + 30 * j);
            if (j == 0)
            {
              model.ToSphere(hits[0]);
            }
          }
          //flush text to view
          device.Font.End();
#endif
          //present
          device.Present();

        });


        //release resource
        teapot.Dispose();
#if CUBEMAP
        dataConstantBuffer.Dispose();

        cubeMapPass.Dispose();
#endif
        standard.Dispose();
#if CUBE
        reflection.Dispose();

        cubeTarget.Dispose();
#endif


      }
    }

    private static void Form_MouseMove(object sender, MouseEventArgs e)
    {
      var form = Form;
      Vector2 screenPos = new Vector2(e.X, e.Y);
      var startRay = CordUtil.ScreenToWorld(screenPos, 0, new Vector2(form.Size.Width, form.Size.Height), View, Projection);

      var viewport = new ViewportF(0, 0, form.Size.Width, form.Size.Height);
      mouse.IsMoved = true;
      mouse.Clicked = e.Button == MouseButtons.Left;
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
    }

    private static void Form_MouseClick(object sender, MouseEventArgs e)
    {
      clicked = new Vector2(e.X, e.Y);
      //System.Diagnostics.Debug.WriteLine(clicked);
    }
  }
}

