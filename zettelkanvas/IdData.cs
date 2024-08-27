using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace zettelkanvas
{
    internal class IdData : IComparable<IdData>
    {
        public List<IdElement> NameParts { get; private set; }

        public IdData(string noteId)
        {
            NameParts = new List<IdElement>();
            foreach (Match match in Regexes.IdElement().Matches(noteId))
            {
                NameParts.Add(new IdElement(match.Value));
            }
        }
        public override string ToString()
        {
            var res = $"";
            foreach (IdElement e in NameParts)
            {
                res += $"{e} ";
            }
            return res;
        }
        public int CompareTo(IdData? other)
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

        public static bool IsBranchBase(IdData branchBase, IdData branch)
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
        public static bool IsSuitableNext(IdData baseNode, IdData nextNode)
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
