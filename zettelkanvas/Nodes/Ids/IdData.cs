using System.Diagnostics;
using System.Text.RegularExpressions;
using Zettelkanvas.Static;

namespace Zettelkanvas.Nodes.Ids
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

        // -1 -> neither branch nor next
        // 0 -> second is first's next
        // 1-> second is firsts's branch
        public static int Relation(IdData first, IdData second)
        {
            Debug.Assert(first.CompareTo(second) < 0);
            int i;
            for (i = 0; i < first.NameParts.Count - 1; i++)
            {
                if (first.NameParts[i].CompareTo(second.NameParts[i]) != 0)
                    return -1;
            }

            if (first.NameParts.Count - second.NameParts.Count > 0) return 0;
            bool noBranches = first.NameParts[i].Branch == "" && second.NameParts[i].Branch == "";
            bool secondHasBuggerNumber = first.NameParts[i].Number < second.NameParts[i].Number;
            if (noBranches && secondHasBuggerNumber) return 0;

            bool sameNumber = first.NameParts[i].Number == second.NameParts[i].Number;
            bool differentBranches = first.NameParts[i].Branch == "" || second.NameParts[i].Branch.Contains(first.NameParts[i].Branch);
            if (sameNumber && differentBranches) return 1;

            return -1;
        }
    }
}
