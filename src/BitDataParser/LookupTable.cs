using System;
using System.Collections;
using System.Collections.Generic;

namespace BitDataParser
{
    public class LookupTable : IEnumerable<Lookup>
    {
        public List<Lookup> Table { get; set; } = new List<Lookup>();
        
        public bool ContainsKey(string key)
        {
            return Table.Exists(tp => tp.From == key);
        }        

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Table).GetEnumerator();
        }

        public int Count => Table.Count;

        public void Add(string key, string val)
        {
            Table.Add(new Lookup(key, val));
        }

        public void Add(Lookup lookup)
        {
            Table.Add(lookup);
        }

        IEnumerator<Lookup> IEnumerable<Lookup>.GetEnumerator()
        {
            return ((IEnumerable<Lookup>)Table).GetEnumerator();
        }

        public string this[string index]
        {
            get
            {
                return Table.Find(tp => tp.From == index).To;
            }
            set
            {
                var finding = Table.Find(tp => tp.From == index);
                if(finding != null)                
                {
                    Table.Remove(finding);
                }
                Table.Add(new Lookup(index, value));
            }
        }

        //public string this[int index]
        //{
        //    get
        //    {
        //        return this[index.ToString()];
        //    }
        //    set
        //    {
        //        this[index.ToString()] = value;
        //    }
        //}        

        public override string ToString()
        {
            return "Count " + Count;
        }
    }

    public class IntLookupTable : IEnumerable<IntLookup>
    {
        public List<IntLookup> Table { get; set; } = new List<IntLookup>();

        public bool ContainsKey(int key)
        {
            return Table.Exists(tp => tp.From == key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Table).GetEnumerator();
        }

        public int Count => Table.Count;

        public void Add(int key, int val)
        {
            Table.Add(new IntLookup(key, val));
        }

        public void Add(IntLookup lookup)
        {
            Table.Add(lookup);
        }

        IEnumerator<IntLookup> IEnumerable<IntLookup>.GetEnumerator()
        {
            return ((IEnumerable<IntLookup>)Table).GetEnumerator();
        }

        public int this[int index]
        {
            get
            {
                return Table.Find(tp => tp.From == index).To;
            }
            set
            {
                var finding = Table.Find(tp => tp.From == index);
                if (finding != null)
                {
                    Table.Remove(finding);
                }
                Table.Add(new IntLookup(index, value));
            }
        }

        public override string ToString()
        {
            return "Count " + Count;
        }
    }

    public class Lookup
    {
        public Lookup()
        {

        }

        public Lookup(string from, string to)
        {
            From = from;
            To = to;
        }

        public string From { get; set; }
        public string To { get; set; }

        public override string ToString()
        {
            return From + "->" + To;
        }
    }

    public class IntLookup
    {
        public IntLookup()
        {

        }

        public IntLookup(int from, int to)
        {
            From = from;
            To = to;
        }

        public int From { get; set; }
        public int To { get; set; }

        public override string ToString()
        {
            return From + "->" + To;
        }
    }
}