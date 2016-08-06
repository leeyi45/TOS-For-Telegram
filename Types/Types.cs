using System;
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
	enum Cycle
	{
		Day = 1,
		Night = 2,
		Lynch = 3
	}

	enum Team
	{
		Neutral = 3,
		Mafia = 2,
		Town = 1
	}


	//Could be using tuples, but why the hell not
	class Triptionary<T, U, V>
	{
		public Triptionary()
		{
			store = new Dictionary<T, Dictionary<U, V>>();
		}

		Dictionary<T, Dictionary<U, V>> store;

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
			get { return store[x]; }
			set { store[x] = value; }
		}

		public void Add(T x, U y, V z)
		{
			this[x, y] = z;
		}
	}

	class Role
	{
		public Role(string _name, Team _team, string d, string a, bool n, bool day)
		{
			Name = _name;
			team = _team;
			attribute = a;
			description = d;
			HasNightAction = n;
			HasDayAction = day;
		}

		public Role() { }

		public string Name { get; set; }

		public Team team { get; set; }

		public string description { get; set; }

		public string attribute { get; set; }

		public bool HasNightAction { get; set; }

		public bool HasDayAction { get; set; }
	}

	class Player : User
	{
		public Role role { get; set; }
	}
}
