using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD26
{
    public class Pair
    {
        public Vector3? First { get; set; }
        public Vector3? Second { get; set; }

        internal void Sort()
        {
            if (First.Value.Length() >= Second.Value.Length())
            {
                var tmp = Second;
                Second = First;
                First = tmp;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Pair)
            {
                var other = obj as Pair;
                if (other.First.ToString() == this.First.ToString() && other.Second.ToString() == this.Second.ToString())
                {
                    return true;
                }
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
