using System;
using System.Collections;
using MagicMonitor.Common.Models.Sensors;

namespace MagicMonitor.Common.Models
{
    public class Bme280DataCollection
    {
        
        public Bme280DataCollection()
        {
            
        }

        public void AddData(Bme280DataModel data)
        {

        }
    }


    public sealed class Bme280DataInstance
    {
        private Bme280DataInstance()
        {
        }
        private Hashtable dataTable = new Hashtable();

        public static Bme280DataInstance Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly Bme280DataInstance instance = new Bme280DataInstance();
        }

        public void AddData(Bme280DataModel data)
        {
            dataTable.Add(DateTime.UnixEpoch.Ticks, data);
        }

        public Bme280DataModel GetData(long key)
        {
            return (Bme280DataModel)dataTable[key];
        }

        public Hashtable GetAllData()
        {
            return dataTable;
        }

        public void RemoveData(long key)
        {
            dataTable.Remove(key);
        }
    }
}