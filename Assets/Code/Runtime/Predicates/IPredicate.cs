
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityUtils {
    public interface IPredicate {
        bool Evaluate();
    }

    public class And : IPredicate {
        List<IPredicate> rules = new List<IPredicate>();
        public bool Evaluate() => rules.All(r => r.Evaluate());
    }

    public class Or : IPredicate {
        List<IPredicate> rules = new List<IPredicate>();
        public bool Evaluate() => rules.Any(r => r.Evaluate());
    }

    public class Not : IPredicate {
        IPredicate rule;
        public bool Evaluate() => !rule.Evaluate();
    }
}