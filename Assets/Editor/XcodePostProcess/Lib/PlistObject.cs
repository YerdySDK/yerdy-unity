using System;
using System.Collections.Generic;

namespace XcodePostProcessLib
{
	using PlistArray = System.Collections.Generic.List<PlistObject>;
	using PlistDict = System.Collections.Generic.Dictionary<string, PlistObject>;
	
	/// <summary>
	/// Greatly reduces the amount of casting that needs to be done elsewhere
	/// </summary>
	public class PlistObject
	{
		private object value;
		
		public PlistObject(object val)
		{
			value = val;
		}
		
		#region Implicit conversions
		// PlistArray
		public static implicit operator PlistArray(PlistObject obj)
	    {
	        return obj.Array;
	    }
		public static implicit operator PlistObject(PlistArray array)
	    {
	        return new PlistObject(array);
	    }
		
		// PlistDict
		public static implicit operator PlistDict(PlistObject obj)
	    {
	        return obj.Dict;
	    }
		public static implicit operator PlistObject(PlistDict dict)
	    {
	        return new PlistObject(dict);
	    }
		
		// String
		public static implicit operator string(PlistObject obj)
	    {
	        return obj.String;
	    }
		public static implicit operator PlistObject(string str)
	    {
	        return new PlistObject(str);
	    }
		
		// Real
		public static implicit operator float(PlistObject obj)
	    {
	        return obj.Real;
	    }
		public static implicit operator PlistObject(float flt)
	    {
	        return new PlistObject(flt);
	    }
		
		// Integer
		public static implicit operator int(PlistObject obj)
	    {
	        return obj.Integer;
	    }
		public static implicit operator PlistObject(int integer)
	    {
	        return new PlistObject(integer);
	    }
		
		// Date
		public static implicit operator DateTime(PlistObject obj)
	    {
	        return obj.Date;
	    }
		public static implicit operator PlistObject(DateTime date)
	    {
	        return new PlistObject(date);
	    }
		
		// Bool
		public static implicit operator bool(PlistObject obj)
	    {
	        return obj.Bool;
	    }
		public static implicit operator PlistObject(bool boolean)
	    {
	        return new PlistObject(boolean);
	    }
		
		// Data
		public static implicit operator byte[](PlistObject obj)
	    {
	        return obj.Data;
	    }
		public static implicit operator PlistObject(byte[] bytes)
	    {
	        return new PlistObject(bytes);
	    }
		#endregion
		
		#region Property accessors
		public PlistArray Array { get { return GetValue<PlistArray>(); } }
		public PlistDict Dict { get { return GetValue<PlistDict>(); } }
		public string String { get { return GetValue<string>(); } }
		public float Real { get { return GetReal(); } }
		public int Integer { get { return GetInteger(); } }
		public DateTime Date { get { return GetValue<DateTime>(); } }
		public bool Bool { get { return GetValue<bool>(); } }
		public byte[] Data { get { return GetValue<byte[]>(); } }
		
		public object Raw { get { return value; } }
		#endregion
		
		#region Subscript accessors
		public PlistObject this[int index]
		{
			get
			{
				return Array[index];
			}
			set
			{
				Array[index] = value;
			}
		}
		
		public PlistObject this[string key]
		{
			get
			{
				return Dict[key];
			}
			set
			{
				Dict[key] = value;
			}
		}
		#endregion
		
		#region Get Values
		private T GetValue<T>()
		{
			if (value is T)
				return (T) value;
			else
				throw new Exception("Trying to access " + value.GetType().ToString() + " as " + typeof(T).ToString());
		}
		
		private float GetReal()
		{
			if (value is float || value is int)
				return (float)value;
			else
				throw new Exception("Trying to access " + value.GetType().ToString() + " as " + typeof(float).ToString());
		}
		
		private int GetInteger()
		{
			if (value is int)
				return (int)value;
			else if (value is float)
				return (int)Math.Round((float)value);
			else
				throw new Exception("Trying to access " + value.GetType().ToString() + " as " + typeof(int).ToString());
		}
		#endregion
	}
	
}