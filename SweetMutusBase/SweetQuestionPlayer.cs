using System;
using System.Windows.Threading;
using System.Windows.Media.Animation;	// for TimeSeekOrigin.
using System.ComponentModel;
using System.Windows.Media;
using System.Threading.Tasks;

namespace Aldentea.SweetMutus
{
	// 現在Baseと呼んでいる部分を、
	// Dataに依存しない部分(Core？)とDataに依存する部分(Base？)に分けた方がいいのかな？

	using Data;

	namespace Base
	{
		// (0.2.0)
		public class SweetQuestionPlayer : INotifyPropertyChanged
		{
			MediaClock _questionClock;
			MediaClock _followClock;
			MediaTimeline _questionTimeLine;
			MediaPlayer _questionMediaPlayer = new MediaPlayer();
			TimeSpan _currentSongDuration = TimeSpan.Zero;
			DispatcherTimer _timer;

			#region CurrentQuestionプロパティ
			/// <summary>
			/// 現在の問題を取得／設定します。
			/// </summary>
			public SweetQuestion CurrentQuestion
			{
				get
				{
					return _currentQuestion;
				}
				set
				{
					if (_currentQuestion != value)
					{
						_currentQuestion = value;
						NotifyPropertyChanged("CurrentQuestion");
					}
				}
			}
			SweetQuestion _currentQuestion = null;
			#endregion

			/// <summary>
			/// 現在の再生位置を取得／設定します。設定は、フォロー時のみ可能です。
			/// </summary>
			public TimeSpan CurrentPosition
			{
				get
				{
					return _questionMediaPlayer.Position;
				}
				set
				{
					if (CurrentPhase == Phase.Follow)
					{
						_followClock.Controller.Seek(value, TimeSeekOrigin.BeginTime);
						NotifyPropertyChanged("CurrentPosition");
						// ↑いらないかな？
						// ↑やっぱり必要。(これがないと)スライダ動かしたときの現在位置の表示の変化がもっさりしてしまう。
					}
				}
			}

			#region *Durationプロパティ
			/// <summary>
			/// 現在の曲の長さを取得します。
			/// </summary>
			public TimeSpan Duration
			{
				get
				{
					return _currentSongDuration;
				}
			}
			#endregion

			#region *CurrentPhaseプロパティ
			protected Phase CurrentPhase
			{
				get
				{
					if (_followClock != null && _questionMediaPlayer.Clock == _followClock)
					{
						return Phase.Follow;
					}
					else
					{
						return Phase.Question;
					}
				}
			}
			#endregion

			#region *Volumeプロパティ
			/// <summary>
			/// 音量を取得／設定します。
			/// </summary>
			public double Volume
			{
				get
				{
					return _questionMediaPlayer.Volume;
				}
				set
				{
					_questionMediaPlayer.Volume = value;
					NotifyPropertyChanged("Volume");
				}
			}
			#endregion

			#region *コンストラクタ(SwetQuestionPlayer)
			public SweetQuestionPlayer()
			{
				_questionMediaPlayer.MediaOpened += questionMediaPlayer_MediaOpened;
				_questionMediaPlayer.MediaEnded += questionMediaPlayer_MediaEnded;
			}
			#endregion

			#region メディアオープン時
			private void questionMediaPlayer_MediaOpened(object sender, EventArgs e)
			{
				// _followClockをsetした後にもこのイベントが呼び出される！
				if (CurrentPhase == Phase.Question)
				{
					_currentSongDuration = _questionMediaPlayer.NaturalDuration.TimeSpan;
					NotifyPropertyChanged("Duration");
					this.MediaOpened(this, EventArgs.Empty);
				}
			}
			/// <summary>
			/// 曲ファイルのオープンが完了したときに発生します。
			/// </summary>
			public event EventHandler MediaOpened = delegate { };
			#endregion

			// ※ClockのDurationに到達したときは発生しない！
			#region 再生停止位置到達時
			private void questionMediaPlayer_MediaEnded(object sender, EventArgs e)
			{
				if (CurrentPhase == Phase.Question)
				{
					Stop();
				}
				else
				{
					// とりあえずPauseしてみる。
					_followClock.Controller.Pause();
				}
			}
			#endregion

			#region *曲ファイルを開く(Open)
			/// <summary>
			/// questionで指定された曲ファイルをオープンします。
			/// オープンが完了すると、MediaOpenedイベントが発生します。
			/// </summary>
			/// <param name="question"></param>
			public void Open(SweetQuestion question)
			{
				Close();

				_questionTimeLine = new MediaTimeline(new Uri(question.FileName));
				//_questionTimeLine.Completed += question_Completed;
				CurrentQuestion = question;
			}
			#endregion

			#region *出題開始(Start)
			public void Start()
			{
				// 停止位置設定を行う．
				if (CurrentQuestion.StopPos > TimeSpan.Zero)
				{
					_questionTimeLine.Duration = CurrentQuestion.StopPos;
				}

				// CurrentPosition更新通知用のタイマーを動かす。
				_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
				_timer.Tick += (sender, e) => { NotifyPropertyChanged("CurrentPosition"); };


				// 再生を開始する。
				_questionClock = (MediaClock)_questionTimeLine.CreateClock(true);
				_questionClock.Controller.Seek(CurrentQuestion.PlayPos, TimeSeekOrigin.BeginTime);
				_questionClock.Completed += question_Completed;
				_questionMediaPlayer.Clock = _questionClock;
				_timer.Start();

			}
			#endregion

			private void question_Completed(object sender, EventArgs e)
			{
				Stop();
			}

			#region *出題停止(Stop)
			public void Stop()
			{
				_questionClock.Controller.Pause();

				_questionTimeLine.Duration = System.Windows.Duration.Automatic;
				_followClock = (MediaClock)_questionTimeLine.CreateClock(true);
				_followClock.Controller.Seek(_questionMediaPlayer.Position, TimeSeekOrigin.BeginTime);

				_timer.Stop();

				this.QuestionStopped(this, EventArgs.Empty);

				// Clockを切り替えます。
				_questionMediaPlayer.Clock = _followClock;
				_followClock.Controller.Pause();

			}
			#endregion

			/// <summary>
			/// 出題が停止したときに発生します。出題停止位置に達した場合にも発生します。
			/// </summary>
			public event EventHandler QuestionStopped = delegate { };

			public void Follow()
			{
				_followClock.Controller.Resume();
				_timer.Start();
			}

			/// <summary>
			/// フォローモードで再生と一時停止を切り替えます。
			/// </summary>
			public void SwitchPlayPause()
			{
				if (_followClock.IsPaused)
				{
					_followClock.Controller.Resume();
				}
				else
				{
					_followClock.Controller.Pause();
				}
			}


			// (0.1.3.2)_timer.IsEnabledのチェックを追加。
			public void Close()
			{
				// アンドゥの時にしか呼ばれない！

				_questionMediaPlayer.Clock = null;
				//_questionMediaPlayer.Stop();
				_questionClock = null;
				_followClock = null;
				if (_timer != null && _timer.IsEnabled)
				{
					_timer.Stop();
				}
				NotifyPropertyChanged("CurrentPosition");
				_currentSongDuration = TimeSpan.Zero;
				NotifyPropertyChanged("Duration");
			}


			#region INotifyPropertyChanged実装

			protected void NotifyPropertyChanged(string propertyName)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			public event PropertyChangedEventHandler PropertyChanged = delegate { };
			#endregion

			protected enum Phase
			{
				Question,
				Follow
			}
		}

	}
}