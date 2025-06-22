
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityUtils {
    public interface IPredicate {
        bool Evaluate();
    }
    public class And : IPredicate
    {
        private readonly List<IPredicate> _rules = new List<IPredicate>();

        public bool Evaluate()
        {
            foreach (var rule in _rules)
            {
                if (!rule.Evaluate())
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class Or : IPredicate
    {
        private List<IPredicate> rules = new List<IPredicate>();

        public bool Evaluate()
        {
            foreach (var rule in rules)
            {
                if (rule.Evaluate())
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class Not : IPredicate {
        IPredicate rule;
        public bool Evaluate() => !rule.Evaluate();
    }
}