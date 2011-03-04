using System.Collections.Generic;

namespace XnaFlixel
{
    /// <summary>
    /// FlxMonitor is a simple class that aggregates and averages data.
    /// Flixel uses this to display the framerate and profiling data
    /// in the developer console.  It's nice for keeping track of
    /// things that might be changing too fast from frame to frame.
    /// </summary>
    public class FlxMonitor
    {
    	#region Constants

    	#endregion

    	#region Fields

    	/// <summary>
    	/// Stores the requested size of the monitor array.
    	/// </summary>
    	protected int _size;
    	/// <summary>
    	/// Keeps track of where we are in the array.
    	/// </summary>
    	protected int _itr;
    	/// <summary>
    	/// An array to hold all the data we are averaging.
    	/// </summary>
    	protected List<float> _data;

    	#endregion

    	#region Properties

    	#endregion

    	#region Constructors

    	/// <summary>
    	/// Creates the monitor array and sets the size.
    	/// 
    	/// @param	Size	The desired size - more entries means a longer window of averaging.
    	/// @param	Default	The default value of the entries in the array (0 by default).
    	/// </summary>
    	public FlxMonitor(int Size, float Default)
    	{
    		_size = Size;
    		if(_size <= 0)
    			_size = 1;
    		_itr = 0;
    		_data = new List<float>(_size);
    		int i = 0;
    		while(i < _size)
    			_data[i++] = Default;
    	}

    	#endregion

    	#region Methods for/from SuperClass/Interface

    	#endregion

    	#region Static Methods

    	#endregion

    	#region Public Methods

    	/// <summary>
    	/// Adds an entry to the array of data.
    	/// 
    	/// @param	Data	The value you want to track and average.
    	/// </summary>
    	public void Add(float Data)
    	{
    		if (_itr < _data.Count)
    		{
    			_data[_itr++] = Data;
    		}
    		if(_itr >= _size)
    			_itr = 0;
    	}
		
    	/// <summary>
    	/// Averages the value of all the numbers in the monitor window.
    	/// 
    	/// @return	The average value of all the numbers in the monitor window.
    	/// </summary>
    	public float Average()
    	{
    		float sum = 0;
    		int i = 0;
    		while(i < _size)
    			sum += _data[i++];
    		return sum/_size;
    	}

    	#endregion

    	#region Private Methods

    	#endregion
    }
}
