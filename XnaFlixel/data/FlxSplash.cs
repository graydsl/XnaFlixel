using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace XnaFlixel.data
{
    //@benbaird X-flixel only. Moves all of the flixel logo screen stuff to a FlxState.
    public class FlxSplash : FlxState
    {
		//logo stuff
        private List<FlxLogoPixel> _f;
		private static Color _fc = Color.Gray;
		private float _logoTimer = 0;
		private Texture2D _poweredBy;
		private SoundEffect _fSound;
        private static FlxState _nextScreen;

        public FlxSplash()
            : base()
        {
        }

        public override void Create()
        {
            base.Create();
            _f = null;

			_poweredBy = FlxG.Game.Content.Load<Texture2D>("flixel/poweredby");
			_fSound = FlxG.Game.Content.Load<SoundEffect>("flixel/flixel");

            FlxG.flash.start(FlxG.backColor, 1f, null, false);
        }

        public static void setSplashInfo(Color flixelColor, FlxState nextScreen)
        {
            _fc = flixelColor;
            _nextScreen = nextScreen;
        }

        public override void Update()
        {
            if (_f == null)
            {
                _f = new List<FlxLogoPixel>();
                int scale = 10;
                float pwrscale;

                int pixelsize = (FlxG.height / scale);
                int top = (FlxG.height / 2) - (pixelsize * 2);
                int left = (FlxG.width / 2) - pixelsize;

                pwrscale = ((float)pixelsize / 24f);

                //Add logo pixels
                Add(new FlxLogoPixel(left + pixelsize, top, pixelsize, 0, _fc));
                Add(new FlxLogoPixel(left, top + pixelsize, pixelsize, 1, _fc));
                Add(new FlxLogoPixel(left, top + (pixelsize * 2), pixelsize, 2, _fc));
                Add(new FlxLogoPixel(left + pixelsize, top + (pixelsize * 2), pixelsize, 3, _fc));
                Add(new FlxLogoPixel(left, top + (pixelsize * 3), pixelsize, 4, _fc));

                FlxSprite pwr = new FlxSprite((FlxG.width - (int)((float)_poweredBy.Width * pwrscale)) / 2, top + (pixelsize * 4) + 16, _poweredBy);
                pwr.LoadGraphic(_poweredBy, false, false, (int)((float)_poweredBy.Width * pwrscale), (int)((float)_poweredBy.Height * pwrscale));

                pwr.color = _fc;
                pwr.scale = pwrscale;
                Add(pwr);

                _fSound.Play(FlxG.volume, 0f, 0f);
            }

            _logoTimer += FlxG.elapsed;

            base.Update();

            if (_logoTimer > 2.5f)
            {
                FlxG.state = _nextScreen;
            }
        }
    }
}
