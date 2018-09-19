using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using System.IO;
using System.Runtime.InteropServices;

namespace SDL_Extensions
{
    public static class Extensions
    {

    }

    public class Sprite //: Texture, IDisposable
    {
        public Texture Texture { get; internal set; }
        public int X { get { return Rect.x; } set { Rect = new SDL.Rect { x = value, y = Y, h = Height, w = Width }; } }
        public int Y { get { return Rect.y; } set { Rect = new SDL.Rect { x = X, y = value, h = Height, w = Width }; } }
        public int Height { get { return Rect.h; } set { Rect = new SDL.Rect { x = X, y = Y, h = value, w = Width }; } }
        public int Width { get { return Rect.w; } set { Rect = new SDL.Rect { x = X, y = Y, h = Height, w = value }; } }
        public int Z { get; set; }

        public Sprite()
        {
            this.Rect = new SDL.Rect();
        }

        public Sprite(Texture texture)
        {
            var r = new SDL.Rect { h = texture.Height, w = texture.Width };
            this.Texture = texture;
            this.Rect = r;
        }
        public bool Contains(SDL2.SDL.Point point)
        {
            var isWithin = (point.x >= X && point.x <= X + Width) && (point.y >= Y && point.y <= Y + Height);
            if (isWithin)
            {
                //var pixel = SDL2.Image.GetPixel(this.Texture.Surface.IntPtr, point.x - this.X, point.y - this.Y, this.Scale);
                //if (pixel?.a == 0 && isWithin == true)
                //{
                //    isWithin = false;
                //    //Console.WriteLine($@"{DateTime.Now}: Transparent");
                //}

            }
            return isWithin;
        }

        public SDL.Rect Rect { get; internal set; }
        private float _scale = 1f;

        public float Scale { get { return _scale; } set { Rect = new SDL.Rect { x = X, y = Y, h = (int)(Height * value), w = (int)(Width * value) }; _scale = value; } }

        public void MouseDown(SDL.SDL_Event @event)
        {
            Console.WriteLine(new FileInfo(this.Texture.FilePath).Name);
        }
    }

    public class Window : SDLObject, IDisposable
    {
        public string Title { get; internal set; }
        public int Left { get; internal set; }
        public int Top { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public SDL.SDL_WindowFlags Flags { get; internal set; }
        public Renderer Renderer { get; private set; }
        public List<Sprite> Scene { get; internal set; }

        public Window()
        {

        }
        public Window(string title, int left, int top, int width, int height, SDL.SDL_WindowFlags flags, bool createRenderer = false)
        {
            this.Title = title;
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            this.Flags = flags;
            this.IntPtr = SDL.CreateWindow(title, left, top, width, height, flags);
            if (createRenderer)
            {
                //IntPtr windowIntPtr;// = new IntPtr();
                //IntPtr rendrerIntPtr;// = new IntPtr();

                //var rv=SDL.CreateWindowAndRenderer(width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN, out windowIntPtr, out rendrerIntPtr);
                //if (Convert.ToBoolean(windowIntPtr.ToInt64()) && Convert.ToBoolean(rendrerIntPtr.ToInt64()))
                //{
                //this.Renderer = new Renderer() { IntPtr = rendrerIntPtr };// 
                this.Renderer = (Renderer)SDL.CreateRenderer(this.IntPtr, 0, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
                //}
            }


        }

        public void Dispose()
        {
            SDL.DestroyWindow(this.IntPtr);
        }

        internal void DrawSprites(IOrderedEnumerable<Sprite> orderedEnumerable)
        {
            orderedEnumerable.ToList().ForEach(x => DrawSprite(x));
        }

        public static explicit operator IntPtr(Window window)
        {
            return window.IntPtr;
        }

        internal void DrawSprite(Sprite s)
        {
            Copy(s.Texture, s.Texture.Rect, s.Rect);
        }

        internal void Present()
        {
            DrawSprites(Scene.OrderByDescending(sprite => sprite.Z));
            this.Renderer.Present();
        }

        internal void Copy(Texture texture, SDL.Rect sourceRect, SDL.Rect destRect)
        {
            this.Renderer.Copy(texture, sourceRect, destRect);
        }

        internal void Clear()
        {
            this.Renderer.Clear();
        }

        internal bool IsValid()
        {
            return Convert.ToBoolean(this.IntPtr.ToInt64());
        }

        public static explicit operator Window(IntPtr v)
        {
            return new Window { IntPtr = v };
        }

        internal void Show()
        {
            SDL.ShowWindow(this.IntPtr);
        }

        internal void Raise()
        {
            SDL.RaiseWindow(this.IntPtr);

        }
    }
    public class TextureManager : Dictionary<Int64, Texture>, IDisposable
    {
        public void Dispose()
        {
        }
    }
    public class Renderer : SDLObject, IDisposable
    {
        public TextureManager TextureManager { get; set; }
        public Window Window { get; set; }

        public Renderer()
        {

        }
        public Renderer(Window window, int index, SDL.SDL_RendererFlags renderFlags)
        {
            this.Window = window;
            this.IntPtr = SDL.CreateRenderer((IntPtr)window, index, renderFlags);

        }
        public void Dispose()
        {
            SDL.DestroyRenderer(this.IntPtr);
            if (TextureManager != null)
            {
                var queue = new Queue<IntPtr>(TextureManager.Values.Select(x => x.IntPtr));
                while (queue.Any())
                {
                    SDL.DestroyTexture(queue.Dequeue());
                }

            }
        }

        public static explicit operator IntPtr(Renderer renderer)
        {
            return renderer.IntPtr;
        }

        public static explicit operator Renderer(IntPtr v)
        {
            return new Renderer { IntPtr = v };
        }

        internal int Clear()
        {
            return SDL.RenderClear(this.IntPtr);
        }

        internal int Copy(Texture texture, SDL.Rect sourceRect, SDL.Rect destRect)
        {
            //return SDL.SetRenderDrawBlendMode(this.IntPtr, SDL.SDL_BlendMode.SDL_BLENDMODE_ADD);
            return SDL.RenderCopy(this.IntPtr, texture.IntPtr, ref sourceRect, ref destRect);
        }

        internal void Present()
        {
            SDL.RenderPresent(this.IntPtr);
        }

        internal Texture LoadTexture(string filepath)
        {
            Texture texture=null;
            if (File.Exists(filepath))
            {
                if (this.TextureManager == null)
                    this.TextureManager = new TextureManager();
                texture = new Texture(this, filepath);
                this.TextureManager.Add(texture.IntPtr.ToInt64(), texture);
            }
            else
            {
                Console.Error.WriteLine($@"File not found: {filepath}");
            }
            return texture;
        }
    }

    public class Surface : SDLObject, IDisposable
    {
        public Surface() { }

        public Surface(string filePath)
        {
            //SDL.Surface surface;
            this.IntPtr = SDL2.Image.Load(filePath);

            //this._surface = SDL2.Image.ToSurface(IntPtr);



        }
        private SDL.Surface _surface { get; set; }

        public uint Flags { get { return _surface.flags; } }
        public IntPtr Format { get { return _surface.format; } }// SDL_PixelFormat*
        public int Width { get { return _surface.w; } }
        public int Height { get { return _surface.h; } }
        public int Pitch { get { return _surface.pitch; } }
        public IntPtr Pixels { get { return _surface.pixels; } } // void*
        public IntPtr UserData { get { return _surface.userdata; } } // void*
        public int Locked { get { return _surface.locked; } }
        public IntPtr LockData { get { return _surface.lock_data; } } // void*
        public SDL.Rect ClipRect { get { return _surface.clip_rect; } }
        public IntPtr Map { get { return _surface.map; } } // SDL_BlitMap*
        public int RefCount { get { return _surface.refcount; } }

        public void Dispose()
        {
            SDL.FreeSurface(this.IntPtr);
        }
    }
    public class Texture : SDLObject, IDisposable
    {
        public int Access { get; internal set; }
        public int Height { get { return Rect.h; } }
        public int Width { get { return Rect.w; } }
        public SDL.Rect Rect { get; internal set; }

        public uint Format { get; internal set; }
        public Renderer Renderer { get; set; }

        public string FilePath { get; private set; }
        public Surface Surface { get; private set; }

        public Texture()
        {

        }


        public Texture(Renderer renderer, string filePath)
        {
            //SDL.QueryTexture((IntPtr)texture, out format, out access, out w, out h);
            this.FilePath = filePath;
            //int access, h, w;
            //uint format;
            this.Surface = new Surface(filePath);
            this.IntPtr = SDL.CreateTextureFromSurface(renderer.IntPtr, Surface.IntPtr);
            //this.IntPtr = Image.IMG_LoadTexture(renderer.IntPtr, filePath);            
            SDL.QueryTexture(this.IntPtr, out uint format, out int access, out int w, out int h);
            this.Access = access;
            this.Rect = new SDL.Rect() { h = h, w = w };
            this.Format = format;
            this.Renderer = renderer;
            this.Rect = new SDL.Rect() { x = 0, y = 0, w = Width, h = Height };
        }

        public void Dispose()
        {
            SDL.DestroyTexture(this.IntPtr);
        }

        internal Sprite CreateSprite()
        {
            var r = new SDL.Rect { h = this.Height, w = this.Width };
            return new Sprite() { Texture = this, Rect = r };
        }

        public static explicit operator Texture(IntPtr v)
        {
            return new Texture() { IntPtr = v };
        }
        public static explicit operator IntPtr(Texture texture)
        {
            return texture.IntPtr;
        }


    }
    public class SDLObject
    {
        public IntPtr IntPtr { get; set; }
    }

}
