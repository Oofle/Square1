using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Extension {
    public static class Methods {
        //https://stackoverflow.com/a/1082938
        public static int mod(this int x, int m) {
            return (x%m + m)%m;
        }
        
        public static bool SameElements(this List<object> a, List<object> b) {
            if (a.Count != b.Count) {
                return false;
            }
            foreach(object element in a) {
                if (!b.Contains(a)) {
                    return false;
                }
            }
            return true;
        }
        
        public static bool SameElements(this object[] a, object[] b) {
            return SameElements(a.ToList(), b.ToList());
        }
    }
}
