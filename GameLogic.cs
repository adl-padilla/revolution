using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Revolution.GameLogic.Token;
using static Revolution.GameLogic.GameBoard.Area;
using static Revolution.GameLogic.GameBoard;

namespace Revolution
{
    internal class GameLogic : IDisposable
    {
        public static GameBoard Board = new GameBoard();
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
        internal class TargetedActionArguments
        {
            public Info Source { get; set; }
            public Info Target { get; set; }

            internal class  Info
            {
                public Area Area { get; set; }
                public Player Player { get; set; }

            }

        }
        internal class Bid : List<Token>
        {
            public Bid()
            {
            }

            public Bid(IEnumerable<Token> collection) : base(collection)
            {
            }

            internal int ToInt()
            {
                int v = 0;
                v += this.Count(x => x.GetType().Name == nameof(Force)) * 256;
                v += this.Count(x => x.GetType().Name == nameof(Blackmail)) * 64;
                v += this.Count(x => x.GetType().Name == nameof(Gold));
                return v;
            }
        }
        internal class Action
        {
            internal Bid Bid { get; set; }
            internal void SetBid(IEnumerable<Token> bid)
            {
                this.Bid = new Bid(bid);
                bid.ToList().ForEach(x => this.PlayerBoard.Player.Tokens.Remove(x));

            }
            public Action()
            {
                this.Bid = new Bid();
            }
            public Player.PlayerBoard PlayerBoard { get; internal set; }

            [NoForce()]
            internal class General : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support++;
                    this.PlayerBoard.Player.Tokens.Add(new Force());
                    Board.Fortress.AddInfluence(this.PlayerBoard.Player);
                    return true;

                }

            };

            [NoForce()]
            internal class Captain : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support++;
                    this.PlayerBoard.Player.Tokens.Add(new Force());
                    Board.Harbor.AddInfluence(this.PlayerBoard.Player);
                    return true;

                }
            };

            [NoBlackmail()]
            internal class Innkeeper : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support += 3;
                    this.PlayerBoard.Player.Tokens.Add(new Blackmail());
                    Board.Tavern.AddInfluence(this.PlayerBoard.Player);
                    return true;

                }
            };

            [NoBlackmail()]
            internal class Magistrate : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support++;
                    this.PlayerBoard.Player.Tokens.Add(new Blackmail());
                    Board.TownHall.AddInfluence(this.PlayerBoard.Player);
                    return true;

                }
            };

            internal class Priest : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support += 6;
                    Board.Cathedral.AddInfluence(this.PlayerBoard.Player);
                    return true;

                }
            };

            internal class Aristocrat : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support += 5;
                    for (int i = 0; i < 3; i++)
                        this.PlayerBoard.Player.Tokens.Add(new Gold());
                    Board.Plantation.AddInfluence(this.PlayerBoard.Player);
                    return true;

                }
            };

            internal class Merchant : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support += 3;
                    for (int i = 0; i < 5; i++)
                        this.PlayerBoard.Player.Tokens.Add(new Gold());
                    Board.Market.AddInfluence(this.PlayerBoard.Player);
                    return true;

                }
            };

            internal class Printer : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    this.PlayerBoard.Player.Support += 10;
                    return true;
                }
            };

            [NoBlackmail()]
            [NoForce()]
            internal class Rogue : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    for (int i = 0; i < 2; i++)
                        this.PlayerBoard.Player.Tokens.Add(new Blackmail());
                    return true;
                }
            };

            [NoBlackmail()]
            internal class Spy : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    var target = arguments.First().Target;
                    var source = arguments.First().Source;
                    //Replace 1 cube with one of your own
                    if (target.Area != null &&
                        target.Player != null && 
                        source.Player !=null&&
                        target.Area.AnyInfluence(target.Player))
                        return target.Area.ReplaceInfluence(target.Player, source.Player);
                    return true;

                }

            }
            

            internal class Apothecary : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    //Swap the cubes in any two Influence spaces
                    for (int i = 0; i < arguments.Count() && i < 2; i++)
                    {
                        var arg = arguments[0];
                        //var info=new List<Tuple<Tuple<Area, Player>, Tuple<Area, Player>>{ new Tuple<Tuple<Area, Player>, Tuple<Area, Player>>, new Tuple<Tuple<Area, Player>, Tuple<Area, Player>>}
                        //(no force)

                    }
                    return false;

                }
            };

            internal class Mercenary : Action
            {
                public override bool Resolve(TargetedActionArguments[] arguments = null)
                {
                    //(no force, no blackmail) 
                    this.PlayerBoard.Player.Support += 3;
                    this.PlayerBoard.Player.Tokens.Add(new Force());
                    return true;

                }
            };

            public virtual bool Resolve(TargetedActionArguments[] arguments=null) { return false; }
        }
        internal class Player
        {
            public string Name { get; set; }
            public int Points { get; set; }
            public PlayerBoard Board { get; set; }
            public IList<Token> Tokens { get; set; }

            public static IList<Player> Instances { get; set; }
            public Player()
            {
                InfluencePool = 25;
                Board = new PlayerBoard() { Player = this };
                //Each player starts the game with one Force, one
                //Blackmail, and three Gold.The rest of the tokens
                //make up the bank.There are 32 Gold, 12 Blackmail,
                //and 12 Force.
                this.Tokens = new List<Token> { new Force(), new Blackmail() };
                for (int i = 0; i < 3; i++)
                    Tokens.Add(new Gold());
                if (Instances == null)
                    Instances = new List<Player>();
                Instances.Add(this);
            }
            internal void Bid(Action action, Token[] bid)
            {
                action.Bid = new Bid(bid);
            }

            public override string ToString()
            {
                return $@"{Name}: {base.GetHashCode()}";
            }
            public Color Color { get; internal set; }
            public int Support { get; internal set; }
            public int InfluencePool { get; internal set; }

            internal class PlayerBoard
            {
                public IList<Action> Actions { get; set; }
                public PlayerBoard()
                {
                    General = new Action.General() { PlayerBoard = this };
                    Captain = new Action.Captain() { PlayerBoard = this };
                    Innkeeper = new Action.Innkeeper() { PlayerBoard = this };
                    Magistrate = new Action.Magistrate() { PlayerBoard = this };

                    Priest = new Action.Priest() { PlayerBoard = this };
                    Aristocrat = new Action.Aristocrat() { PlayerBoard = this };
                    Merchant = new Action.Merchant() { PlayerBoard = this };
                    Printer = new Action.Printer() { PlayerBoard = this };

                    Rogue = new Action.Rogue() { PlayerBoard = this };
                    Spy = new Action.Spy() { PlayerBoard = this };
                    Apothecary = new Action.Apothecary() { PlayerBoard = this };
                    Mercenary = new Action.Mercenary() { PlayerBoard = this };

                    Actions = new List<Action>
                    {
                        General,
                        Captain,
                        Innkeeper,
                        Magistrate,
                        Priest,
                        Aristocrat,
                        Merchant,
                        Printer,
                        Rogue,
                        Spy,
                        Apothecary,
                        Mercenary
                    };

                }
                public void ClearBids()
                {
                    this.General.Bid.Clear();
                    this.Captain.Bid.Clear();
                    this.Innkeeper.Bid.Clear();
                    this.Magistrate.Bid.Clear();

                    this.Priest.Bid.Clear();
                    this.Aristocrat.Bid.Clear();
                    this.Merchant.Bid.Clear();
                    this.Printer.Bid.Clear();

                    this.Rogue.Bid.Clear();
                    this.Spy.Bid.Clear();
                    this.Apothecary.Bid.Clear();
                    this.Mercenary.Bid.Clear();

                }
                public Action.General General { get; set; }
                public Action.Captain Captain { get; set; }
                public Action.Innkeeper Innkeeper { get; set; }
                public Action.Magistrate Magistrate { get; set; }
                public Action.Priest Priest { get; set; }
                public Action.Aristocrat Aristocrat { get; set; }
                public Action.Merchant Merchant { get; set; }
                public Action.Printer Printer { get; set; }
                public Action.Rogue Rogue { get; set; }
                public Action.Spy Spy { get; set; }
                public Action.Apothecary Apothecary { get; set; }
                public Action.Mercenary Mercenary { get; set; }
                public Player Player { get; internal set; }
            }


        }
        internal class Token
        {
            internal class Gold : Token { }
            internal class Blackmail : Token { }
            internal class Force : Token { }

            public override bool Equals(object that)
            {
                return this.GetType() == that.GetType();
            }
            public override int GetHashCode()
            {
                return this.GetType().Name.GetHashCode();
            }
        }
        internal class GameBoard
        {
            public Area.Plantation Plantation { get; set; }

            public Area.Tavern Tavern { get; set; }

            public Area.Cathedral Cathedral { get; set; }

            public Area.TownHall TownHall { get; set; }

            public Area.Fortress Fortress { get; set; }

            public Area.Market Market { get; set; }

            public Area.Harbor Harbor { get; set; }

            public GameBoard()
            {
                Cathedral = new Area.Cathedral();
                Fortress = new Area.Fortress();
                Harbor = new Area.Harbor();
                Market = new Area.Market();
                Plantation = new Area.Plantation();
                Tavern = new Area.Tavern();
                TownHall = new Area.TownHall();
            }

            internal class Area
            {
                public Area()
                {
                    Influence = new List<Player>();
                }
                public byte Spaces
                {
                    get { return (this.GetType().GetCustomAttributes(typeof(SpacesAttribute), false).FirstOrDefault() as SpacesAttribute).Value; }
                }
                public byte PointValue
                {
                    get { return (this.GetType().GetCustomAttributes(typeof(PointValueAttribute), false).FirstOrDefault() as PointValueAttribute).Value; }
                }

                private IList<Player> Influence { get;  set; }
                public bool AddInfluence(Player player)
                {
                    if (Influence.Count + 1 < Spaces && player.InfluencePool-1>=0)
                    {
                        this.Influence.Add(player);
                        player.InfluencePool--;
                        return true;
                    }
                    return false;
                }

                public bool AnyInfluence(Player targetPlayer)
                {
                    return Influence.Any(x => x.Equals(targetPlayer));
                }

                public bool ReplaceInfluence(Player targetPlayer, Player invokingPlayer)
                {
                    Influence.FirstOrDefault(x => x.Equals(targetPlayer));
                    targetPlayer.InfluencePool++;
                    Influence.Add(invokingPlayer);
                    invokingPlayer.InfluencePool--;
                    return true;
                }

                [PointValue(30)]
                [Spaces(6)]
                public class Plantation: Area { }

                [PointValue(20)]
                [Spaces(4)]
                public class Tavern : Area { }

                [PointValue(35)]
                [Spaces(8)]
                public class Cathedral : Area { }

                [PointValue(45)]
                [Spaces(7)]
                public class TownHall : Area { }

                [PointValue(50)]
                [Spaces(8)]
                public class Fortress : Area { }
                [PointValue(25)]
                [Spaces(5)]
                public class Market : Area { }

                [PointValue(40)]
                [Spaces(6)]
                public class Harbor : Area {}
            }

        }
    }
}