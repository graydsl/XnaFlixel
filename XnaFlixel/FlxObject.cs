using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XnaFlixel
{
	/// <summary>
	/// This is the base class for most of the display objects (<code>FlxSprite</code>, <code>FlxText</code>, etc).
	/// It includes some basic attributes about game objects, including retro-style flickering,
	/// basic state information, sizes, scrolling, and basic physics & motion.
	/// </summary>
    public class FlxObject
    {
		#region Constants

		#endregion

		#region Fields

		/// <summary>
		/// The basic speed of this object.
		/// </summary>
		public Vector2 velocity;
		/// <summary>
		/// How fast the speed of this object is changing.
		/// Useful for smooth movement and gravity.
		/// </summary>
		public Vector2 acceleration;
		/// <summary>
		/// This isn't drag exactly, more like deceleration that is only applied
		/// when acceleration is not affecting the sprite.
		/// </summary>
		public Vector2 drag;
		/// <summary>
		/// If you are using <code>acceleration</code>, you can use <code>maxVelocity</code> with it
		/// to cap the speed automatically (very useful!).
		/// </summary>
		public Vector2 maxVelocity;

		//@benbaird We keep some private angle-related members in X-flixel due to XNA rotation differences
		protected float _angle = 0f;
		protected float _radians = 0f;

		/// <summary>
		/// WARNING: The origin of the sprite will default to its center.
		/// If you change this, the visuals and the collisions will likely be
		/// pretty out-of-sync if you do any rotation.
		/// </summary>
		// modified for X-flixel
		protected Vector2 _origin = Vector2.Zero;

		/// <summary>
		/// A handy "empty point" object
		/// </summary>
		static protected Vector2 _pZero = Vector2.Zero;
		
		/// <summary>
		/// A point that can store numbers from 0 to 1 (for X and Y independently)
		/// that governs how much this object is affected by the camera subsystem.
		/// 0 means it never moves, like a HUD element or far background graphic.
		/// 1 means it scrolls along a the same speed as the foreground layer.
		/// scrollFactor is initialized as (1,1) by default.
		/// </summary>
		public Vector2 scrollFactor;
		/// <summary>
		/// Internal helper used for retro-style flickering.
		/// </summary>
		protected bool _flicker;
		/// <summary>
		/// Internal helper used for retro-style flickering.
		/// </summary>
		protected float _flickerTimer;

		/// <summary>
		/// This is just a pre-allocated x-y point container to be used however you like
		/// </summary>
		protected Vector2 _point;
		/// <summary>
		/// This is just a pre-allocated rectangle container to be used however you like
		/// </summary>
		protected Rectangle _rect;
		/// <summary>
		/// This is a pre-allocated Flash Point object, which is useful for certain Flash graphics API calls
		/// </summary>
		protected Vector2 _flashPoint;

		/// <summary>
		/// These store a couple of useful numbers for speeding up collision resolution.
		/// </summary>
		public FlxRect colHullX;
		/// <summary>
		/// These store a couple of useful numbers for speeding up collision resolution.
		/// </summary>
		public FlxRect colHullY;
		/// <summary>
		/// These store a couple of useful numbers for speeding up collision resolution.
		/// </summary>
		public Vector2 colVector;
		/// <summary>
		/// An array of <code>FlxPoint</code> objects.  By default contains a single offset (0,0).
		/// </summary>
		public List<Vector2> colOffsets = new List<Vector2>();
		/// <summary>
		/// Dedicated internal flag for whether or not this class is a FlxGroup.
		/// </summary>
		internal bool _group;

		#endregion

		#region Properties

		/// <summary>
		/// Set <code>solid</code> to true if you want to collide this object.
		/// </summary>
		public bool Solid { get; set; }

		/// <summary>
		/// Set <code>fixed</code> to true if you want the object to stay in place during collisions.
		/// Useful for levels and other environmental objects.
		/// </summary>
		public bool Fixed { get; set; }

		/// <summary>
		/// Kind of a global on/off switch for any objects descended from <code>FlxObject</code>.
		/// </summary>
		public bool Exists { get; set; }

		/// <summary>
		/// If an object is not alive, the game loop will not automatically call <code>update()</code> on it.
		/// </summary>
		public bool Active { get; set; }

		/// <summary>
		/// If an object is not visible, the game loop will not automatically call <code>render()</code> on it.
		/// </summary>
		public bool Visible { get; set; }

		/// <summary>
		/// Set the angle of a sprite to rotate it.
		/// WARNING: rotating sprites decreases rendering
		/// performance for this sprite by a factor of 10x!
		/// </summary>
		public float Angle
		{
			get { return _angle; }
			set { _angle = value; _radians = MathHelper.ToRadians(_angle); }
		}

		/// <summary>
		/// This is how fast you want this sprite to spin.
		/// </summary>
		public float AngularVelocity { get; set; }

		/// <summary>
		/// How fast the spin speed should change.
		/// </summary>
		public float AngularAcceleration { get; set; }

		/// <summary>
		/// Like <code>drag</code> but for spinning.
		/// </summary>
		public float AngularDrag { get; set; }

		/// <summary>
		/// Use in conjunction with <code>angularAcceleration</code> for fluid spin speed control.
		/// </summary>
		public float MaxAngular { get; set; }

		virtual public Vector2 origin
		{
			get { return _origin; }
			set { _origin = value; }
		}

		/// <summary>
		/// If you want to do Asteroids style stuff, check out thrust,
		/// instead of directly accessing the object's velocity or acceleration.
		/// </summary>
		public float Thrust { get; set; }

		/// <summary>
		/// Used to cap <code>thrust</code>, helpful and easy!
		/// </summary>
		public float MaxThrust { get; set; }

		/// <summary>
		/// Handy for storing health percentage or armor points or whatever.
		/// </summary>
		public float Health { get; set; }

		/// <summary>
		/// Handy for tracking gameplay or animations.
		/// </summary>
		public bool Dead { get; set; }

		/// <summary>
		/// Set this to false if you want to skip the automatic motion/movement stuff (see <code>updateMotion()</code>).
		/// FlxObject and FlxSprite default to true.
		/// FlxText, FlxTileblock, FlxTilemap and FlxSound default to false.
		/// </summary>
		public bool Moves { get; set; }

		/// <summary>
		/// Flag that indicates whether or not you just hit the floor.
		/// Primarily useful for platformers, this flag is reset during the <code>updateMotion()</code>.
		/// </summary>
		public bool OnFloor { get; set; }

		/// <summary>
		/// Flag for direction collision resolution.
		/// </summary>
		public bool CollideLeft { get; set; }

		/// <summary>
		/// Flag for direction collision resolution.
		/// </summary>
		public bool CollideRight { get; set; }

		/// <summary>
		/// Flag for direction collision resolution.
		/// </summary>
		public bool CollideTop { get; set; }

		/// <summary>
		/// Flag for direction collision resolution.
		/// </summary>
		public bool CollideBottom { get; set; }

		// X-flixel only: Positioning variables to compensate for the fact that in
		// standard flixel, FlxObject inherits from FlxRect.
		public float X { get; set; }
		public float Y { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new <code>FlxObject</code>.
		/// 
		/// @param	X		The X-coordinate of the point in space.
		/// @param	Y		The Y-coordinate of the point in space.
		/// @param	Width	Desired width of the rectangle.
		/// @param	Height	Desired height of the rectangle.
		/// </summary>
		public FlxObject()
		{
			Initialize(0, 0, 0, 0);
		}

		public FlxObject(float X, float Y, float Width, float Height)
		{
			Initialize(X, Y, Width, Height);
		}

		private void Initialize(float X, float Y, float Width, float Height)
		{
			this.X = X;
			this.Y = Y;
			this.Width = Width;
			this.Height = Height;

			Exists = true;
			Active = true;
			Visible = true;
			Solid = true;
			Fixed = false;
			Moves = true;

			CollideLeft = true;
			CollideRight = true;
			CollideTop = true;
			CollideBottom = true;

			_origin = Vector2.Zero;

			velocity = Vector2.Zero;
			acceleration = Vector2.Zero;
			drag = Vector2.Zero;
			maxVelocity = new Vector2(10000, 10000);

			Angle = 0;
			AngularVelocity = 0;
			AngularAcceleration = 0;
			AngularDrag = 0;
			MaxAngular = 10000;

			Thrust = 0;

			scrollFactor = new Vector2(1, 1);
			_flicker = false;
			_flickerTimer = -1;
			Health = 1;
			Dead = false;
			_point = Vector2.Zero;
			_rect = Rectangle.Empty;
			_flashPoint = Vector2.Zero;

			colHullX = FlxRect.Empty;
			colHullY = FlxRect.Empty;
			colVector = Vector2.Zero;
			colOffsets.Add(Vector2.Zero);
			_group = false;
		}

		#endregion

		#region Methods for/from SuperClass/Interface

		#endregion

		#region Static Methods

		#endregion

		#region Public Methods

		/// <summary>
		/// Called by <code>FlxGroup</code>, commonly when game states are changed.
		/// </summary>
		virtual public void Destroy()
		{
			//Nothing to destroy yet
		}

		/// <summary>
		/// Called by <code>FlxObject.updateMotion()</code> and some constructors to
		/// rebuild the basic collision data for this object.
		/// </summary>
		virtual public void RefreshHulls()
		{
			colHullX.x = X;
			colHullX.y = Y;
			colHullX.Width = Width;
			colHullX.Height = Height;
			colHullY.x = X;
			colHullY.y = Y;
			colHullY.Width = Width;
			colHullY.Height = Height;
		}

		/// <summary>
		/// Just updates the retro-style flickering.
		/// Considered update logic rather than rendering because it toggles visibility.
		/// </summary>
		public virtual void UpdateFlickering()
		{
			if (Flickering())
			{
				if (_flickerTimer > 0)
				{
					_flickerTimer -= FlxG.elapsed;
					if (_flickerTimer == 0)
					{
						_flickerTimer = -1;
					}
				}
				if (_flickerTimer < 0) Flicker(-1);
				else
				{
					_flicker = !_flicker;
					Visible = !_flicker;
				}
			}
		}

		/// <summary>
		/// Called by the main game loop, handles motion/physics and game logic
		/// </summary>
		virtual public void Update()
		{
			UpdateMotion();
			UpdateFlickering();
		}

		/// <summary>
		/// Override this function to draw graphics (see <code>FlxSprite</code>).
		/// </summary>
		virtual public void Render(SpriteBatch spriteBatch)
		{
			//Objects don't have any visual logic/display of their own.
		}

		/// <summary>
		/// Checks to see if some <code>FlxObject</code> object overlaps this <code>FlxObject</code> object.
		/// 
		/// @param	Object	The object being tested.
		/// 
		/// @return	Whether or not the two objects overlap.
		/// </summary>
		virtual public bool Overlaps(FlxObject Object)
		{
			_point = GetScreenXy();
			float tx = _point.X;
			float ty = _point.Y;
			_point = Object.GetScreenXy();
			if((_point.X <= tx-Object.Width) || (_point.X >= tx+Width) || (_point.Y <= ty-Object.Height) || (_point.Y >= ty+Height))
				return false;
			return true;
		}

		/// <summary>
		/// Checks to see if a point in 2D space overlaps this <code>FlxObject</code> object.
		/// 
		/// @param	X			The X coordinate of the point.
		/// @param	Y			The Y coordinate of the point.
		/// @param	PerPixel	Whether or not to use per pixel collision checking (only available in <code>FlxSprite</code> subclass).
		/// 
		/// @return	Whether or not the point overlaps this object.
		/// </summary>
		virtual public bool OverlapsPoint(float X, float Y)
		{
			return OverlapsPoint(X, Y, false);
		}
		virtual public bool OverlapsPoint(float X, float Y, bool PerPixel)
		{
			X = X + FlxU.floor(FlxG.scroll.X);
			Y = Y + FlxU.floor(FlxG.scroll.Y);
			_point = GetScreenXy();
			if((X <= _point.X) || (X >= _point.X+Width) || (Y <= _point.Y) || (Y >= _point.Y+Height))
				return false;
			return true;
		}

		/// <summary>
		/// If you don't want to call <code>FlxU.collide()</code> you can use this instead.
		/// Just calls <code>FlxU.collide(this,Object);</code>.  Will collide against itself
		/// if Object==null.
		/// 
		/// @param	Object		The <FlxObject> you want to collide with.
		/// </summary>
		virtual public bool Collide(FlxObject Object)
		{
			return FlxU.collide(this,((Object==null)?this:Object));
		}

		/// <summary>
		/// <code>FlxU.collide()</code> (and thus <code>FlxObject.collide()</code>) call
		/// this function each time two objects are compared to see if they collide.
		/// It doesn't necessarily mean these objects WILL collide, however.
		/// 
		/// @param	Object	The <code>FlxObject</code> you're about to run into.
		/// </summary>
		virtual public void PreCollide(FlxObject Object)
		{
			//Most objects don't have to do anything here.
		}

		/// <summary>
		/// Called when this object's left side collides with another <code>FlxObject</code>'s right.
		/// NOTE: by default this function just calls <code>hitSide()</code>.
		/// 
		/// @param	Contact		The <code>FlxObject</code> you just ran into.
		/// @param	Velocity	The suggested new velocity for this object.
		/// </summary>
		virtual public void HitLeft(FlxObject Contact, float Velocity)
		{
			HitSide(Contact,Velocity);
		}
		
		/// <summary>
		/// Called when this object's right side collides with another <code>FlxObject</code>'s left.
		/// NOTE: by default this function just calls <code>hitSide()</code>.
		/// 
		/// @param	Contact		The <code>FlxObject</code> you just ran into.
		/// @param	Velocity	The suggested new velocity for this object.
		/// </summary>
		virtual public void HitRight(FlxObject Contact, float Velocity)
		{
			HitSide(Contact,Velocity);
		}

		/// <summary>
		/// Since most games have identical behavior for running into walls,
		/// you can just override this function instead of overriding both hitLeft and hitRight. 
		/// 
		/// @param	Contact		The <code>FlxObject</code> you just ran into.
		/// @param	Velocity	The suggested new velocity for this object.
		/// </summary>
		virtual public void HitSide(FlxObject Contact, float Velocity)
		{
			if(!Fixed || (Contact.Fixed && ((velocity.Y != 0) || (velocity.X != 0))))
				velocity.X = Velocity;
		}

		/// <summary>
		/// Called when this object's top collides with the bottom of another <code>FlxObject</code>.
		/// 
		/// @param	Contact		The <code>FlxObject</code> you just ran into.
		/// @param	Velocity	The suggested new velocity for this object.
		/// </summary>
		virtual public void HitTop(FlxObject Contact, float Velocity)
		{
			if(!Fixed || (Contact.Fixed && ((velocity.Y != 0) || (velocity.X != 0))))
				velocity.Y = Velocity;
		}

		/// <summary>
		/// Called when this object's bottom edge collides with the top of another <code>FlxObject</code>.
		/// 
		/// @param	Contact		The <code>FlxObject</code> you just ran into.
		/// @param	Velocity	The suggested new velocity for this object.
		/// </summary>
		virtual public void HitBottom(FlxObject Contact, float Velocity)
		{
			OnFloor = true;
			if(!Fixed || (Contact.Fixed && ((velocity.Y != 0) || (velocity.X != 0))))
				velocity.Y = Velocity;
		}

		/// <summary>
		/// Call this function to "damage" (or give health bonus) to this sprite.
		/// 
		/// @param	Damage		How much health to take away (use a negative number to give a health bonus).
		/// </summary>
		virtual public void Hurt(float Damage)
		{
			Health = Health - Damage;
			if(Health <= 0)
				Kill();
		}
		
		/// <summary>
		/// Call this function to "kill" a sprite so that it no longer 'exists'.
		/// </summary>
		virtual public void Kill()
		{
			Exists = false;
			Dead = true;
		}

		/// <summary>
		/// Tells this object to flicker, retro-style.
		/// 
		/// @param	Duration	How many seconds to flicker for.
		/// </summary>
		public void Flicker(float Duration) { _flickerTimer = Duration; if(_flickerTimer < 0) { _flicker = false; Visible = true; } }
		
		/// <summary>
		/// Check to see if the object is still flickering.
		/// 
		/// @return	Whether the object is flickering or not.
		/// </summary>
		public bool Flickering() { return _flickerTimer >= 0; }

		/// <summary>
		/// Call this function to figure out the on-screen position of the object.
		/// 
		/// @param	P	Takes a <code>Point</code> object and assigns the post-scrolled X and Y values of this object to it.
		/// 
		/// @return	The <code>Point</code> you passed in, or a new <code>Point</code> if you didn't pass one, containing the screen X and Y position of this object.
		/// </summary>
		virtual public Vector2 GetScreenXy()
		{
			Vector2 Point = Vector2.Zero;
			Point.X = FlxU.floor(X + FlxU.roundingError)+FlxU.floor(FlxG.scroll.X*scrollFactor.X);
			Point.Y = FlxU.floor(Y + FlxU.roundingError)+FlxU.floor(FlxG.scroll.Y*scrollFactor.Y);
			return Point;
		}
		
		/// <summary>
		/// Check and see if this object is currently on screen.
		/// 
		/// @return	Whether the object is on screen or not.
		/// </summary>
		virtual public bool OnScreen()
		{
			_point = GetScreenXy();
			if((_point.X + Width < 0) || (_point.X > FlxG.width) || (_point.Y + Height < 0) || (_point.Y > FlxG.height))
				return false;
			return true;
		}

		/// <summary>
		/// Handy function for reviving game objects.
		/// Resets their existence flags and position, including LAST position.
		/// 
		/// @param	X	The new X position of this object.
		/// @param	Y	The new Y position of this object.
		/// </summary>
		virtual public void Reset(float X, float Y)
		{
			this.X = X;
			this.Y = Y;
			Exists = true;
			Dead = false;
		}

		/// <summary>
		/// Returns the appropriate color for the bounding box depending on object state.
		/// </summary>
		public Color GetBoundingColor()
		{
			if(Solid)
			{
				if(Fixed)
					return new Color(0x00, 0xf2, 0x25, 0x7f);
				else
					return new Color(0xff, 0x00, 0x12, 0x7f);
			}
			else
				return new Color(0x00, 0x90, 0xe9, 0x7f);
		}

		#endregion

		#region Private Methods

		#endregion

		/// <summary>
		/// Internal function for updating the position and speed of this object.
		/// Useful for cases when you need to update this but are buried down in too many supers.
		/// </summary>
        protected void UpdateMotion()
        {
            if (!Moves)
                return;

            if (Solid)
                RefreshHulls();
            OnFloor = false;

            // Motion/physics
            AngularVelocity = FlxU.computeVelocity(AngularVelocity, AngularAcceleration, AngularDrag, MaxAngular);
            Angle += AngularVelocity * FlxG.elapsed;
            Vector2 thrustComponents;
            if (Thrust != 0)
            {
                thrustComponents = FlxU.rotatePoint(-Thrust, 0, 0, 0, Angle);
                Vector2 maxComponents = FlxU.rotatePoint(-MaxThrust, 0, 0, 0, Angle);
                float max = Math.Abs(maxComponents.X);
                if (max > Math.Abs(maxComponents.Y))
                    maxComponents.Y = max;
                else
                    max = Math.Abs(maxComponents.Y);
                maxVelocity.X = Math.Abs(max);
                maxVelocity.Y = Math.Abs(max);
            }
            else
            {
                thrustComponents = Vector2.Zero;
            }
            velocity.X = FlxU.computeVelocity(velocity.X, acceleration.X + thrustComponents.X, drag.X, maxVelocity.X);
            velocity.Y = FlxU.computeVelocity(velocity.Y, acceleration.Y + thrustComponents.Y, drag.Y, maxVelocity.Y);
            X += velocity.X * FlxG.elapsed;
            Y += velocity.Y * FlxG.elapsed;

            //Update collision data with new movement results
            if (!Solid)
                return;
            colVector.X = velocity.X * FlxG.elapsed;
            colVector.Y = velocity.Y * FlxG.elapsed;
            colHullX.Width += ((colVector.X > 0) ? colVector.X : -colVector.X);
            if (colVector.X < 0)
                colHullX.x += colVector.X;
            colHullY.x = X;
            colHullY.Height += ((colVector.Y > 0) ? colVector.Y : -colVector.Y);
            if (colVector.Y < 0)
                colHullY.y += colVector.Y;
        }
    }
}
