using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptual1
{
    class JsonListMain
    {
        List<Model> _modelList = new List<Model>();

        public List<Model> GetList()
        {
            for(int i = 0; i < 10; i++)
            {
                _modelList.Add(ModelMaker(i));
            }

            return (_modelList);
        }
        public Model ModelMaker(int i)
        {
            Model m1 = new Model();
            m1.id = ++i;
            m1.data = "Value" + i++.ToString();
            return m1;
        }

    }


    class Model
    {
        public int id { get; set; }
        public string data { get; set; }
    }

    class JsonListCreation
    {
        public JsonListCreation()
        {
            JsonListMain j1 = new JsonListMain();
            foreach(var data in j1.GetList())
            {
                Console.WriteLine("{0},{1}", data.id, data.data);
            }
        }
    }
}
