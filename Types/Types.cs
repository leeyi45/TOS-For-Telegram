using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuizBot
{
	public enum Cycle
	{
		Day = 1,
		Night = 2,
		Lynch = 3
	}

	public enum Team
	{
		Neutral = 3,
		Mafia = 2,
		Town = 1
	}

	public enum GamePhase
	{
		Inactive,
		Joining,
		Assigning,
		Running
	}


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

		/*
		public Tuple<T, U, V>[] ToArray()
		{
			var arr = new Tuple<T, U, V>[store.Count];
			int i = 0;
			foreach (var each in store)
			{
				arr[i] = new Tuple<T, U, V>(each.Key, each.Value.Item1, each.Value.Item2);
				i++;
			}
			return arr;
		}*/
	}

	/*
	class TripEnumerator<T, U, V> : IEnumerator
	{
		private Triptionary<T, U, V> inside;
		private Tuple<T, U, V>[] arr;
		private int _index = -1;

		public TripEnumerator(Triptionary<T, U, V> _inside)
		{
			inside = _inside;
			arr = _inside.ToArray();
		}

		public void Reset() { _index = -1; }

		public bool MoveNext()
		{
			if (inside.Count < _index + 1) return false;
			else { _index++; return true; }
		}

		public object Current
		{
			get
			{
				return new TripEnumOut<T, U, V>(arr[_index].Item1, arr[_index].Item2, arr[_index].Item3);
			}
		}
		
	}

	class TripEnumOut<T, U, V>
	{
		public TripEnumOut(T x, U y, V z)
		{
			Key = x;
			Value1 = y;
			Value2 = z;
		}

		public T Key { get; private set; }

		public U Value1 { get; private set; }

		public V Value2 { get; private set; }
	}

	/*
	class Triptionary<T, U, V> : IEnumerable
	{
		public Triptionary()
		{
			store = new Dictionary<T, Dictionary<U, V>>();
		}

		Dictionary<T, Tuple<U, V>> store;

		public V this[T x, U y]
		{
			get { return store[x][y]; }
			set 
			{
				try
				{
					store[x][y] = value;
				}
				catch (KeyNotFoundException)
				{
					store[x] = new Dictionary<U, V>();
					store[x][y] = value;
				}
			}
		}

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

		public void Add(T x, U y, V z)
		{
			this[x, y] = z;
		}

		public int Count
		{
			get { return store.Count; }
		}

		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)new TripEnumerator<T, U, V>(this);
		}

		public Dictionary<U, V>[] ToArray()
		{
			var arr = new Dictionary<U, V>[store.Count];
			int i = 0;
			foreach (var each in store)
			{
				arr[i] = each.Value;
				i++;
			}
			return arr;
		}
	}

	
	class TripEnumerator<T, U, V> : IEnumerator
	{
		private int _index = -1;
		private Dictionary<U, V>[] inside;
		private Dictionary<U, V> original;

		public TripEnumerator(Triptionary<T, U, V> _inside)
		{
			//original = _inside;
			inside = _inside.ToArray();
		}

		public void Reset()
		{
			_index = -1;
		}

		public bool MoveNext()
		{
			if (_index + 1 == inside.Length) return false;
			else
			{
				_index += 1;
				return true;
			}
		}

		public object Current
		{
			get
			{
				return new KeyValuePair<int, Dictionary<U, V>>(_index, inside[_index]);
			}
		}
	}*/

  //This class is here so I can store both attributes and roles in the same dictionary
  /// <summary>
  /// Wrapper class for roles and attributes
  /// </summary>
  public class Wrapper
  {
    public virtual Team team { get; set; }

    public virtual string Name { get; set; }
  }

  /// <summary>
  /// Class to represent a team
  /// </summary>
  public class TeamWrapper : Wrapper
  {
    public TeamWrapper(Team team)
    {
      this.team = team;
    }

    public override string Name
    {
      get { return "Random " + team.ToString(); }
    }
  }

  /// <summary>
  /// Class representing the type of role
  /// </summary>
  public class Alignment : Wrapper
  {
    /// <summary>
    /// Initializes an "Any" attribute
    /// </summary>
    public Alignment()
    {
      Name = "Any";
    }

    public Alignment(string name, Team team)
    {
      this.team = team;
      this.name = name;
    }

    public static Alignment Parse(string input)
    {
      string[] args = input.Split(' ');
      if (args.Length > 2 || string.IsNullOrWhiteSpace(input)) throw new FormatException();
      return new Alignment(args[1], (Team)Enum.Parse(typeof(Team), args[0]));
    }

    private string name;

    public override string Name
    {
      get
      {
        if (name != "Any") return team.ToString() + " " + name;
        else return "Any";
      }
      set { name = value; }
    }

  }

  /// <summary>
  /// Class to represent a role
  /// </summary>
	public class Role : Wrapper
	{
		public Role(string _name, Team _team, string d, Alignment a, bool n, bool day)
		{
			Name = _name;
			team = _team;
			attribute = a;
			description = d;
			HasNightAction = n;
			HasDayAction = day;
		}

		public Role() { }

		#region Properties

		public string description { get; set; }

		public Alignment attribute { get; set; }

		public bool HasNightAction { get; set; }

		public bool HasDayAction { get; set; }
		#endregion

		public void DoDayAction()
		{
			if (!HasDayAction) return;
			GameData.DayRoleActions[Name]();
		}

		public void DoNightAction()
		{
			if (!HasNightAction) return;
			GameData.NightRoleActions[Name]();
		}
	}

  /// <summary>
  /// Class representing a player in the game
  /// </summary>
	public class Player
	{
		public Player() { }

		public Player(Role _R)
			: base()
		{
			role = _R;
		}

		public Player(User x)
		{
			FirstName = x.FirstName;
			LastName = x.LastName;
			Id = x.Id;
			Username = x.Username;
		}

		public Role role { get; set; }

		#region User Fields
		public string FirstName { get; private set; }

		public string LastName { get; private set; }

		public string Username { get; private set; }

		public int Id { get; private set; }
    #endregion

    public bool IsAlive { get; set; } = true;

		public string Name
		{
			get { return FirstName + " " + LastName; }
		}

		public string Nickname { get; set; }

		#region Operators
		public static implicit operator Player(User x) 
		{
			return new Player(x);
		}

		public static bool operator ==(Player rhs, Player lhs)
		{
			return rhs.Id == lhs.Id;
		}

		public static bool operator !=(Player rhs, Player lhs)
		{
			return !(rhs == lhs);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
	}
}
