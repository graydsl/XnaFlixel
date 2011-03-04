using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaFlixel.data;

namespace XnaFlixel
{
    //X-flixel only; Replaces the "facing" boolean member
    public enum Flx2DFacing
    {
        NotUsed = -1,
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3
    }

    /// <summary>
   /// The main "game object" class, handles basic physics and animation.
   /// </summary>
    public class FlxSprite : FlxObject
    {
    	#region Constants

    	#endregion

    	#region Fields

    	protected Texture2D _tex;
    	private bool _stretchToFit = false;

    	/// <summary>
    	/// If you changed the size of your sprite object to shrink the bounding box,
    	/// you might need to offset the new bounding box from the top-left corner of the sprite.
    	/// </summary>
    	public Point offset = Point.Zero;

    	/// <summary>
    	/// Change the size of your sprite's graphic.
    	/// NOTE: Scale doesn't currently affect collisions automatically,
    	/// you will need to adjust the width, height and offset manually.
    	/// </summary>
    	public float scale = 1f;
    	/// <summary>
    	/// Blending modes, just like Photoshop!
    	/// E.g. "multiply", "screen", etc.
    	/// @default null
    	/// </summary>
    	public string blend;
    	/// <summary>
    	/// Controls whether the object is smoothed when rotated, affects performance.
    	/// @default false
    	/// </summary>
    	public bool antialiasing;

    	/// <summary>
    	/// Whether the current animation has finished its first (or only) loop.
    	/// </summary>
    	public bool finished;
    	/// <summary>
    	/// The width of the actual graphic or image being displayed (not necessarily the game object/bounding box).
    	/// NOTE: Edit at your own risk!!  This is intended to be read-only.
    	/// </summary>
    	public int frameWidth;
    	/// <summary>
    	/// The height of the actual graphic or image being displayed (not necessarily the game object/bounding box).
    	/// NOTE: Edit at your own risk!!  This is intended to be read-only.
    	/// </summary>
    	public int frameHeight;
    	/// <summary>
    	/// The total number of frames in this image (assumes each row is full).
    	/// </summary>
    	public int frames;

    	//Animation helpers
    	private List<FlxAnim> _animations;
    	private uint _flipped;
    	protected FlxAnim _curAnim;
    	protected int _curFrame;
    	protected int _caf;
    	private float _frameTimer;
    	private FlxAnimationCallback _callback;
    	private Flx2DFacing _facing2d = Flx2DFacing.NotUsed;

    	//Various rendering helpers
    	protected Rectangle _flashRect;
    	protected Rectangle _flashRect2;
    	protected Point _flashPointZero;
    	protected float _alpha;
    	protected Color _color = Color.White;
    	private byte _bytealpha = 0xff;

    	#endregion

    	#region Properties

    	public Flx2DFacing facing
    	{
    		get
    		{
    			return _facing2d;
    		}
    		set
    		{
    			bool c = _facing2d != value;
    			_facing2d = value;
    			if(c) CalcFrame();
    		}
    	}

    	/// <summary>
    	/// Tell the sprite to change to a specific frame of animation.
    	/// 
    	/// @param	Frame	The frame you want to display.
    	/// </summary>
    	public int frame
    	{
    		get
    		{
    			return _caf;
    		}
    		set
    		{
    			_curAnim = null;
    			_caf = value;
    			CalcFrame();
    		}
    	}

    	public Color color
    	{
    		get { return _color; }
    		set { _color = new Color(value.R, value.G, value.B, _bytealpha); }
    	}

    	public float alpha
    	{
    		get { return _alpha; }
    		set { _alpha = value; _bytealpha = (byte)(255f * _alpha); _color = new Color(_color.R, _color.G, _color.B, _bytealpha); }
    	}

    	public List<FlxAnim> animations
    	{
    		get { return _animations; }
    	}

    	#endregion

    	#region Constructors

    	/// <summary>
    	/// Creates a white 8x8 square <code>FlxSprite</code> at the specified position.
    	/// Optionally can load a simple, one-frame graphic instead.
    	/// 
    	/// @param	X				The initial X position of the sprite.
    	/// @param	Y				The initial Y position of the sprite.
    	/// @param	SimpleGraphic	The graphic you want to display (OPTIONAL - for simple stuff only, do NOT use for animated images!).
    	/// </summary>
    	public FlxSprite()
    	{
    		// call our Initialize
    		Initialize(0, 0, null);
    	}

    	public FlxSprite(float X, float Y)
    	{
    		// call our Initialize
    		Initialize(X, Y, null);
    	}
    	public FlxSprite(float X, float Y, Texture2D SimpleGraphic)
    	{
    		// call our Initialize
    		Initialize(X, Y, SimpleGraphic);
    	}

    	private void Initialize(float X, float Y, Texture2D SimpleGraphic)
    	{
    		base.X = X;
    		base.Y = Y;

    		_flashRect = new Rectangle();
    		_flashRect2 = new Rectangle();
    		_flashPointZero = new Point();
    		offset = new Point();

    		scale = 1f;
    		_alpha = 1;
    		_color = Color.White;
    		blend = null;
    		antialiasing = false;

    		finished = false;
    		facing = Flx2DFacing.NotUsed;
    		_animations = new List<FlxAnim>();
    		_flipped = 0;
    		_curAnim = null;
    		_curFrame = 0;
    		_caf = 0;
    		_frameTimer = 0;

    		//_mtx = new Matrix();
    		_callback = null;
    		//if (_gfxSprite == null)
    		//{
    		//    _gfxSprite = new Sprite();
    		//    _gfx = _gfxSprite.graphics;
    		//}

    		if (SimpleGraphic == null)
    			CreateGraphic(8, 8, color);
    		else
    			LoadGraphic(SimpleGraphic);
    	}

    	#endregion

    	#region Methods for/from SuperClass/Interface

    	/// <summary>
    	/// Call this function to figure out the on-screen position of the object.
    	/// 
    	/// @param	P	Takes a <code>Point</code> object and assigns the post-scrolled X and Y values of this object to it.
    	/// 
    	/// @return	The <code>Point</code> you passed in, or a new <code>Point</code> if you didn't pass one, containing the screen X and Y position of this object.
    	/// </summary>
    	override public Vector2 GetScreenXy()
    	{
    		Vector2 Point = Vector2.Zero;
    		Point.X = FlxU.floor(X + FlxU.roundingError) + FlxU.floor(FlxG.scroll.X * scrollFactor.X) - offset.X;
    		Point.Y = FlxU.floor(Y + FlxU.roundingError) + FlxU.floor(FlxG.scroll.Y * scrollFactor.Y) - offset.Y;
    		return Point;
    	}

    	/// <summary>
    	/// Main game loop update function.  Override this to create your own sprite logic!
    	/// Just don't forget to call super.update() or any of the helper functions.
    	/// </summary>
    	public override void Update()
    	{
    		UpdateMotion();
    		UpdateAnimation();
    		UpdateFlickering();
    	}

    	/// <summary>
    	/// Called by game loop, updates then blits or renders current frame of animation to the screen
    	/// </summary>
    	public override void Render(SpriteBatch spriteBatch)
    	{
    		if (Visible == false || Exists == false)
    		{
    			return;
    		}

    		//X-flixel only: adjust for origin (by default we match flixel's behavior by using a center origin).
    		Vector2 pos = Vector2.Zero;
    		Vector2 vc = Vector2.Zero;

    		pos = GetScreenXy() + origin;
    		pos += (new Vector2(_flashRect.Width - Width, _flashRect.Height - Height)
    		        * (origin / new Vector2(Width, Height)));

    		//the origin must be recalculated based on the difference between the
    		//object's actual (collision) dimensions and its art (animation) dimensions.
    		vc = new Vector2(_flashRect.Width, _flashRect.Height);
    		if (!_stretchToFit)
    		{
    			vc *= (origin / new Vector2(Width, Height));
    		}
    		else
    		{
    			vc *= (origin / new Vector2(Width + 1, Height + 1));
    		}

    		if (_facing2d == Flx2DFacing.NotUsed)
    		{
    			if (_stretchToFit)
    			{
    				//if (width == 7)
    				//    vc = vc;
    				spriteBatch.Draw(_tex, new Rectangle((int)pos.X, (int)pos.Y, (int)Width, (int)Height),
    				                 _flashRect, _color,
    				                 _radians, vc, SpriteEffects.None, 0);
    			}
    			else
    			{
    				spriteBatch.Draw(_tex, pos,
    				                 _flashRect, _color,
    				                 _radians, vc, scale, SpriteEffects.None, 0);
    			}
    		}
    		else if (_facing2d == Flx2DFacing.Right)
    		{
    			spriteBatch.Draw(_tex, pos,
    			                 _flashRect, _color,
    			                 _radians, vc, scale, SpriteEffects.None, 0);
    		}
    		else
    		{
    			spriteBatch.Draw(_tex, pos,
    			                 _flashRect, _color,
    			                 _radians, vc, scale, SpriteEffects.FlipHorizontally, 0);
    		}

    		if (FlxG.showBounds)
    		{
    			pos = GetScreenXy();
    			pos.X += offset.X;
    			pos.Y += offset.Y;
    			DrawBounds(spriteBatch, (int)pos.X, (int)pos.Y);
    		}
    	}

    	/// <summary>
    	/// Checks to see if a point in 2D space overlaps this FlxCore object.
    	/// 
    	/// @param	X			The X coordinate of the point.
    	/// @param	Y			The Y coordinate of the point.
    	/// @param	PerPixel	Whether or not to use per pixel collision checking.
    	/// 
    	/// @return	Whether or not the point overlaps this object.
    	/// </summary>
    	override public bool OverlapsPoint(float X, float Y, bool PerPixel)
    	{
    		X = X + FlxU.floor(FlxG.scroll.X);
    		Y = Y + FlxU.floor(FlxG.scroll.Y);
    		_point = GetScreenXy();
    		//if(PerPixel)
    		//    return _framePixels.hitTest(new Point(0,0),0xFF,new Point(X-_point.X,Y-_point.Y));
    		//else
    		if (_stretchToFit == false)
    		{
    			if ((X <= _point.X) || (X >= _point.X + frameWidth) || (Y <= _point.Y) || (Y >= _point.Y + frameHeight))
    				return false;
    		}
    		else
    		{
    			if ((X <= _point.X) || (X >= _point.X + Width) || (Y <= _point.Y) || (Y >= _point.Y + Height))
    				return false;
    		}
    		return true;
    	}

    	#endregion

    	#region Static Methods

    	#endregion

    	#region Public Methods

    	/// <summary>
    	/// Load an image from an embedded graphic file.
    	/// 
    	/// @param	Graphic		The image you want to use.
    	/// @param	Animated	Whether the Graphic parameter is a single sprite or a row of sprites.
    	/// @param	Reverse		Whether you need this class to generate horizontally flipped versions of the animation frames.
    	/// @param	Width		OPTIONAL - Specify the width of your sprite (helps FlxSprite figure out what to do with non-square sprites or sprite sheets).
    	/// @param	Height		OPTIONAL - Specify the height of your sprite (helps FlxSprite figure out what to do with non-square sprites or sprite sheets).
    	/// 
    	/// @return	This FlxSprite instance (nice for chaining stuff together, if you're into that).
    	/// </summary>
    	public virtual FlxSprite LoadGraphic(Texture2D Graphic)
    	{
    		return LoadGraphic(Graphic, false, false, 0, 0);
    	}

    	public virtual FlxSprite LoadGraphic(Texture2D Graphic, bool Animated)
    	{
    		return LoadGraphic(Graphic, Animated, false, 0, 0);
    	}

    	public virtual FlxSprite LoadGraphic(Texture2D Graphic, bool Animated, bool Reverse, int Width)
    	{
    		return LoadGraphic(Graphic, Animated, Reverse, Width, 0);
    	}

    	public virtual FlxSprite LoadGraphic(Texture2D Graphic, bool Animated, bool Reverse, int Width, int Height)
    	{
    		_stretchToFit = false;
    		_tex = Graphic;
    		if(Reverse)
    			_flipped = (uint)Graphic.Width>>1;
    		else
    			_flipped = 0;
    		if(Width == 0)
    		{
    			if(Animated)
    				Width = Graphic.Height;
    			else if(_flipped > 0)
    				Width = (int)((float)Graphic.Width * 0.5f);
    			else
    				Width = Graphic.Width;
    		}
    		base.Width = frameWidth = Width;
    		if(Height == 0)
    		{
    			if(Animated)
    				Height = (int)base.Width;
    			else
    				Height = Graphic.Height;
    		}
    		base.Height = frameHeight = Height;
    		ResetHelpers();
    		return this;
    	}

    	/// <summary>
    	/// This function creates a flat colored square image dynamically.
    	/// 
    	/// @param	Width		The width of the sprite you want to generate.
    	/// @param	Height		The height of the sprite you want to generate.
    	/// @param	Color		Specifies the color of the generated block.
    	/// 
    	/// @return	This FlxSprite instance (nice for chaining stuff together, if you're into that).
    	/// </summary>
    	public FlxSprite CreateGraphic(int Width, int Height, Color Color)
    	{
    		_tex = FlxG.XnaSheet;
    		_stretchToFit = true;
    		_facing2d = Flx2DFacing.NotUsed;

    		frameWidth = 1;
    		frameHeight = 1;
    		base.Width = Width;
    		base.Height = Height;
    		_color = Color;
    		ResetHelpers();
    		return this;
    	}

    	/// <summary>
    	/// Resets some important variables for sprite optimization and rendering.
    	/// </summary>
    	protected void ResetHelpers()
    	{
    		_flashRect2.X = 0;
    		_flashRect2.Y = 0;
    		if (_stretchToFit == false)
    		{
    			_flashRect.X = 0;
    			_flashRect.Y = 0;
    			_flashRect.Width = frameWidth;
    			_flashRect.Height = frameHeight;

    			_flashRect2.Width = _tex.Width;
    			_flashRect2.Height = _tex.Height;
    		}
    		else
    		{
    			_flashRect.X = 1;
    			_flashRect.Y = 1;
    			_flashRect.Width = frameWidth;
    			_flashRect.Height = frameHeight;

    			_flashRect2.Width = (int)Width;
    			_flashRect2.Height = (int)Height;
    		}

    		_origin.X = frameWidth*0.5f;
    		_origin.Y = frameHeight*0.5f;

    		frames = (_flashRect2.Width / _flashRect.Width) * (_flashRect2.Height / _flashRect.Height);

    		_caf = 0;
    		RefreshHulls();
    	}

    	/// <summary>
    	/// Internal function for updating the sprite's animation.
    	/// Useful for cases when you need to update this but are buried down in too many supers.
    	/// This function is called automatically by <code>FlxSprite.update()</code>.
    	/// </summary>
    	protected void UpdateAnimation()
    	{
    		if((_curAnim != null) && (_curAnim.delay > 0) && (_curAnim.looped || !finished))
    		{
    			_frameTimer += FlxG.elapsed;
    			while(_frameTimer > _curAnim.delay)
    			{
    				_frameTimer = _frameTimer - _curAnim.delay;
    				if(_curFrame == _curAnim.frames.Length-1)
    				{
    					if(_curAnim.looped) _curFrame = 0;
    					finished = true;
    				}
    				else
    					_curFrame++;
    				_caf = _curAnim.frames[_curFrame];
    				CalcFrame();
    			}
    		}
    	}

    	/// <summary>
    	/// Triggered whenever this sprite is launched by a <code>FlxEmitter</code>.
    	/// </summary>
    	virtual public void OnEmit() { }

    	/// <summary>
    	/// Adds a new animation to the sprite.
    	/// 
    	/// @param	Name		What this animation should be called (e.g. "run").
    	/// @param	Frames		An array of numbers indicating what frames to play in what order (e.g. 1, 2, 3).
    	/// @param	FrameRate	The speed in frames per second that the animation should play at (e.g. 40 fps).
    	/// @param	Looped		Whether or not the animation is looped or just plays once.
    	/// </summary>
    	public void AddAnimation(string Name, int[] Frames)
    	{
    		_animations.Add(new FlxAnim(Name, Frames, 0, true));
    	}

    	public void AddAnimation(string Name, int[] Frames, int FrameRate)
    	{
    		_animations.Add(new FlxAnim(Name, Frames, FrameRate, true));
    	}

    	public void AddAnimation(string Name, int[] Frames, int FrameRate, bool Looped)
    	{
    		_animations.Add(new FlxAnim(Name, Frames, FrameRate, Looped));
    	}

    	/// <summary>
    	/// Pass in a function to be called whenever this sprite's animation changes.
    	/// 
    	/// @param	AnimationCallback		A function that has 3 parameters: a string name, a uint frame number, and a uint frame index.
    	/// </summary>
    	public void AddAnimationCallback(FlxAnimationCallback ac)
    	{
    		_callback = ac;
    	}

    	/// <summary>
    	/// Plays an existing animation (e.g. "run").
    	/// If you call an animation that is already playing it will be ignored.
    	/// 
    	/// @param	AnimName	The string name of the animation you want to play.
    	/// @param	Force		Whether to force the animation to restart.
    	/// </summary>
    	public void Play(string AnimName, bool Force)
    	{
    		if(!Force && (_curAnim != null) && (AnimName == _curAnim.name)) return;
    		_curFrame = 0;
    		_caf = 0;
    		_frameTimer = 0;
    		for(int i = 0; i < _animations.Count; i++)
    		{
    			if(_animations[i].name == AnimName)
    			{
    				_curAnim = _animations[i];
    				if (_curAnim.delay <= 0)
    					finished = true;
    				else
    					finished = false;
    				_caf = _curAnim.frames[_curFrame];
    				CalcFrame();
    				return;
    			}
    		}
    	}

    	public void Play(string AnimName)
    	{
    		Play(AnimName, false);
    	}

    	//@desc		Tell the sprite which way to face (you can just set 'facing' but this function also updates the animation instantly)
    	//@param	In "normal" flixel, this is an int.

    	/// <summary>
    	/// Tell the sprite to change to a random frame of animation
    	/// Useful for instantiating particles or other weird things.
    	/// </summary>
    	public void RandomFrame()
    	{
    		_curAnim = null;
    		_caf = (int)(FlxU.random()*(_tex.Width/frameWidth));
    		CalcFrame();
    	}

    	/// <summary>
    	/// Internal function to update the current animation frame.
    	/// </summary>
    	protected void CalcFrame()
    	{
    		if (_stretchToFit)
    		{
    			_flashRect = new Rectangle(0, 0, frameWidth, frameHeight);
    			return;
    		}
    		else
    		{
    			uint rx = (uint)(_caf * frameWidth);
    			uint ry = 0;

    			//Handle sprite sheets
    			uint w = (uint)_tex.Width;
    			if (rx >= w)
    			{
    				ry = (uint)(rx / w) * (uint)frameHeight;
    				rx %= w;
    			}

    			//handle reversed sprites
    			if ((_facing2d == Flx2DFacing.Left) && (_flipped > 0))
    				rx = (_flipped << 1) - rx - (uint)frameWidth;

    			_flashRect = new Rectangle((int)rx, (int)ry, frameWidth, frameHeight);
    		}
    		if (_callback != null && _curAnim != null) _callback(_curAnim.name, (uint)_curFrame, _caf);
    	}

    	protected void DrawBounds(SpriteBatch spriteBatch, int X, int Y)
    	{
    		spriteBatch.Draw(FlxG.XnaSheet,
    		                 new Rectangle((int)(FlxU.floor(base.X + FlxU.roundingError) + FlxU.floor(FlxG.scroll.X * scrollFactor.X)),
    		                               (int)(FlxU.floor(base.Y + FlxU.roundingError) + FlxU.floor(FlxG.scroll.Y * scrollFactor.Y)), (int)Width, (int)Height),
    		                 new Rectangle(1, 1, 1, 1), GetBoundingColor());
    	}

    	#endregion

    	#region Private Methods

    	#endregion
        //@benbaird X-flixel only stuff
    }

}
