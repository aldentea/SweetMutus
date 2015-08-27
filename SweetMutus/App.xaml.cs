using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Aldentea.SweetMutus
{
	// 1. Appの継承元を変更する(XAMLも忘れずに！)．


	using Data;

	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Aldentea.Wpf.Application.Application
	{
		#region  2. お決まりの設定．(コピペでいいかも．)
		// 06/18/2014 by aldentea 
		protected App()
			: base()
		{
			this.Document = new SweetMutusDocument();
			this.Exit += new ExitEventHandler(App_Exit);
		}

		void App_Exit(object sender, ExitEventArgs e)
		{
			MySettings.Save();
		}

		// 07/09/2014 by aldentea : Settingsクラスに合わせてinternalに設定．
		// 06/13/2014 by aldentea
		#region *MySettingsプロパティ
		/// <summary>
		/// アプリケーションの設定を取得します．
		/// </summary>
		internal Properties.Settings MySettings
		{
			get
			{
				// 単に"Properties"では通らない．
				return Aldentea.SweetMutus.Properties.Settings.Default;
			}
		}
		#endregion

		// 06/13/2014 by aldentea : これはその都度実装する必要がありますかねぇ．
		public new static App Current
		{
			get
			{
				return System.Windows.Application.Current as App;
			}
		}

		#endregion

		// ここらへんまでお決まり．

		// 2.3.0からAppに実装することになった！
		#region ファイル履歴関連

		public override System.Collections.Specialized.StringCollection FileHistory
		{
			get
			{
				return MySettings.FileHistory;
			}
			set
			{
				MySettings.FileHistory = value;
			}
		}

		public override byte FileHistoryCount
		{
			get { return MySettings.FileHistoryCount; }
		}

		public override byte FileHistoryDisplayCount
		{
			get { return MySettings.FileHistoryDisplayCount; }
		}

		#endregion


	}
}
