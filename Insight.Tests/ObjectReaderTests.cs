﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insight.Database;
using NUnit.Framework;

#pragma warning disable 0649

namespace Insight.Tests
{
	[TestFixture]
	class ObjectReaderTests : BaseDbTest
	{
		#region Converting Types While Reading
		/// <summary>
		/// Tests the conversion of types when reading a value from an object (say, as a string)
		/// and sending it to a TVP column of a different type (say, a guid).
		/// </summary>
		[Test]
		public void TestConvertToTypeWhileReadingObject()
		{
			TestConvertToTypeWhileReading<Guid>("uniqueidentifier", Guid.NewGuid());
			TestConvertToTypeWhileReading<byte>("tinyint");
			TestConvertToTypeWhileReading<short>("smallint");
			TestConvertToTypeWhileReading<int>("int");
			TestConvertToTypeWhileReading<long>("bigint");
			TestConvertToTypeWhileReading<float>("real");
			TestConvertToTypeWhileReading<double>("float");
			TestConvertToTypeWhileReading<decimal>("decimal(18,5)");
			TestConvertToTypeWhileReading<bool>("bit");
			TestConvertToTypeWhileReading<char>("char(1)");
			TestConvertToTypeWhileReading<DateTime>("date");
			TestConvertToTypeWhileReading<DateTimeOffset>("datetimeoffset");
			TestConvertToTypeWhileReading<TimeSpan>("time");
		}

		private void TestConvertToTypeWhileReading<T>(string sqlType, T value = default(T)) where T : struct
		{
			List<string> list = new List<string>() { default(T).ToString() };

			string tableName = String.Format("ObjectReaderTable_{0}", typeof(T).Name);
			string procName = String.Format("ObjectReaderProc_{0}", typeof(T).Name);

			try
			{
				_connection.ExecuteSql(String.Format("CREATE TYPE {1} AS TABLE (value {0})", sqlType, tableName));

				using (var connection = _connectionStringBuilder.OpenWithTransaction())
				{
					connection.ExecuteSql(String.Format("CREATE PROC {0} @values {1} READONLY AS SELECT value FROM @values", procName, tableName));

					// convert a string value to the target type
					connection.Execute(procName, list.Select(item => new { Value = item.ToString() }));

					// convert a null nullable<T> to T
					connection.Execute(procName, list.Select(item => new { Value = (Nullable<T>)null }));

					// convert a non-null nullable<T> to T
					connection.Execute(procName, list.Select(item => new { Value = (Nullable<T>)value }));
				}
			}
			finally
			{
				_connection.ExecuteSql(String.Format("DROP TYPE {0}", tableName));
			}
		}
		#endregion

		#region Using Implicit/Explicit Operators For Conversions
		struct Money
		{
			private decimal _d;

			public static implicit operator decimal(Money m)
			{
				return m._d;
			}
		}

		[Test]
		public void ImplicitOperatorsShouldBeUsedIfAvailable()
		{
			try
			{
				_connection.ExecuteSql("CREATE TYPE ObjectReader_ImplicitTable AS TABLE (value decimal(18,5))");

				using (var connection = _connectionStringBuilder.OpenWithTransaction())
				{
					connection.ExecuteSql("CREATE PROC ObjectReader_Implicit @values ObjectReader_ImplicitTable READONLY AS SELECT value FROM @values");

					Money m = new Money();
					List<Money> list = new List<Money>() { m };
					connection.Execute("ObjectReader_Implicit", list.Select(item => new { Value = item }));
				}
			}
			finally
			{
				_connection.ExecuteSql("DROP TYPE ObjectReader_ImplicitTable");
			}
		}
		#endregion

		#region Using IConvertible For Conversions
		struct ConvertibleMoney : IConvertible
		{
			private decimal _d;

			public TypeCode GetTypeCode()
			{
				throw new NotImplementedException();
			}

			public bool ToBoolean(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public byte ToByte(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public char ToChar(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public DateTime ToDateTime(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public decimal ToDecimal(IFormatProvider provider)
			{
				return _d;
			}

			public double ToDouble(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public short ToInt16(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public int ToInt32(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public long ToInt64(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public sbyte ToSByte(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public float ToSingle(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public string ToString(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public object ToType(Type conversionType, IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public ushort ToUInt16(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public uint ToUInt32(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public ulong ToUInt64(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// For object reads, the system should be able to use IConvertible for types if available.
		/// </summary>
		[Test]
		public void IConvertibleShouldBeUsedIfAvailable()
		{
			try
			{
				_connection.ExecuteSql("CREATE TYPE ObjectReader_IConvertibleTable AS TABLE (value decimal(18,5))");

				using (var connection = _connectionStringBuilder.OpenWithTransaction())
				{
					connection.ExecuteSql("CREATE PROC ObjectReader_IConvertible @values ObjectReader_IConvertibleTable READONLY AS SELECT value FROM @values");

					ConvertibleMoney m = new ConvertibleMoney();
					List<ConvertibleMoney> list = new List<ConvertibleMoney>() { m };
					connection.Execute("ObjectReader_IConvertible", list.Select(item => new { Value = item }));
				}
			}
			finally
			{
				_connection.ExecuteSql("DROP TYPE ObjectReader_IConvertibleTable");
			}
		}
		#endregion
	}
}
