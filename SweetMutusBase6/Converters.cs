using System;
using System.Globalization;
using System.Windows.Data;

namespace Aldentea.SweetMutus.Net6.Base
{
	// (0.1.3.1)SweetMutusBaseに移動．

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


	// (0.0.9)
	#region DurationValidationRuleクラス
	public class DurationValidationRule : System.Windows.Controls.ValidationRule
	{
		public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			string message = "valueには 'm:ss.ff' 形式の文字列を与えて下さい。";
			if (value is string)
			{
				var parts = ((string)value).Split(':');
				int min;
				double second;

				switch (parts.Length)
				{
					case 2:
						return new System.Windows.Controls.ValidationResult(
							int.TryParse(parts[0], out min) && double.TryParse(parts[1], out second),
							message);
					case 1:
						return new System.Windows.Controls.ValidationResult(double.TryParse(parts[0], out second), message);
					default:
						return new System.Windows.Controls.ValidationResult(false, message);
				}
			}
			return new System.Windows.Controls.ValidationResult(false, "こんなん文字列に決まってるやろ！");
		}
	}
	#endregion


	// (0.1.3.1)
	#region EqualsConverterクラス
	public class EqualsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	#endregion

	// (0.2.0)うーん...
	public class FreePlayConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is PlayingPhase)
			{
				switch ((PlayingPhase)value)
				{
					case PlayingPhase.Judged:
					case PlayingPhase.Talking:
						return true;
					default:
						return false;
				}
			}
			else
			{
				throw new ArgumentException("FreePlayConverterは、PlayingPhase型に対してのみ使えます。");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}

}
