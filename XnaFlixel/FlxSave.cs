using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;
#if !WINDOWS_PHONE

namespace XnaFlixel
{
    /// <summary>
    /// A class to help automate and simplify save game functionality.
    /// </summary>
    public class FlxSave : GameComponent
    {
    	#region Constants

    	private const string _savefile = "flixelsavedata.dat";

    	#endregion

    	#region Fields

    	private bool _wantsDevice = false;
    	private IAsyncResult _storageasync = null;

    	/// <summary>
    	/// Allows you to directly access the data container in the local shared object.
    	/// @default null
    	/// </summary>
    	private StorageDevice _device;

    	private FlxSaveData _savedata;
    	//public Dictionary<string, string> data

    	/// <summary>
    	/// The name of the local shared object.
    	/// @default null
    	/// </summary>
    	public string name;
    	/// <summary>
    	/// The local shared object itself.
    	/// @default null
    	/// </summary>
    	protected StorageContainer _so;

    	#endregion

    	#region Properties

    	public FlxSaveData data
    	{
    		get { return _savedata; }
    	}

    	public bool waitingOnDeviceSelector
    	{
    		get { return _wantsDevice; }
    	}
    	public bool canSave
    	{
    		get { return (_device != null && _device.IsConnected); }
    	}

    	#endregion

    	#region Constructors

    	/// <summary>
    	/// Blanks out the containers.
    	/// </summary>
    	public FlxSave()
    		: base(FlxG.Game)
    	{
    		// Storage device was not supplied... get one now.
    		FlxG.Game.Components.Add(this);
    		GetStorageDevice();
    	}
    	public FlxSave(StorageDevice Device)
    		: base(FlxG.Game)
    	{
    		FlxG.Game.Components.Add(this);
    		_device = Device;
    		name = null;
    		_so = null;
    		_savedata = new FlxSaveData();
    	}

    	~FlxSave()
    	{
    		if (_so != null)
    		{
    			ForceSave(0);
    		}
    	}

    	#endregion

    	#region Methods for/from SuperClass/Interface

    	public override void  Update(GameTime gameTime)
    	{
    		base.Update(gameTime);

    		if (_wantsDevice)
    		{
    			if (_storageasync == null)
    			{
    				if (!Guide.IsVisible)
    				{
    					try
    					{
    						_storageasync = StorageDevice.BeginShowSelector(null, null);
    					}
    					catch
    					{
    						// silent fail.
    						_storageasync = null;
    						_wantsDevice = false;
    					}
    				}
    			}
    			else if (_storageasync.IsCompleted)
    			{
    				try
    				{
    					_device = StorageDevice.EndShowSelector(_storageasync);
    				}
    				catch
    				{
    					// Fail silently.
    				}
    				finally
    				{
    					_storageasync = null;
    					_wantsDevice = false;
    				}
    			}
    		}
    	}

    	#endregion

    	#region Static Methods

    	#endregion

    	#region Public Methods

    	/// <summary>
    	/// Automatically creates or reconnects to locally saved data.
    	/// 
    	/// @param	Name	The name of the object (should be the same each time to access old data).
    	/// 
    	/// @return	Whether or not you successfully connected to the save data.
    	/// </summary>
    	public bool Bind(string Name)
    	{
    		_so = null;
    		name = Name;
    		try
    		{
    			// Open a storage container.
    			IAsyncResult result =
    				_device.BeginOpenContainer(Name, null, null);
    			// Wait for the WaitHandle to become signaled.
    			result.AsyncWaitHandle.WaitOne();

    			_so = _device.EndOpenContainer(result);

    			// Close the wait handle.
    			result.AsyncWaitHandle.Close();

    			// Check to see whether the save exists.
    			if (!_so.FileExists(_savefile))
    			{
    				// If not, dispose of the container and return new blank data.
    				_so.Dispose();
    				_savedata = new FlxSaveData();
    				return true;
    			}
    			// Open the file.
    			Stream stream = _so.OpenFile(_savefile, FileMode.Open);
    			_savedata.Deserialize(stream);
    			// Close the file.
    			stream.Close();
    			// Dispose the container.
    			_so.Dispose();
    		}
    		catch
    		{
    			FlxG.log("WARNING: There was a problem binding to\nthe shared object data from FlxSave.");
    			name = null;
    			_so = null;
    			_savedata = null;
    			return false;
    		}
    		return true;
    	}
		
    	/// <summary>
    	/// If you don't like to access the data object directly, you can use this to write to it.
    	/// 
    	/// @param	FieldName		The name of the data field you want to create or overwrite.
    	/// @param	FieldValue		The data you want to store.
    	/// @param	MinFileSize		If you need X amount of space for your save, specify it here.
    	/// 
    	/// @return	Whether or not the write and flush were successful.
    	/// </summary>
    	public bool Write(string FieldName, string FieldValue, uint MinFileSize)
    	{
    		if(_so == null)
    		{
    			FlxG.log("WARNING: You must call FlxSave.bind()\nbefore calling FlxSave.write().");
    			return false;
    		}
    		data[FieldName] = FieldValue;
    		return ForceSave(MinFileSize);
    	}
		
    	/// <summary>
    	/// If you don't like to access the data object directly, you can use this to read from it.
    	/// 
    	/// @param	FieldName		The name of the data field you want to read
    	/// 
    	/// @return	The value of the data field you are reading (null if it doesn't exist).
    	/// </summary>
    	public string Read(string FieldName)
    	{
    		if(_so == null)
    		{
    			FlxG.log("WARNING: You must call FlxSave.bind()\nbefore calling FlxSave.read().");
    			return null;
    		}
    		return data[FieldName];
    	}
		
    	/// <summary>
    	/// Writes the local shared object to disk immediately.
    	///
    	/// @param	MinFileSize		If you need X amount of space for your save, specify it here.
    	///
    	/// @return	Whether or not the flush was successful.
    	/// </summary>
    	public bool ForceSave(uint MinFileSize)
    	{
    		if(_so == null)
    		{
    			FlxG.log("WARNING: You must call FlxSave.bind()\nbefore calling FlxSave.forceSave().");
    			return false;
    		}
    		try
    		{
    			// Open a storage container.
    			IAsyncResult result =
    				_device.BeginOpenContainer(name, null, null);
    			// Wait for the WaitHandle to become signaled.
    			result.AsyncWaitHandle.WaitOne();
    			_so = _device.EndOpenContainer(result);
    			// Close the wait handle.
    			result.AsyncWaitHandle.Close();

    			// Check to see whether the save exists.
    			if (_so.FileExists(_savefile))
    				// Delete it so that we can create one fresh.
    				_so.DeleteFile(_savefile);

    			// Create the file.
    			Stream stream = _so.CreateFile(_savefile);
    			// Convert the object to XML data and put it in the stream.
    			_savedata.Serialize(stream);
    			// Close the file.
    			stream.Close();
    			// Dispose the container, to commit changes.
    			_so.Dispose();
    		}
    		catch
    		{
    			FlxG.log("WARNING: There was a problem flushing\nthe shared object data from FlxSave.");
    			return false;
    		}
    		return true;
    	}
		
    	/// <summary>
    	/// Erases everything stored in the local shared object.
    	/// 
    	/// @param	MinFileSize		If you need X amount of space for your save, specify it here.
    	/// 
    	/// @return	Whether or not the clear and flush was successful.
    	/// </summary>
    	public bool Erase(uint MinFileSize)
    	{
    		if(_so == null)
    		{
    			FlxG.log("WARNING: You must call FlxSave.bind()\nbefore calling FlxSave.erase().");
    			return false;
    		}
    		_savedata = null;
    		ForceSave(MinFileSize);
    		_savedata = new FlxSaveData();
    		return true;
    	}

    	#endregion

    	#region Private Methods

    	private void GetStorageDevice()
    	{
    		if (_wantsDevice == true || _storageasync != null)
    		{
    			return;
    		}
    		_device = null;
    		_wantsDevice = true;
    	}

    	#endregion

    	// X-flixel only.

    	// X-flixel only.
    }

    public class FlxSaveData
    {
        public FlxSaveData()
        {
            _data = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                if (_data.ContainsKey(key))
                    return _data[key];
                return null;
            }
            set
            {
                if (_data.ContainsKey(key))
                {
                    _data[key] = value;
                }
                else
                {
                    _data.Add(key, value);
                }
            }
        }
        private Dictionary<string, string> _data;

        public void Serialize(Stream writer)
        {
            List<FlxSaveDataEntry> entries = new List<FlxSaveDataEntry>(_data.Count);
            foreach (string key in _data.Keys)
            {
                entries.Add(new FlxSaveDataEntry(key, _data[key]));
            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<FlxSaveDataEntry>));
            serializer.Serialize(writer, entries);
        }

        public void Deserialize(Stream reader)
        {
            _data.Clear();
            XmlSerializer serializer = new XmlSerializer(typeof(List<FlxSaveDataEntry>));
            List<FlxSaveDataEntry> list = (List<FlxSaveDataEntry>)serializer.Deserialize(reader);

            foreach (FlxSaveDataEntry entry in list)
            {
                _data[entry.Key] = entry.Value;
            }
        }
    }

    public class FlxSaveDataEntry
    {
            public string Key;
            public string Value;

            public FlxSaveDataEntry()
            {
            }

            public FlxSaveDataEntry(string key, string value)
            {
                Key = key;
                Value = value;
            }
    }
}
#endif
