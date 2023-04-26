using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;
using System.Data;

namespace ObjectDumper
{
    public static class Dumper
    {
        /// <summary>
        /// Default is 10
        /// </summary>
        public static int MaxNestLevel { get; set; } = 10;
        private static readonly Func<object, string> _dumpMethod = o => o != null ? "\"" + o.ToString() + "\"" : "<null>";
        private static readonly ConcurrentDictionary<Type, string> _typeNames = new();
        private static object _fieldCacheLock = new object();
        private static readonly ConcurrentDictionary<Type, List<Tuple<FieldInfo, string>>> _fieldCache = new();
        private static readonly ConcurrentDictionary<Type, Func<object, string>> _primitiveTypeDumpers = new();

        static Dumper()
        {
            _primitiveTypeDumpers[typeof(Boolean)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Byte)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(SByte)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Int16)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Int32)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Int64)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(UInt16)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(UInt32)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(UInt64)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Single)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Double)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Decimal)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(DateTime)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(TimeSpan)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Guid)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Char)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Boolean?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Byte?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(SByte?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Int16?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Int32?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Int64?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(UInt16?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(UInt32?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(UInt64?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Single?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Double?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Decimal?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(DateTime?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(TimeSpan?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Guid?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(Char?)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(String)] = _dumpMethod;
            _primitiveTypeDumpers[typeof(byte[])] = o => o != null ? "\"" + Convert.ToBase64String((byte[])o) + "\"" : "<null>";
        }

        public static string Dump(object o)
        {
            try
            {
                List<object> visitedObjects = new List<object>();
                StringBuilder sb = new();
                DumpInternal(o, visitedObjects, 0, sb);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static void DumpInternal(object o, List<object> visitedObjects, int nestLevel, StringBuilder sb)
        {
            if (nestLevel > 0 && nestLevel > MaxNestLevel)
            {
                sb.Append("[NestLevel exceeded: " + nestLevel + "]");
                return;
            }
            string nestSpace = CalculateNestSpace(nestLevel);
            nestLevel++;
            if (o == null)
            {
                sb.Append("<null>");
                return;
            }
            Type type = o.GetType();
            if (DumpPrimitive(type, o, sb))
            {
                return;
            }
            int hashcode = o.GetHashCode();
            if (DumpVisited(o, visitedObjects, sb, hashcode))
            {
                return;
            }
            sb.Append('[');
            sb.Append(GetTypeName(type, hashcode));
            sb.AppendLine("]");
            visitedObjects.Add(o);
            if (typeof(System.Data.DataSet).IsAssignableFrom(type))
            {
                DumpDataSet((DataSet)o, visitedObjects, nestSpace, sb);
            }
            else if (typeof(System.Data.DataTable).IsAssignableFrom(type))
            {
                DumpDataTable((DataTable)o, visitedObjects, nestSpace, sb);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                DumpDictionary((IDictionary)o, visitedObjects, nestLevel, nestSpace, sb);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                DumpList((IEnumerable)o, visitedObjects, nestLevel, nestSpace, sb);
            }
            else
            {
                DumpFields(o, visitedObjects, nestLevel, nestSpace, sb, type);
            }
        }

        private static bool DumpVisited(object o, List<object> visitedObjects, StringBuilder sb, int hashcode)
        {
            if (visitedObjects.Contains(o))
            {
                sb.Append("[Dumped before: ");
                sb.Append(hashcode);
                sb.Append(']');
                return true;
            }
            return false;
        }

        private static void DumpFields(object o, List<object> visitedObjects, int nestLevel, string nestSpace, StringBuilder sb, Type type)
        {
            List<Tuple<FieldInfo, string>> fields = RetrieveFields(type);
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                object value = field.Item1.GetValue(o);
                sb.Append(nestSpace);
                sb.Append(field.Item2);
                sb.Append(" = ");
                if (!DumpPrimitive(field.Item1.FieldType, value, sb))
                {
                    DumpInternal(value, visitedObjects, nestLevel, sb);
                }
                if (i != fields.Count - 1)
                {
                    sb.AppendLine();
                }
            }
        }

        private static bool DumpVisited(object o, List<object> visitedObjects, string nestSpace, StringBuilder sb, int hashcode)
        {
            if (visitedObjects.Contains(o))
            {
                sb.Append(nestSpace);
                sb.Append("[Dumped before: ");
                sb.Append(hashcode);
                sb.AppendLine("]");
                return true;
            }
            return false;
        }

        private static bool DumpPrimitive(Type type, object value, StringBuilder sb)
        {
            if (_primitiveTypeDumpers.TryGetValue(type, out Func<object, string> primitiveDumper))
            {
                sb.Append(primitiveDumper(value));
                return true;
            }
            return false;
        }

        private static void DumpDictionary(IDictionary dict, List<object> visitedObjects, int nestLevel, string nestSpace, StringBuilder sb)
        {
            bool lineAppended = false;
            foreach (DictionaryEntry entry in dict)
            {
                sb.Append(nestSpace);
                DumpInternal(entry.Key, visitedObjects, nestLevel, sb);
                sb.Append(" = ");
                DumpInternal(entry.Value, visitedObjects, nestLevel, sb);
                sb.AppendLine();
                lineAppended = true;
            }
            if (lineAppended)
            {
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            }
        }

        private static void DumpList(IEnumerable list, List<object> visitedObjects, int nestLevel, string nestSpace, StringBuilder sb)
        {
            sb.Append(nestSpace);
            sb.AppendLine("{");
            foreach (var item in list)
            {
                sb.Append(nestSpace);
                DumpInternal(item, visitedObjects, nestLevel, sb);
                sb.AppendLine(",");
            }
            sb.Append(nestSpace);
            sb.Append("}");
        }

        private static void DumpDataSet(DataSet set, List<object> visitedObjects, string nestSpace, StringBuilder sb)
        {
            sb.Append(nestSpace);
            sb.AppendLine("{");
            sb.Append(nestSpace);
            sb.Append("[DataSetName: ");
            sb.Append(set.DataSetName);
            sb.AppendLine("]");
            for (int i = 0; i < set.Tables.Count; i++)
            {
                DataTable table = set.Tables[i];
                DumpDataTable(table, visitedObjects, nestSpace, sb);
                if (i != set.Tables.Count - 1)
                {
                    sb.AppendLine();
                }
            }
            sb.Append(nestSpace);
            sb.Append("}");
        }

        private static void DumpDataTable(DataTable table, List<object> visitedObjects, string nestSpace, StringBuilder sb)
        {
            sb.Append(nestSpace);
            sb.Append("[TableName: ");
            sb.Append(table.TableName);
            sb.AppendLine("]");
            sb.Append(nestSpace);
            int colCount = table.Columns.Count;
            if (colCount <= 0)
            {
                sb.Append("<empty table>");
                return;
            }
            for (int i = 0; i < colCount; i++)
            {
                sb.Append(table.Columns[i].ColumnName);
                sb.Append(';');
            }
            if (colCount > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.AppendLine();
            if (table.Rows.Count == 0)
            {
                return;
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                sb.Append(nestSpace);
                for (int c = 0; c < colCount; c++)
                {
                    sb.Append(row[c] != DBNull.Value && row[c] != null ? row[c].ToString() : "<null>");
                    sb.Append(';');
                }
                if (colCount > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                if (i != table.Rows.Count - 1)
                {
                    sb.AppendLine();
                }
            }
            sb.Append(nestSpace);
        }

        private static string CalculateNestSpace(int nestLevel)
        {
            string nestSpace = "";
            for (int i = 0; i < nestLevel; i++)
            {
                nestSpace += "\t";
            }

            return nestSpace;
        }

        private static List<Tuple<FieldInfo, string>> RetrieveFields(Type type)
        {
            List<Tuple<FieldInfo, string>> fields;
            if (!_fieldCache.TryGetValue(type, out fields))
            {
                // this type's fields has not been added yet
                // lock _fieldCacheLock and add the type
                lock (_fieldCacheLock)
                {
                    // some other thread might have added the type before we acquire lock
                    // recheck if the type exists
                    if (!_fieldCache.TryGetValue(type, out fields))
                    {
                        fields = new();
                        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
                        {
                            fields.Add(new(field, field.Name.Replace("k__BackingField", "")));
                        }
                        _fieldCache[type] = fields;
                    }
                }
            }
            return fields;
        }

        private static string GetTypeName(Type type, int hashcode)
        {
            if (!_typeNames.TryGetValue(type, out string typeName))
            {
                typeName = type.Namespace + "." + type.Name;
                if (type.GenericTypeArguments != null && type.GenericTypeArguments.Length > 0)
                {
                    typeName += "<";
                    typeName += string.Join(", ", type.GenericTypeArguments.Select(t => t.Namespace + "." + t.Name).ToArray());
                    typeName += ">";
                }
                _typeNames[type] = typeName;
            }
            return "Dumping: " + typeName + ", hashcode: " + hashcode;
        }
    }
}
