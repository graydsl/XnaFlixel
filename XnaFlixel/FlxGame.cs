using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaFlixel.data;

namespace XnaFlixel
{
    public class FlxGame : DrawableGameComponent
    {
    	#region Constants

    	#endregion

    	#region Fields

    	private string[] _helpStrings;

    	//effect stuff
    	private Point _quakeOffset = Point.Zero;

    	private FlxState _firstScreen;

    	private SoundEffect _sndBeep;

    	//basic display stuff
    	internal FlxState _state;
    	private RenderTarget2D _backRender;
    	internal int targetWidth = 0;
    	internal int targetLeft = 0;

    	//basic update stuff
    	private bool _paused;

    	//Pause screen, sound tray, support panel, dev console, and special effects objects
    	internal FlxPause _pausePanel;
    	internal bool _soundTrayVisible;
    	internal Rectangle _soundTrayRect;
    	internal float _soundTrayTimer;
    	internal FlxSprite[] _soundTrayBars;
    	internal FlxText _soundCaption;
    	internal FlxConsole _console;

    	#endregion

    	#region Properties

    	#endregion

    	#region Constructors

    	/// <summary>
    	/// Constructor added for better usability of X-Flixel.
    	/// </summary>
    	public FlxGame() : base(FlxG.Game)
    	{
    		
    	}

    	#endregion

    	#region Methods for/from SuperClass/Interface

    	public override void Initialize()
    	{
    		base.Initialize();

    		_backRender = new RenderTarget2D(GraphicsDevice, FlxG.width, FlxG.height, false, SurfaceFormat.Color,
    		                                DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
    	}

    	protected override void LoadContent()
    	{
    		//load up graphical content used for the flixel engine
    		targetWidth = (int)(GraphicsDevice.Viewport.Height * ((float)FlxG.width / (float)FlxG.height));
    		targetLeft = (GraphicsDevice.Viewport.Width - targetWidth) / 2;

    		FlxG.LoadContent(GraphicsDevice);
    		_sndBeep = FlxG.Game.Content.Load<SoundEffect>("Flixel/beep");

    		InitConsole();

    		if (_firstScreen != null)
    		{
    			FlxG.state = _firstScreen;
    			_firstScreen = null;
    		}
    	}

    	protected override void UnloadContent()
    	{
    		_sndBeep.Dispose();

    		if (FlxG.state != null)
    		{
    			FlxG.state.Destroy();
    		}
    	}

    	public override void Update(GameTime gameTime)
    	{
    		PlayerIndex pi;

    		// added by graydsl
    		FlxG.GameTime = gameTime;

    		//Frame timing
    		FlxG.getTimer = (uint)gameTime.TotalGameTime.TotalMilliseconds;
    		FlxG.elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
    		if (FlxG.elapsed > FlxG.maxElapsed)
    			FlxG.elapsed = FlxG.maxElapsed;
    		FlxG.elapsed *= FlxG.timeScale;
            
    		//Animate flixel HUD elements
    		_console.update();

    		if(_soundTrayTimer > 0)
    			_soundTrayTimer -= FlxG.elapsed;
    		else if(_soundTrayRect.Y > -_soundTrayRect.Height)
    		{
    			_soundTrayRect.Y -= (int)(FlxG.elapsed * FlxG.height * 2);
    			_soundCaption.Y = (_soundTrayRect.Y + 4);
    			for (int i = 0; i < _soundTrayBars.Length; i++)
    			{
    				_soundTrayBars[i].Y = (_soundTrayRect.Y + _soundTrayRect.Height - _soundTrayBars[i].Height - 2);
    			}
    			if(_soundTrayRect.Y < -_soundTrayRect.Height)
    				_soundTrayVisible = false;
    		}

    		//State updating
    		FlxG.keys.update();
    		FlxG.gamepads.update();
    		FlxG.mouse.update();
    		FlxG.updateSounds();
    		if (FlxG.keys.isNewKeyPress(Keys.D0, null, out pi))
    		{
    			FlxG.mute = !FlxG.mute;
    			ShowSoundTray();
    		}
    		else if (FlxG.keys.isNewKeyPress(Keys.OemMinus, null, out pi))
    		{
    			FlxG.mute = false;
    			FlxG.volume -= 0.1f;
    			ShowSoundTray();
    		}
    		else if (FlxG.keys.isNewKeyPress(Keys.OemPlus, null, out pi))
    		{
    			FlxG.mute = false;
    			FlxG.volume += 0.1f;
    			ShowSoundTray();
    		}
    		else if (FlxG.keys.isNewKeyPress(Keys.D1, null, out pi) || FlxG.keys.isNewKeyPress(Keys.OemTilde, null, out pi))
    		{
    			_console.toggle();
    		}
    		else if (FlxG.autoHandlePause && (FlxG.keys.isPauseGame(FlxG.controllingPlayer) || FlxG.gamepads.isPauseGame(FlxG.controllingPlayer)))
    		{
    			FlxG.pause = !FlxG.pause;
    		}

    		if (_paused)
    			return;

    		if (FlxG.state != null)
    		{
    			//Update the camera and game state
    			FlxG.doFollow();
    			FlxG.state.Update();

    			//Update the various special effects
    			if (FlxG.flash.Exists)
    				FlxG.flash.Update();
    			if (FlxG.fade.Exists)
    				FlxG.fade.Update();
    			FlxG.quake.update();
    			_quakeOffset.X = FlxG.quake.x;
    			_quakeOffset.Y = FlxG.quake.y;
    		}
    	}


    	//Rendering
    	public override void Draw(GameTime gameTime)
    	{
    		//Render the screen to our internal game-sized back buffer.
    		GraphicsDevice.SetRenderTarget(_backRender);
    		if (FlxG.state != null)
    		{
    			FlxG.state.PreProcess(FlxG.spriteBatch);
    			FlxG.state.Render(FlxG.spriteBatch);

    			FlxG.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
    			if (FlxG.flash.Exists)
    				FlxG.flash.Render(FlxG.spriteBatch);
    			if (FlxG.fade.Exists)
    				FlxG.fade.Render(FlxG.spriteBatch);

    			if (FlxG.mouse.cursor.Visible)
    				FlxG.mouse.cursor.Render(FlxG.spriteBatch);

    			FlxG.spriteBatch.End();

    			FlxG.state.PostProcess(FlxG.spriteBatch);
    		}
    		//Render sound tray if necessary
    		if (_soundTrayVisible || _paused)
    		{
    			FlxG.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
    			//GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
    			//GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
    			//GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;
    			if (_soundTrayVisible)
    			{
    				FlxG.spriteBatch.Draw(FlxG.XnaSheet, _soundTrayRect,
    				                      new Rectangle(1,1,1,1), _console.color);
    				_soundCaption.Render(FlxG.spriteBatch);
    				for (int i = 0; i < _soundTrayBars.Length; i++)
    				{
    					_soundTrayBars[i].Render(FlxG.spriteBatch);
    				}
    			}
    			if (_paused)
    			{
    				_pausePanel.render(FlxG.spriteBatch);
    			}
    			FlxG.spriteBatch.End();
    		}
    		GraphicsDevice.SetRenderTarget(null);

    		//Copy the result to the screen, scaled to fit
    		GraphicsDevice.Clear(FlxG.backColor);
    		FlxG.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
    		//GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
    		//GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
    		//GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;
    		FlxG.spriteBatch.Draw(_backRender,
    		                      new Rectangle(targetLeft + _quakeOffset.X, _quakeOffset.Y, targetWidth, GraphicsDevice.Viewport.Height),
    		                      Color.White);
    		//Render console if necessary
    		if (_console.visible)
    		{
    			_console.render(FlxG.spriteBatch);
    		}
    		FlxG.spriteBatch.End();
    	}

    	#endregion

    	#region Static Methods

    	#endregion

    	#region Public Methods

    	public void InitGame(int GameSizeX, int GameSizeY,
    	                     FlxState InitialState, Color BGColor,
    	                     bool showFlixelLogo, Color logoColor)
    	{
    		FlxG.backColor = BGColor;
    		FlxG.setGameData(this, GameSizeX, GameSizeY);

    		_paused = false;

    		//activate the first screen.
    		if (showFlixelLogo == false)
    		{
    			_firstScreen = InitialState;
    		}
    		else
    		{
    			FlxSplash.setSplashInfo(logoColor, InitialState);
    			_firstScreen = new FlxSplash();
    		}
    	}

    	//@desc		Sets up the strings that are displayed on the left side of the pause game popup
    	//@param	X		What to display next to the X button
    	//@param	C		What to display next to the C button
    	//@param	Mouse	What to display next to the mouse icon
    	//@param	Arrows	What to display next to the arrows icon
    	protected void Help(string X, string C, string Mouse, string Arrows)
    	{
    		_helpStrings = new string[4];

    		if(X != null)
    			_helpStrings[0] = X;
    		if(C != null)
    			_helpStrings[1] = C;
    		if(Mouse != null)
    			_helpStrings[2] = Mouse;
    		if(Arrows != null)
    			_helpStrings[3] = Arrows;

    		if (_pausePanel != null)
    		{
    			_pausePanel.helpX = _helpStrings[0];
    			_pausePanel.helpC = _helpStrings[1];
    			_pausePanel.helpMouse = _helpStrings[2];
    			_pausePanel.helpArrows = _helpStrings[3];
    		}
    	}

    	public void SwitchState(FlxState newscreen)
    	{
    		FlxG.unfollow();
    		FlxG.keys.reset();
    		FlxG.gamepads.reset();
    		FlxG.mouse.reset();

    		FlxG.flash.stop();
    		FlxG.fade.stop();
    		FlxG.quake.stop();

    		if (_state != null)
    		{
    			_state.Destroy();
    		}
    		_state = newscreen;
    		_state.Create();
    	}

    	#endregion

    	#region Private Methods

    	private void InitConsole()
    	{
    		//initialize the debug console
    		_console = new FlxConsole(targetLeft, targetWidth);

    		_console.log(FlxG.LIBRARY_NAME +
    		             " v" + FlxG.LIBRARY_MAJOR_VERSION.ToString() + "." + FlxG.LIBRARY_MINOR_VERSION.ToString());
    		_console.log("---------------------------------------");

    		//Pause screen popup
    		_pausePanel = new FlxPause();
    		if (_helpStrings != null)
    		{
    			_pausePanel.helpX = _helpStrings[0];
    			_pausePanel.helpC = _helpStrings[1];
    			_pausePanel.helpMouse = _helpStrings[2];
    			_pausePanel.helpArrows = _helpStrings[3];
    		}

    		//Sound Tray popup
    		_soundTrayRect = new Rectangle((FlxG.width - 80) / 2, -30, 80, 30);
    		_soundTrayVisible = false;
			
    		_soundCaption = new FlxText((FlxG.width - 80) / 2, -10, 80, "VOLUME");
    		_soundCaption.SetFormat(null, 1, Color.White, FlxJustification.Center, Color.White).Height = 10;

    		int bx = 10;
    		int by = 14;
    		_soundTrayBars = new FlxSprite[10];
    		for(int i = 0; i < 10; i++)
    		{
    			_soundTrayBars[i] = new FlxSprite(_soundTrayRect.X + (bx * 1), -i, null);
    			_soundTrayBars[i].Width = 4;
    			_soundTrayBars[i].Height = i + 1;
    			_soundTrayBars[i].scrollFactor = Vector2.Zero;
    			bx += 6;
    			by--;
    		}
    	}

    	//@desc		Switch from one FlxState to another
    	//@param	State		The class name of the state you want (e.g. PlayState)

    	/// <summary>
    	/// Internal function to help with basic pause game functionality.
    	/// </summary>
    	internal void UnpauseGame()
    	{
    		//if(!FlxG.panel.visible) flash.ui.Mouse.hide();
    		FlxG.resetInput();
    		_paused = false;
    		//stage.frameRate = _framerate;
    	}

    	/// <summary>
    	/// Internal function to help with basic pause game functionality.
    	/// </summary>
    	internal void PauseGame()
    	{
    		//if((X != 0) || (Y != 0))
    		//{
    		//    X = 0;
    		//    Y = 0;
    		//}
    		//flash.ui.Mouse.show();
    		_paused = true;
    		//stage.frameRate = _frameratePaused;
    	}

    	//@desc		This is the main game loop

    	//@desc		This function is only used by the FlxGame class to do important internal management stuff
    	private void ShowSoundTray()
    	{
    		if (!FlxG.mute)
    		{
    			_sndBeep.Play(FlxG.volume, 0f, 0f);
    		}
    		_soundTrayTimer = 1;
    		_soundTrayRect.Y = 0;
    		_soundTrayVisible = true;

    		_soundCaption.Y = (_soundTrayRect.Y + 4);

    		int gv = (int)Math.Round(FlxG.volume * 10);
    		if(FlxG.mute)
    			gv = 0;
    		for (int i = 0; i < _soundTrayBars.Length; i++)
    		{
    			_soundTrayBars[i].Y = (_soundTrayRect.Y + _soundTrayRect.Height - _soundTrayBars[i].Height - 2);
    			if(i < gv) _soundTrayBars[i].alpha = 1;
    			else _soundTrayBars[i].alpha = 0.5f;
    		}
    	}

    	#endregion

    	//pause stuff

    	//@desc		Constructor
        //@param	GameSizeX		The width of your game in pixels (e.g. 320)
        //@param	GameSizeY		The height of your game in pixels (e.g. 240)
        //@param	InitialState	The class name of the state you want to create and switch to first (e.g. MenuState)
        //@param	BGColor			The color of the app's background
        //@param	FlixelColor		The color of the great big 'f' in the flixel logo

    	//@benbaird initializes the console, the pause overlay, and the soundbar
    }

}
