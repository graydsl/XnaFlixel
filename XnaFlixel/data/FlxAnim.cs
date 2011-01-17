namespace XnaFlixel.data
{	
	/// <summary>
	/// Just a helper structure for the FlxSprite animation system
	/// </summary>
    public class FlxAnim
    {
		public string name;
		public float delay;
		public int[] frames;
		public bool looped;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Name">What this animation should be called (e.g. "run")</param>
		/// <param name="Frames">An array of numbers indicating what frames to play in what order (e.g. 1, 2, 3)</param>
		/// <param name="FrameRate">The speed in frames per second that the animation should play at (e.g. 40 fps)</param>
		/// <param name="Looped">Whether or not the animation is looped or just plays once</param>
		public FlxAnim(string Name, int[] Frames, int FrameRate, bool Looped)
		{
			name = Name;
			delay = 1.0f / (float)FrameRate;
			frames = Frames;
			looped = Looped;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Name">What this animation should be called (e.g. "run")</param>
		/// <param name="Frames">An array of numbers indicating what frames to play in what order (e.g. 1, 2, 3)</param>
		/// <param name="FrameRate">The speed in frames per second that the animation should play at (e.g. 40 fps)</param>
        public FlxAnim(string Name, int[] Frames, int FrameRate)
        {
            name = Name;
            delay = 1.0f / (float)FrameRate;
            frames = Frames;
            looped = true;
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Name">What this animation should be called (e.g. "run")</param>
		/// <param name="Frames">An array of numbers indicating what frames to play in what order (e.g. 1, 2, 3)</param>
        public FlxAnim(string Name, int[] Frames)
        {
            name = Name;
            delay = 0f;
            frames = Frames;
            looped = true;
        }
    }
}
