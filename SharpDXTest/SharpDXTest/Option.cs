using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Option<T>
{
  //値
  public abstract T Value { get; }

  //値が存在する場合には値を返し、存在しない場合には既定値を返す
  public abstract T GetOrDefault(T def);
  //値が存在するかどうかによって別々の処理を実行する
  public abstract void Match(Action none = null, Action<T> some = null);
  //値が存在するかどうかによって別々の関数を実行し、返り値を返す
  public abstract S Match<S>(Func<S> none = null, Func<T, S> some = null);
  //現在の実体から新しい随意型の実体を作成し、返す
  public abstract Option<S> Bind<S>(Func<T, Option<S>> f);

  //現在の実体から新しい随意型の実体を作成し、返す
  public Option<S> Map<S>(Func<T, S> f)
  {
    return Bind((elem) => Option.Return<S>(f(elem)));
  }
}

//値が存在しない随意型
public sealed class None<T> : Option<T>
{
  //値
  public override T Value
  {
    //存在しないので例外を投げる
    get { throw new Exception("not existed value"); }
  }

  //値が存在する場合には値を返し、存在しない場合には既定値を返す
  public override T GetOrDefault(T def)
  {
    //存在しないので既定値を返す
    return def;
  }

  //値が存在するかどうかによって別々の処理を実行する
  public override void Match(Action none = null, Action<T> some = null)
  {
    //値が存在しない場合に実行されるよう指定された処理が渡された場合には処理を実行する
    //渡されなかった場合には何もしない
    if (none != null)
      none();
  }

  //値が存在するかどうかによって別々の関数を実行し、返り値を返す
  public override S Match<S>(Func<S> none = null, Func<T, S> some = null)
  {
    //値が存在しない場合に実行されるよう指定された関数が渡された場合には関数を実行し、返り値を返す
    //渡されなかった場合には例外を投げる
    if (none != null)
      return none();
    else
      throw new Exception("none is null");
  }

  //現在の実体から新しい随意型の実体を作成し、返す
  public override Option<S> Bind<S>(Func<T, Option<S>> f)
  {
    //基になる値が存在しないので値が存在しない新しい随意型を作成し、返す
    return Option.Return<S>();
  }
}

//値が存在する随意型
public sealed class Some<T> : Option<T>
{
  //値を受け取って実体化する
  public Some(T value)
  {
    _value = value;
  }

  //値
  private T _value;
  public override T Value
  {
    //存在するので値を返す
    get { return _value; }
  }

  //値が存在する場合には値を返し、存在しない場合には既定値を返す
  public override T GetOrDefault(T def)
  {
    //存在するので値を返す
    return Value;
  }

  //値が存在するかどうかによって別々の処理を実行する
  public override void Match(Action none = null, Action<T> some = null)
  {
    //値が存在する場合に実行されるよう指定された処理が渡された場合には処理を実行する
    //渡されなかった場合には何もしない
    if (some != null)
      some(Value);
  }

  //値が存在するかどうかによって別々の関数を実行し、返り値を返す
  public override S Match<S>(Func<S> none = null, Func<T, S> some = null)
  {
    //値が存在する場合に実行されるよう指定された関数が渡された場合には関数を実行し、返り値を返す
    //渡されなかった場合には例外を投げる
    if (some != null)
      return some(Value);
    else
      throw new Exception("some is null");
  }

  //現在の実体から新しい随意型の実体を作成し、返す
  public override Option<S> Bind<S>(Func<T, Option<S>> f)
  {
    //基になる値が存在するので値を基に新しい随意型を作成し、返す
    return f(Value);
  }
}

//随意型のための補助的な関数を格納するクラス
public static class Option
{
  //値を受け取って値が存在する随意型を返す
  public static Option<T> Return<T>(T value)
  {
    return new Some<T>(value);
  }

  //値が存在しない随意型を返す
  public static Option<T> Return<T>()
  {
    return new None<T>();
  }

  //随意型の実体を受け取って新しい随意型の実体を作成し、返す
  public static Option<S> Bind<T, S>(Option<T> m, Func<T, Option<S>> f)
  {
    return m.Bind(f);
  }
}