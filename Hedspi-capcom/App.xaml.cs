using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Livet;
using MetroRadiance;
using MetroTrilithon.Lifetime;
using Hedspi_capcom.ViewModels;
using Hedspi_capcom.Views;

namespace Hedspi_capcom
{
	/// <summary>
	/// アプリケーションの状態を示す識別子を定義します。
	/// </summary>
	public enum ApplicationState
	{
		/// <summary>
		/// アプリケーションは起動中です。
		/// </summary>
		Startup,

		/// <summary>
		/// アプリケーションは起動準備が完了し、実行中です。
		/// </summary>
		Running,

		/// <summary>
		/// アプリケーションは終了したか、または終了処理中です。
		/// </summary>
		Terminate,
	}

	sealed partial class Application : INotifyPropertyChanged, IDisposableHolder
	{
		public static MainWindowViewModel ViewModelRoot { get; private set; }

		static Application()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, args) => ReportException(sender, args.ExceptionObject as Exception);
		}

		private readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();
		private event PropertyChangedEventHandler PropertyChangedInternal;

		/// <summary>
		/// 現在の <see cref="AppDomain"/> の <see cref="Application"/> オブジェクトを取得します。
		/// </summary>
		public static Application Instance => Current as Application;

		/// <summary>
		/// アプリケーションの現在の状態を示す識別子を取得します。
		/// </summary>
		public ApplicationState State { get; private set; }


		protected override void OnStartup(StartupEventArgs e)
		{
			this.ChangeState(ApplicationState.Startup);

			// 開発中に多重起動検知ついてると起動できなくて鬱陶しいので
			// デバッグ時は外すんじゃもん
#if !DEBUG
			var appInstance = new MetroTrilithon.Desktop.ApplicationInstance().AddTo(this);
			if (appInstance.IsFirst)
#endif
			{
				this.DispatcherUnhandledException += (sender, args) =>
				{
					ReportException(sender, args.Exception);
					args.Handled = true;
				};

				DispatcherHelper.UIDispatcher = this.Dispatcher;


				ThemeService.Current.Initialize(this, Theme.Dark, Accent.Purple);

				ViewModelRoot = new MainWindowViewModel();
				this.MainWindow = new MainWindow { DataContext = ViewModelRoot };
				this.MainWindow.Show();
			}
#if !DEBUG
			else
			{
				this.ChangeState(ApplicationState.Terminate);
				this.Shutdown();
			}
#endif
		}


		protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
		{
			// TODO: Kill ffplay process

			base.OnSessionEnding(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.ChangeState(ApplicationState.Terminate);
			base.OnExit(e);

			this.compositeDisposable.Dispose();
		}

		/// <summary>
		/// <see cref="State"/> プロパティを更新し、<see cref="INotifyPropertyChanged.PropertyChanged"/> イベントを発生させます。
		/// </summary>
		/// <param name="value"></param>
		private void ChangeState(ApplicationState value)
		{
			if (this.State == value) return;

			this.State = value;
			this.RaisePropertyChanged(nameof(this.State));
		}

		private static void ReportException(object sender, Exception exception)
		{
			#region const

			const string messageFormat = @"
===========================================================
ERROR, date = {0}, sender = {1},
{2}
";
			const string path = "error.log";

			#endregion

			// ToDo: 例外ダイアログ

			try
			{
				var message = string.Format(messageFormat, DateTimeOffset.Now, sender, exception);

				Debug.WriteLine(message);
				File.AppendAllText(path, message);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}

			// とりあえずもう終了させるしかないもじゃ
			// 救えるパターンがあるなら救いたいけど方法わからんもじゃ
			Current.Shutdown();
		}

		#region INotifyPropertyChanged members

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { this.PropertyChangedInternal += value; }
			remove { this.PropertyChangedInternal -= value; }
		}

		private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChangedInternal?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region IDisposable members

		ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

		void IDisposable.Dispose()
		{
			this.compositeDisposable.Dispose();
		}

		#endregion
	}
}
