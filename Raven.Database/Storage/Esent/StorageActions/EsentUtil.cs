﻿// -----------------------------------------------------------------------
//  <copyright file="EsentUtil.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using System.Linq;

namespace Raven.Storage.Esent.StorageActions
{
	public class EsentUtil
	{
		public static void DumpTable(Session session, Table table, Stream stream)
		{
			using (var writer = new StreamWriter(stream))
			{
				var cols = Api.GetTableColumns(session, table).ToArray();
				foreach (var col in cols)
				{
					writer.Write(col);
					writer.Write(",");
				}
				writer.WriteLine("#");
				Api.MoveBeforeFirst(session, table);
				int count = 0;
				while (Api.TryMoveNext(session, table))
				{
					foreach (var col in cols)
					{
						var val = GetvalueFromTable(session, table, col) ?? "NULL";
						writer.Write(val);
						writer.Write(",");
					}
					writer.WriteLine(++count);
				}
				writer.Flush();
			}
		}

		private static object GetvalueFromTable(Session session, Table table, ColumnInfo col)
		{
			switch (col.Coltyp)
			{
				case JET_coltyp.Long:
					return Api.RetrieveColumnAsInt32(session, table, col.Columnid);
				case JET_coltyp.DateTime:
					return Api.RetrieveColumnAsDateTime(session, table, col.Columnid);
				case JET_coltyp.Binary:
					var bytes = Api.RetrieveColumn(session, table, col.Columnid);
					if (bytes == null)
						return null;
					if (bytes.Length == 16)
						return new Guid(bytes);
					return Convert.ToBase64String(bytes);
				case JET_coltyp.LongText:
				case JET_coltyp.Text:
					var str = Api.RetrieveColumnAsString(session, table, col.Columnid);
					if (str == null)
						return null;
					if (str.Contains("\""))
						return "\"" + str.Replace("\"", "\"\"") + "\"";
					return str;
				case JET_coltyp.LongBinary:
					return "long binary val";
				default:
					throw new ArgumentOutOfRangeException("don't know how to handle coltype: " + col.Coltyp);
			}
		}
	}
}