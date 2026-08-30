using System.Diagnostics;

namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal sealed class CommandInvocation : IDisposable
	{
		private bool _disposed;
		private readonly Lock _cancellationLock = new();

		private readonly Stopwatch _timer = new Stopwatch();
		public TimeSpan Elapsed => this._timer.Elapsed;
		public long ElapsedMilliseconds => this._timer.ElapsedMilliseconds;
		public long ElapsedTicks => this._timer.ElapsedTicks;
		public bool IsRunning => this._timer.IsRunning;

		private readonly CancellationTokenSource _cancellationTokenSource = new();
		public CancellationToken CancellationToken => this._cancellationTokenSource.Token;
		public bool IsCancellationRequested => this._cancellationTokenSource.IsCancellationRequested;

		public IWorldEditCommand Command { get; }
		public CommandContext Context { get; }

		public CommandInvocation(IWorldEditCommand command, CommandContext context)
		{
			ArgumentNullException.ThrowIfNull(command);
			ArgumentNullException.ThrowIfNull(context);

			this.Command = command;
			this.Context = context;
		}

		public void Invoke()
		{
			ObjectDisposedException.ThrowIf(this._disposed, this);
			this.CancellationToken.ThrowIfCancellationRequested();

			this._timer.Start();
			try
			{
				this.Command.Execute(this.Context, this.CancellationToken);
			}
			finally
			{
				this._timer.Stop();
			}
		}

		public bool RequestCancellation()
		{
			lock (this._cancellationLock)
			{
				if (this._disposed || this._cancellationTokenSource.IsCancellationRequested) return false;
				this._cancellationTokenSource.Cancel();
				return true;
			}
		}

		public void Dispose()
		{
			lock (this._cancellationLock)
			{
				if (this._disposed) return;

				this._disposed = true;
				this._cancellationTokenSource.Dispose();
			}
		}
	}
}
