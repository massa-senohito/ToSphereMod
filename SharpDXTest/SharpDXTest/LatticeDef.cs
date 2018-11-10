//#define INTERPOLATION
using Reactive.Bindings;
using SharpDX;
using SharpDXTest;
using SharpHelper;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Matrix4x4 = SharpDX.Matrix;
using Vertex = SharpHelper.TexturedVertex;
using LatticeType = 
	System.Collections.Generic.List<Reactive.Bindings.ReactiveProperty<SharpHelper.TexturedVertex>>;
	//System.Collections.Generic.List<SharpHelper.TexturedVertex>;

public class LatticeDef
{
  public bool EnableVertexGroup;
  //BKE_lattice_resize,calc_latt_deformで
  public int[] UVW = new int[3];//分割数
  float fu, du;
  float fv, dv;
  float fw, dw;
  KEYType keytype;
  Matrix4x4 latma;
  //Matrix4x4 myself;
  public LatticeType LatticeData = new LatticeType();//ラティスの頂点座標
  public List<ReactiveProperty<string>> LatticeString = new List<ReactiveProperty<string>> ();
  Vector3[] dvert = new Vector3[1];//頂点ウェイトグループ
  Vertex[] verts;
  Vertex[] overts;//オリジナル頂点
  
  MMDModel mesh;
  public float factor;

  void lattice_deform_vertsFull()
  {
    int len = verts.Length;
    var nverts = new Vertex[len];
    verts.CopyTo(nverts, 0);
    for (int i = 0; i < len; i++)
    {
      if (i >= len) break;
      //var pos = verts[i];
      //Debug.DrawLine(pos, pos + n[i]*0.1f);
      calc_latt_deform(ref nverts[i], factor);
    }
    mesh.Vertice = nverts;
    //mesh.RecalculateNormals();
  }

  //頂点グループ以外done
  void lattice_deform_verts(int count,int id)
  {
    //頂点グループは未対応
    //vgroup && vgroup[0] && use_vgroups
    int len = verts.Length;
    var nverts = new Vertex[len];
    verts.CopyTo(nverts, 0);
    int s = len/count;

	for (int i = id*len; i < id*len+s; i++)
    {
      if (i >= len) break;
      //var pos = verts[i];
      //Debug.DrawLine(pos, pos + n[i]*0.1f);
      calc_latt_deform(ref nverts[i], factor);
    }

    mesh.Vertice = nverts;
    //mesh.RecalculateNormals();
  }

  //移動以外を請け負う
  Vector3 mul_mat3_m4_v3(Matrix4x4 mat, Vector3 vec)
  {
    float x = vec[0];
    float y = vec[1];
    float z = vec[2];
    var xc = x * mat.M11 + y * mat.M21 + mat.M31 * z;
    var yc = x * mat.M12 + y * mat.M22 + mat.M32 * z;
    var zc = x * mat.M13 + y * mat.M23 + mat.M33 * z;
    return new Vector3(xc, yc, zc);
  }

  //lattice_deform_vertsから呼ばれる,notested done
  void init_latt_deform()
  {
    //GameObject ob=null;
    Matrix4x4 latmat;
    Matrix4x4 imat;
		//if (ob == null)
		//{
		//  //デフォームスペースの行列
		//  latmat = latma.inverse;
		//  //デフォーム配列に戻す
		//  imat = latmat.inverse;
		//}
		//else
		{
			imat = latma.Inverted();
			//今のところここに入るパターンが無い
			//latmat = imat * myself;
			latmat = Matrix4x4.Identity;
			//imat = latmat.inverse;
		}

		//BPoint bp;//頂点だけ参照している、lattice
		int id=0;
    var ffu = fu;
    var ffv = fv;
    var ffw = fw;
    for (int w = 0; w < UVW[2]; w++, fw += dw)
    {
      for (int v = 0; v < UVW[1]; v++, fv += dv)
      {
        for (int u = 0; u < UVW[0]; u++,
          //bp++, co += 3, fp += 3, 
          fu += du)
        {
					//if()//coがあるなら,無いと仮定
					Vertex value = LatticeData[ id ].Value;
					Vector3 pos = value.Position - new Vector3( fu , fv , fw );
					value.Position = ( pos );
		  //latmatをかけて行列を戻す
	      value.Position = ( mul_mat3_m4_v3( imat , value.Position ) );
          id++;
        }
        fu = ffu;
      }
      fv = ffv;
    }
    fw = ffw;
    latma = latmat;
  }

	// Use this for initialization
	// デバッグ用
	int topind;
#if INTERPOLATION
	Interpolation.Elastic interp;
	public float val = 2, pow = 10, sca = 1;
	public int bou = 7;
#endif

	public LatticeDef( MMDModel model , int[] divs = null )
	{
		if ( divs == null )
		{
			divs = new int[ 3 ] { 3 , 3 , 3 };
		}
		UVW = divs;
		Start( model );
		//LatticeData は BKE_lattice_resize, init_latt_deform で初期化されている
	}

  void Start(MMDModel model)
  {
	mesh = model;//GetComponent<MeshFilter>().mesh;

    keytype = KEYType.KEY_BSPLINE;
    overts = mesh.OrigVertice;
    verts = mesh.Vertice;
    NotifyChanged();
    init_latt_deform();
    lattice_deform_vertsFull();
	topind = LatticeData.FirstIndex( l => SamePos( l.Value.Position , new Vector3( 0 , 1 , 0 ) ) );
#if INTERPOLATION
		interp = new Interpolation.Elastic(2, 10, 7, 1);
#endif
  }
	void OnApplicationQuit()
	{
		if ( mesh != null )
			mesh.Vertice
			  = overts;
	}

  void NotifyChanged()
  {
    latma = //Matrix4x4.TRS(lattice.localPosition, lattice.rotation, lattice.localScale);
      Matrix4x4.Identity;
    BKE_lattice_resize(UVW[0], UVW[1], UVW[2]);
  }

  struct BPoint
  {
    float[] vec;
    float alfa, weight;
    //alfa:3Dビューのティルト weight:ソフトボディのゴールウェイト
    short f1, hide;
    //f1:選択ステータス hide:ポイントが隠されているかどうか
    float radius, pad;
    //ユーザーによって決められたベベル等のためのポイントごとの半径
  }

  int LT_GRID = 1;
  int gridFlag = 1;
  const int LT_ACTBP_NONE = -1;// object_latticeで使われる、標準ではactbpの初期値 
  //active element index
  int actbp = LT_ACTBP_NONE;
	//#define LT_OUTSIDE	2

	//#define LT_DS_EXPAND	4 anim_channelで使われている
	//curve_deform_vectorはアーマチュアで使われている
	//outside_latticeはrnaから、後方互換を保つ意味でやっているらしい、bepuikで作ったファイルとも保障されるのだろうか

	enum KEYType
  {
    KEY_LINEAR, KEY_CARDINAL, KEY_BSPLINE, KEY_CATMULL_ROM,
  };

  bool alreadyCreate = false;

  void key_curve_position_weights(float t, float[] data, KEYType type)
  {
    float t2, t3, fc;

    if (type == KEYType.KEY_LINEAR)
    {
      data[0] = 0.0f;
      data[1] = -t + 1.0f;
      data[2] = t;
      data[3] = 0.0f;
    }
    else if (type == KEYType.KEY_CARDINAL)
    {
      t2 = t * t;
      t3 = t2 * t;
      fc = 0.71f;

      data[0] = -fc * t3 + 2.0f * fc * t2 - fc * t;
      data[1] = (2.0f - fc) * t3 + (fc - 3.0f) * t2 + 1.0f;
      data[2] = (fc - 2.0f) * t3 + (3.0f - 2.0f * fc) * t2 + fc * t;
      data[3] = fc * t3 - fc * t2;
    }
    else if (type == KEYType.KEY_BSPLINE)
    {
      t2 = t * t;
      t3 = t2 * t;

      data[0] = -0.16666666f * t3 + 0.5f * t2 - 0.5f * t + 0.16666666f;
      data[1] = 0.5f * t3 - t2 + 0.66666666f;
      data[2] = -0.5f * t3 + 0.5f * t2 + 0.5f * t + 0.16666666f;
      data[3] = 0.16666666f * t3;
    }
    else if (type == KEYType.KEY_CATMULL_ROM)
    {
      t2 = t * t;
      t3 = t2 * t;
      fc = 0.5f;

      data[0] = -fc * t3 + 2.0f * fc * t2 - fc * t;
      data[1] = (2.0f - fc) * t3 + (fc - 3.0f) * t2 + 1.0f;
      data[2] = (fc - 2.0f) * t3 + (3.0f - 2.0f * fc) * t2 + fc * t;
      data[3] = fc * t3 - fc * t2;
    }
  }

  Vector3 sv(float x) { return new Vector3(x, x, x); }

  //頂点数だけculcする,done
  void calc_latt_deform(ref Vertex co, float weight)
  {
    float[] tu = new float[4], tv = new float[4], tw = new float[4];
    //obは対象オブジェクト
    int defgrp_index = -1;
    Vector3 co_prev = sv(0);
    float weight_blend = 0;
    if (EnableVertexGroup)
    {
      co_prev = co.Position;
    }
    //coはモデルのローカル頂点、ラティスの座標系に移動する
    var vec = latma.TransByMat( co.Position );
    float u, v, w;
    int ui, vi, wi;
    if (UVW[0] > 1)
    {
      u = (vec[0] - fu) / du;
      ui = Util.FloorToInt(u);
      u -= ui;
      key_curve_position_weights(u, tu, keytype);
    }
    else
    {
      tu[0] = tu[2] = tu[3] = 0.0f; tu[1] = 1.0f;
      ui = 0;
    }

    if (UVW[1] > 1)
    {
      v = (vec[1] - fv) / dv;
      vi = Util.FloorToInt(v);
      v -= vi;
      key_curve_position_weights(v, tv, keytype);
    }
    else
    {
      tv[0] = tv[2] = tv[3] = 0.0f; tv[1] = 1.0f;
      vi = 0;
    }
    if (UVW[2] > 1)
    {
      w = (vec[2] - fw) / dw;
      wi = Util.FloorToInt(w);
      w -= wi;
      key_curve_position_weights(w, tw, keytype);
    }
    else
    {
      tw[0] = tw[2] = tw[3] = 0.0f; tw[1] = 1.0f;
      wi = 0;
    }
    int idx_w = 0, idx_v, idx_u;
    int uu, vv, ww;
    for (ww = wi - 1; ww <= wi + 2; ww++)
    {
      //tw[0~3]
      w = tw[ww - wi + 1];

      if (w != 0.0f)
      {
        if (ww > 0)
        {
          //idx_wをwi-1 * uの分割 *vの分割
          if (ww < UVW[2]) idx_w = ww * UVW[0] * UVW[1];
          else             idx_w = (UVW[2] - 1) * UVW[0] * UVW[1];
        }
        else
        {
          idx_w = 0;
        }
        for (vv = vi - 1; vv <= vi + 2; vv++)
        {
          v = w * tv[vv - vi + 1];
          if (v != 0.0f)
          {
            if (vv > 0)
            {
              if (vv < UVW[1]) idx_v = idx_w + vv * UVW[0];
              else             idx_v = idx_w + (UVW[1] - 1) * UVW[0];
            }
            else
            {
              idx_v = idx_w;
            }

            for (uu = ui - 1; uu <= ui + 2; uu++)
            {
              u = weight * v * tu[uu - ui + 1];
              if (u != 0.0f)
              {
                if (uu > 0)
                {
                  if (uu < UVW[0]) idx_u = idx_v + uu;
                  else             idx_u = idx_v + (UVW[0] - 1);
                }
                else
                {
                  idx_u = idx_v;
                }
								var ldata = LatticeData[ LatticeData.Count - idx_u - 1 ];
                co.Position = ( co.Position + ldata.Value.Position * (u) );

                //co += transform.localPosition;
                //頂点グループが設定されているならウェイトをもらってきて計算に入れる
                if (defgrp_index != -1)  //恐らくdvert[idx_u]
                  weight_blend += (u); //* defvert_find_weight(dvert + idx_u, defgrp_index));
              }
            }
          }
        }
      }
    }
    //if (defgrp_index != -1)  //math_vector.c  weight_blendで線形補完
    //  interp_v3_v3v3(co, co_prev, co, weight_blend);
  }

  Vector2 calc_lat_fudu(int flag, int res)
  {
    if (res == 1)
    {
      return new Vector2(0.0f, 0.0f);
    }
    else if (flag == LT_GRID)
    {
      return new Vector2(-0.5f * (res - 1), 1.0f);
    }
    else
    {
      return new Vector2(-1.0f, 2.0f / (res - 1));
    }
  }

  //リサイズ部分以外終わり
  // latice割数を増やす
  void BKE_lattice_resize(int uNew, int vNew, int wNew)//, Object *ltOb)
  {
    //頂点ウェイトグループはすべて開放される
    if (dvert.Length != 0)
    {
      //BKE_defvert_array_free(lt->dvert, lt->pntsu * lt->pntsv * lt->pntsw);
      //lt->dvert = NULL;
    }
    while (uNew * vNew * wNew > 32000)
    {
      if (uNew >= vNew && uNew >= wNew) uNew--;
      else if (vNew >= uNew && vNew >= wNew) vNew--;
      else wNew--;
    }

    var vertexCos = new TexturedVertex[uNew * vNew * wNew];//tmp_vcos
    var fudu = calc_lat_fudu(gridFlag, uNew);
    var fvdv = calc_lat_fudu(gridFlag, vNew);
    var fwdw = calc_lat_fudu(gridFlag, wNew);
    if (alreadyCreate)
    {
      //1分割まで減らされたり、元から1分割でないなら
      if (uNew != 1 && UVW[0] != 1)
      {
        du = (UVW[0] - 1) * du / (uNew - 1);
      }

      if (vNew != 1 && UVW[1] != 1)
      {
        dv = (UVW[1] - 1) * dv / (vNew - 1);
      }

      if (wNew != 1 && UVW[2] != 1)
      {
        dw = (UVW[2] - 1) * dw / (wNew - 1);
      }
    }

    var co = vertexCos;
    fu = fudu[0];
    du = fudu[1];
    fv = fvdv[0];
    dv = fvdv[1];
    fw = fwdw[0];
    dw = fwdw[1];

    float wc = fw;
    float vc = fv;
    float uc = fu;
    int coi = 0;
    for (int w = 0; w < wNew; w++, wc += dw)
    {
      vc = fv;
      for (int v = 0; v < vNew; v++, vc += dv)
      {
        uc = fu;
        for (int u = 0; u < uNew; u++, coi++, uc += du)
        {
          var cv = new Vector3(uc, vc, wc);
          co[coi].Position = ( cv );
        }
      }
    }
    if (alreadyCreate)
    {
      //var mat = new float[4, 4];
      var typeu = keytype; //lt->typeu, typev = lt->typev, typew = lt->typew;

      //endpointsがマッチするので強制的にlinに変える
      //lt->typeu = lt->typev = lt->typew 
      keytype = KEYType.KEY_LINEAR;

			//変更された座標を使わないように
			//BKE_displist_free(&ltOb->curve_cache->disp);

//オブジェクトの行列を退避、デフォームさせてから戻す
//copy_m4_m4(mat, ltOb->obmat);
//unit_m4(ltOb->obmat);

#if false
			var mat = transform.localToWorldMatrix;
#endif
			//transform.localToWorldMatrix = Matrix4x4.identity;
			lattice_deform_vertsFull();
      //copy_m4_m4(ltOb->obmat, mat);
      keytype = typeu;
      //lt->typeu = typeu;
      //lt->typev = typev;
      //lt->typew = typew;
    }
    alreadyCreate = true;
    UVW[0] = uNew;
    UVW[1] = vNew;
    UVW[2] = wNew;
    actbp = LT_ACTBP_NONE;
		//bpの処理をしていないが、これでいいはず
		LatticeString = co.Select( x => new ReactiveProperty<string>( x.ToString( ) ) ).ToList();
		LatticeData = LatticeString.Select( reactiveProperty ) .ToList( );

  }
	ReactiveProperty<Vertex> reactiveProperty( ReactiveProperty<string> x )
	{
		return x.ToReactivePropertyAsSynchronized( v => v.Value , convert: Vertex.FromString ,
					convertBack: xd => xd.ToString() );
	}

  private void interp_v3_v3v3(Vector3 co1, Vector3 co_prev, Vector3 co2, float weight_blend)
  {
    throw new System.NotImplementedException();
  }

  // Update is called once per frame
  public Matrix4x4 toworld;

  public void FixedUpdate()
  {
    //03 13 23が座標
    lattice_deform_vertsFull();
#if false
		//toworld = gameObject.transform.localToWorldMatrix;
#endif
#if INTERPOLATION
		interp.SetValue(val, pow, bou, sca);
#endif

    for (int id = 0; id < LatticeData.Count; id++)
    {
      var ld = LatticeData[id];
      var pos = ld.Value.Position + mesh.Position;
      //todo latトランスフォームを正しく計算すれば表示位置がおかしい問題もスケールポジションが未実装なのも解決するはず
      //子に移動してみても問題は無い、ラティスの行列計算が思ったのと異なる計算、blenderのに合わせるべし
      //  頂点シェーダに移植
      //Debug.DrawLine(pos, pos + Vector3.up*0.1f);
    }
    bool pulling = false;
    if (current < toward)
    {
      current += nobiritu;
      pulling = true;
    }
    if(current>toward){
      current -= nobiritu;
    }
#if false
		if (iamunity)
    {
      var uchanpos = new Vector3(0, 0.02f, -0.78f);
      //var uchanrot = Quaternion.Euler(90, 0, 0);
      var uchansc = new Vector3(0.79f, 0.8f, 0.8f);

      transform.localPosition = uchanpos;
	  transform.localScale = uchansc;
    }
    if (false)
    {
      var lhop = 14;
      var rhop = 12;
      var vale = interp.InApply(current);
      if (pulling)
      {
        var alpha = Mathf.Lerp(0, -3, current);
        var alpha2 = Mathf.Lerp(0, -1, current);
        latticedata[lhop].x = alpha;
        latticedata[lhop].y = alpha2;
        var alpha3 = Mathf.Lerp(0, 3, current);
        var alpha4 = Mathf.Lerp(0, -1, current);
        latticedata[rhop].x = alpha3;
        latticedata[rhop].y = alpha4;
      }
      else
      {
        var alpha = Mathf.Lerp(-1, -3, vale);
        var alpha2 = Mathf.Lerp(0, -3, vale);
        latticedata[lhop].x = alpha;
        latticedata[lhop].y = alpha2;
        var alpha3 = Mathf.Lerp(1, 3, vale);
        var alpha4 = Mathf.Lerp(0, -3, vale);
        latticedata[rhop].x = alpha3;
        latticedata[rhop].y = alpha4;
      }
    }

#endif
  }

  bool SamePos(Vector3 p, Vector3 pp)
  {
    var px=p.X-pp.X;
    var py=p.Y-pp.Y;
    var pz=p.Z-pp.Z;
    return (px * px) + py * py + pz * pz < float.Epsilon;
  }

  float toward;//0~1
  float current;
  float nobiritu=0.05f;
  void Update()
  {
		//var one = new Vector3(1, 1, 1);
		//var th = Matrix4x4.TRS(sv(2), Quaternion.identity, sv(3));
		//var x= BlenderMatrix.mul_v3_m4v3( th,one);
		//print(x);
#if false
		if (Input.GetKey(KeyCode.A))
    {
      toward = 1-nobiritu;
      // = 2;
    }
    if (Input.GetKeyUp(KeyCode.A))
    {
      toward = nobiritu;
      //latticedata[topind].y = 1;
    }
#endif
  }

}
