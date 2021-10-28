using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
//using System.Json;

namespace Conceptual1
{
    class ArrayOfJson
    {
        public ArrayOfJson()
        { 
            string _jsonArray2 = @"[
                                      {
                                        'id': 'a1',
                                        'value': 'data1',
                                        'city' : 'Kolkata'
                                      },
                                      {
                                        'id': 'a2',
                                        'value': 'data2',
                                        'city' : 'Kolkata'
                                      },
                                      {
                                        'id': 'a3',
                                        'value': 'data3',
                                        'city' : 'Mumbai'
                                      },
                                      {
                                        'id': 'a4',
                                        'value': 'data4',
                                        'city' : 'Bangluru'
                                      }
                                    ]";

            var _updatedJson = JsonFilter(_jsonArray2,@"{'id': 'a3','value': 'data3','city' : 'Mumbai'}");
            Console.WriteLine(_updatedJson);
        }

        public string JsonFilter(string _jsonArr,string target)
        {
            List<Details> myDetailsList = JsonConvert.DeserializeObject<List<Details>>(_jsonArr);
            Details _target = JsonConvert.DeserializeObject<Details>(target);

            //Approach 1
            //myDetailsList.RemoveAll(x=> x.Id == _target.Id && x.Value == _target.Value && x.City == _target.City);

            //Approach 2
            var item = myDetailsList.Find(x => x.Id == _target.Id && x.Value == _target.Value && x.City == _target.City);
            myDetailsList.Remove(item);

            foreach (var row in myDetailsList)
            {
                Console.WriteLine($"{row.Id} -> {row.Value} -> {row.City}");
            }

            string convertedJson = JsonConvert.SerializeObject(myDetailsList);
            return convertedJson;
        }
    }

    public class Details
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string City { get; set; }
    }
}


/*
 * Program of taking an Array Of JSON then remove a particular object from it
 * 
 * 
 * 
 * 
 * 
 */