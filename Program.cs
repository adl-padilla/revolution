using SDL_Extensions;
using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Revolution.GameLogic;
using static Revolution.GameLogic.Player;
using static Revolution.GameLogic.Token;

namespace Revolution
{
    class Program
    {
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall, SetLastError = true)]
        delegate void ManagedTypeSignature();

        static uint SetupTimerCallback(UInt32 interval, IntPtr? param)
        {

            var managedMethod = new ManagedTypeSignature(CallBack);
            Delegate untypedManagedMethod = (Delegate)managedMethod;
            IntPtr intPtr = Marshal.GetFunctionPointerForDelegate(untypedManagedMethod);

            SDL.SDL_Event @event = new SDL.SDL_Event();
            SDL.SDL_UserEvent userevent = new SDL.SDL_UserEvent
            {

                /* In this example, our callback pushes a function
                into the queue, and causes our callback to be called again at the
                same interval: */

                type = (uint)SDL.SDL_EventType.SDL_USEREVENT,
                code = 0,
                data1 = intPtr
            };
            if (param.HasValue)
                userevent.data2 = param.Value;

            @event.type = SDL.SDL_EventType.SDL_USEREVENT;
            @event.user = userevent;

            SDL.SDL_PushEvent(ref @event);
            return (interval);
        }

        public static void CallBack()
        {
            Console.WriteLine(DateTime.Now);
        }
        static void Main(string[] args)
        {
            using (var gl = new GameLogic())
            {
                new Player { Name = "Alex", Color = System.Drawing.Color.Blue };
                new Player { Name = "Randy", Color = System.Drawing.Color.Red };

                Player Alex = Instances.FirstOrDefault(x => x.Name == nameof(Alex));
                Alex.Board.Printer.SetBid(Alex.Tokens.OfType<Gold>().Take(2));
                Alex.Board.Apothecary.SetBid(Alex.Tokens.OfType<Blackmail>().Take(1));

                Player Randy = Instances.FirstOrDefault(x => x.Name == nameof(Randy));
                Randy.Board.Printer.SetBid(Randy.Tokens.OfType<Gold>().Take(3));
                Randy.Board.General.SetBid(Randy.Tokens.OfType<Force>().Take(1));


                var actionsWithBids = Instances.Select(x => x.Board).SelectMany(x => x.Actions.Where(y => y.Bid.Any()));

                var winners = actionsWithBids
                    .Select(x => x.GetType().Name)
                    .Distinct()
                    .ToList()
                    .Select(x =>
                    {
                        var z = actionsWithBids.Where(y => y.GetType().Name == x).OrderByDescending(y => y.Bid.ToInt()).FirstOrDefault();
                        return new
                        {
                            ActionName = x,
                            z.Bid,
                            z.PlayerBoard.Player
                        };
                    })
                     .ToList();

                winners.ForEach(x =>
                {
                    var pi = typeof(PlayerBoard).GetProperty(x.ActionName);
                    var pa = pi?.GetValue(x.Player.Board) as GameLogic.Action;
                    pa?.Resolve();


                });
            }
            var rv = SDL.Init(SDL2.SDL.INIT_EVERYTHING);
            var sdlIntitialized = !Convert.ToBoolean(rv);
            if (sdlIntitialized)
            {
                var mode = new SDL.SDL_DisplayMode();
                var displayRect = new SDL.Rect();
                var displayCount = SDL.GetNumVideoDisplays();
                SDL.GetDisplayBounds(0, out displayRect);
                SDL.GetCurrentDisplayMode(0, out mode);
                int width = 1280, height = 720;//720p

                var left = (int)((mode.w - width) * .5);
                var top = (int)((mode.h - height) * .5);


                using (var window = new Window(nameof(Revolution), left, top, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN, true))
                { 
                
                    if (window.IsValid())
                    {
                        window.Show();
                        window.Raise();
                    }
                    SetupTimerCallback(60, null);
                    var @event = new SDL.SDL_Event();
                    bool run = true;

                    //RecipeCardDeck recipeDeck = new RecipeCardDeck();
                    //var recipeTextures = gameLogic.Recipes.ToList().ToDictionary(x => x.GetHashCode(), x => window.Renderer.LoadTexture(x.Source));

                    var tokenTextures = new Dictionary<char, Texture>
                    {
                        {'b',window.Renderer.LoadTexture("images/blackmail.png") },
                        {'g',window.Renderer.LoadTexture("images/money.png")},
                        {'p',window.Renderer.LoadTexture("images/force.png")}
                    };

                    window.Scene = new List<Sprite>();

                    var boardTexture = window.Renderer.LoadTexture("images/board.png");


                    var boardSprite = boardTexture.CreateSprite();
                    boardSprite.X = 0;
                    boardSprite.Y = 0;
                    boardSprite.Z = 0;
                    boardSprite.Scale = 1f;
                    window.Scene.Add(boardSprite);

                    //Debug.WriteLine(gameLogic.FishMarket);

                    int xc = boardSprite.Width;
                    tokenTextures.ToList().ForEach(x =>
                    {
                        var s = new Sprite(x.Value) { Y = 500, X = xc += x.Value.Width, Z = 0, Scale = 1.0f };
                        window.Scene.Add(s);
                        xc += s.Width;
                    });


                    //var s2 = test.CreateSprite();
                    //s2.X = 100;
                    //s2.Y = 100;
                    //s2.Z = 1;
                    //s2.Scale = 1f;
                    //window.Scene.Add(s2);

                    //var s3 = new Sprite(test)
                    //{
                    //    X = 150,
                    //    Y = 150,
                    //    Z = 2,
                    //    Scale = 1f,
                    //};
                    //window.Scene.Add(generalMarketSprite);
                    SDL.SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1");
                    window.Clear();
                    window.Present();

                    while (run)
                    {
                        SDL.SDL_PollEvent(out @event);
                        //while ( != 0)
                        //{
                        switch (@event.type)
                        {
                            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                                var button = @event.button;
                                var point = new SDL.Point() { x = button.x, y = button.y };
                                var timestamp = new DateTime(button.timestamp);
                                var matches = window.Scene.Where(sprite => sprite.Contains(point)).ToList();
                                var brown = window.Scene.First();
                                matches.ForEach(sprite => sprite.MouseDown(@event));
                                //button.button  1   byte
                                //button.clicks  1   byte
                                //button.padding1    0   byte
                                //button.state   1   byte
                                //button.timestamp   140630  uint
                                //button.@type   SDL_MOUSEBUTTONDOWN SDL2.SDL.SDL_EventType
                                //button.which   0   uint
                                //button.windowID    1   uint
                                //button.x   182 int
                                //button.y   169 int

                                break;
                            case SDL.SDL_EventType.SDL_QUIT:
                                run = false;
                                break;
                            case SDL.SDL_EventType.SDL_FIRSTEVENT:
                                Debug.WriteLine("First Event");
                                break;
                            case SDL.SDL_EventType.SDL_LASTEVENT:
                                Debug.WriteLine("Last Event");
                                break;

                        }
                        //}
                    }
                    //SDL.SDL_WaitEvent(out @event);
                    //var quit = @event.quit;
                }


            }
        }
    }
}
