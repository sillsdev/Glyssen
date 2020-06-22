using GlyssenEngine.Bundle;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VesselTest
{
	public class TestUsingGlyssen
	{
		[Test]
		public void TestCreateGlyssenConnection()
		{
			var bundleFilePath =
				@"C:\Users\andyc\Documents\Faith Comes By Hearing\Projects\Vessel\IntegrationTests\Services\Test Files\TestBundleFile.zip";
			bundleFilePath = @"C:\Temp\TestBundleFile.zip";

			//Act
			var bundle = new GlyssenBundle(bundleFilePath);
			//var xxx = 1;
			Assert.AreEqual(1, 1);
		}
	}
}
