using System;
using System.Collections.Generic;
using System.Linq;
using GlyssenEngine.Utilities;
using NUnit.Framework;

namespace GlyssenEngineTests.Utilities
{
	[TestFixture]
	public class ListExtensionsTests
	{
		[Test]
		public void MergeOrderedList_NullOther_ThrowsArgumentNullException()
		{
			var me = new List<int>( new [] {1, 2, 3});
			List<char> other = null;
			Assert.Throws<ArgumentNullException>(() => me.MergeOrderedList(other, null, null));
		}

		[Test]
		public void MergeOrderedList_NullCompareMethodWithDifferentTypes_ThrowsArgumentNullException()
		{
			var me = new List<int>( new [] {1, 2, 3});
			var other = new List<char>( new [] {'a', 'b', 'c'});
			Assert.Throws<ArgumentNullException>(() => me.MergeOrderedList(other, null));
		}

		[Test]
		public void MergeOrderedList_NullSelectMethodWithDifferentTypes_ThrowsArgumentNullException()
		{
			var me = new List<int>( new [] {1, 2, 3});
			var other = new List<char>( new [] {'a', 'b', 'c'});
			Assert.Throws<ArgumentNullException>(() => me.MergeOrderedList(other, (a, b) => a.CompareTo(b), null));
		}

		[Test]
		public void MergeOrderedList_NullCompareAndSelectMethodWithSameTypes_ListsMerged()
		{
			var me = new List<int>( new [] {1, 2, 3});
			var other = new List<int>( new [] {1, 3, 4});
			me.MergeOrderedList(other, null, null);
			Assert.That(me.SequenceEqual(new [] {1, 2, 3, 4}));
		}

		[Test]
		public void MergeOrderedList_EmptyMe_MeFilledWithItemsFromOther()
		{
			var me = new List<char>();
			var other = new List<int>( new [] {1, 2, 4});
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'a', 'b', 'd'}));
		}

		[Test]
		public void MergeOrderedList_EmptyOther_MeUnchanged()
		{
			var me = new List<char>(new [] {'a', 'b', 'd'});
			me.MergeOrderedList(new List<int>(), Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'a', 'b', 'd'}));
		}

		[Test]
		public void MergeOrderedList_OtherContainsOnlyDuplicates_MeUnchanged()
		{
			var me = new List<char>(new [] {'a', 'b', 'd'});
			var other = new List<int>( new [] {1, 2, 4});
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'a', 'b', 'd'}));
			
			other.RemoveAt(2);
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'a', 'b', 'd'}));
			
			other.RemoveAt(0);
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'a', 'b', 'd'}));
		}

		[Test]
		public void MergeOrderedList_OtherContainsNewItemsBeforeAnyItemInMe_NewItemsPrependedToMe()
		{
			var me = new List<char>(new [] {'d', 'g', 'm'});
			var other = new List<int>( new [] {3});
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'c', 'd', 'g', 'm'}));
			
			other.Insert(0, 2);
			other.Insert(0, 1);
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'a', 'b', 'c', 'd', 'g', 'm'}));
		}

		[Test]
		public void MergeOrderedList_OtherContainsNewItemsAfterAnyItemInMe_NewItemsAppendedToMe()
		{
			var me = new List<char>(new [] {'d', 'e', 'f'});
			var other = new List<int>( new [] {7});
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'d', 'e', 'f', 'g'}));
			
			other.Add(9);
			other.Add(26);
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'d', 'e', 'f', 'g', 'i', 'z'}));
		}

		[Test]
		public void MergeOrderedList_OtherContainsNewItemsIntermingledWithMe_MergedInOrderWithNoDuplicates()
		{
			var me = new List<char>(new [] {'d', 'f', 'm'});
			var other = new List<int>( new [] {1, 2, 5, 8, 26});
			me.MergeOrderedList(other, Compare, IntToChar);
			Assert.That(me.SequenceEqual(new [] {'a', 'b', 'd', 'e', 'f', 'h', 'm', 'z'}));
		}

		private int Compare(char c, int i) => c.CompareTo((char)(i + 'a' - 1));

		private char IntToChar(int i) => (char)(i + 'a' - 1);
	}
}
