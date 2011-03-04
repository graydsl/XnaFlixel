namespace XnaFlixel
{
    public class FlxRect
    {
    	#region Constants

    	#endregion

    	#region Fields

    	public float x;
    	public float y;

    	#endregion

    	#region Properties

    	public float Width { get; set; }

    	public float Height { get; set; }

    	#endregion

    	#region Constructors

    	public FlxRect(float X, float Y, float Width, float Height)
    	{
    		x = X;
    		y = Y;
    		this.Width = Width;
    		this.Height = Height;
    	}

    	#endregion

    	#region Methods for/from SuperClass/Interface

    	#endregion

    	#region Static Methods

    	static public FlxRect Empty
    	{
    		get { return new FlxRect(0, 0, 0, 0); }
    	}

    	#endregion

    	#region Public Methods

    	#endregion

    	#region Private Methods

    	#endregion
    }
}
