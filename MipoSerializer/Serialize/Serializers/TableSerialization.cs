using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
		public void SerializeDataTable(DataTable table)
		{
			var columnCount = table.Columns.Count;
			Writer.Write(columnCount);

			var colTypes = new Type[columnCount];
			for (int col = 0; col < columnCount; col++)
			{
				var column = table.Columns[col];
				Writer.Write(column.ColumnName);
				SerializeType(column.DataType);
				Writer.Write(column.AllowDBNull);
				colTypes[col] = column.DataType.IsSealed && !column.AllowDBNull ? column.DataType : null;
			}

			Writer.Write(table.Rows.Count);
			foreach (DataRow row in table.Rows)
			{
				for (int col = 0; col < columnCount; col++)
					Serialize(row[col], colTypes[col]);
			}
		}

		public DataTable DeserializeDataTable()
		{
			DataColumn column;
			DataRow row;

			var table = new DataTable();
			var columnCount = Reader.ReadInt32();
			var colTypes = new Type[columnCount];
			for (int col = 0; col < columnCount; col++)
			{
				column = new DataColumn(Reader.ReadString(), DeserializeType());
				column.AllowDBNull = Reader.ReadBoolean();
				table.Columns.Add(column);
				colTypes[col] = column.DataType.IsSealed && !column.AllowDBNull ? column.DataType : null;
			}

			var rowCount = Reader.ReadInt32();
			for (int r = 0; r < rowCount; r++)
			{
				row = table.NewRow();
				for (int col = 0; col < columnCount; col++)
					row[col] = Deserialize(colTypes[col]);

				table.Rows.Add(row);
			}
			return table;
		}
	}
}
