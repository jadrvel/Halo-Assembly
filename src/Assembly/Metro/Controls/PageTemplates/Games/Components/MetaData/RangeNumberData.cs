using System;
using System.Collections.Generic;

namespace Assembly.Metro.Controls.PageTemplates.Games.Components.MetaData
{
	/// <summary>
	///     Base class for number data.
	/// </summary>
	/// <typeparam name="T">The type of number to hold.</typeparam>
	public abstract class RangeNumberData<T> : ValueField
	{
		private T _min, _max;

		protected RangeNumberData(string name, uint offset, long address, T min, T max, uint pluginLine, string tooltip)
			: base(name, offset, address, pluginLine, tooltip)
		{
			_min = min;
			_max = max;
		}

		public abstract string Type { get; }

		public T Min
		{
			get { return _min; }
			set
			{
				_min = value;
				NotifyPropertyChanged("Min");
			}
		}

		public T Max
		{
			get { return _max; }
			set
			{
				_max = value;
				NotifyPropertyChanged("Max");
			}
		}

		public override string AsString()
		{
			return string.Format("{0} | {1} | {2} {3}", Type, Name, Min, Max);
		}

		public override object GetAsJson()
		{
			var dict = new Dictionary<string, object> 
			{
				["Min"] = Min,
				["Max"] = Max
			};

			return dict;
		}
	}

	/// <summary>
	///     Unsigned 16-bit integer.
	/// </summary>
	public class RangeInt16Data : RangeNumberData<short>
	{
		public RangeInt16Data(string name, uint offset, long address, short min, short max, uint pluginLine, string tooltip)
			: base(name, offset, address, min, max, pluginLine, tooltip)
		{
		}

		public override string Type => "range16";

        public override void Accept(IMetaFieldVisitor visitor)
		{
			visitor.VisitRangeInt16(this);
		}

		public override MetaField CloneValue()
		{
			return new RangeInt16Data(Name, Offset, FieldAddress, Min, Max, PluginLine, ToolTip);
		}
	}

	/// <summary>
	///     32-bit floating-point number.
	/// </summary>
	public class RangeFloat32Data : RangeNumberData<float>
	{
		public RangeFloat32Data(string name, uint offset, long address, float min, float max, uint pluginLine, string tooltip)
			: base(name, offset, address, min, max, pluginLine, tooltip)
		{
		}

		public override string Type => "rangeF";

        public override void Accept(IMetaFieldVisitor visitor)
		{
			visitor.VisitRangeFloat32(this);
		}

		public override MetaField CloneValue()
		{
			return new RangeFloat32Data(Name, Offset, FieldAddress, Min, Max, PluginLine, ToolTip);
		}
	}

	/// <summary>
	///     32-bit floating-point number, converted from radians to degrees
	/// </summary>
	public class RangeDegreeData : RangeNumberData<float>
	{
		private float _radianmin, _radianmax;

		public RangeDegreeData(string name, uint offset, long address, float min, float max, uint pluginLine, string tooltip)
			: base(name, offset, address, min, max, pluginLine, tooltip)
		{
			_radianmin = min;
			_radianmax = max;
		}

		public override string Type => "rangeD";

        public new float Min
		{
			get { return FromRadian(_radianmin); }
			set
			{
				_radianmin = ToRadian(value);
				NotifyPropertyChanged("Min");
			}
		}

		public float RadianMin
		{
			get { return _radianmin; }
			set
			{
				_radianmin = value;
				NotifyPropertyChanged("RadianMin");
				NotifyPropertyChanged("Min");
			}
		}

		public new float Max
		{
			get { return FromRadian(_radianmax); }
			set
			{
				_radianmax = ToRadian(value);
				NotifyPropertyChanged("Max");
			}
		}

		public float RadianMax
		{
			get { return _radianmax; }
			set
			{
				_radianmax = value;
				NotifyPropertyChanged("RadianMax");
				NotifyPropertyChanged("Max");
			}
		}

		public override void Accept(IMetaFieldVisitor visitor)
		{
			visitor.VisitRangeDegree(this);
		}

		public override MetaField CloneValue()
		{
			return new RangeDegreeData(Name, Offset, FieldAddress, RadianMin, RadianMax, PluginLine, ToolTip);
		}

		public override string AsString()
		{
			return string.Format("{0} | {1} | {2} {3}", Type, Name, Min, Max);
		}

		public override object GetAsJson()
		{
			var dict = new Dictionary<string, object> {
				["Min"] = Min,
				["Max"] = Max
			};

			return dict;
		}
	}
}