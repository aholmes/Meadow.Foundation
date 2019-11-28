using AutoFixture.Xunit2;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Meadow.UnitTests
{
	public class Led_Tests
	{
		[Theory]
		[AutoData]
		public void StartBlink_Sets_Pin_State(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.StartBlink();
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

			sut.StartBlink();
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.True(stateToggleTracking[0]); // if the first item is true then the port was toggled on
		}

		[Theory]
		[AutoData]
		public void Stop_Turns_Led_Off(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.StartBlink();
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.False(sut.IsOn);
		}


		[Theory]
		[AutoData]
		public void IsOn_Sets_Pin_State(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.IsOn = true;

			port.VerifySet(o => o.State = It.IsAny<bool>());
		}
	}
}
