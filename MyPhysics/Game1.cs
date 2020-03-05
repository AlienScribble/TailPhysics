using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AlienScribble;
using System;

namespace MyPhysics {
    public class Game1 : Game {
        #region USUAL STUFF
        // DISPLAY
        const int SCREENWIDTH = 1024, SCREENHEIGHT = 768;     // TARGET FORMAT
        const bool FULLSCREEN = false;                        // not fullscreen because using windowed fill-screen mode 
        GraphicsDeviceManager graphics;     PresentationParameters pp;     SpriteBatch spriteBatch;      QuadBatch quadBatch;      SpriteFont font;
        static public int screenW, screenH;
        //INPUT & UTILS
        Text text;    Input inp;     static public Random rnd;
        //RECTANGLES
        Rectangle screenRect, desktopRect, pixel;             // render target size, desktop screen size        
        //RENDERTARGETS & TEXTURES
        RenderTarget2D MainTarget;                           // render to a standard target and fit it to the desktop resolution
        #endregion

        Texture2D tex;
        Rectangle tailRec;

        //VARS
        Vector2    mpos;
        TailBone[] bones;
        float      tail_size = 20f;


        #region C O N S T R U C T
        public Game1() {
            int initial_screen_width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 10;    int initial_screen_height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 10;
            graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = initial_screen_width,   PreferredBackBufferHeight = initial_screen_height,  IsFullScreen = FULLSCREEN,   PreferredDepthStencilFormat = DepthFormat.Depth16  };
            Mouse.SetPosition(500, 380);
            Window.IsBorderless = true; IsMouseVisible = true; Content.RootDirectory = "Content";
        }
        #endregion


        //--------
        // I N I T
        //--------
        protected override void Initialize() {
            #region // SETUP SPRITEBATCH AND GET TRUE DISPLAY
            spriteBatch = new SpriteBatch(GraphicsDevice); pp = GraphicsDevice.PresentationParameters; SurfaceFormat format = pp.BackBufferFormat;
            MainTarget  = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT);            
            screenW     = MainTarget.Width;   screenH = MainTarget.Height;
            desktopRect = new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);   screenRect = new Rectangle(0, 0, screenW, screenH);
            quadBatch   = new QuadBatch(Content, GraphicsDevice, "QuadEffect", "FontTexture", screenW, screenH); // setup distortable quad class (like spriteBatch)            
            pixel       = new Rectangle(0, 0, 1, 1); quadBatch.PIXEL = pixel; 
            rnd         = new Random();   inp = new Input(pp, MainTarget);
            text        = new Text(quadBatch, screenW, screenH, inp, pixel); text.Size = 0.5f;
            #endregion            
            
            tailRec = new Rectangle(195, 130, 188, 44);

            // M A K E   B O N E S : : : 
            bones = new TailBone[5];
            for(int i=0; i<bones.Length; i++) { bones[i] = new TailBone(tail_size); }
            base.Initialize();
        }


        //--------------
        #region L O A D 
        //--------------
        protected override void LoadContent() {
            font = Content.Load<SpriteFont>("Font");
            tex  = Content.Load<Texture2D>("grass"); 

        } protected override void UnloadContent() { }
        #endregion


        //------------
        // U P D A T E 
        //------------      
        const float tail_wave_size = 1.2f;                    // size of wave to apply to tail
        Vector2 wave(int off) { return new Vector2(0f, (float)Math.Sin(rr + off*0.1f) * tail_wave_size); }
        float x_dir;
        double rr;
        protected override void Update(GameTime gameTime) {
            inp.Update();   if (inp.KeyPress(Keys.Escape)) Exit();         // (EXIT - Change Later)             

            // U P D A T E   B O N E S : : :             
            mpos = inp.mosV;
            float x_dif = mpos.X - inp.omosV.X;
            if (x_dif != 0) x_dir = x_dif;                 // get last non-zero direction of movement  
            float h_bias = 9f;                             // horizontal bias
            rr += 0.1; if (rr > 6.28) rr -= 6.28;      // loop some rotation value to wave the tail a little
            bones[0].Update(mpos, x_dir, h_bias);
            bones[0].pos += wave(0);
            for (int i = 1; i < bones.Length; i++) {
                bones[i].Update(bones[i-1].pos, x_dir, h_bias);
                bones[i].pos += wave(i);
                h_bias *= 0.4f;
            }            
            base.Update(gameTime);
        }
        //--------
        // D R A W
        //--------
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(MainTarget);  GraphicsDevice.Clear(Color.TransparentBlack);


            quadBatch.Begin(tex, BlendState.AlphaBlend);       
            quadBatch.DrawTail(tailRec, Color.White, mpos, bones, 1f);
            quadBatch.End();

            // DRAW MAINTARGET TO BACKBUFFER
            GraphicsDevice.SetRenderTarget(null); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone); spriteBatch.Draw(MainTarget, desktopRect, Color.White); spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
