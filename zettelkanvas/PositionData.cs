using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace zettelkanvas
{
    public class PositionData : IComparable<PositionData>
    {
        public struct Element : IComparable<Element>
        {
            public int Number { get; private set; }
            public string Branch { get; private set; }

            public static Regex ElementRegex = new Regex(@"\d+|[a-z]+", RegexOptions.Compiled);
            public Element(string nodeNameElement)
            {
                MatchCollection parts = ElementRegex.Matches(nodeNameElement);
                Number = int.Parse(parts[0].Value);
                Branch = "";
                if (parts.Count > 1)
                    Branch = parts[1].Value;
            }
            public int CompareTo(Element other)
            {
                if (Number < other.Number) return -1;
                if (Number > other.Number) { return 1; }

                return String.Compare(Branch, other.Branch, StringComparison.InvariantCulture);
            }
            public override string ToString()
            {
                return $"{Number}:\"{Branch}\"";
            }
        }
        public List<Element> NameParts { get; private set; }

        private static Regex NamePartRegex = new Regex(@"\d+[a-z]*", RegexOptions.Compiled);

        public PositionData(string position)
        {
            NameParts = new List<Element>();
            foreach (Match match in NamePartRegex.Matches(position))
            {
                NameParts.Add(new Element(match.Value));
            }
        }
        public override string ToString()
        {
            var res = $"";
            foreach (Element e in NameParts)
            {
                res += $"{e} ";
            }
            return res;
        }
        public int CompareTo(PositionData? other)
        {
            if (other == null) return 1;

            for (int i = 0; i < NameParts.Count; i++)
            {
                if (i == other.NameParts.Count) return 1;
                var partComparison = NameParts[i].CompareTo(other.NameParts[i]);
                if (partComparison != 0) return partComparison;
            }
            return -1;
        }

        public static bool IsBranchBase(PositionData branchBase, PositionData branch)
        {
            if (branchBase.NameParts.Count != branch.NameParts.Count) return false;
            int i = 0;
            while (i < branchBase.NameParts.Count - 1)
            {
                if (branchBase.NameParts[i].CompareTo(branch.NameParts[i]) != 0) return false;
                i++;
            }
            if (branchBase.NameParts[i].Number == branch.NameParts[i].Number && (branchBase.NameParts[i].Branch == "" || branch.NameParts[i].Branch.Contains(branchBase.NameParts[i].Branch))) return true;
            return false;
        }
        public static bool IsSuitableNext(PositionData baseNode, PositionData nextNode)
        {
            if (nextNode.NameParts.Count - baseNode.NameParts.Count > 1) return false;
            int i = 0;
            while (i < baseNode.NameParts.Count - 1)
            {
                if (baseNode.NameParts[i].CompareTo(nextNode.NameParts[i]) != 0) return false;
                i++;
            }
            if (nextNode.NameParts.Count - baseNode.NameParts.Count == 1) return true;
            if (baseNode.NameParts[i].Number < nextNode.NameParts[i].Number && baseNode.NameParts[i].Branch == "" && nextNode.NameParts[i].Branch == "") return true;
            return false;
        }
    }
}
