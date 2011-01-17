using System;
using Microsoft.Xna.Framework;

namespace XnaFlixel.data
{
    /// <summary>
    /// This is a special effects utility class to help FlxGame do the 'fade' effect.
    /// </summary>
    public class FlxFade : FlxSprite
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
		public FlxFade()
            : base(0, 0)
		{
            createGraphic(FlxG.width, FlxG.height, Color.Black);
			scrollFactor.X = 0;
			scrollFactor.Y = 0;
			exists = false;
			solid = false;
			@fixed = true;
		}

		/// <summary>
		/// Reset and trigger this special effect
		/// 
		/// @param	Color			The color you want to use
		/// @param	Duration		How long it should take to fade the screen out
		/// @param	FadeComplete	A function you want to run when the fade finishes
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
        public void start(Color Color, float Duration, EventHandler<FlxEffectCompletedEvent> FadeComplete, bool Force)
		{
			if(!Force && exists) return;
            color = Color;
			_delay = Duration;
			_complete = FadeComplete;
			alpha = 0;
			exists = true;
		}

		/// <summary>
		/// Stops and hides this screen effect.
		/// </summary>
        public void stop()
		{
			exists = false;
		}

		/// <summary>
		/// Updates and/or animates this special effect
		/// </summary>
		override public void update()
		{
			alpha += FlxG.elapsed/_delay;
			if(alpha >= 1)
			{
				alpha = 1;
				if(_complete != null)
					_complete(this, new FlxEffectCompletedEvent(EffectType.FadeOut));
			}
		}

    }
}
