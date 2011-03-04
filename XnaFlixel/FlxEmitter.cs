using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaFlixel.data;

namespace XnaFlixel
{
    /// <summary>
    /// <code>FlxEmitter</code> is a lightweight particle emitter.
    /// It can be used for one-time explosions or for
    /// continuous fx like rain and fire.  <code>FlxEmitter</code>
    /// is not optimized or anything; all it does is launch
    /// <code>FlxSprite</code> objects out at set intervals
    /// by setting their positions and velocities accordingly.
    /// It is easy to use and relatively efficient, since it
    /// automatically redelays its sprites and/or kills
    /// them once they've been launched.
    /// </summary>
    public class FlxEmitter : FlxGroup
    {
    	#region Constants

    	#endregion

    	#region Fields

    	/// <summary>
    	/// The minimum possible velocity of a particle.
    	/// The default value is (-100,-100).
    	/// </summary>
    	public Vector2 minParticleSpeed;

    	/// <summary>
    	/// The maximum possible velocity of a particle.
    	/// The default value is (100,100).
    	/// </summary>
    	public Vector2 maxParticleSpeed;

    	/// <summary>
    	/// The X and Y drag component of particles launched from the emitter.
    	/// </summary>
    	public Vector2 particleDrag;

    	/// <summary>
    	/// This variable has different effects depending on what kind of emission it is.
    	/// During an explosion, delay controls the lifespan of the particles.
    	/// During normal emission, delay controls the time between particle launches.
    	/// NOTE: In older builds, polarity (negative numbers) was used to define emitter behavior.
    	/// THIS IS NO LONGER THE CASE!  FlxEmitter.start() controls that now!
    	/// </summary>
    	public float delay;

    	/// <summary>
    	/// Checks whether you already fired a particle this frame.
    	/// </summary>
    	public bool justEmitted;

    	/// <summary>
    	/// The style of particle emission (all at once, or one at a time).
    	/// </summary>
    	private bool _explode;

    	/// <summary>
    	/// Internal helper for deciding when to launch particles or kill them.
    	/// </summary>
		private float _timer;

    	/// <summary>
    	/// Internal marker for where we are in <code>_sprites</code>.
    	/// </summary>
		private int _particle;

    	/// <summary>
    	/// Internal counter for figuring out how many particles to launch.
    	/// </summary>
		private int _counter;

    	#endregion

    	#region Properties

    	/// <summary>
    	/// The minimum possible angular velocity of a particle.  The default value is -360.
    	/// NOTE: rotating particles are more expensive to draw than non-rotating ones!
    	/// </summary>
    	public float MinRotation { get; set; }

    	/// <summary>
    	/// The maximum possible angular velocity of a particle.  The default value is 360.
    	/// NOTE: rotating particles are more expensive to draw than non-rotating ones!
    	/// </summary>
    	public float MaxRotation { get; set; }

    	/// <summary>
    	/// Sets the <code>acceleration.y</code> member of each particle to this value on launch.
    	/// </summary>
    	public float Gravity { get; set; }

    	/// <summary>
    	/// Determines whether the emitter is currently emitting particles.
    	/// </summary>
    	public bool On { get; set; }

    	/// <summary>
    	/// The number of particles to launch at a time.
    	/// </summary>
    	public int Quantity { get; set; }

    	#endregion

    	#region Constructors

    	/// <summary>
    	/// Creates a new <code>FlxEmitter</code> object at a specific position.
    	/// Does not automatically generate or attach particles!
    	/// 
    	/// @param	X			The X position of the emitter.
    	/// @param	Y			The Y position of the emitter.
    	/// </summary>
    	public FlxEmitter()
    	{
    		Initialize(0, 0);
    	}

    	public FlxEmitter(int x, int y)
    	{
    		Initialize(x, y);
    	}

    	#endregion

    	#region Methods for/from SuperClass/Interface

    	/// <summary>
    	/// Internal function that actually goes through and updates all the group members.
    	/// Overridden here to remove the position update code normally used by a FlxGroup.
    	/// </summary>
    	override protected void UpdateMembers()
    	{
    		FlxObject o;
    		int i = 0;
    		int l = members.Count;
    		while(i < l)
    		{
    			o = members[i++];
    			if((o != null) && o.Exists && o.Active)
    				o.Update();
    		}
    	}

    	/// <summary>
    	/// Called automatically by the game loop, decides when to launch particles and when to "die".
    	/// </summary>
    	override public void Update()
    	{
    		justEmitted = false;
    		base.Update();
    		UpdateEmitter();
    	}

    	/// <summary>
    	/// Call this function to turn off all the particles and the emitter.
    	/// </summary>
    	override public void Kill()
    	{
    		base.Kill();
    		On = false;
    	}

    	#endregion

    	#region Static Methods

    	#endregion

    	#region Public Methods

		/// <summary>
		/// This function generates a new array of sprites to attach to the emitter.
		/// </summary>
		/// <param name="graphics">If you opted to not pre-configure an array of FlxSprite objects, you can simply pass in a particle image or sprite sheet.</param>
		/// <param name="quantity">The number of particles to generate when using the "create from image" option.</param>
		/// <returns>his FlxEmitter instance (nice for chaining stuff together, if you're into that).</returns>
    	public FlxEmitter CreateSprites(Texture2D graphics, int quantity)
    	{
    		return CreateSprites(graphics, quantity, true, 0, 0);
    	}

		/// <summary>
		/// This function generates a new array of sprites to attach to the emitter.
		/// </summary>
		/// <param name="graphics">If you opted to not pre-configure an array of FlxSprite objects, you can simply pass in a particle image or sprite sheet.</param>
		/// <param name="quantity">The number of particles to generate when using the "create from image" option.</param>
		/// <param name="Multiple">Whether the image in the graphics param is a single particle or a bunch of particles (if it's a bunch, they need to be square!).</param>
		/// <returns>his FlxEmitter instance (nice for chaining stuff together, if you're into that).</returns>
    	public FlxEmitter CreateSprites(Texture2D graphics, int quantity, bool Multiple)
    	{
    		return CreateSprites(graphics, quantity, Multiple, 0, 0);
    	}

		/// <summary>
		/// This function generates a new array of sprites to attach to the emitter.
		/// </summary>
		/// <param name="graphics">If you opted to not pre-configure an array of FlxSprite objects, you can simply pass in a particle image or sprite sheet.</param>
		/// <param name="quantity">The number of particles to generate when using the "create from image" option.</param>
		/// <param name="Multiple">Whether the image in the graphics param is a single particle or a bunch of particles (if it's a bunch, they need to be square!).</param>
		/// <param name="Collide">Whether the particles should be flagged as not 'dead' (non-colliding particles are higher performance).  0 means no collisions, 0-1 controls scale of particle's bounding box.</param>
		/// <param name="Bounce">Whether the particles should bounce after colliding with things.  0 means no bounce, 1 means full reflection.</param>
		/// <returns>his FlxEmitter instance (nice for chaining stuff together, if you're into that).</returns>
    	public FlxEmitter CreateSprites(Texture2D graphics, int quantity, bool Multiple, float Collide, float Bounce)
    	{
    		members = new List<FlxObject>();
    		int r;
    		FlxSprite s;
    		int tf = 1;
    		float sw;
    		float sh;
    		if(Multiple)
    		{
    			s = new FlxSprite();
    			s.LoadGraphic(graphics,true);
    			tf = s.frames;
    		}
    		int i = 0;
    		while(i < quantity)
    		{
    			if((Collide > 0) && (Bounce > 0))
    				s = new FlxParticle(Bounce) as FlxSprite;
    			else
    				s = new FlxSprite();
    			if(Multiple)
    			{
    				r = (int)(FlxU.random()*tf);
    				//if(BakedRotations > 0)
    				//    s.loadRotatedGraphic(graphics,BakedRotations,r);
    				//else
    				//{
    				s.LoadGraphic(graphics,true);
    				s.frame = r;
    				//}
    			}
    			else
    			{
    				//if(BakedRotations > 0)
    				//    s.loadRotatedGraphic(graphics,BakedRotations);
    				//else
    				s.LoadGraphic(graphics);
    			}
    			if(Collide > 0)
    			{
    				sw = s.Width;
    				sh = s.Height;
    				s.Width = (int)(s.Width * Collide);
    				s.Height = (int)(s.Height * Collide);
    				s.offset.X = (int)(sw-s.Width)/2;
    				s.offset.Y = (int)(sh-s.Height)/2;
    				s.Solid = true;
    			}
    			else
    				s.Solid = false;
    			s.Exists = false;
    			s.scrollFactor = scrollFactor;
    			Add(s);
    			i++;
    		}
    		return this;
    	}
		

    	/// <summary>
    	/// A more compact way of setting the width and height of the emitter.
    	/// 
    	/// @param	Width	The desired width of the emitter (particles are spawned randomly within these dimensions).
    	/// @param	Height	The desired height of the emitter.
    	/// </summary>
    	public void SetSize(int Width, int Height)
    	{
    		((FlxObject) this).Width = Width;
    		((FlxObject) this).Height = Height;
    	}

    	/// <summary>
    	/// A more compact way of setting the X velocity range of the emitter.
    	/// 
    	/// @param	Min		The minimum value for this range.
    	/// @param	Max		The maximum value for this range.
    	/// </summary>
    	public void SetXSpeed()
    	{
    		SetXSpeed(0, 0);
    	}

    	public void SetXSpeed(float Min, float Max)
    	{
    		minParticleSpeed.X = Min;
    		maxParticleSpeed.X = Max;
    	}

    	/// <summary>
    	/// A more compact way of setting the Y velocity range of the emitter.
    	/// 
    	/// @param	Min		The minimum value for this range.
    	/// @param	Max		The maximum value for this range.
    	/// </summary>
    	public void SetYSpeed()
    	{
    		SetYSpeed(0, 0);
    	}

    	public void SetYSpeed(float Min, float Max)
    	{
    		minParticleSpeed.Y = Min;
    		maxParticleSpeed.Y = Max;
    	}

    	/// <summary>
    	/// A more compact way of setting the angular velocity constraints of the emitter.
    	/// 
    	/// @param	Min		The minimum value for this range.
    	/// @param	Max		The maximum value for this range.
    	/// </summary>
    	public void SetRotation()
    	{
    		SetRotation(0, 0);
    	}

    	public void SetRotation(float Min, float Max)
    	{
    		MinRotation = Min;
    		MaxRotation = Max;
    	}

    	/// <summary>
    	/// Call this function to start emitting particles.
    	/// 
    	/// @param	Explode		Whether the particles should all burst out at once.
    	/// @param	Delay		You can set the delay (or lifespan) here if you want.
    	/// @param	quantity	How many particles to launch.  Default value is 0, or "all the particles".
    	/// </summary>
    	public void Start()
    	{
    		Start(true, 0, 0);
    	}

    	public void Start(bool Explode, float Delay)
    	{
    		Start(Explode, Delay, 0);
    	}

    	public void Start(bool Explode, float Delay, int Quantity)
    	{
    		if(members.Count <= 0)
    		{
    			FlxG.log("WARNING: there are no sprites loaded in your emitter.\nAdd some to FlxEmitter.members or use FlxEmitter.createSprites().");
    			return;
    		}
    		_explode = Explode;
    		if(!_explode)
    			_counter = 0;
    		if(!Exists)
    			_particle = 0;
    		Exists = true;
    		Visible = true;
    		Active = true;
    		Dead = false;
    		On = true;
    		_timer = 0;
    		if(this.Quantity == 0)
    			this.Quantity = Quantity;
    		else if(Quantity != 0)
    			this.Quantity = Quantity;
    		if(Delay != 0)
    			delay = Delay;
    		if(delay < 0)
    			delay = -delay;
    		if(delay == 0)
    		{
    			if(Explode)
    				delay = 3;	//default value for particle explosions
    			else
    				delay = 0.1f;//default value for particle streams
    		}
    	}


    	/// <summary>
    	/// This function can be used both internally and externally to emit the next particle.
    	/// </summary>
    	public void EmitParticle()
    	{
    		_counter++;
    		FlxSprite s = members[_particle] as FlxSprite;
    		s.Visible = true;
    		s.Exists = true;
    		s.Active = true;
    		s.X = X - ((int)s.Width >> 1) + FlxU.random() * Width;
    		s.Y = Y - ((int)s.Height >> 1) + FlxU.random() * Height;
    		s.velocity.X = minParticleSpeed.X;
    		if(minParticleSpeed.X != maxParticleSpeed.X) s.velocity.X += FlxU.random()*(maxParticleSpeed.X-minParticleSpeed.X);
    		s.velocity.Y = minParticleSpeed.Y;
    		if(minParticleSpeed.Y != maxParticleSpeed.Y) s.velocity.Y += FlxU.random()*(maxParticleSpeed.Y-minParticleSpeed.Y);
    		s.acceleration.Y = Gravity;
    		s.AngularVelocity = MinRotation;
    		if(MinRotation != MaxRotation) s.AngularVelocity += FlxU.random()*(MaxRotation-MinRotation);
    		if(s.AngularVelocity != 0) s.Angle = FlxU.random()*360-180;
    		s.drag.X = particleDrag.X;
    		s.drag.Y = particleDrag.Y;
    		_particle++;
    		if(_particle >= members.Count)
    			_particle = 0;
    		s.OnEmit();
    		justEmitted = true;
    	}

    	/// <summary>
    	/// Call this function to stop the emitter without killing it.
    	/// 
    	/// @param	Delay	How long to wait before killing all the particles.  Set to 'zero' to never kill them.
    	/// </summary>
    	public void Stop()
    	{
    		Stop(3f);
    	}

    	public void Stop(float Delay)
    	{
    		_explode = true;
    		delay = Delay;
    		if(delay < 0)
    			delay = -Delay;
    		On = false;
    	}

    	/// <summary>
    	/// Change the emitter's position to the origin of a <code>FlxObject</code>.
    	/// 
    	/// @param	Object		The <code>FlxObject</code> that needs to spew particles.
    	/// </summary>
    	public void At(FlxObject Object)
    	{
    		X = Object.X + Object.origin.X;
    		Y = Object.Y + Object.origin.Y;
    	}

    	#endregion

    	#region Private Methods

    	private void Initialize(int x, int y)
    	{
    		X = x;
    		Y = y;
    		Width = 0;
    		Height = 0;

    		minParticleSpeed = new Vector2(-100, -100);
    		maxParticleSpeed = new Vector2(100, 100);
    		MinRotation = -360;
    		MaxRotation = 360;
    		Gravity = 400;
    		particleDrag = new Vector2();
    		delay = 0;
    		Quantity = 0;
    		_counter = 0;
    		_explode = true;
    		Exists = false;
    		On = false;
    		justEmitted = false;
    	}

    	/// <summary>
    	/// Internal function that actually performs the emitter update (called by update()).
    	/// </summary>
    	private void UpdateEmitter()
    	{
    		if(_explode)
    		{
    			_timer += FlxG.elapsed;
    			if((delay > 0) && (_timer > delay))
    			{
    				Kill();
    				return;
    			}
    			if(On)
    			{
    				On = false;
    				int i = _particle;
    				int l = members.Count;
    				if(Quantity > 0)
    					l = Quantity;
    				l += _particle;
    				while(i < l)
    				{
    					EmitParticle();
    					i++;
    				}
    			}
    			return;
    		}
    		if(!On)
    			return;
    		_timer += FlxG.elapsed;
    		while((_timer > delay) && ((Quantity <= 0) || (_counter < Quantity)))
    		{
    			_timer -= delay;
    			EmitParticle();
    		}
    	}

    	#endregion
    }
}
