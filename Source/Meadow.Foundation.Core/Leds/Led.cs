using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
	/// <summary>
	/// Represents a simple LED
	/// </summary>
	public class Led : ILed
	{
		#region Properties
		/// <summary>
		/// Gets the port that is driving the LED
		/// </summary>
		/// <value>The port</value>
		public IDigitalOutputPort Port { get; protected set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Meadow.Foundation.Leds.Led"/> is on.
		/// </summary>
		/// <value><c>true</c> if is on; otherwise, <c>false</c>.</value>
		public bool IsOn
		{
			get => Port.State;
			set => Port.State = value;
		}
		#endregion

		#region Constructor(s)
		/// <summary>
		/// Creates a LED through a pin directly from the Digital IO of the board
		/// </summary>
		/// <param name="pin"></param>
		public Led(IIODevice device, IPin pin) :
			this(device.CreateDigitalOutputPort(pin, false))
		{ }

		/// <summary>
		/// Creates a LED through a DigitalOutPutPort from an IO Expander
		/// </summary>
		/// <param name="port"></param>
		public Led(IDigitalOutputPort port)
		{
			Port = port;
		}
		#endregion

		#region Private methods
		private volatile CancellationTokenSource _running = new CancellationTokenSource();
		private CancellationTokenSource Start(CancellationToken cancellationToken)
			=> Start(cancellationToken == default
					? new CancellationTokenSource()
					: CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
				);
		private CancellationTokenSource Start(CancellationTokenSource cancellationTokenSource)
		{
			do
			{
				var cts = _running;

				if (Interlocked.CompareExchange(ref _running, cancellationTokenSource, cts) == cts)
				{
					cts.Cancel();
					return cancellationTokenSource;
				}
			} while(true);
		}

		private async Task Blink(uint onDuration, uint offDuration, CancellationToken cancellationToken)
		{
			while(!cancellationToken.IsCancellationRequested)
			{
				IsOn = true;
				await Task.Delay((int)onDuration, cancellationToken).ConfigureAwait(false);

				if (cancellationToken.IsCancellationRequested) return;

				IsOn = false;
				await Task.Delay((int)offDuration, cancellationToken).ConfigureAwait(false);
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
		/// </summary>
		/// <param name="onDuration"></param>
		/// <param name="offDuration"></param>
		public void StartBlink(uint onDuration = 200, uint offDuration = 200)
		{
			var cts = Start(new CancellationTokenSource());
			Task.Run(() => Blink(onDuration, offDuration, cts.Token), cts.Token);
		}

		/// <summary>
		/// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
		/// </summary>
		/// <param name="onDuration"></param>
		/// <param name="offDuration"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task StartBlink(uint onDuration = 200, uint offDuration = 200, CancellationToken cancellationToken = default)
		{
			cancellationToken = Start(cancellationToken).Token;

			return Blink(onDuration, offDuration, cancellationToken);
		}

		/// <summary>
		/// Stop the blink animation
		/// </summary>
		public void Stop() => _running.Cancel();
		#endregion
	}
}