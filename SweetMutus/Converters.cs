using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;

namespace Aldentea.SweetMutus
{

	// (0.0.4)
	#region DurationConverterクラス
	public class DurationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// 00:03:05.6234 -> "3:05.62"

			if (value is TimeSpan)
			{
				var total_seconds = ((TimeSpan)value).TotalSeconds;
				var seconds = total_seconds % 60;

				int? digits = ConvertParameter(parameter);
				if (digits.HasValue)
				{
					var format = "00.00000000000000000000";
					return string.Format("{0:f0}:{1}", Math.Truncate(total_seconds / 60), seconds.ToString(format.Substring(0, digits.Value + 3)));
				}
				else
				{
					return string.Format("{0:d}:{2}{1}", Math.Truncate(total_seconds / 60), seconds.ToString(), seconds < 10 ? "0" : string.Empty);
				}
			}
			else
			{
				throw new ArgumentException("value には TimeSpan型を与えて下さい。");
			}
		}

		#region *パラメータを変換(ConverterParameter)
		int? ConvertParameter(object parameter)
		{
			int digits;
			if (parameter is int)
			{
				return (int)parameter;
			}
			else if (parameter is string && int.TryParse((string)parameter, out digits))
			{
				return digits;
			}
			else
			{
				return null;
			}
		}
		#endregion

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
			{
				var parts = ((string)value).Split(':');
				switch (parts.Length)
				{
					case 2:
						var min = int.Parse(parts[0]);
						var second = double.Parse(parts[1]);
						return TimeSpan.FromSeconds(min * 60 + second);
					case 1:
						return TimeSpan.FromSeconds(double.Parse(parts[0]));
					default:
						throw new ArgumentException("valueには 'm:ss.ff' 形式の文字列を与えて下さい。");
				}
			}
			else
			{
				throw new ArgumentException("valueには文字列を与えて下さい。");
			}
		}
	}
	#endregion

}
