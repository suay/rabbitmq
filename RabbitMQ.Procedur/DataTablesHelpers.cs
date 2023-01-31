using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace RabbitMQ.Procedur
{
    public static class DataTablesHelpers
    {
        public static List<T> ConvertDataTable<T>(DataRow[] dr)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dr)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        public static List<T> ConvertDataTable<T>(this DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            if (temp == typeof(string)) return JToken.FromObject(dr[0]).ToObject<T>();

            T obj = Activator.CreateInstance<T>();
            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        pro.SetValue(obj, Convert.ChangeType(dr[pro.Name], pro.PropertyType), null);
                        //pro.SetValue(obj, dr.IsNull(pro.Name) ? null : dr[pro.Name].ToString(), null);
                    }
                    else
                        continue;
                }
            }
            return obj;
        }

        public static List<Hashtable> DataTableToListHash(this DataTable dt)
        {
            return JArray.Parse(JsonConvert.SerializeObject(dt)).ToObject<List<Hashtable>>();
        }

        public static DataTable SearchDataTable(this DataTable dt, string search, string column = "")
        {
            if (dt.Rows.Count <= 0) return dt;
            if (string.IsNullOrEmpty(search)) return dt;

            search = search.Replace("[", "%");
            search = search.Replace("]", "%");
            var result = new DataTable();
            var ls_where = new List<string>();
            if (string.IsNullOrEmpty(column))
                ls_where = dt.Columns.Cast<DataColumn>().Where(x => x.DataType.Name == "String").Select(x => "[" + x.ColumnName + "] LIKE '%" + search + "%'").ToList();
            else
                ls_where.Add("[" + column + "] LIKE '*" + search + "*'");

            var rows = dt.Select(string.Join(" OR ", ls_where));
            if (rows.Any()) result = rows.CopyToDataTable();
            return result;
        }

        public static DataTable MultiSearchDataTable(this DataTable dt, string ls_search, string column = "")
        {
            if (string.IsNullOrEmpty(ls_search)) return dt;
            var result = dt;
            ls_search.Split(" ").ToList().ForEach(search => result = result.SearchDataTable(search));
            return result;
        }

        public static List<Hashtable> OrderListHash(this List<Hashtable> _list_hash, string orderBy, string orderType)
        {
            if (orderType?.ToLower() == "asc") _list_hash = _list_hash.OrderBy(o => o[orderBy]).ToList();
            else _list_hash = _list_hash.OrderByDescending(o => o[orderBy]).ToList();
            return _list_hash;
        }

        public static List<Hashtable> SearchAndConvertToListHash(this DataTable dt, string search, string orderBy, string orderType)
        {
            dt = dt.SearchDataTable(search);
            var data = dt.DataTableToListHash();
            data = data.OrderListHash(orderBy, orderType);
            return data;
        }
    }
}
