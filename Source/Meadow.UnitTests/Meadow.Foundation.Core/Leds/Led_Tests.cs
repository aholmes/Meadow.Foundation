using AutoFixture.Xunit2;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Meadow.UnitTests.Meadow.Foundation.Core.Leds
{
	public class Led_Tests
	{
		[Theory]
		[AutoData]
		public void StartBlink_Sets_Pin_State(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.StartBlink(200, 200);
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
		}

		[Theory]
		[AutoData]
		public void StartBlink_Sets_Pin_State_And_Led_IsOn(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var stateToggleTracking = new List<bool>();
			port.SetupProperty(o => o.State);
			port.SetupSet(o => o.State = Capture.In(stateToggleTracking));

			sut.StartBlink(200, 200);
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.True(stateToggleTracking[0]); // if the first item is true then the port was toggled on
		}

		[Theory]
		[AutoData]
		public void Stop_Stops_Blinking(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var stateToggleTracking = new List<bool>();
			port.SetupProperty(o => o.State);
			port.SetupSet(o => o.State = Capture.In(stateToggleTracking));

			sut.StartBlink(200, 200);
			Thread.Sleep(100);
			sut.Stop();

			var toggleCount = stateToggleTracking.Count;

			port.VerifySet(o => o.State = It.IsAny<bool>());

			Thread.Sleep(500);
			Assert.Equal(toggleCount, stateToggleTracking.Count);
		}


		[Theory]
		[AutoData]
		public void IsOn_Sets_Pin_State(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.IsOn = true;

			port.VerifySet(o => o.State = It.IsAny<bool>());
		}

		[Theory]
		[AutoData]
		public void StartBlink_Async_Sets_Pin_State(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var task = sut.StartBlink(200, 200, default);
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.True(task.IsCanceled);
		}

		[Theory]
		[AutoData]
		public void StartBlink_Async_Sets_Pin_State_And_Led_IsOn(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var stateToggleTracking = new List<bool>();
			port.SetupProperty(o => o.State);
			port.SetupSet(o => o.State = Capture.In(stateToggleTracking));

			var task = sut.StartBlink(200, 200, default);
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.True(stateToggleTracking[0]); // if the first item is true then the port was toggled on
			Assert.True(task.IsCanceled);
		}

		[Theory]
		[AutoData]
		public void Stop_Async_Stops_Blinking(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var stateToggleTracking = new List<bool>();
			port.SetupProperty(o => o.State);
			port.SetupSet(o => o.State = Capture.In(stateToggleTracking));

			var task = sut.StartBlink(200, 200, default);
			Thread.Sleep(100);
			sut.Stop();

			var toggleCount = stateToggleTracking.Count;

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.True(task.IsCanceled);

			Thread.Sleep(500);
			Assert.Equal(toggleCount, stateToggleTracking.Count);
		}

		[Theory]
		[AutoData]
		public async Task StartBlink_Uses_CancellationToken(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var cts = new CancellationTokenSource(500);
			var task = sut.StartBlink(200, 200, cts.Token);

			await Task.Delay(600);

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.True(task.IsCanceled);
		}

		[Theory]
		[AutoData]
		public void Led_Start_Async_Cancels_Executions_On_Other_Threads(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var tasks = new BlockingCollection<Task>();

			for(var i = 0; i < 10; i++)
			{
				ThreadPool.QueueUserWorkItem((o) => tasks.Add(sut.StartBlink(200, 200, default)));
			}

			Thread.Sleep(1000);

			Assert.Single(tasks.Where(t => t.IsCompleted == false));

			sut.Stop();

			Thread.Sleep(500);

			Assert.DoesNotContain(tasks, t => t.IsCompleted == false);
		}

		[Theory]
		[AutoData]
		public void Led_Start_Async_Allows_Multiple_Calls_And_Stops_Prior_Executions(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var tasks = new Task[]
			{
				sut.StartBlink(200, 200, default),
				sut.StartBlink(200, 200, default),
				sut.StartBlink(200, 200, default),
				sut.StartBlink(200, 200, default),
				sut.StartBlink(200, 200, default)
			};

			Assert.Single(tasks.Where(t => t.IsCompleted == false));

			sut.Stop();

			Thread.Sleep(500);

			Assert.DoesNotContain(tasks, t => t.IsCompleted == false);

		}
	}
}
