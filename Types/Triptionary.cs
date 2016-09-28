using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace QuizBot
{
  //Could be using tuples, but why the hell not
  class Triptionary<T, U, V> : IEnumerable<KeyValuePair<T, Dictionary<U, V>>>
  {
    public Triptionary()
    {
      store = new Dictionary<T, Dictionary<U, V>>();
    }

    protected Triptionary(Dictionary<T, Dictionary<U, V>> it)
    {
      store = it;
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

    public IEnumerator<KeyValuePair<T, Dictionary<U, V>>> GetEnumerator()
    {
      return store.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    public Dictionary<T, Dictionary<U, V>>.KeyCollection Keys
    {
      get { return store.Keys; }
    }

    #region Operators
    public static implicit operator Dictionary<T, Dictionary<U, V>>(Triptionary<T, U, V> it)
    {
      return it.store;
    }

    public static implicit operator Triptionary<T, U, V>(Dictionary<T, Dictionary<U, V>> it)
    {
      return new Triptionary<T, U, V>(it);
    }

    public static bool operator ==(Triptionary<T, U, V> lhs, Triptionary<T, U, V> rhs)
    {
      return lhs.store == rhs.store;
    }

    public static bool operator !=(Triptionary<T, U, V> lhs, Triptionary<T, U, V> rhs)
    {
      return lhs.store != rhs.store;
    }

    public override bool Equals(object obj)
    {
      if (obj is Triptionary<T, U, V>) return (Triptionary<T, U, V>)obj == this;
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion
  }

  public class InstanceList : IEnumerable<Game>
  {
    public InstanceList()
    {
      store = new Dictionary<int, Game>();
      ids = new List<long>();
    }

    private Dictionary<int, Game> store;

    private List<long> ids;

    public Game this[int index]
    {
      get { return store[index]; }
      set { store[index] = value; }
    }

    public Game this[long id]
    {
      get { return store.Values.First(x => x.CurrentGroup == id); }
      set
      {
        var instance = store.Values.First(x => x.CurrentGroup == id);
        instance = value;
      }
    }

    public List<long> IDs { get { return ids; } }

    public int Count { get { return store.Count; } }

    public void Add(Game instance)
    {
      store.Add(instance.PrivateID, instance);
      IDs.Add(instance.CurrentGroup);
    }

    #region Remove
    public bool Remove(Game instance)
    {
      ids.Remove(instance.CurrentGroup);
      return store.Remove(instance.PrivateID);
    }

    public bool Remove(int PrivateId)
    {
      return Remove(PrivateId);
    }

    public bool Remove(long GroupId)
    {
      return Remove(store.Values.First(x => x.PrivateID == GroupId));
    }
    #endregion

    #region Contains
    public bool Contains(Game instance) { return store.Values.Contains(instance); }

    public bool Contains(int privateId)
    {
      try { return this[privateId] == null; }
      catch { return false; }
    }

    public bool Contains(long instanceId) { return ids.Contains(instanceId); }
    #endregion

    public IEnumerator<Game> GetEnumerator() { return store.Values.GetEnumerator(); }

    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    public int GenerateNewPrivateId()
    {
      int i = -1;
      while(true)
      {
        i++;
        if (!store.ContainsKey(i)) return i;
      }
    }
  }
}
