using System;
using Microsoft.Xna.Framework;

namespace XnaFlixel.data
{
    public class FlxFlash : FlxSprite
    {
		/// <summary>
		/// How long the effect should last.
		/// </summary>
		protected float _delay;
		/// <summary>
		/// Callback for when the effect is finished.
		/// </summary>
		protected EventHandler<FlxEffectCompletedEvent> _complete;
		
		/// <summary>
		/// Constructor initializes the fade object
		/// </summary>
        public FlxFlash()
            : base(0, 0)
		{
            createGraphic(FlxG.width, FlxG.height, Color.Black);
			scrollFactor.X = 0;
			scrollFactor.Y = 0;
			Exists = false;
			Solid = false;
			Fixed = true;
		}

		/// <summary>
		/// Reset and trigger this special effect
		/// 
		/// @param	Color			The color you want to use
		/// @param	Duration		How long it takes for the flash to fade
		/// @param	FlashComplete	A function you want to run when the flash finishes
		/// @param	Force			Force the effect to reset
		/// </summary>
        public void start(Color Color)
        {
            start(Color, 1f, null, false);
        }
        public void start(Color Color, float Duration)
        {
            start(Color, Duration, null, false);
        }
        public void start(Color Color, float Duration, EventHandler<FlxEffectCompletedEvent> FlashComplete, bool Force)
		{
			if(!Force && Exists) return;
            color = Color;
			_delay = Duration;
			_complete = FlashComplete;
			alpha = 1;
			Exists = true;
		}

		/// <summary>
		/// Stops and hides this screen effect.
		/// </summary>
        public void stop()
		{
			Exists = false;
		}

		/// <summary>
		/// Updates and/or animates this special effect
		/// </summary>
        override public void update()
		{
			alpha -= FlxG.elapsed/_delay;
			if(alpha <= 0)
			{
				Exists = false;
				if(_complete != null)
					_complete(this, new FlxEffectCompletedEvent(EffectType.Flash));
			}
		}

    }
}
