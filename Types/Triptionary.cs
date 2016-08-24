using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace QuizBot
{
  //Could be using tuples, but why the hell not
  class Triptionary<T, U, V> : IEnumerable
  {
    public Triptionary()
    {
      store = new Dictionary<T, Dictionary<U, V>>();
    }

    private Dictionary<T, Dictionary<U, V>> store;

    public Dictionary<U, V> this[T x]
    {
      get
      {
        try { return store[x]; }
        catch (KeyNotFoundException)
        {
          store[x] = new Dictionary<U, V>();
          return store[x];
        }
      }
      set
      {
        try { store[x] = value; }
        catch (KeyNotFoundException)
        {
          store[x] = new Dictionary<U, V>();
          store[x] = value;
        }
      }
    }

    public V this[T x, U y]
    {
      get { return store[x][y]; }
      set { store[x][y] = value; }
    }

    public int Count
    {
      get { return store.Count; }
    }

    public IEnumerator GetEnumerator()
    {
      return store.GetEnumerator();
    }

    private void IniCheck(T x)
    {
      //if(store.)
    }

  }
}
