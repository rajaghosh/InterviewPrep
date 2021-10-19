using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Conceptual1
{
    class SerializeVsDeserialize
    {
        public SerializeVsDeserialize()
        {
            Person p1 = new Person() { FirstName = "A1", LastName = "B1" };

            string filePath = @"c:\TestFolder\data123.txt";
            DataSerializer _dataSerializer = new DataSerializer();

            Person p2;
            _dataSerializer.BinarySerialize(p1, filePath);

            p2 = _dataSerializer.BinaryDeserialize(filePath) as Person;

            Console.WriteLine($"{p2.FirstName} {p2.LastName}");


        }
    }

    [Serializable] //This will allow to serialize the class. If not included this might throw error
    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }

    
    class DataSerializer //This is a wrapper class which will wrap 2 methods of Serialization and Deserialization
    {
        public void BinarySerialize(object data, string filePath)
        {
            FileStream fileStream;
            BinaryFormatter bf = new BinaryFormatter();
            if (File.Exists(filePath))
                File.Delete(filePath);
            fileStream = File.Create(filePath);

            bf.Serialize(fileStream, data);
            fileStream.Close();
        }

        public object BinaryDeserialize(string filePath)
        {
            object obj = null;

            FileStream fileStream;
            BinaryFormatter bf = new BinaryFormatter();

            if (File.Exists(filePath))
            {
                fileStream = File.OpenRead(filePath);
                obj = bf.Deserialize(fileStream);
                fileStream.Close();
            }

            return obj;
        }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
